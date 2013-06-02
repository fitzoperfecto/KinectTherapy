using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace SWENG.Criteria
{
    /// <summary>
    /// Criteria for an angle between 3 joints
    /// </summary>
    public class AngleCriterion:Criterion
    {
        float minAngle;
        float maxAngle;
        JointType vertex;
        JointType[] otherJoints;

        /// <summary>
        /// Will take the first two joints of the "otherJoints" Array
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="vertex"></param>
        /// <param name="otherJoints"></param>
        public AngleCriterion(float angle, JointType vertex, JointType[] otherJoints,float variance):base(variance)
        {
            this.minAngle = angle - variance;
            this.maxAngle = angle + variance;
            this.vertex = vertex;
            this.otherJoints = new JointType[2];
            for(int i = 0;i<2;i++)
            {
                this.otherJoints[i] = otherJoints[i];
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
            Joint vertexJoint = skeletonStamp.GetTrackedSkeleton().Joints[vertex];
            Joint joint0 = skeletonStamp.GetTrackedSkeleton().Joints[otherJoints[0]];
            Joint joint1 = skeletonStamp.GetTrackedSkeleton().Joints[otherJoints[1]];
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
            return convertedDotAngle > minAngle && convertedDotAngle < maxAngle;
        }
    }
}
