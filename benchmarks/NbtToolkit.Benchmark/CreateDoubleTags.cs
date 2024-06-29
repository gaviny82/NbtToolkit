using BenchmarkDotNet.Attributes;

namespace NbtToolkit.Benchmark;

[MemoryDiagnoser(false)]
public class CreateDoubleTags
{
    [Params(1000, 10000)]
    public int N { get; set; }

    private string[] keys = null!;
    private double[] values = null!;

    private TagCompound _tag = new();
    private fNbt.NbtCompound _fnbtTag = new();
    private Substrate.Nbt.TagNodeCompound _substrateTag = new();
    private Dictionary<string, double> _dictInt = new();


    [GlobalSetup]
    public void Setup()
    {
        keys = new string[N];
        values = new double[N];
        Random random = new();
        for (int i = 0; i < N; i++)
        {
            keys[i] = Guid.NewGuid().ToString();
            values[i] = random.NextDouble();
        }
    }

    [Benchmark(Baseline = true)]
    public void CreateDoubleTags_MCT()
    {
        for (int i = 0; i < N; i++)
        {
            _tag[keys[i]] = new TagDouble(values[i]);
        }
    }

    [Benchmark]
    public void CreateDoubleTags_fNbt()
    {
        for (int i = 0; i < N; i++)
        {
            _fnbtTag[keys[i]] = new fNbt.NbtDouble(keys[i], values[i]);
        }
    }

    [Benchmark]
    public void CreateDoubleTags_Substrate()
    {
        for (int i = 0; i < N; i++)
        {
            _substrateTag[keys[i]] = new Substrate.Nbt.TagNodeDouble(values[i]);
        }
    }

    [Benchmark]
    public void CreateDoubleTags_Reference_Dict()
    {
        for (int i = 0; i < N; i++)
        {
            _dictInt[keys[i]] = values[i];
        }
    }
}
