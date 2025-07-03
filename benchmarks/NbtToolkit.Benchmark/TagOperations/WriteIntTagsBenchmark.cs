using BenchmarkDotNet.Attributes;
using TagCompound_UnionImpl = NbtToolkit.Benchmark.OtherImpls.TagCompound;

namespace NbtToolkit.Benchmark.TagOperations;

[MemoryDiagnoser(false), ShortRunJob]
public class WriteIntTagsBenchmark
{
    const int N = 10000;

    private string[] keys = null!;
    private int[] values = null!;

    [GlobalSetup]
    public void Setup()
    {
        keys = new string[N];
        values = new int[N];
        Random random = new();
        for (int i = 0; i < N; i++)
        {
            keys[i] = Guid.NewGuid().ToString();
            values[i] = random.Next();
        }
    }

    [Benchmark(Baseline = true)]
    public void WriteIntTags()
    {
        var tag = new TagCompound();
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new TagInt(values[i]);
        }
    }

    [Benchmark]
    public void WriteIntTags_Union()
    {
        var tag = new TagCompound_UnionImpl();
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new OtherImpls.TagValue(values[i]);
        }
    }

    [Benchmark]
    public void WriteIntTags_fNbt()
    {
        var tag = new fNbt.NbtCompound();
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new fNbt.NbtInt(keys[i], values[i]);
        }
    }

    [Benchmark]
    public void WriteIntTags_Substrate()
    {
        var tag = new Substrate.Nbt.TagNodeCompound();
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new Substrate.Nbt.TagNodeInt(values[i]);
        }
    }

    [Benchmark]
    public void WriteIntTags_Dict()
    {
        var dict = new Dictionary<string, int>();
        for (int i = 0; i < N; i++)
        {
            dict[keys[i]] = values[i];
        }
    }
}
