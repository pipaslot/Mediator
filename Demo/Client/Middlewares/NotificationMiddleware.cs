using Pipaslot.Mediator.Middlewares;

namespace Demo.Client.Middlewares
{
    public class NotificationMiddleware : IMediatorMiddleware
    {
        public event EventHandler? MessagesHasChanged;
        private List<Message> _messages = new List<Message>();
        public IReadOnlyCollection<Message> Messages => _messages.OrderBy(m => m.Time).ToArray();

        private void Add(string title, string message)
        {
            var msg = new Message
            {
                Title = title,
                Content = message
            };
            _messages.Add(msg);
            MessagesHasChanged?.Invoke(this, new EventArgs());
            var timer = new System.Timers.Timer(5000);
            timer.Elapsed += (sender, args) => { Remove(msg); };
            timer.AutoReset = false;
            timer.Start();
        }
        public void Remove(Message message)
        {
            _messages.Remove(message);
            MessagesHasChanged?.Invoke(this, new EventArgs());
        }

        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            await next(context);
            foreach (var error in context.ErrorMessages)
            {
                Add(action?.GetType()?.ToString() ?? "", error);
            }
        }

        public class Message
        {
            public DateTime Time { get; init; } = DateTime.Now;
            public string Title { get; init; } = "";
            public string Content { get; init; } = "";
        }
    }
}
