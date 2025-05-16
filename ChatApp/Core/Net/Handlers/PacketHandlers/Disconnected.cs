using ChatApp.Constants;
using ChatApp.Core.Interfaces;

namespace ChatApp.Core.Net.Handlers.PacketHandlers
{
    public class DisconnectedPacketHandler : IPacketHandler
    {
        public byte OpCode => OpCodes.Disconnected;

        public event Action<string> OnUserDisconnected;

        public Task HandleAsync(IPacketReader packetReader)
        {
            var dUserId = packetReader.ReadMessage();
            var dUserName = packetReader.ReadMessage();
            OnUserDisconnected?.Invoke(dUserId);
            return Task.CompletedTask;
        }
    }
}
