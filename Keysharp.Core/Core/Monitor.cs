namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for monitor-related functions.
	/// </summary>
	public static class Monitor
	{
		/// <summary>
		/// <see cref="MonitorGet(object, ref object, ref object, ref object, ref object)"/>
		/// </summary>
		public static object MonitorGet()
		{
			object l = null, t = null, r = null, b = null;
			return MonitorGet(null, ref l, ref t, ref r, ref b);
		}

		/// <summary>
		/// <see cref="MonitorGet(object, ref object, ref object, ref object, ref object)"/>
		/// </summary>
		public static object MonitorGet(object obj)
		{
			object l = null, t = null, r = null, b = null;
			return MonitorGet(obj, ref l, ref t, ref r, ref b);
		}

		/// <summary>
		/// <see cref="MonitorGet(object, ref object, ref object, ref object, ref object)"/>
		/// </summary>
		public static object MonitorGet(object obj, ref object l)
		{
			object t = null, r = null, b = null;
			return MonitorGet(obj, ref l, ref t, ref r, ref b);
		}

		/// <summary>
		/// <see cref="MonitorGet(object, ref object, ref object, ref object, ref object)"/>
		/// </summary>
		public static object MonitorGet(object obj, ref object l, ref object t)
		{
			object r = null, b = null;
			return MonitorGet(obj, ref l, ref t, ref r, ref b);
		}

		/// <summary>
		/// <see cref="MonitorGet(object, ref object, ref object, ref object, ref object)"/>
		/// </summary>
		public static object MonitorGet(object obj, ref object l, ref object t, ref object r)
		{
			object b = null;
			return MonitorGet(obj, ref l, ref t, ref r, ref b);
		}

		/// <summary>
		/// Checks if the specified monitor exists and optionally retrieves its bounding coordinates.
		/// </summary>
		/// <param name="n">If omitted, the primary monitor will be used. Otherwise, specify the monitor number, between 1 and the number returned by <see cref="MonitorGetCount"/>.</param>
		/// <param name="left">The left bounding coordinate of the specified monitor.</param>
		/// <param name="top">The top bounding coordinate of the specified monitor.</param>
		/// <param name="right">The right bounding coordinate of the specified monitor.</param>
		/// <param name="bottom">The bottom bounding coordinate of the specified monitor.</param>
		/// <returns>The monitor number which is the same as n unless n was omitted.</returns>
		public static object MonitorGet(object n, ref object left, ref object top, ref object right, ref object bottom)
		{
			var monitorIndex = n.Al(-1L);
			System.Windows.Forms.Screen screen;

			if (monitorIndex > 0 && monitorIndex <= System.Windows.Forms.Screen.AllScreens.Length)
				screen = System.Windows.Forms.Screen.AllScreens[monitorIndex - 1];
			else
				screen = System.Windows.Forms.Screen.PrimaryScreen;

			left  = (long)screen.Bounds.Left;
			top  = (long)screen.Bounds.Top;
			right  = (long)screen.Bounds.Right;
			bottom  = (long)screen.Bounds.Bottom;
			return monitorIndex > 0L ? monitorIndex : 1L;
		}

		/// <summary>
		/// Returns the total number of monitors.
		/// </summary>
		/// <returns>The total number of monitors.</returns>
		public static long MonitorGetCount() => System.Windows.Forms.Screen.AllScreens.Length;

		/// <summary>
		/// Returns the operating system's name of the specified monitor.
		/// </summary>
		/// <param name="n">If omitted, the primary monitor will be used. Otherwise, specify the monitor number, between 1 and the number returned by <see cref="MonitorGetCount"/>.</param>
		/// <returns>A string</returns>
		public static string MonitorGetName(object n = null)
		{
			var monitorIndex = n.Al(-1L);

			if (monitorIndex > 0 && monitorIndex <= System.Windows.Forms.Screen.AllScreens.Length)
				return System.Windows.Forms.Screen.AllScreens[monitorIndex - 1].DeviceName;

			return System.Windows.Forms.Screen.PrimaryScreen.DeviceName;
		}

		/// <summary>
		/// Returns the number of the primary monitor.
		/// </summary>
		/// <returns>The number of the primary monitor. In a single-monitor system, this will be always 1.</returns>
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

		/// <summary>
		/// <see cref="MonitorGetWorkArea(object, ref object, ref object, ref object, ref object)"/>
		/// </summary>
		public static object MonitorGetWorkArea()
		{
			object l = null, t = null, r = null, b = null;
			return MonitorGetWorkArea(null, ref l, ref t, ref r, ref b);
		}

		/// <summary>
		/// <see cref="MonitorGetWorkArea(object, ref object, ref object, ref object, ref object)"/>
		/// </summary>
		public static object MonitorGetWorkArea(object n)
		{
			object l = null, t = null, r = null, b = null;
			return MonitorGetWorkArea(n, ref l, ref t, ref r, ref b);
		}

		/// <summary>
		/// <see cref="MonitorGetWorkArea(object, ref object, ref object, ref object, ref object)"/>
		/// </summary>
		public static object MonitorGetWorkArea(object obj, ref object left)
		{
			object t = null, r = null, b = null;
			return MonitorGetWorkArea(obj, ref left, ref t, ref r, ref b);
		}

		/// <summary>
		/// <see cref="MonitorGetWorkArea(object, ref object, ref object, ref object, ref object)"/>
		/// </summary>
		public static object MonitorGetWorkArea(object obj, ref object left, ref object top)
		{
			object r = null, b = null;
			return MonitorGetWorkArea(obj, ref left, ref top, ref r, ref b);
		}

		/// <summary>
		/// <see cref="MonitorGetWorkArea(object, ref object, ref object, ref object, ref object)"/>
		/// </summary>
		public static object MonitorGetWorkArea(object obj, ref object left, ref object top, ref object right)
		{
			object b = null;
			return MonitorGetWorkArea(obj, ref left, ref top, ref right, ref b);
		}

		/// <summary>
		/// Checks if the specified monitor exists and optionally retrieves the bounding coordinates of its working area.
		/// </summary>
		/// <param name="n">If omitted, the primary monitor will be used. Otherwise, specify the monitor number, between 1 and the number returned by <see cref="MonitorGetCount"/>.</param>
		/// <param name="left">The left bounding coordinate of the work area of the specified monitor.</param>
		/// <param name="top">The top bounding coordinate of the work area of the specified monitor.</param>
		/// <param name="right">The right bounding coordinate of the work area of the specified monitor.</param>
		/// <param name="bottom">The bottom bounding coordinate of the work area of the specified monitor.</param>
		/// <returns>The monitor number which is the same as n unless n was omitted.</returns>
		public static object MonitorGetWorkArea(object n, ref object left, ref object top, ref object right, ref object bottom)
		{
			var monitorIndex = n.Al(-1L);
			System.Windows.Forms.Screen screen;

			if (monitorIndex > 0 && monitorIndex <= System.Windows.Forms.Screen.AllScreens.Length)
				screen = System.Windows.Forms.Screen.AllScreens[monitorIndex - 1];
			else
				screen = System.Windows.Forms.Screen.PrimaryScreen;

			left = (long)screen.WorkingArea.Left;
			top = (long)screen.WorkingArea.Top;
			right = (long)screen.WorkingArea.Right;
			bottom = (long)screen.WorkingArea.Bottom;
			return monitorIndex > 0L ? monitorIndex : 1L;
		}
	}
}