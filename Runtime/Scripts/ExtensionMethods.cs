using System;
using LenixSO.Sequences.Decorator;

namespace LenixSO.Sequences
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Ends the sequence with a callback for when the sequence is finished
        /// </summary>
        /// <param name="sequence">Sequence to be finished</param>
        /// <param name="onEnd">Callback for when the sequence finishes</param>
        public static void EndThen(this ISequence sequence, Action onEnd)
        {
            sequence.ListenNextFinishedCallback(onEnd);
            sequence.End();
        }

        /// <summary>
        /// Begins a sequence and returns itself, allowing for method chaining
        /// </summary>
        public static T BeginChained<T>(this T sequence) where T : ISequence
        {
            sequence.Begin();
            return sequence;
        }

        /// <summary>
        /// Add a listener for the OnFinished event and returns itself, allowing for method chaining
        /// </summary>
        /// <param name="callback"></param>
        public static T AddFinishedCallback<T>(this T sequence, Action callback) where T : ISequence
        {
            sequence.OnFinished += callback;
            return sequence;
        }

        public static void ListenNextFinishedCallback(this ISequence sequence, Action callback)
        {
            sequence.OnFinished += OneShotAction;
            return;
            void OneShotAction()
            {
                sequence.OnFinished -= OneShotAction;
                callback?.Invoke();
            }
        }

        public static ISequence AwaitEndSequence(this ISequence sequence)
        {
            ISequence awaitSequence = CustomSequence.EmptySequence();
            awaitSequence = new CustomSequence(()=>
            {
                sequence.ListenNextFinishedCallback(awaitSequence.End);
                sequence.End();
            }, null);

            return awaitSequence;
        }
    }
}
