using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LenixSO.Sequences.Composite
{
    /// <summary>
    /// A sequence that runs multiple other sequences in parallel.
    /// </summary>
    public class ParallelSequences : ISequence
    {
        public string name { get; set; }
        public bool running { get; private set; }
        public event Action OnFinished;

        private List<ISequence> sequences = new();
        private int sequencesLeft;
        
        /// <summary>
        /// Create a ParallelSequences with some sequences already on them
        /// </summary>
        /// <param name="startingSequences">sequences that will be added</param>
        public ParallelSequences(params ISequence[] startingSequences)
        {
            for (int i = 0; i < startingSequences.Length; i++)
                Add(startingSequences[i]);
        }

        public int count => sequences?.Count ?? 0;
        public ISequence currentSequence => sequences.Count > 0 ? sequences[0] : null;

        public ISequence this[int id]
        {
            get => sequences[id];
            set => sequences[id] = value;
        }

        /// <summary>
        /// Adds a sequence to be executed in parallel with others.
        /// </summary>
        /// <param name="sequence">The sequence to add to the parallel group.</param>
        /// <returns>Returns the current ParallelSequences instance for method chaining.</returns>
        public ParallelSequences Add(ISequence sequence)
        {
            sequences.Add(sequence);
            return this;
        }

        public void Remove(ISequence sequence) => sequences.Remove(sequence);

        /// <summary>
        /// Clears all sequences from the manager and resets the state.
        /// </summary>
        public void Clear()
        {
            sequences.Clear();
        }

        public bool Contains(ISequence sequence) => sequences.Contains(sequence);

        /// <summary>
        /// Starts all sequences in parallel.
        /// </summary>
        public void Begin()
        {
            if (running) return; // Prevent starting if sequences are already running
            sequencesLeft = sequences.Count;
            running = sequences.Count > 0;
            for (int i = 0; i < sequences.Count; i++)
            {
                sequences[i].ListenNextFinishedCallback(OnSequenceFinished);
                sequences[i]?.Begin();
            }

            if (!running && sequencesLeft >= 0) AllSequencesFinished();
        }

        /// <summary>
        /// Ends all sequences in parallel.
        /// </summary>
        public void End()
        {
            if (!running) return; // Prevent ending if sequences are not running
            for (int i = 0; i < sequences.Count; i++)
            {
                if (!sequences[i].running) continue;
                sequences[i]?.End();
            }
        }

        /// <summary>
        /// Called when a sequence finishes, decreases the count of remaining sequences.
        /// </summary>
        private void OnSequenceFinished()
        {
            sequencesLeft--;
            if (sequencesLeft > 0) return; // If there are still sequences running, do nothing
            AllSequencesFinished();
        }

        /// <summary>
        /// Called when all sequences are finished, triggers the OnFinished event.
        /// </summary>
        private void AllSequencesFinished()
        {
            running = false;
            sequencesLeft = -1;
            OnFinished?.Invoke();
        }

        public override string ToString()
        {
            StringBuilder sb = new($"Parallel({name})[");
            for (int i = 0; i < sequences.Count; i++)
            {
                sb.Append(sequences[i]);
                if (i < sequences.Count - 1) sb.Append("|");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
