using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pipaslot.Mediator.Middlewares
{
    public class MediatorContext
    {
        /// <summary>
        /// Unique context identifier
        /// </summary>
        public Guid Guid { get; } = Guid.NewGuid();

        public ExecutionStatus Status { get; set; } = ExecutionStatus.Succeeded;
        [Obsolete]
        private List<string> _errorMessages = new List<string>();
        /// <summary>
        /// Handler error message and error messages colelcted during middleware processing
        /// </summary>
        [Obsolete("Will be replaced by Notifications. For error detection use Status property")]
        public IReadOnlyCollection<string> ErrorMessages => _errorMessages;

        private List<object> _results = new List<object>(1);
        /// <summary>
        /// Handler result objects and object collected during middleware processing
        /// </summary>
        public IReadOnlyCollection<object> Results => _results;

        /// <summary>
        /// Executed/Dispatched action
        /// </summary>
        public IMediatorAction Action { get; }

        /// <summary>
        /// Unique action identifier
        /// </summary>
        public string ActionIdentifier => Action.GetType().ToString();

        /// <summary>
        /// Returns true for Request types and false for Message types
        /// </summary>
        public bool HasActionReturnValue => Action is IMediatorActionProvidingData;

        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken CancellationToken { get; }

        public IMediator Mediator { get; }

        /// <summary>
        /// Parent action contexts. 
        /// Will be empty if current action is executed independently. 
        /// Will contain parent contexts of actions which executed current action as nested call. 
        /// First member is always the root action.
        /// </summary>
        public MediatorContext[] ParentContexts => _contextAccessor.ContextStack.Skip(1).ToArray();

        private readonly IMediatorContextAccessor _contextAccessor;
        internal IServiceProvider Services { get; }

        private object[]? _handlers = null;

        internal MediatorContext(IMediator mediator, IMediatorContextAccessor contextAccessor, IServiceProvider serviceProvider, IMediatorAction action, CancellationToken cancellationToken, object[]? handlers = null)
        {
            Mediator = mediator;
            Services = serviceProvider;
            Action = action ?? throw new System.ArgumentNullException(nameof(action));
            CancellationToken = cancellationToken;
            _handlers = handlers;
        }

        /// <summary>
        /// Copy context without result data
        /// </summary>
        /// <returns></returns>
        public MediatorContext CopyEmpty()
        {
            var copy = new MediatorContext(Mediator, _contextAccessor, Services, Action, CancellationToken, _handlers);
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
            return Status != ExecutionStatus.Succeeded;
        }

        /// <summary>
        /// Register processing errors. Ignores duplicate entries.
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
        /// Register processing error. Ignores duplicate entries.
        /// </summary>
        /// <param name="message"></param>
        public void AddError(string message)
        {
            Status = ExecutionStatus.Failed;
            if (!_errorMessages.Contains(message))
            {
                _errorMessages.Add(message);
            }
        }

        /// <summary>
        /// Register processing results
        /// </summary>
        /// <param name="result"></param>
        public void AddResults(IEnumerable<object> result)
        {
            _results.AddRange(result);
        }

        /// <summary>
        /// Register processing result
        /// </summary>
        /// <param name="result"></param>
        public void AddResult(object result)
        {
            _results.Add(result);
        }

        /// <summary>
        /// Resolve all handlers for action execution
        /// </summary>
        /// <returns></returns>
        public object[] GetHandlers()
        {
            if (_handlers == null)
            {
                _handlers = Services.GetActionHandlers(Action);
            }
            return _handlers;
        }
    }
}
