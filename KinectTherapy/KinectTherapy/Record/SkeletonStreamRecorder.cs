using System;
using System.IO;
using Microsoft.Kinect;
using System.Runtime.Serialization.Formatters.Binary;

namespace SWENG.Record
{
    public class SkeletonStreamRecorder
    {
        private DateTime referenceTime;
        readonly BinaryWriter binaryWriter;

        internal SkeletonStreamRecorder(BinaryWriter binaryWriter)
        {
            this.binaryWriter = binaryWriter;
            this.referenceTime = DateTime.Now;
        }

        /// <summary>
        /// Writes everything out about the SkeletonFrame to a BinaryWriter
        /// </summary>
        /// <param name="frame">Snapshot of the current SkeletonFrame from Kinect</param>
        public void Record(SkeletonFrame frame)
        {
            this.binaryWriter.Write((int)KinectRecordOptions.Skeletons);

            TimeSpan timeSpan = DateTime.Now.Subtract(this.referenceTime);
            this.referenceTime = DateTime.Now;
            this.binaryWriter.Write((long)timeSpan.Milliseconds);

            this.binaryWriter.Write(frame.FloorClipPlane.Item1);
            this.binaryWriter.Write(frame.FloorClipPlane.Item2);
            this.binaryWriter.Write(frame.FloorClipPlane.Item3);
            this.binaryWriter.Write(frame.FloorClipPlane.Item4);
            this.binaryWriter.Write(frame.FrameNumber);

            Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
            frame.CopySkeletonDataTo(skeletons);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(this.binaryWriter.BaseStream, skeletons);
        }
    }
}
