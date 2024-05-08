using BenchmarkDotNet.Attributes;

namespace MinecraftToolkit.Nbt.Benchmark;

[MemoryDiagnoser]
public class TagCreationBenchmark
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
    public TagCompound TagCreation_ValueTags()
    {
        TagCompound tag = new();
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new TagValue();
        }
        return tag;
    }

    [Benchmark]
    public fNbt.NbtCompound TagCreation_ValueTags_fNbt()
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
    public Substrate.Nbt.TagNodeCompound TagCreation_ValueTags_Substrate()
    {
        Substrate.Nbt.TagNodeCompound tag = new();
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new Substrate.Nbt.TagNodeInt(values[i]);
        }
        return tag;
    }
}
