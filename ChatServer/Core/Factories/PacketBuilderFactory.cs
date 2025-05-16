using ChatServer.Core.Factories.Interfaces;
using ChatServer.Core.Interfaces;
using ChatServer.Core.Net.IO;

namespace ChatServer.Core.Factories
{
    public class PacketBuilderFactory : IPacketBuilderFactory
    {
        public IPacketBuilder CreatePacketBuilder()
        {
            return new PacketBuilder();
        }
    }
}
