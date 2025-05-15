```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method               | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
|--------------------- |----------:|---------:|---------:|------:|--------:|--------:|-------:|----------:|------------:|
| SingleAction         |  51.86 μs | 0.707 μs | 0.661 μs |  1.00 |    0.02 |  5.3711 | 1.2207 |  66.51 KB |        1.00 |
| Containing502Actions | 255.90 μs | 3.635 μs | 3.400 μs |  4.93 |    0.09 | 33.2031 | 1.9531 | 415.82 KB |        6.25 |
