using System.Runtime.InteropServices;
using ISchemm.MP4Support.Support;

namespace ISchemm.MP4Support.Atoms {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AtomHeader32 : IAtomHeader {
        public BigEndianUInt32 TotalSize;
        public ASCII32 BoxType;

        public readonly bool Is64Bit =>
            TotalSize.Value == 1;

        readonly long IAtomHeader.AtomSize =>
            TotalSize.Value;

        readonly ASCII32 IAtomHeader.BoxType =>
            BoxType;

        readonly unsafe int IAtomHeader.HeaderSize =>
            sizeof(AtomHeader32);
    }
}
