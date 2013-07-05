using System;
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
                return 0f + 1f * Variance * .01f;
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

        public override bool matchesCriterion(SkeletonStamp skeletonStamp)
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
                    alignmentValue = JointAnalyzer.areJointsAligned(centerJoint, trackedJoints);
                    break;
                case Alignment.Horizontal:
                    alignmentValue = JointAnalyzer.alignedHorizontally(trackedJoints[0], trackedJoints[1]);
                    break;
                case Alignment.Vertical:
                    alignmentValue = JointAnalyzer.alignedVertically(trackedJoints[0], trackedJoints[1]);
                    break;
            }
            Debug.WriteLine("Align:" + Alignment + " Joint:" + trackedJoints[0].JointType.ToString() + " val:" + alignmentValue + " Min:" + MinimumAcceptedRange + " Max:" + MaximumAcceptedRange);
            return alignmentValue > MinimumAcceptedRange && alignmentValue < MaximumAcceptedRange;
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
                    tempValue = JointAnalyzer.areJointsAligned(centerJoint, trackedJoints);
                    alignmentValue[(int)CenterJoint.GetJointType()] = tempValue;
                    alignmentValue[(int)trackedJoints[0].JointType] = tempValue;
                    alignmentValue[(int)trackedJoints[1].JointType] = tempValue;
                    break;
                case Alignment.Horizontal:
                    tempValue = JointAnalyzer.alignedHorizontally(trackedJoints[0], trackedJoints[1]);
                    alignmentValue[(int)trackedJoints[0].JointType] = tempValue;
                    alignmentValue[(int)trackedJoints[1].JointType] = tempValue;
                    break;
                case Alignment.Vertical:
                    tempValue = JointAnalyzer.alignedVertically(trackedJoints[0], trackedJoints[1]);
                    alignmentValue[(int)trackedJoints[0].JointType] = tempValue;
                    alignmentValue[(int)trackedJoints[1].JointType] = tempValue;
                    break;
            }
            return alignmentValue;
        }
    }


    [Serializable()]
    public enum Alignment { Point, Horizontal, Vertical };
}
