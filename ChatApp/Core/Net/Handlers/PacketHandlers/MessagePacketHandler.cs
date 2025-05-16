using ChatApp.Constants;
using ChatApp.Core.Interfaces;

namespace ChatApp.Core.Net.Handlers.PacketHandlers
{
    public class MessagePacketHandler : IPacketHandler
    {
        public byte OpCode => OpCodes.Message;

        public event Action<string> OnMessageReceived;

        public Task HandleAsync(IPacketReader packetReader)
        {
            var message = packetReader.ReadMessage();
            OnMessageReceived?.Invoke(message);
            return Task.CompletedTask;
        }
    }
}
