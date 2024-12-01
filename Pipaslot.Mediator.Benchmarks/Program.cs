using BenchmarkDotNet.Running;
using Pipaslot.Mediator.Benchmarks;

// BenchmarkRunner.Run(typeof(Program).Assembly);
BenchmarkRunner.Run<DefaultNodeFormatterBenchmark>();