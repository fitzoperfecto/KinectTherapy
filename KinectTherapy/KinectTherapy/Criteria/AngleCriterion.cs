using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Xml.Serialization;

namespace SWENG.Criteria
{
    /// <summary>
    /// Criteria for an angle between 3 joints
    /// </summary>
    [Serializable()]
    public class AngleCriterion:Criterion
    {
        public float MinimumAngle
        {
            get
            {
                return Angle - Variance;
            }
        }
                
        public float MaximumAngle
        {
            get
            {
                return Angle + Variance;
            }
        }

        [XmlAttribute("Angle")]
        public float Angle {get;set;}
        [XmlElement("Vertex")]
        public XmlJointType Vertex { get; set; }
        [XmlArray("AdjacentJoints")]
        [XmlArrayItem("Joint", typeof(XmlJointType))]
        public XmlJointType[] OtherJoints { get; set; }

        /// <summary>
        /// Empty Constructor Needed for XmlSerializer
        /// </summary>
        public AngleCriterion()
        {
        }

        /// <summary>
        /// Will take the first two joints of the "otherJoints" Array
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="vertex"></param>
        /// <param name="otherJoints"></param>
        public AngleCriterion(float angle, XmlJointType vertex, XmlJointType[] otherJoints,float variance):base(variance)
        {
            this.Angle = angle;
            this.Vertex = vertex;
            this.OtherJoints = new XmlJointType[2];
            for(int i = 0;i<2;i++)
            {
                this.OtherJoints[i] = otherJoints[i];
            }
        }

        /// <summary>
        /// Compares a skeleton against the angle criterion
        /// </summary>
        /// <param name="skeletonStamp"></param>
        /// <returns></returns>
        public override bool matchesCriterion(SkeletonStamp skeletonStamp)
        {
            // get the vertex and other joints off the skeleton stamp
            Joint vertexJoint = skeletonStamp.GetTrackedSkeleton().Joints[Vertex.GetJointType()];
            Joint[] adjacentJoints = new Joint[2];
            adjacentJoints[0] = skeletonStamp.GetTrackedSkeleton().Joints[OtherJoints[0].GetJointType()];
            adjacentJoints[1] = skeletonStamp.GetTrackedSkeleton().Joints[OtherJoints[1].GetJointType()];
           
            int convertedDotAngle = JointAnalyzer.findAngle(vertexJoint,adjacentJoints);
            Debug.WriteLine("Angle: " + convertedDotAngle);
            return convertedDotAngle > MinimumAngle && convertedDotAngle < MaximumAngle;
        }
    }
}
