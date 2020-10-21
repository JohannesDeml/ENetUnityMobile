// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExampleEchoClientUi.cs">
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
using UnityEngine.UI;

namespace Supyrb
{
	public class ExampleEchoClientUi : MonoBehaviour
	{
		[SerializeField]
		private ExampleEchoClient client = null;

		#region UiFields

		[Header("Settings Input")]
		[SerializeField]
		private InputField serverIpInput = null;

		[SerializeField]
		private InputField serverPortInput = null;

		[SerializeField]
		private Button udpConnectButton = null;

		[SerializeField]
		private Button disconnectButton = null;

		[Header("Connection")]
		[SerializeField]
		private InputField messageInputField = null;

		[SerializeField]
		private Button sendMessageButton = null;

		[SerializeField]
		private Text serverResponseText = null;

		[SerializeField]
		private Text stateInfoText = null;

		#endregion

		private void Start()
		{
			udpConnectButton.onClick.AddListener(TriggerUdpConnect);
			disconnectButton.onClick.AddListener(TriggerDisconnect);
			sendMessageButton.onClick.AddListener(OnSendEcho);
		}

		public void UpdateState(ENetClient client)
		{
			UpdateStateInfoText(client);
			bool connected = client != null && client.IsConnected;
			sendMessageButton.interactable = connected;
			disconnectButton.interactable = connected;
			udpConnectButton.interactable = !connected;
		}

		private void UpdateStateInfoText(ENetClient client)
		{
			if (client == null)
			{
				return;
			}

			var text = $"Server ip: {client.Address.GetHost()}, Server port: {client.Address.Port}\n" +
			           $"IsConnected: {client.IsConnected}\n ";
			stateInfoText.text = text;
		}

		private void TriggerUdpConnect()
		{
			Connect();
		}

		private void TriggerDisconnect()
		{
			client.Disconnect();
		}

		private void Connect()
		{
			var serverIp = serverIpInput.text;
			var serverPort = int.Parse(serverPortInput.text);
			client.ApplyInputAndConnect(serverIp, serverPort);
		}

		[ContextMenu("Send message")]
		private void OnSendEcho()
		{
			var message = Encoding.UTF8.GetBytes(messageInputField.text);
			client.SendEcho(message);
		}

		public void AddResponseText(string messages)
		{
			serverResponseText.text = $"{messages}\n{serverResponseText.text}";
		}
	}
}