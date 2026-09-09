using System;

namespace LenixSO.Sequences
{
    public class GateSequence : ISequence
    {
        public string name { get; set; }
        public bool running { get; private set; }
        public event Action OnFinished;

        public bool open { get; private set; }
        
        public GateSequence(bool gateOpen = true) => open = gateOpen;
        
        public void Begin()
        {
            if (running) return;
            running = true;
            if (open) End();
        }

        public void End()
        {
            if (!running) return;
            running = false;
            OnFinished?.Invoke();
        }

        public void OpenGate()
        {
            open = true;
            if (running) End();
        }
        
        public void CloseGate()
        {
            open = false;
        }
    }
}
