using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Client;
using Sample.Shared;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sample.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            //////// Mediator implementation
            builder.Services.AddMediatorClient(o => {
                o.Endpoint = Constants.CustomMediatorUrl;
            });
            ////////

            await builder.Build().RunAsync();
        }
    }
}
