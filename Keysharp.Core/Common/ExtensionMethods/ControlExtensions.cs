namespace System.Windows.Forms
{
	/// <summary>
	/// Extension methods for various WinForms Control classes.
	/// By using extension methods, it relieves us of having to write some of these methods twice.
	/// For example, SetFont() exists on Gui as well as on GuiControl. Rather than implement that function
	/// in both classes, we just implement it once here as an extension for Control, then expose a wrapper in each of these classes
	/// which just passes the arguments to the extension method here, which gets called on either the underlying form or control member.
	/// </summary>
	internal static class ControlExtensions
	{
		/// <summary>
		/// Calls an Action inside of BeginInvoke() with various checks/options.
		/// Before invoking on the control, ensure it's not null, has been created and is not disposing/disposed.
		/// </summary>
		/// <param name="control">The Control to call BeginInvoke() on.</param>
		/// <param name="action">The Action to invoke.</param>
		/// <param name="runIfNull">True to call action inline if control is null, else don't call action.</param>
		/// <param name="forceInvoke">True to call BeginInvoke() on control, even if it's not required.</param>
		internal static void CheckedBeginInvoke(this Control control, Action action, bool runIfNull, bool forceInvoke)
		{
			if (control == null)
			{
				if (runIfNull)
					action();

				return;
			}

			if (!runIfNull && (!control.IsHandleCreated || control.IsDisposed || control.Disposing))
				return;

			if ((control.InvokeRequired || forceInvoke) && control.IsHandleCreated && !control.IsDisposed && !control.Disposing)
				_ = control.BeginInvoke(action);
			else
				action();
		}

		/// <summary>
		/// Calls an Action inside of Invoke() with various checks/options.
		/// Before invoking on the control, ensure it's not null, has been created and is not disposing/disposed.
		/// </summary>
		/// <param name="control">The Control to call Invoke() on.</param>
		/// <param name="action">The Action to invoke.</param>
		/// <param name="runIfNull">True to call action inline if control is null, else don't call action.</param>
		internal static void CheckedInvoke(this Control control, Action action, bool runIfNull)
		{
			if (control == null)
			{
				if (runIfNull)
					action();

				return;
			}

			if (!runIfNull && (!control.IsHandleCreated || control.IsDisposed || control.Disposing))
				return;

			if (control.InvokeRequired && control.IsHandleCreated && !control.IsDisposed && !control.Disposing)
				control.Invoke(action);
			else
				action();
		}

		/// <summary>
		/// Calls a Func<typeparamref name="T"/> inside of Invoke() with various checks/options.
		/// Before invoking on the control, ensure it's not null, has been created and is not disposing/disposed.
		/// </summary>
		/// <param name="control">The Control to call Invoke() on.</param>
		/// <param name="action">The Func to invoke.</param>
		/// <param name="runIfNull">True to call action inline if control is null, else don't call action.</param>
		/// <returns>The result of action, of type <typeparamref name="T"/>.</returns>
		internal static T CheckedInvoke<T>(this Control control, Func<T> action, bool runIfNull)
		{
			if (control == null)
				return runIfNull ? action() : default;

			if (!runIfNull && (!control.IsHandleCreated || control.IsDisposed || control.Disposing))
				return default;

			if (control.InvokeRequired && control.IsHandleCreated && !control.IsDisposed && !control.Disposing)
				return (T)control.Invoke(action);//Linux needs the cast.
			else
				return action();
		}

		/// <summary>
		/// Finds the parent of a control whose type matches <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type of the parent control to match.</typeparam>
		/// <param name="control">The control whose parent is to be found.</param>
		/// <returns>The first parent matching type <typeparamref name="T"/>, else null.</returns>
		internal static T FindParent<T>(this Control control) where T : Control
		{
			var parent = control.Parent;

			while (parent != null)
			{
				if (parent is T pt)
					return pt;

				parent = parent.Parent;
			}

			return null;
		}

		/// <summary>
		/// Finds all parents of a control whose type matches <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type of the parent controls to match.</typeparam>
		/// <param name="control">The control whose parents are to be found.</param>
		/// <returns>A HashSet<typeparamref name="T"/> of parents whose type matched <typeparamref name="T"/>.</returns>
		internal static HashSet<T> FindParents<T>(this Control control) where T : Control
		{
			var parent = control.Parent;
			var parents = new HashSet<T>();

			while (parent != null)
			{
				if (parent is T pt)
					parents.Add(pt);

				parent = parent.Parent;
			}

			return parents;
		}

		/// <summary>
		/// Finds the first tab within a TabControl whose text matches text.
		/// The search is case insensitive.
		/// </summary>
		/// <param name="tc">The TabControl to search.</param>
		/// <param name="text">The text to match.</param>
		/// <param name="exact">True to match the entire text of a tab, else only match the start of the text.</param>
		/// <returns>The matching tab if found, else null.</returns>
		internal static TabPage FindTab(this TabControl tc, string text, bool exact)
		{
			foreach (TabPage tp in tc.TabPages)
				if (exact)
				{
					if (string.Compare(tp.Text, text, true) == 0)
						return tp;
				}
				else if (tp.Text.StartsWith(text, StringComparison.OrdinalIgnoreCase))
					return tp;

			return null;
		}

		/// <summary>
		/// Returns a collection of all child controls of a Control whose type matches <typeparamref name="T"/>, recursively.
		/// </summary>
		/// <typeparam name="T">The type of the controls to find.</typeparam>
		/// <param name="control">The Control whose children will be matched.</param>
		/// <returns>A HashSet<typeparamref name="T"/> of children whose type matched <typeparamref name="T"/>.</returns>
		internal static HashSet<T> GetAllControlsRecursive<T>(this Control control) where T : class, new ()
		{
			var rtn = new HashSet<T>();

			foreach (Control item in control.Controls)
			{
				if (item is T ctrl)
					_ = rtn.Add(ctrl);

				rtn.AddRange(GetAllControlsRecursive<T>(item));
			}

			return rtn;
		}

		/// <summary>
		/// Returns the Tag member of a Control as a GuiControl.
		/// </summary>
		/// <param name="control">The Control whose Tag member will be returned.</param>
		/// <returns>A GuiControl if the Tag member was a GuiTag, else null.</returns>
		internal static GuiControl GetGuiControl(this Control control) => control.Tag is GuiTag guiTag ? guiTag.GuiControl : null;

		/// <summary>
		/// Returns all child ToolStripItems of a ToolStrip, recursively.
		/// Gotten from here and fixed: https://www.codeproject.com/tips/264690/how-to-iterate-recursive-through-all-menu-items-in
		/// </summary>
		/// <param name="menuStrip">The ToolStrip whose child items will be returned.</param>
		/// <returns>The child items of the ToolStrip.</returns>
		internal static List<ToolStripItem> GetItems(this ToolStrip menuStrip)
		{
			var myItems = new List<ToolStripItem>();

			foreach (var o in menuStrip.Items)
				if (o is ToolStripItem i)
					GetMenuItems(i, myItems);

			return myItems;
		}

		//internal static T GetNthControlRecursive<T>(this Control control, int index) where T : class, new ()
		//{
		//  T ctrl = null;
		//  var list = control.GetAllControlsRecursive<T>().ToList();

		//  for (var i = 0; i <= index; i++)
		//      if (i == index)
		//      {
		//          ctrl = list[i];
		//          break;
		//      }

		//  return ctrl;
		//}

		//internal static T GetParentOfType<T>(this Control control) where T : class, new ()
		//{
		//  while (control != null && control.Parent is Control cp)
		//  {
		//      if (cp is T t)
		//          return t;

		//      control = cp;
		//  }

		//  return null;
		//}

		/// <summary>
		/// Resume drawing of a Control.
		/// </summary>
		/// <param name="control">The Control to resume drawing for.</param>
		internal static void ResumeDrawing(this Control control)
		{
#if LINUX
			control.ResumeLayout();
#elif WINDOWS
			_ = WindowsAPI.SendMessage(control.Handle, WindowsAPI.WM_SETREDRAW, 1, 0);
#endif
			control.Refresh();
		}

		/// <summary>
		/// Finds the right most and bottom most child controls of a Control.
		/// The .Right and .Bottom properties of the controls are used to identify the controls.
		/// </summary>
		/// <param name="control">The Control whose children will be traversed.</param>
		/// <returns>A Control,Control tuple containing the right and bottom most child controls of the Control.
		/// null,null if none found.
		/// </returns>
		internal static (Control, Control) RightBottomMost(this Control control)
		{
			var maxx = 0;
			var maxy = 0;
			(Control right, Control bottom) p = (null, null);

			foreach (Control ctrl in control.Controls)
			{
				if (!(ctrl is KeysharpStatusStrip))//Don't count a status strip in the bounds since its placement is handled manually.
				{
					var temp = ctrl.Right;

					if (temp > maxx)
					{
						maxx = temp;
						p.right = ctrl;
					}

					temp = ctrl.Bottom;

					if (temp > maxy)
					{
						maxy = temp;
						p.bottom = ctrl;
					}
				}
			}

			return p;
		}

		/// <summary>
		/// Selects or deselects a ListBox item based on a text match.
		/// If no match is found, all existing selections are cleared.
		/// </summary>
		/// <param name="lb">The ListBox whose item will be selected or deselected.</param>
		/// <param name="text">The text of the item to match.</param>
		/// <param name="clear">True to deselect the item. Default: false.</param>
		internal static void SelectItem(this ListBox lb, string text, bool clear = false)
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

		/// <summary>
		/// Selects or deselects a ComboBox item based on a text match.
		/// If no match is found, the existing selection is cleared.
		/// </summary>
		/// <param name="cb">The ComboBox whose item will be selected or deselected.</param>
		/// <param name="text">The text of the item to match.</param>
		/// <param name="clear">True to deselect the item. Default: false.</param>
		internal static void SelectItem(this ComboBox cb, string text, bool clear = false)
		{
			var index = cb.FindString(text);

			if (index != ListBox.NoMatches)
				cb.SelectedIndex = index;
			else if (clear)
				cb.SelectedIndex = ListBox.NoMatches;
		}

		/// <summary>
		/// Sets the font of a control.
		/// </summary>
		/// <param name="control">The Control whose font will be set.</param>
		/// <param name="options">The font options.</param>
		/// <param name="family">The font family.</param>
		internal static void SetFont(this Control control, object options = null, object family = null)
		{
			var opts = options.As();
			control.Font = Conversions.ParseFont(control.Font, opts, family.As());
			var c = Control.DefaultForeColor;

			//Special processing is required to set the ForeColor that is not present in Conversions.ParseFont().
			foreach (Range r in opts.AsSpan().SplitAny(Keywords.Spaces))
			{
				var opt = opts.AsSpan(r).Trim();

				if (opt.Length > 0 && Options.TryParse(opt, "c", ref c))
				{
					control.ForeColor = c;
					break;
				}
			}
		}

		/// <summary>
		/// Sets the format of a DateTimePicker control.
		/// </summary>
		/// <param name="dtp">The DateTimePicker whose format will be set.</param>
		/// <param name="format">The format to use, such as "shortdate", "longdate", "time" or "".</param>
		internal static void SetFormat(this DateTimePicker dtp, object format)
		{
			var fmt = format.As();

			if (string.Compare(fmt, "shortdate", true) == 0)
				dtp.Format = DateTimePickerFormat.Short;
			else if (string.Compare(fmt, "longdate", true) == 0)
				dtp.Format = DateTimePickerFormat.Long;
			else if (string.Compare(fmt, "time", true) == 0)
				dtp.Format = DateTimePickerFormat.Time;
			else if (fmt != "")
			{
				dtp.Format = DateTimePickerFormat.Custom;
				dtp.CustomFormat = fmt;
			}
		}

		/// <summary>
		/// Suspends drawing for a Control.
		/// </summary>
		/// <param name="control">The Control to suspend drawing for.</param>
		internal static void SuspendDrawing(this Control control)
		{
#if LINUX
			control.SuspendLayout();
#elif WINDOWS
			_ = WindowsAPI.SendMessage(control.Handle, WindowsAPI.WM_SETREDRAW, 0, 0);
#endif
		}

		/// <summary>
		/// Returns the height of the tabs of a TabControl.
		/// Returns 0 if the tabs are not on the top or bottom of the control.
		/// This uses GetTabRect() rather than TabControl.ItemSize.Height because the latter
		/// doesn't work.
		/// </summary>
		/// <param name="control">The TabControl whose tab height will be returned.</param>
		/// <returns>The height of the tabs if found, else 0.</returns>
		internal static int TabHeight(this TabControl control)
		{
			if (control.TabPages.Count > 0)
			{
				if (control.Alignment == TabAlignment.Top || control.Alignment == TabAlignment.Bottom)
					return control.GetTabRect(0).Height;//GetTabRect() works, tc.ItemSize.Height does not.
			}

			return 0;
		}

		/// <summary>
		/// Returns the width of the tabs of a TabControl.
		/// Returns 0 if the tabs are not on the left or right of the control.
		/// This uses GetTabRect() rather than TabControl.ItemSize.Width because the latter
		/// doesn't work.
		/// </summary>
		/// <param name="control">The TabControl whose tab width will be returned.</param>
		/// <returns>The width of the tabs if found, else 0.</returns>
		internal static int TabWidth(this TabControl control)
		{
			if (control.TabPages.Count > 0)
			{
				if (control.Alignment == TabAlignment.Left || control.Alignment == TabAlignment.Right)
					return control.GetTabRect(0).Width;
			}

			return 0;
		}

		/// <summary>
		/// Tags a Control by setting its Tag property to a GuiTag object
		/// with its Index property set to the current count of child controls
		/// of its parent.
		/// This is used in cases where the index of each control must be known and preserved
		/// because the Controls collection of child controls of a Control are not necessarily
		/// stored in the order they were added.
		/// </summary>
		/// <param name="control">The Control to add a newly tagged Control to.</param>
		/// <param name="add">The Control to tag and add to control.</param>
		internal static void TagAndAdd(this Control control, Control add)
		{
			add.Tag = new GuiTag { Index = control.Controls.Count };
			control.Controls.Add(add);
		}

		/// <summary>
		/// Tags a GuiControl by setting the Index property of its Tag property to the current count of child controls
		/// of its parent.
		/// This is used in cases where the index of each control must be known and preserved
		/// because the Controls collection of child controls of a Control are not necessarily
		/// stored in the order they were added.
		/// </summary>
		/// <param name="control">The Control to add a newly tagged GuiControl to.</param>
		/// <param name="add">The GuiControl to tag and add to control.</param>
		internal static void TagAndAdd(this Control control, GuiControl add)
		{
			((GuiTag)add.Control.Tag).Index = control.Controls.Count;//The reference to the control was set in the constructor, so set the index here.
			control.Controls.Add(add.Control);
		}

		/// <summary>
		/// Recursively adds all child menu items of a ToolStripItem to the passed in list.
		/// </summary>
		/// <param name="item">The ToolStripMenuItem whose DropDownItems property will be traversed.</param>
		/// <param name="items">All list of all recursively found ToolStripMenuItem.DropDownItems which are of type ToolStripMenuItem.</param>
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