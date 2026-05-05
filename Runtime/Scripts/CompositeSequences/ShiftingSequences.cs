using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LenixSO.Sequences.Composite
{
    public class ShiftingSequences: ISequence
    {
        public string name { get; set; }
        public event Action OnFinished;
        private List<ISequence> sequences = new();
        private int currentId = 0;
        private bool running = false;
        public ISequence currentSequence
        {
            get
            {
                return currentId < sequences.Count ? sequences[currentId] : null;
            }
            set
            {
                if (running) return;
                if (currentId >= sequences.Count) Add(value);
                else sequences[currentId] = value;
            }
        }
        public ISequence this[int id]
        {
            get => sequences[id];
            set => sequences[id] = value;
        }

        public ShiftingSequences(params ISequence[] sequences)
        {
            for (int i = 0; i < sequences.Length; i++) 
                Add(sequences[i]);
        }

        public ShiftingSequences Add(ISequence sequence)
        {
            sequences.Add(sequence);
            return this;
        }
        
        public void Begin()
        {
            running = true;
            currentSequence?.ListenNextFinishedCallback(OnCurrentSequenceFinished);
            currentSequence?.Begin();
        }
        
        public void End()
        {
            running = false;
            if (currentSequence == null) OnCurrentSequenceFinished();
            else currentSequence?.End();
        }
        
        private void OnCurrentSequenceFinished()
        {
            currentId = (currentId + 1) % sequences.Count;
            OnFinished?.Invoke();
        }

        public override string ToString()
        {
            StringBuilder sb = new($"Shifting({name})[");
            for (int i = 0; i < sequences.Count; i++)
            {
                sb.Append(sequences[i]);
                if (i < sequences.Count - 1) sb.Append("|");
            }
            sb.Append($"] => {currentSequence}");
            return sb.ToString();
        }
    }
}
