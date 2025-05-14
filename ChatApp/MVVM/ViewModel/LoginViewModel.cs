using ChatApp.Core.Services.Interfaces;
using ChatApp.Core;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatApp.MVVM.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        private SolidColorBrush _statusColor = Brushes.Black;
        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;

            LoginCommand = new RelayCommand(
                async param => await ExecuteLoginAsync(param as PasswordBox),
                param => !string.IsNullOrWhiteSpace(Username) && param is PasswordBox passwordBox &&
                         !string.IsNullOrWhiteSpace(passwordBox.Password)
            );

            RegisterCommand = new RelayCommand(
                async param => await ExecuteRegisterAsync(param as PasswordBox),
                param => !string.IsNullOrWhiteSpace(Username) && param is PasswordBox passwordBox &&
                         !string.IsNullOrWhiteSpace(passwordBox.Password)
            );

            _authService.OnAuthSuccessful += username => {
                StatusMessage = $"Welcome, {username}!";
                StatusColor = Brushes.Green;
            };

            _authService.OnAuthFailed += message => {
                StatusMessage = message;
                StatusColor = Brushes.Red;
            };
        }

        private async Task ExecuteLoginAsync(PasswordBox passwordBox)
        {
            if (passwordBox == null) return;

            StatusMessage = "Logging in...";
            StatusColor = Brushes.Black;

            try
            {
                await _authService.LoginAsync(Username, passwordBox.Password);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                StatusColor = Brushes.Red;
            }
        }

        private async Task ExecuteRegisterAsync(PasswordBox registerPasswordBox)
        {
            if (registerPasswordBox == null) return;

            StatusMessage = "Registering...";
            StatusColor = Brushes.Black;

            try
            {
                await _authService.RegisterAsync(Username, registerPasswordBox.Password);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                StatusColor = Brushes.Red;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
