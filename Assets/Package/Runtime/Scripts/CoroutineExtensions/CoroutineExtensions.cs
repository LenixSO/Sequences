using System;
using System.Collections;
using UnityEngine;

namespace LenixSO.Sequences.Coroutines
{
    /// <summary>
    /// Provides extension methods for managing coroutines, including support for ExpandedCoroutine wrappers
    /// and static coroutine execution without requiring a MonoBehaviour reference.
    /// </summary>
    public static class CoroutineExtensions
    {
        private static CoroutineHolder coroutineHolder;

        /// <summary>
        /// Gets or creates a singleton GameObject to host coroutines when no specific MonoBehaviour is available.
        /// </summary>
        public static MonoBehaviour Holder
        {
            get
            {
                if (!coroutineHolder)
                {
                    coroutineHolder = new GameObject("CoroutineHolder").AddComponent<CoroutineHolder>();
                }

                return coroutineHolder;
            }
        }

        /// <summary>
        /// Starts a coroutine using the static coroutine holder when no MonoBehaviour context is available.
        /// </summary>
        /// <param name="coroutine">The coroutine iterator to execute.</param>
        public static void StartCoroutine(IEnumerator coroutine)
        {
            Holder.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Stops a previously started coroutine on the static coroutine holder.
        /// </summary>
        /// <param name="coroutine">The coroutine iterator to stop. Does nothing if null.</param>
        public static void StopCoroutine(IEnumerator coroutine)
        {
            if (coroutine == null) return;
            Holder.StopCoroutine(coroutine);
        }

        /// <summary>
        /// Begins an ExpandedCoroutine using the specified MonoBehaviour as the execution context.
        /// </summary>
        /// <param name="mono">The MonoBehaviour that will host and manage the coroutine.</param>
        /// <param name="coroutine">The ExpandedCoroutine to begin executing.</param>
        public static void BeginCoroutine(this MonoBehaviour mono, ExpandedCoroutine coroutine)
        {
            mono.StartCoroutine(coroutine?.Coroutine);
        }

        /// <summary>
        /// Creates and begins an ExpandedCoroutine with a completion callback using the specified MonoBehaviour.
        /// </summary>
        /// <param name="mono">The MonoBehaviour that will host and manage the coroutine.</param>
        /// <param name="coroutine">The coroutine iterator to execute.</param>
        /// <param name="onEnd">Callback action to invoke when the coroutine completes.</param>
        /// <returns>The created ExpandedCoroutine instance for further management.</returns>
        public static ExpandedCoroutine BeginCoroutine(this MonoBehaviour mono, IEnumerator coroutine, Action onEnd)
        {
            ExpandedCoroutine expandedCoroutine = new(coroutine, onEnd);
            mono.BeginCoroutine(expandedCoroutine);
            return expandedCoroutine;
        }

        /// <summary>
        /// Stops and cleans up an ExpandedCoroutine, ensuring proper termination and callback invocation.
        /// </summary>
        /// <param name="mono">The MonoBehaviour that is currently hosting the coroutine.</param>
        /// <param name="coroutine">The ExpandedCoroutine to stop. Handles null and already-stopped coroutines gracefully.</param>
        public static void EndCoroutine(this MonoBehaviour mono, ExpandedCoroutine coroutine)
        {
            if (coroutine is { running: true })
                mono.StopCoroutine(coroutine?.Coroutine);
            coroutine?.EndCoroutine();
        }

        /// <summary>
        /// Executes a callback after waiting for one frame using the static coroutine holder.
        /// </summary>
        /// <param name="onDone">The action to invoke after one frame has passed.</param>
        public static void WaitAFrame(Action onDone) => StartCoroutine(FrameDelay(onDone));

        /// <summary>
        /// Coroutine that waits for one frame then invokes the specified callback.
        /// </summary>
        /// <param name="onDone">The action to invoke after the frame delay.</param>
        /// <returns>IEnumerator for coroutine execution.</returns>
        public static IEnumerator FrameDelay(Action onDone)
        {
            yield return null;
            onDone?.Invoke();
        }

        /// <summary>
        /// Starts a coroutine, then triggers a callback when it finishes
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="onEnd"></param>
        public static void AwaitCoroutine(IEnumerator coroutine, Action onEnd) => Holder.StartCoroutine(CoroutineAwaiter(coroutine, onEnd));

        public static IEnumerator DelayCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
        }
        
        public static IEnumerator DelayedCoroutine(IEnumerator coroutine, float delay)
        {
            yield return DelayCoroutine(delay);
            yield return coroutine;
        }

        private static IEnumerator CoroutineAwaiter(IEnumerator coroutine, Action onEnd)
        {
            yield return coroutine;
            onEnd?.Invoke();
        }
    }

    /// <summary>
    /// Enhanced coroutine wrapper that provides completion events and runtime state tracking.
    /// Allows for external coroutine management and cleanup when stopped prematurely.
    /// </summary>
    public class ExpandedCoroutine
    {
        private IEnumerator _coroutine;
        private Func<IEnumerator> _coroutineMethod;

        /// <summary>
        /// Event invoked when the coroutine completes execution, either naturally or when stopped.
        /// </summary>
        public event Action onEndCoroutine;

        private IEnumerator currentCoroutine;

        /// <summary>
        /// Indicates whether the coroutine is currently running.
        /// </summary>
        public bool running;

        /// <summary>
        /// Initializes a new instance of the ExpandedCoroutine class. This constructor only allows the coroutine to be called once.
        /// </summary>
        /// <param name="coroutine">The coroutine iterator to wrap and manage.</param>
        /// <param name="OnEnd">Optional callback to register for completion notification.</param>
        public ExpandedCoroutine(IEnumerator coroutine, Action OnEnd = null)
        {
            _coroutine = coroutine;
            onEndCoroutine += OnEnd;
        }

        /// <summary>
        /// Initializes a new instance of the ExpandedCoroutine class.
        /// </summary>
        /// <param name="coroutine">Method that creates the coroutine iterator to wrap and manage.</param>
        /// <param name="OnEnd">Optional callback to register for completion notification.</param>
        public ExpandedCoroutine(Func<IEnumerator> coroutine, Action OnEnd = null)
        {
            _coroutineMethod += coroutine;
            onEndCoroutine += OnEnd;
        }

        /// <summary>
        /// Gets the wrapped coroutine iterator that includes completion tracking logic.
        /// </summary>
        public IEnumerator Coroutine
        {
            get
            {
                if (currentCoroutine == null) currentCoroutine = CoroutineEnder();
                return currentCoroutine;
            }
        }

        /// <summary>
        /// Wrapper coroutine that tracks execution state and ensures completion callbacks are invoked.
        /// </summary>
        private IEnumerator CoroutineEnder()
        {
            running = true;
            yield return _coroutine ?? _coroutineMethod?.Invoke();
            EndCoroutine();
        }

        /// <summary>
        /// Stops the coroutine execution, cleans up resources, and invokes completion callbacks.
        /// Safe to call on already-stopped coroutines.
        /// </summary>
        public void EndCoroutine()
        {
            if (!running) return;
            running = false;
            currentCoroutine = null;
            TriggerEndCallback();
        }

        public void TriggerEndCallback() => onEndCoroutine?.Invoke();
    }

    /// <summary>
    /// Internal MonoBehaviour component used by CoroutineExtensions to host coroutines
    /// when no specific MonoBehaviour context is available.
    /// </summary>
    internal class CoroutineHolder : MonoBehaviour
    {
    }
}