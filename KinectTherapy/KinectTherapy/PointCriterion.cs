using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SWENG
{
    class PointCriterion:Criterion
    {
        public String Axis { get; set; }
        public PointCriterion(float variance):base(variance)
        {

        }
        public override bool matchesCriterion(SkeletonStamp skeletonStamp)
        {
            return true;
        }
    }


}
