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

		private ENetClient client;

		private void Start()
		{
			client = new ENetClient();
		}

		private void OnDestroy()
		{
			Disconnect();
		}

		[ContextMenu("Connect")]
		private void Connect()
		{
			client.Connect(serverIp, serverPort);
		}

		[ContextMenu("Disconnect")]
		public void Disconnect()
		{
			client.Disconnect();
		}

		private void Update()
		{
			ui.UpdateState(client);
			bool connected = client != null && client.IsConnected;

			if (!connected || client.BufferPointer.Count == 0)
			{
				return;
			}

			while (client.BufferPointer.Count > 0)
			{
				var pointer = client.BufferPointer.Dequeue();

				var message = Encoding.UTF8.GetString(client.Buffer, pointer.Start, pointer.Length);
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
				client.Send(message);
			}
		}

		private void OnApplicationQuit()
		{
			client.Dispose();
		}
	}
}