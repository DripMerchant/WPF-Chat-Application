using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace SuperDripChat.Server
{
    class Server
    {
        private readonly ConcurrentDictionary<int, TcpClient> clients = new ConcurrentDictionary<int, TcpClient>();

        public void StartServer()
        {
            int port = 12345; // Choose a port number

            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();

            Console.WriteLine("Server started...");

            int clientId = 0;

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                clients.TryAdd(clientId, client);
                int currentClientId = clientId;
                clientId++;

                Thread clientThread = new Thread(() => HandleClient(currentClientId));
                clientThread.Start();
            }
        }

        void HandleClient(int clientId)
        {
            TcpClient client;
            if (!clients.TryGetValue(clientId, out client))
            {
                return;
            }

            NetworkStream stream = client.GetStream();
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
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {message}");

                // Broadcast the message to all clients (excluding the sender)
                BroadcastMessage(message, clientId);
            }

            clients.TryRemove(clientId, out _);
            client.Close();
            Console.WriteLine($"Client {clientId} disconnected.");
        }

        void BroadcastMessage(string message, int senderId)
        {
            foreach (var client in clients)
            {
                if (client.Key != senderId)
                {
                    TcpClient clientSocket = client.Value;
                    NetworkStream clientStream = clientSocket.GetStream();
                    byte[] buffer = Encoding.ASCII.GetBytes(message);
                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush();
                }
            }
        }
    }
}
