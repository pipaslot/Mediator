using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator
{
    public interface IPipelineRegistrator
    {
        /// <summary>
        /// Register pipeline only for actions implementing TActionMarker
        /// </summary>
        IConditionalPipelineConfigurator AddPipeline<TActionMarker>() where TActionMarker : IMediatorAction;

        /// <summary>
        /// Allways use this pipeline if not previous pipeline is applied.
        /// All next pipelines will be skipped! 
        /// MUST BE REGISTERED AS THE LAST PIPELINE!
        /// </summary>
        IConditionalPipelineConfigurator AddDefaultPipeline();
    }
}