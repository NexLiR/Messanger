using ChatServer.Core.Interfaces;
using System.Net.Sockets;

namespace ChatServer.Core.Net
{
    public abstract class ServerBase
    {
        protected readonly IClientManager _clientManager;
        protected TcpListener _listener;
        protected bool _isRunning;

        protected ServerBase(IClientManager clientManager)
        {
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning) return;

            _isRunning = true;
            InitializeListener();

            try
            {
                _listener.Start();
                OnServerStarted();

                while (_isRunning && !cancellationToken.IsCancellationRequested)
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken);
                    await HandleNewClientAsync(tcpClient);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[{DateTime.Now}]: Server stopping due to cancellation request");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}]: Server error: {ex.Message}");
            }
            finally
            {
                Stop();
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _listener?.Stop();
            OnServerStopped();
        }

        protected abstract void InitializeListener();
        protected abstract Task HandleNewClientAsync(TcpClient tcpClient);
        protected abstract void OnServerStarted();
        protected abstract void OnServerStopped();
    }
}
