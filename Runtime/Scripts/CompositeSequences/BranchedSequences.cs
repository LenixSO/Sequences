using System;

namespace LenixSO.Sequences.Composite
{
    /// <summary>
    /// Represents a sequence that branches between two different sequences (main and alternative)
    /// based on a specified condition. Only one of the sequences will be executed when begun.
    /// </summary>
    public class BranchedSequence : ISequence
    {
        public string name { get; set; }
        private ISequence mainSequence;
        private ISequence altSequence;
        private Func<bool> contidion;
        private ISequence currentSequence;

        /// <summary>
        /// Occurs when the active branched sequence (either main or alternative) has finished execution.
        /// </summary>
        public event Action OnFinished;

        /// <summary>
        /// Initializes a new instance of the BranchedSequence class with specified main sequence, 
        /// alternative sequence, and branching condition.
        /// </summary>
        /// <param name="main">The main sequence to execute if the condition evaluates to true.</param>
        /// <param name="alt">The alternative sequence to execute if the condition evaluates to false.</param>
        /// <param name="contidion">The condition delegate that determines which sequence to execute.</param>
        public BranchedSequence(ISequence main, ISequence alt, Func<bool> contidion)
        {
            mainSequence = main;
            altSequence = alt;
            this.contidion = contidion;
        }

        /// <summary>
        /// Starts execution of the branched sequence. The condition is evaluated to determine
        /// whether to run the main sequence or alternative sequence, then begins the selected sequence.
        /// </summary>
        public void Begin()
        {
            currentSequence = contidion?.Invoke() ?? true ? mainSequence : altSequence;
            currentSequence.ListenNextFinishedCallback(OnSequenceEnd);
            currentSequence.Begin();
        }

        /// <summary>
        /// Ends the currently active sequence (if any).
        /// </summary>
        public void End() => currentSequence?.End();

        /// <summary>
        /// Handles completion of the active sequence, cleans up event handlers, and invokes the OnFinished event.
        /// </summary>
        private void OnSequenceEnd()
        {
            currentSequence = null;
            OnFinished?.Invoke();
        }

        public override string ToString()
        {
            return $"Branched({name})[{contidion?.Invoke()} ? {mainSequence} : {altSequence}";
        }
    }
}
