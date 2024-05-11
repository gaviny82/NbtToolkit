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
    const string LevelFile = "sample-files/sample-world-1_20_6-default/level.dat";
    private byte[] _levelFileBytes = null!;
    private MemoryStream _levelFileStream = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _levelFileBytes = File.ReadAllBytes(LevelFile);
    }

    [IterationSetup]
    public void Setup()
    {
        _levelFileStream = new MemoryStream(_levelFileBytes);
    }

    [IterationCleanup]
    public void Cleanup()
    {
        _levelFileStream.Dispose();
    }

    [Benchmark(Baseline = true)]
    public TagCompound Parse_MCT()
    {
        var reader = new NbtReader(_levelFileStream, NbtCompression.GZip);
        return reader.ReadRootTag();
    }

    [Benchmark]
    public fNbt.NbtCompound Parse_fNbt()
    {
        fNbt.NbtFile file = new fNbt.NbtFile();
        file.LoadFromStream(_levelFileStream, fNbt.NbtCompression.GZip);
        return file.RootTag;
    }

    [Benchmark]
    public Substrate.Nbt.TagNodeCompound Parse_Substrate()
    {
        Substrate.Nbt.NbtTree tree = new();
        tree.ReadFrom(new GZipStream(_levelFileStream, CompressionMode.Decompress));
        return tree.Root;
    }
}
