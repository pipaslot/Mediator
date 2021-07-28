using Pipaslot.Mediator.Contracts;

namespace Pipaslot.Mediator.Client
{
    public class ClientMediatorOptions
    {
        public string Endpoint { get; set; } = MediatorRequestSerializable.Endpoint;
    }
}
