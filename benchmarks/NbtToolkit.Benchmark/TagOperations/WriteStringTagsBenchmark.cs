using BenchmarkDotNet.Attributes;

namespace NbtToolkit.Benchmark.TagOperations;

[MemoryDiagnoser(false)]
public class WriteStringTagsBenchmark
{
    const int N = 10000;

    private string[] keys = null!;
    private string[] values = null!;

    [GlobalSetup]
    public void Setup()
    {
        keys = new string[N];
        values = new string[N];
        Random random = new();
        for (int i = 0; i < N; i++)
        {
            keys[i] = Guid.NewGuid().ToString();
            values[i] = random.Next().ToString();
        }
    }

    [Benchmark(Baseline = true)]
    public void WriteStringTags()
    {
        var tag = new TagCompound();
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new TagString(values[i]);
        }
    }

    [Benchmark]
    public void WriteStringTags_fNbt()
    {
        var tag = new fNbt.NbtCompound();
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new fNbt.NbtString(keys[i], values[i]);
        }
    }

    [Benchmark]
    public void WriteStringTags_Substrate()
    {
        var tag = new Substrate.Nbt.TagNodeCompound();
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new Substrate.Nbt.TagNodeString(values[i]);
        }
    }

    [Benchmark]
    public void WriteStringTags_Dict()
    {
        var dict = new Dictionary<string, string>();
        for (int i = 0; i < N; i++)
        {
            dict[keys[i]] = values[i];
        }
    }
}
