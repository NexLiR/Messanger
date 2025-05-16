using ChatServer.Core.Interfaces;

namespace ChatServer.Core.Net.ClientOperations.Interfaces
{
    public interface IClientOperation
    {
        Task ExecuteAsync(Client client, IPacketReader packetReader);
    }
}
