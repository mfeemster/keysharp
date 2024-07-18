namespace Keysharp.Core
{
	public static class Monitor
	{
		public static object MonitorGet()
		{
			object l = null, t = null, r = null, b = null;
			return MonitorGet(null, ref l, ref t, ref r, ref b);
		}

		public static object MonitorGet(object obj)
		{
			object l = null, t = null, r = null, b = null;
			return MonitorGet(obj, ref l, ref t, ref r, ref b);
		}

		public static object MonitorGet(object obj, ref object l)
		{
			object t = null, r = null, b = null;
			return MonitorGet(obj, ref l, ref t, ref r, ref b);
		}

		public static object MonitorGet(object obj, ref object l, ref object t)
		{
			object r = null, b = null;
			return MonitorGet(obj, ref l, ref t, ref r, ref b);
		}

		public static object MonitorGet(object obj, ref object l, ref object t, ref object r)
		{
			object b = null;
			return MonitorGet(obj, ref l, ref t, ref r, ref b);
		}

		public static object MonitorGet(object obj, ref object l, ref object t, ref object r, ref object b)
		{
			var n = obj.Al(-1L);
			System.Windows.Forms.Screen screen;

			if (n > 0 && n <= System.Windows.Forms.Screen.AllScreens.Length)
				screen = System.Windows.Forms.Screen.AllScreens[n - 1];
			else
				screen = System.Windows.Forms.Screen.PrimaryScreen;

			l  = screen.Bounds.Left;
			t  = screen.Bounds.Top;
			r  = screen.Bounds.Right;
			b  = screen.Bounds.Bottom;
			return n > 0L ? n : 1L;
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

		public static object MonitorGetWorkArea()
		{
			object l = null, t = null, r = null, b = null;
			return MonitorGetWorkArea(null, ref l, ref t, ref r, ref b);
		}

		public static object MonitorGetWorkArea(object obj)
		{
			object l = null, t = null, r = null, b = null;
			return MonitorGetWorkArea(obj, ref l, ref t, ref r, ref b);
		}

		public static object MonitorGetWorkArea(object obj, ref object l)
		{
			object t = null, r = null, b = null;
			return MonitorGetWorkArea(obj, ref l, ref t, ref r, ref b);
		}

		public static object MonitorGetWorkArea(object obj, ref object l, ref object t)
		{
			object r = null, b = null;
			return MonitorGetWorkArea(obj, ref l, ref t, ref r, ref b);
		}

		public static object MonitorGetWorkArea(object obj, ref object l, ref object t, ref object r)
		{
			object b = null;
			return MonitorGetWorkArea(obj, ref l, ref t, ref r, ref b);
		}

		public static object MonitorGetWorkArea(object obj, ref object l, ref object t, ref object r, ref object b)
		{
			var n = obj.Al(-1L);
			System.Windows.Forms.Screen screen;

			if (n > 0 && n <= System.Windows.Forms.Screen.AllScreens.Length)
				screen = System.Windows.Forms.Screen.AllScreens[n - 1];
			else
				screen = System.Windows.Forms.Screen.PrimaryScreen;

			l = screen.WorkingArea.Left;
			t = screen.WorkingArea.Top;
			r = screen.WorkingArea.Right;
			b = screen.WorkingArea.Bottom;
			return n > 0L ? n : 1L;
		}
	}
}