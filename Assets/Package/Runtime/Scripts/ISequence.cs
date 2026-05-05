using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LenixSO.Sequences
{
    /// <summary>
    /// Interface defining a sequence of actions, which can be started and ended manually.
    /// </summary>
    public interface ISequence
    {
        public string name { get; set; }
        /// <summary>
        /// Event triggered when the sequence has finished.
        /// </summary>
        public event Action OnFinished;
        
        /// <summary>
        /// Starts the sequence by invoking its begin action.
        /// </summary>
        public void Begin();

        /// <summary>
        /// Forcefully ends the sequence, bypassing the normal finish procedure.
        /// </summary>
        public void End();
    }

}
