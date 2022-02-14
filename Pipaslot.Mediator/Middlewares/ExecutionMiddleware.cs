using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Services;
using System;
using System.Reflection;
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
        protected abstract Task HandleMessage(MediatorContext context);
        protected abstract Task HandleRequest(MediatorContext context);

        private readonly IServiceProvider _serviceProvider;

        public ExecutionMiddleware(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            if (context.Action is IMediatorActionProvidingData)
            {
                await HandleRequest(context);
            }
            else
            {
                await HandleMessage(context);
            }
        }

        /// <summary>
        /// Execute handler
        /// </summary>
        protected async Task ExecuteMessage(object handler, MediatorContext context)
        {
            var method = handler.GetType().GetMethod(nameof(IMediatorHandler<IMediatorAction>.Handle));
            try
            {
                await OnBeforeHandlerExecution(handler, context);
                var task = (Task?)method!.Invoke(handler, new object[] { context.Action, context.CancellationToken })!;
                if (task != null)
                {
                    await task;
                }
                context.ExecutedHandlers++;
                await OnSuccessExecution(handler, context);

            }
            catch (TargetInvocationException e)
            {
                await OnFailedExecution(handler, context, e.InnerException ?? e);
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
                await OnAfterHandlerExecution(handler, context);
            }
        }

        /// <summary>
        /// Execute handler
        /// </summary>
        protected async Task ExecuteRequest(object handler, MediatorContext context)
        {
            var method = handler.GetType().GetMethod(nameof(IMediatorHandler<IMediatorAction<object>, object>.Handle));
            try
            {
                await OnBeforeHandlerExecution(handler, context);
                var task = (Task?)method!.Invoke(handler, new object[] { context.Action, context.CancellationToken })!;
                if (task != null)
                {
                    await task.ConfigureAwait(false);

                    var resultProperty = task.GetType().GetProperty("Result");
                    var result = resultProperty?.GetValue(task);
                    context.ExecutedHandlers++;
                    if (result != null)
                    {
                        await OnSuccessExecution(handler, context);
                        context.Results.Add(result);
                    }
                }
            }
            catch (TargetInvocationException e)
            {
                await OnFailedExecution(handler, context, e.InnerException ?? e);
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
                await OnAfterHandlerExecution(handler, context);
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
        protected virtual Task OnBeforeHandlerExecution(object handler, MediatorContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook method called always after handler execution
        /// </summary>
        protected virtual Task OnAfterHandlerExecution(object handler, MediatorContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook called only after successful handler execution. Is omitted if exception is thrown
        /// </summary>
        /// <param name="handler">Request handler</param>
        /// <param name="request">Handler input data</param>
        /// <returns></returns>
        protected virtual Task OnSuccessExecution(object handler, MediatorContext context)
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
        protected virtual Task OnFailedExecution(object handler, MediatorContext context, Exception e)
        {
            return Task.CompletedTask;
        }

    }
}
