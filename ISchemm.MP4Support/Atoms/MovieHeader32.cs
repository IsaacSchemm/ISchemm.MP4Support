using System;
using System.Runtime.InteropServices;
using ISchemm.MP4Support.Support;

namespace ISchemm.MP4Support.Atoms {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MovieHeader32 : IMovieHeader {
        public AtomHeader32 Header;
        public byte Version;
        public fixed byte Flags[3];
        public BigEndianInt32 CreationTime;
        public BigEndianInt32 ModificationTime;
        public BigEndianInt32 TimeScale;
        public BigEndianInt32 Duration;
        public BigEndianInt32 Rate;
        public BigEndianInt16 Volume;
        public fixed byte ReservedA[10];
        public TransformationMatrix Matrix;
        public fixed byte ReservedB[24];
        public BigEndianUInt32 NextTrackID;

        public readonly bool Is64Bit =>
            Version == 1;

        private readonly static DateTime MacintoshEpoch =
            new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
