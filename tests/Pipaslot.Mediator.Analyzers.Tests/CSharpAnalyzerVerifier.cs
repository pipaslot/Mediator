using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Pipaslot.Mediator.Analyzers.Tests;

internal static class CSharpAnalyzerVerifier<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

    public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        => await VerifyAnalyzerAsync([source], expected);

    public static async Task VerifyAnalyzerAsync(string[] sources, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestState =
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                AdditionalReferences = { typeof(Middlewares.IMediatorMiddleware).Assembly },
            },
        };
        foreach (var source in sources)
        {
            test.TestState.Sources.Add(source);
        }
        test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync(CancellationToken.None);
    }
}