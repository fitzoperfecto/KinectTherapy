using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SWENG
{
    /// <summary>
    /// Test implementation of the IRepetition Just counts a rep every 3 seconds. 
    /// </summary>
    class TimerRepetition:IRepetition
    {
        // these are just going to be used to test
        private DateTime startTime;
        private DateTime endTime;
        //***********************************

        public TimerRepetition()
        {
        }
        
        /// <summary>
        /// 
        /// Always starts the rep. We ignore the skeletonStamp in this case
        /// </summary>
        /// <returns></returns>
        public bool isRepStarted(SkeletonStamp skeletonStamp)
        {
            startTime = DateTime.Now;
            /// add 30 seconds
            TimeSpan time = new TimeSpan(0, 0, 0, 3);
            endTime = startTime.Add(time);
            return true;
        }

        /// <summary>
        /// 
        /// Ends the rep after the current time is greater than the end time created upon the start of the rep. 
        /// </summary>
        /// <returns></returns>
        public bool isRepComplete(SkeletonStamp skeletonStamp)
        {
            return DateTime.Now > endTime;
        }

        public double[] checkForm(SkeletonStamp skeletonStamp)
        {
            return new double[20];
        }
    }
}
