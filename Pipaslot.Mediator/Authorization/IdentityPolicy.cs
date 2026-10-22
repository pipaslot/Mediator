using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Policy over the identity of the current user - authentication, roles and claims - read from <see cref="IClaimPrincipalAccessor"/>.
/// </summary>
/// <remarks>
/// Start from <see cref="Authenticated"/>, <see cref="Anonymous"/>, <see cref="Claim"/> or <see cref="Role"/> and chain
/// <see cref="HasRole"/>/<see cref="HasClaim"/> for more requirements. Its rules have <see cref="RuleScope.Identity"/>, so a
/// client may cache the answer until the user changes. For the common cases the attributes
/// <see cref="AnonymousPolicyAttribute"/>, <see cref="AuthenticatedPolicyAttribute"/> and <see cref="RolePolicyAttribute"/>
/// need no code.
/// </remarks>
/// <param name="operator">How the required claims combine: <see cref="Operator.And"/> requires all of them,
/// <see cref="Operator.Or"/> any of them.</param>
public class IdentityPolicy(Operator @operator = Operator.And) : IPolicy
{
    /// <summary>
    /// <see cref="Rule.Name"/> of the rule produced when no claim is required; its <see cref="Rule.Value"/> is
    /// <see cref="AuthenticatedValue"/> or <see cref="AnonymousValue"/>. Lets a custom node formatter recognise the rule.
    /// </summary>
    public const string AuthenticationPolicyName = "Authentication";

    /// <summary>
    /// <see cref="Rule.Value"/> of the <see cref="AuthenticationPolicyName"/> rule requiring an authenticated user.
    /// </summary>
    public const string AuthenticatedValue = "Authenticated";

    /// <summary>
    /// <see cref="Rule.Value"/> of the <see cref="AuthenticationPolicyName"/> rule letting any caller through.
    /// </summary>
    public const string AnonymousValue = "Anonymous";

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

    private readonly List<(string Name, string Value)> _claims = [];
    private AuthStatus _authStatus = AuthStatus.AuthenticateIfNoClaim;

    /// <summary>
    /// Lets an unauthenticated caller through, as long as no claim is required. Once a claim is required, only the claim
    /// check decides.
    /// </summary>
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

    /// <summary>
    /// Requires a claim of type <paramref name="name"/> with <paramref name="value"/>, both compared case-insensitively.
    /// Several required claims combine by the operator passed to the constructor. Once any claim is required, no separate
    /// authentication check is made.
    /// </summary>
    public IdentityPolicy HasClaim(string name, string value)
    {
        _claims.Add((name, value));
        return this;
    }

    /// <inheritdoc/>
    public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
    {
        var principal = services.GetService<IClaimPrincipalAccessor>()?.Principal;
        var collection = new RuleSet(Operator.And);
        if (!_claims.Any())
        {
            var isAnonymous = _authStatus == AuthStatus.AnonymousIfNoClaim;
            var isAuthenticationGranted = isAnonymous || (principal?.Identity?.IsAuthenticated ?? false);
            var shouldBeAuthenticated = !isAnonymous;
            var value = shouldBeAuthenticated
                ? AuthenticatedValue
                : AnonymousValue;
            var authRule = new Rule(AuthenticationPolicyName, value, isAuthenticationGranted, RuleScope.Identity);
            collection.Rules.Add(authRule);
        }
        else
        {
            var userClaims = principal?.Claims?.ToArray() ?? [];
            var claimRules = new RuleSet(@operator);
            collection.RuleSets.Add(claimRules);
            foreach (var required in _claims)
            {
                var hasClaim = userClaims.Any(c => c.Type.Equals(required.Name, StringComparison.OrdinalIgnoreCase)
                                                   && c.Value.Equals(required.Value, StringComparison.OrdinalIgnoreCase));
                claimRules.Rules.Add(new Rule(required.Name, required.Value, hasClaim, RuleScope.Identity));
            }
        }

        return Task.FromResult(collection);
    }

    /// <inheritdoc cref="IPolicy.op_BitwiseAnd"/>
    public static IPolicy operator &(IdentityPolicy c1, IPolicy c2)
    {
        return c1.And(c2);
    }

    /// <inheritdoc cref="IPolicy.op_BitwiseOr"/>
    public static IPolicy operator |(IdentityPolicy c1, IPolicy c2)
    {
        return c1.Or(c2);
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
        AnonymousIfNoClaim
    }
}