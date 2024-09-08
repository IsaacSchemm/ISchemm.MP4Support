[<AutoOpen>]
module internal Shared

open System
open System.Linq
open Microsoft.VisualStudio.TestTools.UnitTesting
open ISchemm.MP4Support
open ISchemm.MP4Support.Atoms
open ISchemm.MP4Support.Support
open ISchemm.MP4Support.MetadataSources

let bi16 = BigEndianInt16.FromValue
let bi32 = BigEndianInt32.FromValue
let bu32 = BigEndianUInt32.FromValue
let bi64 = BigEndianInt64.FromValue
let bu64 = BigEndianUInt64.FromValue
let ascii32 = ASCII32.FromString

let shouldBe (expected: 'T list) (actual: 'T list) =
    Assert.AreEqual(expected, actual)

let header32 boxType size = new AtomHeader32(
    BoxType = ascii32 boxType,
    TotalSize = bu32 size)

let header64 boxType size = new AtomHeader64(
    BoxType = ascii32 boxType,
    TotalSize = bu32 1u,
    ExtendedSize = bu64 size)

let locator offset header = new AtomLocator(
    Offset = offset,
    Header = header)

let enumerateRootAtoms source =
    MP4MetadataProvider.EnumerateAtomsAsync(source)
        .ToListAsync()
        .GetAwaiter()
        .GetResult()
        |> List.ofSeq

let enumerateAtoms source parent =
    MP4MetadataProvider.EnumerateAtomsAsync(source, parent)
        .ToListAsync()
        .GetAwaiter()
        .GetResult()
        |> List.ofSeq

let readMovieHeader source atom =
    MP4MetadataProvider.ReadMovieHeaderAsync(source, atom)
        .GetAwaiter()
        .GetResult()

let readTrackHeader source atom =
    MP4MetadataProvider.ReadTrackHeaderAsync(source, atom)
        .GetAwaiter()
        .GetResult()

let epoch = DateTimeOffset.Parse("1904-01-01T00:00:00Z")

let limitBytesRead (limit: int64) (source: IMetadataSource) =
    let mutable bytesRead = 0L
    {
        new IMetadataSource with
            member _.Dispose() = ()
            member _.GetRangeAsync(startIndex, endIndex) =
                let len = endIndex - startIndex
                bytesRead <- bytesRead + len
                if bytesRead > 1024 then
                    Assert.Fail("More than a kilobyte of data read")
                source.GetRangeAsync(startIndex, endIndex)
    }
