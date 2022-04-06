using System.Collections.Generic;
using System.Linq;
using Keysharp.Core;
using Keysharp.Core.Windows;

namespace System.Windows.Forms
{
	public static class ControlExtensions
	{
		public static void CheckedBeginInvoke(this Control control, Action action)
		{
			if (control == null || control.IsDisposed || control.Disposing || !control.IsHandleCreated)
				return;

			if (control.InvokeRequired)
				control.BeginInvoke(action);
			else
				action();
		}

		public static void CheckedInvoke(this Control control, Action action)
		{
			if (control == null || control.IsDisposed || control.Disposing || !control.IsHandleCreated)
				return;

			if (control.InvokeRequired)
				control.Invoke(action);
			else
				action();
		}

		public static TabPage FindTab(this TabControl tc, string text, bool exact)
		{
			foreach (TabPage tp in tc.TabPages)
				if (exact)
				{
					if (string.Compare(tp.Text, text, true) == 0)
						return tp;
				}
				else if (tp.Text.StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
					return tp;

			return null;
		}

		public static HashSet<T> GetAllControlsRecusrvive<T>(this Control control) where T : class, new ()
		{
			var rtn = new HashSet<T>();

			foreach (Control item in control.Controls)
			{
				if (item is T ctrl)
					rtn.Add(ctrl);
				else
					rtn.AddRange(GetAllControlsRecusrvive<T>(item));
			}

			return rtn;
		}

		/// <summary>
		/// Gotten from here and fixed: https://www.codeproject.com/tips/264690/how-to-iterate-recursive-through-all-menu-items-in
		/// </summary>
		/// <param name="menuStrip"></param>
		/// <returns></returns>
		public static List<ToolStripItem> GetItems(this ToolStrip menuStrip)
		{
			var myItems = new List<ToolStripItem>();

			foreach (var o in menuStrip.Items)
				if (o is ToolStripItem i)
					GetMenuItems(i, myItems);

			return myItems;
		}

		public static T GetNthControlRecusrvive<T>(this Control control, int index) where T : class, new ()
		{
			T ctrl = null;
			var list = control.GetAllControlsRecusrvive<T>().ToList();

			for (var i = 0; i <= index; i++)
				if (i == index)
				{
					ctrl = list[i];
					break;
				}

			return ctrl;
		}

		public static T GetParentOfType<T>(this Control control) where T : class, new ()
		{
			while (control != null && control.Parent is Control cp)
			{
				if (cp is T t)
					return t;

				control = cp;
			}

			return null;
		}

		public static void Invoke(this Control control, Action action)
		{
			if (control == null || control.IsDisposed || control.Disposing || !control.IsHandleCreated)
				return;

			if (control.InvokeRequired)
				control.Invoke(action);
			else
				action();
		}

		public static void ResumeDrawing(this Control control)
		{
			_ = WindowsAPI.SendMessage(control.Handle, WindowsAPI.WM_SETREDRAW, 1, 0);
			control.Refresh();
		}

		public static void SelectItem(this ListBox lb, string text, bool clear = false)
		{
			if (lb.SelectionMode == SelectionMode.One)
			{
				var index = lb.FindString(text);

				if (index != ListBox.NoMatches)
					lb.SetSelected(index, true);
				else if (clear)
					lb.ClearSelected();
			}
			else if (lb.SelectionMode != SelectionMode.None)
			{
				for (var i = 0; i < lb.Items.Count; i++)
					if (lb.Items[i] is string s)
						lb.SetSelected(i, s.StartsWith(text, StringComparison.CurrentCultureIgnoreCase));
			}
		}

		public static void SelectItem(this ComboBox cb, string text, bool clear = false)
		{
			var index = cb.FindString(text);

			if (index != ListBox.NoMatches)
				cb.SelectedIndex = index;
			else if (clear)
				cb.SelectedIndex = ListBox.NoMatches;
		}

		public static void SetFont(this Control control, params object[] obj)
		{
			var (options, family) = obj.L().S2();
			control.Font = Conversions.ParseFont(control.Font, options, family);
			var c = Control.DefaultForeColor;
			var opts = Options.ParseOptions(options);

			foreach (var opt in opts)
				if (Options.TryParse(opt, "c", ref c))
				{
					control.ForeColor = c;
					break;
				}
		}

		public static void SetFormat(this DateTimePicker dtp, string format)
		{
			if (string.Compare(format, "shortdate", true) == 0)
				dtp.Format = DateTimePickerFormat.Short;
			else if (string.Compare(format, "longdate", true) == 0)
				dtp.Format = DateTimePickerFormat.Long;
			else if (string.Compare(format, "time", true) == 0)
				dtp.Format = DateTimePickerFormat.Time;
			else if (format != "")
			{
				dtp.Format = DateTimePickerFormat.Custom;
				dtp.CustomFormat = format;
			}
		}

		public static void SuspendDrawing(this Control control) => _ = WindowsAPI.SendMessage(control.Handle, WindowsAPI.WM_SETREDRAW, 0, 0);

		private static void GetMenuItems(ToolStripItem item, List<ToolStripItem> items)
		{
			items.Add(item);

			if (item is ToolStripMenuItem tsmi)
			{
				foreach (ToolStripItem i in tsmi.DropDownItems)
				{
					if (i is ToolStripMenuItem item1)
					{
						GetMenuItems(item1, items);
					}
				}
			}
		}
	}
}