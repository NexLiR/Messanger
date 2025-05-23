using ChatServer.Core.Interfaces;

namespace ChatServer.Core.Services.Handlers
{
    public class LoggingHandler : MessageHandlerBase
    {
        public override async Task HandleAsync(IClient client, string message)
        {
            Console.WriteLine($"Message from {client.UserName} successfully processed and broadcast");
            await base.HandleAsync(client, message);
        }
    }
}
