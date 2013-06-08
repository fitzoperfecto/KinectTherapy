using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace SWENG.Criteria
{
    /// <summary>
    /// Simple object to contain a single criterion of a Joint's exercise criteria
    /// </summary>
    public abstract class Criterion
    {
        public float Variance { get; set; }
        public Criterion(float variance)
        {
            this.Variance = variance;
        }

        public Skeleton GetTrackedSkeleton(Skeleton[] skeletonData)
        {
            Skeleton trackedSkelly = null;
            foreach (Skeleton skeleton in skeletonData)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    trackedSkelly = skeleton;
                    break;
                }
            }
            return trackedSkelly;
        }

        public abstract bool matchesCriterion(SkeletonStamp skeletonStamp);
    }
}
