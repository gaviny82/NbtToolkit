using BenchmarkDotNet.Attributes;
using MinecraftToolkit.Nbt.Parsing;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Benchmark;

[MemoryDiagnoser(false)]
public class ParseLevelDatFile
{
    private const string LevelFile = "sample-files/sample-world-1_20_6-default/level.dat";
    private MemoryStream _stream = null!;

    private NbtReader_StateMachineImpl _nbtReaderMct = null!;
    private NbtReader _nbtRecursiveReaderMct = null!;
    private fNbt.NbtFile _nbtFilefNbt = null!;
    private Substrate.Nbt.NbtTree _nbtTreeSubstrate = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Decompress the file and store it in memory
        using var fileStream = new FileStream(LevelFile, FileMode.Open, FileAccess.Read);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        using var memoryStream = new MemoryStream();
        gzipStream.CopyTo(memoryStream);
        byte[] bytes = memoryStream.ToArray();

        // Create a stream from the decompressed bytes
        _stream = new MemoryStream(bytes);

        // Prepare parsers
        _nbtReaderMct = new NbtReader_StateMachineImpl(_stream, NbtCompression.None, true);
        _nbtRecursiveReaderMct = new NbtReader(_stream, NbtCompression.None, true);
        _nbtFilefNbt = new fNbt.NbtFile();
        _nbtTreeSubstrate = new Substrate.Nbt.NbtTree();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _stream.Dispose();
        _nbtReaderMct.Dispose();
        _nbtRecursiveReaderMct.Dispose();
    }

    [Benchmark(Baseline = true)]
    public void Parse_MCT()
    {
        _stream.Position = 0;
        _nbtReaderMct.ReadRootTag();
    }

    [Benchmark]
    public void Parse_MCT_Recursive()
    {
        _stream.Position = 0;
        _nbtRecursiveReaderMct.ReadRootTag();
    }

    [Benchmark]
    public void Parse_fNbt()
    {
        _stream.Position = 0;
        _nbtFilefNbt.LoadFromStream(_stream, fNbt.NbtCompression.None);
    }

    [Benchmark]
    public void Parse_Substrate()
    {
        _stream.Position = 0;
        _nbtTreeSubstrate.ReadFrom(_stream);
    }
}
