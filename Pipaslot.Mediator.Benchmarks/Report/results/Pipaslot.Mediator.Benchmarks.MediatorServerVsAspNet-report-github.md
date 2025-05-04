```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method     | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Middleware |  6.373 μs | 0.1239 μs | 0.1378 μs |  1.00 |    0.03 | 0.7324 |   9.57 KB |        1.00 |
| MinimalApi | 17.656 μs | 0.4968 μs | 1.4649 μs |  2.77 |    0.24 | 0.7324 |  10.22 KB |        1.07 |
| Controller | 21.833 μs | 0.4317 μs | 0.4972 μs |  3.43 |    0.10 | 0.9766 |  12.07 KB |        1.26 |
| Mediator   | 35.489 μs | 0.7021 μs | 0.8623 μs |  5.57 |    0.18 | 1.7090 |  23.37 KB |        2.44 |
