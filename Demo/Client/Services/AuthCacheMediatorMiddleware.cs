using Microsoft.AspNetCore.Components.Authorization;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Middlewares;
using System.Collections.Concurrent;

namespace Demo.Client.Services
{
    public class AuthCacheMediatorMiddleware : IMediatorMiddleware, IDisposable
    {
        private readonly ConcurrentDictionary<string, IsAuthorizedRequestResponse> _cache = new ConcurrentDictionary<string, IsAuthorizedRequestResponse>();
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        public AuthCacheMediatorMiddleware(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _authenticationStateProvider.AuthenticationStateChanged += ClearCache;
        }

        public void Dispose()
        {
            _authenticationStateProvider.AuthenticationStateChanged -= ClearCache;
        }

        public async void ClearCache(Task<AuthenticationState> authState)
        {
            _cache.Clear();
        }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            if (context.Action is IsAuthorizedRequest authRequest && authRequest.Action != null)
            {
                var actionName = authRequest.Action.GetType().FullName ?? throw new Exception("Can not get action name");
                if (_cache.TryGetValue(actionName, out var cached))
                {
                    context.AddResult(cached);
                    context.Status = ExecutionStatus.Succeeded;
                    Console.WriteLine($"Authentication state for action '{actionName}' was loaded from cache.");
                }
                else
                {
                    await next(context);
                    var toBeCached = context.Results
                        .Where(r => r is IsAuthorizedRequestResponse)
                        .Cast<IsAuthorizedRequestResponse>()
                        .FirstOrDefault();
                    if (toBeCached != null && !toBeCached.IsIdentityStatic)
                    {
                        _cache.TryAdd(actionName, toBeCached);
                    }
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}
