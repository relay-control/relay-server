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
	class DeviceState {
		public uint id { get; set; }
		public bool acquired { get; set; }
		public int numButtons { get; set; }
		public int numContPovs { get; set; }
		public int numDiscPovs { get; set; }
		public Dictionary<int, bool> axes { get; set; }
	}

	class DeviceStateEvent {
		public string eventType = "open";
		public List<DeviceState> devices = new List<DeviceState> { };

		public DeviceStateEvent(List<uint> _devices) {
			foreach (var device in _devices) {
				//devices.Add(new JoystickDevice(device).GetDeviceInfo());
			}
		}
	}

	class Camel {
		public static string Serialize(object o) {
			var serializerSettings = new JsonSerializerSettings();
			serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			return JsonConvert.SerializeObject(o, serializerSettings);
		}
	}

	class WebSocketConnection {
		public WebSocket _webSocket;
		public uint[] _devices;

		public WebSocketConnection(WebSocket webSocket, uint[] devices) {
			_webSocket = webSocket;
			_devices = devices;
		}

		public async Task SendMessage(string message) {
			await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length), WebSocketMessageType.Text, true, CancellationToken.None);
		}
	}

	public class WebSocketCollection {
		JoystickManager _joystickManager;

		List<object> connections = new List<object>();
		Dictionary<uint, List<WebSocket>> deviceConnections = new Dictionary<uint, List<WebSocket>>();

		public WebSocketCollection(JoystickManager joystickManager) {
			_joystickManager = joystickManager;
		}

		public void Add(WebSocket webSocket, uint[] devices) {
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

			WebSocketConnection socket = new WebSocketConnection(webSocket, devices);
			connections.Add(socket);
			var message = Camel.Serialize(response);
			socket.SendMessage(message);
		}

		public void Remove(WebSocket webSocket) {
			foreach (WebSocketConnection connection in connections) {
				if (connection._webSocket == webSocket) {
					connections.Remove(connection);
					// once no more clients use a single device it may be relinquished
					foreach (var device in connection._devices) {
						deviceConnections[device].Remove(webSocket);
						if (deviceConnections[device].Count == 0) {
							_joystickManager.RelinquishDevice(device);
							Console.WriteLine("Relinquished device {0}", device);
						}
					}
				}
			}
		}

		public void Broadcast(string key, bool state) {
			foreach (WebSocketConnection connection in connections) {
				dynamic response = new ExpandoObject();
				response.eventType = "state";
				response.key = key;
				response.state = state;

				var message = Camel.Serialize(response);
				connection.SendMessage(message);
			}
		}
	}
}
