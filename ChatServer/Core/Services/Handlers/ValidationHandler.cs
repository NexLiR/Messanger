using ChatServer.Core.Interfaces;

namespace ChatServer.Core.Services.Handlers
{
    public class ValidationHandler : MessageHandlerBase
    {
        public override async Task HandleAsync(IClient client, string message)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            if (client.UID == Guid.Empty)
            {
                Console.WriteLine("Client has no UID assigned.");
                return;
            }

            await base.HandleAsync(client, message);
        }
    }
}
