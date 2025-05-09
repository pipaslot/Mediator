using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;

namespace Pipaslot.Mediator.Benchmarks.Actions;

[AnonymousPolicy]
internal record RequestAction(string Message) : IMediatorAction<RequestActionResult>;

internal record RequestActionResult(string Message);