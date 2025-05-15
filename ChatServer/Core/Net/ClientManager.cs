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

            var lastAddedClient = clientsCopy.LastOrDefault();

            if (lastAddedClient != null)
            {
                foreach (var existingClient in clientsCopy.Where(c => c != lastAddedClient))
                {
                    try
                    {
                        if (lastAddedClient.ClientSocket.Connected)
                        {
                            using var packet = new PacketBuilder();
                            packet.WriteOpCode(OpCodes.Connected);
                            packet.WriteMessage(existingClient.UserName);
                            packet.WriteMessage(existingClient.UID.ToString());

                            await lastAddedClient.ClientSocket.Client.SendAsync(
                                packet.GetPacketBytes(),
                                System.Net.Sockets.SocketFlags.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now}]: Failed to send existing client info to new client: {ex.Message}");
                    }
                }

                foreach (var existingClient in clientsCopy.Where(c => c != lastAddedClient))
                {
                    try
                    {
                        if (existingClient.ClientSocket.Connected)
                        {
                            using var packet = new PacketBuilder();
                            packet.WriteOpCode(OpCodes.Connected);
                            packet.WriteMessage(lastAddedClient.UserName);
                            packet.WriteMessage(lastAddedClient.UID.ToString());

                            await existingClient.ClientSocket.Client.SendAsync(
                                packet.GetPacketBytes(),
                                System.Net.Sockets.SocketFlags.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now}]: Failed to send new client info to existing client: {ex.Message}");
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

            List<IClient> failedClients = new List<IClient>();

            int successCount = 0;
            foreach (var client in clientsCopy)
            {
                if (!client.ClientSocket.Connected)
                {
                    failedClients.Add(client);

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
                    failedClients.Add(client);
                }
                Console.WriteLine($"[{DateTime.Now}]: Message broadcast to {successCount} of {clientsCopy.Count} clients");
            }

            foreach (var failedClient in failedClients)
            {
                if (failedClient.UID != Guid.Empty)
                {
                    await BroadcastDisconnectAsync(failedClient.UID.ToString());
                }
                else
                {
                    lock (_lock)
                    {
                        _clients.Remove(failedClient);
                    }
                }
            }
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
                }

                remainingClients = _clients.ToList().AsReadOnly();
            }

            if (disconnectedClient == null)
                return;

            foreach (var client in remainingClients)
            {
                if (!client.ClientSocket.Connected)
                {
                    continue;
                }

                try
                {
                    using var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(OpCodes.Disconnect);
                    broadcastPacket.WriteMessage(uid);

                    await client.ClientSocket.Client.SendAsync(
                        broadcastPacket.GetPacketBytes(),
                        System.Net.Sockets.SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to broadcast disconnect: {ex.Message}");
                }
            }

            await BroadcastMessageAsync($"[{DateTime.Now}]: User {disconnectedClient.UserName} disconnected.");
        }
    }
}
