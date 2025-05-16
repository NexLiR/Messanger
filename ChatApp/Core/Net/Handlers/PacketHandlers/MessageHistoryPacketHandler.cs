using ChatApp.Constants;
using ChatApp.Core.Interfaces;

namespace ChatApp.Core.Net.Handlers.PacketHandlers
{
    public class MessageHistoryPacketHandler : IPacketHandler
    {
        public byte OpCode => OpCodes.MessageHistory;

        public event Action<string> OnMessageReceived;

        public Task HandleAsync(IPacketReader packetReader)
        {
            var historyMessage = packetReader.ReadMessage();
            OnMessageReceived?.Invoke(historyMessage);
            return Task.CompletedTask;
        }
    }
}
