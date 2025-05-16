using ChatApp.Constants;
using ChatApp.Core.Interfaces;
using ChatApp.Core.Services.Interfaces;

namespace ChatApp.Core.Net.Handlers.PacketHandlers
{
    public class ConnectedPacketHandler : IPacketHandler
    {
        private readonly IUserService _userService;

        public byte OpCode => OpCodes.Connected;

        public event Action<string> OnUserConnected;

        public ConnectedPacketHandler(IUserService userService)
        {
            _userService = userService;
        }

        public Task HandleAsync(IPacketReader packetReader)
        {
            var userName = packetReader.ReadMessage();
            var userId = packetReader.ReadMessage();
            OnUserConnected?.Invoke(userName);
            return Task.CompletedTask;
        }
    }
}
