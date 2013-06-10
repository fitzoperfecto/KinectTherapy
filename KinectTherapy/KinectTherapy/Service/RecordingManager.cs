using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Kinect.Toolbox.Record;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework.Storage;

namespace SWENG.Service
{
    public enum RecordingManagerStatus
    {
        Recording,
        Replaying,
        Standby
    }

    public class RecordingManager : IGameComponent
    {
        public KinectRecorder kinectRecorder { get; internal set; }
        public KinectReplay kinectReplay { get; internal set; }

        private FileStream recordingStream;
        private FileStream replayStream;

        private string fileLocation;
        public Dictionary<string, string> filesUsed { get; internal set; }
        public RecordingManagerStatus Status { get; internal set; }
        public List<EventHandler<ReplaySkeletonFrameReadyEventArgs>> SkeletonEventListener { get; internal set; }
        public List<EventHandler<ReplayColorImageFrameReadyEventArgs>> ColorEventListener { get; internal set; }
        public List<EventHandler<ReplayDepthImageFrameReadyEventArgs>> DepthEventListener { get; internal set; }

        public RecordingManager()
        {
            fileLocation = @"c:\school\" + DateTime.Now.ToFileTime() + @"\";
            if (!Directory.Exists(fileLocation))
            {
                Directory.CreateDirectory(fileLocation);
            }
            filesUsed = new Dictionary<string, string>();
            Status = Service.RecordingManagerStatus.Standby;
            SkeletonEventListener = new List<EventHandler<ReplaySkeletonFrameReadyEventArgs>>();
        }

        public void Initialize()
        {
            string fakeFile = fileLocation + Guid.NewGuid().ToString();
            recordingStream = new FileStream(fakeFile, FileMode.OpenOrCreate);
            kinectRecorder = new KinectRecorder(0, recordingStream);

            replayStream = new FileStream(fakeFile + "1", FileMode.OpenOrCreate);
            kinectReplay = new KinectReplay(replayStream);
        }

        public void StartRecording(KinectRecordOptions options)
        {
            string fileId = Guid.NewGuid().ToString();
            StopReplaying();

            filesUsed.Add(fileId, fileLocation += fileId);
            
            if (null != kinectRecorder && kinectRecorder.IsRecording)
            {
                kinectRecorder.Stop();
            }

            recordingStream = new FileStream(
                filesUsed[fileId], 
                FileMode.OpenOrCreate
            );
            kinectRecorder = new KinectRecorder(options, recordingStream);
            kinectRecorder.Start();
            Status = Service.RecordingManagerStatus.Recording;
        }

        public void StopRecording()
        {
            if (null != kinectRecorder && kinectRecorder.IsRecording)
            {
                kinectRecorder.Stop();
            }

            if (null != recordingStream)
            {
                recordingStream.Close();
                recordingStream.Dispose();
                recordingStream = null;
            }
            Status = Service.RecordingManagerStatus.Standby;
        }

        public void StartReplaying(string fileId)
        {
            StopRecording();

            if (null != kinectReplay && kinectReplay.Started)
            {
                kinectReplay.Dispose();
            }

            foreach (EventHandler<ReplaySkeletonFrameReadyEventArgs> eventHandler 
                in SkeletonEventListener)
            {
                kinectReplay.SkeletonFrameReady -= eventHandler;
            }

            replayStream = new FileStream(
                filesUsed[fileId], 
                FileMode.Open,
                FileAccess.Read
            );
            kinectReplay = new KinectReplay(replayStream);

            foreach (EventHandler<ReplaySkeletonFrameReadyEventArgs> eventHandler
                in SkeletonEventListener)
            {
                kinectReplay.SkeletonFrameReady += eventHandler;
            }

            kinectReplay.Start();
            Status = Service.RecordingManagerStatus.Replaying;
        }

        public void StopReplaying()
        {
            if (null != kinectReplay && kinectReplay.Started)
            {
                kinectReplay.Dispose();
            }
            Status = Service.RecordingManagerStatus.Standby;
        }

        public void RestartReplay()
        {
            if (null != kinectReplay && kinectReplay.Started)
            {
                kinectReplay.Stop();
                kinectReplay.Start();
            }
            Status = Service.RecordingManagerStatus.Replaying;
        }

        public void Record(ColorImageFrame frame)
        {
            if (null != kinectRecorder && kinectRecorder.IsRecording)
            {
                kinectRecorder.Record(frame);
            }
        }

        public void Record(DepthImageFrame frame)
        {
            if (null != kinectRecorder && kinectRecorder.IsRecording)
            {
                kinectRecorder.Record(frame);
            }
        }

        public void Record(SkeletonFrame frame)
        {
            if (null != kinectRecorder && kinectRecorder.IsRecording)
            {
                kinectRecorder.Record(frame);
            }
        }
    }
}
