using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Recon.Core {
	[Route("[controller]")]
	public class WebSocketController : ControllerBase {
		private readonly WebSocketCollection _webSocketCollection;
		private readonly IEnumerable<IInputMessageProcessor> _inputMessageProcessors;

		public WebSocketController(WebSocketCollection webSocketCollection, IEnumerable<IInputMessageProcessor> inputMessageProcessors) {
			Console.WriteLine("WebSocketController");
			_webSocketCollection = webSocketCollection;
			_inputMessageProcessors = inputMessageProcessors;
		}

		// GET: api/<controller>
		[HttpGet]
		public async Task Get(uint[] devices) {
			var context = HttpContext;
			if (context.WebSockets.IsWebSocketRequest) {
				await Connect(context, devices);
			}
		}

		public async Task Connect(HttpContext context, uint[] devices) {
			WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

			// register client with its requested devices
			_webSocketCollection.Add(webSocket, devices);

			await Echo(webSocket);

			_webSocketCollection.Remove(webSocket);
		}

		async Task Echo(WebSocket webSocket) {
			var buffer = new byte[1024 * 4];
			WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			while (!result.CloseStatus.HasValue) {
				var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

				var inputData = JsonConvert.DeserializeObject<InputMessage>(message);
				foreach (var processor in _inputMessageProcessors) {
					if (processor.Process(inputData)) break;
				}

				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}
			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
		}
	}
}
