using System;
namespace LenixSO.Sequences.Decorator
{
    public class LoopingSequence : ISequence
    {
        public string name { get; set; }
        public bool running { get; private set; }
        public event Action OnFinished;
        
        private ISequence sequence;
        private int sequenceLoops;
        
        private int currentLoop;

        public LoopingSequence(ISequence loopingSequence, int loops = -1)
        {
            sequence = loopingSequence;
            sequenceLoops = loops;
        }
        
        public void Begin()
        {
            running = true;
            currentLoop = sequenceLoops;
            BeginSequence();
        }

        private void BeginSequence()
        {
            sequence.ListenNextFinishedCallback(OnSequenceEnd);
            sequence.Begin();
        }
        
        public void End()
        {
            currentLoop = 1;
            sequence.End();
        }

        private void OnSequenceEnd()
        {
            currentLoop = Math.Max(currentLoop - 1, -1);
            if (currentLoop == 0)
            {
                running = false;
                OnFinished?.Invoke();
                return;
            }
            BeginSequence();
        }
    }
}
