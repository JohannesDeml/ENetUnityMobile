// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExampleEchoClient.cs">
//   Copyright (c) 2020 Johannes Deml. All rights reserved.
// </copyright>
// <author>
//   Johannes Deml
//   public@deml.io
// </author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetCoreServer;
using UnityEngine;

namespace Supyrb
{
	public class ExampleEchoClient : MonoBehaviour
	{
		[SerializeField]
		private ExampleEchoClientUi ui = null;

		#region SettingFields

		[SerializeField]
		private string serverIp = "127.0.0.1";

		[SerializeField]
		private int serverPort = 3333;

		[Tooltip("Number of times the message is repeated to simulate more requests.")]
		[SerializeField]
		private int repeatMessage = 0;

		#endregion

		// Buffer will be used for sending and receiving to avoid creating garbage
		private byte[] buffer;
		private bool disconnecting;
		private bool applicationQuitting;

		private ENetClient _client;
		private BenchmarkData _benchmarkData;
		
		private void Start()
		{
			disconnecting = false;
			_benchmarkData = new BenchmarkData();
			_client = new ENetClient(_benchmarkData);
		}

		private void OnDestroy()
		{
			Disconnect();
		}

		[ContextMenu("Connect")]
		private void Connect()
		{
			_client.Connect(serverIp, serverPort);
		}

		[ContextMenu("Disconnect")]
		public void Disconnect()
		{
			_client.Disconnect();	
		}

		private int _lastReceivedMessages = 0;
		private void Update()
		{
			ui.UpdateState(_client);
			bool connected = _client != null && _client.IsConnected;

			if (!connected || _benchmarkData.MessagesClientReceived == _lastReceivedMessages)
			{
				return;
			}

			string messages = $"Messages received at frame {Time.frameCount}: {_benchmarkData.MessagesClientReceived}\n";
			_lastReceivedMessages = _benchmarkData.MessagesClientReceived;

			ui.AddResponseText(messages);
		}
		

		private void TriggerDisconnect()
		{
			Disconnect();
		}

		public void ApplyInputAndConnect(string serverIpInput, int serverPortInput)
		{
			serverIp = serverIpInput;
			serverPort = serverPortInput;
			Connect();
		}

		public void SendEcho(byte[] message)
		{
			Send(message);
		}

		private void Send(byte[] message)
		{
			for (int i = 0; i < 1 + repeatMessage; i++)
			{
				_client.Send(message);
			}
		}

		private void OnApplicationQuit()
		{
			_client.Dispose();
			applicationQuitting = true;
		}
	}
}
