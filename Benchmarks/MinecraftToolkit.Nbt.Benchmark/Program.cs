using BenchmarkDotNet.Running;
using MinecraftToolkit.Nbt.Benchmark;

BenchmarkRunner.Run<AccessIntTagsBenchmark>();
BenchmarkRunner.Run<CreateIntTagsBenchmark>();
BenchmarkRunner.Run<CreateDoubleTagsBenchmark>();
BenchmarkRunner.Run<CreateStringTagsBenchmark>();
