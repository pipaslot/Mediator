```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method   | Mean     | Error   | StdDev  | Gen0   | Allocated |
|--------- |---------:|--------:|--------:|-------:|----------:|
| Execute  | 469.4 ns | 3.22 ns | 3.02 ns | 0.1135 |    1424 B |
| Dispatch | 294.4 ns | 2.86 ns | 2.67 ns | 0.0777 |     976 B |
