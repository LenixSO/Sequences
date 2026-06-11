using System;

namespace LenixSO.Sequences.Decorator
{
    /// <summary>
    /// A sequence that only runs if a specified condition is true.
    /// </summary>
    public class ConditionalSequence : ISequence
    {
        public string name { get; set; }
        public bool running { get; private set; }
        private Func<bool> condition;
        private ISequence Sequence;
        public event Action OnFinished;

        private bool playedSequence;

        /// <summary>
        /// Initializes a conditional sequence that will only run if the condition is true.
        /// </summary>
        /// <param name="sequenceCondition">A function that returns true if the sequence should run.</param>
        /// <param name="sequence">The sequence to execute if the condition is met.</param>
        public ConditionalSequence(Func<bool> sequenceCondition, ISequence sequence)
        {
            condition = sequenceCondition;
            Sequence = sequence;
            sequence.OnFinished += Finish; // Notify when the sequence finishes
        }
        
        /// <summary>
        /// Starts the sequence if the condition returns true, otherwise ends immediately.
        /// </summary>
        public void Begin()
        {
            running = true;
            playedSequence = condition?.Invoke() ?? true;
            if (playedSequence) Sequence?.Begin();
            else End();
        }

        /// <summary>
        /// Ends the sequence if it was played, or finishes immediately if it was skipped.
        /// </summary>
        public void End()
        {
            if (playedSequence) Sequence?.End();
            else Finish();
        }

        /// <summary>
        /// Marks the conditional sequence as finished and triggers the OnFinished event.
        /// </summary>
        private void Finish()
        {
            playedSequence = false;
            running = false;
            OnFinished?.Invoke();
        }

        public override string ToString()
        {
            return $"Conditional({name})[{condition?.Invoke()}=>{Sequence}]";
        }
    }

}
