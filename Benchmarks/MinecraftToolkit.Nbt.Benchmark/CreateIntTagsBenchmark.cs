using BenchmarkDotNet.Attributes;

namespace MinecraftToolkit.Nbt.Benchmark;

[MemoryDiagnoser]
public class CreateIntTagsBenchmark
{
    [Params(1000, 10000)]
    public int N { get; set; }

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
    public TagCompound CreateIntTags()
    {
        TagCompound tag = new();
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new TagValue();
        }
        return tag;
    }

    [Benchmark]
    public fNbt.NbtCompound CreateIntTags_fNbt()
    {
        fNbt.NbtCompound tag = new();
        for (int i = 0; i < N; i++)
        {
            fNbt.NbtInt t = new(keys[i], values[i]);
            tag[keys[i]] = t;
        }
        return tag;
    }

    [Benchmark]
    public Substrate.Nbt.TagNodeCompound CreateIntTags_Substrate()
    {
        Substrate.Nbt.TagNodeCompound tag = new();
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new Substrate.Nbt.TagNodeInt(values[i]);
        }
        return tag;
    }

    [Benchmark]
    public Dictionary<string, int> CreateIntTags_Reference_DictInt()
    {
        Dictionary<string, int > dict = new();
        for (int i = 0; i < N; i++)
        {
            dict[keys[i]] = values[i];
        }
        return dict;
    }

    [Benchmark]
    public Dictionary<string, TagValue> CreateIntTags_Reference_DictTagValue()
    {
        Dictionary<string, TagValue> dict = new();
        for (int i = 0; i < N; i++)
        {
            dict[keys[i]] = TagValue.CreateInt(values[i]);
        }
        return dict;
    }
}
