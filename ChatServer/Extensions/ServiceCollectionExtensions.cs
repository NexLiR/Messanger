using ChatServer.Constants;
using ChatServer.Core.Interfaces;
using ChatServer.Core.Net;
using ChatServer.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterChatServerServices(this IServiceCollection services)
        {
            services.AddSingleton<ClientManager>();
            services.AddSingleton<IClientManager>(provider => provider.GetRequiredService<ClientManager>());

            services.AddSingleton<MessageHandlerService>();
            services.AddSingleton<IMessageHandler>(provider => provider.GetRequiredService<MessageHandlerService>());

            services.AddSingleton<ServerService>();

            return services;
        }
    }
}