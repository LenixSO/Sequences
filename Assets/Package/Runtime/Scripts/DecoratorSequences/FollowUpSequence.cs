using System;
namespace LenixSO.Sequences.Decorator
{
    /// <summary>
    /// Represents a sequence that executes a main sequence and then automatically begins a follow-up sequence upon completion.
    /// The main sequence is executed first, and when it finishes, the follow-up sequence begins automatically.
    /// </summary>
    public class FollowUpSequence : ISequence
    {
        public string name { get; set; }
        private ISequence sequence;
        private ISequence followUp;

        /// <summary>
        /// Occurs when the main sequence has finished execution, just before the follow-up sequence begins.
        /// </summary>
        public event Action OnFinished;

        /// <summary>
        /// Initializes a new instance of the FollowUpSequence class with the specified main and follow-up sequences.
        /// </summary>
        /// <param name="mainSequence">The main sequence to execute first.</param>
        /// <param name="followUpSequence">The sequence to execute immediately after the main sequence finishes.</param>
        public FollowUpSequence(ISequence mainSequence, ISequence followUpSequence)
        {
            sequence = mainSequence;
            followUp = followUpSequence;
        }

        /// <summary>
        /// Begins execution of the main sequence. The follow-up sequence will automatically start
        /// when the main sequence completes.
        /// </summary>
        public void Begin()
        {
            sequence.OnFinished += OnMainSequenceFinished;
            sequence.Begin();
        }

        /// <summary>
        /// Ends both the main sequence and the follow-up sequence, stopping execution of both sequences.
        /// </summary>
        public void End()
        {
            sequence.End();
            followUp.End();
        }

        /// <summary>
        /// Handles completion of the main sequence by unsubscribing from its event,
        /// invoking the OnFinished event, and beginning the follow-up sequence.
        /// </summary>
        private void OnMainSequenceFinished()
        {
            sequence.OnFinished -= OnMainSequenceFinished;
            OnFinished?.Invoke();
            followUp.Begin();
        }

        public override string ToString()
        {
            return $"FollowUp({name})[{sequence}=>{followUp}]";
        }
    }

}
