```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method        | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| RawHttpClient | 33.21 μs | 0.664 μs | 0.621 μs |  1.00 |    0.03 | 0.5493 | 0.2441 |   6.88 KB |        1.00 |
| Mediator      | 78.19 μs | 1.396 μs | 1.306 μs |  2.36 |    0.06 | 1.4648 | 0.2441 |  17.98 KB |        2.61 |
