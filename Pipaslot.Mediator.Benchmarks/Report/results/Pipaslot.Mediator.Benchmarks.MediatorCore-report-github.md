```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method                    | Mean       | Error    | StdDev   | Gen0   | Allocated |
|-------------------------- |-----------:|---------:|---------:|-------:|----------:|
| Message                   |   213.4 ns |  1.45 ns |  1.28 ns | 0.0751 |     944 B |
| Request                   |   266.0 ns |  1.86 ns |  1.74 ns | 0.0911 |    1144 B |
| RequestWithAuthentication | 3,569.3 ns | 35.11 ns | 31.13 ns | 0.4578 |    5897 B |
