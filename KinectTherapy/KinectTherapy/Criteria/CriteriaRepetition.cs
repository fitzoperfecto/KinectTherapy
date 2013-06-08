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
    /// This class expects the start and stop to be the same position. But will expect that the rep 
    /// must take more than 2 seconds to complete. 
    /// </summary>
    class CriteriaRepetition:IRepetition
    {
        private Exercise Exercise;
        private DateTime startTime;
        private DateTime endTime;
        private int _checkpoint;

        //***********************************
        public CriteriaRepetition(Exercise criteria)
        {
            this.Exercise = criteria;
            this._checkpoint = 0;
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

            matches = Exercise.matchesCriteria(skeletonStamp,Exercise.Checkpoints[_checkpoint].Criteria);
            if (matches)
            {
                // increment the checkpoint
                _checkpoint++;

                if (_checkpoint >= Exercise.Checkpoints.Length)
                {
                    // we have now completed a repetition reset our counter
                    _checkpoint = 0;
                    return true;
                }
            }
            
            return false;
        }

        public double[] checkForm(SkeletonStamp skeletonStamp)
        {
            return Exercise.checkForm(skeletonStamp);
        }
    }
}
