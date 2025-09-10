using Pipaslot.Mediator.Authorization;
using System;
using System.Threading;

namespace Pipaslot.Mediator.Tests.Authorization;

public class PolicyTests
{
    [Test]
    [Arguments(true, true, true)]
    [Arguments(true, false, false)]
    [Arguments(false, true, false)]
    [Arguments(false, false, false)]
    public async Task And_Instance(bool left, bool right, bool expected)
    {
        var policy = new FakeBoolPolicy(left).And(new FakeBoolPolicy(right));
        await AssertPolicy(policy, expected);
    }

    [Test]
    [Arguments(true, true, true)]
    [Arguments(true, false, false)]
    [Arguments(false, true, false)]
    [Arguments(false, false, false)]
    public async Task And_Operator(bool left, bool right, bool expected)
    {
        var policy = (IPolicy)new FakeBoolPolicy(left) & new FakeBoolPolicy(right);
        await AssertPolicy(policy, expected);
    }

    [Test]
    [Arguments(true, true, true)]
    [Arguments(true, false, false)]
    [Arguments(false, true, false)]
    [Arguments(false, false, false)]
    public async Task And_Static(bool left, bool right, bool expected)
    {
        var policy = Policy.And(new FakeBoolPolicy(left), new FakeBoolPolicy(right));
        await AssertPolicy(policy, expected);
    }

    [Test]
    [Arguments(true, true, true)]
    [Arguments(true, false, true)]
    [Arguments(false, true, true)]
    [Arguments(false, false, false)]
    public async Task Or_Instance(bool left, bool right, bool expected)
    {
        var policy = new FakeBoolPolicy(left).Or(new FakeBoolPolicy(right));
        await AssertPolicy(policy, expected);
    }

    [Test]
    [Arguments(true, true, true)]
    [Arguments(true, false, true)]
    [Arguments(false, true, true)]
    [Arguments(false, false, false)]
    public async Task Or_operator(bool left, bool right, bool expected)
    {
        var policy = (IPolicy)new FakeBoolPolicy(left) | new FakeBoolPolicy(right);
        await AssertPolicy(policy, expected);
    }

    [Test]
    [Arguments(true, true, true)]
    [Arguments(true, false, true)]
    [Arguments(false, true, true)]
    [Arguments(false, false, false)]
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
        private readonly bool _value;

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