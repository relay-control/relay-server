using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Relay {
	class JoystickMessage {
		public int DeviceId { get; set; }
		public int Button { get; set; }
		public bool IsPressed { get; set; }
	}

	class ButtonProcessor : IInputProcessor {
		public InputType InputType { get; } = InputType.Button;
		private readonly ILogger _logger;
		private readonly JoystickCollection _joystickCollection;

		public ButtonProcessor(ILogger<InputHub> logger, JoystickCollection joystickCollection) {
			_logger = logger;
			_joystickCollection = joystickCollection;
		}

		public void Process(InputMessageConverter inputMessage) {
			var input = inputMessage.GetInputDescriptor<JoystickMessage>();
			var device = _joystickCollection.GetDevice(input.DeviceId);
			device.SetButton(input.Button, input.IsPressed);
			_logger.LogInformation($"Button: {input.DeviceId} {input.Button} {input.IsPressed}");
		}
	}
}
