using Pipaslot.Mediator.Authorization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Authorization
{
    public class Rule_OperatorTests
    {
        [Fact]
        public void And_TwoRules_AndRuleSet()
        {
            var combined = Rule.Allow(true)
                         & Rule.Allow(true);
            AssertTwoRules(combined, Operator.And);
        }

        [Fact]
        public void Or_TwoRules_OrRuleSet()
        {
            var combined = Rule.Allow(true)
                         | Rule.Allow(true);
            AssertTwoRules(combined, Operator.Or);
        }

        private static void AssertTwoRules(RuleSet combined, Operator expectedOperator)
        {
            Assert.Equal(expectedOperator, combined.Operator);
            Assert.Equal(2, combined.Rules.Count);
        }

    }
}
