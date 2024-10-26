using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Configuration;

public interface IActionTypeProvider
{
    ICollection<Type> GetActionTypes();
    ICollection<Type> GetMessageActionTypes();
    ICollection<Type> GetRequestActionTypes();
}