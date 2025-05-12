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
        }

        public void RemoveClient(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            lock (_lock)
            {
                _clients.Remove(client);
            }
        }

        public async Task BroadcastConnectionAsync()
        {
            IReadOnlyList<IClient> clientsCopy;

            lock (_lock)
            {
                clientsCopy = _clients.ToList().AsReadOnly();
            }

            foreach (var user in clientsCopy)
            {
                foreach (var client in clientsCopy)
                {
                    try
                    {
                        using var broadcastPacket = new PacketBuilder();
                        broadcastPacket.WriteOpCode(OpCodes.Connected);
                        broadcastPacket.WriteMessage(client.UserName);
                        broadcastPacket.WriteMessage(client.UID.ToString());

                        await user.ClientSocket.Client.SendAsync(
                            broadcastPacket.GetPacketBytes(),
                            System.Net.Sockets.SocketFlags.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error broadcasting connection to {user.UserName}: {ex.Message}");
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

            foreach (var client in clientsCopy)
            {
                try
                {
                    using var messagePacket = new PacketBuilder();
                    messagePacket.WriteOpCode(OpCodes.Message);
                    messagePacket.WriteMessage(message);

                    await client.ClientSocket.Client.SendAsync(
                        messagePacket.GetPacketBytes(),
                        System.Net.Sockets.SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error broadcasting message to {client.UserName}: {ex.Message}");
                }
            }
        }

        public async Task BroadcastDisconnectAsync(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                throw new ArgumentException("UID cannot be null or empty", nameof(uid));

            IClient? disconnectedClient = null;
            IReadOnlyList<IClient> clientsCopy;

            lock (_lock)
            {
                disconnectedClient = _clients.FirstOrDefault(u => u.UID.ToString() == uid);

                if (disconnectedClient != null)
                {
                    _clients.Remove(disconnectedClient);
                }

                clientsCopy = _clients.ToList().AsReadOnly();
            }

            if (disconnectedClient == null)
                return;

            foreach (var client in clientsCopy)
            {
                try
                {
                    using var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(OpCodes.Disconnected);
                    broadcastPacket.WriteMessage(uid);

                    await client.ClientSocket.Client.SendAsync(
                        broadcastPacket.GetPacketBytes(),
                        System.Net.Sockets.SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error broadcasting disconnect to {client.UserName}: {ex.Message}");
                }
            }

            await BroadcastMessageAsync($"User <{disconnectedClient.UserName}> disconnected.");
        }
    }
}
