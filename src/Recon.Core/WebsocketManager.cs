using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Recon.Core {
	class CamelCaseSerializer {
		public static string Serialize(object value) {
			return JsonConvert.SerializeObject(value, new JsonSerializerSettings() {
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
			});
		}
	}

	public class WebSocketConnection {
		public WebSocket _webSocket;
		public List<InputDevice> devices = new List<InputDevice> { };
		public uint[] _ownedDevices;

		public WebSocketConnection(WebSocket webSocket, IEnumerable<IInputManager> inputManagers, uint[] devices) {
			_webSocket = webSocket;
			_ownedDevices = devices;
			foreach (var v in inputManagers) {
				this.devices.Add(v.CreateDevice());
			}
			this.devices.ForEach(i => i.OnConnected(this));
		}

		public async Task SendMessage(string message) {
			var buffer = new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length);
			await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
		}

		public async Task Receive() {
			var buffer = new byte[1024 * 4];
			var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			while (!result.CloseStatus.HasValue) {
				var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

				var inputData = JsonConvert.DeserializeObject<Input>(message);
				devices.ForEach(d => d.Process(inputData));

				result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}
			await _webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
		}
	}

	public class WebSocketCollection {
		public List<WebSocketConnection> _connections = new List<WebSocketConnection>();

		public WebSocketConnection CreateConnection(WebSocket webSocket, IEnumerable<IInputManager> inputManagers, uint[] devices) {
			var socket = new WebSocketConnection(webSocket, inputManagers, devices);
			_connections.Add(socket);
			return socket;
		}

		public void DestroyConnection(WebSocketConnection connection) {
			_connections.Remove(connection);
		}
	}

	public class WebSocketManager {
		private readonly IEnumerable<IInputManager> _inputManagers;

		WebSocketCollection webSocketCollection = new WebSocketCollection();

		public WebSocketManager(IEnumerable<IInputManager> inputManagers) {
			_inputManagers = inputManagers;
		}

		public async Task OnConnected(WebSocket webSocket, uint[] devices) {
			var connection = webSocketCollection.CreateConnection(webSocket, _inputManagers, devices);

			await connection.Receive();

			webSocketCollection.DestroyConnection(connection);

			connection.devices.ForEach(i => i.OnDisconnected());
		}

		public async Task Broadcast(string message) {
			foreach (var connection in webSocketCollection._connections) {
				await connection.SendMessage(message);
			}
		}
	}
}
