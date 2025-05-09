```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method                    | Mean       | Error    | StdDev    | Gen0   | Allocated |
|-------------------------- |-----------:|---------:|----------:|-------:|----------:|
| Message                   |   216.2 ns |  1.96 ns |   1.73 ns | 0.0725 |     912 B |
| Request                   |   287.5 ns |  5.03 ns |   4.70 ns | 0.0882 |    1112 B |
| RequestWithAuthentication | 4,081.6 ns | 80.48 ns | 104.65 ns | 0.4883 |    6225 B |
