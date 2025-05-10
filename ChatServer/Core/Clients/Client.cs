using ChatServer.Core.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Core.Clients
{
    public class Client
    {
        public string UserName { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }
        PacketReader _packetReader;
        public Client(TcpClient clientSocket)
        {
            ClientSocket = clientSocket;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = _packetReader.ReadByte();
            UserName = _packetReader.ReadMessage();

            Console.WriteLine($"[{DateTime.Now}]: Client <{UserName}> connected: {ClientSocket.Client.RemoteEndPoint}");

            Task.Run(() =>
            {
                Process();
            });
        }

        void Process()
        {
            while (true)
            {
                try
                {
                    var opCode = _packetReader.ReadByte();
                    switch (opCode)
                    {
                        case 5:
                            var message = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message received from {UserName}: {message}");
                            Program.BroadcastMessage($"[{DateTime.Now}]: [{UserName}]: {message}");
                            break;
                        default:
                            Console.WriteLine($"[{DateTime.Now}]: Unknown opcode: {opCode}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now}]: [{UID.ToString()}]: Disconnected - Error: {ex.Message}");
                    Program.BroadcastDisconnect(UID.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }
    }
}
