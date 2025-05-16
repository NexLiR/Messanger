using ChatServer.Constants;
using ChatServer.Core.Factories.Interfaces;
using ChatServer.Core.Interfaces;
using ChatServer.Core.Net.ClientOperations.Interfaces;
using ChatServer.Data.Repositories.Interfaces;
using System.Net.Sockets;

namespace ChatServer.Core.Net.ClientOperations
{
    public class LoginOperation : IClientOperation
    {
        private readonly IUserRepository _userRepository;
        private readonly IPacketBuilderFactory _packetBuilderFactory;

        public LoginOperation(
            IUserRepository userRepository,
            IPacketBuilderFactory packetBuilderFactory)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _packetBuilderFactory = packetBuilderFactory ?? throw new ArgumentNullException(nameof(packetBuilderFactory));
        }

        public async Task ExecuteAsync(Client client, IPacketReader packetReader)
        {
            var username = packetReader.ReadMessage();
            var password = packetReader.ReadMessage();

            try
            {
                var user = await _userRepository.AuthenticateUserAsync(username, password);

                if (user == null)
                {
                    client.SendAuthFailedMessage("Invalid username or password");
                    return;
                }

                client.UserName = user.UserName;
                client.UID = user.UID;
                client.SetUser(user);
                client.SetAuthenticated(true);

                var packet = _packetBuilderFactory.CreatePacketBuilder();
                packet.WriteOpCode(OpCodes.AuthSuccess);
                packet.WriteMessage(user.UserName);
                packet.WriteMessage(user.UID.ToString());

                await client.ClientSocket.Client.SendAsync(packet.GetPacketBytes(), SocketFlags.None);
                Console.WriteLine($"[{DateTime.Now}]: User {username} logged in successfully with UID {client.UID}");
            }
            catch (Exception ex)
            {
                client.SendAuthFailedMessage($"Login failed: {ex.Message}");
            }
        }
    }
}
