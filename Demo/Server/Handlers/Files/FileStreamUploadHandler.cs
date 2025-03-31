using Demo.Shared.Files;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Abstractions;

namespace Demo.Server.Handlers.Files;

public class FileStreamUploadHandler(IMediatorFacade mediator) : IMediatorHandler<FileStreamUpload>
{
    public Task Handle(FileStreamUpload action, CancellationToken cancellationToken)
    {
        foreach (var file in action.Files)
        {
            mediator.AddInformationNotification($"File '{file.Name}' with size: {file.Content.Length/1024}KB was received.");
        }
        
        return Task.CompletedTask;
    }
}