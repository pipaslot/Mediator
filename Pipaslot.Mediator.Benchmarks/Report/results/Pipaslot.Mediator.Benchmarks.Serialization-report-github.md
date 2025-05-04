```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0     | Gen1     | Gen2     | Allocated | Alloc Ratio |
|------------------------- |---------:|---------:|---------:|------:|--------:|---------:|---------:|---------:|----------:|------------:|
| SystemTextJsonSerializer | 451.5 μs |  9.03 μs | 11.09 μs |  1.00 |    0.03 | 126.4648 | 126.4648 | 126.4648 | 440.06 KB |        1.00 |
| V3Serializer             | 850.9 μs | 15.70 μs | 14.68 μs |  1.89 |    0.06 | 132.8125 | 117.1875 | 111.3281 | 666.12 KB |        1.51 |
