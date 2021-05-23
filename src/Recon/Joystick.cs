using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace Recon.Core {
	public class JoystickManager {
		vJoy joystick = new vJoy();

		public JoystickManager() {
			if (!joystick.vJoyEnabled()) {
				Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.");
				return;
			}

			// test if DLL matches the driver
			uint DllVer = 0, DrvVer = 0;
			bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
			if (match)
				Console.WriteLine("Version of Driver Matches DLL Version ({0:X})", DllVer);
			else
				Console.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})", DrvVer, DllVer);
		}

		public VjdStat GetVJDStatus(uint deviceID) {
			return joystick.GetVJDStatus(deviceID);
		}

		public bool IsDeviceAcquired(uint id) {
			return joystick.GetVJDStatus(id) == VjdStat.VJD_STAT_OWN;
		}

		public bool AcquireDevice(uint deviceID) {
			if (!joystick.isVJDExists(deviceID)) {
				return false;
			}

			VjdStat status = joystick.GetVJDStatus(deviceID);
			switch (status) {
				case VjdStat.VJD_STAT_OWN:
					Console.WriteLine("vJoy Device {0} is already owned by this feeder", deviceID);
					return true;
				case VjdStat.VJD_STAT_FREE:
					Console.WriteLine("vJoy Device {0} is free", deviceID);
					break;
				case VjdStat.VJD_STAT_BUSY:
					Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue", deviceID);
					return false;
				case VjdStat.VJD_STAT_MISS:
					Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue", deviceID);
					return false;
				default:
					Console.WriteLine("vJoy Device {0} general error\nCannot continue", deviceID);
					return false;
			};

			if ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(deviceID))) {
				Console.WriteLine("Failed to acquire vJoy device number {0}.", deviceID);
				return false;
			} else
				Console.WriteLine("Acquired: vJoy device number {0}.", deviceID);

			ResetDeviceAxes(deviceID);

			return true;

			// joystick.RegisterRemovalCB(ChangedCB, label2);

			// void CALLBACK ChangedCB(bool Removed, bool First, object userData)
			/*
				This function is called when a process of vJoy device removal starts or ends and when a process of vJoy device
				arrival starts or ends. The function must return as soon as possible.
				• When a process of vJoy device removal starts, Parameter Removed = TRUE and parameter First = TRUE.
				• When a process of vJoy device removal ends, Parameter Removed = TRUE and parameter First = FALSE.
				• When a process of vJoy device arrival starts, Parameter Removed = FALSE and parameter First = TRUE.
				• When a process of vJoy device arrival ends, Parameter Removed = FALSE and parameter First = FALSE.
				Parameter userData is always an object registered as second parameter of function RegisterRemovalCB.
			*/

			// register device-client allocation
		}

		public void ResetDeviceAxes(uint deviceID) {
			foreach (var axis in Enum.GetValues(typeof(HID_USAGES))) {
				joystick.SetAxis(0x4000, deviceID, (HID_USAGES)axis);
			}
		}

		public void RelinquishDevice(uint deviceID) {
			joystick.RelinquishVJD(deviceID);
		}

		public bool IsDeviceEnabled(uint deviceID) {
			return joystick.isVJDExists(deviceID);
		}

		public int GetDeviceNumButtons(uint deviceID) {
			return joystick.GetVJDButtonNumber(deviceID);
		}

		public int GetDeviceNumContPovs(uint deviceID) {
			return joystick.GetVJDContPovNumber(deviceID);
		}

		public int GetDeviceNumDiscPovs(uint deviceID) {
			return joystick.GetVJDContPovNumber(deviceID);
		}

		public bool IsDeviceAxisEnabled(uint deviceID, int axis) {
			return joystick.GetVJDAxisExist(deviceID, (HID_USAGES)axis);
		}

		public void PressButton(int deviceID, byte button) {
			joystick.SetBtn(true, (uint)deviceID, button);
		}

		public void ReleaseButton(int deviceID, byte button) {
			joystick.SetBtn(false, (uint)deviceID, button);
		}

		public void SetAxis(int deviceID, byte axis, byte value) {
			// value: 0x0 - 0x4000
			joystick.SetAxis(value * 327, (uint)deviceID, (HID_USAGES)axis + 47);
			//Console.WriteLine("Device {0}, axis: {1}, value: {2}", input.Axis, input.Value);
		}
	}
}
