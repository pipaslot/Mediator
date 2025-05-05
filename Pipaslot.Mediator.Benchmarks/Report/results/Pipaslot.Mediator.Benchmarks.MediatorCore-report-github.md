```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method   | Mean     | Error   | StdDev  | Gen0   | Allocated |
|--------- |---------:|--------:|--------:|-------:|----------:|
| Execute  | 279.7 ns | 5.60 ns | 9.21 ns | 0.0873 |    1096 B |
| Dispatch | 214.3 ns | 4.20 ns | 4.12 ns | 0.0732 |     920 B |
