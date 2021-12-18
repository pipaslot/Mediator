using System;

namespace Pipaslot.Mediator.Http.Configuration
{
    internal class NopCredibleActionProvider : ICredibleActionProvider
    {
        public void VerifyCredibility(Type actionType)
        {
            //No operation
        }
    }
}
