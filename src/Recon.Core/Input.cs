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

		public int Delay { get; set; }

		public Input[] Actions { get; set; }
	}

	public class Input : InputMessage {
		public uint DeviceID { get; set; }
	}

	public interface IInputMessageProcessor {
		bool Process(InputMessage input);
	}

	public interface InputDevice {
		void Process(Input input);
		void OnConnected(WebSocketConnection connection);
		void OnDisconnected();
	}

	public interface IInputManager {
		InputDevice CreateDevice();
	}
}
