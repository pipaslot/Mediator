using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator
{
    public interface IPipelineRegistrator
    {
        /// <summary>
        /// Register action-specific pipeline applied only for actions implementing TActionMarker. The pipeline will be overwritten if already exists for the same action marker type.
        /// </summary>
        IConditionalPipelineConfigurator AddPipeline<TActionMarker>();

        /// <summary>
        /// Register default pipeline applied if no action-specific pipeline matches expected marker type.
        /// </summary>
        IConditionalPipelineConfigurator AddDefaultPipeline();
    }
}