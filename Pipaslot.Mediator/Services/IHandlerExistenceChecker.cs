using System;

namespace Pipaslot.Mediator.Services;

/// <summary>
/// Startup or test-time check that the registered actions have a usable set of handlers and authorization.
/// </summary>
/// <remarks>
/// Resolve it from the service provider. It only inspects actions registered through
/// <see cref="Configuration.IMediatorConfigurator.AddActions"/> or one of its assembly-scanning variants, so an action
/// that was never registered is not checked. Run it from an automated test or, in development, at startup; the
/// server integration runs it from <c>UseMediator(checkMatchingHandlers: true)</c>.
/// </remarks>
public interface IHandlerExistenceChecker
{
    /// <summary>
    /// Verifies every registered action against the checks enabled in <paramref name="setting"/>.
    /// </summary>
    /// <param name="setting">Selects the checks to run; when none is enabled, nothing is verified.</param>
    /// <exception cref="MediatorException">
    /// At least one problem was found. All problems are reported together, one entry per problem in <see cref="Exception.Data"/>.
    /// </exception>
    void Verify(ExistenceCheckerSetting setting);
}