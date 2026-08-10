using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Pipaslot.Mediator.Analyzers;

/// <summary>
/// Flags a statement-level call to <c>IMediator.Dispatch</c>/<c>IMediator.Execute&lt;TResult&gt;</c> whose returned
/// <c>IMediatorResponse</c>/<c>IMediatorResponse&lt;TResult&gt;</c> is discarded - <c>mediator.Dispatch(x);</c> or
/// <c>await mediator.Dispatch(x);</c> as a bare <see cref="ExpressionStatementSyntax"/>, not assigned anywhere.
/// Neither method throws on failure, so discarding the result silently drops it. <c>DispatchUnhandled</c>/
/// <c>ExecuteUnhandled</c> are exempt - they throw on failure, so discarding their result is not the same bug.
/// This rule is intentionally statement-level only, not a full dataflow analysis of whether an assigned local is
/// later read: <c>var result = await mediator.Dispatch(x);</c> without ever reading <c>result.Success</c> is out
/// of scope. Deciding that reliably would require tracking every branch, early return, and possible storage of
/// the local into a field/collection/closure - a much higher false-positive surface than the statement-level
/// check below, for a bug class (silently dropping a failure) that the statement-level check already catches.
/// The idiomatic suppression <c>_ = await mediator.Dispatch(x);</c> is naturally not flagged - its expression is
/// an <see cref="AssignmentExpressionSyntax"/>, not an invocation/await, so it never matches.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IgnoredMediatorResponseAnalyzer : DiagnosticAnalyzer
{
    internal const string MediatorMetadataName = "Pipaslot.Mediator.IMediator";
    internal const string DispatchMethodName = "Dispatch";
    internal const string ExecuteMethodName = "Execute";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.IgnoredMediatorResponse);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var compilation = compilationContext.Compilation;
            var mediatorInterface = compilation.GetTypeByMetadataName(MediatorMetadataName);
            if (mediatorInterface is null)
            {
                // Project doesn't reference Pipaslot.Mediator - nothing to analyze.
                return;
            }

            // Dispatch has no overloads on IMediator; Execute<TResult> is the only member named "Execute" - a
            // plain name lookup is enough, no need to disambiguate by signature.
            var dispatchMethod = mediatorInterface.GetMembers(DispatchMethodName).OfType<IMethodSymbol>().SingleOrDefault();
            var executeMethod = mediatorInterface.GetMembers(ExecuteMethodName).OfType<IMethodSymbol>().SingleOrDefault();
            if (dispatchMethod is null || executeMethod is null)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeExpressionStatement(nodeContext, dispatchMethod, executeMethod),
                SyntaxKind.ExpressionStatement);
        });
    }

    private static void AnalyzeExpressionStatement(
        SyntaxNodeAnalysisContext context,
        IMethodSymbol dispatchMethod,
        IMethodSymbol executeMethod)
    {
        var statement = (ExpressionStatementSyntax)context.Node;
        var expression = statement.Expression;

        if (expression is AwaitExpressionSyntax awaitExpression)
        {
            expression = awaitExpression.Expression;
        }

        if (expression is not InvocationExpressionSyntax invocation)
        {
            // Not a bare call/await - e.g. `_ = await mediator.Dispatch(x);` is an AssignmentExpressionSyntax and
            // never reaches here, which is the intended, idiomatic way to suppress this rule.
            return;
        }

        var symbol = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
        if (symbol is null)
        {
            return;
        }

        string? methodName = null;
        if (IsMediatorMethod(symbol, dispatchMethod))
        {
            methodName = DispatchMethodName;
        }
        else if (IsMediatorMethod(symbol, executeMethod))
        {
            methodName = ExecuteMethodName;
        }

        if (methodName is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.IgnoredMediatorResponse,
            invocation.GetLocation(),
            methodName));
    }

    /// <summary>
    /// True when <paramref name="method"/> is <paramref name="mediatorMethod"/> itself (the common case - the
    /// call site is typed as <c>IMediator</c>) or its implementation on a concrete type that implements
    /// <c>IMediator</c> directly (a consumer calling through a concrete class reference instead of the interface).
    /// </summary>
    private static bool IsMediatorMethod(IMethodSymbol method, IMethodSymbol mediatorMethod)
    {
        if (SymbolEqualityComparer.Default.Equals(method.OriginalDefinition, mediatorMethod))
        {
            return true;
        }

        var containingType = method.ContainingType;
        if (containingType is null)
        {
            return false;
        }

        var implementation = containingType.FindImplementationForInterfaceMember(mediatorMethod);
        return implementation is not null
            && SymbolEqualityComparer.Default.Equals(implementation.OriginalDefinition, method.OriginalDefinition);
    }
}
