using System;
using System.Collections.Generic;
using System.Text;

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
        private List<ISequence> runningSequences = new();
        private int sequencesLeft;

        
        /// <summary>
        /// Create a ParallelSequences with some sequences already on them
        /// </summary>
        /// <param name="sequences">sequences that will be added</param>
        public ParallelSequences(params ISequence[] startingSequences)
        {
            for (int i = 0; i < startingSequences.Length; i++)
                Add(startingSequences[i]);
        }

        /// <summary>
        /// Adds a sequence to be executed in parallel with others.
        /// </summary>
        /// <param name="sequence">The sequence to add to the parallel group.</param>
        /// <returns>Returns the current ParallelSequences instance for method chaining.</returns>
        public ParallelSequences Add(ISequence sequence)
        {
            sequences.Add(sequence);
            sequence.OnFinished += OnFinishedSequence;
            sequence.OnFinished += OnSequenceFinished;
            return this;

            void OnFinishedSequence()
            {
                runningSequences.Remove(sequence);
            }
        }

        /// <summary>
        /// Starts all sequences in parallel.
        /// </summary>
        public void Begin()
        {
            if (running) return; // Prevent starting if sequences are already running
            sequencesLeft = sequences.Count;
            for (int i = 0; i < sequences.Count; i++)
            {
                runningSequences.Add(sequences[i]);
                sequences[i]?.Begin();
            }

            if (!running) AllSequencesFinished();
        }

        /// <summary>
        /// Ends all sequences in parallel.
        /// </summary>
        public void End()
        {
            if (!running) return; // Prevent ending if sequences are not running
            for (int i = 0; i < sequences.Count; i++)
            {
                if (!runningSequences.Contains(sequences[i])) continue;
                sequences[i]?.End();
            }
        }

        /// <summary>
        /// Called when a sequence finishes, decreases the count of remaining sequences.
        /// </summary>
        private void OnSequenceFinished()
        {
            sequencesLeft--;
            if (running) return; // If there are still sequences running, do nothing
            AllSequencesFinished();
        }

        /// <summary>
        /// Called when all sequences are finished, triggers the OnFinished event.
        /// </summary>
        private void AllSequencesFinished()
        {
            running = false;
            runningSequences.Clear();
            sequencesLeft = 0;
            OnFinished?.Invoke();
        }

        /// <summary>
        /// Clears the list of sequences and removes the event handlers.
        /// </summary>
        private void ClearSequence()
        {
            for (int i = 0; i < sequences.Count; i++)
                sequences[i].OnFinished -= OnSequenceFinished;
            sequences.Clear();
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
