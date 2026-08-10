using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Pipaslot.Mediator.Analyzers;

/// <summary>
/// Flags a handler class that declares a public method matching the shape of
/// <c>IHandlerAuthorization&lt;TAction&gt;.Authorize</c>/<c>IHandlerAuthorizationAsync&lt;TAction&gt;.AuthorizeAsync</c>
/// - name, parameter type (one of the handler's own handled action types, or a base of one), and return type - but
/// does not actually implement either interface. <c>PolicyResolver</c> only ever invokes these methods reflectively
/// after checking <c>handler is IHandlerAuthorizationMarker</c>, so a method matching the shape but missing from the
/// base list is silently never called - the handler runs unauthorized. A handler that already implements an
/// authorization interface is PIPMED003's territory (a type-parameter mismatch), not this rule's - the two are
/// mutually exclusive by construction. A method with any reference from elsewhere inside the type (including
/// another partial declaration of the same type) is treated as a deliberate, manually-invoked helper rather than an
/// orphaned interface implementation, and is not flagged.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OrphanedAuthorizeMethodAnalyzer : DiagnosticAnalyzer
{
    private const string HandlerAuthorizationMarkerMetadataName = "Pipaslot.Mediator.Authorization.IHandlerAuthorizationMarker";
    private const string PolicyMetadataName = "Pipaslot.Mediator.Authorization.IPolicy";
    private const string CancellationTokenMetadataName = "System.Threading.CancellationToken";
    private const string TaskOfTMetadataName = "System.Threading.Tasks.Task`1";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.OrphanedAuthorizeMethod);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var compilation = compilationContext.Compilation;
            var handlerInterface = compilation.GetTypeByMetadataName(HandlerActionTypes.HandlerMetadataName);
            var handlerWithResultInterface = compilation.GetTypeByMetadataName(HandlerActionTypes.HandlerWithResultMetadataName);
            var markerInterface = compilation.GetTypeByMetadataName(HandlerAuthorizationMarkerMetadataName);
            var policyInterface = compilation.GetTypeByMetadataName(PolicyMetadataName);
            var cancellationTokenType = compilation.GetTypeByMetadataName(CancellationTokenMetadataName);
            var taskOfTType = compilation.GetTypeByMetadataName(TaskOfTMetadataName);
            if (handlerInterface is null || handlerWithResultInterface is null || markerInterface is null
                || policyInterface is null || cancellationTokenType is null || taskOfTType is null)
            {
                // Project doesn't reference Pipaslot.Mediator - nothing to analyze.
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeTypeDeclaration(
                    nodeContext, handlerInterface, handlerWithResultInterface, markerInterface, policyInterface, cancellationTokenType, taskOfTType),
                SyntaxKind.ClassDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.RecordStructDeclaration);
        });
    }

    private static void AnalyzeTypeDeclaration(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol handlerInterface,
        INamedTypeSymbol handlerWithResultInterface,
        INamedTypeSymbol markerInterface,
        INamedTypeSymbol policyInterface,
        INamedTypeSymbol cancellationTokenType,
        INamedTypeSymbol taskOfTType)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var containingType = context.SemanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken);
        if (containingType is null || containingType.AllInterfaces.Contains(markerInterface, SymbolEqualityComparer.Default))
        {
            // Already implements an authorization interface - PIPMED003's territory, not this rule's.
            return;
        }

        var handledActionTypes = HandlerActionTypes.GetHandledActionTypes(containingType, handlerInterface, handlerWithResultInterface);
        if (handledActionTypes.Count == 0)
        {
            // Not a handler - out of scope for this rule.
            return;
        }

        if (!IsFirstDeclaringReference(containingType, typeDeclaration))
        {
            // A partial type triggers this analysis once per declared part, all resolving to the same symbol -
            // process it from one part only so a member found via the shared symbol isn't reported once per part.
            return;
        }

        var compilation = context.SemanticModel.Compilation;

        foreach (var member in containingType.GetMembers())
        {
            if (member is not IMethodSymbol { MethodKind: MethodKind.Ordinary, DeclaredAccessibility: Accessibility.Public } method)
            {
                continue;
            }

            var actionParameterType = GetOrphanedActionParameterType(method, policyInterface, cancellationTokenType, taskOfTType);
            if (actionParameterType is null)
            {
                continue;
            }

            if (!handledActionTypes.Any(handled => IsAssignableTo(compilation, handled, actionParameterType)))
            {
                // Parameter isn't one of (or a base of) any action this handler actually handles - e.g. an
                // Authorize(SomethingUnrelated) helper method that has nothing to do with this handler's actions.
                continue;
            }

            if (HasInternalReference(context.SemanticModel, containingType, method, context.CancellationToken))
            {
                continue;
            }

            var location = method.Locations.FirstOrDefault(l => l.IsInSource);
            if (location is null)
            {
                continue;
            }

            var interfaceName = method.Name == "AuthorizeAsync" ? "IHandlerAuthorizationAsync" : "IHandlerAuthorization";
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.OrphanedAuthorizeMethod,
                location,
                containingType.Name,
                method.Name,
                actionParameterType.Name,
                interfaceName));
        }
    }

    /// <summary>
    /// Returns the action parameter type when <paramref name="method"/>'s name, parameter list, and return type
    /// exactly match the shape of <c>IHandlerAuthorization&lt;TAction&gt;.Authorize</c> or
    /// <c>IHandlerAuthorizationAsync&lt;TAction&gt;.AuthorizeAsync</c> - <c>null</c> otherwise. The match is
    /// intentionally exact (return type must be <c>IPolicy</c>/<c>Task&lt;IPolicy&gt;</c> itself, not merely
    /// assignable to it) since that is what actually implementing either interface would require.
    /// </summary>
    private static ITypeSymbol? GetOrphanedActionParameterType(
        IMethodSymbol method, INamedTypeSymbol policyInterface, INamedTypeSymbol cancellationTokenType, INamedTypeSymbol taskOfTType)
    {
        if (method.Name == "Authorize"
            && method.Parameters.Length == 1
            && SymbolEqualityComparer.Default.Equals(method.ReturnType, policyInterface))
        {
            return method.Parameters[0].Type;
        }

        if (method.Name == "AuthorizeAsync"
            && method.Parameters.Length == 2
            && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, cancellationTokenType)
            && method.ReturnType is INamedTypeSymbol { IsGenericType: true } returnType
            && SymbolEqualityComparer.Default.Equals(returnType.ConstructedFrom, taskOfTType)
            && SymbolEqualityComparer.Default.Equals(returnType.TypeArguments[0], policyInterface))
        {
            return method.Parameters[0].Type;
        }

        return null;
    }

    private static bool IsFirstDeclaringReference(INamedTypeSymbol containingType, TypeDeclarationSyntax typeDeclaration)
    {
        var first = containingType.DeclaringSyntaxReferences.FirstOrDefault();
        return first is not null && first.SyntaxTree == typeDeclaration.SyntaxTree && first.Span == typeDeclaration.Span;
    }

    /// <summary>
    /// True when <paramref name="method"/> is invoked or referenced anywhere inside any declared part of
    /// <paramref name="containingType"/> - a legitimate manually-invoked helper rather than an orphaned interface
    /// implementation that PolicyResolver would otherwise never reach. Walks every partial declaration separately
    /// (each may live in a different syntax tree, hence its own <see cref="SemanticModel"/>) rather than only the
    /// declaration syntax that triggered this analysis.
    /// </summary>
    private static bool HasInternalReference(
        SemanticModel currentTreeSemanticModel, INamedTypeSymbol containingType, IMethodSymbol method, CancellationToken cancellationToken)
    {
        foreach (var syntaxReference in containingType.DeclaringSyntaxReferences)
        {
            var typeSyntax = syntaxReference.GetSyntax(cancellationToken);
            var semanticModel = GetSemanticModel(currentTreeSemanticModel, typeSyntax.SyntaxTree);

            foreach (var node in typeSyntax.DescendantNodes())
            {
                if (node is not (InvocationExpressionSyntax or IdentifierNameSyntax))
                {
                    continue;
                }

                var symbol = semanticModel.GetSymbolInfo(node, cancellationToken).Symbol;
                if (symbol is IMethodSymbol candidate && SymbolEqualityComparer.Default.Equals(candidate, method))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Reuses <paramref name="currentTreeSemanticModel"/> when it already covers <paramref name="syntaxTree"/> -
    /// true for a non-partial type, or for whichever partial part triggered this analysis - and only falls back to
    /// <see cref="Compilation.GetSemanticModel(SyntaxTree, bool)"/> for a different tree, which happens only when a
    /// handler is declared across multiple files. RS1030 warns against that fallback in general (it bypasses the
    /// host's semantic model cache), but there is no other way to inspect a partial declaration living in a syntax
    /// tree other than the one this analyzer instance was invoked for.
    /// </summary>
    private static SemanticModel GetSemanticModel(SemanticModel currentTreeSemanticModel, SyntaxTree syntaxTree)
    {
        if (currentTreeSemanticModel.SyntaxTree == syntaxTree)
        {
            return currentTreeSemanticModel;
        }

#pragma warning disable RS1030
        return currentTreeSemanticModel.Compilation.GetSemanticModel(syntaxTree);
#pragma warning restore RS1030
    }

    private static bool IsAssignableTo(Compilation compilation, ITypeSymbol actionType, ITypeSymbol authorizedType)
    {
        var conversion = compilation.ClassifyConversion(actionType, authorizedType);
        return conversion.Exists && (conversion.IsIdentity || conversion.IsReference || conversion.IsBoxing);
    }
}
