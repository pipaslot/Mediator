using Pipaslot.Mediator.Services;

namespace Pipaslot.Mediator.Server
{
    public class HttpMediator : Mediator
    {
        public string HttpMethod { get; }

        public HttpMediator(ServiceResolver handlerResolver, string httpMethod) : base(handlerResolver)
        {
            HttpMethod = httpMethod;
        }
    }
}
