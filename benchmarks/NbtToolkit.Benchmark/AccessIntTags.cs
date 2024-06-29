using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NbtToolkit.Benchmark;

public class AccessIntTags
{
    [Params(1000, 10000)]
    public int N { get; set; }

    private string[] keys = null!;
    private int[] values = null!;

    private TagCompound tag = new();
    private fNbt.NbtCompound fnbtTag = new();
    private Substrate.Nbt.TagNodeCompound substrateTag = new();

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

        // Populate the tags
        for (int i = 0; i < N; i++)
        {
            tag[keys[i]] = new TagInt(values[i]);
            fNbt.NbtInt t = new(keys[i], values[i]);
            fnbtTag[keys[i]] = t;
            substrateTag[keys[i]] = new Substrate.Nbt.TagNodeInt(values[i]);
        }
    }

    [Benchmark(Baseline = true)]
    public int AccessIntTags_MCT()
    {
        int sum = 0;
        for (int i = 0; i < N; i++)
        {
            sum += tag[keys[i]].AsInt();
        }
        return sum;
    }

    [Benchmark]
    public int AccessIntTags_fNbt()
    {
        int sum = 0;
        for (int i = 0; i < N; i++)
        {
            sum += fnbtTag[keys[i]]?.IntValue ?? 0;
        }
        return sum;
    }

    [Benchmark]
    public int AccessIntTags_Substrate()
    {
        int sum = 0;
        for (int i = 0; i < N; i++)
        {
            sum += substrateTag[keys[i]]?.ToTagInt().Data ?? 0;
        }
        return sum;
    }


}
