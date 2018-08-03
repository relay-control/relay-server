using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
#define HID_USAGE_X		0x30
#define HID_USAGE_Y		0x31
#define HID_USAGE_Z		0x32
#define HID_USAGE_RX	0x33
#define HID_USAGE_RY	0x34
#define HID_USAGE_RZ	0x35
#define HID_USAGE_SL0	0x36
#define HID_USAGE_SL1	0x37
#define HID_USAGE_WHL	0x38
#define HID_USAGE_POV	0x39
*/

namespace Recon.Core {
	public class InputMessage {
		public string Type { get; set; }

		public string Key { get; set; }

		public byte Device { get; set; }
		public string InputType { get; set; }

		public byte Button { get; set; }
		public bool State { get; set; }

		public byte Axis { get; set; }
		public byte Value { get; set; }
	}

	public interface IInputMessageProcessor {
		bool Process(InputMessage input);
	}

	public class InputMessageProcessor {
		private readonly IEnumerable<IInputMessageProcessor> _inputMessageProcessors;

		public InputMessageProcessor(IEnumerable<IInputMessageProcessor> inputMessageProcessors) {
			_inputMessageProcessors = inputMessageProcessors;
		}

		public bool Process(InputMessage input) {
			foreach (var processor in _inputMessageProcessors) {
				if (processor.Process(input)) {
					return true;
				}
			}
			return false;
		}
	}

	class KeyboardMessageProcessor : IInputMessageProcessor {
		Keyboard keyboard = new Keyboard();

		public KeyboardMessageProcessor() {
		}

		public bool Process(InputMessage input) {
			if (input.Type == "key") {
				Console.WriteLine("Key: {0}, state: {1}", input.Key, input.State);
				if (input.State) {
					keyboard.PressKey(input.Key);
				} else {
					keyboard.ReleaseKey(input.Key);
				}
				return true;
			}
			return false;
		}
	}

	class JoystickMessageProcessor : IInputMessageProcessor {
		JoystickManager _joystickManager;

		public JoystickMessageProcessor(JoystickManager joystickManager) {
			_joystickManager = joystickManager;
		}

		public bool Process(InputMessage input) {
			if (input.Type == "button") {
				Console.WriteLine("Button: {0}, state: {1}", input.Button, input.State);
				if (input.State) {
					_joystickManager.PressButton(input.Device, input.Button);
				} else {
					_joystickManager.ReleaseButton(input.Device, input.Button);
				}
				return true;
			}
			if (input.Type == "axis") {
				Console.WriteLine("Axis: {0}, value: {1}", input.Axis, input.Value);
				_joystickManager.SetAxis(input.Device, input.Axis, input.Value);
				return true;
			}
			if (input.Type == "macro") {
				// yes let's clam
				return true;
			}
			// send changes to clients that use the same device
			return false;
		}
	}
}
