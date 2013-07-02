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
    /// The purpose of the joint analyzer is to encapsulate the mathematics behind comparing joints.
    /// These methods should return a numerical result representing the degree off perfect alignment.
    /// 
    /// </summary>
    public class JointAnalyzer
    {
        /// <summary>
        /// to see if the joints are aligned we must choose one joint as the starting point (NOT THE CENTER) and compare to the other two joints
        /// the Dot product will determine if they are in alignment. 
        /// 1 = parallel; 0 = perpendicular; -1 = pointing opposite directions
        /// </summary>
        /// <param name="centerJoint"></param>
        /// <param name="otherJoints"></param>
        /// <returns></returns>
        public static float areJointsAligned(Joint centerJoint, Joint[] otherJoints)
        {
            // 
            Joint startingJoint = otherJoints[0];
            Joint endingJoint = otherJoints[1];

            Vector3 vector0 = new Vector3(centerJoint.Position.X - startingJoint.Position.X, centerJoint.Position.Y - startingJoint.Position.Y, centerJoint.Position.Z - startingJoint.Position.Z);
            // get vector vertex -> joint1
            Vector3 vector1 = new Vector3(endingJoint.Position.X - startingJoint.Position.X, endingJoint.Position.Y - startingJoint.Position.Y, endingJoint.Position.Z - startingJoint.Position.Z);

            // Normalize vectors so we can use Dot product correctly
            vector0.Normalize();
            vector1.Normalize();
            return Vector3.Dot(vector0, vector1);
        }

        /// <summary>
        /// Negative response means aligning joint is below joint to be aligned to
        /// Positive response means aligning joint is above joint to be aligned to. 
        /// 0 means they are at the same level
        /// </summary>
        /// <param name="aligningJoint"></param>
        /// <param name="jointToBeAlignedTo"></param>
        /// <returns></returns>
        public static float alignedHorizontally(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            // KISS then expand
            return aligningJoint.Position.Y - jointToBeAlignedTo.Position.Y;
        }

        /// <summary>
        /// Negative response means aligning joint is to the left of joint to be aligned to
        /// Positive response means aligning joint is to the right of joint to be aligned to. 
        /// 0 means they are at the same level
        /// </summary>
        /// <param name="aligningJoint"></param>
        /// <param name="jointToBeAlignedTo"></param>
        /// <returns></returns>
        public static float alignedVertically(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            return aligningJoint.Position.X - jointToBeAlignedTo.Position.X;
        }

        /// <summary>
        /// Determines if the aligningJoint is to the left of the jointToBeAlignedTo
        /// </summary>
        /// <param name="aligningJoint"></param>
        /// <param name="jointToBeAlignedTo"></param>
        /// <returns></returns>
        public static bool isLeftOf(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            return alignedVertically(aligningJoint,jointToBeAlignedTo) < 0;
        }

        /// <summary>
        /// Determines if the aligningJoint is to the right of the jointToBeAlignedTo
        /// </summary>
        /// <param name="aligningJoint"></param>
        /// <param name="jointToBeAlignedTo"></param>
        /// <returns></returns>
        public static bool isRightOf(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            return alignedVertically(aligningJoint, jointToBeAlignedTo) > 0;
        }

        /// <summary>
        /// Determines if the aligningJoint is above the jointToBeAlignedTo
        /// </summary>
        /// <param name="aligningJoint"></param>
        /// <param name="jointToBeAlignedTo"></param>
        /// <returns></returns>
        public static bool isAbove(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            return alignedHorizontally(aligningJoint, jointToBeAlignedTo) > 0;
        }

        /// <summary>
        /// Determines if the aligningJoint is below the jointToBeAlignedTo
        /// </summary>
        /// <param name="aligningJoint"></param>
        /// <param name="jointToBeAlignedTo"></param>
        /// <returns></returns>
        public static bool isBelow(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            return alignedHorizontally(aligningJoint, jointToBeAlignedTo) < 0;
        }

        /// <summary>
        /// Determines if the aligningJoint is in front of the jointToBeAlignedTo
        /// </summary>
        /// <param name="aligningJoint"></param>
        /// <param name="jointToBeAlignedTo"></param>
        /// <returns></returns>
        public static bool isAhead(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            return alignedHorizontally(aligningJoint, jointToBeAlignedTo) < 0;
        }

        /// <summary>
        /// Determines if the aligningJoint is behind the jointToBeAlignedTo
        /// </summary>
        /// <param name="aligningJoint"></param>
        /// <param name="jointToBeAlignedTo"></param>
        /// <returns></returns>
        public static bool isBehind(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            return alignedHorizontally(aligningJoint, jointToBeAlignedTo) < 0;
        }

        /// <summary>
        /// Determines the angle between three joints
        /// </summary>
        /// <param name="vertexJoint"></param>
        /// <param name="adjacentJoints"></param>
        /// <returns></returns>
        public static int findAngle(Joint vertexJoint, Joint[] adjacentJoints)
        {
            // get the vertex and other joints off the skeleton stamp
            Joint joint0 = adjacentJoints[0];
            Joint joint1 = adjacentJoints[1];
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
            int convertedDotAngle;

            // got into a NaN situation when leaving the screen
            if (!float.IsNaN(dotAngle))
                convertedDotAngle = Convert.ToInt32(dotAngle * (180.0 / Math.PI));
            else
                convertedDotAngle = 0;

            return convertedDotAngle;
        }
    }
}
