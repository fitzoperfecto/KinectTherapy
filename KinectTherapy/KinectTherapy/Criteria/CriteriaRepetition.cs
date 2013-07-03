using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace SWENG.Criteria
{
    /// <summary>
    /// This class expects a criteria for the start/stop position to be provided 
    /// and the criteria will ensure the skeleton will match the criteria position.
    /// 
    /// </summary>
    // publisher
    public delegate void CheckpointChangedEventHandler(object sender, CheckpointChangedEventArgs e);

    public class CheckpointChangedEventArgs : EventArgs
    {
        public string FileId;

        public CheckpointChangedEventArgs(string fileId)
        {
            FileId = fileId;
        }
    }
    public class Repetition:IRepetition
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

        private Exercise Exercise;
        private DateTime startTime;
        private DateTime endTime;
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
                    OnChanged(new CheckpointChangedEventArgs(Exercise.Id + value));
                }
                _checkpoint = value;
            }
        }

        //***********************************
        public Repetition(Exercise criteria)
        {
            this.Exercise = criteria;
            this.Checkpoint = 0;
        }

        /// <summary>
        /// Determines if the repetition has been started
        /// </summary>
        /// <param name="skeletonStamp"></param>
        /// <returns></returns>
        public bool isRepStarted(SkeletonStamp skeletonStamp)
        {
            bool matches = Exercise.matchesCriteria(skeletonStamp,Exercise.StartingCriteria);
            if (matches)
            {
                startTime = DateTime.Now;
                /// add 2 seconds (hopefully they have moved within that time)
                TimeSpan time = new TimeSpan(0, 0, 0, 2);
                endTime = startTime.Add(time);
            }
            return matches;
        }

        /// <summary>
        /// Determines whether the rep has been completed. 
        /// </summary>
        /// <param name="skeletonStamp"></param>
        /// <returns></returns>
        public bool isRepComplete(SkeletonStamp skeletonStamp)
        {
            bool matches = false;
            // see if we completed the current checkpoint.
            matches = Exercise.matchesCriteria(skeletonStamp,Exercise.Checkpoints[_checkpoint].Criteria);
            // if we completed the current checkpoint increment and update the checkpoint picture.
            if (matches)
            {
                // increment the checkpoint
                Checkpoint++;

                if (Checkpoint >= Exercise.Checkpoints.Length)
                {
                    // we have now completed a repetition reset our counter
                    Checkpoint = 0;
                    return true;
                }
            }

            // Issue #11 fix:
            // I don't like this but for now it will do. If the timer has gone off you have gone back to starting position consider rep complete.
            matches = Exercise.matchesCriteria(skeletonStamp,Exercise.StartingCriteria);
            if (matches && endTime < DateTime.Now)
            {
                // short circuit (Man Johnny Five is Alive)
                Checkpoint = 0;
                return true;
            }
            return false;
        }

        public double[] checkForm(SkeletonStamp skeletonStamp)
        {
            return Exercise.checkForm(skeletonStamp);
        }
    }
}
