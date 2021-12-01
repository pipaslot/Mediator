﻿using Pipaslot.Mediator;
using Sample.Shared.Requests;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Server.Handlers
{
    public class WheatherForecastRequestHandler : IRequestHandler<WeatherForecast.Request, WeatherForecast.Result[]>
    {

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
            return Task.FromResult(forecast);
        }
    }
}
