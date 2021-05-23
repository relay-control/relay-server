using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recon.Core {
	class Macro : InputDevice {
		public List<InputDevice> devices;

		public void OnConnected(WebSocketConnection connection) { }
		public void OnDisconnected() { }

		public void Process(Input input) {
			if (input.Type != "macro") return;
			Task.Run(() => {
				foreach (var action in input.Actions) {
					foreach (var device in devices) {
						device.Process(action);
					}
					if (action.Type == "delay") {
						Thread.Sleep(action.Delay);
					}
				}
			});
		}
	}

	public class MacroManager {
		public InputDevice CreateDevice(List<InputDevice> devices) {
			var macroDevice = new Macro();
			macroDevice.devices = devices;
			return macroDevice;
		}
	}
}
