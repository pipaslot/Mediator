using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Tests.Abstractions;

public class MediatorActionExtensionsTests
{
    [Test]
    [Arguments(-1, "Rude.Dude.TheMITUniversityILove_Action", "The MIT university I love action")]
    [Arguments(0, null, "")]
    [Arguments(0, "Rude.Dude.TheMITUniversityILove_Action", "The MIT university I love action")]
    [Arguments(0, "Rude.Dude.TheMITUniversityILove+Action", "The MIT university I love action")]
    [Arguments(0, "TheMITUniversityILove_Action", "The MIT university I love action")]
    [Arguments(1, "Rude.Dude.TheMITUniversityILove_Action", "The MIT university I love")]
    [Arguments(1, "Rude.Dude.TheMITUniversityILove+Action", "The MIT university I love")]
    [Arguments(2, "Rude.Dude.TheMITUniversityILove_Action", "The MIT university I")]
    [Arguments(3, "Rude.Dude.TheMITUniversityILove_Action", "The MIT university")]
    [Arguments(4, "Rude.Dude.TheMITUniversityILove_Action", "The MIT")]
    [Arguments(5, "Rude.Dude.TheMITUniversityILove_Action", "The")]
    [Arguments(6, "Rude.Dude.TheMITUniversityILove_Action", "")]
    [Arguments(7, "Rude.Dude.TheMITUniversityILove_Action", "")]
    public void GetActionFriendlyName_WithoutLastWord(int ignoredLastWords, string? source, string expected)
    {
        var name = source.GetActionFriendlyName(ignoredLastWords);
        Assert.Equal(expected, name);
    }
}