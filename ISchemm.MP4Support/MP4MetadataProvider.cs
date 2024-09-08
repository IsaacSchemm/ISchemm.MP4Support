using ISchemm.MP4Support.Atoms;
using ISchemm.MP4Support.MetadataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ISchemm.MP4Support
{
    public class MP4MetadataProvider
    {
        public static async Task<AtomLocator?> FetchAtomHeaderAsync(IMetadataSource dataSource, long offset = 0)
        {
            if (await dataSource.GetRangeAsync(offset, offset + Marshal.SizeOf<AtomHeader32>()) is not byte[] data32)
                return null;

            AtomHeader32 header32 = MemoryMarshal.Cast<byte, AtomHeader32>(data32)[0];
            if (!header32.Is64Bit)
                return new AtomLocator(offset, header32);

            if (await dataSource.GetRangeAsync(offset, offset + Marshal.SizeOf<AtomHeader64>()) is not byte[] data64)
                return null;

            AtomHeader64 header64 = MemoryMarshal.Cast<byte, AtomHeader64>(data64)[0];
            return new AtomLocator(offset, header64);
        }

        public static async Task<IMovieHeader> ReadMovieHeaderAsync(IMetadataSource dataSource, AtomLocator atom)
        {
            if (atom.Header.BoxType.String != "mvhd")
                throw new ArgumentException("Incorrect atom type", nameof(atom));

            if (atom.Header.AtomSize == Marshal.SizeOf<MovieHeader32>())
                if (await dataSource.GetRangeAsync(atom.Offset, atom.Offset + atom.Header.AtomSize) is byte[] data32)
                    if (MemoryMarshal.TryRead(data32, out MovieHeader32 atom32) && atom32.Header.BoxType.String == "mvhd")
                        return atom32;

            if (atom.Header.AtomSize == Marshal.SizeOf<MovieHeader64>())
                if (await dataSource.GetRangeAsync(atom.Offset, atom.Offset + atom.Header.AtomSize) is byte[] data64)
                    if (MemoryMarshal.TryRead(data64, out MovieHeader64 atom64) && atom64.Header.BoxType.String == "mvhd")
                        return atom64;

            throw new Exception($"Incorrect atom offset ({atom.Offset}) or size ({atom.Header.AtomSize})");
        }

        public static async Task<ITrackHeader> ReadTrackHeaderAsync(IMetadataSource dataSource, AtomLocator atom)
        {
            if (atom.Header.BoxType.String != "tkhd")
                throw new ArgumentException("Incorrect atom type", nameof(atom));

            if (atom.Header.AtomSize == Marshal.SizeOf<TrackHeader32>())
                if (await dataSource.GetRangeAsync(atom.Offset, atom.Offset + atom.Header.AtomSize) is byte[] data32)
                    if (MemoryMarshal.TryRead(data32, out TrackHeader32 atom32) && atom32.Header.BoxType.String == "tkhd")
                        return atom32;

            if (atom.Header.AtomSize == Marshal.SizeOf<TrackHeader64>())
                if (await dataSource.GetRangeAsync(atom.Offset, atom.Offset + atom.Header.AtomSize) is byte[] data64)
                    if (MemoryMarshal.TryRead(data64, out TrackHeader64 atom64) && atom64.Header.BoxType.String == "tkhd")
                        return atom64;

            throw new Exception($"Incorrect atom offset ({atom.Offset}) or size ({atom.Header.AtomSize})");
        }

        public static async IAsyncEnumerable<AtomLocator> EnumerateAtomsAsync(IMetadataSource dataSource, AtomLocator? parent = null)
        {
            long end = parent == null
                ? long.MaxValue
                : parent.Offset + parent.Header.AtomSize;

            var atom = parent != null
                ? await FetchAtomHeaderAsync(dataSource, parent.Offset + parent.Header.HeaderSize)
                : await FetchAtomHeaderAsync(dataSource);

            while (atom is AtomLocator current)
            {
                yield return current;

                long next = current.Offset + current.Header.AtomSize;
                if (next >= end)
                    yield break;

                atom = await FetchAtomHeaderAsync(dataSource, next);
            }
        }

        public static async Task<MP4Metadata> GetMetadataAsync(IMetadataSource dataSource)
        {
            var metadata = new MP4Metadata
            {
                HasAudio = false,
                HasVideo = false
            };

            var moov = await EnumerateAtomsAsync(dataSource)
                .Where(atom => atom.Header.BoxType.String == "moov")
                .FirstOrDefaultAsync();

            if (moov == null)
                return metadata;

            var childAtoms = await EnumerateAtomsAsync(dataSource, moov).ToListAsync();

            var mvhd = childAtoms
                .Where(atom => atom.Header.BoxType.String == "mvhd")
                .FirstOrDefault();

            if (mvhd != null && await ReadMovieHeaderAsync(dataSource, mvhd) is IMovieHeader movieHeader)
                metadata.Duration = movieHeader.Duration;

            var tracks = childAtoms
                .Where(atom => atom.Header.BoxType.String == "trak");

            foreach (var track in tracks)
            {
                var tkhd = await EnumerateAtomsAsync(dataSource, track)
                    .Where(atom => atom.Header.BoxType.String == "tkhd")
                    .FirstOrDefaultAsync();

                if (tkhd == null)
                    continue;

                var trackHeader = await ReadTrackHeaderAsync(dataSource, tkhd);

                if (trackHeader.Volume != 0)
                {
                    metadata.HasAudio = true;
                }

                if (trackHeader.Width != 0 || trackHeader.Height != 0)
                {
                    metadata.Width ??= trackHeader.Width;
                    metadata.Height ??= trackHeader.Height;
                    metadata.HasVideo = true;
                }
            }

            return metadata;
        }
    }
}
