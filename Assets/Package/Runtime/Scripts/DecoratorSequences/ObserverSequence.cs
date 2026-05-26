using System;
namespace LenixSO.Sequences.Decorator
{
    public class ObserverSequence : ISequence
    {
        public string name { get; set; }
        public bool running { get; private set; }
        public event Action OnFinished;

        public event Action OnBeginCall;
        public event Action OnEndCall;
        
        private ISequence sequence;

        public ObserverSequence(ISequence sequence, Action onBeginCall = null, Action onEndCall = null)
        {
            this.sequence = sequence;
            OnBeginCall += onBeginCall;
            OnEndCall += onEndCall;
        }
        
        public void Begin()
        {
            running = true;
            sequence.ListenNextFinishedCallback(OnSequenceFinished);
            OnBeginCall?.Invoke();
            sequence.Begin();
        }
        public void End()
        {
            OnEndCall?.Invoke();
            sequence.End();
        }

        private void OnSequenceFinished()
        {
            running = false;
            OnFinished?.Invoke();
        }
    }
}
