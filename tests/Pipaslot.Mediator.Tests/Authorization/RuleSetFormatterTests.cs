using Pipaslot.Mediator.Authorization;

namespace Pipaslot.Mediator.Tests.Authorization
{
    public class RuleSetFormatterTests
    {
        [Theory]
        [InlineData(Operator.And)]
        [InlineData(Operator.Or)]
        public void Format(Operator @operator)
        {
            var sut = RuleSetFormatter.Instance;
            var hiddenSet = RuleSet.Create(
                Operator.Or,
                new Rule("Role", "A1", true),
                new Rule("Claim", "A2")
                );
            var shownOrSet = RuleSet.Create(
                Operator.Or,
                new Rule("Role", "A3"),
                new Rule("Claim", "A4")
                );
            var shownDuplicateOrSet = RuleSet.Create(
                Operator.Or,
                new Rule("Role", "A5"),
                new Rule("Role", "A6")
                );
            var shownAndSet = new RuleSet(
                new Rule("Claim", "A7"),
                new Rule("Claim", "A8")
                );
            var collection = RuleSet.Create(@operator, hiddenSet, shownOrSet, shownDuplicateOrSet, shownAndSet);
            var expected = "("
                + sut.Format(shownOrSet)
                + $" {@operator} "
                + sut.Format(shownDuplicateOrSet)
                + $" {@operator} "
                + sut.Format(shownAndSet)
                + ")";
            Assert.Equal(expected, sut.Format(collection));
        }


        [Fact]
        public void Format_Single()
        {
            var sut = RuleSetFormatter.Instance;
            var set = new RuleSet(
                new Rule("Role", "Admin"),
                new Rule("Ignored", "IgnoredValue", true)
                );
            var expected = $"{{'Role': 'Admin'}}";
            Assert.Equal(expected, sut.Format(set));
        }

        [Theory]
        [InlineData(Operator.And)]
        [InlineData(Operator.Or)]
        public void Format_TwoWithUniqueName(Operator @operator)
        {
            var sut = RuleSetFormatter.Instance;
            var set = RuleSet.Create(
                @operator,
                new Rule("Role", "A1"),
                new Rule("Claim", "A2"),
                new Rule("Ignored", "IgnoredValue", true)
                );
            var expected = $"({{'Role': 'A1'}} {@operator} {{'Claim': 'A2'}})";
            Assert.Equal(expected, sut.Format(set));
        }

        [Theory]
        [InlineData(Operator.And, true)]
        [InlineData(Operator.And, false)]
        [InlineData(Operator.Or, true)]
        [InlineData(Operator.Or, false)]
        public void Format_TwoWithDuplicateName(Operator @operator, bool theSameNameCase)
        {
            var sut = RuleSetFormatter.Instance;
            var name = "Role";
            var set = RuleSet.Create(
                @operator,
                new Rule(name, "A1"),
                new Rule(theSameNameCase ? name : name.ToUpper(), "A2"),
                new Rule("Ignored", "IgnoredValue", true)
                );
            var expected = $"{{'Role': ['A1' {@operator} 'A2']}}";
            Assert.Equal(expected, sut.Format(set));
        }

        [Fact]
        public void Format_CollectionWithSingleRuleSet_ReturnOnlySet()
        {
            var sut = RuleSetFormatter.Instance;
            var set = new RuleSet(
                new Rule("Role", "A1"),
                new Rule("Claim", "A2")
                );
            var collection = new RuleSet(set);
            Assert.Equal(sut.Format(set), sut.Format(collection));
        }
    }
}
