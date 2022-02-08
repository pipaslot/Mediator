using System;

namespace Pipaslot.Mediator.Configuration
{
    public interface IPipelineRegistrator
    {
        /// <summary>
        /// Register condition-specific pipeline applied only for actions matching this condition. The pipeline will be overwritten if already exists with the same identifier.
        /// </summary>
        /// <param name="identifier">Pipeline name as an unique identifier</param>
        /// <param name="actionCondition"></param>
        IConditionalPipelineConfigurator AddPipeline(string identifier, Func<Type, bool> actionCondition);

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