using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Pipaslot.Mediator.Analyzers;

/// <summary>
/// Flags a handler class whose <c>IHandlerAuthorization&lt;TAction&gt;</c>/<c>IHandlerAuthorizationAsync&lt;TAction&gt;</c>
/// type parameter does not match, and is not a base type of, any action the handler actually handles via a closed
/// <c>IMediatorHandler&lt;TAction&gt;</c>/<c>IMediatorHandler&lt;TAction,TResult&gt;</c>. <c>PolicyResolver</c> pairs the
/// two purely by reflection at dispatch time - a mismatch is invisible at build and startup time and only surfaces as an
/// <c>ArgumentException</c> from <c>MethodBase.Invoke</c> when the unmatched action is actually dispatched. Authorizing a
/// shared base type of the handler's actions (including <c>IMediatorAction</c> itself) is a supported pattern and is not
/// flagged - see docs/wiki/7.-Authorization.md#keeping-the-authorized-action-in-sync-with-the-handled-action-pipmed003. A
/// handler that implements no handler interface at all, or no authorization interface at all, is out of scope for this
/// rule (the latter is PIPMED004's territory).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HandlerAuthorizationTypeMismatchAnalyzer : DiagnosticAnalyzer
{
    private const string HandlerAuthorizationMetadataName = "Pipaslot.Mediator.Authorization.IHandlerAuthorization`1";
    private const string HandlerAuthorizationAsyncMetadataName = "Pipaslot.Mediator.Authorization.IHandlerAuthorizationAsync`1";
    private const string HandlerAuthorizationMarkerMetadataName = "Pipaslot.Mediator.Authorization.IHandlerAuthorizationMarker";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.HandlerAuthorizationTypeMismatch);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var compilation = compilationContext.Compilation;
            var handlerInterface = compilation.GetTypeByMetadataName(HandlerActionTypes.HandlerMetadataName);
            var handlerWithResultInterface = compilation.GetTypeByMetadataName(HandlerActionTypes.HandlerWithResultMetadataName);
            var authorizationInterface = compilation.GetTypeByMetadataName(HandlerAuthorizationMetadataName);
            var authorizationAsyncInterface = compilation.GetTypeByMetadataName(HandlerAuthorizationAsyncMetadataName);
            var markerInterface = compilation.GetTypeByMetadataName(HandlerAuthorizationMarkerMetadataName);
            if (handlerInterface is null || handlerWithResultInterface is null || authorizationInterface is null
                || authorizationAsyncInterface is null || markerInterface is null)
            {
                // Project doesn't reference Pipaslot.Mediator - nothing to analyze.
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeTypeDeclaration(
                    nodeContext, handlerInterface, handlerWithResultInterface, authorizationInterface, authorizationAsyncInterface, markerInterface),
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
        INamedTypeSymbol authorizationInterface,
        INamedTypeSymbol authorizationAsyncInterface,
        INamedTypeSymbol markerInterface)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var containingType = context.SemanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken);
        if (containingType is null || !containingType.AllInterfaces.Contains(markerInterface, SymbolEqualityComparer.Default))
        {
            // No IHandlerAuthorization<T>/IHandlerAuthorizationAsync<T> at all - not this rule's territory (PIPMED004).
            return;
        }

        var handledActionTypes = HandlerActionTypes.GetHandledActionTypes(containingType, handlerInterface, handlerWithResultInterface);
        if (handledActionTypes.Count == 0)
        {
            // Implements an authorization interface but no handler interface at all - dead authorization, a different
            // diagnostic than a type mismatch, and not covered by this rule.
            return;
        }

        var authorizedTypes = GetAuthorizedTypes(containingType, authorizationInterface, authorizationAsyncInterface);
        var compilation = context.SemanticModel.Compilation;
        var authorizedTypeNames = string.Join(", ", authorizedTypes.Select(t => t.Name));

        foreach (var actionType in handledActionTypes)
        {
            if (authorizedTypes.Any(authorizedType => IsAssignableTo(compilation, actionType, authorizedType)))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.HandlerAuthorizationTypeMismatch,
                typeDeclaration.Identifier.GetLocation(),
                containingType.Name,
                actionType.Name,
                authorizedTypeNames));
        }
    }

    /// <summary>
    /// Collects the type argument of every closed <c>IHandlerAuthorization&lt;T&gt;</c>/<c>IHandlerAuthorizationAsync&lt;T&gt;</c>
    /// on the type - a handler may combine several, e.g. one action authorized synchronously and another asynchronously.
    /// </summary>
    private static ImmutableHashSet<ITypeSymbol> GetAuthorizedTypes(
        INamedTypeSymbol handlerType,
        INamedTypeSymbol authorizationInterface,
        INamedTypeSymbol authorizationAsyncInterface)
    {
        var builder = ImmutableHashSet.CreateBuilder<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var iface in handlerType.AllInterfaces)
        {
            if (!iface.IsGenericType)
            {
                continue;
            }

            var definition = iface.ConstructedFrom;
            if (SymbolEqualityComparer.Default.Equals(definition, authorizationInterface)
                || SymbolEqualityComparer.Default.Equals(definition, authorizationAsyncInterface))
            {
                builder.Add(iface.TypeArguments[0]);
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// True when <paramref name="actionType"/> is <paramref name="authorizedType"/> or derives from/implements it -
    /// the covariant case where a handler authorizes a shared base type (or <c>IMediatorAction</c>) of its actions.
    /// <c>PolicyResolver</c> invokes <c>Authorize</c>/<c>AuthorizeAsync</c> reflectively with the dispatched action
    /// instance, so this mirrors <see cref="System.Reflection.MethodBase.Invoke"/>'s own assignability check rather
    /// than requiring an exact type match.
    /// </summary>
    private static bool IsAssignableTo(Compilation compilation, ITypeSymbol actionType, ITypeSymbol authorizedType)
    {
        var conversion = compilation.ClassifyConversion(actionType, authorizedType);
        return conversion.Exists && (conversion.IsIdentity || conversion.IsReference || conversion.IsBoxing);
    }
}
