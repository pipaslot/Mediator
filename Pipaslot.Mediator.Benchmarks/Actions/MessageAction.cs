using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;

namespace Pipaslot.Mediator.Benchmarks.Actions;

[AnonymousPolicy]
internal record MessageAction() : IMediatorAction;