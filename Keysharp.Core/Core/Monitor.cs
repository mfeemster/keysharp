using System.Collections.Generic;

namespace Keysharp.Core
{
	public static class Monitor
	{
		public static Keysharp.Core.Map MonitorGet(object obj = null)
		{
			var n = obj.Al(-1L);
			System.Windows.Forms.Screen screen;

			if (n > 0 && n <= System.Windows.Forms.Screen.AllScreens.Length)
				screen = System.Windows.Forms.Screen.AllScreens[n - 1];
			else
				screen = System.Windows.Forms.Screen.PrimaryScreen;

			return new Keysharp.Core.Map(new Dictionary<object, object>()
			{
				{ "Left", screen.Bounds.Left },
				{ "Top", screen.Bounds.Top },
				{ "Right", screen.Bounds.Right },
				{ "Bottom", screen.Bounds.Bottom },
				{ "N", n > 0L ? n : 1L }
			});
		}

		public static long MonitorGetCount() => System.Windows.Forms.Screen.AllScreens.Length;

		public static string MonitorGetName(object obj = null)
		{
			var n = obj.Al(-1L);

			if (n > 0 && n <= System.Windows.Forms.Screen.AllScreens.Length)
				return System.Windows.Forms.Screen.AllScreens[n - 1].DeviceName;

			return System.Windows.Forms.Screen.PrimaryScreen.DeviceName;
		}

		public static long MonitorGetPrimary()
		{
			long i;

			for (i = 0L; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
			{
				if (System.Windows.Forms.Screen.AllScreens[i] == System.Windows.Forms.Screen.PrimaryScreen)
					break;
			}

			return i + 1L;
		}

		public static Keysharp.Core.Map MonitorGetWorkArea(object obj = null)
		{
			var n = obj.Al(-1L);
			System.Windows.Forms.Screen screen;

			if (n > 0 && n <= System.Windows.Forms.Screen.AllScreens.Length)
				screen = System.Windows.Forms.Screen.AllScreens[n - 1];
			else
				screen = System.Windows.Forms.Screen.PrimaryScreen;

			return new Keysharp.Core.Map(new Dictionary<object, object>()
			{
				{ "Left", screen.WorkingArea.Left },
				{ "Top", screen.WorkingArea.Top },
				{ "Right", screen.WorkingArea.Right },
				{ "Bottom", screen.WorkingArea.Bottom },
				{ "N", n > 0L ? n : 1L }
			});
		}
	}
}