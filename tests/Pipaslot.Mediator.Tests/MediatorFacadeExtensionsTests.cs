using Pipaslot.Mediator.Notifications;

namespace Pipaslot.Mediator.Tests;

/// <summary>
/// Each extension builds a <see cref="Notification"/> from its shorthand parameters and forwards it to
/// <see cref="IMediatorFacade.AddNotification(Notification)"/>. Assertions match all four
/// <see cref="Notification"/> properties explicitly rather than via <see cref="Notification.Equals(Notification?)"/>,
/// because that override intentionally ignores <see cref="Notification.StopPropagation"/>.
/// </summary>
public class MediatorFacadeExtensionsTests
{
    private readonly IMediatorFacade _facade = Substitute.For<IMediatorFacade>();

    [Fact]
    public void AddNotification_ContentSourceAndType_BuildsNotificationWithGivenValues()
    {
        _facade.AddNotification("Content", "Source", NotificationType.Warning, stopPropagation: true);

        VerifyAdded("Content", "Source", NotificationType.Warning, true);
    }

    [Fact]
    public void AddNotification_ContentAndTypeWithoutSource_DefaultsSourceToEmpty()
    {
        _facade.AddNotification("Content", NotificationType.Success);

        VerifyAdded("Content", "", NotificationType.Success, false);
    }

    [Fact]
    public void AddErrorNotification_ContentAndSource_BuildsErrorNotification()
    {
        _facade.AddErrorNotification("Content", "Source", stopPropagation: true);

        VerifyAdded("Content", "Source", NotificationType.Error, true);
    }

    [Fact]
    public void AddWarningNotification_ContentAndSource_BuildsWarningNotification()
    {
        _facade.AddWarningNotification("Content", "Source");

        VerifyAdded("Content", "Source", NotificationType.Warning, false);
    }

    [Fact]
    public void AddInformationNotification_ContentAndSource_BuildsInformationNotification()
    {
        _facade.AddInformationNotification("Content", "Source");

        VerifyAdded("Content", "Source", NotificationType.Information, false);
    }

    [Fact]
    public void AddSuccessNotification_ContentAndSource_BuildsSuccessNotification()
    {
        _facade.AddSuccessNotification("Content", "Source");

        VerifyAdded("Content", "Source", NotificationType.Success, false);
    }

    private void VerifyAdded(string content, string source, NotificationType type, bool stopPropagation)
    {
        _facade.Received(1).AddNotification(Arg.Is<Notification>(n =>
            n != null
            && n.Content == content
            && n.Source == source
            && n.Type == type
            && n.StopPropagation == stopPropagation));
    }
}
