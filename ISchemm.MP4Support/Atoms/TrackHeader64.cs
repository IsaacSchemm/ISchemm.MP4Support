using System;
using System.Runtime.InteropServices;
using ISchemm.MP4Support.Support;

namespace ISchemm.MP4Support.Atoms {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct TrackHeader64 : ITrackHeader {
        public AtomHeader32 Header;
        public byte Version;
        public fixed byte Flags[3];
        public BigEndianUInt64 CreationTime;
        public BigEndianUInt64 ModificationTime;
        public BigEndianUInt32 TrackID;
        public fixed byte ReservedA[4];
        public BigEndianUInt64 Duration;
        public fixed byte ReservedB[8];
        public BigEndianInt16 Layer;
        public BigEndianInt16 AlternateGroup;
        public BigEndianInt16 Volume;
        public fixed byte ReservedC[2];
        public TransformationMatrix TransformMatrix;
        public BigEndianInt32 Width;
        public BigEndianInt32 Height;

        readonly float ITrackHeader.Volume =>
            Volume.Value / 256f;

        readonly int ITrackHeader.Width =>
            Width.Value / 65536;

        readonly int ITrackHeader.Height =>
            Height.Value / 65536;
    }
}
