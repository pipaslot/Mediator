using Pipaslot.Mediator.Authorization;

namespace Pipaslot.Mediator.Tests.Authorization
{
    public class RuleSetTests
    {
        [Theory]
        [InlineData(Operator.And, true, true, true)]
        [InlineData(Operator.And, false, true, false)]
        [InlineData(Operator.And, true, false, false)]
        [InlineData(Operator.And, false, false, false)]
        [InlineData(Operator.Or, true, true, true)]
        [InlineData(Operator.Or, false, true, true)]
        [InlineData(Operator.Or, true, false, true)]
        [InlineData(Operator.Or, false, false, false)]
        public void Granted(Operator @operator, bool rule1, bool rule2, bool expected)
        {
            var sut = RuleSet.Create(
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
        [InlineData(Operator.And)]
        [InlineData(Operator.Or)]
        public void StringifyNotGranted_TwoWithUniqueName(Operator @operator)
        {
            var set = RuleSet.Create(
                @operator,
                new Rule("Role", "A1"),
                new Rule("Claim", "A2"),
                new Rule("Ignored", "IgnoredValue", true)
                );
            var expected = $"({{'Role': 'A1'}} {@operator} {{'Claim': 'A2'}})";
            Assert.Equal(expected, set.StringifyNotGranted());
        }

        [Theory]
        [InlineData(Operator.And, true)]
        [InlineData(Operator.And, false)]
        [InlineData(Operator.Or, true)]
        [InlineData(Operator.Or, false)]
        public void StringifyNotGranted_TwoWithDuplicateName(Operator @operator, bool theSameNameCase)
        {
            var name = "Role";
            var set = RuleSet.Create(
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
            var collection = new RuleSet(set);
            Assert.Equal(set.StringifyNotGranted(), collection.StringifyNotGranted());
        }
    }
}
