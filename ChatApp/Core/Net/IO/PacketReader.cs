using ChatApp.Core.Interfaces;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ChatApp.Core.Net.IO
{
    public class PacketReader : IPacketReader, IDisposable
    {
        private readonly NetworkStream _ns;
        private readonly BinaryReader _reader;

        public PacketReader(NetworkStream ns)
        {
            _ns = ns ?? throw new ArgumentNullException(nameof(ns));
            _reader = new BinaryReader(_ns);
        }

        public byte ReadByte()
        {
            try
            {
                return _reader.ReadByte();
            }
            catch (EndOfStreamException)
            {
                throw new InvalidOperationException("Client disconnected unexpectedly.");
            }
        }

        public string ReadMessage()
        {
            try
            {
                var length = _reader.ReadInt32();
                if (length <= 0 || length > 1024 * 1024 * 16)
                {
                    throw new InvalidOperationException("Invalid message length.");
                }

                var buffer = new byte[length];
                var bytesRead = _ns.Read(buffer, 0, length);

                if (bytesRead != length)
                {
                    throw new InvalidOperationException("Could not read the entire message.");
                }

                return Encoding.UTF8.GetString(buffer);
            }
            catch (Exception ex) when (ex is EndOfStreamException || ex is IOException)
            {
                throw new InvalidOperationException("Client disconnected while reading message.", ex);
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
