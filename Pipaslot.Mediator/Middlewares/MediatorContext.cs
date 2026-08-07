using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares.Features;
using Pipaslot.Mediator.Middlewares.Handlers;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// State of a single action execution, passed through every middleware of the pipeline down to the handler - the
/// mediator's equivalent of ASP.NET Core's <c>HttpContext</c>.
/// </summary>
/// <remarks>
/// One instance per <see cref="IMediator"/> call. It carries the dispatched <see cref="Action"/>, the
/// <see cref="Results"/> collected so far, the <see cref="Status"/> deciding success or failure, and an
/// <see cref="Features"/> bag for passing data between middlewares without changing any signature. Middlewares mutate it
/// in place; there is no need (and no way) to return a modified copy from <see cref="IMediatorMiddleware.Invoke"/>.
/// <para>
/// Outside a middleware or handler, reach the current instance through <see cref="IMediatorContextAccessor"/> rather than
/// injecting this type. Inside a nested call the context is a new one whose <see cref="Depth"/> is greater than 1 and
/// whose <see cref="ParentContexts"/> lead to the root - results and notifications propagate up to the parent
/// automatically. In unit tests, build one with <see cref="Create"/> instead of mocking it.
/// </para>
/// </remarks>
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

    /// <summary>
    /// Outcome of the execution, deciding <see cref="IMediatorResponse.Success"/> for the caller.
    /// <see cref="ExecutionStatus.Succeeded"/> until a middleware or handler says otherwise - setting it to
    /// <see cref="ExecutionStatus.Failed"/> fails the action without reporting anything, leaving an
    /// <see cref="IMediator.ExecuteUnhandled{TResult}"/> caller with an empty <see cref="MediatorUnhandledErrorException"/>.
    /// Prefer <see cref="MediatorContextExtensions.AddError(MediatorContext, string, bool)"/>, which also adds a
    /// client-facing message, or <see cref="AddException"/>, which preserves an exception to rethrow.
    /// </summary>
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Succeeded;

    private readonly List<object> _results = new(1);

    /// <summary>
    /// Handler result objects and object collected during middleware processing
    /// </summary>
    public IReadOnlyCollection<object> Results => _results;

    private readonly List<Exception> _exceptions = new(0);

    /// <summary>
    /// Exceptions recorded via <see cref="AddException"/>. Kept separate from
    /// <see cref="Results"/> by construction: only <see cref="IMediator.DispatchUnhandled"/>/<see cref="IMediator.ExecuteUnhandled"/>
    /// read this collection (to rethrow the original exception instead of a generic wrapper) - it never reaches
    /// <see cref="IMediator.Dispatch"/>/<see cref="IMediator.Execute{TResult}"/> callers or gets serialized to a client.
    /// </summary>
    public IReadOnlyCollection<Exception> Exceptions => _exceptions;

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

    /// <summary>
    /// Mediator that started this execution. Use it for nested calls from a middleware.
    /// </summary>
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

    /// <summary>
    /// Creates a context outside a mediator execution, so that a custom middleware or exception handler can be
    /// invoked directly - against a real context rather than a mock - without building a service container and
    /// dispatching an action. Also usable for hosting middlewares outside the mediator.
    /// <see cref="Features"/> is arranged after creation, the same way a middleware would do it.
    /// </summary>
    /// <param name="action">Action the context is created for.</param>
    /// <param name="services">
    /// Services used to resolve handlers. When omitted, resolving any service throws <see cref="MediatorException"/>
    /// naming this method, instead of failing later with a less obvious error.
    /// </param>
    /// <param name="mediator">
    /// Value of <see cref="Mediator"/>. Defaults to the <see cref="IMediator"/> registered in <paramref name="services"/>,
    /// and when there is none, to a stub throwing on every call - so a nested call made without a test double reports itself.
    /// </param>
    /// <param name="cancellationToken">Value of <see cref="CancellationToken"/>.</param>
    /// <param name="depth">
    /// Nesting level. 1 = root execution; a greater value makes <see cref="IsNested"/> true without an actual nested
    /// call. <see cref="ParentContexts"/> stays empty either way - a synthesized context has no parent chain.
    /// </param>
    public static MediatorContext Create(IMediatorAction action, IServiceProvider? services = null, IMediator? mediator = null,
        CancellationToken cancellationToken = default, int depth = 1)
    {
        if (depth < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), depth, "Root execution starts at 1.");
        }

        var resolvedMediator = mediator
                               ?? services?.GetService(typeof(IMediator)) as IMediator
                               ?? DetachedMediator.Instance;
        var context = new MediatorContext(resolvedMediator, null, services ?? DetachedServiceProvider.Instance, new ReflectionCache(), action,
            cancellationToken, null, null);
        context.SetDepth(depth);

        return context;
    }

    /// <summary>
    /// Error messages collected in <see cref="Results"/> so far, as reported by middlewares and handlers.
    /// </summary>
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
    /// Register processing result.
    /// Adding an error-typed <see cref="Notification"/> here does not by itself change <see cref="Status"/> -
    /// use <see cref="MediatorContextExtensions.AddError(MediatorContext, string, bool)"/>/<see cref="MediatorContextExtensions.AddErrors"/> to both
    /// report an error message and fail the action.
    /// </summary>
    /// <param name="result"></param>
    public void AddResult(object result)
    {
        if (result is Notification notification)
        {
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
    
    /// <summary>
    /// Record an exception that <see cref="IMediator.DispatchUnhandled"/>/<see cref="IMediator.ExecuteUnhandled"/> should
    /// rethrow (or aggregate, if more than one is recorded) instead of wrapping it in a generic <see cref="MediatorUnhandledErrorException"/>.
    /// Sets <see cref="MediatorContext.Status"/> to <see cref="ExecutionStatus.Failed"/>. Does not add a <see cref="Notification"/>
    /// and never appears in <see cref="MediatorContext.Results"/> - unlike <see cref="MediatorContextExtensions.AddError(MediatorContext, string, bool)"/>,
    /// it is invisible to <see cref="IMediator.Dispatch"/>/<see cref="IMediator.Execute{TResult}"/> callers.
    /// </summary>
    /// <param name="exception">The original exception to preserve for the re-throw bridge</param>
    public void AddException(Exception exception)
    {
        Status = ExecutionStatus.Failed;
        _exceptions.Add(exception);
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