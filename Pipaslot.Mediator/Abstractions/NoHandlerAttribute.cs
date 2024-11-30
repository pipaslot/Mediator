using Pipaslot.Mediator.Services;
using System;

namespace Pipaslot.Mediator.Abstractions;

/// <summary>
/// Mediator actions with this attribute won't throw exception from <see cref="HandlerExistenceChecker"/> because missing handler.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class NoHandlerAttribute : Attribute;