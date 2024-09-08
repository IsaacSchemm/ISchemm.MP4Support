using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace ISchemm.MP4Support.Support {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct BigEndianInt16 {
        public short _data;

        public readonly short Value =>
            BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(_data)
            : _data;

        public readonly override unsafe string ToString() =>
            $"{Value} ({Value:X4})";

        public static unsafe BigEndianInt16 FromValue(short value) => new()
        {
            _data = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(value)
                : value
        };
    }
}
