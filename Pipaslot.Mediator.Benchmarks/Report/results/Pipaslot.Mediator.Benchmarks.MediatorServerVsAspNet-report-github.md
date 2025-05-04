```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method     | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Middleware |  6.333 μs | 0.1251 μs | 0.1390 μs |  1.00 |    0.03 | 0.7324 |   9.57 KB |        1.00 |
| MinimalApi | 15.244 μs | 0.6759 μs | 1.9930 μs |  2.41 |    0.32 | 0.7324 |  10.17 KB |        1.06 |
| Controller | 21.559 μs | 0.4304 μs | 0.4957 μs |  3.41 |    0.11 | 0.9766 |  12.04 KB |        1.26 |
| Mediator   | 37.282 μs | 0.7176 μs | 0.6712 μs |  5.89 |    0.16 | 1.7090 |  23.16 KB |        2.42 |
