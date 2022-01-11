using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Relay;

public enum InputType {
	None,
	Key,
	Button,
	Axis,
	Command,
	Macro,
	Delay,
}

public class InputMessage {
	public InputType Type { get; set; }
	[JsonExtensionData]
	public JsonObject ExtensionData { get; set; }
}

public class InputMessageConverter {
	readonly JsonSerializerOptions SerializerOptions = new() {
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};
	private readonly JsonObject _extensionData;

	public InputMessageConverter(JsonObject extensionData) {
		_extensionData = extensionData;
	}

	public T GetInputDescriptor<T>() where T : class, new() {
		return JsonSerializer.Deserialize<T>(_extensionData, SerializerOptions);
	}
}
