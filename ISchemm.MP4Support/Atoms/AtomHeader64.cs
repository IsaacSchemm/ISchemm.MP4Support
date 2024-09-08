using System.Runtime.InteropServices;
using ISchemm.MP4Support.Support;

namespace ISchemm.MP4Support.Atoms {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AtomHeader64 : IAtomHeader {
        public BigEndianUInt32 TotalSize;
        public ASCII32 BoxType;
        public BigEndianUInt64 ExtendedSize;

        readonly long IAtomHeader.AtomSize =>
            checked((long)ExtendedSize.Value);

        readonly ASCII32 IAtomHeader.BoxType =>
            BoxType;

        readonly unsafe int IAtomHeader.HeaderSize =>
            sizeof(AtomHeader64);
    }
}
