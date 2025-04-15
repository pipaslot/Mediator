using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Threading;

namespace Pipaslot.Mediator.Tests;

internal static class MediatorContextFactory
{
    internal static MediatorContext Create(IServiceProvider services, IMediatorAction action)
    {
        var mediator = services.GetRequiredService<IMediator>();
        var ca = services.GetRequiredService<IMediatorContextAccessor>();
        return new MediatorContext(mediator, ca, services, action, CancellationToken.None, null, null);
    }
}