using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Kinect;
using SWENG;

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
                /** In case multiple types of basic data needs to be recorded eventually */
                reader.ReadInt32();
                CreateFromReader(reader);
            }
        }

        private void CreateFromReader(BinaryReader reader)
        {
            SkeletonStamp frame = new SkeletonStamp(new Skeleton[0], 0);
            frame.PercentBad = new double[jointLength];

            for (int i = 0; i < jointLength; ++i)
            {
                if (reader.BaseStream.Position == reader.BaseStream.Length)
                    return;
                frame.PercentBad[i] = reader.ReadInt32();
            }

            if (reader.BaseStream.Position == reader.BaseStream.Length)
                return;
            frame.TimeStamp = reader.ReadInt64();

            frames.Add(frame);
        }
    }
}
