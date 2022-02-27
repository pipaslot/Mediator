using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Services;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class ServiceResolver_AddPipelineOrderCheckTests
    {
        [Fact]
        public void AddPipeline_NoPipeline_Pass()
        {
            Factory.CreateServiceProvider(c => { });
        }

        [Fact]
        public void AddPipeline_StandardPipelineOnly_Pass()
        {
            Factory.CreateServiceProvider(c => c
                .AddPipeline<IRequest>());
        }

        [Fact]
        public void AddPipeline_MultipleStandardPipelines_OnlyTheLastOneIsUsed()
        {
            var sr = Factory.CreateServiceProvider(c => c
                .AddPipeline<IRequest>()
                    .Use<FakeMiddleware1>()
                .AddPipeline<IRequest>()
                    .Use<FakeMiddleware2>()
                    );
            var pipeline = sr.GetPipeline(new Request());
            var middleware = pipeline.FirstOrDefault();
            Assert.Equal(typeof(FakeMiddleware2), middleware.GetType());
        }

        [Fact]
        public void AddDefaultPipeline_DefaultPipelineOnly_Pass()
        {
            Factory.CreateServiceProvider(c => c
                .AddDefaultPipeline());
        }

        [Fact]
        public void AddDefaultPipeline_SingleAsLast_Pass()
        {
            Factory.CreateServiceProvider(c => c
                .AddPipeline<IRequest>()
                .AddDefaultPipeline());
        }

        [Fact]
        public void AddDefaultPipeline_MultipleDefaultPipelines_OnlyTheLastOneIsUsed()
        {
            var sr = Factory.CreateServiceProvider(c => c
                .AddDefaultPipeline()
                    .Use<FakeMiddleware1>()
                .AddDefaultPipeline()
                    .Use<FakeMiddleware2>()
                    );
            var pipeline = sr.GetPipeline(new Request());
            var middleware = pipeline.FirstOrDefault();
            Assert.Equal(typeof(FakeMiddleware2), middleware.GetType());
        }
        public class Request : IRequest<int> { }

        private class FakeMiddleware1 : IMediatorMiddleware
        {
            public Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                throw new System.NotImplementedException();
            }
        }

        private class FakeMiddleware2 : IMediatorMiddleware
        {
            public Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
