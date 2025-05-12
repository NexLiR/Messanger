namespace ChatApp.Core.Interfaces
{
    public interface IPacketReader
    {
        byte ReadByte();
        string ReadMessage();
    }
}
