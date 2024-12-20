﻿using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Represents an async continuation for the next task to execute in the pipeline
/// </summary>
/// <returns>Awaitable task</returns>
public delegate Task MiddlewareDelegate(MediatorContext context);