using ChatApp.Constants;
using ChatApp.Core.Interfaces;

namespace ChatApp.Core.Net.Handlers.PacketHandlers
{
    public class AuthSuccessPacketHandler : IPacketHandler
    {
        public byte OpCode => OpCodes.AuthSuccess;

        public event Action<string, string> OnAuthSuccessful;

        public Task HandleAsync(IPacketReader packetReader)
        {
            var username = packetReader.ReadMessage();
            var userId = packetReader.ReadMessage();
            OnAuthSuccessful?.Invoke(username, userId);
            return Task.CompletedTask;
        }
    }
}
