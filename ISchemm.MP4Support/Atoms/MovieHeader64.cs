using System;
using System.Runtime.InteropServices;
using ISchemm.MP4Support.Support;

namespace ISchemm.MP4Support.Atoms {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MovieHeader64 : IMovieHeader {
        public AtomHeader32 Header;
        public byte Version;
        public byte FlagsA;
        public byte FlagsB;
        public byte FlagsC;
        public BigEndianUInt64 CreationTime;
        public BigEndianUInt64 ModificationTime;
        public BigEndianInt32 TimeScale;
        public BigEndianUInt64 Duration;
        public BigEndianInt32 Rate;
        public BigEndianInt16 Volume;
        public byte ReservedA01;
        public byte ReservedA02;
        public byte ReservedA03;
        public byte ReservedA04;
        public byte ReservedA05;
        public byte ReservedA06;
        public byte ReservedA07;
        public byte ReservedA08;
        public byte ReservedA09;
        public byte ReservedA10;
        public TransformationMatrix Matrix;
        public long ReservedB01;
        public long ReservedB02;
        public long ReservedB03;
        public BigEndianUInt32 NextTrackID;

        private static readonly DateTime MacintoshEpoch =
            new(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        readonly DateTimeOffset IMovieHeader.CreationTime =>
            MacintoshEpoch.AddSeconds(CreationTime.Value);

        readonly DateTimeOffset IMovieHeader.ModificationTime =>
            MacintoshEpoch.AddSeconds(ModificationTime.Value);

        readonly int IMovieHeader.TimeScale =>
            TimeScale.Value;

        readonly TimeSpan IMovieHeader.Duration =>
            TimeSpan.FromSeconds(Duration.Value / (double)TimeScale.Value);
    }
}
