using ISchemm.MP4Support.Support;

namespace ISchemm.MP4Support.Atoms {
    public interface IAtomHeader {
        long AtomSize { get; }
        ASCII32 BoxType { get; }
        int HeaderSize { get; }
    }
}
