using System.Threading.Tasks;

namespace ISchemm.MP4Support
{
    public interface IMetadataSource
    {
        Task<byte[]?> GetRangeAsync(long start, long end);
    }
}
