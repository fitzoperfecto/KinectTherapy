using System;
using System.IO;
using Microsoft.Kinect;

namespace SWENG.Record
{
    public class KinectStreamRecorder
    {
        private const int SECONDS_PER_CLIP = 60;

        private Stream recordedStream;
        private readonly BinaryWriter binaryWriter;
        private readonly ColorStreamRecorder colorStreamRecorder;
        private readonly SkeletonStreamRecorder skeletonStreamRecorder;
        private readonly DepthStreamRecorder depthStreamRecorder;

        private DateTime previousFlushDate;

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

            if ((this.Sections & KinectRecordOptions.Depth) != 0)
            {
                this.depthStreamRecorder = new DepthStreamRecorder(this.binaryWriter);
            }

            if ((this.Sections & KinectRecordOptions.Skeletons) != 0)
            {
                this.skeletonStreamRecorder = new SkeletonStreamRecorder(this.binaryWriter);
            }

            this.previousFlushDate = DateTime.Now;
        }

        public void Record(ColorImageFrame frame)
        {
            if (this.binaryWriter != null
                && this.colorStreamRecorder != null)
            {
                this.colorStreamRecorder.Record(frame);
                Flush();
            }
        }

        public void Record(SkeletonFrame frame)
        {
            if (this.binaryWriter != null
                && this.skeletonStreamRecorder != null)
            {
                this.skeletonStreamRecorder.Record(frame);
                Flush();
            }
        }

        public void Record(DepthImageFrame frame)
        {
            if (this.binaryWriter != null
                && this.depthStreamRecorder != null)
            {
                this.depthStreamRecorder.Record(frame);
                Flush();
            }
        }

        void Flush()
        {
            DateTime now = DateTime.Now;

            if (now.Subtract(this.previousFlushDate).TotalSeconds > SECONDS_PER_CLIP)
            {
                this.previousFlushDate = now;
                this.binaryWriter.Flush();
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
