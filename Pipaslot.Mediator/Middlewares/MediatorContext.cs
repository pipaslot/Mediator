using Pipaslot.Mediator.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pipaslot.Mediator.Middlewares
{
    public class MediatorContext
    {
        /// <summary>
        /// Contains number of executed handlers set by Execution middleware 
        /// </summary>
        internal int ExecutedHandlers { get; set; }

        internal string[] UniqueErrorMessages => ErrorMessages.Distinct().ToArray();

        /// <summary>
        /// Handler error message and error messages colelcted during middleware processing
        /// </summary>
        public List<string> ErrorMessages { get; } = new List<string>();

        /// <summary>
        /// Handler result objects and object collected during middleware processing
        /// </summary>
        public List<object> Results { get; } = new List<object>(1);

        /// <summary>
        /// Executed/Dispatched action
        /// </summary>
        public IMediatorAction Action { get; }

        /// <summary>
        /// Unique action identifier
        /// </summary>
        public string ActionIdentifier => Action.GetType().ToString();

        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken CancellationToken { get; }

        internal MediatorContext(IMediatorAction action, CancellationToken cancellationToken)
        {
            Action = action ?? throw new System.ArgumentNullException(nameof(action));
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Copy context without result data
        /// </summary>
        /// <returns></returns>
        public MediatorContext CopyEmpty()
        {
            var copy = new MediatorContext(Action, CancellationToken);
            return copy;
        }

        /// <summary>
        /// Copy context without result data and custom action
        /// </summary>
        /// <returns></returns>
        public MediatorContext CopyWith(IMediatorAction action)
        {
            var copy = new MediatorContext(action, CancellationToken);
            return copy;
        }

        /// <summary>
        /// Append result properties from context
        /// </summary>
        /// <param name="context"></param>
        public void Append(MediatorContext context)
        {
            ErrorMessages.AddRange(context.ErrorMessages);
            Results.AddRange(context.Results);
            ExecutedHandlers += context.ExecutedHandlers;
        }

        /// <summary>
        /// Append result properties from response
        /// </summary>
        /// <param name="response"></param>
        public void Append(IMediatorResponse response)
        {
            ErrorMessages.AddRange(response.ErrorMessages);
            Results.AddRange(response.Results);
        }
    }
}
