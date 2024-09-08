using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace ISchemm.MP4Support.Support {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct BigEndianUInt32 {
        public uint _data;

        public readonly uint Value =>
            BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(_data)
            : _data;

        public readonly override unsafe string ToString() =>
            $"{Value} ({Value:X8})";

        public static unsafe BigEndianUInt32 FromValue(uint value) => new()
        {
            _data = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(value)
                : value
        };
    }
}
