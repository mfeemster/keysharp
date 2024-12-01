namespace Keysharp.Core.Common.Platform
{
	internal class WindowGroup
	{
		internal Stack<long> activated = new ();
		internal Stack<long> deactivated = new ();
		internal bool lastWasDeactivate = false;
		internal List<SearchCriteria> sc = [];
	}

	/// <summary>
	/// Platform independent window manager.
	/// </summary>
	internal abstract class WindowManagerBase
	{
		internal abstract WindowItemBase ActiveWindow { get; }
		internal abstract IEnumerable<WindowItemBase> AllWindows { get; }
		internal Dictionary<string, WindowGroup> Groups { get; } = new Dictionary<string, WindowGroup>(StringComparer.OrdinalIgnoreCase);

		internal virtual WindowItemBase LastFound
		{
			get => CreateWindow(Script.HwndLastUsed);
			set => Script.HwndLastUsed = value.Handle;
		}

		internal abstract WindowItemBase CreateWindow(nint id);

		internal abstract IEnumerable<WindowItemBase> FilterForGroups(IEnumerable<WindowItemBase> windows);

		internal virtual WindowItemBase FindWindow(SearchCriteria criteria, bool last = false)
		{
			WindowItemBase found = null;

			if (criteria.IsEmpty)
				return found;

			if (criteria.HasID)
			{
				var temp = CreateWindow(criteria.ID);

				if (temp.Exists)
					return temp;
			}

			foreach (var window in AllWindows)
			{
				if (window.Equals(criteria))
				{
					found = window;

					if (!last)
						break;
				}
			}

			return found;
		}

		internal WindowItemBase FindWindow(object title, string text, string excludeTitle, string excludeText, bool last = false)
		{
			WindowItemBase foundWindow = null;
			var (parsed, ptr) = Core.Window.CtrlToIntPtr(title);

			if (parsed)
				if (IsWindow(ptr))
					return LastFound = WindowProvider.Manager.CreateWindow(ptr);

			if (title is Gui gui)
			{
				return LastFound = CreateWindow(gui.Hwnd);
			}
			else if ((title == null || title is string s && string.IsNullOrEmpty(s)) && string.IsNullOrEmpty(text) && string.IsNullOrEmpty(excludeTitle) && string.IsNullOrEmpty(excludeText))
			{
				foundWindow = LastFound;
			}
			else
			{
				var criteria = SearchCriteria.FromString(title, text, excludeTitle, excludeText);
				foundWindow = FindWindow(criteria, last);
			}

			if (foundWindow != null && foundWindow.IsSpecified)
				LastFound = foundWindow;

			return foundWindow;
		}

		internal virtual List<WindowItemBase> FindWindowGroup(SearchCriteria criteria, bool forceAll = false)
		{
			var found = new List<WindowItemBase>();
			//Keysharp.Scripting.Script.OutputDebug($"About to iterate AllWindows in FindWindowGroup()");

			foreach (var window in AllWindows)
			{
				//Keysharp.Scripting.Script.OutputDebug($"FindWindowGroup(): about to examine window: {window.Title}");
				if (criteria.IsEmpty || window.Equals(criteria))
				{
					found.Add(window);

					if (!forceAll && string.IsNullOrEmpty(criteria.Group))//If it was a group match, or if the caller specified that they want all matched windows, add it and keep going.
						break;
				}
			}

			return found;
		}

		internal (List<WindowItemBase>, SearchCriteria) FindWindowGroup(object title, string text, string excludeTitle, string excludeText, bool forceAll = false)
		{
			SearchCriteria criteria = null;
			var foundWindows = new List<WindowItemBase>();

			if ((title == null || title is string s && string.IsNullOrEmpty(s)) && string.IsNullOrEmpty(text) && string.IsNullOrEmpty(excludeTitle) && string.IsNullOrEmpty(excludeText))
			{
				if (LastFound != null)
					foundWindows.Add(LastFound);
			}
			else
			{
				criteria = SearchCriteria.FromString(title, text, excludeTitle, excludeText);
				foundWindows = FindWindowGroup(criteria, forceAll);

				if (foundWindows != null && foundWindows.Count > 0 && foundWindows[0].IsSpecified)
					LastFound = foundWindows[0];
			}

			return (foundWindows, criteria);
		}

		internal abstract uint GetFocusedCtrlThread(ref nint apControl, nint aWindow);

		internal abstract IntPtr GetForeGroundWindowHwnd();

		internal abstract bool IsWindow(IntPtr handle);

		internal abstract void MaximizeAll();

		internal abstract void MinimizeAll();

		internal abstract void MinimizeAllUndo();

		internal abstract WindowItemBase WindowFromPoint(Point location);
	}
}