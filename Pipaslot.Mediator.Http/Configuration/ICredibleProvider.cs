using System;

namespace Pipaslot.Mediator.Http.Configuration;

/// <summary>
/// Allowlist consulted before the HTTP transport deserializes a type coming from the wire. Throws
/// <see cref="MediatorHttpException"/> for a type that was not explicitly trusted.
/// </summary>
/// <remarks>
/// A security boundary, not a configuration nuisance to work around: the wire format names the concrete type to
/// instantiate, so accepting an arbitrary name lets a caller pick a type whose construction or deserialization has side
/// effects. The check is what keeps that choice limited to types the application registered on purpose. Two independent
/// instances exist - one for actions arriving at the server, one for results arriving at the client (see
/// <see cref="BaseMediatorOptions{TBuilder}.DeserializeOnlyCredibleResultTypes"/>).
/// <para>
/// Getting <see cref="MediatorHttpException"/> here means the type is missing from the allowlist, so widen the allowlist
/// rather than disabling the check: register actions with <c>AddActionsFromAssemblyOf&lt;T&gt;</c> on the configurator,
/// and result contracts with <see cref="BaseMediatorOptions{TBuilder}.AddCredibleResultType{T}"/> or
/// <see cref="BaseMediatorOptions{TBuilder}.AddCredibleResultAssemblyOf{T}"/>. Implement this interface only to source
/// the same decision from elsewhere - never to return unconditionally. Unrelated to which JSON shape is produced, that is
/// the contract serializer's job. See docs/wiki/8.-HTTP-transport-and-configuration-for-Client-Server-usage.md.
/// </para>
/// </remarks>
public interface ICredibleProvider
{
    /// <summary>
    /// Verifies that <paramref name="type"/> may be deserialized from the wire.
    /// </summary>
    /// <param name="type">Type named by the incoming payload</param>
    /// <exception cref="MediatorHttpException"><paramref name="type"/> is not among the trusted types or assemblies.</exception>
    void VerifyCredibility(Type type);
}
