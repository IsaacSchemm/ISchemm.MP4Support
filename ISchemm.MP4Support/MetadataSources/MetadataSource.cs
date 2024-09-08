using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ISchemm.MP4Support.MetadataSources
{
    public static class MetadataSource
    {
        private class StreamMetadataSource : IMetadataSource
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

        private class HttpMetadataSource : IMetadataSource
        {
            private readonly Uri _uri;
            private readonly HttpClient _httpClient;

            public HttpMetadataSource(Uri uri, HttpClient httpClient)
            {
                _uri = uri ?? throw new ArgumentNullException(nameof(uri));
                _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            }

            public async Task<byte[]?> GetRangeAsync(long start, long end)
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, _uri);
                req.Headers.Range = new RangeHeaderValue(start, end);
                using var resp = await _httpClient.SendAsync(req);

                if (resp.StatusCode == System.Net.HttpStatusCode.RequestedRangeNotSatisfiable)
                    return null;

                resp.EnsureSuccessStatusCode();

                return await resp.Content.ReadAsByteArrayAsync();
            }

            void IDisposable.Dispose() { }
        }

        private static readonly Lazy<HttpClient> _defaultHttpClient = new(() => new HttpClient());

        public static IMetadataSource FromUri(Uri uri) =>
            new HttpMetadataSource(
                uri,
                _defaultHttpClient.Value);

        public static IMetadataSource FromUri(Uri uri, HttpClient httpClient) =>
            new HttpMetadataSource(
                uri,
                httpClient);
    }
}
