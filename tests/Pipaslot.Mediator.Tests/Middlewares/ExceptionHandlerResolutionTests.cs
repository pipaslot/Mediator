using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares.Handlers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Middlewares;

/// <summary>
/// Typed exception handler registration, specificity resolution and invocation, exercised standalone against
/// <see cref="ExceptionHandlerCache"/> and <see cref="ExceptionHandlerExecutor{TException}"/> directly - no <see cref="IMediator"/>
/// wiring exists yet (that lands with the Execute/Dispatch boundary wiring). This is a genuinely new trigger condition,
/// not covered by any existing E2E fixture.
/// </summary>
public class ExceptionHandlerResolutionTests
{
    #region Registration

    [Fact]
    public void AddExceptionHandler_SingleExceptionType_CreatesOneRoutingEntryResolvingToRegisteredType()
    {
        var services = Factory.CreateServiceProvider(c => c.AddExceptionHandler<ValidationExceptionHandler>());
        var configurator = services.GetRequiredService<MediatorConfigurator>();

        var entry = configurator.ExceptionHandlerCache.Resolve(typeof(ValidationException));

        Assert.NotNull(entry);
    }

    [Fact]
    public void AddExceptionHandler_HandlerImplementingTwoInterfaces_CreatesTwoRoutingEntriesForSameServiceType()
    {
        var services = Factory.CreateServiceProvider(c => c.AddExceptionHandler<MultiExceptionHandler>());
        var configurator = services.GetRequiredService<MediatorConfigurator>();

        var validationEntry = configurator.ExceptionHandlerCache.Resolve(typeof(ValidationException));
        var timeoutEntry = configurator.ExceptionHandlerCache.Resolve(typeof(TimeoutException));

        Assert.NotNull(validationEntry);
        Assert.NotNull(timeoutEntry);

        var validationHandler = services.GetRequiredService(typeof(IMediatorExceptionHandler<ValidationException>));
        var timeoutHandler = services.GetRequiredService(typeof(IMediatorExceptionHandler<TimeoutException>));
        Assert.IsType<MultiExceptionHandler>(validationHandler);
        Assert.IsType<MultiExceptionHandler>(timeoutHandler);
    }

    [Fact]
    public void AddExceptionHandler_TypeImplementingNoExceptionHandlerInterface_ThrowsAtStartup()
    {
        var sc = new Mock<IServiceCollection>();
        var sut = new MediatorConfigurator(sc.Object);

        Assert.Throws<MediatorException>(() => sut.AddExceptionHandler<NotAnExceptionHandler>());
    }

    [Fact]
    public void AddExceptionHandler_DefaultLifetime_IsTransient()
    {
        var services = Factory.CreateServiceProvider(c => c.AddExceptionHandler<ValidationExceptionHandler>());

        var first = services.CreateScope().ServiceProvider.GetRequiredService<IMediatorExceptionHandler<ValidationException>>();
        var second = services.CreateScope().ServiceProvider.GetRequiredService<IMediatorExceptionHandler<ValidationException>>();

        Assert.NotSame(first, second);
    }

    [Fact]
    public void AddExceptionHandler_ExplicitSingletonLifetime_IsHonoredByResolution()
    {
        var services = Factory.CreateServiceProvider(c => c.AddExceptionHandler<ValidationExceptionHandler>(ServiceLifetime.Singleton));

        var first = services.CreateScope().ServiceProvider.GetRequiredService<IMediatorExceptionHandler<ValidationException>>();
        var second = services.CreateScope().ServiceProvider.GetRequiredService<IMediatorExceptionHandler<ValidationException>>();

        Assert.Same(first, second);
    }

    [Fact]
    public void AddExceptionHandlers_MultipleTypes_RegistersAll()
    {
        var services = Factory.CreateServiceProvider(c =>
            c.AddExceptionHandlers([typeof(ValidationExceptionHandler), typeof(TimeoutExceptionHandler)]));
        var configurator = services.GetRequiredService<MediatorConfigurator>();

        Assert.NotNull(configurator.ExceptionHandlerCache.Resolve(typeof(ValidationException)));
        Assert.NotNull(configurator.ExceptionHandlerCache.Resolve(typeof(TimeoutException)));
    }

    [Fact]
    public void AddExceptionHandlers_OneOfManyTypesIsInvalid_ThrowsAtStartup()
    {
        var sc = new Mock<IServiceCollection>();
        var sut = new MediatorConfigurator(sc.Object);

        Assert.Throws<MediatorException>(() =>
            sut.AddExceptionHandlers([typeof(ValidationExceptionHandler), typeof(NotAnExceptionHandler)]));
    }

    #endregion

    #region Resolution / specificity

    [Fact]
    public void Resolve_ExactlyRegisteredType_Resolves()
    {
        var cache = new ExceptionHandlerCache();
        cache.Add(typeof(ValidationException), typeof(ExceptionHandlerExecutor<ValidationException>));

        var entry = cache.Resolve(typeof(ValidationException));

        Assert.NotNull(entry);
        Assert.Equal(typeof(ValidationException), entry!.ExceptionType);
    }

