using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Shared.Playground;

[AnonymousPolicy]
public record DemoDownload(string FileName) : IMessage;