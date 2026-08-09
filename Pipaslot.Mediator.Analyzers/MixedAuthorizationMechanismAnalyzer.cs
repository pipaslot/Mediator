using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Pipaslot.Mediator.Analyzers;

/// <summary>
/// Flags a handler + handled-action pair whose authorization is declared in more than one place: either split
/// across the pair - an active source on the action *and* an active source on the handler, regardless of whether
/// both sides use the same mechanism kind (e.g. a policy attribute on the action plus a different one on the
/// handler, or <c>IActionAuthorization</c> on the action plus <c>IHandlerAuthorization&lt;T&gt;</c> on the
/// handler) - or a kind mix on one class alone (an authorization interface together with a policy attribute on
/// the same action or the same handler). <c>PolicyResolver</c> ANDs every active source together regardless of
/// how many are declared, so none of these fail at runtime - it is a maintainability smell: the effective policy
/// can no longer be read from one place, and a future edit to one source can silently leave another stale. Every
/// active source living on a *single* class - whether one attribute, several attributes, or the interface alone -
/// is still readable from one place and is not flagged.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MixedAuthorizationMechanismAnalyzer : DiagnosticAnalyzer
{
    private const string ActionAuthorizationMetadataName = "Pipaslot.Mediator.Authorization.IActionAuthorization";
    private const string HandlerAuthorizationMarkerMetadataName = "Pipaslot.Mediator.Authorization.IHandlerAuthorizationMarker";
    private const string PolicyMetadataName = "Pipaslot.Mediator.Authorization.IPolicy";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.MixedAuthorizationMechanism);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var compilation = compilationContext.Compilation;
            var handlerInterface = compilation.GetTypeByMetadataName(HandlerActionTypes.HandlerMetadataName);
            var handlerWithResultInterface = compilation.GetTypeByMetadataName(HandlerActionTypes.HandlerWithResultMetadataName);
            var actionAuthorizationInterface = compilation.GetTypeByMetadataName(ActionAuthorizationMetadataName);
            var markerInterface = compilation.GetTypeByMetadataName(HandlerAuthorizationMarkerMetadataName);
            var policyInterface = compilation.GetTypeByMetadataName(PolicyMetadataName);
            if (handlerInterface is null || handlerWithResultInterface is null || actionAuthorizationInterface is null
                || markerInterface is null || policyInterface is null)
            {
                // Project doesn't reference Pipaslot.Mediator - nothing to analyze.
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeTypeDeclaration(
                    nodeContext, handlerInterface, handlerWithResultInterface, actionAuthorizationInterface, markerInterface, policyInterface),
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
        INamedTypeSymbol actionAuthorizationInterface,
        INamedTypeSymbol markerInterface,
        INamedTypeSymbol policyInterface)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var handlerType = context.SemanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken);
        if (handlerType is null)
        {
            return;
        }

        var handledActionTypes = HandlerActionTypes.GetHandledActionTypes(handlerType, handlerInterface, handlerWithResultInterface);
        if (handledActionTypes.Count == 0)
        {
            // Not a handler - out of scope for this rule.
            return;
        }

        var handlerHasInterface = IsHandlerAuthorizedByInterface(handlerType, markerInterface);
        var handlerSources = GetSources(handlerType, "handler", handlerHasInterface, policyInterface);
        var handlerHasAttribute = handlerSources.Any(s => !s.IsInterface);
        var handlerActive = handlerHasInterface || handlerHasAttribute;
        var handlerMixesKinds = handlerHasInterface && handlerHasAttribute;

        foreach (var actionType in handledActionTypes)
        {
            var actionHasInterface = IsActionAuthorizedByInterface(actionType, actionAuthorizationInterface);
            var actionSources = GetSources(actionType, "action", actionHasInterface, policyInterface);
            var actionHasAttribute = actionSources.Any(s => !s.IsInterface);
            var actionActive = actionHasInterface || actionHasAttribute;
            var actionMixesKinds = actionHasInterface && actionHasAttribute;

            // Fragmented across the pair - both sides declare something, regardless of kind - or a kind mix
            // sitting on one class alone. A single class declaring any number of same-kind sources is fine.
            var isSplitAcrossPair = actionActive && handlerActive;
            if (!isSplitAcrossPair && !actionMixesKinds && !handlerMixesKinds)
            {
                continue;
            }

            var description = string.Join(", ", actionSources.Concat(handlerSources).Select(s => s.Label));
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MixedAuthorizationMechanism,
                typeDeclaration.Identifier.GetLocation(),
                handlerType.Name,
                actionType.Name,
                description));
        }
    }

    private static bool IsActionAuthorizedByInterface(ITypeSymbol actionType, INamedTypeSymbol actionAuthorizationInterface)
        => actionType.AllInterfaces.Contains(actionAuthorizationInterface, SymbolEqualityComparer.Default);

    private static bool IsHandlerAuthorizedByInterface(INamedTypeSymbol handlerType, INamedTypeSymbol markerInterface)
        => handlerType.AllInterfaces.Contains(markerInterface, SymbolEqualityComparer.Default);

    private static List<(string Label, bool IsInterface)> GetSources(
        ITypeSymbol type, string entityLabel, bool hasAuthorizationInterface, INamedTypeSymbol policyInterface)
    {
        var sources = new List<(string, bool)>();
        if (hasAuthorizationInterface)
        {
            sources.Add(($"an authorization interface on {entityLabel} '{type.Name}'", true));
        }

        foreach (var attributeName in GetPolicyAttributeNames(type, policyInterface))
        {
            sources.Add(($"[{attributeName}] on {entityLabel} '{type.Name}'", false));
        }

        return sources;
    }

    /// <summary>
    /// Mirrors <c>PolicyResolver.GetPolicyAttributes</c>: attributes on the type itself - including inherited from
    /// a base class, since none of the shipped policy attributes opt out of <c>AttributeUsage.Inherited</c> - and
    /// attributes declared directly on any interface the type implements, since a policy attribute can be placed
    /// on a shared marker interface instead of on every implementer.
    /// </summary>
    private static IEnumerable<string> GetPolicyAttributeNames(ITypeSymbol type, INamedTypeSymbol policyInterface)
    {
        for (ITypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            foreach (var name in GetPolicyAttributeNamesDeclaredOn(current, policyInterface))
            {
                yield return name;
            }
        }

        foreach (var iface in type.AllInterfaces)
        {
            foreach (var name in GetPolicyAttributeNamesDeclaredOn(iface, policyInterface))
            {
                yield return name;
            }
        }
    }

    private static IEnumerable<string> GetPolicyAttributeNamesDeclaredOn(ITypeSymbol symbol, INamedTypeSymbol policyInterface)
    {
        foreach (var attributeData in symbol.GetAttributes())
        {
            var attributeClass = attributeData.AttributeClass;
            if (attributeClass is not null && attributeClass.AllInterfaces.Contains(policyInterface, SymbolEqualityComparer.Default))
            {
                yield return attributeClass.Name;
            }
        }
    }
}
