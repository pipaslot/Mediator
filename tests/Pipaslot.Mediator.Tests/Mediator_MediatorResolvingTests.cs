using Pipaslot.Mediator.Middlewares;
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
            var sut = Factory.CreateConfiguredMediator(m =>
            {
                m
                .Use<BlockingMiddleware>()
                .Use<TestedMiddleware>();
            });
            await sut.Dispatch(new ValidActions.NopMessage());
            Assert.False(TestedMiddleware.WasConstructed);
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
}
