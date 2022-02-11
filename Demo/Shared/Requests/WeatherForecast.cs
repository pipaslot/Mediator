using Pipaslot.Mediator;
using System;

namespace Demo.Shared.Requests
{
    public static class WeatherForecast
    {
        public class Request : IRequest<Result[]>
        {
            public DateTime Date { get; set; } = DateTime.Now;

            public override int GetHashCode()
            {
                return Date.GetHashCode();
            }
        }
        public record RequestRecord : IRequest<Result[]>
        {
            public DateTime Date { get; set; } = DateTime.Now;
            public bool AttachNotification { get; set; }

            //Hash code do not need to be provided
        }

        public class Result
        {
            public DateTime Date { get; set; }

            public int TemperatureC { get; set; }

            public string? Summary { get; set; }

            public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        }
    }
}
