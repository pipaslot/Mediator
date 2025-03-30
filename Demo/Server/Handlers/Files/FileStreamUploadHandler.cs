using Demo.Shared.Files;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Abstractions;

namespace Demo.Server.Handlers.Files;

public class FileStreamUploadHandler(IMediatorFacade mediator) : IMediatorHandler<FileStreamUpload>
{
    public Task Handle(FileStreamUpload action, CancellationToken cancellationToken)
    {
        mediator.AddInformationNotification($"File '{action.File.Name}' with size: {action.File.Content.Length}B was received.");
        return Task.CompletedTask;
    }
}