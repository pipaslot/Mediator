using Pipaslot.Mediator;

namespace Demo.Shared.Requests
{
    public class DemoDownload : IMessage
    {
        public string FileName { get; init; }

        public DemoDownload(string fileName)
        {
            FileName = fileName;
        }
    }
}
