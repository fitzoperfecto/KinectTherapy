using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace SWENG.Criteria
{
    public class AlignmentCriterion : Criterion
    {
        public enum Alignment { Point, Horizontal, Vertical };

        JointType[] joints;
        JointType centerJointType;
        Alignment alignment;
        float minVariance;
        float maxVariance;

        public AlignmentCriterion(JointType[] joints, Alignment alignment, float variance):base(variance)
        {
            this.alignment = alignment;
            this.joints = joints;
            this.minVariance = 0 - variance;
            this.maxVariance = 0 + variance;
        }

        public AlignmentCriterion(JointType centerJoint, JointType[] joints, float variance)
            : base(variance)
        {
            this.alignment = Alignment.Point;
            this.centerJointType = centerJoint;
            this.joints = joints;
        }

        public override bool matchesCriterion(SkeletonStamp skeletonStamp)
        {
            float alignmentValue = 0.0f;
            Joint[] trackedJoints = new Joint[joints.Length];
            Joint centerJoint;
            int i = 0;
            // get the joints to be aligned
            foreach (JointType type in joints)
            {
                trackedJoints[i++] = skeletonStamp.GetTrackedSkeleton().Joints[type];
            }
            switch (alignment)
            {
                case Alignment.Point:
                    centerJoint = skeletonStamp.GetTrackedSkeleton().Joints[centerJointType];
                    alignmentValue = JointAnalyzer.areJointsAligned(centerJoint, trackedJoints);
                    break;
                case Alignment.Horizontal:
                    alignmentValue = JointAnalyzer.alignedHorizontally(trackedJoints[0], trackedJoints[1]);
                    break;
                case Alignment.Vertical:
                    alignmentValue = JointAnalyzer.alignedVertically(trackedJoints[0], trackedJoints[1]);
                    break;
            }
            return alignmentValue > minVariance && alignmentValue < maxVariance;;
        }
    }
}
