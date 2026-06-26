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
        public bool running { get; private set; }
        public int count => sequences.Count;

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
        
        public void RemoveAt(int index)
        {
            if (index > currentId) currentId--;
            sequences.RemoveAt(index);
        }

        /// <summary>
        /// Clears all sequences from the manager and resets the state.
        /// </summary>
        public void Clear()
        {
            sequences.Clear(); // Clears the sequence list
            currentId = 0; // Reset the current sequence index
        }

        /// <summary>
        /// Checks if a specific sequence exists in the sequence manager.
        /// </summary>
        /// <param name="sequence">The sequence to check for.</param>
        /// <returns>Returns true if the sequence is in the list; otherwise, false.</returns>
        public bool Contains(ISequence sequence) => sequences.Contains(sequence);

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the specified sequence in the collection.
        /// </summary>
        /// <param name="sequence">The sequence to locate in the sequence list.</param>
        /// <returns>
        /// The zero-based index of the first occurrence of the specified sequence if found; otherwise, -1.
        /// </returns>
        public int IndexOf(ISequence sequence) => sequences.IndexOf(sequence);
        
        /// <summary>
        /// Returns the zero-based index of the last occurrence of the specified sequence in the collection.
        /// </summary>
        /// <param name="sequence">The sequence to locate in the sequence list.</param>
        /// <returns>
        /// The zero-based index of the last occurrence of the specified sequence if found; otherwise, -1.
        /// </returns>
        public int LastIndexOf(ISequence sequence) => sequences.LastIndexOf(sequence);
        
        public void Begin()
        {
            running = true;
            currentSequence?.ListenNextFinishedCallback(OnCurrentSequenceFinished);
            currentSequence?.Begin();
        }
        
        public void End()
        {
            if (currentSequence == null) OnCurrentSequenceFinished();
            else currentSequence?.End();
        }
        
        private void OnCurrentSequenceFinished()
        {
            running = false;
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
