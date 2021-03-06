﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Xml.Serialization;
using System.Diagnostics;

namespace SWENG.Criteria
{
    [Serializable()]
    public class AlignmentCriterion : Criterion
    {
        [XmlArray("Joints")]
        [XmlArrayItem("Joint")]
        public XmlJointType[] Joints;
        [XmlElement("CenterJoint")]
        public XmlJointType CenterJoint;
        [XmlAttribute("Alignment")]
        public Alignment Alignment;
        public float MinimumAcceptedRange
        {
            get
            {
                return 0f - 1f * Variance * .01f;
            }
        }
        public float MaximumAcceptedRange
        {
            get
            {
                return 1f - (Variance / 100);
            }
        }

        /// <summary>
        /// Empty Constructor Needed for XmlSerializer
        /// </summary>
        public AlignmentCriterion()
        {
        }

        public AlignmentCriterion(XmlJointType[] joints, Alignment alignment, float variance)
            : base(variance)
        {
            this.Alignment = alignment;
            this.Joints = joints;
        }

        public AlignmentCriterion(XmlJointType centerJoint, XmlJointType[] joints, float variance)
            : base(variance)
        {
            this.Alignment = Alignment.Point;
            this.CenterJoint = centerJoint;
            this.Joints = joints;
        }

        public override bool MatchesCriterion(SkeletonStamp skeletonStamp)
        {
            double alignmentValue = 0.0;
            Joint[] trackedJoints = new Joint[Joints.Length];
            Joint centerJoint;
            int i = 0;
            // get the joints to be aligned
            foreach (XmlJointType type in Joints)
            {
                trackedJoints[i++] = skeletonStamp.GetTrackedSkeleton().Joints[type.GetJointType()];
            }
            switch (Alignment)
            {
                case Alignment.Point:
                    centerJoint = skeletonStamp.GetTrackedSkeleton().Joints[CenterJoint.GetJointType()];
                    alignmentValue = JointAnalyzer.AreJointsAligned(centerJoint, trackedJoints);
                    break;
                case Alignment.Horizontal:
                    alignmentValue = JointAnalyzer.AlignedHorizontally(trackedJoints[0], trackedJoints[1]);
                    break;
                case Alignment.Vertical:
                    alignmentValue = JointAnalyzer.AlignedVertically(trackedJoints[0], trackedJoints[1]);
                    break;
            }
            return alignmentValue > MaximumAcceptedRange;
        }


        public override double[] CheckForm(SkeletonStamp skeletonStamp)
        {
            return FindAlignment(skeletonStamp);
        }

        /// <summary>
        /// Checks the alignment of the joints. returns a value for each joint measured
        /// </summary>
        /// <param name="skeletonStamp"></param>
        /// <returns></returns>
        private double[] FindAlignment(SkeletonStamp skeletonStamp)
        {
            // initialize the values of the array to -2 since the value we're returning is between -1 and +1
            double[] alignmentValue = Enumerable.Repeat(-999.0, 20).ToArray();
            Joint[] trackedJoints = new Joint[Joints.Length];
            Joint centerJoint;
            int i = 0;
            // get the joints to be aligned
            foreach (XmlJointType type in Joints)
            {
                trackedJoints[i++] = skeletonStamp.GetTrackedSkeleton().Joints[type.GetJointType()];
            }
            double tempValue;
            switch (Alignment)
            {
                case Alignment.Point:
                    centerJoint = skeletonStamp.GetTrackedSkeleton().Joints[CenterJoint.GetJointType()];
                    tempValue = 1 - JointAnalyzer.AreJointsAligned(centerJoint, trackedJoints);
                    alignmentValue[(int)CenterJoint.GetJointType()] = tempValue;
                    alignmentValue[(int)trackedJoints[0].JointType] = tempValue;
                    alignmentValue[(int)trackedJoints[1].JointType] = tempValue;
                    break;
                case Alignment.Horizontal:
                    tempValue = 1 - JointAnalyzer.AlignedHorizontally(trackedJoints[0], trackedJoints[1]);
                    alignmentValue[(int)trackedJoints[0].JointType] = tempValue;
                    alignmentValue[(int)trackedJoints[1].JointType] = tempValue;
                    break;
                case Alignment.Vertical:
                    tempValue = 1 - JointAnalyzer.AlignedVertically(trackedJoints[0], trackedJoints[1]);
                    alignmentValue[(int)trackedJoints[0].JointType] = tempValue;
                    alignmentValue[(int)trackedJoints[1].JointType] = tempValue;
                    break;
            }

            return alignmentValue;
        }

        /// <summary>
        /// This is a future enhancement to create a skeleton based upon the current skeletonstamp and the criteria for the exercise.
        /// </summary>
        /// <returns></returns>
        public override List<Joint> MatchSkeletonToCriterion()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// String representation of the Criterion used for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Alignment: ");
            sb.Append(Alignment);
            if (CenterJoint != null)
            {
                sb.Append("\nCenterJoint: ");
                sb.Append(CenterJoint.GetJointType().ToString());
            }
            sb.Append("\nJoint1: ");
            sb.Append(Joints[0].GetJointType().ToString());
            sb.Append("\nJoint2: ");
            sb.Append(Joints[1].GetJointType().ToString());
            return sb.ToString();
        }
    }


    [Serializable()]
    public enum Alignment { Point, Horizontal, Vertical };
}
