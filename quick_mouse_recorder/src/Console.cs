using System;
using System.Diagnostics;
using System.Linq;

namespace quick_mouse_recorder
{
	class cn
	{
		[System.Diagnostics.Conditional("DEBUG")]
		public static void log()
		{
			var st2 = new System.Diagnostics.StackTrace(new System.Diagnostics.StackFrame(1, true));
			log(st2.GetFrame(0).GetFileName(), st2.GetFrame(0).GetFileLineNumber());
		}
		public static void log(params object[] msgs)
		{
			if (msgs != null && 0 < msgs.Length) {
				var s = String.Join(" ", msgs.Select(e => e == null ? "null" : e.ToString()).ToArray());
				Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "    " + s);
			}
			else
				Debug.WriteLine("");
		}
	}
}
