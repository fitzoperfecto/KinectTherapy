using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SWENG.Record
{
    public class ReplaySkeletonFrameReadyEventArgs : EventArgs
    {
        public ReplaySkeletonFrame SkeletonFrame { get; set; }
    }
}
