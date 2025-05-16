using ChatServer.Core.Interfaces;
using System.Net.Sockets;

namespace ChatServer.Core.Factories.Interfaces
{
    public interface IPacketReaderFactory
    {
        IPacketReader CreatePacketReader(NetworkStream stream);
    }
}

