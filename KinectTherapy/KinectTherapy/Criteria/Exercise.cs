using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Collections;
using System.Xml.Serialization;

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

        /// <summary>
        /// Empty Constructor Needed for XmlSerializer
        /// </summary>
        public Exercise()
        {
        }

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
            // loop through each joint and determine if it is bad or 
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
