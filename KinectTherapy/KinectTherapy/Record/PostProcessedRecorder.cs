/**
 * Patterned after the Kinect.Toolbox.Record
 */
using System;
using System.IO;
using System.Text;

namespace SWENG.Record
{
    public class PostProcessedRecorder
    {
        readonly BinaryWriter writer;

        Stream recordStream;
        DateTime previousFlushDate;

        readonly DataRecorder dataRecorder;

        public DateTime startTime { get; internal set; }
        public bool IsRecording { get; internal set; }

        public PostProcessedRecorder(Stream stream)
        {
            recordStream = stream;
            writer = new BinaryWriter(recordStream);

            if (writer != null)
            {
                dataRecorder = new DataRecorder(writer);
            }
            else
            {
                throw new Exception("Unable to create binary writer for the processed data.");
            }

            IsRecording = false;
            previousFlushDate = DateTime.Now;
        }

        public void Record(double[] processed, long milliseconds)
        {
            if (writer != null && IsRecording)
            {
                if (dataRecorder == null)
                    return;

                dataRecorder.Record(processed, milliseconds);
                Flush();
            }
        }

        void Flush()
        {
            var now = DateTime.Now;

            if (now.Subtract(previousFlushDate).TotalSeconds > 60)
            {
                previousFlushDate = now;
                if (writer != null)
                {
                    writer.Flush();
                }
            }
        }

        public void Stop()
        {
            if (writer != null)
            {
                writer.Close();
                writer.Dispose();
            }

            if (recordStream != null)
            {
                recordStream.Close();
                recordStream.Dispose();
                recordStream = null;
            }
        }

        public void Start()
        {
            IsRecording = true;
        }
    }
}
