using System;

namespace Pipaslot.Mediator
{
    /// <summary>
    /// Specify Pipeline and marker type for which should be applied
    /// </summary>
    internal class PipelineDefinition
    {
        public PipelineDefinition(Type pipelineType, Type? markerType = null)
        {
            PipelineType = pipelineType;
            MarkerType = markerType;
        }

        public Type PipelineType { get;  }
        /// <summary>
        /// Class or interface which needs to be implemented by Request object to be apply the pipeline for. Will be applied always if is null.
        /// </summary>
        public Type? MarkerType { get;  }
    }
}
