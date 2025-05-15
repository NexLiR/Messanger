using ChatServer.Constants;
using ChatServer.Core.Interfaces;
using ChatServer.Core.Net.IO;

namespace ChatServer.Core.Net
{
    public class ClientManager : IClientManager
    {
        private readonly List<IClient> _clients = new List<IClient>();
        private readonly object _lock = new object();

        public IReadOnlyList<IClient> Clients => _clients.AsReadOnly();

        public void AddClient(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            lock (_lock)
            {
                _clients.Add(client);
            }

            Console.WriteLine($"[{DateTime.Now}]: Client {client.UserName} added to client manager. Active clients: {_clients.Count}");
        }

        public void RemoveClient(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            lock (_lock)
            {
                _clients.Remove(client);
            }

            Console.WriteLine($"[{DateTime.Now}]: Client {client.UserName} removed from client manager. Active clients: {_clients.Count}");
        }

        public async Task BroadcastConnectionAsync()
        {
            IReadOnlyList<IClient> clientsCopy;

            lock (_lock)
            {
                clientsCopy = _clients.ToList().AsReadOnly();
            }

            foreach (var receiver in clientsCopy)
            {
                foreach (var sender in clientsCopy)
                {
                    if (!receiver.ClientSocket.Connected)
                    {
                        Console.WriteLine($"[{DateTime.Now}]: Cannot broadcast to {receiver.UserName} - client disconnected");
                        continue;
                    }

                    try
                    {
                        using var broadcastPacket = new PacketBuilder();
                        broadcastPacket.WriteOpCode(OpCodes.Connected);
                        broadcastPacket.WriteMessage(sender.UserName);
                        broadcastPacket.WriteMessage(sender.UID.ToString());

                        await receiver.ClientSocket.Client.SendAsync(
                            broadcastPacket.GetPacketBytes(),
                            System.Net.Sockets.SocketFlags.None);

                        Console.WriteLine($"[{DateTime.Now}]: Sent user {sender.UserName} info to {receiver.UserName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now}]: Error broadcasting connection of {sender.UserName} to {receiver.UserName}: {ex.Message}");
                    }
                }
            }
        }

        public async Task BroadcastMessageAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));

            IReadOnlyList<IClient> clientsCopy;

            lock (_lock)
            {
                clientsCopy = _clients.ToList().AsReadOnly();
            }

            int successCount = 0;
            foreach (var client in clientsCopy)
            {
                if (!client.ClientSocket.Connected)
                {
                    Console.WriteLine($"[{DateTime.Now}]: Cannot broadcast message to {client.UserName} - client disconnected");
                    continue;
                }

                try
                {
                    using var messagePacket = new PacketBuilder();
                    messagePacket.WriteOpCode(OpCodes.Message);
                    messagePacket.WriteMessage(message);

                    await client.ClientSocket.Client.SendAsync(
                        messagePacket.GetPacketBytes(),
                        System.Net.Sockets.SocketFlags.None);

                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now}]: Error broadcasting message to {client.UserName}: {ex.Message}");
                }
            }

            Console.WriteLine($"[{DateTime.Now}]: Message broadcast to {successCount} of {clientsCopy.Count} clients");
        }

        public async Task BroadcastDisconnectAsync(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                throw new ArgumentException("UID cannot be null or empty", nameof(uid));

            IClient disconnectedClient = null;
            IReadOnlyList<IClient> remainingClients;

            lock (_lock)
            {
                disconnectedClient = _clients.FirstOrDefault(u => u.UID.ToString() == uid);

                if (disconnectedClient != null)
                {
                    _clients.Remove(disconnectedClient);
                    Console.WriteLine($"[{DateTime.Now}]: Removed disconnected client {disconnectedClient.UserName} from client list");
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now}]: Could not find client with UID {uid} to remove");
                }

                remainingClients = _clients.ToList().AsReadOnly();
            }

            if (disconnectedClient == null)
                return;

            int successCount = 0;
            foreach (var client in remainingClients)
            {
                if (!client.ClientSocket.Connected)
                {
                    Console.WriteLine($"[{DateTime.Now}]: Cannot broadcast disconnect to {client.UserName} - client disconnected");
                    continue;
                }

                try
                {
                    using var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(OpCodes.Disconnected);
                    broadcastPacket.WriteMessage(uid);
                    broadcastPacket.WriteMessage(disconnectedClient.UserName);

                    await client.ClientSocket.Client.SendAsync(
                        broadcastPacket.GetPacketBytes(),
                        System.Net.Sockets.SocketFlags.None);

                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now}]: Error broadcasting disconnect to {client.UserName}: {ex.Message}");
                }
            }

            Console.WriteLine($"[{DateTime.Now}]: Disconnect notification for {disconnectedClient.UserName} broadcast to {successCount} of {remainingClients.Count} clients");

            await BroadcastMessageAsync($"System: User {disconnectedClient.UserName} disconnected.");
        }
    }
}
