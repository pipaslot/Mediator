using Pipaslot.Mediator.Middlewares;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class Mediator_MediatorResolvingTests
    {
        /// <summary>
        /// Check that middleware will be resolved only when should be executed.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PreviousMiddlewareEndsProcessing_TestedMiddlewareWontBeConstructed()
        {
            var sut = Factory.CreateMediator(m =>
            {
                m.AddDefaultPipeline()
                .Use<BlockingMiddleware>()
                .Use<TestedMiddleware>();
            });
            await sut.Dispatch(new ValidActions.NopMessage());
            Assert.False(TestedMiddleware.WasConstructed);
        }

        public class BlockingMiddleware : IMediatorMiddleware
        {
            public Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
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

            public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
            {
                await next(context);
            }
        }
    }
}
