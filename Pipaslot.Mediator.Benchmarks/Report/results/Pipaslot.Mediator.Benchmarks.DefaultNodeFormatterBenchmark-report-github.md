```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method          | Mean      | Error     | StdDev    | Gen0    | Gen1   | Allocated |
|---------------- |----------:|----------:|----------:|--------:|-------:|----------:|
| Reduce          |  2.562 μs | 0.0480 μs | 0.1023 μs |  0.8965 | 0.0076 |  11.02 KB |
| ReduceAndFormat | 50.496 μs | 0.3576 μs | 0.3345 μs | 19.5923 | 0.3052 | 240.62 KB |
