```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method      | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------ |----------:|----------:|----------:|-------:|----------:|
| PostMessage |  6.833 μs | 0.1243 μs | 0.1163 μs | 0.1984 |   2.53 KB |
| PostRequest |  6.811 μs | 0.0938 μs | 0.0877 μs | 0.1831 |   2.53 KB |
| GetMessage  |  9.018 μs | 0.1168 μs | 0.1093 μs | 0.6714 |   8.44 KB |
| GetRequest  | 16.860 μs | 0.2340 μs | 0.2188 μs | 1.2817 |  16.05 KB |
