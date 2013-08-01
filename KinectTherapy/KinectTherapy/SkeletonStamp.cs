using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace SWENG
{
    public class SkeletonStamp
    {
        private Skeleton[] skeletonData;
        public long TimeStamp { get; set; }
        public bool IsActive { get; set; }
        public bool InUse { get; set; }
        public bool IsProcessed { get; set; }
        public double[] PercentBad { get; set; }

        public Skeleton[] SkeletonData 
        {
            get { return skeletonData; } 
            set 
            {
                if (skeletonData == null || value.Length != skeletonData.Length)
                {
                    skeletonData = new Skeleton[value.Length];
                }

                value.CopyTo(skeletonData, 0);
            }
        }

        public SkeletonStamp(Skeleton[] skeletonData, long timeStamp)
        {
            this.SkeletonData = skeletonData;
            this.TimeStamp = timeStamp;
            this.IsActive = false;
            this.InUse = false;
            this.PercentBad = new double[20];
            this.IsProcessed = false;
        }

        public Skeleton GetTrackedSkeleton()
        {
            Skeleton trackedSkelly = null;
            foreach (Skeleton skeleton in SkeletonData)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    trackedSkelly = skeleton;
                    break;
                }
            }
            return trackedSkelly;
        }
    }
}
