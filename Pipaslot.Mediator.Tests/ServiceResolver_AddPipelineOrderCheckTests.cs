using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Services;
using System.Linq;
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
                    .UseConcurrentMultiHandler()
                .AddPipeline<IRequest>()
                    .Use<MultiHandlerSequenceExecutionMiddleware>()
                    );
            var pipeline = sr.GetPipeline(typeof(IRequest));
            var middleware = pipeline.FirstOrDefault();
            Assert.Equal(typeof(MultiHandlerSequenceExecutionMiddleware), middleware.GetType());
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
                    .UseConcurrentMultiHandler()
                .AddDefaultPipeline()
                    .Use<MultiHandlerSequenceExecutionMiddleware>()
                    );
            var pipeline = sr.GetPipeline(typeof(IRequest));
            var middleware = pipeline.FirstOrDefault();
            Assert.Equal(typeof(MultiHandlerSequenceExecutionMiddleware), middleware.GetType());
        }

    }
}
