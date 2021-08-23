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
        public void AddDefaultPipeline_StandardPipelineAfterDefault_ThrowException()
        {
            Assert.Throws<MediatorConfigurationException>(() =>
            {
                Factory.CreateServiceResolver(c => c
                .AddDefaultPipeline()
                .AddPipeline<IRequest>());
            });

        }

        [Fact]
        public void AddDefaultPipeline_MultipleDefaultPipelines_ThrowException()
        {
            Assert.Throws<MediatorConfigurationException>(() =>
            {
                Factory.CreateServiceResolver(c => c
                .AddDefaultPipeline()
                .AddDefaultPipeline());
            });
        }


        [Fact]
        public void AddDefaultPipeline_MultipleDefaultPipelinesCombinedWithStandard_ThrowException()
        {
            Assert.Throws<MediatorConfigurationException>(() =>
            {
                Factory.CreateServiceResolver(c => c
                .AddDefaultPipeline()
                .AddPipeline<IRequest>()
                .AddDefaultPipeline());
            });
        }
    }
}
