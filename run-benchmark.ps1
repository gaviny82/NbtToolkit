# Run benchmarks for MinecraftToolkit.Nbt in the CLI provided by BenchmarkDotNet
$benchmarkProjectPath = "./Benchmarks/MinecraftToolkit.Nbt.Benchmark"
$runtimeArgs = $args -join ' '
$cmd = "dotnet run --project $benchmarkProjectPath --configuration Release -- $runtimeArgs"
Invoke-Expression $cmd
