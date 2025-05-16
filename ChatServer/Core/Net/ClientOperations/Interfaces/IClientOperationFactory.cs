namespace ChatServer.Core.Net.ClientOperations.Interfaces
{
    public interface IClientOperationFactory
    {
        IClientOperation GetOperation(byte opCode);
    }
}
