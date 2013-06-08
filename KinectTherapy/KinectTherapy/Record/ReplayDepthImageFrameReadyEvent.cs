using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SWENG.Record
{
    public class ReplayDepthImageFrameReadyEventArgs : EventArgs
    {
        public ReplayDepthImageFrame DepthImageFrame { get; set; }
    }
}
