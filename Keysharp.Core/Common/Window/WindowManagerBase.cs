using System;
using System.Collections.Generic;
using System.Drawing;

namespace Keysharp.Core.Common.Window
{
	internal class WindowGroup
	{
		internal Stack<long> activated = new Stack<long>();
		internal Stack<long> deactivated = new Stack<long>();
		internal bool lastWasDeactivate = false;
		internal List<SearchCriteria> sc = new List<SearchCriteria>();
	}

	/// <summary>
	/// Platform independent window manager.
	/// </summary>
	internal abstract class WindowManagerBase
	{
		internal abstract WindowItemBase ActiveWindow { get; }

		internal abstract IEnumerable<WindowItemBase> AllWindows { get; }

		internal Dictionary<string, WindowGroup> Groups { get; } = new Dictionary<string, WindowGroup>();

		internal abstract WindowItemBase LastFound { get; set; }

		internal abstract WindowItemBase CreateWindow(IntPtr id);

		internal abstract IEnumerable<WindowItemBase> FilterForGroups(IEnumerable<WindowItemBase> windows);

		internal abstract WindowItemBase FindWindow(SearchCriteria criteria, bool last = false);

		internal WindowItemBase FindWindow(object title, string text, string excludeTitle, string excludeText, bool last = false)
		{
			WindowItemBase foundWindow = null;

			if (title is Gui gui)
			{
				return (LastFound = CreateWindow(gui.Hwnd));
			}
			else if ((title == null || (title is string s && string.IsNullOrEmpty(s))) && string.IsNullOrEmpty(text) && string.IsNullOrEmpty(excludeTitle) && string.IsNullOrEmpty(excludeText))
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

		internal abstract List<WindowItemBase> FindWindowGroup(SearchCriteria criteria, bool forceAll = false);

		internal (List<WindowItemBase>, SearchCriteria) FindWindowGroup(object title, string text, string excludeTitle, string excludeText, bool forceAll = false)
		{
			SearchCriteria criteria = null;
			var foundWindows = new List<WindowItemBase>();

			if ((title == null || (title is string s && string.IsNullOrEmpty(s))) && string.IsNullOrEmpty(text) && string.IsNullOrEmpty(excludeTitle) && string.IsNullOrEmpty(excludeText))
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

		internal abstract uint GetFocusedCtrlThread(ref IntPtr apControl, IntPtr aWindow);

		internal abstract WindowItemBase GetForeGroundWindow();

		internal abstract void MinimizeAll();

		internal abstract void MinimizeAllUndo();

		internal abstract WindowItemBase WindowFromPoint(Point location);
	}
}