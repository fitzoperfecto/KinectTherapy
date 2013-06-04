using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SWENG.Record
{
    public class ReplayColorImageFrameReadyEventArgs : EventArgs
    {
        public ReplayColorImageFrame ColorImageFrame { get; set; }
    }
}
