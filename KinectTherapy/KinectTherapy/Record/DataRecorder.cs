using System;
using System.IO;

namespace SWENG.Record
{
    class DataRecorder
    {
        private readonly BinaryWriter _writer;

        private DateTime _referenceTime;

        public DataRecorder(BinaryWriter writer)
        {
            _writer = writer;
            _referenceTime = DateTime.Now;
        }

        public void Record(double[] processed, long milliseconds)
        {
            TimeSpan timeSpan = DateTime.Now.Subtract(_referenceTime);
            _referenceTime = DateTime.Now;

            _writer.Write((long)timeSpan.TotalMilliseconds);

            for (int i = 0; i < processed.Length; ++i)
            {
                _writer.Write(processed[i]);
            }
        }
    }
}
