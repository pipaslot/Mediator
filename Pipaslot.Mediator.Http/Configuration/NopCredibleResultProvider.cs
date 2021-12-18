using System;

namespace Pipaslot.Mediator.Http.Configuration
{
    internal class NopCredibleResultProvider : ICredibleResultProvider
    {
        public void VerifyCredibility(Type actionType)
        {
            //No operation
        }
    }
}
