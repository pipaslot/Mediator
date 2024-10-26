using System;

namespace Pipaslot.Mediator.Http.Configuration;

internal class NopCredibleProvider : ICredibleProvider
{
    public void VerifyCredibility(Type actionType)
    {
        //No operation
    }
}