using Pipaslot.Mediator;

namespace Demo.Shared.Playground
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
