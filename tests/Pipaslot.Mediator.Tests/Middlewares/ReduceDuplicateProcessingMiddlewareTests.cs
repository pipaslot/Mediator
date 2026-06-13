using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Middlewares;

[CollectionDefinition(nameof(ReduceDuplicateProcessingMiddlewareTests), DisableParallelization = true)]
public class ReduceDuplicateProcessingMiddlewareTestsCollection;

[Collection(nameof(ReduceDuplicateProcessingMiddlewareTests))]
public class ReduceDuplicateProcessingMiddlewareTests
{
    [Fact]
    public async Task SameHashConcurrentCalls_ShouldTriggerSingleBackendCall()
    {
        CountingActionHandler.Reset(holdFirstExecution: true);
        var mediator = CreateMediatorWithDedupMiddleware();

        var task1 = mediator.Execute(new CountingAction { Value = 10 });
        await CountingActionHandler.FirstExecutionStarted;
        var task2 = mediator.Execute(new CountingAction { Value = 10 });
        CountingActionHandler.ReleaseFirstExecution();

        await Task.WhenAll(task1, task2);
        var response1 = await task1;
        var response2 = await task2;

        Assert.True(response1.Success);
        Assert.True(response2.Success);
        Assert.Equal(response1.Result.Number, response2.Result.Number);
        Assert.Equal(1, CountingActionHandler.ExecutedCount);
    }

    [Fact]
    public async Task DifferentHashConcurrentCalls_ShouldTriggerTwoBackendCalls()
    {
        CountingActionHandler.Reset();
        var mediator = CreateMediatorWithDedupMiddleware();

        var task1 = mediator.Execute(new CountingAction { Value = 20 });
        var task2 = mediator.Execute(new CountingAction { Value = 21 });

        await Task.WhenAll(task1, task2);
        var response1 = await task1;
        var response2 = await task2;

        Assert.True(response1.Success);
        Assert.True(response2.Success);
        Assert.NotEqual(response1.Result.Number, response2.Result.Number);
        Assert.Equal(2, CountingActionHandler.ExecutedCount);
    }

    [Fact]
    public async Task CancelledFirstCall_SecondCallAfterCancellation_ShouldRunNewRequest()
    {
        CancellationRaceActionHandler.Reset();
        var mediator = CreateMediatorWithDedupMiddleware();
        var action = new CancellationRaceAction { Value = 30 };

        using var cts = new CancellationTokenSource();
        var first = mediator.Execute(action, cts.Token);

        await CancellationRaceActionHandler.FirstExecutionStarted;
        cts.Cancel();
        await CancellationRaceActionHandler.FirstCancellationObserved;
        CancellationRaceActionHandler.ReleaseFirstExecution();

        var firstResponse = await first;
        Assert.False(firstResponse.Success);

        var secondResponse = await mediator.Execute(action);
        Assert.True(secondResponse.Success);
        Assert.Equal(2, CancellationRaceActionHandler.ExecutedCount);
    }

    [Fact]
    public async Task CancelledFirstCall_SecondCallDuringCancellation_ShouldNotFailByForeignToken()
    {
        CancellationRaceActionHandler.Reset();
        var mediator = CreateMediatorWithDedupMiddleware();
        var action = new CancellationRaceAction { Value = 40 };

        using var cts = new CancellationTokenSource();
        var first = mediator.Execute(action, cts.Token);

        await CancellationRaceActionHandler.FirstExecutionStarted;
        cts.Cancel();
        await CancellationRaceActionHandler.FirstCancellationObserved;

        var second = mediator.Execute(action);
        CancellationRaceActionHandler.ReleaseFirstExecution();

        var firstResponse = await first;
        var secondResponse = await second;

        Assert.False(firstResponse.Success);
        Assert.True(secondResponse.Success);
    }

    [Fact]
    public async Task CancelledFirstCall_SecondAndThirdCallsDuringCancellation_ShouldNotFailByForeignToken()
    {
        CancellationRaceActionHandler.Reset();
        var mediator = CreateMediatorWithDedupMiddleware();
        var action = new CancellationRaceAction { Value = 50 };

        using var cts = new CancellationTokenSource();
        var first = mediator.Execute(action, cts.Token);

        await CancellationRaceActionHandler.FirstExecutionStarted;
        cts.Cancel();
        await CancellationRaceActionHandler.FirstCancellationObserved;

        var second = mediator.Execute(action);
        var third = mediator.Execute(action);
        CancellationRaceActionHandler.ReleaseFirstExecution();

        var firstResponse = await first;
        var secondResponse = await second;
        var thirdResponse = await third;

        Assert.False(firstResponse.Success);
        Assert.True(secondResponse.Success);
        Assert.True(thirdResponse.Success);
    }

