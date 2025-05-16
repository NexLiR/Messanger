using ChatServer.Core.Factories.Interfaces;
using ChatServer.Core.Interfaces;
using ChatServer.Core.Net.IO;
using System.Net.Sockets;

namespace ChatServer.Core.Factories
{
    public class PacketReaderFactory : IPacketReaderFactory
    {
        public IPacketReader CreatePacketReader(NetworkStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new PacketReader(stream);
        }
    }
}
