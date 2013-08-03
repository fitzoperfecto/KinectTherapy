using System;
using System.Collections.Generic;
using System.IO;
using Kinect.Toolbox.Record;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using SWENG.Record;

namespace SWENG.Service
{
    public enum RecordingManagerStatus
    {
        Recording,
        Replaying,
        Standby
    }

    public delegate void RecordingStatusChanged(object sender, RecordingStatusChangedEventArg e);

    public class RecordingStatusChangedEventArg : EventArgs
    {
        public string FileId;

        public RecordingStatusChangedEventArg(string fileId)
        {
            FileId = fileId;
        }
    }

    /// <summary>
    /// The recording manager is a service responsible for handling the recording and replaying of Kinect frames.
    /// </summary>
    public class RecordingManager : IGameComponent
    {
        #region event stuff
        public event RecordingStatusChanged RecordingStatusChanged;

        // Invoke the Changed event; called whenever repetitions changes
        protected virtual void OnRecordingStatusChanged(RecordingStatusChangedEventArg e)
        {
            if (RecordingStatusChanged != null)
                RecordingStatusChanged(this, e);
        }
        #endregion

        private FileStream _recordingStream;
        private FileStream _replayStream;
        private FileStream _dataOutStream;
        private string _fileLocation;

        public KinectRecorder KinectRecorder { get; private set; }
        public KinectReplay KinectReplay { get; private set; }
        public PostProcessedRecorder DataRecorder { get; private set; }

        public Dictionary<string, string> FilesUsed { get; private set; }
        public RecordingManagerStatus Status { get; private set; }
        public List<EventHandler<ReplaySkeletonFrameReadyEventArgs>> SkeletonEventListener { get; private set; }
        public List<EventHandler<ReplayColorImageFrameReadyEventArgs>> ColorEventListener { get; private set; }
        public List<EventHandler<ReplayDepthImageFrameReadyEventArgs>> DepthEventListener { get; private set; }

        public RecordingManager()
        {
            _fileLocation = @"c:\school\" + DateTime.Now.ToFileTime() + @"\";
            if (!Directory.Exists(_fileLocation))
            {
                Directory.CreateDirectory(_fileLocation);
            }
            FilesUsed = new Dictionary<string, string>();
            Status = Service.RecordingManagerStatus.Standby;
            SkeletonEventListener = new List<EventHandler<ReplaySkeletonFrameReadyEventArgs>>();
            ColorEventListener = new List<EventHandler<ReplayColorImageFrameReadyEventArgs>>();
            DepthEventListener = new List<EventHandler<ReplayDepthImageFrameReadyEventArgs>>();
        }

        public void Initialize()
        {
            string fakeFile = _fileLocation + Guid.NewGuid().ToString();
            _recordingStream = new FileStream(fakeFile, FileMode.OpenOrCreate);
            KinectRecorder = new KinectRecorder(0, _recordingStream);

            _replayStream = new FileStream(fakeFile + "1", FileMode.OpenOrCreate);
            KinectReplay = new KinectReplay(_replayStream);

            _dataOutStream = new FileStream(fakeFile + "2", FileMode.OpenOrCreate);
            DataRecorder = new PostProcessedRecorder(_dataOutStream);
        }

        private void StartRecording(KinectRecordOptions options)
        {
            StopReplaying();
            StopRecording();

            string fileId = Guid.NewGuid().ToString();
            FilesUsed.Add(fileId, _fileLocation + fileId);

            if (null != KinectRecorder && KinectRecorder.IsRecording)
            {
                KinectRecorder.Stop();
            }

            _recordingStream = new FileStream(
                FilesUsed[fileId], 
                FileMode.OpenOrCreate
            );

            if (null != DataRecorder && DataRecorder.IsRecording)
            {
                DataRecorder.Stop();
            }

            _dataOutStream = new FileStream(
                FilesUsed[fileId] + "_data",
                FileMode.OpenOrCreate
            );

            KinectRecorder = new KinectRecorder(options, _recordingStream);
            KinectRecorder.Start();

            DataRecorder = new PostProcessedRecorder(_dataOutStream);
            DataRecorder.Start();

            Status = Service.RecordingManagerStatus.Recording;
            OnRecordingStatusChanged(new RecordingStatusChangedEventArg(fileId));
        }

