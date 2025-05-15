```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method        | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| RawHttpClient | 37.12 μs | 0.631 μs | 0.591 μs |  1.00 |    0.02 | 0.4883 | 0.1221 |   6.88 KB |        1.00 |
| Mediator      | 90.85 μs | 1.375 μs | 1.286 μs |  2.45 |    0.05 | 1.4648 | 0.2441 |  19.27 KB |        2.80 |
