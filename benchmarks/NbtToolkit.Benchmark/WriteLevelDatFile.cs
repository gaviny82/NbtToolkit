using BenchmarkDotNet.Attributes;
using NbtToolkit.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NbtToolkit.Benchmark;

[MemoryDiagnoser(false)]
public class WriteLevelDatFile
{
    private const string LevelFile = "sample-files/sample-world-1_20_6-default/level.dat";
    private byte[] _buffer = new byte[128 * 1024];
    private MemoryStream _stream = null!;

    // Tags
    private TagCompound _rootTag = null!;
    private Substrate.Nbt.TagNodeCompound _rootTagSubstrate = null!;
    private fNbt.NbtCompound _rootTagFNbt = null!;

    // Writers
    private NbtWriter _nbtWriterMct = null!;
    private fNbt.NbtFile _nbtFilefNbt = null!;
    private Substrate.Nbt.NbtTree _nbtTreeSubstrate = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Read file into memory
        using (var fileStream = File.Open(LevelFile, FileMode.Open, FileAccess.Read))
        {
            using var nbtReader = new NbtReader(fileStream, NbtCompression.GZip);
            _rootTag = nbtReader.ReadRootTag();
        }
        using (var fileStream = File.Open(LevelFile, FileMode.Open, FileAccess.Read))
        {
            var fNbtFile = new fNbt.NbtFile();
            fNbtFile.LoadFromStream(fileStream, fNbt.NbtCompression.GZip);
            _rootTagFNbt = fNbtFile.RootTag;
        }
        using (var fileStream = File.Open(LevelFile, FileMode.Open, FileAccess.Read))
        {
            var substrate = new Substrate.Nbt.NbtTree();
            substrate.ReadFrom(new GZipStream(fileStream, CompressionMode.Decompress));
            _rootTagSubstrate = substrate.Root;
        }

        // Prepare write buffer
        _stream = new MemoryStream(_buffer);

        // Prepare writers
        _nbtWriterMct = new NbtWriter(_stream, NbtCompression.None, true);
        _nbtFilefNbt = new fNbt.NbtFile(_rootTagFNbt);
        _nbtTreeSubstrate = new Substrate.Nbt.NbtTree(_rootTagSubstrate);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _stream.Dispose();
        _nbtWriterMct.Dispose();
    }

    [Benchmark(Baseline = true)]
    public void Write()
    {
        _stream.Position = 0;
        _nbtWriterMct.WriteRootTag(_rootTag);
    }

    [Benchmark]
    public void Write_fNbt()
    {
        _stream.Position = 0;
        _nbtFilefNbt.SaveToStream(_stream, fNbt.NbtCompression.None);
    }

    [Benchmark]
    public void Write_Substrate()
    {
        _stream.Position = 0;
        _nbtTreeSubstrate.WriteTo(_stream);
    }
}
