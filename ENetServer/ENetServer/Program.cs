using System;
using ENet;

namespace ENetServer
{
    class Program
    {
        private static Host _server;
        
        static void Main(string[] args)
        {
            ENet.Library.Initialize();
            
            _server = new Host();
            Address address = new Address();
            address.Port = 3333;
            int maxClients = 4000;
            
            _server.Create(address, maxClients);
            Console.WriteLine($"Server started with port {address.Port}");
            
            Event netEvent;
            while (!Console.KeyAvailable)
            {
                bool polled = false;

                while (!polled)
                {
                    if (_server.CheckEvents(out netEvent) <= 0)
                    {
                        if (_server.Service(15, out netEvent) <= 0)
                            break;

                        polled = true;
                    }

                    switch (netEvent.Type)
                    {
                        case EventType.None:
                            break;

                        case EventType.Connect:
                            Console.WriteLine("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            netEvent.Peer.Timeout(32, 1000, 4000);
                            break;

                        case EventType.Disconnect:
                            Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            break;

                        case EventType.Timeout:
                            Console.WriteLine("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            break;

                        case EventType.Receive:
                            Console.WriteLine("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                            netEvent.Packet.Dispose();
                            break;
                    }
                }

                _server.Flush();
            }
            
            ENet.Library.Deinitialize();
        }
    }
}