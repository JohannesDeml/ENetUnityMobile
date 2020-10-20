using System;
using System.Runtime.InteropServices;
using System.Text;
using ENet;

namespace ENetServer
{
    class Program
    {
        private static Host _server;
        private static byte[] _buffer;
        
        static void Main(string[] args)
        {
            ENet.Library.Initialize();
            
            _server = new Host();
            _buffer = new byte[500];
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
                            var packet = CreateMessagePacket(netEvent);

                            netEvent.Packet.Dispose();
                            BroadcastMessage(packet);
                            break;
                    }
                }

                _server.Flush();
            }

            ENet.Library.Deinitialize();
        }

        private static Packet CreateMessagePacket(Event netEvent)
        {
            var pointer = Encoding.UTF8.GetBytes(netEvent.Peer.IP + ": ", _buffer);
            Marshal.Copy(netEvent.Packet.Data, _buffer, pointer, netEvent.Packet.Length);
            pointer += netEvent.Packet.Length;
            
            var packet = default(Packet);
            packet.Create(_buffer, pointer);
            return packet;
        }
        
        private static void BroadcastMessage(Packet packet)
        {
            _server.Broadcast(0, ref packet);
        }
    }
}