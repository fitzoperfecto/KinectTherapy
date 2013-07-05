using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Collections;
using System.Xml.Serialization;
using System.Diagnostics;

namespace SWENG.Criteria
{
    /// <summary>
    /// Contains the criteria for an exercise.  
    /// 
    /// Transformed from serialized data format.
    /// 
    /// Will contain a map of the joints to be tracked, what axis should be tracked and the variance allowed
    /// </summary>
    /// 
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("Exercise")]
    public class Exercise
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlArray("StartingCriteria")]
        [XmlArrayItem("Criterion")]
        public Criterion[] StartingCriteria { get; set; }
        [XmlArray("TrackingCriteria")]
        [XmlArrayItem("Criterion")]
        public Criterion[] TrackingCriteria { get; set; }
        [XmlArray("Checkpoints")]
        [XmlArrayItem(ElementName = "Checkpoint", NestingLevel = 0)]
        public Checkpoint[] Checkpoints { get; set; }
        [XmlAttribute("Repetitions")]
        public int Repetitions { get; set; }
        [XmlAttribute("Description")]
        public string Description { get; set; }
        [XmlAttribute("Category")]
        public string Category { get; set; }
        [XmlAttribute("Variance")]
        public float Variance
        {
            get
            {
                return _variance;
            }

            set
            {
                // must set the variance of all the criterion as well
                if (null != Checkpoints)
                {
                    foreach (Checkpoint cp in Checkpoints)
                    {
                        UpdateVariance(value, cp.Criteria);
                    }
                }
                if (null != StartingCriteria)
                {
                    UpdateVariance(value, StartingCriteria);
                }
                if (null != TrackingCriteria)
                {
                    UpdateVariance(value, TrackingCriteria);
                }
                _variance = value;
            }
        }
        private float _variance;
        private void UpdateVariance(float newVariance, Criterion[] Criteria)
        {
            foreach (Criterion criterion in Criteria)
            {
                criterion.Variance = newVariance;
            }
        }
        /// <summary>
        /// Empty Constructor Needed for XmlSerializer
        /// </summary>
        public Exercise()
        {
        }

        /// <summary>
        /// Check form will validate all the tracked joints based on the criteria provided by trackedJoints
        /// Joints which are not compared are considered perfectly accurate (0) 
        /// </summary>
        /// <returns></returns>
        public double[] CheckForm(SkeletonStamp skeletonStamp)
        {
            double[] jointAccuracy = new double[20];
            /* Keeps track of how many times the joint has been updated */
            double[] timesJointUpdated = new double[20];
            /** loop through each Criterion and determine if it is bad or not */
            /** aggregate all the results */
            foreach (Criterion criterion in TrackingCriteria)
            {
                double[] result = criterion.CheckForm(skeletonStamp);

                for (int i = 0; i < jointAccuracy.Length; i++)
                {
                    if (result[i] != -999)
                    {
                        jointAccuracy[i] += result[i];
                        timesJointUpdated[i]++;
                    }

                }
            }
            /* Take the average result based on the amount of times a joint was updated */
            for (int i = 0; i < jointAccuracy.Length; i++)
            {
                jointAccuracy[i] = jointAccuracy[i] / timesJointUpdated[i];
            }
            /** Joints which are not compared are considered perfectly accurate (0) */
            return jointAccuracy;
        }

        /// <summary>
        /// Determines whether the supplied skeleton matches the criteria
        /// </summary>
        /// <param name="skeletonStamp"></param>
        /// <returns></returns>
        internal bool matchesCriteria(SkeletonStamp skeletonStamp, Criterion[] criterion)
        {
            bool matches = true;
            // go through each joint's criteria and verify it is true
            foreach (Criterion c in criterion)
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

    [Serializable()]
    [System.Xml.Serialization.XmlRoot("Workout")]
    public class Workout
    {
        [XmlArray("Exercises")]
        [XmlArrayItem("Exercise", typeof(Exercise))]
        public Exercise[] Exercises { get; set; }
    }
}
