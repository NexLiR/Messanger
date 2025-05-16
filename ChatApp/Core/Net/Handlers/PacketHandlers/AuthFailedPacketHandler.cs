using ChatApp.Constants;
using ChatApp.Core.Interfaces;

namespace ChatApp.Core.Net.Handlers.PacketHandlers
{
    public class AuthFailedPacketHandler : IPacketHandler
    {
        public byte OpCode => OpCodes.AuthFailed;

        public event Action<string> OnAuthFailed;

        public Task HandleAsync(IPacketReader packetReader)
        {
            var errorMessage = packetReader.ReadMessage();
            OnAuthFailed?.Invoke(errorMessage);
            return Task.CompletedTask;
        }
    }
}
