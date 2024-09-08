using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace ISchemm.MP4Support.Support {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct BigEndianUInt64 {
        public ulong _data;

        public readonly ulong Value =>
            BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(_data)
            : _data;

        public readonly override unsafe string ToString() =>
            $"{Value} ({Value:X16})";

        public static unsafe BigEndianUInt64 FromValue(ulong value) => new()
        {
            _data = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(value)
                : value
        };
    }
}
