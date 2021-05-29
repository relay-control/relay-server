using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Relay {
	class MacroMessage {
		public List<InputMessage> Actions { get; set; }
	}

	class DelayMessage {
		public int Delay { get; set; }
	}

	public class MacroProcessor {
		private readonly ILogger _logger;
		private readonly InputProcessor _inputProcessor;

		public MacroProcessor(ILogger<InputHub> logger, InputProcessor inputProcessor) {
			_logger = logger;
			_inputProcessor = inputProcessor;
		}

		public void Process(InputMessage input2) {
			var inputMessage = new InputMessageConverter(input2.ExtensionData);
			var input = inputMessage.GetInputDescriptor<MacroMessage>();

			foreach (var action in input.Actions) {
				if (action.Type == InputType.Delay) {
					var inputMessage3 = new InputMessageConverter(action.ExtensionData);
					var input3 = inputMessage3.GetInputDescriptor<DelayMessage>();
					_logger.LogInformation($"Delay: {input3.Delay}");
					Thread.Sleep(input3.Delay);
				} else {
					_inputProcessor.Process(action);
				}
			}
		}
	}
}
