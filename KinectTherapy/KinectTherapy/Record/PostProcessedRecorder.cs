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
        private readonly BinaryWriter _writer;
        private readonly DataRecorder _dataRecorder;

        private Stream _recordStream;
        private DateTime _previousFlushDate;

        public DateTime StartTime { get; internal set; }
        public bool IsRecording { get; internal set; }

        public PostProcessedRecorder(Stream stream)
        {
            _recordStream = stream;
            _writer = new BinaryWriter(_recordStream);

            if (_writer != null)
            {
                _dataRecorder = new DataRecorder(_writer);
            }
            else
            {
                throw new Exception("Unable to create binary writer for the processed data.");
            }

            IsRecording = false;
            _previousFlushDate = DateTime.Now;
        }

        public void Record(double[] processed, long milliseconds)
        {
            if (_writer != null && IsRecording)
            {
                if (_dataRecorder == null)
                    return;

                _dataRecorder.Record(processed, milliseconds);
                Flush();
            }
        }

        void Flush()
        {
            DateTime now = DateTime.Now;

            if (now.Subtract(_previousFlushDate).TotalSeconds > 60)
            {
                _previousFlushDate = now;
                if (_writer != null)
                {
                    _writer.Flush();
                }
            }
        }

        public void Stop()
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer.Dispose();
            }

            if (_recordStream != null)
            {
                _recordStream.Close();
                _recordStream.Dispose();
                _recordStream = null;
            }
        }

        public void Start()
        {
            IsRecording = true;
        }
    }
}
