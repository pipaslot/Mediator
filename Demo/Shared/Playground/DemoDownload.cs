using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Http;

namespace Demo.Shared.Playground;

[AnonymousPolicy]
public record DemoDownload(string FileName) : IRequest<IMediatorHttpResult>;