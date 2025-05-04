using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

var config = new ManualConfig { ArtifactsPath = "../../../Report" }
    .AddExporter(MarkdownExporter.GitHub)
    .AddLogger(ConsoleLogger.Default)
    .AddColumnProvider(DefaultColumnProviders.Instance)
    .AddValidator(DefaultConfig.Instance.GetValidators().ToArray());

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);