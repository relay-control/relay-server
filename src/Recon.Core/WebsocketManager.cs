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

namespace Recon.Core
{
	public class WebsocketCollection
	{
		Joystick joystick = new Joystick();

		internal static List<object> connections = new List<object>();
		Dictionary<int, List<WebSocket>> deviceConnections = new Dictionary<int, List<WebSocket>>();

		public List<object> Greet()
		{
			return connections;
		}

		public void Add(WebSocket s, StringValues devices)
		{
			connections.Add(s);
			foreach (var device in devices)
			{
				if (joystick.AcquireDevice(Convert.ToUInt32(device)))
				{
					if (!deviceConnections.ContainsKey(Convert.ToInt32(device)))
						deviceConnections[Convert.ToInt32(device)] = new List<WebSocket>();
					deviceConnections[Convert.ToInt32(device)].Add(s);
				}
			}
		}

		public void Remove(WebSocket s)
		{
			// once no more clients use a single device it may be relinquished
			foreach (var device in deviceConnections.Keys)
			{
				deviceConnections[device].Remove(s);
				if (deviceConnections[device].Count == 0)
				{
					joystick.RelinquishDevice((uint)device);
					Console.WriteLine("Relinquished device " + device);
				}
			}
		}
	}

	class WebsocketManager
	{
		private readonly RequestDelegate _next;
		private readonly WebsocketCollection _greeter;

		IList<IInputMessageProcessor> processors = new List<IInputMessageProcessor> {
			new KeyboardMessageProcessor(),
			new JoystickMessageProcessor(),
		};

		Joystick joystick = new Joystick();

		public WebsocketManager(RequestDelegate next, WebsocketCollection greeter)
		{
			_next = next;
			_greeter = greeter;
			Console.WriteLine("lul");
		}

		public async Task Invoke(HttpContext context)
		{
			if (context.WebSockets.IsWebSocketRequest)
			{
				await Connect(context);
			}
			else
			{
				await _next(context);
			}
		}

		public async Task Connect(HttpContext context)
		{
			WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

			dynamic response = new ExpandoObject();
			response.eventType = "open";
			response.devices = new List<dynamic> { };
			var devices = context.Request.Query["devices"];
			foreach (var device in devices)
			{
				Console.WriteLine(device);
				response.devices.Add(GetDeviceInfo(Convert.ToUInt32(device)));
			}

			// register client with its requested devices
			_greeter.Add(webSocket, devices);

			var message = JsonConvert.SerializeObject(response);
			await webSocket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length), WebSocketMessageType.Text, true, CancellationToken.None);

			await Echo(context, webSocket);
		}

		private async Task Echo(HttpContext context, WebSocket webSocket)
		{
			var buffer = new byte[1024 * 4];
			WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			while (!result.CloseStatus.HasValue)
			{
				var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

				// convert JSON string into input object
				var inputData = JsonConvert.DeserializeObject<InputMessage>(message);
				foreach (var processor in processors)
				{
					if (processor.Process(inputData)) break;
				}

				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}
			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

			// relinquish devices
			_greeter.Remove(webSocket);
		}

		public dynamic GetDeviceInfo(uint deviceID)
		{
			dynamic deviceData = new ExpandoObject();
			deviceData.id = deviceID;
			deviceData.acquired = joystick.AcquireDevice(deviceID);
			if (deviceData.acquired)
			{
				deviceData.numButtons = joystick.GetDeviceNumButtons(deviceID);
				deviceData.numContPovs = joystick.GetDeviceNumContPovs(deviceID);
				deviceData.numDiscPovs = joystick.GetDeviceNumDiscPovs(deviceID);

				deviceData.axes = new Dictionary<int, bool>();
				int[] axes = (int[])Enum.GetValues(typeof(HID_USAGES));
				for (int i = 0; i < axes.Length; i++)
				{
					deviceData.axes[i + 1] = joystick.IsDeviceAxisEnabled(deviceID, axes[i]);
				}
			}
			return deviceData;
		}
	}
}
