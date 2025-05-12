namespace ChatServer.Core.Interfaces
{
    public interface IPacketReader
    {
        byte ReadByte();
        string ReadMessage();
    }
}
