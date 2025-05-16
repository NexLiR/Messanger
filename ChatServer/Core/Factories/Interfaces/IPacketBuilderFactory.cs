using ChatServer.Core.Interfaces;
using System.Net.Sockets;

namespace ChatServer.Core.Factories.Interfaces
{
    public interface IPacketBuilderFactory
    {
        IPacketBuilder CreatePacketBuilder();
    }
}

