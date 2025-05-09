```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method     | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Middleware |  6.270 μs | 0.1246 μs | 0.1826 μs |  1.00 |    0.04 | 0.7324 |   9.57 KB |        1.00 |
| MinimalApi | 14.905 μs | 0.6393 μs | 1.8850 μs |  2.38 |    0.31 | 0.7324 |  10.19 KB |        1.06 |
| Controller | 22.074 μs | 0.2542 μs | 0.2254 μs |  3.52 |    0.11 | 0.9766 |  12.05 KB |        1.26 |
| Mediator   | 35.166 μs | 0.6931 μs | 0.6483 μs |  5.61 |    0.19 | 1.7090 |  23.02 KB |        2.41 |
