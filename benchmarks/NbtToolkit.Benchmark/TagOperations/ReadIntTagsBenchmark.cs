﻿using BenchmarkDotNet.Attributes;

namespace NbtToolkit.Benchmark.TagOperations;

public class ReadIntTagsBenchmark
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
    public int ReadIntTags()
    {
        int sum = 0;
        for (int i = 0; i < N; i++)
        {
            sum += tag[keys[i]].AsInt();
        }
        return sum;
    }

    [Benchmark]
    public int ReadIntTags_fNbt()
    {
        int sum = 0;
        for (int i = 0; i < N; i++)
        {
            sum += fnbtTag[keys[i]]?.IntValue ?? 0;
        }
        return sum;
    }

    [Benchmark]
    public int ReadIntTags_Substrate()
    {
        int sum = 0;
        for (int i = 0; i < N; i++)
        {
            sum += substrateTag[keys[i]]?.ToTagInt().Data ?? 0;
        }
        return sum;
    }
}
