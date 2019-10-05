
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GigaTrax
{
	public class Debug {
		[DllImport("DebugView", CharSet = CharSet.Ansi)]
		private static extern void DebugView_Log(string msg);

		[DllImport("DebugView", CharSet = CharSet.Unicode)]
		private static extern void DebugView_LogW(string msg);

		public static void Log(string s)
		{
			DebugView_LogW(s);
		}
	}
}
