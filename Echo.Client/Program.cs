using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Echo.Client
{
    class ClientInfo
    {
        public string Id { get; set; }
        public string Room { get; set; }

        public ClientInfo(string id, string room)
        {
            Id = id;
            Room = room;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var mainAsync = MainAsync(args);
            mainAsync.Wait();
        }

        private static IPEndPoint CreateEndPoint()
        {
            var port = 45000;
            var ep = new IPEndPoint(IPAddress.Loopback, port);
            return ep;
        }

        static ClientInfo GetClientInfo(string[] args)
        {
            if (args.Length < 2) throw new InvalidOperationException("Usage: echo.client <clientid> <roomname>");
            return new ClientInfo(args[0],args[1]);
        }

        private static async Task MainAsync(string[] args) // TODO: arg parse for endpoint
        {
            try
            {
                var clientInfo = GetClientInfo(args);
                Console.WriteLine($@"Starting client {clientInfo.Id} for room {clientInfo.Room}");

                var ep = CreateEndPoint();

#if DEBUG
                await Task.Delay(1500); // give some time to server for start
#endif

                var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(ep.Address, ep.Port);

                Console.WriteLine($"Connected to server at {ep.Address}:{ep.Port}\nPress Ctrl+C to exit");

                Console.WriteLine("Loggin in");

                var stream = tcpClient.GetStream();
                var writer = new StreamWriter(stream) {AutoFlush = true};

                // login to room
                await writer.WriteLineAsync(clientInfo.Id);
                await writer.WriteLineAsync(clientInfo.Room);

                // TODO: good idea to add some acceptance responce from server

                Console.WriteLine("Logged");

                Task.WaitAll(
                    SendMessagesToRoom(writer),
                    DispatchInMessages(new StreamReader(stream)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

        private static async Task SendMessagesToRoom(StreamWriter writer)
        {
            var rnd = new Random();

            while (true)
            {
                await Task.Delay(100);

                await writer.WriteLineAsync(rnd.Next().ToString());
            }
        }

        private static async Task DispatchInMessages(StreamReader reader)
        {
            while (true)
            {
                var incomingMessage = await reader.ReadLineAsync();
                Console.WriteLine(incomingMessage);
            }
        }
    }
}