    [Fact]
    public void Resolve_SubtypeOfRegisteredType_FallsBackToBaseTypeHandler()
    {
        var cache = new ExceptionHandlerCache();
        cache.Add(typeof(ValidationException), typeof(ExceptionHandlerExecutor<ValidationException>));

        var entry = cache.Resolve(typeof(FieldValidationException));

        Assert.NotNull(entry);
        Assert.Equal(typeof(ValidationException), entry!.ExceptionType);
    }

    [Fact]
    public void Resolve_BaseAndDerivedBothRegistered_PicksMoreSpecificOne()
    {
        var cache = new ExceptionHandlerCache();
        cache.Add(typeof(Exception), typeof(ExceptionHandlerExecutor<Exception>));
        cache.Add(typeof(ValidationException), typeof(ExceptionHandlerExecutor<ValidationException>));

        var entry = cache.Resolve(typeof(FieldValidationException));

        Assert.NotNull(entry);
        Assert.Equal(typeof(ValidationException), entry!.ExceptionType);
    }

    [Fact]
    public void Resolve_ThreeClassesInChainAllRegistered_PicksMostDerived()
    {
        var cache = new ExceptionHandlerCache();
        cache.Add(typeof(Exception), typeof(ExceptionHandlerExecutor<Exception>));
        cache.Add(typeof(ValidationException), typeof(ExceptionHandlerExecutor<ValidationException>));
        cache.Add(typeof(FieldValidationException), typeof(ExceptionHandlerExecutor<FieldValidationException>));

        var entry = cache.Resolve(typeof(FieldValidationException));

        Assert.NotNull(entry);
        Assert.Equal(typeof(FieldValidationException), entry!.ExceptionType);
    }

    [Fact]
    public void Resolve_ThreeClassesInChainRegisteredOutOfOrder_StillPicksMostDerived()
    {
        var cache = new ExceptionHandlerCache();
        cache.Add(typeof(FieldValidationException), typeof(ExceptionHandlerExecutor<FieldValidationException>));
        cache.Add(typeof(Exception), typeof(ExceptionHandlerExecutor<Exception>));
        cache.Add(typeof(ValidationException), typeof(ExceptionHandlerExecutor<ValidationException>));

        var entry = cache.Resolve(typeof(FieldValidationException));

        Assert.NotNull(entry);
        Assert.Equal(typeof(FieldValidationException), entry!.ExceptionType);
    }

    [Fact]
    public void Resolve_NoMatchingHandler_ReturnsNullWithoutThrowing()
    {
        var cache = new ExceptionHandlerCache();
        cache.Add(typeof(ValidationException), typeof(ExceptionHandlerExecutor<ValidationException>));

        var entry = cache.Resolve(typeof(TimeoutException));

        Assert.Null(entry);
    }

    [Fact]
    public void Resolve_UnmappedTypeResolvedTwice_NegativeResultIsCachedAfterFirstScan()
    {
        var cache = new ExceptionHandlerCache();
        cache.Add(typeof(ValidationException), typeof(ExceptionHandlerExecutor<ValidationException>));

        cache.Resolve(typeof(TimeoutException));
        cache.Resolve(typeof(TimeoutException));

        Assert.Equal(1, cache.ResolveScanCount);
    }

    [Fact]
    public void Resolve_MappedTypeResolvedTwice_PositiveResultIsCachedAfterFirstScan()
    {
        var cache = new ExceptionHandlerCache();
        cache.Add(typeof(ValidationException), typeof(ExceptionHandlerExecutor<ValidationException>));

        cache.Resolve(typeof(FieldValidationException));
        cache.Resolve(typeof(FieldValidationException));

        Assert.Equal(1, cache.ResolveScanCount);
    }

    #endregion

    #region Invocation / fault isolation

    [Fact]
    public async Task Handle_RegisteredHandlerThrows_DegradesToNotHandled()
    {
        var services = Factory.CreateServiceProvider(c => c.AddExceptionHandler<ThrowingExceptionHandler>());
        var configurator = services.GetRequiredService<MediatorConfigurator>();
        var executor = services.GetExceptionHandlerExecutor(configurator.ExceptionHandlerCache, typeof(ValidationException));

        Assert.NotNull(executor);
        var result = await executor!.Handle(new ValidationException("boom"), services, CancellationToken.None);

        Assert.False(result.IsHandled);
    }

