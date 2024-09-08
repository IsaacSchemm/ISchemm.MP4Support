# ISchemm.MP4Support

A C# library to find and read data from the `mvhd` and `tkhd` atoms  (in .mp4,
.m4a,  or .mov files) without having to read the entire file.

Basic usage:

    IMetadataSource source = MetadataSource.FromUri(
        new Uri("https://www.example.com/video.mp4"));
    MP4Metadata metadata = await MP4MetadataProvider.GetMetadataAsync(source);

or

    IMetadataSource source = MetadataSource.FromFile("../video.mp4");
    MP4Metadata metadata = await MP4MetadataProvider.GetMetadataAsync(source);

This provides:

* whether an audio stream was found (volume not zero)
* whether a video stream was found (resolution not zero)
* width in pixels
* height in pixels
* duration (as a `System.TimeSpan`)

Targets .NET Framework 4.8 and .NET 8.0+.
