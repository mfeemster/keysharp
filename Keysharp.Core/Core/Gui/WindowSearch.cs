using Keysharp.Core.Windows;
using static Keysharp.Core.WindowHelper;

namespace Keysharp.Core
{
	internal static class WindowSearch
	{
		internal static WindowItemBase SearchControl(object ctrl, object title, object text, object excludeTitle, object excludeText, bool throwifnull = true)
		{
			var (parsed, ptr) = CtrlTonint(ctrl);
			var script = Script.TheScript;
			var mgr = script.WindowProvider.Manager;
			
			if (parsed)
			{
				if (mgr.IsWindow(ptr))
					return mgr.CreateWindow(ptr);
				else if (throwifnull && !script.IsMainWindowClosing)
					 _ = Errors.TargetErrorOccurred($"Could not find child control with handle: {ptr}");

				return null;
			}

			var parent = SearchWindow(title, text, excludeTitle, excludeText, true);

			if (ctrl == null)
				return parent;

			var sc = new SearchCriteria();
			string classortext = null;
			string s = ctrl as string;

			if (!string.IsNullOrEmpty(s))
			{
				var isClass = s.Length > 0 && char.IsDigit(s[ ^ 1]);

				if (isClass)
					sc.ClassName = s;
				else
					sc.Text = s;

				classortext = s;
			}

			var childitem = parent.FirstChild(sc);

			if (classortext != null && childitem == null)
			{
				if (string.IsNullOrEmpty(sc.Text))
				{
					sc.Text = sc.ClassName;
					sc.ClassName = "";
				}
				else
				{
					sc.ClassName = sc.Text;
					sc.Text = "";
				}

				childitem = parent.FirstChild(sc);

				if (childitem == null)//Final attempt, just use title.
				{
					if (string.IsNullOrEmpty(sc.Text))
					{
						sc.Title = sc.ClassName;
						sc.ClassName = "";
					}
					else
					{
						sc.Title = sc.Text;
						sc.Text = "";
					}

					childitem = parent.FirstChild(sc);
				}
			}

			if (childitem == null && throwifnull && !script.IsMainWindowClosing)
			{
				_ = Errors.TargetErrorOccurred("Could not find child control using text or class name match \"" + s + $"\"", title, text, excludeTitle, excludeText);//Can't use interpolated string here because the AStyle formatter misinterprets it.
				return default;
			}

			return childitem;
		}

		internal static WindowItemBase SearchWindow(object winTitle,
				object winText,
				object excludeTitle,
				object excludeText,
				bool throwifnull,
				bool last = false,
				bool ignorePureID = false)
		{
			var script = Script.TheScript;
			var win = script.WindowProvider.Manager.FindWindow(winTitle, winText, excludeTitle, excludeText, last, ignorePureID);

			if (win == null && throwifnull && !script.IsMainWindowClosing)
			{
				_ = Errors.TargetErrorOccurred(winTitle, winText, excludeTitle, excludeText);
				return default;
			}

			return win;
		}

		internal static List<WindowItemBase> SearchWindows(object winTitle = null,
				object winText = null,
				object excludeTitle = null,
				object excludeText = null)
		{
			var (windows, crit) = Script.TheScript.WindowProvider.Manager.FindWindowGroup(winTitle, winText, excludeTitle, excludeText);
			return windows;
		}

		internal static object WinGetControlsHelper(bool nn,
				object winTitle,
				object winText,
				object excludeTitle,
				object excludeText)
		{
			var script = Script.TheScript;
			var win = script.WindowProvider.Manager.FindWindow(winTitle, winText, excludeTitle, excludeText);

			if (win != null)
			{
				var controls = win.ChildWindows;

				if (controls.Count == 0)
					return DefaultObject;

				var arr = new Array()
				{
					Capacity = controls.Count
				};

				if (nn)
				{
					foreach (var ctrl in controls)
						_ = arr.Add(ctrl.GetClassNN(controls));
				}
				else
				{
					foreach (var ctrl in controls)
						_ = arr.Add(ctrl.Handle.ToInt64());
				}

				WindowItemBase.DoWinDelay();
				return arr;
			}
			else if (!script.IsMainWindowClosing)
				return Errors.TargetErrorOccurred(winTitle, winText, excludeTitle, excludeText);

			return DefaultObject;
		}
	}
}