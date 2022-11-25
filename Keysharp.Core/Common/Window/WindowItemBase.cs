using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Core.Windows;//Code in Common probably shouldn't be referencing windows specific code.//MATT

namespace Keysharp.Core.Common.Window
{
	internal class PointAndHwnd
	{
		internal double distanceFound;
		internal IntPtr hwndFound = IntPtr.Zero;
		internal bool ignoreDisabled;
		internal POINT pt;
		internal RECT rectFound = new RECT();

		internal PointAndHwnd(POINT p) => pt = p;
	}

	/// <summary>
	/// Abstraction of a single Platform independend Window
	/// </summary>
	internal abstract class WindowItemBase
	{
		internal abstract bool Active { get; set; }
		internal abstract bool AlwaysOnTop { get; set; }
		internal abstract bool Bottom { set; }
		internal abstract List<WindowItemBase> ChildWindows { get; }
		internal abstract string ClassName { get; }

		/// <summary>
		/// Get the ClassName + number of occurence of this window (control)
		/// </summary>
		internal abstract string ClassNN { get; }
		internal abstract Rectangle ClientLocation { get; }
		internal int Delay { get; set; } = 100;
		internal abstract bool Enabled { get; set; }
		internal abstract bool Exists { get; }
		internal abstract long ExStyle { get; set; }
		internal IntPtr Handle { get; set; } = IntPtr.Zero;
		internal abstract bool IsHung { get; }
		internal bool IsSpecified => Handle != IntPtr.Zero;
		internal abstract Rectangle Location { get; set; }
		internal abstract WindowItemBase ParentWindow { get; }

		internal virtual string Path
		{
			get
			{
				var filename = "";

				try
				{
					var proc = System.Diagnostics.Process.GetProcessById((int)PID.ToInt64());
					filename = proc.MainModule.FileName;
				}
				catch
				{
				}

				return filename;
			}
		}

		internal abstract IntPtr PID { get; }
		internal abstract WindowItemBase PreviousWindow { get; }

		internal virtual string ProcessName
		{
			get
			{
				var filename = "";

				try
				{
					var proc = System.Diagnostics.Process.GetProcessById((int)PID.ToInt64());
					filename = proc.MainModule.ModuleName;
				}
				catch
				{
				}

				return filename;
			}
		}

		internal abstract Size Size { get; set; }
		internal abstract long Style { get; set; }
		internal abstract string[] Text { get; }
		internal abstract string Title { get; set; }
		internal abstract object Transparency { get; set; }
		internal abstract object TransparentColor { get; set; }
		internal abstract bool Visible { get; set; }
		internal abstract FormWindowState WindowState { get; set; }

		internal WindowItemBase(IntPtr handle) => Handle = handle;

		/// <summary>
		/// Define Standard Equalty Opertaor
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj) => obj is WindowItemBase ? (obj as WindowItemBase).Handle == Handle : base.Equals(obj);

		public override int GetHashCode() => base.GetHashCode();

		public override string ToString() => $"{Handle.ToInt64()}";

		internal static void DoControlDelay() => DoDelay((long)Accessors.A_ControlDelay);

		//public override string ToString() => IsSpecified ? Title : "not specified window";
		internal static void DoWinDelay() => DoDelay((long)Accessors.A_WinDelay);

		internal abstract void ChildFindPoint(PointAndHwnd pah);

		/// <summary>
		/// Left-Clicks on this window/control
		/// </summary>
		/// <param name="location"></param>
		internal virtual void Click(Point? location = null)
		{
			SendMouseEvent(MOUSEEVENTF.LEFTDOWN, location);
			SendMouseEvent(MOUSEEVENTF.LEFTUP, location);
		}

		/// <summary>
		/// Right-Clicks on this window/control
		/// </summary>
		/// <param name="location"></param>
		internal virtual void ClickRight(Point? location = null)
		{
			SendMouseEvent(MOUSEEVENTF.RIGHTDOWN, location);
			SendMouseEvent(MOUSEEVENTF.RIGHTUP, location);
		}

		internal abstract bool Close();

