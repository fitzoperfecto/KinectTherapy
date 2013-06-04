using System;
using System.IO;
using Microsoft.Kinect;

namespace SWENG.Record
{
    class ColorStreamRecorder
    {
        private DateTime referenceTime;
        readonly BinaryWriter binaryWriter;

        internal ColorStreamRecorder(BinaryWriter binaryWriter)
        {
            this.binaryWriter = binaryWriter;
            this.referenceTime = DateTime.Now;
        }

        /// <summary>
        /// Writes everything out about the ColorImageFrame to a BinaryWriter
        /// </summary>
        /// <param name="frame">Snapshot of the current ColorStream from Kinect</param>
        public void Record(ColorImageFrame frame)
        {
            this.binaryWriter.Write((int)KinectRecordOptions.Color);

            TimeSpan timeSpan = DateTime.Now.Subtract(this.referenceTime);
            this.referenceTime = DateTime.Now;

            this.binaryWriter.Write((long)timeSpan.Milliseconds);
            this.binaryWriter.Write(frame.BytesPerPixel);
            this.binaryWriter.Write((int)frame.Format);
            this.binaryWriter.Write(frame.Width);
            this.binaryWriter.Write(frame.Height);
            this.binaryWriter.Write(frame.FrameNumber);

            byte[] bytes = new byte[frame.PixelDataLength];
            frame.CopyPixelDataTo(bytes);

            this.binaryWriter.Write(bytes.Length);
            this.binaryWriter.Write(bytes);
        }
    }
}
