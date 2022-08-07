﻿using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Authorization
{
    public class IdentityPolicyTests
    {
        const string ClaimName = "Role";
        const string ClaimValue = "Admin";

        [Theory]
        [InlineData(false)]
        public async Task AnonymousPolicy_HasOnlyAuthenticatedRule(bool isAuthenticated)
        {
            var sut = IdentityPolicy.Anonymous();
            var rules = await sut.Resolve(CreateServiceProvider(isAuthenticated), CancellationToken.None);

            Assert.Single(rules);
            var rule = rules.First();
            Assert.Equal(IdentityPolicy.AuthenticatedPolicyName, rule.Name);
            Assert.Equal("False", rule.Value);
            Assert.True(rule.Granted);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AuthenticatedPolicy_HasOnlyAuthenticatedRule(bool isAuthenticated)
        {
            var sut = IdentityPolicy.Authenticated();
            var rules = await sut.Resolve(CreateServiceProvider(isAuthenticated), CancellationToken.None);

            Assert.Single(rules);
            AssertAuthenticatedRule(rules, isAuthenticated);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ClaimPolicy_HasAuthenticatedRule(bool isAuthenticated)
        {
            var claim = new Claim(ClaimName, ClaimValue);

            var sut = IdentityPolicy.Claim(ClaimName, ClaimValue);
            var rules = await sut.Resolve(CreateServiceProvider(isAuthenticated, claim), CancellationToken.None);

            AssertAuthenticatedRule(rules, isAuthenticated);
        }

        [Fact]
        public async Task ClaimPolicy_HasClaimRule()
        {
            var claim = new Claim(ClaimName, ClaimValue);

            var sut = IdentityPolicy.Claim(ClaimName, ClaimValue);
            var rules = await sut.Resolve(CreateServiceProvider(true, claim), CancellationToken.None);

            AssertClaimRule(rules, claim, true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task RolePolicy_HasAuthenticatedRule(bool isAuthenticated)
        {
            var claim = new Claim(ClaimTypes.Role, ClaimValue);

            var sut = IdentityPolicy.Role(ClaimValue);
            var rules = await sut.Resolve(CreateServiceProvider(isAuthenticated, claim), CancellationToken.None);

            AssertAuthenticatedRule(rules, isAuthenticated);
        }

        [Fact]
        public async Task RolePolicy_HasClaimRule()
        {
            var claim = new Claim(ClaimTypes.Role, ClaimValue);

            var sut = IdentityPolicy.Role(ClaimValue);
            var rules = await sut.Resolve(CreateServiceProvider(true, claim), CancellationToken.None);

            AssertClaimRule(rules, claim, true);
        }

        private static void AssertAuthenticatedRule(IEnumerable<Rule> rules, bool shouldGrant)
        {
            var rule = rules.First(r => r.Name == IdentityPolicy.AuthenticatedPolicyName);
            Assert.Equal("True", rule.Value);
            Assert.Equal(shouldGrant, rule.Granted);
        }

        private static void AssertClaimRule(IEnumerable<Rule> rules, Claim claim, bool shouldGrant)
        {
            var rule = rules.First(r => r.Name == claim.Type);
            Assert.Equal(shouldGrant, rule.Granted);
        }

        private IServiceProvider CreateServiceProvider(bool isAuthenticated, params Claim[] claims)
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, isAuthenticated ? "JWT" : null));
            var cpMock = new Mock<IClaimPrincipalAccessor>();
            cpMock
                .Setup(m => m.Principal)
                .Returns(principal);

            var collection = new ServiceCollection();
            collection.AddScoped<IClaimPrincipalAccessor>(s => cpMock.Object);
            return collection.BuildServiceProvider();
        }
    }
}
