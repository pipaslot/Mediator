using Pipaslot.Mediator;
using Demo.Shared.Requests;
using Pipaslot.Mediator.Notifications;

namespace Demo.Server.Handlers
{
    public class WheatherForecastRequestHandler : IRequestHandler<WeatherForecast.Request, WeatherForecast.Result[]>
    {
        private readonly INotificationProvider _notificationProvider;

        public WheatherForecastRequestHandler(INotificationProvider notificationProvider)
        {
            _notificationProvider = notificationProvider;
        }

        private static readonly string[] _summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public Task<WeatherForecast.Result[]> Handle(WeatherForecast.Request request, CancellationToken cancellationToken)
        {
            var rng = new Random();
            var forecast = Enumerable.Range(0, 4).Select(index => new WeatherForecast.Result
            {
                Date = request.Date.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = _summaries[rng.Next(_summaries.Length)]
            })
            .ToArray();
            if (request.AttachNotification)
            {
                _notificationProvider.AddSuccess("Enjoy the beautifull days", "Forecast");
            }
            return Task.FromResult(forecast);
        }
    }

    public class WheatherForecastRequestRecordHandler : IRequestHandler<WeatherForecast.RequestRecord, WeatherForecast.Result[]>
    {
        private readonly INotificationProvider _notificationProvider;

        public WheatherForecastRequestRecordHandler(INotificationProvider notificationProvider)
        {
            _notificationProvider = notificationProvider;
        }
        private static readonly string[] _summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public Task<WeatherForecast.Result[]> Handle(WeatherForecast.RequestRecord request, CancellationToken cancellationToken)
        {
            var rng = new Random();
            var forecast = Enumerable.Range(0, 4).Select(index => new WeatherForecast.Result
            {
                Date = request.Date.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = _summaries[rng.Next(_summaries.Length)]
            })
            .ToArray();

            if (request.AttachNotification)
            {
                _notificationProvider.AddSuccess("Enjoy the beautifull days", "Forecast");
            }
            return Task.FromResult(forecast);
        }
    }
}
