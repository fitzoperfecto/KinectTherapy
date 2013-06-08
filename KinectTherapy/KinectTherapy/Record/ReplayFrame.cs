using System.IO;

namespace SWENG.Record
{
    public abstract class ReplayFrame
    {
        public int FrameNumber { get; protected set; }
        public long TimeStamp { get; protected set; }

        internal abstract void CreateFromReader(BinaryReader binaryReader);
    }
}
