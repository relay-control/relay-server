using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GhostKeyboard;
using Microsoft.Extensions.Logging;

namespace Recon {
	class KeyboardMessage {
		public string Key { get; set; }
		public int Modifiers { get; set; }
		public bool IsPressed { get; set; }
	}

	class KeyboardProcessor : IInputProcessor {
		public InputType InputType { get; } = InputType.Key;
		private readonly ILogger _logger;

		public KeyboardProcessor(ILogger<InputHub> logger) {
			_logger = logger;
		}

		public void Process(InputMessageConverter inputMessage) {
			var input = inputMessage.GetInputDescriptor<KeyboardMessage>();

			var virtualKey = KeyChord.GetVirtualKey(input.Key);
			if (virtualKey != 0) {
				Keyboard.SetKey(virtualKey, (ModifierKeys)input.Modifiers, input.IsPressed);
			} else {
				Keyboard.SetKey(input.Key[0], (ModifierKeys)input.Modifiers, input.IsPressed);
			}
			_logger.LogInformation($"Key: {input.Key} {input.IsPressed}");
		}
	}
}
