using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Relay {
	public interface IInputClient {
		Task ReceiveMessage(string user, string message);
	}

	public class InputHub : Hub<IInputClient> {
		private readonly ILogger _logger;
		private readonly InputProcessor _inputProcessor;
		private readonly MacroProcessor _macroProcessor;
		private readonly JoystickCollection _joystickCollection;
		private readonly PanelHosting _panelHosting;

		public InputHub(
			ILogger<InputHub> logger,
			InputProcessor inputProcessor,
			MacroProcessor macroProcessor,
			JoystickCollection joystickCollection,
			PanelHosting panelHosting
		) {
			_logger = logger;
			_inputProcessor = inputProcessor;
			_macroProcessor = macroProcessor;
			_joystickCollection = joystickCollection;
			_panelHosting = panelHosting;
		}

		public override async Task OnConnectedAsync() {
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception exception) {
			// relinquish devices, release buttons
			_joystickCollection.RelinquishDevice(Context.ConnectionId);
			await base.OnDisconnectedAsync(exception);
		}

		public async Task<DeviceProperties> AcquireDevice(int deviceId) {
			_joystickCollection.AcquireDevice(deviceId, Context.ConnectionId);
			return _joystickCollection.GetDeviceProperties(deviceId);
		}

		public async Task RelinquishDevice(int deviceId) {
			_joystickCollection.RelinquishDevice(deviceId, Context.ConnectionId);
		}

		public async Task<InputResult> SendInput(InputMessage input) {
			try {
				if (input.Type == InputType.Macro) {
					_macroProcessor.Process(input);
				} else {
					_inputProcessor.Process(input);
				}
				return new InputResult { Ok = true };
			} catch (Exception e) {
				return new InputResult {
					Ok = false,
					Message = e.Message,
				};
			}
		}

		public async Task<IEnumerable<string>> GetPanels() {
			return PanelHosting.GetPanels();
		}

		public async Task SendMessage(string user, string message) {
			await Clients.All.ReceiveMessage(user, message);
			_logger.LogInformation("Client sent message");
		}
	}
}
