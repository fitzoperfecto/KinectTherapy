using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SWENG.Record
{
    class DataRecorder
    {
        DateTime referenceTime;
        readonly BinaryWriter writer;

        public DataRecorder(BinaryWriter writer)
        {
            this.writer = writer;
            referenceTime = DateTime.Now;
        }

        public void Record(double[] processed, long milliseconds)
        {
            TimeSpan timeSpan = DateTime.Now.Subtract(referenceTime);
            referenceTime = DateTime.Now;

            writer.Write(1);
            writer.Write((long)timeSpan.TotalMilliseconds);

            for (int i = 0; i < processed.Length; ++i)
            {
                writer.Write(processed[i]);
            }
        }
    }
}
