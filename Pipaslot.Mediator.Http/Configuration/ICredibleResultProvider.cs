using System;

namespace Pipaslot.Mediator.Http.Configuration
{
    public interface ICredibleResultProvider
    {
        void VerifyCredibility(Type resultType);
    }
}
