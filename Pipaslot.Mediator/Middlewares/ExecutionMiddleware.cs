using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Services;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Marks middleare as final/last which executes handlers. Pipeline ends with this milleware evenf it some next middlewares are registered.
    /// This interface was introduced to connect pipeline definitions and <see cref="Services.HandlerExistenceChecker"/>.
    /// </summary>
    public abstract class ExecutionMiddleware : IExecutionMiddleware
    {
        public abstract bool ExecuteMultipleHandlers { get; }
        protected abstract Task HandleMessage<TMessage>(TMessage message, MediatorContext context, CancellationToken cancellationToken);
        protected abstract Task HandleRequest<TRequest>(TRequest request, MediatorContext context, CancellationToken cancellationToken);

        private readonly IServiceProvider _serviceProvider;

        public ExecutionMiddleware(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));

            }
            if (action is IMediatorActionProvidingData e)
            {
                await HandleRequest(e, context, cancellationToken);
            }
            else
            {
                await HandleMessage((IMediatorAction)action, context, cancellationToken);
            }
        }

        /// <summary>
        /// Execute handler
        /// </summary>
        protected async Task ExecuteMessage<TMessage>(object handler, TMessage message, MediatorContext context, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var method = handler.GetType().GetMethod(nameof(IMediatorHandler<IMediatorAction>.Handle));
            try
            {
                await OnBeforeHandlerExecution(handler, message);
                var task = (Task?)method!.Invoke(handler, new object[] { message, cancellationToken })!;
                if (task != null)
                {
                    await task;
                }
                context.ExecutedHandlers++;
                await OnSuccessExecution(handler, message);

            }
            catch (TargetInvocationException e)
            {
                await OnFailedExecution(handler, message, e.InnerException ?? e);
                if (e.InnerException != null)
                {
                    // Unwrap exception
                    context.ErrorMessages.Add(e.InnerException.Message);
                    throw e.InnerException;
                }

                throw;
            }
            catch (Exception e)
            {
                context.ErrorMessages.Add(e.Message);
                throw;
            }
            finally
            {
                await OnAfterHandlerExecution(handler, message);
            }
        }

        /// <summary>
        /// Execute handler
        /// </summary>
        protected async Task ExecuteRequest<TRequest>(object handler, TRequest request, MediatorContext context, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var method = handler.GetType().GetMethod(nameof(IMediatorHandler<IMediatorAction<object>, object>.Handle));
            try
            {
                await OnBeforeHandlerExecution(handler, request);
                var task = (Task?)method!.Invoke(handler, new object[] { request, cancellationToken })!;
                if (task != null)
                {
                    await task.ConfigureAwait(false);

                    var resultProperty = task.GetType().GetProperty("Result");
                    var result = resultProperty?.GetValue(task);
                    context.ExecutedHandlers++;
                    if (result != null)
                    {
                        await OnSuccessExecution(handler, request);
                        context.Results.Add(result);
                    }
                }
            }
            catch (TargetInvocationException e)
            {
                await OnFailedExecution(handler, request, e.InnerException ?? e);
                if (e.InnerException != null)
                {
                    context.ErrorMessages.Add(e.InnerException.Message);
                    // Unwrap exception
                    throw e.InnerException;
                }

                throw;
            }
            catch (Exception e)
            {
                context.ErrorMessages.Add(e.Message);
                throw;
            }
            finally
            {
                await OnAfterHandlerExecution(handler, request);
            }
        }

        /// <summary>
        /// Resolve message handlers from service collection
        /// </summary>
        protected object[] GetMessageHandlers(Type? messageType)
        {
            return _serviceProvider.GetMessageHandlers(messageType);
        }

        /// <summary>
        /// Resolve request handlers from service collection
        /// </summary>
        protected object[] GetRequestHandlers(Type? requestType)
        {
            var resultType = RequestGenericHelpers.GetRequestResultType(requestType);
            return _serviceProvider.GetRequestHandlers(requestType, resultType);
        }

        /// <summary>
        /// Hook method called always before handler execution
        /// </summary>
        protected virtual Task OnBeforeHandlerExecution<TMessage>(object handler, TMessage request)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook method called always after handler execution
        /// </summary>
        protected virtual Task OnAfterHandlerExecution<TMessage>(object handler, TMessage request)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook called only after successful handler execution. Is omitted if exception is thrown
        /// </summary>
        /// <param name="handler">Request handler</param>
        /// <param name="request">Handler input data</param>
        /// <returns></returns>
        protected virtual Task OnSuccessExecution<TMessage>(object handler, TMessage request)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook called only if exception is thrown during handler execution
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="request"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnFailedExecution<TMessage>(object handler, TMessage request, Exception e)
        {
            return Task.CompletedTask;
        }

    }
}
