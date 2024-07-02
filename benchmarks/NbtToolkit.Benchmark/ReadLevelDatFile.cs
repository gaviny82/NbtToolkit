using BenchmarkDotNet.Attributes;
using NbtToolkit.Binary;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NbtToolkit.Benchmark;

[MemoryDiagnoser(false)]
public class ReadLevelDatFile
{
    private const string LevelFile = "sample-files/sample-world-1_20_6-default/level.dat";
    private MemoryStream _stream = null!;

    private NbtReader_StateMachineImpl _nbtReader_StateMachine = null!;
    private NbtReader _nbtReader = null!;
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
        _nbtReader_StateMachine = new NbtReader_StateMachineImpl(_stream, NbtCompression.None, true);
        _nbtReader = new NbtReader(_stream, NbtCompression.None, true);
        _nbtFilefNbt = new fNbt.NbtFile();
        _nbtTreeSubstrate = new Substrate.Nbt.NbtTree();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _stream.Dispose();
        _nbtReader_StateMachine.Dispose();
        _nbtReader.Dispose();
    }

    [Benchmark(Baseline = true)]
    public void Read()
    {
        _stream.Position = 0;
        _nbtReader.ReadRootTag();
    }

    [Benchmark]
    public void Read_StateMachine()
    {
        _stream.Position = 0;
        _nbtReader_StateMachine.ReadRootTag();
    }

    [Benchmark]
    public void Read_fNbt()
    {
        _stream.Position = 0;
        _nbtFilefNbt.LoadFromStream(_stream, fNbt.NbtCompression.None);
    }

    [Benchmark]
    public void Read_Substrate()
    {
        _stream.Position = 0;
        _nbtTreeSubstrate.ReadFrom(_stream);
    }
}
