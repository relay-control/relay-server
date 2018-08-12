using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recon.Core {
	class KeyEventArgs : EventArgs {
		public string Key { get; set; }
	}

	class Keyboard : InputDevice {
		List<string> pressedKeys = new List<string> { };

		public void OnConnected(WebSocketConnection connection) { }

		public void Process(Input input) {
			if (input.Type == "key") {
				Console.WriteLine("Key: {0}, state: {1}", input.Key, input.State);
				if (input.State) {
					PressKey(input.Key);
				} else {
					ReleaseKey(input.Key);
				}
			}
		}

		void PressKey(string key) {
			pressedKeys.Add(key);
			var args = new KeyEventArgs();
			args.Key = key;
			OnKeyPressed(args);
		}

		void ReleaseKey(string key) {
			pressedKeys.Remove(key);
			if (pressedKeys.Contains(key)) {
				return;
			}
			var args = new KeyEventArgs();
			args.Key = key;
			OnKeyReleased(args);
		}

		public void OnDisconnected() {
			pressedKeys.ForEach(key => ReleaseKey(key));
		}

		public bool IsKeyPressed(string key) {
			return pressedKeys.Contains(key);
		}

		public event EventHandler<KeyEventArgs> KeyPressed;

		protected virtual void OnKeyPressed(KeyEventArgs e) {
			KeyPressed?.Invoke(this, e);
		}

		public event EventHandler<KeyEventArgs> KeyReleased;

		protected virtual void OnKeyReleased(KeyEventArgs e) {
			KeyReleased?.Invoke(this, e);
		}
	}

	class KeyboardManager : IInputManager {
		Keyboard2 keyboard = new Keyboard2();

		List<Keyboard> keyboards = new List<Keyboard> { };

		public InputDevice CreateDevice() {
			var keyboard = new Keyboard();
			keyboards.Add(keyboard);
			keyboard.KeyPressed += OnKeyPressed;
			keyboard.KeyReleased += OnKeyReleased;
			return keyboard;
		}

		void OnKeyPressed(object kbd, KeyEventArgs e) {
			PressKey(e.Key);
		}

		void OnKeyReleased(object kbd, KeyEventArgs e) {
			if (keyboards.Any(kbd2 => kbd2.IsKeyPressed(e.Key))) {
				return;
			}
			ReleaseKey(e.Key);
		}

		void PressKey(string key) {
			SendInput(key);
			keyboard.PressKey(key);
		}

		void ReleaseKey(string key) {
			SendInput(key);
			keyboard.ReleaseKey(key);
		}

		public static void SendInput(object input) { }
	}
}
