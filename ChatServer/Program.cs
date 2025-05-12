using ChatServer.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChatServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.RegisterChatServerServices();
                })
                .Build();

            await host.Services.GetRequiredService<Core.Services.ServerService>().StartAsync();
        }
    }
}