using Pipaslot.Mediator.Notifications;
using Pipaslot.Mediator.Tests.InvalidActions;
using Pipaslot.Mediator.Tests.ValidActions;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

/// <summary>
/// MediatorExecutionException splits into three subtypes depending on the failure trigger (no handler found,
/// status set to Failed without a recorded exception, or expected result missing). These tests cover properties
/// that apply across all three at once - existing <c>catch (MediatorExecutionException)</c> code still works, and
/// <c>Response</c> is populated the same way regardless of subtype - so they don't belong to any single trigger's
/// own test file. Each trigger's own "throws the correct subtype" assertion lives with that trigger's test
/// (no-handler in E2E.Nohandler, status-Failed-without-throw in E2E.NoHandlerWithoutError, missing-result in
/// E2E.ResultWasTakenFromTheContext) - don't duplicate that here.
/// </summary>
public class Mediator_ExecutionExceptionSubtypeTests
{
    [Theory]
    [InlineData(SubtypeTrigger.NoHandler)]
    [InlineData(SubtypeTrigger.MissingResult)]
    [InlineData(SubtypeTrigger.UnhandledError)]
    public async Task ExecuteUnhandled_AnySubtype_StillCatchableAsBaseMediatorExecutionException(SubtypeTrigger trigger)
    {
        var sut = trigger switch
        {
            SubtypeTrigger.MissingResult => Factory.CreateConfiguredMediator(c => c.Use<E2E.ResultWasTakenFromTheContext.RemoveResultFromContextMiddleware>()),
            SubtypeTrigger.UnhandledError => Factory.CreateConfiguredMediator(c => c.Use<E2E.NoHandlerWithoutError.BlockRequestMiddleware>()),
            _ => Factory.CreateConfiguredMediator(),
        };

        var caught = false;
        try
        {
            if (trigger == SubtypeTrigger.UnhandledError)
            {
                await sut.ExecuteUnhandled(new E2E.NoHandlerWithoutError.BlockedRequest());
            }
            else
            {
                await sut.ExecuteUnhandled(new RequestWithoutHandler());
            }
        }
        catch (MediatorExecutionException ex)
        {
            caught = true;
            var expectedType = trigger switch
            {
                SubtypeTrigger.NoHandler => typeof(MediatorNoHandlerFoundException),
                SubtypeTrigger.MissingResult => typeof(MediatorMissingResultException),
                SubtypeTrigger.UnhandledError => typeof(MediatorUnhandledErrorException),
                _ => throw new System.ArgumentOutOfRangeException(nameof(trigger)),
            };
            Assert.IsType(expectedType, ex);
        }

        Assert.True(caught, "catch (MediatorExecutionException) should have caught the subtype instance.");
    }

    public enum SubtypeTrigger
    {
        NoHandler,
        MissingResult,
        UnhandledError,
    }

    [Fact]
    public async Task ExecuteUnhandled_StatusFailedWithoutThrow_ResponsePopulatedIdenticallyToBaseType()
    {
        // Response is built by the MediatorExecutionException base constructor, unchanged by this unit - this test
        // proves that reading Response off the now-thrown MediatorUnhandledErrorException still reflects the real
        // pipeline results exactly as it did off the base type before this change (no regression in what a caught
        // exception exposes). Reuses NoHandlerAndErrorReturned's fixture rather than a local copy.
        var sut = Factory.CreateConfiguredMediator(c => c.Use<E2E.NoHandlerAndErrorReturned.AddErrorAndEndMiddleware>());
        var action = new SingleHandler.Request(true);

        var ex = await Assert.ThrowsAsync<MediatorUnhandledErrorException>(() => sut.ExecuteUnhandled(action));

        Assert.False(ex.Response.Success);
        Assert.Contains(ex.Response.Results.OfType<Notification>(), n => n.Content == E2E.NoHandlerAndErrorReturned.Error);
    }
}
