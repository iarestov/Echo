using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Echo.Domain;

namespace Echo.Server
{
    class Program
    {
        static readonly TimeSpan _roomTtl = TimeSpan.FromMinutes(1);
        static readonly TimeSpan _roomCleanupInterval = TimeSpan.FromSeconds(10);
        static readonly EchoRoomServer EchoRoomServer = new EchoRoomServer(_roomTtl); 

        static void Main(string[] args) // TODO: arg parse for endpoint
        {
            try
            {
                var ep = CreateEndPoint();

                var tcpListener = new TcpListener(ep);

                tcpListener.Start();

                Console.WriteLine($"Server started at {ep.Address}:{ep.Port}\nPress Ctrl+C to exit");

                Task.WaitAll(
                    ProcessIncomingClients(tcpListener),
                    DispatchOutMessages(),
                    CleanSilentRooms()
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

        private static IPEndPoint CreateEndPoint()
        {
            var port = 45000;
            var ep = new IPEndPoint(IPAddress.Loopback, port);
            return ep;
        }
        

        private static Task CleanSilentRooms()
        {
            while (true)
            {
                Task.Delay(TimeSpan.FromMilliseconds(_roomTtl.TotalMilliseconds/2));
                EchoRoomServer.DropSilentRooms();
            }
        }

        private static async Task ProcessIncomingClients(TcpListener tcpListener)
        {
            while (true)
            {
                try
                {
                    var tcpClient = await tcpListener.AcceptTcpClientAsync();
                    Task.Factory.StartNew(() => ProcessConnectedClient(tcpClient));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static async Task DispatchOutMessages()
        {
            var lastCleanupTime = DateTime.Now;

            while (true)
            {
                if ((DateTime.Now - lastCleanupTime) > _roomCleanupInterval) // clean up each 10 seconds
                {
                    lastCleanupTime = DateTime.Now;
                    EchoRoomServer.DropSilentRooms();
                }

                var message = EchoRoomServer.GetMessageToSend();
                if (message == null)
                {
                    Thread.Sleep(0); // better way is to add event to sever that signals queue change and wait on it
                    continue;
                }

                var client = message.Client.NetworkId as TcpClient;
                if (client == null || !client.Connected) continue;

                var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };

                await writer.WriteLineAsync($"{DateTime.Now} {message.Author.RoomName} {message.MessageType} {message.Author.Id}:{message.Data}");
            }
        }


        /// <summary>
        /// Login process for client
        /// it's done simple - client send two lines:
        ///   login
        ///   room
        /// all lines after that is messages
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static async Task<IClient> Login(TcpClient tcpClient, StreamReader reader)
        {
            var clientId = await reader.ReadLineAsync();
            if (clientId == null) return null;
            var roomName = await reader.ReadLineAsync();
            if (roomName == null) return null;

            // TODO: in real world apps some auth suggested

            return EchoRoomServer.EnterInRoom(clientId, tcpClient, roomName);
        }

        private static async Task ProcessConnectedClient(TcpClient tcpClient)
        {
            Console.WriteLine($"Received connection request from {tcpClient.Client.RemoteEndPoint}");

            try
            {
                var networkStream = tcpClient.GetStream();
                var reader = new StreamReader(networkStream);

                var client = await Login(tcpClient, reader);
                if (client != null)
                {
                    while (true)
                    {
                        var request = await reader.ReadLineAsync();
                        if (request == null) break; // Client closed connection

                        client.Say(request);
                        Console.WriteLine($"Client \"{client.Id}\" says to room \"{client.RoomName}\": \"{request}\"");
                    }
                }
                Console.WriteLine($"Client {tcpClient.Client.RemoteEndPoint} closed connection");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (tcpClient.Connected)
                    tcpClient.Close();
            }
        }


    }
}
