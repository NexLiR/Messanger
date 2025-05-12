using System.Net.Sockets;

namespace ChatServer.Core.Interfaces
{
    public interface IClient
    {
        string UserName { get; }
        Guid UID { get; }
        TcpClient ClientSocket { get; }
        Task ProcessAsync();
        void Disconnect();
    }
}
