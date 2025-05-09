```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method               | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
|--------------------- |----------:|---------:|---------:|------:|--------:|--------:|-------:|----------:|------------:|
| SingleAction         |  11.05 μs | 0.081 μs | 0.076 μs |  1.00 |    0.01 |  1.5259 | 0.3662 |   18.8 KB |        1.00 |
| Containing502Actions | 145.42 μs | 1.664 μs | 1.556 μs | 13.16 |    0.16 | 21.4844 | 3.9063 | 265.43 KB |       14.12 |
