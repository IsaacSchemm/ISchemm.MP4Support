using ISchemm.MP4Support.Support;

namespace ISchemm.MP4Support.Atoms {
    public struct TransformationMatrix {
        public BigEndianUInt32 A;
        public BigEndianUInt32 B;
        public BigEndianUInt32 U;
        public BigEndianUInt32 C;
        public BigEndianUInt32 D;
        public BigEndianUInt32 V;
        public BigEndianUInt32 X;
        public BigEndianUInt32 Y;
        public BigEndianUInt32 W;
    }
}
