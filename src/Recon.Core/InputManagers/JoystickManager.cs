using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recon.Core {
	class JoystickConnectionEventArgs : EventArgs {
		public WebSocketConnection Connection { get; set; }
		public uint[] Devices { get; set; }
	}

	class JoystickDeviceEventArgs : EventArgs {
		public uint DeviceID { get; set; }
		public bool Acquired { get; set; }
	}

	class JoystickEventArgs : EventArgs {
		public uint DeviceID { get; set; }
		public byte Button { get; set; }
	}

	class Joystick : InputDevice {
		List<uint> ownedDevices = new List<uint> { };
		Dictionary<uint, List<byte>> pressedButtons = new Dictionary<uint, List<byte>> { };

		public void OnConnected(WebSocketConnection connection) {
			foreach (var device in connection._ownedDevices) {
				RequestDevice(device);
			}
			var args = new JoystickConnectionEventArgs();
			args.Connection = connection;
			args.Devices = connection._ownedDevices;
			OnConnected(args);
		}

		public void OnDisconnected() {
			var ownedDevices2 = new List<uint>(ownedDevices);
			ownedDevices.Clear();
			foreach (var device in ownedDevices2) {
				foreach (var button in pressedButtons[device]) {
					var args = new JoystickEventArgs();
					args.DeviceID = device;
					args.Button = button;
					OnButtonReleased(args);
				}
				var args2 = new JoystickDeviceEventArgs();
				args2.DeviceID = device;
				OnJoystickRelinquished(args2);
			}
			pressedButtons.Clear();
		}

		public void Process(Input input) {
			if (!IsDeviceOwned(input.DeviceID)) return;
			if (input.Type == "button") {
				Console.WriteLine("Button: {0}, state: {1}", input.Button, input.State);
				if (input.State) {
					PressButton(input.DeviceID, input.Button);
				} else {
					ReleaseButton(input.DeviceID, input.Button);
				}
			}
			if (input.Type == "axis") {
			}
		}

		void PressButton(uint deviceID, byte button) {
			pressedButtons[deviceID].Add(button);
			var args = new JoystickEventArgs();
			args.DeviceID = deviceID;
			args.Button = button;
			OnButtonPressed(args);
		}

		void ReleaseButton(uint deviceID, byte button) {
			pressedButtons[deviceID].Remove(button);
			if (IsButtonPressed(deviceID, button)) {
				return;
			}
			var args = new JoystickEventArgs();
			args.DeviceID = deviceID;
			args.Button = button;
			OnButtonReleased(args);
		}

		void RequestDevice(uint deviceID) {
			var args = new JoystickDeviceEventArgs();
			args.DeviceID = deviceID;
			OnJoystickRequested(args);
			if (args.Acquired) {
				ownedDevices.Add(deviceID);
				pressedButtons.Add(deviceID, new List<byte> { });
			}
		}

		void RelinquishDevice(uint deviceID) {
			ownedDevices.Remove(deviceID);
			pressedButtons.Remove(deviceID);
			var args = new JoystickDeviceEventArgs();
			args.DeviceID = deviceID;
			OnJoystickRelinquished(args);
		}

		public bool IsDeviceOwned(uint deviceID) {
			return ownedDevices.Contains(deviceID);
		}

		public bool IsButtonPressed(uint deviceID, byte button) {
			return IsDeviceOwned(deviceID) && pressedButtons[deviceID].Contains(button);
		}

		public event EventHandler<JoystickConnectionEventArgs> Connected;

		protected virtual void OnConnected(JoystickConnectionEventArgs e) {
			Connected?.Invoke(this, e);
		}

		public event EventHandler<JoystickDeviceEventArgs> JoystickRequested;

		protected virtual void OnJoystickRequested(JoystickDeviceEventArgs e) {
			JoystickRequested?.Invoke(this, e);
		}

		public event EventHandler<JoystickDeviceEventArgs> JoystickRelinquished;

		protected virtual void OnJoystickRelinquished(JoystickDeviceEventArgs e) {
			JoystickRelinquished?.Invoke(this, e);
		}

		public event EventHandler<JoystickEventArgs> ButtonPressed;

		protected virtual void OnButtonPressed(JoystickEventArgs e) {
			ButtonPressed?.Invoke(this, e);
		}

		public event EventHandler<JoystickEventArgs> ButtonReleased;

		protected virtual void OnButtonReleased(JoystickEventArgs e) {
			ButtonReleased?.Invoke(this, e);
		}
	}

	public class JoystickMgr : IInputManager {
		JoystickManager joystickManager = new JoystickManager();

		List<Joystick> joysticks = new List<Joystick> { };

		public InputDevice CreateDevice() {
			var joystick = new Joystick();
			joysticks.Add(joystick);
			joystick.Connected += OnConnected;
			joystick.ButtonPressed += OnButtonPressed;
			joystick.ButtonReleased += OnButtonReleased;
			joystick.JoystickRequested += OnJoystickRequested;
			joystick.JoystickRelinquished += OnJoystickRelinquished;
			return joystick;
		}

		void OnConnected(object joystick, JoystickConnectionEventArgs e) {
			e.Connection.SendMessage(CamelCaseSerializer.Serialize(GetOpenEvent(e.Devices)));
		}

		public dynamic GetOpenEvent(uint[] devices) {
			//var response = new DeviceStateEvent(devices);
			dynamic response = new ExpandoObject();
			response.eventType = "open";
			response.devices = new List<dynamic> { };
			foreach (var device in devices) {
				dynamic deviceData = new ExpandoObject();
				deviceData.id = device;
				deviceData.acquired = joystickManager.IsDeviceAcquired(device);
				if (deviceData.acquired) {
					deviceData.numButtons = joystickManager.GetDeviceNumButtons(device);
					deviceData.numContPovs = joystickManager.GetDeviceNumContPovs(device);
					deviceData.numDiscPovs = joystickManager.GetDeviceNumDiscPovs(device);

					deviceData.axes = new Dictionary<int, bool>();
					int[] axes = (int[])Enum.GetValues(typeof(HID_USAGES));
					for (int i = 0; i < axes.Length; i++) {
						deviceData.axes[i + 1] = joystickManager.IsDeviceAxisEnabled(device, axes[i]);
					}
				}
				response.devices.Add(deviceData);
			}
			return response;
		}

		void PressButton(uint deviceID, byte button) {
			joystickManager.PressButton((int)deviceID, button);
		}

		void ReleaseButton(uint deviceID, byte button) {
			joystickManager.ReleaseButton((int)deviceID, button);
		}

		void OnButtonPressed(object joystick, JoystickEventArgs e) {
			PressButton(e.DeviceID, e.Button);
		}

		void OnButtonReleased(object joystick, JoystickEventArgs e) {
			if (joysticks.Any(j => j.IsButtonPressed(e.DeviceID, e.Button))) {
				return;
			}
			ReleaseButton(e.DeviceID, e.Button);
		}

		void OnJoystickRequested(object joystick, JoystickDeviceEventArgs e) {
			var acquired = joystickManager.AcquireDevice(e.DeviceID);
			e.Acquired = acquired;
		}

		void OnJoystickRelinquished(object joystick, JoystickDeviceEventArgs e) {
			// once a device is no longer used by any clients it should get relinquished
			if (joysticks.Any(j => j.IsDeviceOwned(e.DeviceID))) {
				return;
			}
			joystickManager.RelinquishDevice(e.DeviceID);
			Console.WriteLine("Relinquished device {0}", e.DeviceID);
		}
	}
}
