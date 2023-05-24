﻿using Microsoft.Extensions.DependencyInjection;

namespace Pipaslot.Mediator
{
    /// <summary>
    /// Handler marked with this interface will be registered with <see cref="ServiceLifetime.Scoped"/> in Dependency Injection instead of Transient.
    /// </summary>
    public interface IScoped
    {
    }

}
