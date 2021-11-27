using System;

namespace Pipaslot.Mediator.Http.Contracts
{
    public class MediatorRequestDeserialized
    {
        public object? Content { get; }
        public Type? ActionType { get; }
        public string ObjectName { get; }

        public MediatorRequestDeserialized(object? content, Type? actionType, string? objectName)
        {
            Content = content;
            ActionType = actionType;
            ObjectName = objectName ?? string.Empty;
        }
    }
}
