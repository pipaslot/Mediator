```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method      | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------ |----------:|----------:|----------:|-------:|----------:|
| PostMessage |  7.008 μs | 0.0592 μs | 0.0554 μs | 0.2136 |   2.95 KB |
| PostRequest |  7.062 μs | 0.0771 μs | 0.0721 μs | 0.2136 |   2.95 KB |
| GetMessage  |  9.150 μs | 0.1398 μs | 0.1308 μs | 0.7019 |   8.74 KB |
| GetRequest  | 16.889 μs | 0.3014 μs | 0.2819 μs | 1.2817 |  16.28 KB |