    [Fact]
    public async Task Handle_HandlerResolvesToNullViaDi_DegradesToNotHandledWithoutThrowing()
    {
        var services = Factory.CreateServiceProvider((c, sc) =>
        {
            c.AddExceptionHandler<ValidationExceptionHandler>();
            sc.RemoveAll<IMediatorExceptionHandler<ValidationException>>();
        });
        var configurator = services.GetRequiredService<MediatorConfigurator>();
        var executor = services.GetExceptionHandlerExecutor(configurator.ExceptionHandlerCache, typeof(ValidationException));

        Assert.NotNull(executor);
        var result = await executor!.Handle(new ValidationException("boom"), services, CancellationToken.None);

        Assert.False(result.IsHandled);
    }

    [Fact]
    public async Task Handle_RegisteredHandler_ReturnsTranslatedMessage()
    {
        var services = Factory.CreateServiceProvider(c => c.AddExceptionHandler<ValidationExceptionHandler>());
        var configurator = services.GetRequiredService<MediatorConfigurator>();
        var executor = services.GetExceptionHandlerExecutor(configurator.ExceptionHandlerCache, typeof(ValidationException));

        Assert.NotNull(executor);
        var result = await executor!.Handle(new ValidationException("boom"), services, CancellationToken.None);

        Assert.True(result.IsHandled);
        Assert.Equal(ValidationExceptionHandler.TranslatedMessage, result.Message);
    }

    #endregion

    #region Test substitution / removal

    [Fact]
    public async Task Handle_HandlerSubstitutedViaDiAfterConfiguratorRegistration_RunsTheSubstitute()
    {
        var services = Factory.CreateServiceProvider((c, sc) =>
        {
            c.AddExceptionHandler<ValidationExceptionHandler>();
            // Microsoft.Extensions.DependencyInjection uses the last registered service
            sc.AddTransient<IMediatorExceptionHandler<ValidationException>, FakeValidationExceptionHandler>();
        });
        var configurator = services.GetRequiredService<MediatorConfigurator>();
        var executor = services.GetExceptionHandlerExecutor(configurator.ExceptionHandlerCache, typeof(ValidationException));

        Assert.NotNull(executor);
        var result = await executor!.Handle(new ValidationException("boom"), services, CancellationToken.None);

        Assert.True(result.IsHandled);
        Assert.Equal(FakeValidationExceptionHandler.TranslatedMessage, result.Message);
    }

    [Fact]
    public async Task Handle_HandlerRemovedViaDiAfterConfiguratorRegistration_FallsBackToNotHandled()
    {
        var services = Factory.CreateServiceProvider((c, sc) =>
        {
            c.AddExceptionHandler<ValidationExceptionHandler>();
            sc.RemoveAll<IMediatorExceptionHandler<ValidationException>>();
        });
        var configurator = services.GetRequiredService<MediatorConfigurator>();
        var executor = services.GetExceptionHandlerExecutor(configurator.ExceptionHandlerCache, typeof(ValidationException));

        Assert.NotNull(executor);
        var result = await executor!.Handle(new ValidationException("boom"), services, CancellationToken.None);

        Assert.False(result.IsHandled);
    }

    #endregion

    #region Fixtures

    public class ValidationException(string message) : Exception(message);

    public class FieldValidationException(string message) : ValidationException(message);

    private class ValidationExceptionHandler : IMediatorExceptionHandler<ValidationException>
    {
        public const string TranslatedMessage = "Validation failed.";

        public Task<string> Handle(ValidationException exception, CancellationToken cancellationToken)
        {
            return Task.FromResult(TranslatedMessage);
        }
    }

    private class AnotherValidationExceptionHandler : IMediatorExceptionHandler<ValidationException>
    {
        public Task<string> Handle(ValidationException exception, CancellationToken cancellationToken)
        {
            return Task.FromResult("Other translation.");
        }
    }

    private class FakeValidationExceptionHandler : IMediatorExceptionHandler<ValidationException>
    {
        public const string TranslatedMessage = "Fake translation.";

        public Task<string> Handle(ValidationException exception, CancellationToken cancellationToken)
        {
            return Task.FromResult(TranslatedMessage);
        }
    }

    private class TimeoutExceptionHandler : IMediatorExceptionHandler<TimeoutException>
    {
        public Task<string> Handle(TimeoutException exception, CancellationToken cancellationToken)
        {
            return Task.FromResult("Timeout occurred.");
        }
    }

    private class MultiExceptionHandler : IMediatorExceptionHandler<ValidationException>, IMediatorExceptionHandler<TimeoutException>
    {
        Task<string> IMediatorExceptionHandler<ValidationException>.Handle(ValidationException exception, CancellationToken cancellationToken)
        {
            return Task.FromResult("Validation.");
        }

        Task<string> IMediatorExceptionHandler<TimeoutException>.Handle(TimeoutException exception, CancellationToken cancellationToken)
        {
            return Task.FromResult("Timeout.");
        }
    }

    private class ThrowingExceptionHandler : IMediatorExceptionHandler<ValidationException>
    {
        public Task<string> Handle(ValidationException exception, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Handler itself is broken.");
        }
    }

    private class NotAnExceptionHandler;

    #endregion
}
