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
			get => CreateWindow((nint)Script.TheScript.HwndLastUsed);
			set => Script.TheScript.HwndLastUsed = value.Handle;
		}

		internal abstract WindowItemBase CreateWindow(nint id);

		internal abstract IEnumerable<WindowItemBase> FilterForGroups(IEnumerable<WindowItemBase> windows);

		internal virtual WindowItemBase FindWindow(SearchCriteria criteria, bool last = false)
		{
			WindowItemBase found = null;

			if (criteria.IsEmpty)
				return found;

			if (criteria.ID != 0)
			{
				if (IsWindow(criteria.ID) && CreateWindow(criteria.ID) is WindowItemBase temp && temp.Equals(criteria))
					return temp;
				return null;
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

		internal WindowItemBase FindWindow(object winTitle, object winText, object excludeTitle, object excludeText, bool last = false, bool ignorePureID = false)
		{
			WindowItemBase foundWindow = null;
			var (parsed, ptr) = WindowHelper.CtrlTonint(winTitle);

			if (parsed)
				if (!ignorePureID && IsWindow(ptr))
					return LastFound = CreateWindow(ptr);

			var text = winText.As();
			var exclTitle = excludeTitle.As();
			var exclTxt = excludeText.As();

			if (winTitle is Gui gui)
			{
				return LastFound = CreateWindow((nint)gui.Hwnd);
			}
			else if ((winTitle == null || winTitle is string s && string.IsNullOrEmpty(s)) &&
					 string.IsNullOrEmpty(text) &&
					 string.IsNullOrEmpty(exclTitle) &&
					 string.IsNullOrEmpty(exclTxt))
			{
				foundWindow = LastFound;
			}
			else
			{
				var criteria = SearchCriteria.FromString(winTitle, text, excludeTitle, excludeText);
				foundWindow = FindWindow(criteria, last);
			}

			if (foundWindow != null && foundWindow.IsSpecified)
				LastFound = foundWindow;

			return foundWindow;
		}

		internal virtual List<WindowItemBase> FindWindowGroup(SearchCriteria criteria, bool forceAll = false, bool ignorePureID = false)
		{
			var found = new List<WindowItemBase>();

			if (!ignorePureID && criteria.IsPureID)
			{
				if (IsWindow(criteria.ID))
				{
					var window = CreateWindow(criteria.ID);

					if (window.Equals(criteria)) // Other criteria may be present such as ExcludeTitle etc
						found.Add(window);
				}

				return found;
			}

			//KeysharpEnhancements.OutputDebugLine($"About to iterate AllWindows in FindWindowGroup()");

			foreach (var window in AllWindows)
			{
				//KeysharpEnhancements.OutputDebugLine($"FindWindowGroup(): about to examine window: {window.Title}");
				if (criteria.IsEmpty || window.Equals(criteria))
				{
					found.Add(window);

					if (!forceAll && string.IsNullOrEmpty(criteria.Group))//If it was a group match, or if the caller specified that they want all matched windows, add it and keep going.
						break;
				}
			}

			return found;
		}

		internal (List<WindowItemBase>, SearchCriteria) FindWindowGroup(object winTitle,
				object winText,
				object excludeTitle,
				object excludeText,
				bool forceAll = false,
				bool ignorePureID = false)
		{
			SearchCriteria criteria = null;
			var foundWindows = new List<WindowItemBase>();
			var text = winText.As();
			var exclTitle = excludeTitle.As();
			var exclText = excludeText.As();

			if ((winTitle == null || winTitle is string s && string.IsNullOrEmpty(s)) &&
					string.IsNullOrEmpty(text) &&
					string.IsNullOrEmpty(exclTitle) &&
					string.IsNullOrEmpty(exclText))
			{
				if (LastFound != null)
					foundWindows.Add(LastFound);
			}
			else
			{
				criteria = SearchCriteria.FromString(winTitle, text, exclTitle, exclText);
				foundWindows = FindWindowGroup(criteria, forceAll);

				if (foundWindows != null && foundWindows.Count > 0 && foundWindows[0].IsSpecified)
					LastFound = foundWindows[0];
			}

			return (foundWindows, criteria);
		}

		internal abstract uint GetFocusedCtrlThread(ref nint apControl, nint aWindow);

		internal abstract nint GetForeGroundWindowHwnd();

		internal abstract bool IsWindow(nint handle);

		internal abstract void MaximizeAll();

		internal abstract void MinimizeAll();

		internal abstract void MinimizeAllUndo();

		internal abstract WindowItemBase WindowFromPoint(Point location);
	}
}