using ChatServer.Core.Interfaces;

namespace ChatServer.Core.Services
{
    public class MessageHandlerService : IMessageHandler
    {
        private readonly IClientManager _clientManager;

        public MessageHandlerService(IClientManager clientManager)
        {
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
        }

        public async Task HandleMessageAsync(IClient client, string message)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));

            var formattedMessage = $"[{DateTime.Now}]: [{client.UserName}]: {message}";
            await _clientManager.BroadcastMessageAsync(formattedMessage);
        }
    }
}