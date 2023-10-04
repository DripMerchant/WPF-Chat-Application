using System;
using System.Threading;
using System.Windows;
using SuperDripChat.Server;

namespace SuperDripChat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Server.Server server;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Start the server in a separate thread
            Thread serverThread = new Thread(StartServer);
            serverThread.Start();
        }

        private void StartServer()
        {
            // Create an instance of your server class
            server = new Server.Server();

            // Start the server
            server.StartServer();
        }
    }
}
