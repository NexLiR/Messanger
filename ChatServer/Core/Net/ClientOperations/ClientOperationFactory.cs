using ChatServer.Constants;
using ChatServer.Core.Net.ClientOperations.Interfaces;

namespace ChatServer.Core.Net.ClientOperations
{
    public class ClientOperationFactory : IClientOperationFactory
    {
        private readonly Dictionary<byte, IClientOperation> _operations;

        public ClientOperationFactory(
            MessageOperation messageOperation,
            RegisterOperation registerOperation,
            LoginOperation loginOperation,
            IdentifyOperation identifyOperation,
            DisconnectOperation disconnectOperation,
            LogoutOperation logoutOperation)
        {
            _operations = new Dictionary<byte, IClientOperation>
            {
                { OpCodes.Message, messageOperation },
                { OpCodes.Register, registerOperation },
                { OpCodes.Login, loginOperation },
                { OpCodes.Identify, identifyOperation },
                { OpCodes.Disconnect, disconnectOperation },
                { OpCodes.Logout, logoutOperation }
            };
        }

        public IClientOperation GetOperation(byte opCode)
        {
            if (_operations.TryGetValue(opCode, out var operation))
            {
                return operation;
            }

            throw new ArgumentException($"Unknown operation code: {opCode}");
        }
    }
}
