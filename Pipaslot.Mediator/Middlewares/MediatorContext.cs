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
        /// Append result properties from context
        /// </summary>
        /// <param name="context"></param>
        public void Append(MediatorContext context)
        {
            AddErrors(context.ErrorMessages);
            AddResults(context.Results);
            ExecutedHandlers += context.ExecutedHandlers;
        }

        /// <summary>
        /// Append result properties from response
        /// </summary>
        /// <param name="response"></param>
        public void Append(IMediatorResponse response)
        {
            AddErrors(response.ErrorMessages);
            AddResults(response.Results);
        }

        public bool HasError()
        {
            return ErrorMessages.Any();
        }

        /// <summary>
        /// Register processing errors
        /// </summary>
        /// <param name="messages"></param>
        public void AddErrors(IEnumerable<string> messages)
        {
            foreach (var message in messages)
            {
                AddError(message);
            }
        }

        /// <summary>
        /// Register processing error
        /// </summary>
        /// <param name="message"></param>
        public void AddError(string message)
        {
            ErrorMessages.Add(message);
        }

        /// <summary>
        /// Register processing results
        /// </summary>
        /// <param name="result"></param>
        public void AddResults(IEnumerable<object> result)
        {
            Results.AddRange(result);
        }

        /// <summary>
        /// Register processing result
        /// </summary>
        /// <param name="result"></param>
        public void AddResult(object result)
        {
            Results.Add(result);
        }
    }
}
