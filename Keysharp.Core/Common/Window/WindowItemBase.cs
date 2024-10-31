namespace Keysharp.Core.Common.Window
{
	internal class PointAndHwnd
	{
		internal double distanceFound = 0.0;
		internal IntPtr hwndFound = IntPtr.Zero;
		internal bool ignoreDisabled = false;
		internal Point pt;
		internal Rectangle rectFound = new ();

		internal PointAndHwnd(Point p) => pt = p;
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
		internal virtual string ClassNN
		{
			get
			{
				var className = ClassName;
				var classNN = className;
				// to get the classNN we must know the enumeration
				// of our parent window:
				var parent = ParentWindow;

				if (parent.IsSpecified)
				{
					var nn = 1; // Class NN counter

					// now we must know the postion of our "control"
					foreach (var c in parent.ChildWindows)
					{
						if (c.IsSpecified)
						{
							if (c.ClassName == className)
							{
								if (c.Equals(this))
									break;
								else
									++nn;  // if its the same class but not our control
							}
						}
					}

					classNN += nn.ToString(); // if its the same class and our control
				}

				return classNN;
			}
		}

		internal abstract Rectangle ClientLocation { get; }
		internal int Delay { get; set; } = 100;
		internal abstract bool Enabled { get; set; }
		internal abstract bool Exists { get; }
		internal abstract long ExStyle { get; set; }
		internal IntPtr Handle { get; set; } = IntPtr.Zero;
		internal abstract bool IsHung { get; }
		internal bool IsSpecified => Handle != IntPtr.Zero;
		internal abstract Rectangle Location { get; set; }
		internal virtual string NetClassName
		{
			get
			{
				if (Control.FromHandle(Handle) is Control c)
					return c.GetType().Name;

				return "";
			}
		}
		internal virtual string NetClassNN
		{
			get
			{
				if (Control.FromHandle(Handle) is Control ctrl)
				{
					var className = ctrl.GetType().Name;
					var classNN = className;
					var parent = ctrl.Parent;

					if (parent != null)
					{
						var nn = 1; // Class NN counter

						// now we must know the postion of our "control"
						foreach (var c in parent.GetAllControlsRecursive<Control>())
						{
							if (c.GetType().Name == className)
							{
								if (c == ctrl)
									break;
								else
									++nn;  // if its the same class but not our control
							}
						}

						classNN += nn.ToString(); // if its the same class and our control
					}

					return classNN;
				}

				return "";
			}
		}
		internal abstract WindowItemBase NonChildParentWindow { get; }
		internal abstract WindowItemBase ParentWindow { get; }
		internal virtual bool IsIconic => WindowState == FormWindowState.Minimized;

		internal virtual string Path
		{
			get
			{
				try
				{
					var proc = Process.GetProcessById((int)PID);
					return proc.MainModule.FileName;
				}
				catch
				{
					return "";
				}
			}
		}

		internal abstract long PID { get; }

		internal virtual string ProcessName
		{
			get
			{
				var filename = "";

				try
				{
					var proc = Process.GetProcessById((int)PID);
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
		internal abstract List<string> Text { get; }
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

		internal static void DoControlDelay()
		{ }// => DoDelay(ThreadAccessors.A_ControlDelay);//These cause out of order execution bugs with threads and are not needed anyway.

		//public override string ToString() => IsSpecified ? Title : "not specified window";
		internal static void DoWinDelay()
		{ }// => DoDelay(ThreadAccessors.A_WinDelay);

		internal abstract void ChildFindPoint(PointAndHwnd pah);

		/// <summary>
		/// Left-Clicks on this window/control
		/// </summary>
		/// <param name="location"></param>
		internal abstract void Click(Point? location = null);

		/// <summary>
		/// Right-Clicks on this window/control
		/// </summary>
		/// <param name="location"></param>
		internal abstract void ClickRight(Point? location = null);

		internal abstract Point ClientToScreen();

		internal virtual void ClientToScreen(ref Point pt) => pt = ClientToScreen();

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
				if (WindowProvider.Manager.Groups.TryGetValue(criteria.Group, out var stack))
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

			if (!string.IsNullOrEmpty(criteria.Title))//Put title first because it's the most likely.
			{
				if (criteria.Title == "A")
					return Active;

				if (!TitleCompare(Title, criteria.Title))
					return false;

				if (!ThreadAccessors.A_DetectHiddenWindows && !Visible)
					return false;
			}

			if (criteria.ID != IntPtr.Zero && Handle != criteria.ID)
				return false;

			if (criteria.PID != 0L && PID != criteria.PID)
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

		internal abstract bool Hide();

		internal abstract bool Kill();

		//internal abstract WindowItemBase RealChildWindowFromPoint(Point location);

		internal abstract bool Redraw();

		//internal abstract void SetTransparency(byte level, Color color);

		internal abstract bool Show();

		internal bool Wait(double seconds) => Wait(seconds, () => Exists);

		internal bool WaitActive(double seconds) => Wait(seconds, () => Active);

		internal bool WaitClose(double seconds) => Wait(seconds, () => !Exists);

		internal bool WaitNotActive(double seconds) => Wait(seconds, () => !Active);

		private static void DoDelay(long delay)
		{
			if (delay >= 0)
				Flow.Sleep(delay);
		}

		private static bool TitleCompare(string a, string b, StringComparison comp = StringComparison.CurrentCulture)
		{
			if (string.IsNullOrEmpty(a))
				return false;

			switch (Threads.GetThreadVariables().titleMatchMode.ToString().ToLowerInvariant())
			{
				case "1":
					return a.StartsWith(b, comp);

				case "2":
					return a.Contains(b, comp);

				case "3":
					return a.Equals(b, comp);

				case Keyword_RegEx:
				{
					object outvar = null;
					_ = Core.Strings.RegExMatch(a, b, ref outvar, 1);
					RegExResults output = (RegExResults)outvar;
					return output.Count > 0 && !string.IsNullOrEmpty(output[0]);
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

				Flow.Sleep(Delay);
			}

			return true;
		}
	}
}