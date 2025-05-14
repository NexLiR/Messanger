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

        public ICommand SendMessageCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

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
            _serverConnection = serverConnection;
            _messageService = messageService;
            _userService = userService;
            _authService = authService;

            InitializeServerConnectionEvents();
            InitializeCommands();

            ConnectToServerAsync().ConfigureAwait(false);
        }

        private void InitializeServerConnectionEvents()
        {
            _serverConnection.OnMessageReceived += HandleMessageReceived;
            _serverConnection.OnUserConnected += HandleUserConnected;
            _serverConnection.OnUserDisconnected += HandleUserDisconnected;
        }

        private void InitializeCommands()
        {
            SendMessageCommand = new RelayCommand(
                async _ => await SendMessageAsync(),
                _ => !string.IsNullOrWhiteSpace(Message)
            );

            LogoutCommand = new RelayCommand(
                async _ => await LogoutAsync(),
                _ => true
            );
        }

        private async Task ConnectToServerAsync()
        {
            try
            {
                await _serverConnection.ConnectAsync(_authService.CurrentUsername);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SendMessageAsync()
        {
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

                _messageService.ClearMessages();
                _userService.ClearUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Logout error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleMessageReceived(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                _messageService.AddMessage(message);
                OnPropertyChanged(nameof(Messages));
            });
        }

        private void HandleUserConnected(string userName)
        {
            Application.Current.Dispatcher.Invoke(() => {
                _userService.AddUser(userName);
                OnPropertyChanged(nameof(Users));
            });
        }

        private void HandleUserDisconnected(string userName)
        {
            Application.Current.Dispatcher.Invoke(() => {
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    _userService.RemoveUser(userName);
                    OnPropertyChanged(nameof(Users));
                }
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
