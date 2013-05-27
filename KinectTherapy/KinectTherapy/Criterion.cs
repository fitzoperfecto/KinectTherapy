using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace SWENG
{
    /// <summary>
    /// Simple object to contain a single criterion of a Joint's exercise criteria
    /// </summary>
    abstract class Criterion
    {
        public float Variance { get; set; }
        public Criterion(float variance)
        {
            this.Variance = variance;
        }

        public abstract bool matchesCriterion(SkeletonStamp skeletonStamp);
    }
}
