using System;

namespace ISchemm.MP4Support.Atoms {
    public interface IMovieHeader {
        DateTimeOffset CreationTime { get; }
        DateTimeOffset ModificationTime { get; }
        int TimeScale { get; }
        TimeSpan Duration { get; }
    }
}
