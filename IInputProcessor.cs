namespace Relay;

public interface IInputProcessor {
	InputType InputType { get; }
	void Process(InputMessageConverter inputMessage);
}
