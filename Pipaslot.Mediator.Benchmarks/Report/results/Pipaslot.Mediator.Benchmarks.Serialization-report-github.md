```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0     | Gen1     | Gen2     | Allocated | Alloc Ratio |
|------------------------- |---------:|--------:|--------:|------:|--------:|---------:|---------:|---------:|----------:|------------:|
| SystemTextJsonSerializer | 437.6 μs | 4.22 μs | 3.95 μs |  1.00 |    0.01 | 125.4883 | 125.4883 | 125.4883 | 440.06 KB |        1.00 |
| V3Serializer             | 551.1 μs | 7.07 μs | 6.61 μs |  1.26 |    0.02 |  60.5469 |  60.5469 |  60.5469 | 467.47 KB |        1.06 |
