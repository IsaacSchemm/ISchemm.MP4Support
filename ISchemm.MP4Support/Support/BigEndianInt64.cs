using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace ISchemm.MP4Support.Support {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct BigEndianInt64 {
        public long _data;

        public readonly long Value =>
            BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(_data)
            : _data;

        public readonly override unsafe string ToString() =>
            $"{Value} ({Value:X16})";

        public static unsafe BigEndianInt64 FromValue(long value) => new()
        {
            _data = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(value)
                : value
        };
    }
}
