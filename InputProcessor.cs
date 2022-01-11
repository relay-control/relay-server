namespace Relay;

public class InputProcessor {
	private readonly IEnumerable<IInputProcessor> _inputProcessors;

	public InputProcessor(IEnumerable<IInputProcessor> inputProcessors) {
		_inputProcessors = inputProcessors;
	}

	public void Process(InputMessage input) {
		var inputMessage = new InputMessageConverter(input.ExtensionData);
		foreach (var inputProcessor in _inputProcessors) {
			if (inputProcessor.InputType == input.Type) {
				inputProcessor.Process(inputMessage);
			}
		}
	}
}
