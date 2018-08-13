using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Recon.Core {
	class Keyboard2 {
		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr process);

		[DllImport("user32.dll")]
		static extern IntPtr GetKeyboardLayout(uint idThread);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern ushort MapVirtualKey(uint uCode, uint uMapType);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

		[DllImport("user32.dll", SetLastError = true)]
		static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputs);

		[StructLayout(LayoutKind.Sequential)]
		internal struct INPUT {
			public uint Type;
			public MOUSEKEYBDHARDWAREINPUT Data;
		}

		[StructLayout(LayoutKind.Explicit)]
		internal struct MOUSEKEYBDHARDWAREINPUT {
			[FieldOffset(0)]
			public HARDWAREINPUT Hardware;
			[FieldOffset(0)]
			public KEYBDINPUT Keyboard;
			[FieldOffset(0)]
			public MOUSEINPUT Mouse;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct HARDWAREINPUT {
			public uint Msg;
			public ushort ParamL;
			public ushort ParamH;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct KEYBDINPUT {
			public ushort Vk;
			public ushort Scan;
			public uint Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MOUSEINPUT {
			public int X;
			public int Y;
			public uint MouseData;
			public uint Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		internal const uint
			INPUT_MOUSE = 0,
			INPUT_KEYBOARD = 1,
			INPUT_HARDWARE = 2;

		internal const uint
			MOUSEEVENTF_MOVE = 1, // just apply X/Y (delta due to not setting absolute flag)
			MOUSEEVENTF_LEFTDOWN = 2,
			MOUSEEVENTF_LEFTUP = 4,
			MOUSEEVENTF_RIGHTDOWN = 8,
			MOUSEEVENTF_RIGHTUP = 16,
			MOUSEEVENTF_MIDDLEDOWN = 32,
			MOUSEEVENTF_MIDDLEUP = 64,
			MOUSEEVENTF_XBUTTONDOWN = 128,
			MOUSEEVENTF_XBUTTONUP = 256,
			MOUSEEVENTF_WHEEL = 0x0800,
			MOUSEEVENTF_HWHEEL = 0x1000,
			MOUSEEVENTF_MIDDLEWDOWN = 0x0020,
			MOUSEEVENTF_MIDDLEWUP = 0x0040,
			KEYEVENTF_EXTENDEDKEY = 1,
			KEYEVENTF_KEYUP = 2,
			KEYEVENTF_UNICODE = 0x0004,
			KEYEVENTF_SCANCODE = 0x0008,
			MAPVK_VK_TO_VSC = 0,
			EXTENDED_FLAG = 0x100;

		// will allow for keyboard + mouse/tablet input within one SendInput call, or two mouse events
		INPUT[] input = new INPUT[4];

		//public Keyboard()
		//{
		//CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US");
		//pointer = LoadKeyboardLayout(cultureInfo.KeyboardLayoutId.ToString("X8"), 1);
		//	IntPtr pointer = GetKeyboardLayout(0);
		//	Console.WriteLine("0x{0:x}", pointer.ToString("x"));
		//}

		void SendInput(INPUT[] input) {
			SendInput((uint)input.Length, input, Marshal.SizeOf<INPUT>());
		}

		public void PressKey(string key) {
			var input = BuildKeyboardEvent(key);
			SendInput(input);
		}

		public void ReleaseKey(string key) {
			var input = BuildKeyboardEvent(key);
			input[0].Data.Keyboard.Flags |= KEYEVENTF_KEYUP;
			SendInput(input);
		}

		INPUT[] BuildKeyboardEvent(string key) {
			IntPtr hWnd = GetForegroundWindow();
			uint lpdwProcessId = GetWindowThreadProcessId(hWnd, IntPtr.Zero);
			IntPtr pointer = GetKeyboardLayout(lpdwProcessId);

			ushort virtualKey;
			if (Enum.IsDefined(typeof(VirtualKey), key))
				virtualKey = (ushort)(VirtualKey)Enum.Parse(typeof(VirtualKey), key);
			else
				virtualKey = (ushort)(VkKeyScanEx(key[0], pointer) & 0xff);
			ushort scancode = MapVirtualKey(virtualKey, MAPVK_VK_TO_VSC);
			//ushort scancode = scancodeFromVK(key);

			//Console.WriteLine("Keyboard layout: 0x{0:x}", pointer.ToString("x"));
			//Console.WriteLine("Virtual key: 0x{0:x}", virtualKey);
			//Console.WriteLine("Scan code: 0x{0:x}", scancode);

			bool extended = (scancode & 0x100) != 0;
			uint curflags = extended ? KEYEVENTF_EXTENDEDKEY : 0;

			input[0].Type = INPUT_KEYBOARD;
			input[0].Data.Keyboard.ExtraInfo = IntPtr.Zero;
			//input[0].Data.Keyboard.Flags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;
			input[0].Data.Keyboard.Flags = curflags;
			input[0].Data.Keyboard.Scan = scancode;
			input[0].Data.Keyboard.Time = 0;
			input[0].Data.Keyboard.Vk = virtualKey;

			return input;
		}
	}
}
