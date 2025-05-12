using ChatApp.Core.Interfaces;
using System.IO;
using System.Text;

namespace ChatApp.Core.Net.IO
{
    public class PacketBuilder : IPacketBuilder, IDisposable
    {
        private readonly MemoryStream _ms;

        public PacketBuilder()
        {
            _ms = new MemoryStream();
        }

        public void WriteOpCode(byte opCode)
        {
            _ms.WriteByte(opCode);
        }

        public void WriteMessage(string message)
        {
            var messageLength = message.Length;
            _ms.Write(BitConverter.GetBytes(messageLength));
            _ms.Write(Encoding.UTF8.GetBytes(message));
        }

        public byte[] GetPacketBytes()
        {
            return _ms.ToArray();
        }

        public void Dispose()
        {
            _ms.Dispose();
        }
    }
}
