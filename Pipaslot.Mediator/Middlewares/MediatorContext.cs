using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares.Features;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pipaslot.Mediator.Middlewares;

public class MediatorContext
{
    /// <summary>
    /// Unique context identifier
    /// </summary>
    private Guid? _guid;
    public Guid Guid => _guid ??= Guid.NewGuid();

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
    public MediatorContext[] ParentContexts => _contextAccessor.GetParentContexts();
    
    private readonly IMediatorContextAccessor _contextAccessor;
    internal IServiceProvider Services { get; }

    private object[]? _handlers;

    internal MediatorContext(IMediator mediator, IMediatorContextAccessor contextAccessor, IServiceProvider serviceProvider, IMediatorAction action,
        CancellationToken cancellationToken, object[]? handlers, IFeatureCollection? defaultFeatures)
    {
        Mediator = mediator;
        _contextAccessor = contextAccessor;
        Services = serviceProvider;
        Action = action ?? throw new ArgumentNullException(nameof(action));
        CancellationToken = cancellationToken;
        _handlers = handlers;
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
        var copy = new MediatorContext(Mediator, _contextAccessor, Services, Action, CancellationToken, _handlers, _features);
        return copy;
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

            if (!ContainsNotification(notification))
            {
                _results.Add(notification);
            }
        }
        else
        {
            _results.Add(result);
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

    /// <summary>
    /// Resolve all handlers for action execution
    /// </summary>
    /// <returns></returns>
    public object[] GetHandlers()
    {
        return _handlers ??= Services.GetActionHandlers(Action);
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