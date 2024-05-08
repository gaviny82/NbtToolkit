using BenchmarkDotNet.Attributes;

namespace MinecraftToolkit.Nbt.Benchmark;

[MemoryDiagnoser(false)]
public class CreateStringTagsBenchmark
{
    [Params(1000, 10000)]
    public int N { get; set; }

    private string[] keys = null!;
    private string[] values = null!;

    private TagCompound _tag = new();
    private fNbt.NbtCompound _fnbtTag = new();
    private Substrate.Nbt.TagNodeCompound _substrateTag = new();
    private Dictionary<string, string> _dictInt = new();


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
    public void CreateStringTags()
    {
        for (int i = 0; i < N; i++)
        {
            _tag[keys[i]] = TagValue.CreateString(values[i]);
        }
    }

    [Benchmark]
    public void CreateStringTags_fNbt()
    {
        for (int i = 0; i < N; i++)
        {
            _fnbtTag[keys[i]] = new fNbt.NbtString(keys[i], values[i]);
        }
    }

    [Benchmark]
    public void CreateStringTags_Substrate()
    {
        for (int i = 0; i < N; i++)
        {
            _substrateTag[keys[i]] = new Substrate.Nbt.TagNodeString(values[i]);
        }
    }

    [Benchmark]
    public void CreateStringTags_Reference_Dict()
    {
        for (int i = 0; i < N; i++)
        {
            _dictInt[keys[i]] = values[i];
        }
    }
}
