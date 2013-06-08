using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Collections;

namespace SWENG.Criteria
{
    /// <summary>
    /// Contains the criteria for an exercise.  
    /// 
    /// Transformed from serialized data format.
    /// 
    /// Will contain a map of the joints to be tracked, what axis should be tracked and the variance allowed
    /// </summary>
    public class Exercise
    {
        public string Name { get; set; }
        public Criterion[] StartingCriteria { get; set; }
        public Criterion[] TrackingCriteria { get; set; }
        public int Repetitions { get; set; }
        /// <summary>
        /// Move this to an interface
        /// Check form will validate all the tracked joints based on the criteria provided by trackedJoints
        ///
        /// </summary>
        /// <param name="original">The skeleton provided at the start of the repetition</param>
        /// <param name="current">The current skeleton during the repetition</param>
        /// <returns></returns>
        public double[] checkForm(SkeletonStamp skeletonStamp)
        {
            double[] jointAccuracy = new double[20];
            // loop through each joint and determine if it is bad or not
            foreach (Criterion criterion in TrackingCriteria)
            {
                double percentBad = 0.0;
                if (!criterion.matchesCriterion(skeletonStamp))
                {
                    percentBad = 1.0;
                }
                // store into an array indexed by joint id. 
                //jointAccuracy[(int)trackedJoint.Key] = percentBad;
            }
            return jointAccuracy;
        }

        /// <summary>
        /// Determines whether the supplied skeleton matches the criteria
        /// </summary>
        /// <param name="skeletonStamp"></param>
        /// <returns></returns>
        internal bool matchesCriteria(SkeletonStamp skeletonStamp)
        {
            bool matches = true;
            // go through each joint's criteria and verify it is true
            foreach (Criterion c in StartingCriteria)
            {
                if (!c.matchesCriterion(skeletonStamp))
                {
                    // no need to keep checking. 
                    return false;
                }
            }

            return matches;
        }
    }
}
