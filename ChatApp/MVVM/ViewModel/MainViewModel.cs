using System.ComponentModel;
using System.Windows;
using ChatApp.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.MVVM.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthService _authService;

        public event PropertyChangedEventHandler PropertyChanged;

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public MainViewModel(IServiceProvider serviceProvider, IAuthService authService)
        {
            _serviceProvider = serviceProvider;
            _authService = authService;

            CurrentView = _serviceProvider.GetRequiredService<LoginViewModel>();

            _authService.OnAuthSuccessful += HandleAuthSuccess;
            _authService.OnAuthFailed += HandleAuthFailed;
        }

        private void HandleAuthSuccess(string username)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var chatViewModel = _serviceProvider.GetRequiredService<ChatViewModel>();
                CurrentView = chatViewModel;
            });
        }

        private void HandleAuthFailed(string errorMessage)
        {
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
