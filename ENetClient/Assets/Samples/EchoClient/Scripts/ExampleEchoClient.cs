// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExampleEchoClient.cs">
//   Copyright (c) 2020 Johannes Deml. All rights reserved.
// </copyright>
// <author>
//   Johannes Deml
//   public@deml.io
// </author>
// --------------------------------------------------------------------------------------------------------------------

using System.Text;
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

		private ENetClient _client;

		private void Start()
		{
			_client = new ENetClient();
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

		private void Update()
		{
			ui.UpdateState(_client);
			bool connected = _client != null && _client.IsConnected;

			if (!connected || _client.BufferPointer.Count == 0)
			{
				return;
			}

			while (_client.BufferPointer.Count > 0)
			{
				(int start, int length) = _client.BufferPointer.Dequeue();

				var message = Encoding.UTF8.GetString(_client.Buffer, start, length);
				ui.AddResponseText(message);
			}
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
		}
	}
}