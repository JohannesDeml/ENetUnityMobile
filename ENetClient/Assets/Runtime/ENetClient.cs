// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ENetClient.cs">
//   Copyright (c) 2020 Johannes Deml. All rights reserved.
// </copyright>
// <author>
//   Johannes Deml
//   public@deml.io
// </author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ENet;
using UnityEngine;

namespace Supyrb
{
	public class ENetClient
	{
		/// <summary>
		/// Tells whether the client is running and listening for net events
		/// </summary>
		public bool IsRunning;

		public bool IsConnected => peer.State == PeerState.Connected;

		/// <summary>
		/// Address used to connect to the server
		/// </summary>
		public Address Address => address;

		/// <summary>
		/// Buffer for all received messages
		/// </summary>
		public byte[] Buffer => buffer;

		/// <summary>
		/// Pointers on the buffer for the received messages.
		/// Dequeue them regularly and completely to make space for the next messages
		/// </summary>
		public Queue<BufferPointer> BufferPointer => bufferPointer;

		/// <summary>
		/// True if the instance is disposed
		/// </summary>
		public bool IsDisposed { get; private set; }
		
		public Version EnetVersion { get; private set; }

		private readonly Host host;
		private Address address;
		private Peer peer;
		private readonly byte[] buffer;
		private readonly Queue<BufferPointer> bufferPointer;
		private Task listenTask;
		private int tickRateClient = 60;

		public ENetClient()
		{
			buffer = new byte[2000];
			bufferPointer = new Queue<BufferPointer>();

			ENet.Library.Initialize();
			address = new Address();
			host = new Host();
			host.Create();
			IsDisposed = false;

			int major = (int)(ENet.Library.version >> 16 & 0xFFu);
			int minor = (int)(ENet.Library.version >> 8 & 0xFFu);
			int build = (int)(ENet.Library.version & 0xFFu);
			EnetVersion = new Version(major, minor, build);
		}

		public void Connect(string address, int port)
		{
			IsRunning = true;
			this.address.SetHost(address);
			this.address.Port = (ushort) port;
			peer = host.Connect(this.address, 4);

			listenTask = Task.Factory.StartNew(Listen, TaskCreationOptions.LongRunning);
			IsDisposed = false;
		}

		public void Send(byte[] message)
		{
			SendUnreliable(message, 0, peer);
		}

		public void Disconnect()
		{
			if (!IsRunning)
			{
				return;
			}
			peer.DisconnectNow(0);
			IsRunning = false;
		}

		public async void Dispose()
		{
			if (!IsRunning || IsDisposed)
			{
				return;
			}
			while (!listenTask.IsCompleted)
			{
				await Task.Delay(10);
			}

			listenTask.Dispose();
			host.Flush();
			host.Dispose();
			ENet.Library.Deinitialize();
			IsDisposed = true;
		}

		private void Listen()
		{
			while (IsRunning)
			{
				host.Service(1000 / tickRateClient, out ENet.Event netEvent);

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
						if (bufferPointer.Count > 0)
						{
							startIndex = bufferPointer.Peek().Start;
						}

						Marshal.Copy(netEvent.Packet.Data, buffer, startIndex, length);
						bufferPointer.Enqueue(new BufferPointer(startIndex, length));

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