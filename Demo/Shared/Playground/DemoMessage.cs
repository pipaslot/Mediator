using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Shared.Playground;

[AnonymousPolicy]
public record DemoMessage(bool Fail) : IMessage;