namespace ISchemm.MP4Support.Tests

open System
open System.IO
open System.Linq
open System.Text
open System.Threading.Tasks
open Microsoft.VisualStudio.TestTools.UnitTesting
open ISchemm.MP4Support
open ISchemm.MP4Support.Atoms
open ISchemm.MP4Support.Support

[<TestClass>]
type TestClass() =
    let asSource (data: byte[]) =
        let mutable bytesRead = 0L
        {
            new IMetadataSource with
                member _.GetRangeAsync (startIndex, endIndex) = task {
                    if startIndex >= data.Length
                    then return null
                    else
                        let len = endIndex - startIndex
                        bytesRead <- bytesRead + len
                        if bytesRead > 1024 then
                            Assert.Fail("More than a kilobyte of data read")
                        return data.AsMemory().Slice(int startIndex, int len).ToArray()
                }
        }

    let bi16 = BigEndianInt16.FromValue
    let bi32 = BigEndianInt32.FromValue
    let bu32 = BigEndianUInt32.FromValue
    let bi64 = BigEndianInt64.FromValue
    let bu64 = BigEndianUInt64.FromValue
    let ascii32 = ASCII32.FromString

    let shouldBe (expected: 'T list) (actual: 'T seq) =
        Assert.AreEqual(expected, List.ofSeq actual)

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

    let epoch = DateTimeOffset.Parse("1904-01-01T00:00:00Z")

    [<TestMethod>]
    member _.TestMP4() =
        task {
            let source =
                "../../../demo.mp4"
                |> File.ReadAllBytes
                |> asSource

            let! topLevelAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source).ToListAsync()

            topLevelAtoms |> shouldBe [
                locator 0 (header32 "ftyp" 0x20u)
                locator 32 (header32 "free" 0x8u)
                locator 40 (header32 "mdat" 0x10aebu)
                locator 68371 (header32 "moov" 0x14e6u)
            ]

            let! nextLevelAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source, topLevelAtoms[3]).ToListAsync()

            nextLevelAtoms |> shouldBe [
                locator 68379 (header32 "mvhd" 0x6cu)
                locator 68487 (header32 "trak" 0xe27u)
                locator 72110 (header32 "trak" 0x5eau)
                locator 73624 (header32 "udta" 0x61u)
            ]

            let! movieHeader = MP4MetadataProvider.ReadMovieHeaderAsync(source, nextLevelAtoms[0])

            [movieHeader.Duration] |> shouldBe [TimeSpan.Parse("0:00:02.484")]

            let timeScale = 1000

            [movieHeader] |> shouldBe [
                let mutable atom = new MovieHeader32()
                atom.Duration <- (TimeSpan.Parse("0:00:02.484").TotalSeconds * float timeScale) |> int32 |> bi32
                atom.Header <-
                    let mutable header = new AtomHeader32()
                    header.BoxType <- ascii32 "mvhd"
                    header.TotalSize <- bu32 108u
                    header
                atom.Matrix <-
                    let mutable matrix = new TransformationMatrix()
                    matrix.A <- bu32 0x10000u
                    matrix.D <- bu32 0x10000u
                    matrix.W <- bu32 0x40000000u
                    matrix
                atom.NextTrackID <- bu32 3u
                atom.Rate <- bi32 0x10000
                atom.TimeScale <- bi32 timeScale
                atom.Volume <- bi16 256s
                atom
            ]

            let! videoAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source, nextLevelAtoms[1]).ToListAsync()
            videoAtoms |> shouldBe [
                locator 68495 (header32 "tkhd" 0x5cu)
                locator 68587 (header32 "edts" 0x24u)
                locator 68623 (header32 "mdia" 0xd9fu)
            ]

            let! videoTrackHeader = MP4MetadataProvider.ReadTrackHeaderAsync(source, videoAtoms[0])

            [videoTrackHeader.Volume] |> shouldBe [0f]
            [videoTrackHeader.Width] |> shouldBe [640]
            [videoTrackHeader.Height] |> shouldBe [360]

            [videoTrackHeader] |> shouldBe [
                let mutable atom = new TrackHeader32()
                atom.Duration <- (TimeSpan.Parse("0:00:02.484").TotalSeconds * float timeScale) |> uint32 |> bu32
                atom.Header <-
                    let mutable header = new AtomHeader32()
                    header.BoxType <- ascii32 "tkhd"
                    header.TotalSize <- bu32 92u
                    header
                atom.Height <- bi32 (360 <<< 16)
                atom.TrackID <- bu32 1u
                atom.TransformMatrix <-
                    let mutable matrix = new TransformationMatrix()
                    matrix.A <- bu32 0x10000u
                    matrix.D <- bu32 0x10000u
                    matrix.W <- bu32 0x40000000u
                    matrix
                atom.Width <- bi32 (640 <<< 16)
                atom
            ]

            let! audioAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source, nextLevelAtoms[2]).ToListAsync()
            audioAtoms |> shouldBe [
                locator 72118 (header32 "tkhd" 0x5cu)
                locator 72210 (header32 "edts" 0x24u)
                locator 72246 (header32 "mdia" 0x53du)
                locator 73587 (header32 "udta" 0x25u)
            ]

            let! audioTrackHeader = MP4MetadataProvider.ReadTrackHeaderAsync(source, audioAtoms[0])

            [audioTrackHeader.Volume] |> shouldBe [1.0f]
            [audioTrackHeader.Width] |> shouldBe [0]
            [audioTrackHeader.Height] |> shouldBe [0]

            [audioTrackHeader] |> shouldBe [
                let mutable atom = new TrackHeader32()
                atom.AlternateGroup <- bi16 1s
                atom.Duration <- (TimeSpan.Parse("0:00:02.454").TotalSeconds * float timeScale) |> uint32 |> bu32
                atom.Header <-
                    let mutable header = new AtomHeader32()
                    header.BoxType <- ascii32 "tkhd"
                    header.TotalSize <- bu32 92u
                    header
                atom.TrackID <- bu32 2u
                atom.TransformMatrix <-
                    let mutable matrix = new TransformationMatrix()
                    matrix.A <- bu32 0x10000u
                    matrix.D <- bu32 0x10000u
                    matrix.W <- bu32 0x40000000u
                    matrix
                atom.Volume <- bi16 256s
                atom
            ]
        }
        :> Task

    [<TestMethod>]
    member _.TestMOV() =
        task {
            let source =
                "../../../demo.mov"
                |> File.ReadAllBytes
                |> asSource

            let! topLevelAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source).ToListAsync()

            topLevelAtoms |> shouldBe [
                locator 0 (header32 "ftyp" 0x14u)
                locator 20 (header32 "wide" 0x8u)
                locator 28 (header32 "mdat" 0x18a09u)
                locator 100901 (header32 "moov" 0x1e30u)
            ]

            let! nextLevelAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source, topLevelAtoms[3]).ToListAsync()

            nextLevelAtoms |> shouldBe [
                locator 100909 (header32 "mvhd" 0x6cu)
                locator 101017 (header32 "trak" 0x152au)
                locator 106435 (header32 "trak" 0x872u)
                locator 108597 (header32 "udta" 0x20u)
            ]

            let! movieHeader = MP4MetadataProvider.ReadMovieHeaderAsync(source, nextLevelAtoms[0])

            [movieHeader.Duration] |> shouldBe [TimeSpan.Parse("0:00:03.95")]

            let! videoAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source, nextLevelAtoms[1]).ToListAsync()
            videoAtoms |> shouldBe [
                locator 101025 (header32 "tkhd" 0x5cu)
                locator 101117 (header32 "edts" 0x24u)
                locator 101153 (header32 "mdia" 0x14a2u)
            ]

            let! videoTrackHeader = MP4MetadataProvider.ReadTrackHeaderAsync(source, videoAtoms[0])

            [videoTrackHeader.Volume] |> shouldBe [0f]
            [videoTrackHeader.Width] |> shouldBe [640]
            [videoTrackHeader.Height] |> shouldBe [360]

            let! audioAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source, nextLevelAtoms[2]).ToListAsync()
            audioAtoms |> shouldBe [
                locator 106443 (header32 "tkhd" 0x5cu)
                locator 106535 (header32 "edts" 0x24u)
                locator 106571 (header32 "mdia" 0x7c5u)
                locator 108560 (header32 "udta" 0x25u)
            ]

            let! audioTrackHeader = MP4MetadataProvider.ReadTrackHeaderAsync(source, audioAtoms[0])

            [audioTrackHeader.Volume] |> shouldBe [1.0f]
            [audioTrackHeader.Width] |> shouldBe [0]
            [audioTrackHeader.Height] |> shouldBe [0]
        }
        :> Task

    [<TestMethod>]
    member _.TestMP4ExtendedLength() =
        task {
            let source = asSource [|
                yield! Convert.FromBase64String "AAAAHGZ0eXBpc29tAAIAAGlzb21pc28ybXA0MQAAAAFtZGF0AAAAAAFBrUs="
                yield! Array.zeroCreate<byte> 21081403
                yield! Convert.FromHexString "00000008"
                yield! Encoding.ASCII.GetBytes "demo"
            |]

            let! topLevelAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source).ToListAsync()

            topLevelAtoms |> shouldBe [
                locator 0 (header32 "ftyp" 28u)
                locator 28 (header64 "mdat" 21081419UL)
                locator 21081447 (header32 "demo" 8u)
            ]
        }
        :> Task

    [<TestMethod>]
    member _.TestISMA() =
        task {
            let source =
                "AAAAIGZ0eXBpc21sAAAAAWlzbWxwaWZmaXNvMmlzb20AAAJdbW9vdgAAAHhtdmhkAQAAAAAAAADioqd3AAAAAOKip3cAmJaAAAAAMndso9UAAQAAAQAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAbQAAAbV0cmFrAAAAaHRraGQBAAAHAAAAAOKip3cAAAAA4qKndwAAAGwAAAAAAAAAMndso9UAAAAAAAAAAAAAAAABAAAAAAEAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAFFbWRpYQAAACxtZGhkAQAAAAAAAADioqd3AAAAAOKip3cAmJaAAAAAMndso9UVxwAAAAAAIWhkbHIAAAAAAAAAAHNvdW4AAAAAAAAAAAAAAAAAAAAA8G1pbmYAAAAQc21oZAAAAAAAAAAAAAAAJGRpbmYAAAAcZHJlZgAAAAAAAAABAAAADHVybCAAAAABAAAAtHN0YmwAAABoc3RzZAAAAAAAAAABAAAAWG1wNGEAAAAAAAAAAQAAAAAAAAAAAAIAEAAAAABdwAAAAAAANGVzZHMAAAAAAyYAAAAEFEAVAAYAAAIOqgAB9AAFBRMQVuWYBgECQwNlbmdDA2VuZwAAABBzdHRzAAAAAAAAAAAAAAAQc3RzYwAAAAAAAAAAAAAAFHN0c3oAAAAAAAAAAAAAAAAAAAAQc3RjbwAAAAAAAAAAAAAAKG12ZXgAAAAgdHJleAAAAAAAAABsAAAAAQAAAAAAAAAAAAEAAA=="
                |> Convert.FromBase64String
                |> asSource

            let! topLevelAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source).ToListAsync()

            topLevelAtoms |> shouldBe [
                locator 0 (header32 "ftyp" 32u)
                locator 32 (header32 "moov" 605u)
            ]

            let! nextLevelAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source, topLevelAtoms[1]).ToListAsync()

            nextLevelAtoms |> shouldBe [
                locator 40 (header32 "mvhd" 120u)
                locator 160 (header32 "trak" 437u)
                locator 597 (header32 "mvex" 40u)
            ]

            let! movieHeader = MP4MetadataProvider.ReadMovieHeaderAsync(source, nextLevelAtoms[0])

            let creationTime = DateTimeOffset.Parse("2024-06-27T05:21:59Z")
            let modificationTime = creationTime
            let duration = TimeSpan.Parse("06:01:15.1973333")
            let timeScale = 10000000

            [movieHeader.CreationTime] |> shouldBe [creationTime]
            [movieHeader.ModificationTime] |> shouldBe [modificationTime]
            [movieHeader.Duration] |> shouldBe [duration]

            [movieHeader] |> shouldBe [
                let mutable atom = new MovieHeader64()
                atom.CreationTime <- (creationTime - epoch).TotalSeconds |> int64 |> bi64
                atom.Duration <- (duration.TotalSeconds * float timeScale) |> int64 |> bi64
                atom.Header <-
                    let mutable header = new AtomHeader32()
                    header.BoxType <- ascii32 "mvhd"
                    header.TotalSize <- bu32 120u
                    header
                atom.Matrix <-
                    let mutable matrix = new TransformationMatrix()
                    matrix.A <- bu32 0x10000u
                    matrix.D <- bu32 0x10000u
                    matrix.W <- bu32 0x40000000u
                    matrix
                atom.ModificationTime <- (modificationTime - epoch).TotalSeconds |> int64 |> bi64
                atom.NextTrackID <- bu32 109u
                atom.Rate <- bi32 0x10000
                atom.TimeScale <- bi32 timeScale
                atom.Version <- 1uy
                atom.Volume <- bi16 256s
                atom
            ]

            let! trackAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source, nextLevelAtoms[1]).ToListAsync()
            trackAtoms |> shouldBe [
                locator 168 (header32 "tkhd" 104u)
                locator 272 (header32 "mdia" 325u)
            ]

            let! trackHeader = MP4MetadataProvider.ReadTrackHeaderAsync(source, trackAtoms[0])

            [trackHeader.Volume] |> shouldBe [1.0f]
            [trackHeader.Width] |> shouldBe [0]
            [trackHeader.Height] |> shouldBe [0]

            [trackHeader] |> shouldBe [
                let mutable atom = new TrackHeader64()
                atom.CreationTime <- (creationTime - epoch).TotalSeconds |> uint64 |> bu64
                atom.Duration <- (duration.TotalSeconds * float timeScale) |> uint64 |> bu64
                atom.Header <-
                    let mutable header = new AtomHeader32()
                    header.BoxType <- ascii32 "tkhd"
                    header.TotalSize <- bu32 104u
                    header
                atom.ModificationTime <- (modificationTime - epoch).TotalSeconds |> uint64 |> bu64
                atom.TrackID <- bu32 108u
                atom.TransformMatrix <-
                    let mutable matrix = new TransformationMatrix()
                    matrix.A <- bu32 0x10000u
                    matrix.D <- bu32 0x10000u
                    matrix.W <- bu32 0x40000000u
                    matrix
                atom.Version <- 1uy
                atom.Volume <- bi16 256s
                atom
            ]
        }
        :> Task

    [<TestMethod>]
    member _.TestISMV() =
        task {
            let source =
                "AAAAIGZ0eXBpc21sAAAAAWlzbWxwaWZmaXNvMmlzb20AAAKsbW9vdgAAAHhtdmhkAQAAAAAAAADioqd3AAAAAOKip3cAmJaAAAAAMndtDAAAAQAAAQAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAZQAAAgR0cmFrAAAAaHRraGQBAAAHAAAAAOKip3cAAAAA4qKndwAAAGQAAAAAAAAAMndtDAAAAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAABAAAAAAUAAAAC0AAAAAAGUbWRpYQAAACxtZGhkAQAAAAAAAADioqd3AAAAAOKip3cAmJaAAAAAMndtDABVxAAAAAAAIWhkbHIAAAAAAAAAAHZpZGUAAAAAAAAAAAAAAAAAAAABP21pbmYAAAAUdm1oZAAAAAEAAAAAAAAAAAAAACRkaW5mAAAAHGRyZWYAAAAAAAAAAQAAAAx1cmwgAAAAAQAAAP9zdGJsAAAAs3N0c2QAAAAAAAAAAQAAAKNhdmMxAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAUAAtABIAAAASAAAAAAAAAABCU1FRElBS0lORAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAGP//AAAAOWF2Y0MBTUAN/+EAImdNQA2WUoKDPz4DagICAoAAAAMAgAAAGXBgAMNQBhl3OAUBAARo7zyAAAAAFGJ0cnQAAMMAAAYagAAE4gAAAAAQc3R0cwAAAAAAAAAAAAAAEHN0c2MAAAAAAAAAAAAAABRzdHN6AAAAAAAAAAAAAAAAAAAAEHN0Y28AAAAAAAAAAAAAAChtdmV4AAAAIHRyZXgAAAAAAAAAZAAAAAEAAAAAAAAAAAABAAA="
                |> Convert.FromBase64String
                |> asSource

            let! topLevelAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source).ToListAsync()

            topLevelAtoms |> shouldBe [
                locator 0 (header32 "ftyp" 32u)
                locator 32 (header32 "moov" 684u)
            ]

            let! nextLevelAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source, topLevelAtoms[1]).ToListAsync()

            nextLevelAtoms |> shouldBe [
                locator 40 (header32 "mvhd" 120u)
                locator 160 (header32 "trak" 516u)
                locator 676 (header32 "mvex" 40u)
            ]

            let! movieHeader = MP4MetadataProvider.ReadMovieHeaderAsync(source, nextLevelAtoms[0])

            let creationTime = DateTimeOffset.Parse("2024-06-27T05:21:59Z")
            let modificationTime = creationTime
            let duration = TimeSpan.Parse("06:01:15.2")
            let timeScale = 10000000

            [movieHeader.CreationTime] |> shouldBe [creationTime]
            [movieHeader.ModificationTime] |> shouldBe [modificationTime]
            [movieHeader.Duration] |> shouldBe [duration]

            [movieHeader] |> shouldBe [
                let mutable atom = new MovieHeader64()
                atom.CreationTime <- (creationTime - epoch).TotalSeconds |> int64 |> bi64
                atom.Duration <- (duration.TotalSeconds * float timeScale) |> int64 |> bi64
                atom.Header <-
                    let mutable header = new AtomHeader32()
                    header.BoxType <- ascii32 "mvhd"
                    header.TotalSize <- bu32 120u
                    header
                atom.Matrix <-
                    let mutable matrix = new TransformationMatrix()
                    matrix.A <- bu32 0x10000u
                    matrix.D <- bu32 0x10000u
                    matrix.W <- bu32 0x40000000u
                    matrix
                atom.ModificationTime <- (modificationTime - epoch).TotalSeconds |> int64 |> bi64
                atom.NextTrackID <- bu32 101u
                atom.Rate <- bi32 0x10000
                atom.TimeScale <- bi32 timeScale
                atom.Version <- 1uy
                atom.Volume <- bi16 256s
                atom
            ]

            let! trackAtoms = MP4MetadataProvider.EnumerateAtomsAsync(source, nextLevelAtoms[1]).ToListAsync()
            trackAtoms |> shouldBe [
                locator 168 (header32 "tkhd" 104u)
                locator 272 (header32 "mdia" 404u)
            ]

            let! trackHeader = MP4MetadataProvider.ReadTrackHeaderAsync(source, trackAtoms[0])

            [trackHeader.Volume] |> shouldBe [0f]
            [trackHeader.Width] |> shouldBe [320]
            [trackHeader.Height] |> shouldBe [180]

            [trackHeader] |> shouldBe [
                let mutable atom = new TrackHeader64()
                atom.CreationTime <- (creationTime - epoch).TotalSeconds |> uint64 |> bu64
                atom.Duration <- (duration.TotalSeconds * float timeScale) |> uint64 |> bu64
                atom.Header <-
                    let mutable header = new AtomHeader32()
                    header.BoxType <- ascii32 "tkhd"
                    header.TotalSize <- bu32 104u
                    header
                atom.Height <- bi32 (180 <<< 16)
                atom.ModificationTime <- (modificationTime - epoch).TotalSeconds |> uint64 |> bu64
                atom.TrackID <- bu32 100u
                atom.TransformMatrix <-
                    let mutable matrix = new TransformationMatrix()
                    matrix.A <- bu32 0x10000u
                    matrix.D <- bu32 0x10000u
                    matrix.W <- bu32 0x40000000u
                    matrix
                atom.Version <- 1uy
                atom.Width <- bi32 (320 <<< 16)
                atom
            ]
        }
        :> Task
