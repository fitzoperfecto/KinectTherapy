using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace SWENG.Criteria
{
    public class PointCriterion:Criterion
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
    }


}
