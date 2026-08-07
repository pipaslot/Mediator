using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Pipaslot.Mediator.Analyzers;

/// <summary>
/// Flags a <c>catch (Exception)</c> (or bare <c>catch</c>) block inside a class implementing
/// <c>IMediatorMiddleware</c> that wraps a call to the <c>MiddlewareDelegate next</c> continuation and does not
/// rethrow or record the exception via <c>MediatorContext.AddException</c> - the deprecated catch-all
/// <c>ErrorHandlingMiddleware</c> pattern replaced by <c>IMediatorExceptionHandler&lt;TException&gt;</c>. A
/// <c>catch (Exception)</c> guarding unrelated code in the same method (not the <c>next</c> call) is ordinary
/// error handling and out of scope for this rule. <c>AddException</c> is exempt because, unlike
/// <c>AddError(e.Message)</c>, it never puts exception detail into the client-facing <c>Results</c> and preserves
/// the exception's original type for <c>*Unhandled</c> callers - see
/// docs/wiki/6.2.-Exception-handling.md#fail-an-action-from-a-middleware-while-keeping-the-original-exception.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CatchAllMiddlewareAnalyzer : DiagnosticAnalyzer
{
    private const string MediatorMiddlewareMetadataName = "Pipaslot.Mediator.Middlewares.IMediatorMiddleware";
    private const string MiddlewareDelegateMetadataName = "Pipaslot.Mediator.Middlewares.MiddlewareDelegate";
    private const string MediatorContextMetadataName = "Pipaslot.Mediator.Middlewares.MediatorContext";
    private const string ExceptionMetadataName = "System.Exception";
    private const string AddExceptionMethodName = "AddException";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.CatchAllMiddleware);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var compilation = compilationContext.Compilation;
            var middlewareInterface = compilation.GetTypeByMetadataName(MediatorMiddlewareMetadataName);
            var middlewareDelegateType = compilation.GetTypeByMetadataName(MiddlewareDelegateMetadataName);
            var mediatorContextType = compilation.GetTypeByMetadataName(MediatorContextMetadataName);
            var exceptionType = compilation.GetTypeByMetadataName(ExceptionMetadataName);
            if (middlewareInterface is null || middlewareDelegateType is null || mediatorContextType is null || exceptionType is null)
            {
                // Project doesn't reference Pipaslot.Mediator (or a corelib without System.Exception) - nothing to analyze.
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeCatchClause(nodeContext, middlewareInterface, middlewareDelegateType, mediatorContextType, exceptionType),
                SyntaxKind.CatchClause);
        });
    }

    private static void AnalyzeCatchClause(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol middlewareInterface,
        INamedTypeSymbol middlewareDelegateType,
        INamedTypeSymbol mediatorContextType,
        INamedTypeSymbol exceptionType)
    {
        var catchClause = (CatchClauseSyntax)context.Node;

        if (catchClause.Declaration is not null)
        {
            // Only the broad `catch (Exception ...)` is the deprecated pattern - a narrower, specific
            // exception type is legitimate targeted handling and out of scope for this rule.
            var caughtType = context.SemanticModel.GetTypeInfo(catchClause.Declaration.Type, context.CancellationToken).Type;
            if (!SymbolEqualityComparer.Default.Equals(caughtType, exceptionType))
            {
                return;
            }
        }
        // A bare `catch` (no declaration) catches everything, which is at least as broad - treat it the same way.

        var typeDeclaration = catchClause.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeDeclaration is null)
        {
            return;
        }

        var containingType = context.SemanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken);
        if (containingType is null || !containingType.AllInterfaces.Contains(middlewareInterface, SymbolEqualityComparer.Default))
        {
            return;
        }

        if (catchClause.Parent is not TryStatementSyntax tryStatement
            || !CallsMiddlewareDelegate(tryStatement.Block, context.SemanticModel, middlewareDelegateType, context.CancellationToken))
        {
            // The catch doesn't guard the `next(context)` continuation, so it isn't hiding a pipeline exception
            // from the mediator boundary - it's ordinary error handling for unrelated code in the same method.
            return;
        }

        if (RethrowsException(catchClause))
        {
            return;
        }

        if (CallsAddException(catchClause, context.SemanticModel, mediatorContextType, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.CatchAllMiddleware,
            catchClause.CatchKeyword.GetLocation(),
            containingType.Name));
    }

    /// <summary>
    /// Matches both <c>next(context)</c> and <c>next.Invoke(context)</c>: either form resolves to the delegate's
    /// synthesized <c>Invoke</c> method, regardless of what the parameter/variable holding it is named.
    /// </summary>
    private static bool CallsMiddlewareDelegate(
        SyntaxNode block,
        SemanticModel semanticModel,
        INamedTypeSymbol middlewareDelegateType,
        CancellationToken cancellationToken)
    {
        foreach (var invocation in block.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var symbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol;
            if (symbol is IMethodSymbol { MethodKind: MethodKind.DelegateInvoke } method
                && SymbolEqualityComparer.Default.Equals(method.ContainingType, middlewareDelegateType))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Recognizes <c>context.AddException(e)</c> regardless of what the <c>MediatorContext</c>-typed
    /// variable/parameter is named - it records the exception server-side (readable by <c>*Unhandled</c> callers
    /// with its original type) without ever putting it into the client-facing <c>Results</c>, so it does not have
    /// the message-leak problem this rule otherwise targets.
    /// </summary>
    private static bool CallsAddException(
        CatchClauseSyntax catchClause,
        SemanticModel semanticModel,
        INamedTypeSymbol mediatorContextType,
        CancellationToken cancellationToken)
    {
        foreach (var invocation in catchClause.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var symbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol;
            if (symbol is IMethodSymbol { Name: AddExceptionMethodName } method
                && SymbolEqualityComparer.Default.Equals(method.ContainingType, mediatorContextType))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Conservative on purpose: any `throw` reachable in the catch clause suppresses the diagnostic, even if it
    /// sits inside a nested lambda/local function and would not actually propagate the caught exception. A false
    /// negative here is cheaper than a false positive on a legitimate rethrow.
    /// </summary>
    private static bool RethrowsException(CatchClauseSyntax catchClause)
    {
        return catchClause.DescendantNodes().OfType<ThrowStatementSyntax>().Any()
            || catchClause.DescendantNodes().OfType<ThrowExpressionSyntax>().Any();
    }
}
