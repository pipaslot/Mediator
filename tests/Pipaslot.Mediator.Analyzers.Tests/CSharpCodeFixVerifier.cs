using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Pipaslot.Mediator.Analyzers.Tests;

internal static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(diagnosticId);

    public static async Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource, int codeActionIndex = 0)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                AdditionalReferences = { typeof(Middlewares.IMediatorMiddleware).Assembly },
            },
            FixedState =
            {
                Sources = { fixedSource },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                AdditionalReferences = { typeof(Middlewares.IMediatorMiddleware).Assembly },
            },
            CodeActionIndex = codeActionIndex,
        };
        test.ExpectedDiagnostics.Add(expected);

        await test.RunAsync(CancellationToken.None);
    }
}
