using Pipaslot.Mediator;
using System;

namespace Sample.Shared.Requests
{
    public static class WeatherForecast
    {
        public class Request : IRequest<Result[]>
        {

        }

        public class Result
        {
            public DateTime Date { get; set; }

            public int TemperatureC { get; set; }

            public string Summary { get; set; }

            public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        }
    }
}
