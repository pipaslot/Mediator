using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    public class IdentityPolicy : IPolicy
    {
        /// <summary>
        /// Authenticated user can access the resource
        /// </summary>
        public static IdentityPolicy Authenticated()
        {
            return new IdentityPolicy();
        }

        /// <summary>
        /// Everybody can access the resource
        /// </summary>
        public static IdentityPolicy Anonymous()
        {
            var policy = new IdentityPolicy();
            policy.IsAnonymous();
            return policy;
        }

        /// <summary>
        /// Only user with specific claim can access the resource
        /// </summary>
        public static IdentityPolicy Claim(string name, string value)
        {
            var policy = new IdentityPolicy();
            policy.HasClaim(name, value);
            return policy;
        }

        /// <summary>
        /// Only user with specific role can access the resource
        /// </summary>
        public static IdentityPolicy Role(string value)
        {
            var policy = new IdentityPolicy();
            policy.HasRole(value);
            return policy;
        }

        private bool _anonymous = false;
        private readonly List<(string Name, string Value)> _claims = new();

        public IdentityPolicy IsAnonymous()
        {
            _anonymous = true;
            return this;
        }

        public IdentityPolicy HasRole(string value)
        {
            return HasClaim(ClaimTypes.Role, value);
        }

        public IdentityPolicy HasClaim(string name, string value)
        {
            _claims.Add((name, value));
            return this;
        }

        public Task<IEnumerable<Rule>> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            var principal = ClaimsPrincipal.Current;
            var isAuthenticatedUser = principal?.Identity?.IsAuthenticated ?? false;
            var isAuthenticated = _anonymous || isAuthenticatedUser;
            var result = new List<Rule>();
            result.Add(new Rule("Authenticated", isAuthenticatedUser.ToString(), isAuthenticated));

            var claims = principal?.Claims ?? new Claim[0];
            foreach (var required in _claims)
            {
                var hasClaim = claims.Any(c => c.Type.Equals(required.Name, StringComparison.OrdinalIgnoreCase)
                                            && c.Value.Equals(required.Value, StringComparison.OrdinalIgnoreCase));
                result.Add(new Rule(required.Name, required.Value, hasClaim));
            }
            return Task.FromResult((IEnumerable<Rule>)result);
        }
    }
}
