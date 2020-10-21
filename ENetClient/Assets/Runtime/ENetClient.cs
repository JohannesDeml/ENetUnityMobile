// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnityUdpClient.cs">
//   Copyright (c) 2020 Johannes Deml. All rights reserved.
// </copyright>
// <author>
//   Johannes Deml
//   public@deml.io
// </author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ENet;
using UnityEngine;

namespace NetCoreServer
{
	public class ENetClient
	{
		/// <summary>
		/// Tells whether the client is running and listening for net events
		/// </summary>
		public bool IsRunning;

		public bool IsConnected => _peer.State == PeerState.Connected;

		/// <summary>
		/// Address used to connect to the server
		/// </summary>
		public Address Address => _address;

		/// <summary>
		/// Buffer for all received messages
		/// </summary>
		public byte[] Buffer => _buffer;

		/// <summary>
		/// Pointers on the buffer for the received messages.
		/// Dequeue them regularly and completely to make space for the next messages
		/// </summary>
		public Queue<(int, int)> BufferPointer => _bufferPointer;

		/// <summary>
		/// True if the instance is disposed
		/// </summary>
		public bool IsDisposed { get; private set; }

		private readonly Host _host;
		private Address _address;
		private Peer _peer;
		private readonly byte[] _buffer;
		private readonly Queue<(int Start, int Length)> _bufferPointer;
		private Task _listenTask;
		private int _tickRateClient = 60;

		public ENetClient()
		{
			_buffer = new byte[2000];
			_bufferPointer = new Queue<(int, int)>();

			ENet.Library.Initialize();
			_address = new Address();
			_host = new Host();
			_host.Create();
			IsDisposed = false;
		}

		public void Connect(string address, int port)
		{
			IsRunning = true;
			_address.SetHost(address);
			_address.Port = (ushort) port;
			_peer = _host.Connect(_address, 4);

			_listenTask = Task.Factory.StartNew(Listen, TaskCreationOptions.LongRunning);
			IsDisposed = false;
		}

		public void Send(byte[] message)
		{
			SendUnreliable(message, 0, _peer);
		}

		public void Disconnect()
		{
			_peer.DisconnectNow(0);
			IsRunning = false;
		}

		public async void Dispose()
		{
			while (!_listenTask.IsCompleted)
			{
				await Task.Delay(10);
			}

			_listenTask.Dispose();
			_host.Flush();
			_host.Dispose();
			ENet.Library.Deinitialize();
			IsDisposed = true;
		}

		private void Listen()
		{
			while (IsRunning)
			{
				_host.Service(1000 / _tickRateClient, out ENet.Event netEvent);

				switch (netEvent.Type)
				{
					case ENet.EventType.None:
						break;

					case ENet.EventType.Connect:
						Debug.Log("Client connected");
						break;

					case ENet.EventType.Receive:
						var startIndex = 0;
						var length = netEvent.Packet.Length;
						if (_bufferPointer.Count > 0)
						{
							startIndex = _bufferPointer.Peek().Start;
						}

						Marshal.Copy(netEvent.Packet.Data, _buffer, startIndex, length);
						_bufferPointer.Enqueue((startIndex, length));

						netEvent.Packet.Dispose();

						break;
				}
			}
		}

		private void SendReliable(byte[] data, byte channelID, Peer peer)
		{
			Packet packet = default(Packet);

			packet.Create(data, data.Length, PacketFlags.Reliable | PacketFlags.NoAllocate); // Reliable Sequenced
			peer.Send(channelID, ref packet);
		}

		private void SendUnreliable(byte[] data, byte channelID, Peer peer)
		{
			Packet packet = default(Packet);

			packet.Create(data, data.Length, PacketFlags.None | PacketFlags.NoAllocate); // Unreliable Sequenced
			peer.Send(channelID, ref packet);
		}
	}
}