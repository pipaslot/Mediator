using Blazored.LocalStorage;
using Demo.Shared.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Pipaslot.Mediator;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Demo.Client.Services;

public class AuthService : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly IMediator _mediator;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient, IMediator mediator, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _mediator = mediator;
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var authState = await _localStorage.GetItemAsync<AuthState>(AuthState.Name);
        if (authState == null
            || string.IsNullOrWhiteSpace(authState.Username)
            || string.IsNullOrWhiteSpace(authState.BearerToken)
            || authState.Expiration < DateTime.Now)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        if (authState.BearerToken != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", authState.BearerToken);
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, authState.Username) }, "jwt")));
    }

    public async Task SignIn(string username, string password)
    {
        var response = await _mediator.Execute(new LoginRequest { Login = username, Password = password });
        if (!response.Success)
        {
            throw new Exception("Authentication request failed");
        }

        await _localStorage.SetItemAsync(AuthState.Name,
            new AuthState { Username = username, BearerToken = response.Result.Token, Expiration = response.Result.Expiration });
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOut()
    {
        await _localStorage.RemoveItemAsync(AuthState.Name);
        _httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private class AuthState
    {
        internal const string Name = "Auth";
        public string? BearerToken { get; set; }
        public string? Username { get; set; }
        public DateTime Expiration { get; set; }
    }
}