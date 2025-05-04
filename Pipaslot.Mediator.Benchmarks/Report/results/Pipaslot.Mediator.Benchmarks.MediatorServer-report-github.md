```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method  | Mean     | Error     | StdDev    | Gen0   | Allocated |
|-------- |---------:|----------:|----------:|-------:|----------:|
| Message | 6.627 μs | 0.0504 μs | 0.0471 μs | 0.1984 |   2.53 KB |
| Request | 6.561 μs | 0.0412 μs | 0.0386 μs | 0.1984 |   2.53 KB |
