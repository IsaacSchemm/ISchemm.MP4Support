using ISchemm.MP4Support.Atoms;

namespace ISchemm.MP4Support
{
    public record AtomLocator(
        long Offset,
        IAtomHeader Header);
}
