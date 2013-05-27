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
        public IDictionary<JointType, Criterion[]> trackedJoints = new Dictionary<JointType,Criterion[]>();

        /// <summary>
        /// Move this to an interface
        /// Check form will validate all the tracked joints based on the criteria provided by trackedJoints
        ///
        /// </summary>
        /// <param name="original">The skeleton provided at the start of the repetition</param>
        /// <param name="current">The current skeleton during the repetition</param>
        /// <returns></returns>
        //public List<FormResult> checkForm(Skeleton original, Skeleton current)
        //{
        //    List<FormResult> results = new List<FormResult>();
        //    foreach(KeyValuePair<JointType,Criterion[]> trackedJoint in trackedJoints)
        //    {
        //        FormResult result = new FormResult();
        //        result.JointType = trackedJoint.Key;
        //        // get the original joint based on current tracked joint being processed
        //        Joint originalJoint = original.Joints[trackedJoint.Key];
        //        Joint currentJoint = current.Joints[trackedJoint.Key];
        //        // compute the accepted ranges
        //        // use some reflection here to get the property by the criteria axis value.
        //        foreach(Criterion criterion in trackedJoint.Value)
        //        {
        //           float originalPoint = (float) originalJoint.Position.GetType().GetProperty(criterion.Axis).GetValue(originalJoint,null);
        //           float minRange = originalPoint - criterion.Variance;
        //           float maxRange = originalPoint + criterion.Variance;
        //           // compare against the current joint and update the results
        //           float currentPoint = (float)currentJoint.Position.GetType().GetProperty(criterion.Axis).GetValue(currentJoint, null);
        //           result.InRange = result.InRange && currentPoint >= minRange && currentPoint <= maxRange;
        //           result.addPointVariance(criterion.Axis, originalPoint - currentPoint);
        //        }
        //        results.Add(result);
        //    }
        //    return results; 
        //}

        /// <summary>
        /// Determines whether the supplied skeleton matches the criteria
        /// </summary>
        /// <param name="skeletonStamp"></param>
        /// <returns></returns>
        internal bool matchesCriteria(SkeletonStamp skeletonStamp)
        {
            bool matches=true;
            // go through each joint's criteria and verify it is true
            foreach (Criterion[] criterion in trackedJoints.Values)
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

        public void addCriterion(JointType type, Criterion[] criterion)
        {
            trackedJoints.Add(type, criterion);
        }

    }
}
