using Pipaslot.Mediator.Authorization;

namespace Pipaslot.Mediator.Tests.Authorization
{
    public class Rule_OperatorTests
    {
        [Fact]
        public void Add_TwoRules_AndRuleSet()
        {
            var combined = Rule.AllowOrDeny(true)
                         + Rule.AllowOrDeny(true);
            AssertTwoRules(combined, Operator.Add);
        }

        [Fact]
        public void And_TwoRules_AndRuleSet()
        {
            var combined = Rule.AllowOrDeny(true)
                         & Rule.AllowOrDeny(true);
            AssertTwoRules(combined, Operator.And);
        }

        [Fact]
        public void Or_TwoRules_OrRuleSet()
        {
            var combined = Rule.AllowOrDeny(true)
                         | Rule.AllowOrDeny(true);
            AssertTwoRules(combined, Operator.Or);
        }

        private static void AssertTwoRules(RuleSet combined, Operator expectedOperator)
        {
            Assert.Equal(expectedOperator, combined.Operator);
            Assert.Equal(2, combined.Rules.Count);
        }

    }
}
