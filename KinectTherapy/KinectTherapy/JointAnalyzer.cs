using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

namespace SWENG
{
    /// <summary>
    /// The purpose of the joint analyzer is to encapsulate the mathematics behind comparing joints.
    /// These methods should return a numerical result representing the degree off perfect alignment.
    /// 
    /// </summary>
    class JointAnalyzer
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

        public static bool isLeftOf(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            return alignedVertically(aligningJoint,jointToBeAlignedTo) < 0;
        }

        public static bool isRightOf(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            return alignedVertically(aligningJoint, jointToBeAlignedTo) > 0;
        }

        public static bool isAbove(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            return alignedHorizontally(aligningJoint, jointToBeAlignedTo) > 0;
        }

        public static bool isBelow(Joint aligningJoint, Joint jointToBeAlignedTo)
        {
            return alignedHorizontally(aligningJoint, jointToBeAlignedTo) < 0;
        }

    }
}
