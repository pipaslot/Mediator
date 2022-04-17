using System;

namespace Pipaslot.Mediator.Http.Configuration
{
    public interface ICredibleProvider
    {
        void VerifyCredibility(Type actionType);
    }
}
