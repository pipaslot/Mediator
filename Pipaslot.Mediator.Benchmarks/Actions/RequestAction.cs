using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Benchmarks.Actions;

internal record RequestAction(string Message) : IMediatorAction<string>;