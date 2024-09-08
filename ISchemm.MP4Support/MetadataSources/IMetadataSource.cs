using System;
using System.Threading.Tasks;

namespace ISchemm.MP4Support.MetadataSources
{
    public interface IMetadataSource : IDisposable
    {
        Task<byte[]?> GetRangeAsync(long start, long end);
    }
}
