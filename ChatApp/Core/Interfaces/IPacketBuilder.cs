namespace ChatApp.Core.Interfaces
{
    public interface IPacketBuilder
    {
        void WriteOpCode(byte opCode);
        void WriteMessage(string message);
        byte[] GetPacketBytes();
    }
}
