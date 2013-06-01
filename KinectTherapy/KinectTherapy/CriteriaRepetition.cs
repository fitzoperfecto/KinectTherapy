using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace SWENG
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
        private Criteria criteria;
        private DateTime startTime;
        private DateTime endTime;
        //***********************************
        public CriteriaRepetition(Criteria criteria)
        {
            this.criteria = criteria;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skeletonStamp"></param>
        /// <returns></returns>
        public bool isRepStarted(SkeletonStamp skeletonStamp)
        {
            bool matches = criteria.matchesCriteria(skeletonStamp);
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
            if (DateTime.Now > endTime)
            {
                matches = criteria.matchesCriteria(skeletonStamp);
            }
            return matches;
        }

        public double[] checkForm(SkeletonStamp skeletonStamp)
        {
            return criteria.checkForm(skeletonStamp);
        }
    }
}
