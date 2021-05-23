using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Recon {
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
		public Dictionary<string, JsonElement> ExtensionData { get; set; }
	}

	public class InputMessageConverter {
		readonly JsonSerializerOptions SerializerOptions = new() {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};
		private readonly Dictionary<string, JsonElement> _extensionData;

		public InputMessageConverter(Dictionary<string, JsonElement> extensionData) {
			_extensionData = extensionData;
		}

		public T GetInputDescriptor<T>() where T : class, new() {
			var json = JsonSerializer.Serialize(_extensionData);
			return JsonSerializer.Deserialize<T>(json, SerializerOptions);
		}
	}
}
