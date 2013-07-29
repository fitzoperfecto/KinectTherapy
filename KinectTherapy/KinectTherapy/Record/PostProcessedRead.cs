using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Kinect;
using SWENG;
using System.Diagnostics;

namespace SWENG.Record
{
    public class PostProcessedRead
    {
        private Stream stream;
        private int jointLength;
        private List<SkeletonStamp> frames;
        public SkeletonStamp[] Data { get { return frames.ToArray(); } }

        public PostProcessedRead(Stream stream)
        {
            this.stream = stream;
            BinaryReader reader = new BinaryReader(stream);
            frames = new List<SkeletonStamp>();
            jointLength = Enum.GetNames(typeof(JointType)).Length;

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                CreateFromReader(reader);
            }
        }

        private void CreateFromReader(BinaryReader reader)
        {
            SkeletonStamp frame = new SkeletonStamp(new Skeleton[0], 0);
            try
            {
                frame.PercentBad = new double[jointLength];

                frame.TimeStamp = reader.ReadInt64();
                for (int i = 0; i < jointLength; ++i)
                {
                    frame.PercentBad[i] = reader.ReadDouble();
                }

                frames.Add(frame);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return;
            }
        }
    }
}
