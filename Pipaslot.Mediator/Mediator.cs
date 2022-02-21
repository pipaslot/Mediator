using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator
{
    /// <summary>
    /// Mediator which wrapps handler execution into pipelines
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ConcurrentDictionary<Guid, IMediatorAction> _runningActions = new();

        public event EventHandler<ActionStartedEventArgs>? ActionStarted;
        public event EventHandler<ActionCompletedEventArgs>? ActionCompleted;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IMediatorResponse> Dispatch(IMediatorAction message, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceProvider.GetPipeline(message.GetType());
            var context = CreateContext(message, cancellationToken);
            var guid = AddToQueue(message);
            try
            {
                await ProcessPipeline(pipeline, context);

                var success = context.ErrorMessages.Count == 0;
                return new MediatorResponse(success, context.Results, context.UniqueErrorMessages);
            }
            catch (Exception e)
            {
                context.ErrorMessages.Add(e.Message);
                return new MediatorResponse(false, context.Results, context.UniqueErrorMessages);
            }
            finally
            {
                RemoveFromQueue(guid);
            }
        }

        public async Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceProvider.GetPipeline(message.GetType());
            var context = CreateContext(message, cancellationToken);
            var guid = AddToQueue(message);
            try
            {
                await ProcessPipeline(pipeline, context);

                if (context.ErrorMessages.Any())
                {
                    throw MediatorExecutionException.CreateForUnhandledError(context);
                }
            }
            finally
            {
                RemoveFromQueue(guid);
            }
        }

        public async Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceProvider.GetPipeline(request.GetType());
            var context = CreateContext(request, cancellationToken);
            var guid = AddToQueue(request);
            try
            {
                await ProcessPipeline(pipeline, context);

                var success = context.ErrorMessages.Count == 0 && context.Results.Any(r => r is TResult);
                return new MediatorResponse<TResult>(success, context.Results, context.UniqueErrorMessages);
            }
            catch (Exception e)
            {
                context.ErrorMessages.Add(e.Message);
                return new MediatorResponse<TResult>(false, context.Results, context.UniqueErrorMessages);
            }
            finally
            {
                RemoveFromQueue(guid);
            }
        }

        public async Task<TResult> ExecuteUnhandled<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceProvider.GetPipeline(request.GetType());
            var context = CreateContext(request, cancellationToken);
            var guid = AddToQueue(request);
            try
            {
                await ProcessPipeline(pipeline, context);

                var success = context.ErrorMessages.Count == 0 && context.Results.Any(r => r is TResult);
                if (context.ErrorMessages.Any())
                {
                    throw MediatorExecutionException.CreateForUnhandledError(context);
                }
                var result = context.Results
                    .Where(r => r is TResult)
                    .Cast<TResult>()
                    .FirstOrDefault();
                if (result == null)
                {
                    throw new MediatorExecutionException($"No result matching type {typeof(TResult)} was returned from pipeline", context);
                }
                return result;
            }
            finally
            {
                RemoveFromQueue(guid);
            }
        }

        private async Task ProcessPipeline(IEnumerable<IMediatorMiddleware> pipeline, MediatorContext context)
        {
            var enumerator = pipeline.GetEnumerator();
            Task next(MediatorContext ctx)
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current.Invoke(ctx, next);
                }
                return Task.CompletedTask;
            };
            await next(context);
        }

        private MediatorContext CreateContext(IMediatorAction action, CancellationToken cancellationToken)
        {
            return new MediatorContext(action, cancellationToken);
        }

        private Guid AddToQueue(IMediatorAction action)
        {
            var guid = Guid.NewGuid();
            _runningActions.TryAdd(guid, action);
            var runningActions = _runningActions.Values.ToArray();
            ActionStarted?.Invoke(this, new ActionStartedEventArgs(action, runningActions));
            return guid;
        }

        private void RemoveFromQueue(Guid guid)
        {
            _runningActions.TryRemove(guid, out var action);
            var runningActions = _runningActions.Values.ToArray();
            ActionCompleted?.Invoke(this, new ActionCompletedEventArgs(action, runningActions));
        }
    }
}