using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Kinect;

namespace SWENG.Record
{
    public class PostProcessedRead
    {
        private int _jointLength;
        
        private Stream _stream;
        private List<SkeletonStamp> _frames;
        
        public SkeletonStamp[] Data { get { return _frames.ToArray(); } }

        public PostProcessedRead(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            _stream = stream;
            _frames = new List<SkeletonStamp>();
            _jointLength = Enum.GetNames(typeof(JointType)).Length;

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                CreateFromReader(reader);
            }
        }

        private void CreateFromReader(BinaryReader reader)
        {
            SkeletonStamp frame = new SkeletonStamp(new Skeleton[0], 0);

            frame.PercentBad = new double[_jointLength];
            frame.TimeStamp = reader.ReadInt64();
            for (int i = 0; i < _jointLength; ++i)
            {
                frame.PercentBad[i] = reader.ReadDouble();
            }

            _frames.Add(frame);
        }
    }
}