    [Fact]
    public async Task CallCanceledByOwnToken_ShouldFail()
    {
        CancellationRaceActionHandler.Reset();
        var mediator = CreateMediatorWithDedupMiddleware();
        var action = new CancellationRaceAction { Value = 60 };

        using var cts = new CancellationTokenSource();
        var call = mediator.Execute(action, cts.Token);

        await CancellationRaceActionHandler.FirstExecutionStarted;
        cts.Cancel();
        await CancellationRaceActionHandler.FirstCancellationObserved;
        CancellationRaceActionHandler.ReleaseFirstExecution();

        var response = await call;
        Assert.False(response.Success);
    }

    [Fact]
    public async Task SingleCall_ShouldPropagateHandlerData()
    {
        var mediator = CreateMediatorWithDedupMiddleware();

        var response = await mediator.Execute(new PayloadAction { Value = 77 });

        Assert.True(response.Success);
        Assert.Equal("value-77", response.Result.Value);
    }

    #region Setup

    public class CountingAction : IMediatorAction<CountingActionResult>
    {
        public int Value { get; init; }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class CountingActionHandler : IMediatorHandler<CountingAction, CountingActionResult>
    {
        private static int _count;
        private static bool _holdFirstExecution;
        private static TaskCompletionSource<bool> _firstExecutionStarted = CreateSignal();
        private static TaskCompletionSource<bool> _releaseFirstExecution = CreateSignal();

        public static int ExecutedCount => _count;
        public static Task FirstExecutionStarted => _firstExecutionStarted.Task;

        public static void Reset(bool holdFirstExecution = false)
        {
            _count = 0;
            _holdFirstExecution = holdFirstExecution;
            _firstExecutionStarted = CreateSignal();
            _releaseFirstExecution = CreateSignal();
        }

        public static void ReleaseFirstExecution()
        {
            _releaseFirstExecution.TrySetResult(true);
        }

        public async Task<CountingActionResult> Handle(CountingAction action, CancellationToken cancellationToken)
        {
            var call = Interlocked.Increment(ref _count);

            if (_holdFirstExecution && call == 1)
            {
                _firstExecutionStarted.TrySetResult(true);
                await _releaseFirstExecution.Task;
            }

            return new CountingActionResult(call);
        }

        private static TaskCompletionSource<bool> CreateSignal()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    public record CountingActionResult(int Number);

    public class CancellationRaceAction : IMediatorAction<CancellationRaceActionResult>
    {
        public int Value { get; init; }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class CancellationRaceActionHandler : IMediatorHandler<CancellationRaceAction, CancellationRaceActionResult>
    {
        private static int _count;
        private static TaskCompletionSource<bool> _firstExecutionStarted = CreateSignal();
        private static TaskCompletionSource<bool> _firstCancellationObserved = CreateSignal();
        private static TaskCompletionSource<bool> _releaseFirstExecution = CreateSignal();

        public static int ExecutedCount => _count;
        public static Task FirstExecutionStarted => _firstExecutionStarted.Task;
        public static Task FirstCancellationObserved => _firstCancellationObserved.Task;

        public static void Reset()
        {
            _count = 0;
            _firstExecutionStarted = CreateSignal();
            _firstCancellationObserved = CreateSignal();
            _releaseFirstExecution = CreateSignal();
        }

        public static void ReleaseFirstExecution()
        {
            _releaseFirstExecution.TrySetResult(true);
        }

        public async Task<CancellationRaceActionResult> Handle(CancellationRaceAction action, CancellationToken cancellationToken)
        {
            var call = Interlocked.Increment(ref _count);
            if (call == 1)
            {
                _firstExecutionStarted.TrySetResult(true);
                using var registration = cancellationToken.Register(() => _firstCancellationObserved.TrySetResult(true));
                await _firstCancellationObserved.Task;
                await _releaseFirstExecution.Task;
                cancellationToken.ThrowIfCancellationRequested();
            }

            return new CancellationRaceActionResult(call);
        }

        private static TaskCompletionSource<bool> CreateSignal()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    public record CancellationRaceActionResult(int Number);

    public class PayloadAction : IMediatorAction<PayloadActionResult>
    {
        public int Value { get; init; }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class PayloadActionHandler : IMediatorHandler<PayloadAction, PayloadActionResult>
    {
        public Task<PayloadActionResult> Handle(PayloadAction action, CancellationToken cancellationToken)
        {
            return Task.FromResult(new PayloadActionResult($"value-{action.Value}"));
        }
    }

    public record PayloadActionResult(string Value);

    private static IMediator CreateMediatorWithDedupMiddleware()
    {
        return Factory.CreateConfiguredMediator(s =>
        {
            s.Use<ReduceDuplicateProcessingMiddleware>();
        });
    }

    #endregion
}