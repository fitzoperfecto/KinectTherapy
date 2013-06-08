using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Xml.Serialization;

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
                return 0 - Variance;
            }
        }
        public float MaximumAcceptedRange
        {
            get
            {
                return 0 + Variance;
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
            float alignmentValue = 0.0f;
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
            return alignmentValue > MinimumAcceptedRange && alignmentValue < MaximumAcceptedRange;;
        }
    }

    [Serializable()]
    public enum Alignment { Point, Horizontal, Vertical };
}
