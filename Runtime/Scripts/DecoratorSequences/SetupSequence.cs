using System;

namespace LenixSO.Sequences.Decorator
{
    /// <summary>
    /// A sequence that runs another sequence after performing an initial setup action.
    /// </summary>
    public class SetupSequence: ISequence
    {
        public string name { get; set; }
        public bool running { get; private set; }
        private Action SetupAction;
        private ISequence Sequence;
        public event Action OnFinished;

        /// <summary>
        /// Initializes a sequence that will first perform the setup action, then begin the nested sequence.
        /// </summary>
        /// <param name="setup">The setup action to perform before the sequence begins.</param>
        /// <param name="sequence">The sequence that will run after the setup is complete.</param>
        public SetupSequence(Action setup, ISequence sequence)
        {
            SetupAction = setup;
            Sequence = sequence;
            sequence.OnFinished += Finish; // Notify when the nested sequence finishes
        }
        
        /// <summary>
        /// Begins the sequence by first running the setup action and then starting the nested sequence.
        /// </summary>
        public void Begin()
        {
            running = true;
            SetupAction?.Invoke();
            Sequence?.Begin();
        }

        /// <summary>
        /// Ends the nested sequence by invoking its end method.
        /// </summary>
        public void End() => Sequence?.End();

        /// <summary>
        /// Invoked when the nested sequence finishes, triggers the OnFinished event.
        /// </summary>
        private void Finish()
        {
            running = false;
            OnFinished?.Invoke();
        }

        public override string ToString()
        {
            return $"Setup({name})[{Sequence}]";
        }
    }
}
