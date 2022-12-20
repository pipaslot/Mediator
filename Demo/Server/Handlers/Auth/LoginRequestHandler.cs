using Demo.Shared.Auth;
using Demo.Shared.Auth.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pipaslot.Mediator;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Demo.Server.Handlers.Auth
{
    public class LoginRequestHandler : IRequestHandler<LoginRequest, LoginRequestResult>
    {
        private readonly IOptions<AuthOptions> _authOptions;

        public LoginRequestHandler(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions;
        }

        public Task<LoginRequestResult> Handle(LoginRequest action, CancellationToken cancellationToken)
        {
            var options = _authOptions.Value;
            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, action.Login),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("role", "visitor")
                };
            var expiration = DateTime.UtcNow.AddMinutes(options.TokenExpirationInMinutes);
            var jwtToken = new JwtSecurityToken(
                options.Issuer,
                options.Audience,
                claims,
                expires: expiration,
                signingCredentials: new SigningCredentials(
                    CreateSymetricKey(options.SecretKey),
                    SecurityAlgorithms.HmacSha256));

            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);


            return Task.FromResult(new LoginRequestResult { 
                Token = token!,
                Expiration = expiration
            });
        }

        public static SymmetricSecurityKey CreateSymetricKey(string secret)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }

        public class AuthOptions
        {
            public string SecretKey { get; set; } = string.Empty;
            public string Issuer { get; set; } = string.Empty;
            public string Audience { get; set; } = string.Empty;
            public int TokenExpirationInMinutes { get; set; }
        }
    }
}
