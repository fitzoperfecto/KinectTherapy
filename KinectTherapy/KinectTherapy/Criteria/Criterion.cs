using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Xml.Serialization;

namespace SWENG.Criteria
{
    /// <summary>
    /// Simple object to contain a single criterion of a Joint's exercise criteria
    /// </summary>
    [Serializable()]
    [XmlInclude(typeof(AlignmentCriterion))]
    [XmlInclude(typeof(AngleCriterion))]
    [XmlRoot("Criterion")]
    public abstract class Criterion
    {
        [XmlAttribute("Variance")]
        public float Variance { get; set; }

        /// <summary>
        /// Empty Constructor Needed for XmlSerializer
        /// </summary>
        public Criterion()
        {
        }
        public Criterion(float variance)
        {
            this.Variance = variance;
        }

        public abstract bool MatchesCriterion(SkeletonStamp skeletonStamp);

        public abstract double[] CheckForm(SkeletonStamp skeletonStamp);

        public abstract List<Joint> MatchSkeletonToCriterion();
    }
}
