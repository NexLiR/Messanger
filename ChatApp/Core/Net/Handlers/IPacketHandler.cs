using ChatApp.Core.Interfaces;

namespace ChatApp.Core.Net.Handlers
{
    public interface IPacketHandler
    {
        byte OpCode { get; }
        Task HandleAsync(IPacketReader packetReader);
    }
}
