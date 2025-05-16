```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method      | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------ |----------:|----------:|----------:|-------:|----------:|
| PostMessage |  6.500 μs | 0.0886 μs | 0.0829 μs | 0.1831 |   2.53 KB |
| PostRequest |  6.556 μs | 0.0628 μs | 0.0587 μs | 0.1831 |   2.53 KB |
| GetMessage  |  8.360 μs | 0.0613 μs | 0.0573 μs | 0.6714 |   8.38 KB |
| GetRequest  | 15.736 μs | 0.2071 μs | 0.1938 μs | 1.2207 |  15.56 KB |
