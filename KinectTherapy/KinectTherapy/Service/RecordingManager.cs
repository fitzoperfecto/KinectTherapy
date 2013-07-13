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
using System.Xml;

// TODO: place enum into a new file
// ?keep delegate and eventarg class here?
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
            ColorEventListener = new List<EventHandler<ReplayColorImageFrameReadyEventArgs>>();
            DepthEventListener = new List<EventHandler<ReplayDepthImageFrameReadyEventArgs>>();
        }

        public void Initialize()
        {
            string fakeFile = fileLocation + Guid.NewGuid().ToString();
            recordingStream = new FileStream(fakeFile, FileMode.OpenOrCreate);
            kinectRecorder = new KinectRecorder(0, recordingStream);

            replayStream = new FileStream(fakeFile + "1", FileMode.OpenOrCreate);
            kinectReplay = new KinectReplay(replayStream);
        }

        private void StartRecording(KinectRecordOptions options)
        {
            StopReplaying();
            StopRecording();

            string fileId = Guid.NewGuid().ToString();
            filesUsed.Add(fileId, fileLocation + fileId);

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
            OnRecordingStatusChanged(new RecordingStatusChangedEventArg(fileId));
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

            if (null != SkeletonEventListener)
            {
                foreach (EventHandler<ReplaySkeletonFrameReadyEventArgs> eventHandler
                    in SkeletonEventListener)
                {
                    kinectReplay.SkeletonFrameReady -= eventHandler;
                }
            }

            if (null != ColorEventListener)
            {
                foreach (EventHandler<ReplayColorImageFrameReadyEventArgs> eventHandler
                    in ColorEventListener)
                {
                    kinectReplay.ColorImageFrameReady -= eventHandler;
                }
            }

            replayStream = new FileStream(
                filesUsed[fileId],
                FileMode.Open,
                FileAccess.Read
            );

            kinectReplay = new KinectReplay(replayStream);

            if (null != SkeletonEventListener)
            {
                foreach (EventHandler<ReplaySkeletonFrameReadyEventArgs> eventHandler
                    in SkeletonEventListener)
                {
                    kinectReplay.SkeletonFrameReady += eventHandler;
                }
            }

            if (null != ColorEventListener)
            {
                foreach (EventHandler<ReplayColorImageFrameReadyEventArgs> eventHandler
                    in ColorEventListener)
                {
                    kinectReplay.ColorImageFrameReady += eventHandler;
                }
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
            if (null != kinectReplay && null != replayStream)
            {
                replayStream.Position = 0;
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

        /// <summary>
        /// start recording
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void StartRecording(object sender, EventArgs args)
        {
            StartRecording(KinectRecordOptions.Skeletons | KinectRecordOptions.Color);
        }

        /// <summary>
        /// stop recording
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void StopRecording(object sender, EventArgs args)
        {
            StopRecording();
        }
    }
}
