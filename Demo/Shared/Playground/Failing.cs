using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Shared.Playground;

public static class Failing
{
    [AnonymousPolicy]
    public class Request : IRequest<Result>;

    [AnonymousPolicy]
    public class Message : IMessage;

    public class Result;
}