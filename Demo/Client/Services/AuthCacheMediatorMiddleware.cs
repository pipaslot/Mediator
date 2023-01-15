using Microsoft.AspNetCore.Components.Authorization;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Middlewares;
using System.Collections.Concurrent;

namespace Demo.Client.Services
{
    public class AuthCacheMediatorMiddleware : IMediatorMiddleware, IDisposable
    {
        private readonly ConcurrentDictionary<string, AuthorizeRequestResponse> _cache = new ConcurrentDictionary<string, AuthorizeRequestResponse>();
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

        public void ClearCache(Task<AuthenticationState> authState)
        {
            _cache.Clear();
        }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            if (context.Action is AuthorizeRequest authRequest && authRequest.Action != null)
            {
                var actionName = authRequest.Action.GetType().FullName ?? throw new Exception("Can not get action name");
                var cacheKey = $"{actionName}##{authRequest.Action.GetHashCode()}";
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    context.AddResult(cached);
                    context.Status = ExecutionStatus.Succeeded;
                    Console.WriteLine($"Authentication state for action '{cacheKey}' was loaded from cache.");
                }
                else
                {
                    await next(context);
                    var toBeCached = context.Results
                        .Where(r => r is AuthorizeRequestResponse)
                        .Cast<AuthorizeRequestResponse>()
                        .FirstOrDefault();
                    if (toBeCached != null && toBeCached.IsIdentityStatic)
                    {
                        _cache.TryAdd(cacheKey, toBeCached);
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
