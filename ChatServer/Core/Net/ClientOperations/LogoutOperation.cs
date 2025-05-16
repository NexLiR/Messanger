using ChatServer.Core.Interfaces;
using ChatServer.Core.Net.ClientOperations.Interfaces;

namespace ChatServer.Core.Net.ClientOperations
{
    public class LogoutOperation : IClientOperation
    {
        public Task ExecuteAsync(Client client, IPacketReader packetReader)
        {
            var username = packetReader.ReadMessage();
            Console.WriteLine($"[{DateTime.Now}]: User {username} logged out");
            client.SetAuthenticated(false);
            client.SetUser(null);
            return Task.CompletedTask;
        }
    }
}
