﻿using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace ISchemm.MP4Support.Support {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct BigEndianInt32 {
        public int _data;

        public readonly int Value =>
            BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(_data)
            : _data;

        public readonly override unsafe string ToString() =>
            $"{Value} ({Value:X8})";

        public static unsafe BigEndianInt32 FromValue(int value) => new()
        {
            _data = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(value)
                : value
        };
    }
}
