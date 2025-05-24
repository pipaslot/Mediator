using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.IO;
using System.Text;

namespace Pipaslot.Mediator.Http.Tests;

internal static class Factory
{
    public static IMediator CreateMediator(Action<IMediatorConfigurator> setup)
    {
        var services = CreateServiceProvider(c =>
            {
                c.AddActionsFromAssembly(typeof(Factory).Assembly)
                    .AddActionsFromAssemblyOf<SingleHandler.Message>()
                    .AddHandlersFromAssembly(typeof(Factory).Assembly)
                    .AddHandlersFromAssemblyOf<SingleHandler.MessageHandler>();
                setup(c);
            }
        );
        return services.GetRequiredService<IMediator>();
    }

    public static IServiceProvider CreateServiceProvider(Action<IMediatorConfigurator> setup)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        setup(collection.AddMediator());
        return collection.BuildServiceProvider();
    }
    
    
    #region Streams
    internal static Stream ConvertToStream(this string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    internal static string ConvertToString(this Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    #endregion
}