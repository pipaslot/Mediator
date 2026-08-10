using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Pipaslot.Mediator.Analyzers;

/// <summary>
/// Shared by PIPMED003 and PIPMED004: computes the set of action types a handler class handles by walking its
/// closed <c>IMediatorHandler&lt;TAction&gt;</c>/<c>IMediatorHandler&lt;TAction,TResult&gt;</c> interfaces in
/// <see cref="INamedTypeSymbol.AllInterfaces"/>. Naming layers above the root (<c>IMessageHandler&lt;TMessage&gt;</c>,
/// <c>IRequestHandler&lt;TRequest,TResponse&gt;</c>, or a consumer's own) all flow through the same closed root
/// interfaces, so this only needs to look for the two.
/// </summary>
internal static class HandlerActionTypes
{
    public const string HandlerMetadataName = "Pipaslot.Mediator.Abstractions.IMediatorHandler`1";
    public const string HandlerWithResultMetadataName = "Pipaslot.Mediator.Abstractions.IMediatorHandler`2";

    public static ImmutableHashSet<ITypeSymbol> GetHandledActionTypes(
        INamedTypeSymbol handlerType,
        INamedTypeSymbol handlerInterface,
        INamedTypeSymbol handlerWithResultInterface)
    {
        var builder = ImmutableHashSet.CreateBuilder<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var iface in handlerType.AllInterfaces)
        {
            if (!iface.IsGenericType)
            {
                continue;
            }

            var definition = iface.ConstructedFrom;
            if (SymbolEqualityComparer.Default.Equals(definition, handlerInterface)
                || SymbolEqualityComparer.Default.Equals(definition, handlerWithResultInterface))
            {
                builder.Add(iface.TypeArguments[0]);
            }
        }

        return builder.ToImmutable();
    }
}
