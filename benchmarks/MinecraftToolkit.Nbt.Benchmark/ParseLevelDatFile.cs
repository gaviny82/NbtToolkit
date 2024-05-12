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

[MemoryDiagnoser]
public class ParseLevelDatFile
{
    //[Params(100)]
    public int N { get; set; }

    private const string LevelFile = "sample-files/sample-world-1_20_6-default/level.dat";

    private MemoryStream _stream = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var levelFileBytes = File.ReadAllBytes(LevelFile);
        _stream = new MemoryStream(levelFileBytes);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _stream.Dispose();
    }

    [Benchmark(Baseline = true)]
    public TagCompound Parse_MCT()
    {
        _stream.Position = 0;
        using var reader = new NbtReader(_stream, NbtCompression.GZip, true);
        return reader.ReadRootTag();
    }

    [Benchmark]
    public fNbt.NbtCompound Parse_fNbt()
    {
        _stream.Position = 0;
        fNbt.NbtFile file = new fNbt.NbtFile();
        file.LoadFromStream(_stream, fNbt.NbtCompression.GZip);
        return file.RootTag;
    }

    [Benchmark]
    public Substrate.Nbt.TagNodeCompound Parse_Substrate()
    {
        _stream.Position = 0;
        Substrate.Nbt.NbtTree tree = new();
        tree.ReadFrom(new GZipStream(_stream, CompressionMode.Decompress));
        return tree.Root;
    }
}
