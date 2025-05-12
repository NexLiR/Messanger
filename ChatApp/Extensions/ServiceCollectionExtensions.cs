using Microsoft.Extensions.DependencyInjection;
using ChatApp.Core.Interfaces;
using ChatApp.Core.Net;
using ChatApp.Core.Services;
using ChatApp.Core.Services.Interfaces;
using ChatApp.MVVM.ViewModel;

namespace ChatApp.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterChatServices(this IServiceCollection services)
        {
            services.AddSingleton<IServerConnection, ServerConnection>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IUserService, UserService>();

            services.AddTransient<MainViewModel>();

            return services;
        }
    }
}
