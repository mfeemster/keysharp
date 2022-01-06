using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Keysharp.Core.Common;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//MATT
using System.Linq;
using System.Diagnostics;
using System.Globalization;

namespace Keysharp.Core
{
	public static class Monitor
	{
		public static MonitorOutput MonitorGet(params object[] obj)
		{
			var n = obj.L().I1(-1);
			System.Windows.Forms.Screen screen;

			if (n > 0 && n <= System.Windows.Forms.Screen.AllScreens.Length)
				screen = System.Windows.Forms.Screen.AllScreens[n - 1];
			else
				screen = System.Windows.Forms.Screen.PrimaryScreen;

			return new MonitorOutput()
			{
				Left = screen.Bounds.Left,
				Top = screen.Bounds.Top,
				Right = screen.Bounds.Right,
				Bottom = screen.Bounds.Bottom,
				N = n > 0L ? n : 1L
			};
		}

		public static long MonitorGetCount(params object[] obj) => System.Windows.Forms.Screen.AllScreens.Length;

		public static string MonitorGetName(params object[] obj)
		{
			var n = obj.L().I1(-1);

			if (n > 0 && n <= System.Windows.Forms.Screen.AllScreens.Length)
				return System.Windows.Forms.Screen.AllScreens[n - 1].DeviceName;

			return System.Windows.Forms.Screen.PrimaryScreen.DeviceName;
		}

		public static long MonitorGetPrimary(params object[] obj)
		{
			long i;

			for (i = 0L; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
			{
				if (System.Windows.Forms.Screen.AllScreens[i] == System.Windows.Forms.Screen.PrimaryScreen)
					break;
			}

			return i + 1L;
		}

		public static MonitorOutput MonitorGetWorkArea(params object[] obj)
		{
			var n = obj.L().I1(-1);
			System.Windows.Forms.Screen screen;

			if (n > 0 && n <= System.Windows.Forms.Screen.AllScreens.Length)
				screen = System.Windows.Forms.Screen.AllScreens[n - 1];
			else
				screen = System.Windows.Forms.Screen.PrimaryScreen;

			return new MonitorOutput()
			{
				Left = screen.WorkingArea.Left,
				Top = screen.WorkingArea.Top,
				Right = screen.WorkingArea.Right,
				Bottom = screen.WorkingArea.Bottom,
				N = n > 0L ? n : 1L
			};
		}
	}

	public class MonitorOutput
	{
		public long Left { get; set; }
		public long Top { get; set; }
		public long Right { get; set; }
		public long Bottom { get; set; }
		public long N { get; set; }
	}
}
