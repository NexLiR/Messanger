using ChatServer.Core.Interfaces;
using System.Net.Sockets;

namespace ChatServer.Core.Factories.Interfaces
{
    public interface IClientFactory
    {
        IClient CreateClient(TcpClient clientSocket);
    }
}
