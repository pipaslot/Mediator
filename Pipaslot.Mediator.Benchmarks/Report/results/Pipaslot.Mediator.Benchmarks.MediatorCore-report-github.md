```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method                     | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------|-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Message                    |   138.3 ns |  1.34 ns |  1.26 ns |  1.00 |    0.01 | 0.0446 |     560 B |        1.00 |
| Request                    |   186.7 ns |  2.99 ns |  2.50 ns |  1.35 |    0.02 | 0.0606 |     760 B |        1.36 |
| MessageWithContextAccessor |   211.4 ns |  3.13 ns |  2.93 ns |  1.53 |    0.02 | 0.0751 |     944 B |        1.69 |
| RequestWithContextAccessor |   262.8 ns |  1.84 ns |  1.72 ns |  1.90 |    0.02 | 0.0911 |    1144 B |        2.04 |
| RequestWithAuthentication  | 3,440.3 ns | 43.99 ns | 39.00 ns | 24.88 |    0.35 | 0.4272 |    5513 B |        9.84 |
