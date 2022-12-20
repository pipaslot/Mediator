using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.RuleSetFormatters;

namespace Pipaslot.Mediator.Tests.Authorization.RuleSetFormatters
{
    public class ExceptionRuleSetFormatterTests
    {
        private ExceptionRuleSetFormatter Create()
        {
            return new ExceptionRuleSetFormatter();
        }

        [Theory]
        [InlineData(Operator.And)]
        [InlineData(Operator.Or)]
        public void Format(Operator @operator)
        {
            var sut = Create();
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
                + sut.FormatInternal(shownOrSet)
                + $" {@operator} "
                + sut.FormatInternal(shownDuplicateOrSet)
                + $" {@operator} "
                + sut.FormatInternal(shownAndSet)
                + ")";
            Assert.Equal(expected, sut.FormatInternal(collection));
        }


        [Fact]
        public void Format_Single()
        {
            var sut = Create();
            var set = new RuleSet(
                new Rule("Role", "Admin"),
                new Rule("Ignored", "IgnoredValue", true)
                );
            var expected = $"{{'Role': 'Admin'}}";
            Assert.Equal(expected, sut.FormatInternal(set));
        }

        [Theory]
        [InlineData(Operator.And)]
        [InlineData(Operator.Or)]
        public void Format_TwoWithUniqueName(Operator @operator)
        {
            var sut = Create();
            var set = RuleSet.Create(
                @operator,
                new Rule("Role", "A1"),
                new Rule("Claim", "A2"),
                new Rule("Ignored", "IgnoredValue", true)
                );
            var expected = $"({{'Role': 'A1'}} {@operator} {{'Claim': 'A2'}})";
            Assert.Equal(expected, sut.FormatInternal(set));
        }

        [Theory]
        [InlineData(Operator.And, true)]
        [InlineData(Operator.And, false)]
        [InlineData(Operator.Or, true)]
        [InlineData(Operator.Or, false)]
        public void Format_TwoWithDuplicateName(Operator @operator, bool theSameNameCase)
        {
            var sut = Create();
            var name = "Role";
            var set = RuleSet.Create(
                @operator,
                new Rule(name, "A1"),
                new Rule(theSameNameCase ? name : name.ToUpper(), "A2"),
                new Rule("Ignored", "IgnoredValue", true)
                );
            var expected = $"{{'Role': ['A1' {@operator} 'A2']}}";
            Assert.Equal(expected, sut.FormatInternal(set));
        }

        [Fact]
        public void Format_CollectionWithSingleRuleSet_ReturnOnlySet()
        {
            var sut = Create();
            var set = new RuleSet(
                new Rule("Role", "A1"),
                new Rule("Claim", "A2")
                );
            var collection = new RuleSet(set);
            Assert.Equal(sut.FormatInternal(set), sut.FormatInternal(collection));
        }
    }
}
