using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GhostJoystick;

namespace Relay {
	public class DeviceProperties {
		public int Id { get; set; }
		public bool IsAcquired { get; set; }
		public int NumButtons { get; set; }
		public int NumContinuousPovs { get; set; }
		public int NumDiscretePovs { get; set; }
		public List<Axis> Axes { get; set; } = new();
	}

	public class JoystickCollection {
		readonly Dictionary<int, Device> AcquiredDevices = new();

		public void AcquireDevice(int deviceId, string clientId) {
			if (JoystickController.DeviceIsEnabled(deviceId)
			 && JoystickController.GetDeviceStatus(deviceId) == DeviceStatus.Free) {
				var joystick = JoystickController.AcquireDevice(deviceId);
				AcquiredDevices.Add(deviceId, new(joystick, new()));
			}
			if (JoystickController.DeviceIsAcquired(deviceId)) {
				AcquiredDevices[deviceId].Clients.Add(clientId);
			}
		}

		public void RelinquishDevice(int deviceId, string clientId) {
			if (JoystickController.DeviceIsAcquired(deviceId)) {
				AcquiredDevices[deviceId].Clients.Remove(clientId);
			}
			if (AcquiredDevices.ContainsKey(deviceId) && AcquiredDevices[deviceId].Clients.Count == 0) {
				AcquiredDevices.Remove(deviceId);
				JoystickController.RelinquishDevice(deviceId);
			}
		}

		public void RelinquishDevice(string clientId) {
			foreach (var e in AcquiredDevices) {
				if (e.Value.Clients.Contains(clientId)) {
					RelinquishDevice(e.Key, clientId);
				}
			}
		}

		public Joystick GetDevice(int deviceId) {
			return AcquiredDevices[deviceId]?.Joystick;
		}

		internal DeviceProperties GetDeviceProperties(int deviceId) {
			var deviceProperties = new DeviceProperties {
				Id = deviceId,
			};
			if (AcquiredDevices.ContainsKey(deviceId)) {
				var device = GetDevice(deviceId);
				deviceProperties.IsAcquired = true;
				deviceProperties.NumButtons = device.GetNumButtons();
				deviceProperties.NumContinuousPovs = device.GetNumContinuousPovs();
				deviceProperties.NumDiscretePovs = device.GetNumDiscretePovs();
				foreach (var axis in Enum.GetValues(typeof(Axis))) {
					if (device.AxisIsEnabled((Axis)axis)) {
						deviceProperties.Axes.Add((Axis)axis);
					}
				}
			} else {
				deviceProperties.IsAcquired = false;
			}
			return deviceProperties;
		}

		record Device(Joystick Joystick, List<string> Clients);
	}
}
