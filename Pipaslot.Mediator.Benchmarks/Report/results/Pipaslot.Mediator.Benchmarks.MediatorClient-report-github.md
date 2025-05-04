```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method        | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| RawHttpClient | 32.24 μs | 0.523 μs | 0.489 μs |  1.00 |    0.02 | 0.5493 | 0.2441 |   6.88 KB |        1.00 |
| Mediator      | 88.62 μs | 1.186 μs | 0.990 μs |  2.75 |    0.05 | 1.7090 | 0.4883 |  20.96 KB |        3.04 |
