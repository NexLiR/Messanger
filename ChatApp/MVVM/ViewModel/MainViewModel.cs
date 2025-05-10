using ChatApp.MVVM.Core;
using ChatApp.MVVM.Model;
using ChatApp.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatApp.MVVM.ViewModel
{
    public class MainViewModel
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }

        public string UserName { get; set; }
        public string Message { get; set; }

        private Server _server;

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();

            _server = new Server();
            _server.connectedEvent += UserConnected;
            _server.messageReceivedEvent += MessageReceived;
            _server.userDisconnectedEvent += RemoveUser;
            ConnectToServerCommand = new RelayCommand(o => 
            { 
                _server.ConnectToServer(UserName); 
            }, o => 
            {
                if (string.IsNullOrEmpty(UserName))
                {
                    return false;
                }
                if (UserName.Length <= 3)
                {
                    return false;
                }
                return true;
            });
            SendMessageCommand = new RelayCommand(o =>
            {
                _server.SendMessageToServer(Message);
            }, o =>
            {
                if (string.IsNullOrEmpty(Message))
                {
                    return false;
                }
                return true;
            });
        }

        private void RemoveUser()
        {
            var uid = _server._packetReader.ReadMessage();
            var user = Users.Where(u => u.UID == uid).FirstOrDefault();

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (user != null)
                {
                    Users.Remove(user);
                }
            });
        }

        private void MessageReceived()
        {
            var msg = _server._packetReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(msg);
            });
        }

        private void UserConnected()
        {
            var user = new UserModel
            {
                UserName = _server._packetReader.ReadMessage(),
                UID = _server._packetReader.ReadMessage()
            };

            if (!Users.Any(u => u.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Users.Add(user);
                });
            }
        }
    }
}
