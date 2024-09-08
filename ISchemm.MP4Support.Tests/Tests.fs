namespace ISchemm.MP4Support.Tests

open System
open System.Text
open Microsoft.VisualStudio.TestTools.UnitTesting
open ISchemm.MP4Support.MetadataSources
open ISchemm.MP4Support.Atoms
open ISchemm.MP4Support

[<TestClass>]
type Tests() =
    [<TestMethod>]
    member _.TestMP4() =
        let source = MetadataSource.FromFile("../../../demo.mp4")

        let topLevelAtoms = enumerateRootAtoms source

        topLevelAtoms |> shouldBe [
            locator 0 (header32 "ftyp" 0x20u)
            locator 32 (header32 "free" 0x8u)
            locator 40 (header32 "mdat" 0x10aebu)
            locator 68371 (header32 "moov" 0x14e6u)
        ]

        let nextLevelAtoms = enumerateAtoms source topLevelAtoms[3]

        nextLevelAtoms |> shouldBe [
            locator 68379 (header32 "mvhd" 0x6cu)
            locator 68487 (header32 "trak" 0xe27u)
            locator 72110 (header32 "trak" 0x5eau)
            locator 73624 (header32 "udta" 0x61u)
        ]

        let movieHeader = readMovieHeader source nextLevelAtoms[0]

        [movieHeader.Duration] |> shouldBe [TimeSpan.Parse("0:00:02.484")]

        let timeScale = 1000

        [movieHeader] |> shouldBe [
            let mutable atom = new MovieHeader32()
            atom.Duration <- (TimeSpan.Parse("0:00:02.484").TotalSeconds * float timeScale) |> uint32 |> bu32
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

        let videoAtoms = enumerateAtoms source nextLevelAtoms[1]
        videoAtoms |> shouldBe [
            locator 68495 (header32 "tkhd" 0x5cu)
            locator 68587 (header32 "edts" 0x24u)
            locator 68623 (header32 "mdia" 0xd9fu)
        ]

        let videoTrackHeader = readTrackHeader source videoAtoms[0]

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

        let audioAtoms = enumerateAtoms source nextLevelAtoms[2]
        audioAtoms |> shouldBe [
            locator 72118 (header32 "tkhd" 0x5cu)
            locator 72210 (header32 "edts" 0x24u)
            locator 72246 (header32 "mdia" 0x53du)
            locator 73587 (header32 "udta" 0x25u)
        ]

        let audioTrackHeader = readTrackHeader source audioAtoms[0]

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

        let result = MP4MetadataProvider.GetMetadataAsync(source).GetAwaiter().GetResult()

        [result] |> shouldBe [
            let mutable res = new MP4Metadata()
            res.HasAudio <- true
            res.HasVideo <- true
            res.Width <- Nullable 640
            res.Height <- Nullable 360
            res.Duration <- Nullable (TimeSpan.Parse("0:00:02.484"))
            res
        ]

    [<TestMethod>]
    member _.TestMOV() =
        let source = MetadataSource.FromFile("../../../demo.mov")

        let topLevelAtoms = enumerateRootAtoms source

        topLevelAtoms |> shouldBe [
            locator 0 (header32 "ftyp" 0x14u)
            locator 20 (header32 "wide" 0x8u)
            locator 28 (header32 "mdat" 0x18a09u)
            locator 100901 (header32 "moov" 0x1e30u)
        ]

        let nextLevelAtoms = enumerateAtoms source topLevelAtoms[3]

        nextLevelAtoms |> shouldBe [
            locator 100909 (header32 "mvhd" 0x6cu)
            locator 101017 (header32 "trak" 0x152au)
            locator 106435 (header32 "trak" 0x872u)
            locator 108597 (header32 "udta" 0x20u)
        ]

        let movieHeader = readMovieHeader source nextLevelAtoms[0]

        [movieHeader.Duration] |> shouldBe [TimeSpan.Parse("0:00:03.95")]

        let videoAtoms = enumerateAtoms source nextLevelAtoms[1]
        videoAtoms |> shouldBe [
            locator 101025 (header32 "tkhd" 0x5cu)
            locator 101117 (header32 "edts" 0x24u)
            locator 101153 (header32 "mdia" 0x14a2u)
        ]

        let videoTrackHeader = readTrackHeader source videoAtoms[0]

        [videoTrackHeader.Volume] |> shouldBe [0f]
        [videoTrackHeader.Width] |> shouldBe [640]
        [videoTrackHeader.Height] |> shouldBe [360]

        let audioAtoms = enumerateAtoms source nextLevelAtoms[2]
        audioAtoms |> shouldBe [
            locator 106443 (header32 "tkhd" 0x5cu)
            locator 106535 (header32 "edts" 0x24u)
            locator 106571 (header32 "mdia" 0x7c5u)
            locator 108560 (header32 "udta" 0x25u)
        ]

        let audioTrackHeader = readTrackHeader source audioAtoms[0]

        [audioTrackHeader.Volume] |> shouldBe [1.0f]
        [audioTrackHeader.Width] |> shouldBe [0]
        [audioTrackHeader.Height] |> shouldBe [0]

    [<TestMethod>]
    member _.TestMP4ExtendedLength() =
        let source = MetadataSource.FromByteArray [|
            yield! Convert.FromBase64String "AAAAHGZ0eXBpc29tAAIAAGlzb21pc28ybXA0MQAAAAFtZGF0AAAAAAFBrUs="
            yield! Array.zeroCreate<byte> 21081403
            yield! Convert.FromHexString "00000008"
            yield! Encoding.ASCII.GetBytes "demo"
        |]

        let topLevelAtoms = enumerateRootAtoms source

        topLevelAtoms |> shouldBe [
            locator 0 (header32 "ftyp" 28u)
            locator 28 (header64 "mdat" 21081419UL)
            locator 21081447 (header32 "demo" 8u)
        ]

    [<TestMethod>]
    member _.TestISMAPartial() =
        let source =
            "AAAAIGZ0eXBpc21sAAAAAWlzbWxwaWZmaXNvMmlzb20AAAJdbW9vdgAAAHhtdmhkAQAAAAAAAADioqd3AAAAAOKip3cAmJaAAAAAMndso9UAAQAAAQAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAbQAAAbV0cmFrAAAAaHRraGQBAAAHAAAAAOKip3cAAAAA4qKndwAAAGwAAAAAAAAAMndso9UAAAAAAAAAAAAAAAABAAAAAAEAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAFFbWRpYQAAACxtZGhkAQAAAAAAAADioqd3AAAAAOKip3cAmJaAAAAAMndso9UVxwAAAAAAIWhkbHIAAAAAAAAAAHNvdW4AAAAAAAAAAAAAAAAAAAAA8G1pbmYAAAAQc21oZAAAAAAAAAAAAAAAJGRpbmYAAAAcZHJlZgAAAAAAAAABAAAADHVybCAAAAABAAAAtHN0YmwAAABoc3RzZAAAAAAAAAABAAAAWG1wNGEAAAAAAAAAAQAAAAAAAAAAAAIAEAAAAABdwAAAAAAANGVzZHMAAAAAAyYAAAAEFEAVAAYAAAIOqgAB9AAFBRMQVuWYBgECQwNlbmdDA2VuZwAAABBzdHRzAAAAAAAAAAAAAAAQc3RzYwAAAAAAAAAAAAAAFHN0c3oAAAAAAAAAAAAAAAAAAAAQc3RjbwAAAAAAAAAAAAAAKG12ZXgAAAAgdHJleAAAAAAAAABsAAAAAQAAAAAAAAAAAAEAAA=="
            |> Convert.FromBase64String
            |> MetadataSource.FromByteArray

        let topLevelAtoms = enumerateRootAtoms source

        topLevelAtoms |> shouldBe [
            locator 0 (header32 "ftyp" 32u)
            locator 32 (header32 "moov" 605u)
        ]

        let nextLevelAtoms = enumerateAtoms source topLevelAtoms[1]

        nextLevelAtoms |> shouldBe [
            locator 40 (header32 "mvhd" 120u)
            locator 160 (header32 "trak" 437u)
            locator 597 (header32 "mvex" 40u)
        ]

        let movieHeader = readMovieHeader source nextLevelAtoms[0]

        let creationTime = DateTimeOffset.Parse("2024-06-27T05:21:59Z")
        let modificationTime = creationTime
        let duration = TimeSpan.Parse("06:01:15.1973333")
        let timeScale = 10000000

        [movieHeader.CreationTime] |> shouldBe [creationTime]
        [movieHeader.ModificationTime] |> shouldBe [modificationTime]
        [movieHeader.Duration] |> shouldBe [duration]

        [movieHeader] |> shouldBe [
            let mutable atom = new MovieHeader64()
            atom.CreationTime <- (creationTime - epoch).TotalSeconds |> uint64 |> bu64
            atom.Duration <- (duration.TotalSeconds * float timeScale) |> uint64 |> bu64
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
            atom.ModificationTime <- (modificationTime - epoch).TotalSeconds |> uint64 |> bu64
            atom.NextTrackID <- bu32 109u
            atom.Rate <- bi32 0x10000
            atom.TimeScale <- bi32 timeScale
            atom.Version <- 1uy
            atom.Volume <- bi16 256s
            atom
        ]

        let trackAtoms = enumerateAtoms source nextLevelAtoms[1]
        trackAtoms |> shouldBe [
            locator 168 (header32 "tkhd" 104u)
            locator 272 (header32 "mdia" 325u)
        ]

        let trackHeader = readTrackHeader source trackAtoms[0]

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

    [<TestMethod>]
    member _.TestISMVPartial() =
        let source =
            "AAAAIGZ0eXBpc21sAAAAAWlzbWxwaWZmaXNvMmlzb20AAAKsbW9vdgAAAHhtdmhkAQAAAAAAAADioqd3AAAAAOKip3cAmJaAAAAAMndtDAAAAQAAAQAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAZQAAAgR0cmFrAAAAaHRraGQBAAAHAAAAAOKip3cAAAAA4qKndwAAAGQAAAAAAAAAMndtDAAAAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAABAAAAAAUAAAAC0AAAAAAGUbWRpYQAAACxtZGhkAQAAAAAAAADioqd3AAAAAOKip3cAmJaAAAAAMndtDABVxAAAAAAAIWhkbHIAAAAAAAAAAHZpZGUAAAAAAAAAAAAAAAAAAAABP21pbmYAAAAUdm1oZAAAAAEAAAAAAAAAAAAAACRkaW5mAAAAHGRyZWYAAAAAAAAAAQAAAAx1cmwgAAAAAQAAAP9zdGJsAAAAs3N0c2QAAAAAAAAAAQAAAKNhdmMxAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAUAAtABIAAAASAAAAAAAAAABCU1FRElBS0lORAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAGP//AAAAOWF2Y0MBTUAN/+EAImdNQA2WUoKDPz4DagICAoAAAAMAgAAAGXBgAMNQBhl3OAUBAARo7zyAAAAAFGJ0cnQAAMMAAAYagAAE4gAAAAAQc3R0cwAAAAAAAAAAAAAAEHN0c2MAAAAAAAAAAAAAABRzdHN6AAAAAAAAAAAAAAAAAAAAEHN0Y28AAAAAAAAAAAAAAChtdmV4AAAAIHRyZXgAAAAAAAAAZAAAAAEAAAAAAAAAAAABAAA="
            |> Convert.FromBase64String
            |> MetadataSource.FromByteArray

        let topLevelAtoms = enumerateRootAtoms source

        topLevelAtoms |> shouldBe [
            locator 0 (header32 "ftyp" 32u)
            locator 32 (header32 "moov" 684u)
        ]

        let nextLevelAtoms = enumerateAtoms source topLevelAtoms[1]

        nextLevelAtoms |> shouldBe [
            locator 40 (header32 "mvhd" 120u)
            locator 160 (header32 "trak" 516u)
            locator 676 (header32 "mvex" 40u)
        ]

        let movieHeader = readMovieHeader source nextLevelAtoms[0]

        let creationTime = DateTimeOffset.Parse("2024-06-27T05:21:59Z")
        let modificationTime = creationTime
        let duration = TimeSpan.Parse("06:01:15.2")
        let timeScale = 10000000

        [movieHeader.CreationTime] |> shouldBe [creationTime]
        [movieHeader.ModificationTime] |> shouldBe [modificationTime]
        [movieHeader.Duration] |> shouldBe [duration]

        [movieHeader] |> shouldBe [
            let mutable atom = new MovieHeader64()
            atom.CreationTime <- (creationTime - epoch).TotalSeconds |> uint64 |> bu64
            atom.Duration <- (duration.TotalSeconds * float timeScale) |> uint64 |> bu64
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
            atom.ModificationTime <- (modificationTime - epoch).TotalSeconds |> uint64 |> bu64
            atom.NextTrackID <- bu32 101u
            atom.Rate <- bi32 0x10000
            atom.TimeScale <- bi32 timeScale
            atom.Version <- 1uy
            atom.Volume <- bi16 256s
            atom
        ]

        let trackAtoms = enumerateAtoms source nextLevelAtoms[1]
        trackAtoms |> shouldBe [
            locator 168 (header32 "tkhd" 104u)
            locator 272 (header32 "mdia" 404u)
        ]

        let trackHeader = readTrackHeader source trackAtoms[0]

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

    [<TestMethod>]
    member _.TestHTTP() =
        let source =
            "https://ia801405.us.archive.org/6/items/SampleVideo1280x7205mb/SampleVideo_1280x720_5mb.mp4"
            |> Uri
            |> MetadataSource.FromUri

        let topLevelAtoms = enumerateRootAtoms source

        topLevelAtoms |> shouldBe [
            locator 0 (header32 "ftyp" 0x20u)
            locator 32 (header32 "free" 0x8u)
            locator 40 (header32 "mdat" 0x4fe4cbu)
            locator 5235955 (header32 "mdat" 0x8u)
            locator 5235963 (header32 "moov" 0x45fdu)
        ]

        let nextLevelAtoms = enumerateAtoms source topLevelAtoms[4]

        nextLevelAtoms |> shouldBe [
            locator 5235971 (header32 "mvhd" 0x6cu)
            locator 5236079 (header32 "trak" 0x1959u)
            locator 5242568 (header32 "trak" 0x2bd0u)
            locator 5253784 (header32 "udta" 0x60u)
        ]

        let movieHeader = readMovieHeader source nextLevelAtoms[0]

        [movieHeader.CreationTime] |> shouldBe [DateTimeOffset.UnixEpoch]
        [movieHeader.ModificationTime] |> shouldBe [DateTimeOffset.Parse("2014-07-19T17:22:11Z")]
        [movieHeader.Duration] |> shouldBe [TimeSpan.Parse("00:00:29.568")]

        let videoTrackAtoms = enumerateAtoms source nextLevelAtoms[1]
        videoTrackAtoms |> shouldBe [
            locator 5236087 (header32 "tkhd" 0x5cu)
            locator 5236179 (header32 "edts" 0x24u)
            locator 5236215 (header32 "mdia" 0x18d1u)
        ]

        let videoTrackHeader = readTrackHeader source videoTrackAtoms[0]

        [videoTrackHeader.Volume] |> shouldBe [0f]
        [videoTrackHeader.Width] |> shouldBe [1280]
        [videoTrackHeader.Height] |> shouldBe [720]

        let audioTrackAtoms = enumerateAtoms source nextLevelAtoms[2]
        audioTrackAtoms |> shouldBe [
            locator 5242576 (header32 "tkhd" 0x5cu)
            locator 5242668 (header32 "edts" 0x24u)
            locator 5242704 (header32 "mdia" 0x2b48u)
        ]

        let audioTrackHeader = readTrackHeader source audioTrackAtoms[0]

        [audioTrackHeader.Volume] |> shouldBe [1f]
        [audioTrackHeader.Width] |> shouldBe [0]
        [audioTrackHeader.Height] |> shouldBe [0]

    [<TestMethod>]
    member _.TestStopReading() =
        let interiorSource = MetadataSource.FromFile("../../../demo.m4a")

        let source = {
            new IMetadataSource with
                member _.Dispose() = ()
                member _.GetRangeAsync(startIndex, endIndex) =
                    if endIndex > 1015 then
                        Assert.Fail("Read past headers")
                    interiorSource.GetRangeAsync(startIndex, endIndex)
        }

        let result = MP4MetadataProvider.GetMetadataAsync(source).GetAwaiter().GetResult()

        [result] |> shouldBe [
            let mutable res = new MP4Metadata()
            res.HasAudio <- true
            res.HasVideo <- false
            res.Width <- Nullable()
            res.Height <- Nullable()
            res.Duration <- Nullable (TimeSpan.Parse("0:00:01.227"))
            res
        ]
