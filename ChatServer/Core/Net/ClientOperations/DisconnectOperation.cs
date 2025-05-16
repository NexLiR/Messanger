using ChatServer.Core.Interfaces;
using ChatServer.Core.Net.ClientOperations.Interfaces;

namespace ChatServer.Core.Net.ClientOperations
{
    public class DisconnectOperation : IClientOperation
    {
        public Task ExecuteAsync(Client client, IPacketReader packetReader)
        {
            var username = packetReader.ReadMessage();
            Console.WriteLine($"[{DateTime.Now}]: User {username} requested disconnect");
            client.Disconnect();
            return Task.CompletedTask;
        }
    }
}
