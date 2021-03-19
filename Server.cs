using System.Net.Sockets;
using System;
using System.Net;
using System.Threading;

namespace HTTPServer
{
    class Server
    {
        private TcpListener Listener { get; set; }

        public Server() : this(8080) {}

        public Server(int port)
        {
            Listener = new TcpListener(IPAddress.Any, port);
            Listener.Start();

            for (;;)
            {
                var client = Listener.AcceptTcpClient();

                var thread = new Thread(new ParameterizedThreadStart(HandleClient));
                thread.Start(client);
            }
        }

        private static void HandleClient(object tcpClient)
        {
            Console.WriteLine("хаха");
            new Client((TcpClient)tcpClient);
        }

        ~Server()
        {
            if (Listener != null)
            {
                Listener.Stop();
            }
        }
    }
}
