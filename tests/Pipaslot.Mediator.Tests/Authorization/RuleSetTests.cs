using Pipaslot.Mediator.Authorization;

namespace Pipaslot.Mediator.Tests.Authorization
{
    public class RuleSetTests
    {
        [Theory]
        [InlineData(RuleOperator.And, true, true, true)]
        [InlineData(RuleOperator.And, false, true, false)]
        [InlineData(RuleOperator.And, true, false, false)]
        [InlineData(RuleOperator.And, false, false, false)]
        [InlineData(RuleOperator.Or, true, true, true)]
        [InlineData(RuleOperator.Or, false, true, true)]
        [InlineData(RuleOperator.Or, true, false, true)]
        [InlineData(RuleOperator.Or, false, false, false)]
        public void Granted(RuleOperator @operator, bool rule1, bool rule2, bool expected)
        {
            var sut = new RuleSet(
                    @operator,
                    new Rule("Role", "A1", rule1),
                    new Rule("Role", "A2", rule2)
                );
            Assert.Equal(expected, sut.Granted);
        }

        [Fact]
        public void StringifyNotGranted_Single()
        {
            var set = new RuleSet(
                new Rule("Role", "Admin"),
                new Rule("Ignored", "IgnoredValue", true)
                );
            var expected = $"{{'Role': 'Admin'}}";
            Assert.Equal(expected, set.StringifyNotGranted());
        }

        [Theory]
        [InlineData(RuleOperator.And)]
        [InlineData(RuleOperator.Or)]
        public void StringifyNotGranted_TwoWithUniqueName(RuleOperator @operator)
        {
            var set = new RuleSet(
                @operator,
                new Rule("Role", "A1"),
                new Rule("Claim", "A2"),
                new Rule("Ignored", "IgnoredValue", true)
                );
            var expected = $"({{'Role': 'A1'}} {@operator} {{'Claim': 'A2'}})";
            Assert.Equal(expected, set.StringifyNotGranted());
        }

        [Theory]
        [InlineData(RuleOperator.And, true)]
        [InlineData(RuleOperator.And, false)]
        [InlineData(RuleOperator.Or, true)]
        [InlineData(RuleOperator.Or, false)]
        public void StringifyNotGranted_TwoWithDuplicateName(RuleOperator @operator, bool theSameNameCase)
        {
            var name = "Role";
            var set = new RuleSet(
                @operator,
                new Rule(name, "A1"),
                new Rule(theSameNameCase ? name : name.ToUpper(), "A2"),
                new Rule("Ignored", "IgnoredValue", true)
                );
            var expected = $"{{'Role': ['A1' {@operator} 'A2']}}";
            Assert.Equal(expected, set.StringifyNotGranted());
        }

        [Fact]
        public void StringifyNotGranted_CollectionWithSingleRuleSet_ReturnOnlySet()
        {
            var set = new RuleSet(
                new Rule("Role", "A1"),
                new Rule("Claim", "A2")
                );
            var collection = new RuleSetCollection(set);
            Assert.Equal(set.StringifyNotGranted(), collection.StringifyNotGranted());
        }
    }
}
