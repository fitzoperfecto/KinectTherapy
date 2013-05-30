using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace SWENG
{
    class PointCriterion:Criterion
    {
        public String Axis { get; set; }
        public JointType JointType;
        public PointCriterion(String Axis,JointType jointType,float variance):base(variance)
        {
            this.Axis = Axis;
            this.JointType = jointType;
        }

        public override bool matchesCriterion(SkeletonStamp skeletonStamp)
        {
            throw new NotSupportedException();
        }

        public override bool compareToOriginal(Skeleton[] originalSkeleton, SkeletonStamp currentSkeleton)
        {
            Joint originalJoint = GetTrackedSkeleton(originalSkeleton).Joints[JointType];
            Joint currentJoint = currentSkeleton.GetTrackedSkeleton().Joints[JointType];
            float originalPoint = (float)originalJoint.Position.GetType().GetProperty(Axis).GetValue(originalJoint.Position, null);
            float minRange = originalPoint - Variance;
            float maxRange = originalPoint + Variance;
            // compare against the current joint and update the results
            float currentPoint = (float)currentJoint.Position.GetType().GetProperty(Axis).GetValue(currentJoint.Position, null);
            // for now this will just be 0.0 or 100.0  we will update to a range in the future. 
            return currentPoint >= minRange && currentPoint <= maxRange;
          
        }
    }


}
