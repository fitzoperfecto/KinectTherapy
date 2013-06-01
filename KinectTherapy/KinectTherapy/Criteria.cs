using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Collections;

namespace SWENG
{
    /// <summary>
    /// Contains the criteria for an exercise.  
    /// 
    /// Transformed from serialized data format.
    /// 
    /// Will contain a map of the joints to be tracked, what axis should be tracked and the variance allowed
    /// </summary>
    class Criteria
    {
        public IDictionary<JointType, Criterion[]> startingJoints = new Dictionary<JointType,Criterion[]>();
        public IDictionary<JointType, Criterion[]> trackedJoints = new Dictionary<JointType, Criterion[]>();
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
            // loop through each joint and determine if it is bad or not.
            foreach (KeyValuePair<JointType, Criterion[]> trackedJoint in trackedJoints)
            {
                double percentBad = 0.0;
                
                foreach (Criterion criterion in trackedJoint.Value)
                {
                    if (!criterion.matchesCriterion(skeletonStamp))
                    {
                        percentBad = 1.0;
                    }
                }
                // store into an array indexed by joint id. 
                jointAccuracy[(int)trackedJoint.Key] = percentBad;
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
            bool matches=true;
            // go through each joint's criteria and verify it is true
            foreach (Criterion[] criterion in startingJoints.Values)
            {
                foreach (Criterion c in criterion)
                {
                    if (!c.matchesCriterion(skeletonStamp))
                    {
                        // no need to keep checking. 
                        return false;
                    }
                }
            }
            return matches;

        }

        public void addStartingCriterion(JointType type, Criterion[] criterion)
        {
            startingJoints.Add(type, criterion);
        }

        public void addTrackingCriterion(JointType type, Criterion[] criterion)
        {
            trackedJoints.Add(type, criterion);
        }
    }
}
