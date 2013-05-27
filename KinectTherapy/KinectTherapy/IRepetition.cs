using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SWENG
{
    /// <summary>
    /// Interface for tracking the life of a repetition. 
    /// </summary>
    interface IRepetition
    {
        bool isRepStarted(SkeletonStamp skeletonStamp);
        bool isRepComplete(SkeletonStamp skeletonStamp);
    }
}
