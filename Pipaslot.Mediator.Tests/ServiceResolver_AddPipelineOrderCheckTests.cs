using Pipaslot.Mediator.Middlewares;
using System.Linq;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class ServiceResolver_AddPipelineOrderCheckTests
    {
        [Fact]
        public void AddPipeline_NoPipeline_Pass()
        {
            Factory.CreateServiceResolver(c => { });
        }

        [Fact]
        public void AddPipeline_StandardPipelineOnly_Pass()
        {
            Factory.CreateServiceResolver(c => c
                .AddPipeline<IRequest>());
        }

        [Fact]
        public void AddPipeline_MultipleStandardPipelines_OnlyTheLastOneIsUsed()
        {
            var sr = Factory.CreateServiceResolver(c => c
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
            Factory.CreateServiceResolver(c => c
                .AddDefaultPipeline());
        }

        [Fact]
        public void AddDefaultPipeline_SingleAsLast_Pass()
        {
            Factory.CreateServiceResolver(c => c
                .AddPipeline<IRequest>()
                .AddDefaultPipeline());
        }

        [Fact]
        public void AddDefaultPipeline_MultipleDefaultPipelines_ThrowException()
        {
            var sr = Factory.CreateServiceResolver(c => c
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
