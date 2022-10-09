using Microsoft.Extensions.DependencyInjection;
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
        public const string AuthenticatedPolicyName = "Authenticated";

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
        /// Only authenticated user with specific claim can access the resource
        /// </summary>
        public static IdentityPolicy Claim(string name, string value)
        {
            return new IdentityPolicy()
                .HasClaim(name, value);
        }

        /// <summary>
        /// Only authenticated user with specific role can access the resource
        /// </summary>
        public static IdentityPolicy Role(string value)
        {
            return new IdentityPolicy()
                .HasRole(value);
        }

        private readonly List<(string Name, string Value)> _claims = new();
        private Operator _claimOperator;
        private AuthStatus _authStatus = AuthStatus.AuthenticateIfNoClaim;

        public IdentityPolicy(Operator @operator = Operator.And)
        {
            _claimOperator = @operator;
        }

        public IdentityPolicy IsAnonymous()
        {
            _authStatus = AuthStatus.AnonymousIfNoClaim;
            return this;
        }

        /// <summary>
        /// Ensure that user has Role claim
        /// </summary>
        public IdentityPolicy HasRole(string value)
        {
            return HasClaim(ClaimTypes.Role, value);
        }

        public IdentityPolicy HasClaim(string name, string value)
        {
            _claims.Add((name, value));
            return this;
        }

        public Task<IRuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            var principal = services.GetService<IClaimPrincipalAccessor>()?.Principal;
            var collection = new List<IRuleSet>();
            if (_claims.Count() == 0)
            {
                var isAnonymous = _authStatus == AuthStatus.AnonymousIfNoClaim;
                var isAuthenticationGranted = isAnonymous || (principal?.Identity?.IsAuthenticated ?? false);
                var shouldBeAuthenticated = !isAnonymous;
                var authRule = new Rule(AuthenticatedPolicyName, shouldBeAuthenticated.ToString(), isAuthenticationGranted);
                collection.Add(new RuleSet(authRule));
            }
            else
            {
                var userClaims = principal?.Claims ?? new Claim[0];
                var claimRules = new List<Rule>();
                foreach (var required in _claims)
                {
                    var hasClaim = userClaims.Any(c => c.Type.Equals(required.Name, StringComparison.OrdinalIgnoreCase)
                                                && c.Value.Equals(required.Value, StringComparison.OrdinalIgnoreCase));
                    claimRules.Add(new Rule(required.Name, required.Value, hasClaim));
                }
                collection.Add(new RuleSet(_claimOperator, claimRules));
            }
            return Task.FromResult((IRuleSet)new RuleSet(Operator.And, collection));
        }

        private enum AuthStatus
        {
            /// <summary>
            /// Default state will force to define at least rule to authenticate user. 
            /// Reduce unnecessary Authentication rules.
            /// </summary>
            AuthenticateIfNoClaim,
            /// <summary>
            /// Define rule for for anonymous authentication when no other claim is required. 
            /// Reduce unnecessary Authentication rules.
            /// </summary>
            AnonymousIfNoClaim,
        }
    }
}
