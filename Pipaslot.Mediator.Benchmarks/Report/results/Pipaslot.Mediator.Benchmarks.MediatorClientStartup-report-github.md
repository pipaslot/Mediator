```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method                 | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------- |------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| MediatorWithNoAction   |    711.5 ns |  11.47 ns |  10.73 ns |  1.00 |    0.02 | 0.3109 | 0.0029 |   3.82 KB |        1.00 |
| MediatorWith502Actions | 31,836.6 ns | 633.33 ns | 777.79 ns | 44.75 |    1.25 | 5.3101 | 0.1221 |  65.51 KB |       17.15 |
