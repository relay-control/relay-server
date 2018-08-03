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
		public uint[] _ownedDevices;

		public WebSocketConnection(WebSocket webSocket, uint[] devices) {
			_webSocket = webSocket;
			_ownedDevices = devices;
		}

		public async Task SendMessage(string message) {
			var buffer = new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length);
			await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
		}
	}

	public class WebSocketCollection {
		public List<WebSocketConnection> _connections = new List<WebSocketConnection>();

		public WebSocketConnection CreateConnection(WebSocket webSocket, uint[] devices) {
			var socket = new WebSocketConnection(webSocket, devices);
			_connections.Add(socket);
			return socket;
		}

		public void DestroyConnection(WebSocketConnection connection) {
			_connections.Remove(connection);
		}
	}

	public class WebSocketManager {
		private readonly InputMessageProcessor _inputMessageProcessor;
		private readonly JoystickManager _joystickManager;

		WebSocketCollection webSocketCollection = new WebSocketCollection();

		public WebSocketManager(JoystickManager joystickManager, InputMessageProcessor inputMessageProcessor) {
			_joystickManager = joystickManager;
			_inputMessageProcessor = inputMessageProcessor;
		}

		public async Task OnConnected(WebSocket webSocket, uint[] devices) {
			// register client with its requested devices
			var connection = webSocketCollection.CreateConnection(webSocket, devices.Where(e => _joystickManager.AcquireDevice(e)).ToArray());

			var message = CamelCaseSerializer.Serialize(GetOpenEvent(devices));
			await connection.SendMessage(message);

			await Receive(webSocket);

			webSocketCollection.DestroyConnection(connection);

			// once a device is no longer used by any clients it should get relinquished
			foreach (var device in connection._ownedDevices) {
				if (!IsDeviceInUse(device)) {
					_joystickManager.RelinquishDevice(device);
					Console.WriteLine("Relinquished device {0}", device);
				}
			}
			//RelinquishUnusedDevices();
		}

		public async Task Receive(WebSocket webSocket) {
			var buffer = new byte[1024 * 4];
			var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			while (!result.CloseStatus.HasValue) {
				var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

				var inputData = JsonConvert.DeserializeObject<InputMessage>(message);
				if (_inputMessageProcessor.Process(inputData)) {
					dynamic response = new ExpandoObject();
					response.eventType = "state";
					response.state = inputData;
					var message2 = CamelCaseSerializer.Serialize(response);
					await Broadcast(message2);
				}

				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}
			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
		}

		public async Task Broadcast(string message) {
			foreach (var connection in webSocketCollection._connections) {
				await connection.SendMessage(message);
			}
		}

		public dynamic GetOpenEvent(uint[] devices) {
			//var response = new DeviceStateEvent(devices);
			dynamic response = new ExpandoObject();
			response.eventType = "open";
			response.devices = new List<dynamic> { };
			foreach (var device in devices) {
				//_joystickManager.AcquireDevice(webSocket, device);
				dynamic deviceData = new ExpandoObject();
				deviceData.id = device;
				deviceData.acquired = _joystickManager.AcquireDevice(device);
				if (deviceData.acquired) {
					deviceData.numButtons = _joystickManager.GetDeviceNumButtons(device);
					deviceData.numContPovs = _joystickManager.GetDeviceNumContPovs(device);
					deviceData.numDiscPovs = _joystickManager.GetDeviceNumDiscPovs(device);

					deviceData.axes = new Dictionary<int, bool>();
					int[] axes = (int[])Enum.GetValues(typeof(HID_USAGES));
					for (int i = 0; i < axes.Length; i++) {
						deviceData.axes[i + 1] = _joystickManager.IsDeviceAxisEnabled(device, axes[i]);
					}
				}
				response.devices.Add(deviceData);
			}
			return response;
		}

		void RelinquishUnusedDevices() {
		}

		public bool IsDeviceInUse(uint device) {
			return webSocketCollection._connections.Any(e => e._ownedDevices.Contains(device));
		}
	}
}
