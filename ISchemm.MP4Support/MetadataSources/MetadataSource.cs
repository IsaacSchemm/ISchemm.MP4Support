using System;
using System.IO;
using System.Threading.Tasks;

namespace ISchemm.MP4Support.MetadataSources
{
    public static class MetadataSource
    {
        internal class StreamMetadataSource : IMetadataSource
        {
            private readonly Stream _stream;

            public StreamMetadataSource(Stream stream)
            {
                _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            }

            public void Dispose()
            {
                _stream.Dispose();
            }

            public async Task<byte[]?> GetRangeAsync(long start, long end)
            {
                byte[] buffer = new byte[end - start];
                _stream.Position = start;
#if NET
                int read = await _stream.ReadAsync(buffer);
#else
                int read = await _stream.ReadAsync(buffer, 0, buffer.Length);
#endif
                return read == 0 ? null
                    : read == buffer.Length ? buffer
                    : throw new Exception("Not enough data in stream");
            }
        }

        public static IMetadataSource FromByteArray(byte[] data) =>
            new StreamMetadataSource(
                new MemoryStream(
                    data,
                    writable: false));

        public static IMetadataSource FromFile(string path) =>
            new StreamMetadataSource(
                new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read));
    }
}
