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
            Joint joint0 = skeletonStamp.GetTrackedSkeleton().Joints[OtherJoints[0].GetJointType()];
            Joint joint1 = skeletonStamp.GetTrackedSkeleton().Joints[OtherJoints[1].GetJointType()];
            // get vector vertex -> joint0
            
            Vector3 vector0 = new Vector3(joint0.Position.X - vertexJoint.Position.X, joint0.Position.Y - vertexJoint.Position.Y, joint0.Position.Z - vertexJoint.Position.Z);
            // get vector vertex -> joint1
            Vector3 vector1 = new Vector3(joint1.Position.X - vertexJoint.Position.X, joint1.Position.Y - vertexJoint.Position.Y, joint1.Position.Z - vertexJoint.Position.Z);

            //// determine the angle from two vectors in a 2D space...
           // the old way
            //float computedAngle = (float)Math.Atan2(vector0.Y, vector0.X) - (float)Math.Atan2(vector1.Y, vector1.X);
            //// compare to the varience
            //int convertedAngle = Convert.ToInt32(computedAngle * (180.0 / Math.PI));
            
            // the right way we're in 3D
            // using vector provided methods
            vector0.Normalize();
            vector1.Normalize();
            float dotAngle = (float)Math.Acos(Vector3.Dot(vector0, vector1));
            int convertedDotAngle = Convert.ToInt32(dotAngle * (180.0 / Math.PI));
            Debug.WriteLine("Angle: " + convertedDotAngle);
            return convertedDotAngle > MinimumAngle && convertedDotAngle < MaximumAngle;
        }
    }
}
