using System;

namespace LenixSO.Sequences.Decorator
{
    /// <summary>
    /// A customizable sequence of actions that allows you to specify custom begin and end actions.
    /// </summary>
    public class CustomSequence : ISequence
    {
        public string name { get; set; }
        public event Action OnFinished;
        /// <summary>
        /// Should the sequence check if Begin has been called before calling End?
        /// </summary>
        public bool ignoreStartedOnEnd = false;

        private Action begin;
        private Action end;
        private bool started;

        /// <summary>
        /// Initializes a new sequence where a single action is executed on Begin, followed by finishing the sequence.
        /// </summary>
        /// <param name="action">The action to execute when the sequence begins.</param>
        public CustomSequence(Action action)
        {
            begin = action;
            begin += End; // Automatically call End after the action completes
            end = FinishSequence; // End action
        }

        /// <summary>
        /// Initializes a sequence with customizable begin and end actions.
        /// </summary>
        /// <param name="beginAction">The action to execute when the sequence begins.</param>
        /// <param name="endAction">The action to execute when the sequence ends.</param>
        public CustomSequence(Action beginAction, Action endAction)
        {
            begin = beginAction;
            end = endAction;
            end += FinishSequence; // Automatically finish the sequence when end action completes
        }

        /// <summary>
        /// Starts the sequence by invoking the specified begin action.
        /// </summary>
        public void Begin()
        {
            if (started) return; // Prevent starting if the sequence has already started
            started = true;
            begin?.Invoke();
        }

        /// <summary>
        /// Ends the sequence by invoking the specified end action.
        /// </summary>
        public void End()
        {
            if (!(started || ignoreStartedOnEnd)) return; // Prevent ending if the sequence hasn't started
            end?.Invoke();
        }

        /// <summary>
        /// Sets a custom end action for this sequence.
        /// </summary>
        /// <param name="action">The custom action to invoke when the sequence ends.</param>
        /// <returns>Returns the current sequence instance for method chaining.</returns>
        public CustomSequence SetEndAction(Action action)
        {
            end = action;
            end += FinishSequence; // Automatically finish the sequence when end action completes
            return this;
        }

        /// <summary>
        /// Marks the sequence as finished and triggers the OnFinished event.
        /// </summary>
        public void FinishSequence()
        {
            started = false;
            OnFinished?.Invoke();
        }

        /// <summary>
        /// Creates an empty sequence that does nothing when executed.
        /// </summary>
        /// <returns>A new instance of an empty sequence.</returns>
        public static CustomSequence EmptySequence() => new(null) { name = "Empty" };

        public override string ToString()
        {
            return $"Custom({name})";
        }
    }
}
