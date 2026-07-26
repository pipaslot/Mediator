using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares.Features;
using Pipaslot.Mediator.Middlewares.Handlers;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pipaslot.Mediator.Middlewares;

public class MediatorContext
{
    private Guid? _guid;

    /// <summary>
    /// Unique context identifier
    /// </summary>
    public Guid Guid => _guid ??= CreateGuid();

    private static Guid CreateGuid()
    {
        #if NET9_0_OR_GREATER
        return Guid.CreateVersion7();
        #else
        return Guid.NewGuid();
        #endif
    }

public ExecutionStatus Status { get; set; } = ExecutionStatus.Succeeded;

    private readonly List<object> _results = new(1);

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
    public string ActionIdentifier => Action.GetActionName();

    /// <summary>
    /// Returns true for Request types and false for Message types
    /// </summary>
    public bool HasActionReturnValue => Action is IMediatorActionProvidingData;

    /// <summary>
    /// Cancellation token
    /// </summary>
    public CancellationToken CancellationToken { get; private set; }

    private IFeatureCollection? _features;
    
    private static readonly IFeatureCollection _defaultFeatures = CreateDefaultFeatures();

    private static IFeatureCollection CreateDefaultFeatures()
    {
        var featureCollection = new FeatureCollection();
        featureCollection.Set(MiddlewareParametersFeature.Default);
        return featureCollection;
    }

    /// <inheritdoc cref="IFeatureCollection"/>
    public IFeatureCollection Features => _features ??= new FeatureCollection(_defaultFeatures);
    
    internal bool FeaturesAreInitialized => _features is not null;

    public IMediator Mediator { get; }

    /// <summary>
    /// Parent action contexts.
    /// Will be empty if current action is executed independently.
    /// Will contain parent contexts of actions which executed current action as nested call.
    /// The last member is always the root action.
    /// </summary>
    public MediatorContext[] ParentContexts => _contextAccessor?.GetParentContexts() ?? [];

    /// <summary>
    /// Nesting level of the current execution. 1 = root execution, 2 = first nesting level, and so on.
    /// </summary>
    public int Depth { get; private set; } = 1;

    /// <summary>
    /// True when this execution was started from within another mediator execution (i.e. <see cref="Depth"/> is greater than 1).
    /// </summary>
    public bool IsNested => Depth > 1;
    
    private readonly IMediatorContextAccessor? _contextAccessor;
    internal readonly IServiceProvider Services;

    private readonly ReflectionCache _reflectionCache;
    private HandlerExecutor? _handlerExecutor;

    internal MediatorContext(IMediator mediator, IMediatorContextAccessor? contextAccessor, IServiceProvider serviceProvider, ReflectionCache reflectionCache, IMediatorAction action,
        CancellationToken cancellationToken, HandlerExecutor? handlerExecutor, IFeatureCollection? defaultFeatures)
    {
        Mediator = mediator;
        _contextAccessor = contextAccessor;
        Services = serviceProvider;
        _reflectionCache = reflectionCache;
        Action = action ?? throw new ArgumentNullException(nameof(action));
        CancellationToken = cancellationToken;
        _handlerExecutor = handlerExecutor;
        _features = defaultFeatures;
    }

    public IEnumerable<string> ErrorMessages => _results
        .GetNotifications()
        .GetErrorMessages();

    /// <summary>
    /// Copy context without result data
    /// </summary>
    /// <returns></returns>
    public MediatorContext CopyEmpty()
    {
        var copy = new MediatorContext(Mediator, _contextAccessor, Services, _reflectionCache, Action, CancellationToken, _handlerExecutor, _features);
        copy.Depth = Depth;
        return copy;
    }

    /// <summary>
    /// Sets the nesting depth once resolved by the pipeline construction.
    /// </summary>
    internal void SetDepth(int depth)
    {
        Depth = depth;
    }

    /// <summary>
    /// Register processing result
    /// </summary>
    /// <param name="result"></param>
    public void AddResult(object result)
    {
        if (result is Notification notification)
        {
            if (notification.Type.IsError())
            {
                Status = ExecutionStatus.Failed;
            }

            AppendNotification(notification);
        }
        else
        {
            _results.Add(result);
        }
    }

    /// <summary>
    /// Registers a notification forwarded from a nested/child context's own Results (used by <see cref="NotificationPropagationMiddleware"/>).
    /// Unlike <see cref="AddResult"/>, this never mutates <see cref="Status"/> - a descendant context's already-resolved outcome
    /// must not silently flip this context's own status; only this context's own local AddResult/AddError calls do that.
    /// </summary>
    internal void AddForwardedNotification(Notification notification)
    {
        AppendNotification(notification);
    }

    private void AppendNotification(Notification notification)
    {
        if (!ContainsNotification(notification))
        {
            _results.Add(notification);
        }
    }

    private bool ContainsNotification(Notification notification)
    {
        foreach (var res in Results)
        {
            if (res is Notification n && n.Equals(notification))
            {
                return true;
            }
        }

        return false;
    }

    internal HandlerExecutor GetHandlerExecutor()
    {
        if (_handlerExecutor == null)
        {
            var actionType = Action.GetType();
            _handlerExecutor = Services.GetHandlerExecutor(_reflectionCache ,actionType);
        }
        return _handlerExecutor;
    }

    /// <summary>
    /// Resolve all handlers for action execution
    /// </summary>
    /// <returns></returns>
    public object[] GetHandlers()
    {
        return GetHandlerExecutor().GetHandlers(Services);
    }

    /// <summary>
    /// Replace actual cancellation token by own one. 
    /// Can be used as hooking to application events to cancel operations relevant for abandoned pages/requests.
    /// </summary>
    public void SetCancellationToken(CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
    }
}