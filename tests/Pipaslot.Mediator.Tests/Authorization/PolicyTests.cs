using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Authorization
{
    public class PolicyTests
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public async Task And_Instance(bool left, bool right, bool expected)
        {
            var policy = new FakeBoolPolicy(left).And(new FakeBoolPolicy(right));
            await AssertPolicy(policy, expected);
        }
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public async Task And_Operator(bool left, bool right, bool expected)
        {
            var policy = (IPolicy)new FakeBoolPolicy(left) & new FakeBoolPolicy(right);
            await AssertPolicy(policy, expected);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public async Task And_Static(bool left, bool right, bool expected)
        {
            var policy = Policy.And(new FakeBoolPolicy(left), new FakeBoolPolicy(right));
            await AssertPolicy(policy, expected);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public async Task Or_Instance(bool left, bool right, bool expected)
        {
            var policy = new FakeBoolPolicy(left).Or(new FakeBoolPolicy(right));
            await AssertPolicy(policy, expected);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public async Task Or_operator(bool left, bool right, bool expected)
        {
            var policy = (IPolicy)new FakeBoolPolicy(left) | new FakeBoolPolicy(right);
            await AssertPolicy(policy, expected);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public async Task Or_Static(bool left, bool right, bool expected)
        {
            var policy = Policy.Or(new FakeBoolPolicy(left), new FakeBoolPolicy(right));
            await AssertPolicy(policy, expected);
        }

        private async Task AssertPolicy(IPolicy policy, bool expected)
        {
            var services = new Mock<IServiceProvider>();
            var set = await policy.Resolve(services.Object, CancellationToken.None);
            var evaluated = set.Reduce();
            Assert.Equal(expected, evaluated.Outcome == RuleOutcome.Allow);
        }

        private class FakeBoolPolicy : IPolicy
        {
            private bool _value;

            public FakeBoolPolicy(bool value)
            {
                _value = value;
            }

            public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
            {
                var set = new RuleSet(new Rule("FakeName", "FakeValue", _value));
                return Task.FromResult(set);
            }
        }
    }
}
