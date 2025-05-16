using ChatApp.Constants;
using ChatApp.Core.Interfaces;

namespace ChatApp.Core.Net.Handlers.PacketHandlers
{
    public class DisconnectPacketHandler : IPacketHandler
    {
        public byte OpCode => OpCodes.Disconnect;

        public event Action<string> OnUserDisconnected;

        public Task HandleAsync(IPacketReader packetReader)
        {
            var disconnectedUserId = packetReader.ReadMessage();
            OnUserDisconnected?.Invoke(disconnectedUserId);
            return Task.CompletedTask;
        }
    }
}
