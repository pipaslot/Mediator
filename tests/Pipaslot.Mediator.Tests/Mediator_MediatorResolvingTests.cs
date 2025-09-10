using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator.Tests;

public class Mediator_MediatorResolvingTests
{
    /// <summary>
    /// Check that middleware will be resolved only when should be executed.
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task PreviousMiddlewareEndsProcessing_TestedMiddlewareWontBeConstructed()
    {
        var sut = Factory.CreateConfiguredMediator(m =>
        {
            m
                .Use<BlockingMiddleware>()
                .Use<TestedMiddleware>();
        });
        await sut.Dispatch(new ValidActions.NopMessage());
        await Assert.That(TestedMiddleware.WasConstructed).IsFalse();
    }

    public class BlockingMiddleware : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            //do not execute next middleware
            return Task.CompletedTask;
        }
    }

    public class TestedMiddleware : IMediatorMiddleware
    {
        internal static bool WasConstructed;

        public TestedMiddleware()
        {
            WasConstructed = true;
        }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            await next(context);
        }
    }
}