using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recon.Core {
	class CommandEventArgs : EventArgs {
		public string Command { get; set; }
		public string Args { get; set; }
	}

	class Command : InputDevice {
		public void Process(Input input) {
			if (input.Type != "command") return;
			System.Diagnostics.Process.Start(input.Command, input.Args);
		}

		public void OnConnected(WebSocketConnection connection) {

		}

		public void OnDisconnected() {

		}
	}

	class CommandManager : IInputManager {
		public InputDevice CreateDevice() {
			return new Command();
		}
	}
}
