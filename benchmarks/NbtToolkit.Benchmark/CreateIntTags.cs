using BenchmarkDotNet.Attributes;

namespace NbtToolkit.Benchmark;

[MemoryDiagnoser(false)]
public class CreateIntTags
{
    [Params(1000, 10000)]
    public int N { get; set; }

    private string[] keys = null!;
    private int[] values = null!;

    private TagCompound _tag = new();
    private fNbt.NbtCompound _fnbtTag = new();
    private Substrate.Nbt.TagNodeCompound _substrateTag = new();
    private Dictionary<string, int> _dictInt = new();


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
    public void CreateIntTags_MCT()
    {
        for (int i = 0; i < N; i++)
        {
            _tag[keys[i]] = new TagInt(values[i]);
        }
    }

    [Benchmark]
    public void CreateIntTags_fNbt()
    {
        for (int i = 0; i < N; i++)
        {
            _fnbtTag[keys[i]] = new fNbt.NbtInt(keys[i], values[i]);
        }
    }

    [Benchmark]
    public void CreateIntTags_Substrate()
    {
        for (int i = 0; i < N; i++)
        {
            _substrateTag[keys[i]] = new Substrate.Nbt.TagNodeInt(values[i]);
        }
    }

    [Benchmark]
    public void CreateIntTags_Dict()
    {
        for (int i = 0; i < N; i++)
        {
            _dictInt[keys[i]] = values[i];
        }
    }
}
