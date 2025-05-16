using ChatServer.Core.Interfaces;
using ChatServer.Core.Net.ClientOperations.Interfaces;

namespace ChatServer.Core.Net.ClientOperations
{
    public class MessageOperation : IClientOperation
    {
        private readonly IMessageHandler _messageHandler;

        public MessageOperation(IMessageHandler messageHandler)
        {
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        }

        public async Task ExecuteAsync(Client client, IPacketReader packetReader)
        {
            if (!client.IsAuthenticated)
            {
                client.SendAuthFailedMessage("You must authenticate first.");
                return;
            }

            var message = packetReader.ReadMessage();
            Console.WriteLine($"[{DateTime.Now}]: Message received from {client.UserName}: {message}");
            await _messageHandler.HandleMessageAsync(client, message);
        }
    }
}