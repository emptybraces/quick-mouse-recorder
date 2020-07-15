using System;
using System.Runtime.InteropServices;

namespace quick_mouse_recorder
{
	public class InterceptInput
	{
		Key _key;
		Mouse _mouse;
		public static bool EnableMouseInput { get; set; } = false;
		public static bool EnableKeyInput { get; set; } = true;

		public InterceptInput()
		{
			_key = new Key();
			_mouse = new Mouse();
			//_hookIDMouse = NativeForKeyboard.SetWindowsHookEx(WH_MOUSE_LL, _procMouse, IntPtr.Zero, 0);
			//UnhookWindowsHookEx(_hookID);
		}
		public void AddEvent(Action<uint, Key.HookData> e) => _key.AddEvent(e);
		public void RemoveEvent(Action<uint, Key.HookData> e) => _key.RemoveEvent(e);
		public void AddEvent(Action<uint, Mouse.HookData> e) => _mouse.AddEvent(e);
		public void RemoveEvent(Action<uint, Mouse.HookData> e) => _mouse.RemoveEvent(e);
		public void Stop()
		{
			_key.Stop();
			_mouse.Stop();
		}

		public class Key
		{
			public const int WH_KEYBOARD_LL = 13;
			public const int WM_KEYDOWN     = 0x0100;
			public const int WM_KEYUP       = 0x101;
			public const int WM_SYSKEYDOWN  = 0x104;
			public const int WM_SYSKEYUP    = 0x105;
			static Action<uint, HookData> _cbHook;
			static IntPtr _handle = IntPtr.Zero;
			static Native.LowLevelProc _proc = CbHook;
			public Key()
			{
				_handle = Native.SetWindowsHookEx(WH_KEYBOARD_LL, _proc, IntPtr.Zero, 0);
			}
			static IntPtr CbHook(int nCode, uint wParam, ref HookData lParam)
			{
				if (EnableKeyInput && 0 <= nCode) {
					//int vkCode = Marshal.ReadInt32(lParam);
					_cbHook?.Invoke(wParam, lParam);
				}
				return Native.CallNextHookEx(_handle, nCode, wParam, ref lParam);
			}
			public void AddEvent(Action<uint, HookData> cb) => _cbHook += cb;
			public void RemoveEvent(Action<uint, HookData> cb) => _cbHook -= cb;
			public void Stop()
			{
				Native.UnhookWindowsHookEx(_handle);
				_handle = IntPtr.Zero;
				_cbHook = null;
			}

			static class Native
			{
				public delegate IntPtr LowLevelProc(int nCode, uint wParam, ref HookData lParam);
				[DllImport("user32.dll")]
				public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

				[DllImport("user32.dll")]
				[return: MarshalAs(UnmanagedType.Bool)]
				public static extern bool UnhookWindowsHookEx(IntPtr hhk);

				[DllImport("user32.dll")]
				public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, uint wParam, ref HookData lParam);

				[DllImport("kernel32.dll")]
				public static extern IntPtr GetModuleHandle(string lpModuleName);
			}
			[Serializable]
			public struct HookData
			{
				public uint vkCode;
				public uint scanCode;
				public uint flags;
				public uint time;
				public IntPtr dwExtraInfo;
			}
		}

		public class Mouse
		{
			public const int WH_MOUSE_LL		= 14;
			public const int WM_MOUSEMOVE		= 0x0200;
			public const int WM_LBUTTONDOWN		= 0x0201;
			public const int WM_LBUTTONUP		= 0x0202;
			public const int WM_RBUTTONDOWN		= 0x0204;
			public const int WM_RBUTTONUP		= 0x0205;
			public const int WM_MBUTTONDOWN		= 0x0207;
			public const int WM_MBUTTONUP		= 0x0208;
			public const int WM_MOUSEWHEEL		= 0x020A;
			static Action<uint, HookData> _cbHook;
			static IntPtr _handle = IntPtr.Zero;
			static Native.LowLevelProc _proc = CbHook;
			public Mouse()
			{
				_handle = Native.SetWindowsHookEx(WH_MOUSE_LL, _proc, IntPtr.Zero, 0);
			}
			static IntPtr CbHook(int nCode, uint wParam, ref HookData lParam)
			{
				if (EnableMouseInput && 0 <= nCode) {
					_cbHook?.Invoke(wParam, lParam);
				}
				return Native.CallNextHookEx(_handle, nCode, wParam, ref lParam);
			}
			public void AddEvent(Action<uint, HookData> cb) => _cbHook += cb;
			public void RemoveEvent(Action<uint, HookData> cb) => _cbHook -= cb;
			public void Stop()
			{
				Native.UnhookWindowsHookEx(_handle);
				_handle = IntPtr.Zero;
				_cbHook = null;
			}

			static class Native
			{
				public delegate IntPtr LowLevelProc(int nCode, uint wParam, ref HookData lParam);
				[DllImport("user32.dll")]
				public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

				[DllImport("user32.dll")]
				[return: MarshalAs(UnmanagedType.Bool)]
				public static extern bool UnhookWindowsHookEx(IntPtr hhk);

				[DllImport("user32.dll")]
				public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, uint wParam, ref HookData lParam);

				[DllImport("kernel32.dll")]
				public static extern IntPtr GetModuleHandle(string lpModuleName);

			}
			[Serializable]
			public struct HookData
			{
				public Point pt;
				public uint mouseData;
				public uint flags;
				public uint time;
				public IntPtr dwExtraInfo;
			}
			[Serializable]
			public struct Point
			{
				public int x;
				public int y;
			}
		}
	}
}
