using ChatApp.Core.Interfaces;
using ChatApp.Core.Services.Interfaces;
using ChatApp.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;

namespace ChatApp.MVVM.ViewModel
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private readonly IServerConnection _serverConnection;
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Messages => new(_messageService.GetMessages());
        public ObservableCollection<string> Users => new(_userService.GetUserNames());

        public ICommand SendMessageCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string CurrentUser => _authService.CurrentUsername;

        public ChatViewModel(
            IServerConnection serverConnection,
            IMessageService messageService,
            IUserService userService,
            IAuthService authService)
        {
            _serverConnection = serverConnection ?? throw new ArgumentNullException(nameof(serverConnection));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            _userService.ClearUsers();
            _messageService.ClearMessages();

            InitializeServerConnectionEvents();
            InitializeCommands();

            ConnectToServerAsync().ConfigureAwait(false);
        }

        private void InitializeServerConnectionEvents()
        {
            _serverConnection.OnMessageReceived += message =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _messageService.AddMessage(message);
                    OnPropertyChanged(nameof(Messages));
                });
            };

            _serverConnection.OnUserConnected += userName =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _userService.AddUser(userName);
                    OnPropertyChanged(nameof(Users));
                });
            };

            _serverConnection.OnUserDisconnected += userId =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!string.IsNullOrWhiteSpace(userId))
                    {
                        _userService.RemoveUser(userId);
                        OnPropertyChanged(nameof(Users));
                    }
                });
            };
        }

        private void InitializeCommands()
        {
            SendMessageCommand = new RelayCommand(
                async _ => await SendMessageAsync(),
                _ => !string.IsNullOrWhiteSpace(Message) && _serverConnection != null
            );

            LogoutCommand = new RelayCommand(
                async _ => await LogoutAsync(),
                _ => _authService.IsAuthenticated
            );
        }

        private async Task ConnectToServerAsync()
        {
            try
            {
                await _serverConnection.UseExistingConnectionAsync(_authService.CurrentUsername);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;

            try
            {
                await _serverConnection.SendMessageAsync(Message);
                Message = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Send message error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LogoutAsync()
        {
            try
            {
                await _serverConnection.DisconnectAsync();
                await _authService.LogoutAsync();

                _userService.ClearUsers();
                _messageService.ClearMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Logout error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
