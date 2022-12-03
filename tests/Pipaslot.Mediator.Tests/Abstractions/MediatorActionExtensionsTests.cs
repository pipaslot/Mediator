using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Tests.Abstractions
{
    public class MediatorActionExtensionsTests
    {
        [Theory]
        [InlineData(-1, "Rude.Dude.TheMITUniversityILove_Action", "The MIT university I love action")]
        [InlineData(0, null, "")]
        [InlineData(0, "Rude.Dude.TheMITUniversityILove_Action", "The MIT university I love action")]
        [InlineData(0, "Rude.Dude.TheMITUniversityILove+Action", "The MIT university I love action")]
        [InlineData(0, "TheMITUniversityILove_Action"          , "The MIT university I love action")]
        [InlineData(1, "Rude.Dude.TheMITUniversityILove_Action", "The MIT university I love")]
        [InlineData(2, "Rude.Dude.TheMITUniversityILove_Action", "The MIT university I")]
        [InlineData(3, "Rude.Dude.TheMITUniversityILove_Action", "The MIT university")]
        [InlineData(4, "Rude.Dude.TheMITUniversityILove_Action", "The MIT")]
        [InlineData(5, "Rude.Dude.TheMITUniversityILove_Action", "The")]
        [InlineData(6, "Rude.Dude.TheMITUniversityILove_Action", "")]
        [InlineData(7, "Rude.Dude.TheMITUniversityILove_Action", "")]
        public void GetActionFriendlyName_WithoutLastWord(int ignoredLastWords, string source, string expected)
        {
            var name = source.GetActionFriendlyName(ignoredLastWords);
            Assert.Equal(expected, name);
        }
    }
}
