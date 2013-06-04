using System;
using System.IO;
using Microsoft.Kinect;

namespace SWENG.Record
{
    public class KinectStreamRecorder
    {
        private Stream recordedStream;
        private readonly BinaryWriter binaryWriter;
        private readonly ColorStreamRecorder colorStreamRecorder;
        private readonly SkeletonStreamRecorder skeletonStreamRecorder;

        public KinectRecordOptions Sections { get; set; }

        public KinectStreamRecorder(KinectRecordOptions sections, Stream stream)
        {
            this.recordedStream = stream;
            this.Sections = sections;

            this.binaryWriter = new BinaryWriter(this.recordedStream);

            this.binaryWriter.Write((int)this.Sections);

            if ((this.Sections & KinectRecordOptions.Color) != 0)
            {
                this.colorStreamRecorder = new ColorStreamRecorder(this.binaryWriter);
            }

            if ((this.Sections & KinectRecordOptions.Skeletons) != 0)
            {
                this.skeletonStreamRecorder = new SkeletonStreamRecorder(this.binaryWriter);
            }
        }

        public void Record(ColorImageFrame frame)
        {
            if (this.binaryWriter != null
                && this.colorStreamRecorder != null)
            {
                this.colorStreamRecorder.Record(frame);
            }
        }

        public void Record(SkeletonFrame frame)
        {
            if (this.binaryWriter != null
                && this.skeletonStreamRecorder != null)
            {
                this.skeletonStreamRecorder.Record(frame);
            }
        }

        public void StopRecording()
        {
            if (this.binaryWriter != null)
            {
                this.binaryWriter.Close();
                this.binaryWriter.Dispose();

                this.recordedStream.Dispose();
                this.recordedStream = null;
            }
        }
    }
}
