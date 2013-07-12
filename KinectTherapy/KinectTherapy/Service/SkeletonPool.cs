namespace SWENG.Service
{
    using System;
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// This is meant to allocate space for the program to use and
    /// keep track of its Skeletons
    /// </summary>
    public class SkeletonPool : IGameComponent
    {
        private int maxInstances;
        Queue<SkeletonStamp> free;
        SkeletonStamp[] universe;

        public SkeletonPool(Game game, int maxInstances)
        {
            this.maxInstances = maxInstances;
        }

        public int MaxInstances { get { return this.maxInstances; } }

        public int Available()
        {
            return this.free.Count;
        }

        public int Count()
        {
            return this.universe.Length;
        }

        public SkeletonStamp[] All()
        {
            return this.universe;
        }

        public SkeletonStamp GetSkeleton(int index)
        {
            return this.universe[index];
        }

        /// <summary>
        /// Will return the oldest Skeleton that is active and not in current use
        /// </summary>
        /// <returns></returns>
        public SkeletonStamp GetOldestActiveSkeleton()
        {
            int index = int.MinValue;

            for (int i = 0; i < this.universe.Length; ++i)
            {
                if (!this.universe[i].IsActive && this.universe[i].InUse)
                {
                    continue;
                }

                if (index == int.MinValue)
                {
                    index = i;
                }
                else if (this.universe[index].TimeStamp > this.universe[i].TimeStamp)
                {
                    index = i;
                }
            }
            if (index == int.MinValue)
            {
                index = 0;
            }
            this.universe[index].InUse = true;

            return this.universe[index];
        }

        /// <summary>
        /// Will return the oldest Skeleton that is active and not in current use
        /// </summary>
        /// <returns></returns>
        public SkeletonStamp GetOldestProcessedSkeleton()
        {
            int index = int.MinValue;

            for (int i = 0; i < this.universe.Length; ++i)
            {
                if (!this.universe[i].IsProcessed && this.universe[i].InUse)
                {
                    continue;
                }

                if (index == int.MinValue)
                {
                    index = i;
                }
                else if (this.universe[index].TimeStamp > this.universe[i].TimeStamp)
                {
                    index = i;
                }
            }
            if (index == int.MinValue)
            {
                index = 0;
            }
            this.universe[index].InUse = true;

            return this.universe[index];
        }

        public void Initialize()
        {
            this.universe = new SkeletonStamp[maxInstances];
            this.free = new Queue<SkeletonStamp>(maxInstances);

            for (int i = 0; i < maxInstances; ++i)
            {
                universe[i] = new SkeletonStamp(new Skeleton[0], long.MaxValue);
                free.Enqueue(universe[i]);
            }
        }

        /// <summary>
        /// Dequeues from the queue if available and accepting
        /// </summary>
        /// <param name="skeleton">Skeletal array data from the SkeletonFrame</param>
        public void Add(Skeleton[] skeleton, long timeStamp)
        {
            // Is there any space left?
            if (free.Count == 0)
            {
                for (int i = 0; i < maxInstances; ++i)
                {
                    //   enqueue again so long as no one else is using this
                    if (!this.universe[i].InUse)
                    {
                        free.Enqueue(this.universe[i]);
                    }
                }
            }

            if (free.Count > 0)
            {
                SkeletonStamp s = free.Dequeue();
                s.TimeStamp = timeStamp;
                s.SkeletonData = skeleton;
                s.InUse = false;
                s.IsActive = true;
                //Debug.WriteLine("Adding {0}", free.Count);
            }
            else
            {
                Debug.WriteLine("Out of skeletons!  Consider increasing the max instances on initialization.");
            }
        }

        /// <summary>
        /// Use when you want to say you are done with the skeleton 
        /// but is not ready to be removed
        /// </summary>
        /// <param name="timeStamp">Time stamp of the skeleton you are done with</param>
        /// <param name="percentBad">How bad was the skeleton (between 0.0 and 1.0)</param>
        public void FinishedWithSkeleton(long timeStamp)
        {
            foreach (SkeletonStamp skeleton in this.universe)
            {
                if (skeleton.TimeStamp != timeStamp)
                {
                    continue;
                }
                else
                {
                    /*
                     * Pool now knows this skeleton has been processed and is waiting to be post processed by the draw
                     */
                    skeleton.IsProcessed = true;
                    skeleton.InUse = false;
                }

            }
        }

        /// <summary>
        /// Remove all instances that are older than or equal to this timeStamp
        /// </summary>
        /// <param name="timeStamp">The time stamp of the skeleton data</param>
        public void Remove(long timeStamp)
        {
            for (int i = 0; i < this.universe.Length; ++i)
            {
                // If this isn't active then we don't need to touch it
                if (!this.universe[i].IsActive)
                {
                    continue;
                }

                if (this.universe[i].TimeStamp <= timeStamp)
                {
                    this.universe[i].InUse = false;
                    this.universe[i].IsActive = false;
                    this.free.Enqueue(this.universe[i]);
                }
            }
        }
    }
}