        public void StopRecording()
        {
            if (null != KinectRecorder && KinectRecorder.IsRecording)
            {
                KinectRecorder.Stop();
            }

            if (null != _recordingStream)
            {
                
                _recordingStream.Close();
                _recordingStream.Dispose();
                _recordingStream = null;
            }

            if (null != DataRecorder && DataRecorder.IsRecording)
            {
                DataRecorder.Stop();
            }

            if (null != _dataOutStream)
            {

                _dataOutStream.Close();
                _dataOutStream.Dispose();
                _dataOutStream = null;
            }

            Status = Service.RecordingManagerStatus.Standby;
        }

        public void StartReplaying(string fileId)
        {
            StopRecording();

            if (null != KinectReplay && KinectReplay.Started)
            {
                KinectReplay.Dispose();
            }

            if (null != SkeletonEventListener)
            {
                foreach (EventHandler<ReplaySkeletonFrameReadyEventArgs> eventHandler
                    in SkeletonEventListener)
                {
                    KinectReplay.SkeletonFrameReady -= eventHandler;
                }
            }

            if (null != ColorEventListener)
            {
                foreach (EventHandler<ReplayColorImageFrameReadyEventArgs> eventHandler
                    in ColorEventListener)
                {
                    KinectReplay.ColorImageFrameReady -= eventHandler;
                }
            }

            _replayStream = new FileStream(
                FilesUsed[fileId],
                FileMode.Open,
                FileAccess.Read
            );

            KinectReplay = new KinectReplay(_replayStream);

            if (null != SkeletonEventListener)
            {
                foreach (EventHandler<ReplaySkeletonFrameReadyEventArgs> eventHandler
                    in SkeletonEventListener)
                {
                    KinectReplay.SkeletonFrameReady += eventHandler;
                }
            }

            if (null != ColorEventListener)
            {
                foreach (EventHandler<ReplayColorImageFrameReadyEventArgs> eventHandler
                    in ColorEventListener)
                {
                    KinectReplay.ColorImageFrameReady += eventHandler;
                }
            }

            KinectReplay.Start();
            Status = Service.RecordingManagerStatus.Replaying;
        }

        public void StopReplaying()
        {
            if (null != KinectReplay && KinectReplay.Started)
            {
                KinectReplay.Dispose();
            }
            Status = Service.RecordingManagerStatus.Standby;
        }

        public void RestartReplay()
        {
            if (null != KinectReplay && null != _replayStream)
            {
                _replayStream.Position = 0;
                KinectReplay.Stop();
                KinectReplay.Start();
            }
            Status = Service.RecordingManagerStatus.Replaying;
        }

        public void Record(ColorImageFrame frame)
        {
            if (null != KinectRecorder && KinectRecorder.IsRecording)
            {
                KinectRecorder.Record(frame);
            }
        }

        public void Record(DepthImageFrame frame)
        {
            if (null != KinectRecorder && KinectRecorder.IsRecording)
            {
                KinectRecorder.Record(frame);
            }
        }

        public void Record(SkeletonFrame frame)
        {
            if (null != KinectRecorder && KinectRecorder.IsRecording)
            {
                KinectRecorder.Record(frame);
            }
        }

        public void Record(double[] processed, long milliseconds)
        {
            if (null != DataRecorder && DataRecorder.IsRecording)
            {
                DataRecorder.Record(processed, milliseconds);
            }
        }

        public void StartRecording(object sender, EventArgs args)
        {
            StartRecording(KinectRecordOptions.Skeletons | KinectRecordOptions.Color);
        }

        public void StopRecording(object sender, EventArgs args)
        {
            StopRecording();
        }

        public SkeletonStamp[] ReadProcessedData(string fileId)
        {
            SkeletonStamp[] stamps = new SkeletonStamp[0];

            using (FileStream fs = new FileStream(FilesUsed[fileId] + "_data", FileMode.Open, FileAccess.Read))
            {
                PostProcessedRead ppr = new PostProcessedRead(fs);
                stamps = ppr.Data;
            }

            return stamps;
        }

        public void RemoveFiles()
        {
            try
            {
                File.Delete(_fileLocation);
            }
            catch (Exception e) { }
        }
    }
}
