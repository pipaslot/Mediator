using Demo.Shared;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers;

public class WeatherForecastRequestHandler : IRequestHandler<WeatherForecast.Request, WeatherForecast.IResult[]>
{
    private static readonly string[] _summaries = {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public Task<WeatherForecast.IResult[]> Handle(WeatherForecast.Request request, CancellationToken cancellationToken)
    {
        var rng = new Random();
        var forecast = Enumerable.Range(0, 4).Select(index => (WeatherForecast.IResult)new WeatherForecast.Result
            {
                Date = request.Date.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = _summaries[rng.Next(_summaries.Length)]
            })
            .ToArray();
        return Task.FromResult(forecast);
    }
}

public class WeatherForecastRequestRecordHandler : IRequestHandler<WeatherForecast.RequestRecord, WeatherForecast.Result[]>
{
    private static readonly string[] _summaries = {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public Task<WeatherForecast.Result[]> Handle(WeatherForecast.RequestRecord request, CancellationToken cancellationToken)
    {
        var rng = new Random();
        var forecast = Enumerable.Range(0, 4).Select(index => new WeatherForecast.Result
            {
                Date = request.Date.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = _summaries[rng.Next(_summaries.Length)]
            })
            .ToArray();
        return Task.FromResult(forecast);
    }
}