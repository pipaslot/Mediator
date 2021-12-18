using System;

namespace Pipaslot.Mediator.Http.Configuration
{
    public interface ICredibleActionProvider
    {
        void VerifyCredibility(Type actionType);
    }
}
