using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GhostJoystick;
using Microsoft.Extensions.Logging;

namespace Recon {
	class AxisMessage {
		public int DeviceId { get; set; }
		public Axis Axis { get; set; }
		public int Value { get; set; }
	}

	class AxisProcessor : IInputProcessor {
		public InputType InputType { get; } = InputType.Axis;
		private readonly ILogger _logger;
		private readonly JoystickCollection _joystickCollection;

		public AxisProcessor(ILogger<InputHub> logger, JoystickCollection joystickCollection) {
			_logger = logger;
			_joystickCollection = joystickCollection;
		}

		public void Process(InputMessageConverter inputMessage) {
			var input = inputMessage.GetInputDescriptor<AxisMessage>();
			var device = _joystickCollection.GetDevice(input.DeviceId);
			device.SetAxis(input.Axis, input.Value);
			_logger.LogInformation($"Axis: {input.DeviceId} {input.Axis} {input.Value}");
		}
	}
}
