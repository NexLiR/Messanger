﻿using ChatServer.Core.Factories.Interfaces;
using ChatServer.Core.Factories;
using ChatServer.Core.Interfaces;
using ChatServer.Core.Net;
using ChatServer.Core.Net.ClientOperations.Interfaces;
using ChatServer.Core.Net.ClientOperations;
using ChatServer.Core.Services;
using ChatServer.Data;
using ChatServer.Data.Repositories;
using ChatServer.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterChatServerServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetConnectionString("DefaultConnection") == null)
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is missing in configuration");
            }

            services.AddDbContext<ChatDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();

            services.AddSingleton<IPacketReaderFactory, PacketReaderFactory>();
            services.AddSingleton<IPacketBuilderFactory, PacketBuilderFactory>();

            services.AddSingleton<ClientManager>();
            services.AddSingleton<IClientManager>(provider => provider.GetRequiredService<ClientManager>());

            services.AddSingleton<MessageHandlerService>();
            services.AddSingleton<IMessageHandler>(provider => provider.GetRequiredService<MessageHandlerService>());

            services.AddSingleton<MessageOperation>();
            services.AddSingleton<RegisterOperation>();
            services.AddSingleton<LoginOperation>();
            services.AddSingleton<IdentifyOperation>();
            services.AddSingleton<DisconnectOperation>();
            services.AddSingleton<LogoutOperation>();

            services.AddSingleton<IClientOperationFactory, ClientOperationFactory>();

            services.AddSingleton<IClientFactory, ClientFactory>();

            services.AddSingleton<Server>();

            services.AddSingleton<ServerService>();

            return services;
        }
    }
}