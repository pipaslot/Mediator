using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Pipaslot.Mediator.Middlewares;

namespace Demo.Client.Services;

/// <summary>
/// Cancel mediator action when navigation was changed
/// </summary>
public class CancellationOnNavigationMediatorMiddleware : IMediatorMiddleware, IDisposable
{
    private IDisposable? registration;
    private CancellationTokenSource _cancellationTokenSource = new();

    public CancellationOnNavigationMediatorMiddleware(NavigationManager navigation)
    {
        registration = navigation.RegisterLocationChangingHandler(OnLocationChanging);
    }

    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        if (context.CancellationToken == CancellationToken.None)
        {
            Console.WriteLine("Cancellation token was overriden");
            context.SetCancellationToken(_cancellationTokenSource.Token);
        }

        await next(context);
    }


    private ValueTask OnLocationChanging(LocationChangingContext context)
    {
        Console.WriteLine("Changing location");
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        registration?.Dispose();
    }
}