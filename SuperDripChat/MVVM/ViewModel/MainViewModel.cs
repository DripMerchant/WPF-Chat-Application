using SuperDripChat.Core;
using SuperDripChat.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace SuperDripChat.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public ObservableCollection<MessageModel> Messages { get; set; }
        public ObservableCollection<ContactModel> Contacts { get; set; }


        /* Commands */
        public RelayCommand SendCommand { get; set; }

        private ContactModel _selectedContact;

        public ContactModel SelectedContact
        {
            get { return _selectedContact; }
            set {
                _selectedContact = value;
                OnPropertyChanged();
                }
        }
        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }


        private TcpClient client;
        private NetworkStream stream;

        public MainViewModel()
        {
            
            Messages = new ObservableCollection<MessageModel>();
            Contacts = new ObservableCollection<ContactModel>();


            SendCommand = new RelayCommand(o =>
            {
            Messages.Add(new MessageModel
            {
                Message = Message,
                FirstMessage = false
            });

                Message = "";
            });

            // Fake contacts for Testing Purposes

            Messages.Add(new MessageModel
            {
                Username = "John Doe",
                UsernameColor = "Black",
                ImageSource = "https://images.mubicdn.net/images/cast_member/9020/cache-148043-1465730065/image-w856.jpg?size=800x",
                Message = "Test",
                Time = DateTime.Now,
                IsNativeOrigin = false,
                FirstMessage = true
            });

            for (int i = 0; i < 3; i++)
            {
                Messages.Add(new MessageModel
                {
                    Username = "Jane Doe",
                    UsernameColor = "Black",
                    ImageSource = "https://i.imgur.com/yMWvLXd.png",
                    Message = "Test",
                    Time = DateTime.Now,
                    IsNativeOrigin = false,
                    FirstMessage = false
                });
            }

            for (int i = 0; i < 5; i++)
            {
                Contacts.Add(new ContactModel
                {
                    Username = $"Jane Doe {i}",
                    ImageSource = "https://i.imgur.com/i2szTsp.png",
                    Messages = Messages
                });
            }

            // Connect to the server (adjust the IP and port as needed)
            int serverPort = 12345; // Use the same port as your server
            client = new TcpClient("localhost", serverPort); // Connect to the server
            stream = client.GetStream();

            // Start a background thread to listen for incoming messages
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            
        }

        private void SendMessage(object parameter)
        {
            if (!string.IsNullOrWhiteSpace(Message))
            {
                // Send the message to the server
                byte[] buffer = Encoding.ASCII.GetBytes(Message);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();

                // Add the sent message to the client's own Messages collection
                Messages.Add(new MessageModel
                {
                    Message = Message,
                    FirstMessage = false
                });

                Message = ""; // Clear the message input
            }
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
                catch
                {
                    // Handle connection closed or errors
                    break;
                }

                if (bytesRead == 0)
                {
                    // Connection closed
                    break;
                }

                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Handle the received message (add it to Messages collection, update UI, etc.)
                App.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(new MessageModel
                    {
                        Message = receivedMessage,
                        FirstMessage = false // You can set this based on your logic
                    });
                });
            }

            // Handle disconnection here
        }

        public void CloseConnection()
        {
            stream.Close();
            client.Close();
        }
    }
}
