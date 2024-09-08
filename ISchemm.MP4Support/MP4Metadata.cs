using System;

namespace ISchemm.MP4Support
{
    public struct MP4Metadata
    {
        public bool HasAudio;
        public bool HasVideo;
        public int? Width;
        public int? Height;
        public TimeSpan? Duration;
    }
}
