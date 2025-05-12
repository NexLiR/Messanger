using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using ChatApp.Core.Interfaces;
using ChatApp.Core.Services.Interfaces;
using ChatApp.Core;

namespace ChatApp.MVVM.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IServerConnection _serverConnection;
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Messages => new(_messageService.GetMessages());
        public ObservableCollection<string> Users => new(_userService.GetUserNames());

        public ICommand ConnectCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }

        private string _userName;
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

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
        public MainViewModel()
        {

        }
        public MainViewModel( IServerConnection serverConnection, IMessageService messageService, IUserService userService)
        {
            _serverConnection = serverConnection;
            _messageService = messageService;
            _userService = userService;

            InitializeServerConnectionEvents();
            InitializeCommands();
        }

        private void InitializeServerConnectionEvents()
        {
            _serverConnection.OnMessageReceived += HandleMessageReceived;
            _serverConnection.OnUserConnected += HandleUserConnected;
            _serverConnection.OnUserDisconnected += HandleUserDisconnected;
        }

        private void InitializeCommands()
        {
            ConnectCommand = new RelayCommand(
                async _ => await ConnectToServerAsync(),
                _ => !string.IsNullOrWhiteSpace(UserName) && UserName.Length > 3
            );

            SendMessageCommand = new RelayCommand(
                async _ => await SendMessageAsync(),
                _ => !string.IsNullOrWhiteSpace(Message)
            );
        }

        private async Task ConnectToServerAsync()
        {
            try
            {
                await _serverConnection.ConnectAsync(UserName);
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

        private void HandleMessageReceived(string message)
        {
            _messageService.AddMessage(message);
            OnPropertyChanged(nameof(Messages));
        }

        private void HandleUserConnected(string userName)
        {
            _userService.AddUser(userName);
            OnPropertyChanged(nameof(Users));
        }

        private void HandleUserDisconnected(string userName)
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                _userService.RemoveUser(userName);
                OnPropertyChanged(nameof(Users));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
