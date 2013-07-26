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
    public class AngleCriterion : Criterion
    {
        public float MinimumAngle
        {
            get
            {
                return Angle - ((180 * Variance * .01f) / 2);
            }
        }

        public float MaximumAngle
        {
            get
            {
                return Angle + ((180 * Variance * .01f) / 2);
            }
        }

        [XmlAttribute("Angle")]
        public float Angle { get; set; }
        [XmlElement("Vertex")]
        public XmlJointType Vertex { get; set; }
        [XmlArray("AdjacentJoints")]
        [XmlArrayItem("Joint", typeof(XmlJointType))]
        public XmlJointType[] OtherJoints { get; set; }
        [XmlAttribute("Operation")]
        public Operation Operation { get; set; }

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
        public AngleCriterion(float angle, XmlJointType vertex, XmlJointType[] otherJoints, float variance)
            : base(variance)
        {
            this.Angle = angle;
            this.Vertex = vertex;
            this.OtherJoints = new XmlJointType[2];
            for (int i = 0; i < 2; i++)
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
            Joint vertexJoint; Joint[] adjacentJoints;
            int convertedDotAngle = FindAngle(skeletonStamp, out vertexJoint, out adjacentJoints);
            return convertedDotAngle > MinimumAngle && convertedDotAngle < MaximumAngle;
        }

        private int FindAngle(SkeletonStamp skeletonStamp, out Joint vertexJoint, out Joint[] adjacentJoints)
        {
            // Test bone orientations
            Skeleton skeleton = skeletonStamp.GetTrackedSkeleton();
            BoneOrientation bo = skeleton.BoneOrientations[Vertex.GetJointType()];

            // get the vertex and other joints off the skeleton stamp
            vertexJoint = skeletonStamp.GetTrackedSkeleton().Joints[Vertex.GetJointType()];

            adjacentJoints = new Joint[2];
            adjacentJoints[0] = skeletonStamp.GetTrackedSkeleton().Joints[OtherJoints[0].GetJointType()];
            adjacentJoints[1] = skeletonStamp.GetTrackedSkeleton().Joints[OtherJoints[1].GetJointType()];
            int convertedDotAngle = JointAnalyzer.findAngle(vertexJoint, adjacentJoints);

            return convertedDotAngle;
        }

        public override double[] CheckForm(SkeletonStamp skeletonStamp)
        {
            Joint vertexJoint; Joint[] adjacentJoints;
            int convertedDotAngle = FindAngle(skeletonStamp, out vertexJoint, out adjacentJoints);

            double normalizedAccuracy = 0;
            switch (Operation)
            {
                case Operation.GreaterThan:
                    if (convertedDotAngle < MinimumAngle)
                    {
                        normalizedAccuracy = (Angle - convertedDotAngle) / 180;
                    }
                    break;
                case Operation.LessThan:
                    if (convertedDotAngle > MaximumAngle)
                    {
                        normalizedAccuracy = (Angle - convertedDotAngle) / 180;
                    }
                    break;
                case Operation.Equals:
                    normalizedAccuracy = (Angle - convertedDotAngle) / 180;
                    break;
            }
            double[] results = new double[20];
            results[(int)vertexJoint.JointType] = normalizedAccuracy;
            results[(int)adjacentJoints[0].JointType] = normalizedAccuracy;
            results[(int)adjacentJoints[1].JointType] = normalizedAccuracy;
            return results;
        }

        public override List<Joint> MatchSkeletonToCriterion()
        {
            List<JointType> jointTypes = new List<JointType>();

            jointTypes.Add(Vertex.GetJointType());
            foreach (XmlJointType xmlJointType in OtherJoints)
            {
                jointTypes.Add(xmlJointType.GetJointType());
            }
            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Angle: ");
            sb.Append(Angle);
            sb.Append("\nVertex: ");
            sb.Append(Vertex.GetJointType().ToString());
            sb.Append("\nJoint1: ");
            sb.Append(OtherJoints[0].GetJointType().ToString());
            sb.Append("\nJoint2: ");
            sb.Append(OtherJoints[1].GetJointType().ToString());
            sb.Append("\nOperation: ");
            sb.Append(Operation.ToString());


            return sb.ToString();
        }
    }
}
