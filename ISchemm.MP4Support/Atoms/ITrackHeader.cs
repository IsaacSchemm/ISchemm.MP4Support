using System;

namespace ISchemm.MP4Support.Atoms {
    public interface ITrackHeader {
        float Volume { get; }
        int Width { get; }
        int Height { get; }
    }
}