		internal bool Equals(SearchCriteria criteria)//Make internal to avoid dupes.
		{
			if (!IsSpecified)
				return false;

			if (criteria.Active)
			{
				return Active;
			}

			if (!string.IsNullOrEmpty(criteria.Group))
			{
				if (WindowManagerProvider.Instance.Groups.TryGetValue(criteria.Group, out var stack))
				{
					if (stack.sc.Count > 0)//An empty group is assumed to want to match all windows.
					{
						var anypassed = false;

						foreach (var crit in stack.sc)
						{
							if (Equals(crit))//If any criteria in the group matched something, then it's considered a valid match.
							{
								anypassed = true;
								break;
							}
						}

						if (!anypassed)
							return false;
					}
				}
			}

			//Documentation says in the case of multiple matched:
			//If multiple windows match WinTitle and any other criteria, the topmost matching window is used.
			//Need to implement that.//MATT
			if (!string.IsNullOrEmpty(criteria.Title))//Put title first because it's the most likely.
			{
				if (!TitleCompare(Title, criteria.Title))
					return false;

				if (!(bool)Keysharp.Core.Accessors.A_DetectHiddenWindows && !WindowsAPI.IsWindowVisible(Handle))
					return false;
			}

			if (criteria.ID != IntPtr.Zero && Handle != criteria.ID)
				return false;

			if (criteria.PID != IntPtr.Zero && PID != criteria.PID)
				return false;

			if (!string.IsNullOrEmpty(criteria.Path))
			{
				var comp = Environment.OSVersion.Platform == PlatformID.Win32NT ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture;

				if (!TitleCompare(Path, criteria.Path, comp))
					return false;
			}

			if (!string.IsNullOrEmpty(criteria.ClassName))
			{
				if (!TitleCompare(ClassName, criteria.ClassName) && !TitleCompare(ClassNN, criteria.ClassName))//Check both just to be safe because they can be slightly different.
					return false;
			}

			if (!string.IsNullOrEmpty(criteria.Text))
			{
				foreach (var text in Text)
					if (TitleCompare(text, criteria.Text))
						return true;

				return false;
			}

			if (!string.IsNullOrEmpty(criteria.ExcludeTitle))
			{
				if (TitleCompare(Title, criteria.ExcludeTitle))
					return false;
			}

			if (!string.IsNullOrEmpty(criteria.ExcludeText))
			{
				foreach (var text in Text)
					if (TitleCompare(text, criteria.ExcludeText))
						return false;
			}

			return true;
		}

		internal WindowItemBase FirstChild(SearchCriteria sc)
		{
			WindowItemBase item = null;

			foreach (var child in ChildWindows)
			{
				if (child.Equals(sc))
				{
					item = child;
					break;
				}
			}

			return item;
		}

		internal abstract uint GetMenuItemId(params string[] items);

		internal abstract bool Hide();

		internal abstract bool Kill();

		internal abstract WindowItemBase RealChildWindowFromPoint(Point location);

		internal abstract bool Redraw();

		internal abstract void SendMouseEvent(MOUSEEVENTF mouseevent, Point? location = null);

		internal abstract void SetTransparency(byte level, Color color);

		internal abstract bool Show();

		internal bool Wait(double seconds) => Wait(seconds, () => Exists);

		internal bool WaitActive(double seconds) => Wait(seconds, () => Active);

		internal bool WaitClose(double seconds) => Wait(seconds, () => !Exists);

		internal bool WaitNotActive(double seconds) => Wait(seconds, () => !Active);

		private static void DoDelay(long delay)
		{
			if (delay >= 0)
				Keysharp.Core.Flow.Sleep(delay);
		}

		private static bool TitleCompare(string a, string b, StringComparison comp = StringComparison.CurrentCulture)
		{
			if (string.IsNullOrEmpty(a))
				return false;

			switch (Accessors.A_TitleMatchMode.ToString().ToLowerInvariant())
			{
				case "1":
					return a.StartsWith(b, comp);

				case "2":
					return a.IndexOf(b, comp) != -1;

				case "3":
					return a.Equals(b, comp);

				case Core.Keyword_RegEx:
				{
					var output = Strings.RegExMatch(a, b, 1);
					return output.Count > 0 && !string.IsNullOrEmpty(output[0]);
					//return new Regex(b).IsMatch(a);
				}
			}

			return false;
		}

		private bool Wait(double seconds, Func<bool> func)
		{
			var start = DateTime.Now;

			while (!func())
			{
				if (seconds != 0 && (DateTime.Now - start).TotalSeconds >= seconds)
					return false;

				Keysharp.Core.Flow.Sleep(Delay);
			}

			return true;
		}
	}
}