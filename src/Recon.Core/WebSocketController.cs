using System;
using System.Collections.Generic;
using System.Dynamic;
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
		private readonly WebSocketManager _webSocketManager;

		public WebSocketController(WebSocketManager webSocketManager) {
			Console.WriteLine("WebSocketController");
			_webSocketManager = webSocketManager;
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
			var webSocket = await context.WebSockets.AcceptWebSocketAsync();
			await _webSocketManager.OnConnected(webSocket, devices);
		}
	}
}
