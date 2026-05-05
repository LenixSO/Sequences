using System;
using UnityEngine;

namespace LenixSO.Sequences.Coroutines
{
    /// <summary>
    /// Adapter that allows ExpandedCoroutine to be used as an ISequence implementation.
    /// Bridges the gap between coroutine-based execution and the sequence system.
    /// </summary>
    public class CoroutineSequence : ISequence
    {
        public string name { get; set; }
        private ExpandedCoroutine expandedCoroutine;
        private MonoBehaviour monoBehaviour;

        /// <summary>
        /// Event invoked when the wrapped coroutine completes execution.
        /// </summary>
        public event Action OnFinished;

        /// <summary>
        /// Initializes a new instance of the CoroutineSequence class.
        /// </summary>
        /// <param name="coroutine">The ExpandedCoroutine to wrap and manage as a sequence.</param>
        /// <param name="target">The MonoBehaviour that will host the coroutine execution.</param>
        public CoroutineSequence(ExpandedCoroutine coroutine, MonoBehaviour target = null)
        {
            monoBehaviour = target ?? CoroutineExtensions.Holder;
            expandedCoroutine = coroutine;
            expandedCoroutine.onEndCoroutine += OnCoroutineFinished;
        }

        /// <summary>
        /// Begins execution of the wrapped coroutine using the specified MonoBehaviour host.
        /// </summary>
        public void Begin()
        {
            if (expandedCoroutine.running) return;
            if (!monoBehaviour.gameObject.activeInHierarchy) expandedCoroutine.TriggerEndCallback();
            else monoBehaviour.BeginCoroutine(expandedCoroutine);
        }

        /// <summary>
        /// Stops the coroutine execution and cleans up resources.
        /// </summary>
        public void End()
        {
            if (!expandedCoroutine.running) return;
            monoBehaviour.EndCoroutine(expandedCoroutine);
        }

        /// <summary>
        /// Handles coroutine completion by propagating the event through the ISequence interface.
        /// </summary>
        private void OnCoroutineFinished() => OnFinished?.Invoke();

        public override string ToString()
        {
            return $"Coroutine({name})";
        }
    }

}
