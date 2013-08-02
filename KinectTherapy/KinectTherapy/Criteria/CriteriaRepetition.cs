using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace SWENG.Criteria
{
    public delegate void CheckpointChangedEventHandler(object sender, CheckpointChangedEventArgs e);
    public class CheckpointChangedEventArgs : EventArgs
    {
        public string FileId;

        public CheckpointChangedEventArgs(string fileId)
        {
            FileId = fileId;
        }
    }

    /// <summary>
    /// This class expects a criteria for the start/stop position to be provided 
    /// and the criteria will ensure the skeleton will match the criteria position.
    /// </summary>
    public class Repetition : IRepetition
    {

        #region event stuff
        public event CheckpointChangedEventHandler CheckpointChanged;

        // Invoke the Changed event; called whenever checkpoint changes
        protected virtual void OnChanged(CheckpointChangedEventArgs e)
        {
            if (CheckpointChanged != null)
                CheckpointChanged(this, e);
        }

        #endregion

        private Exercise _exercise;
        private int _checkpoint;

        public int Checkpoint
        {
            get
            {
                return
                    _checkpoint;
            }
            internal set
            {
                if (_checkpoint != value)
                {
                    // if the checkpoint value changes send a new event.
                    OnChanged(new CheckpointChangedEventArgs(_exercise.Id + value));
                }
                _checkpoint = value;
            }
        }

        //***********************************
        public Repetition(Exercise exercise)
        {
            this._exercise = exercise;
            this.Checkpoint = 0;
        }

        /// <summary>
        /// Determines if the patient is in the starting position
        /// </summary>
        /// <param name="skeletonStamp"></param>
        /// <returns></returns>
        public bool isRepStarted(SkeletonStamp skeletonStamp)
        {
            bool matches = _exercise.MatchesCriteria(skeletonStamp, _exercise.StartingCriteria);
            return matches;
        }

        /// <summary>
        /// Determines whether the rep has been completed by the patient
        /// </summary>
        /// <param name="skeletonStamp"></param>
        /// <returns></returns>
        public bool isRepComplete(SkeletonStamp skeletonStamp)
        {
            bool matches = false;

            matches = _exercise.MatchesCriteria(skeletonStamp, _exercise.Checkpoints[_checkpoint].Criteria);
            if (matches)
            {
                // increment the checkpoint
                Checkpoint++;

                if (Checkpoint >= _exercise.Checkpoints.Length)
                {
                    // we have now completed a repetition reset our counter
                    Checkpoint = 0;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check the form of the patient while they perform the repetition
        /// </summary>
        /// <param name="skeletonStamp"></param>
        /// <returns></returns>
        public double[] checkForm(SkeletonStamp skeletonStamp)
        {
            return _exercise.CheckForm(skeletonStamp);
        }
    }
}
