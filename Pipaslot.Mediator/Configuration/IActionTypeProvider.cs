using System;

namespace Pipaslot.Mediator.Configuration
{
    public interface IActionTypeProvider
    {
        Type[] GetMessageActionTypes();
        Type[] GetRequestActionTypes();
    }
}
