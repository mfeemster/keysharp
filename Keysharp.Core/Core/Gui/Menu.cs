using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Keysharp.Core.Common;
using static Keysharp.Core.Core;

namespace Keysharp.Core
{
	public class Menu
	{
		internal ToolStripItem defaultItem;
		private static int menuCount = 0;
		private Dictionary<ToolStripItem, List<GenericFunction>> clickHandlers = new Dictionary<ToolStripItem, List<GenericFunction>>();

		public long ClickCount { get; set; } = 2;

		public string Default
		{
			get => defaultItem != null ? defaultItem.Text : "";

			set
			{
				if (GetMenuItem(value) is ToolStripMenuItem item)
				{
					var allitems = GetMenu().GetItems();
					defaultItem = item;

					foreach (var defitem in allitems)
						defitem.Font = defitem == item
									   ? new Font(item.Font, item.Font.Style | FontStyle.Bold)
									   : new Font(item.Font, item.Font.Style & ~FontStyle.Bold);
				}
				else
					defaultItem = null;
			}
		}

		public long MenuItemCount => GetMenu().Items.Count;

		public long Handle => GetMenu().Handle.ToInt64();

		internal ContextMenuStrip MenuItem { get; set; } = new ContextMenuStrip();

		public Menu(params object[] obj)
		{
			GetMenu().ImageScalingSize = new System.Drawing.Size(28, 28);
			var newCount = Interlocked.Increment(ref menuCount);
			GetMenu().Name = $"Menu_{newCount}";
		}

		public static Menu New() => new Menu();

		public ToolStripMenuItem Add(params object[] obj)
		{
			if (obj.Length == 0)
			{
				_ = GetMenu().Items.Add(new ToolStripSeparator());
				return null;
			}

			var (name, funcorsub, options) = obj.L().Sos();
			return AddOrInsert("", name, funcorsub, options);
		}

		public void AddStandard(params object[] obj)
		{
			ToolStripMenuItem item;

			if (Accessors.A_AllowMainWindow)
			{
				item = Add("&Open");

				if (defaultItem == null)
					Default = item.Name;
			}

			item = Add("&Help");
			item = Add();
			item = Add("&Window Spy");
			item = Add("&Reload Script");
			item = Add("&Edit Script");
			item = Add();
			item = Add("&Suspend Hotkeys");
			item = Add("&Pause Script");
			item = Add("E&xit");
		}

		public void Check(params object[] obj) => Check(obj.L().S1(), eCheckToggle.Check);

		public void Delete(params object[] obj)
		{
			var s = obj.L().S1();

			if (s?.Length == 0)
			{
				GetMenu().Items.Clear();
				clickHandlers.Clear();
				return;
			}

			if (GetMenuItem(s) is ToolStripItem item)
			{
				if (item == defaultItem)
					defaultItem = null;

				if (item.GetCurrentParent() is ToolStripDropDownMenu tsddm)
					tsddm.Items.Remove(item);
				else
					GetMenu().Items.Remove(item);

				GetMenu().Refresh();
				_ = clickHandlers.Remove(item);
			}
		}

		public void Disable(params object[] obj) => Enable(obj.L().S1(), eCheckToggle.Uncheck);

		public void Enable(params object[] obj) => Enable(obj.L().S1(), eCheckToggle.Check);

		public void HideItem(params object[] obj) => MakeVisible(obj.L().S1(), eCheckToggle.Uncheck);

		public ToolStripMenuItem Insert(params object[] obj)
		{
			var (insertbefore, name, funcorsub, options) = obj.L().S2os();
			return AddOrInsert(insertbefore, name, funcorsub, options);
		}

		public string MenuItemId(params object[] obj)
		{
			var s = obj.L().S1();
			return GetMenuItem(s) is ToolStripMenuItem tsmi ? tsmi.Name : "";
		}

		public void Rename(params object[] obj)
		{
			var (name, newname) = obj.L().S2("", "-");
			var item = GetMenuItem(name);

			if (item is ToolStripSeparator tss)
			{
				var index = (int)GetIndex(tss);
				var newItem = new ToolStripMenuItem(newname);
				newItem.Name = newItem.Text = newname;
				GetMenu().Items.RemoveAt(index);
				GetMenu().Items.Insert(index, newItem);
			}

			if (item is ToolStripItem tsi)
				tsi.Name = tsi.Text = newname;
		}

		public void SetColor(params object[] obj)
		{
			var (name, submenus) = obj.L().Sb("", true);
			HandleColor(GetMenu(), name, submenus, true);
		}

		public void SetForeColor(params object[] obj)
		{
			var (name, submenus) = obj.L().Sb("", true);
			HandleColor(GetMenu(), name, submenus, false);
		}

		public void SetIcon(params object[] obj)
		{
			var (name, filename, iconnumber, width) = obj.L().S2i2("", "", 1, 0);

			if (GetMenuItem(name) is ToolStripItem tsmi)
			{
				if (iconnumber > 0)
					iconnumber--;

				if (ImageHelper.LoadImage(filename, width, 0, iconnumber) is Bitmap bmp)
					tsmi.Image = bmp;
			}
		}

		public void Show(params object[] obj)
		{
			var (x, y) = obj.L().I2(Cursor.Position.X, Cursor.Position.Y);
			var pt = new Point(x, y);

			if (Mouse.Coords.Menu == CoordModeType.Screen)
				if (Form.ActiveForm is Form form)
					pt = form.PointToClient(pt);

			MenuItem.Show(pt);
		}

		public void ShowItem(params object[] obj) => MakeVisible(obj.L().S1(), eCheckToggle.Check);

		public void ToggleCheck(params object[] obj) => Check(obj.L().S1(), eCheckToggle.Toggle);

		public void ToggleEnable(params object[] obj) => Enable(obj.L().S1(), eCheckToggle.Toggle);

		public void ToggleItemVis(params object[] obj) => MakeVisible(obj.L().S1(), eCheckToggle.Toggle);

		public void UnCheck(params object[] obj) => Check(obj.L().S1(), eCheckToggle.Uncheck);

		internal void Tsmi_Click(object sender, EventArgs e)
		{
			if (sender is ToolStripMenuItem tsmi)
			{
				if (clickHandlers.TryGetValue(tsmi, out var handler))
				{
					var index = GetIndex(tsmi);
					_ = handler.InvokeEventHandlers(tsmi.Text, ++index, this);
				}
			}
		}

		protected static void HandleColor(ToolStrip menu, string name, bool submenus, bool backcolor)
		{
			if (Conversions.TryParseColor(name, out var color))
			{
				if (backcolor)
					menu.BackColor = color;
				else
					menu.ForeColor = color;

				if (submenus)
				{
					var items = menu.GetItems();

					foreach (var item in items)
						if (backcolor)
							item.BackColor = color;
						else
							item.ForeColor = color;
				}
			}
		}

		protected virtual long GetIndex(ToolStripItem tsi) => tsi.GetCurrentParent() is ToolStripDropDownMenu tsddm ? tsddm.Items.IndexOf(tsi) : GetMenu().Items.IndexOf(tsi);

		protected virtual ToolStrip GetMenu() => MenuItem;

		protected virtual ToolStripItem GetMenuItem(string s)
		{
			if (s.EndsWith('&') && int.TryParse(s.Trim('&'), out var i) && i > 0)
			{
				if (GetMenu().Items[--i] is ToolStripItem tsmi)
					return tsmi;
			}
			else if (GetMenu().Items.Find(s, true).FirstOrDefault() is ToolStripItem tsmi)
				return tsmi;

			return null;
		}

		private ToolStripMenuItem AddOrInsert(string insertbefore, string name, object funcorsub, string options)
		{
			ToolStripMenuItem item = null;

			if (!string.IsNullOrEmpty(insertbefore))
			{
				if (GetMenu().Items.Find(insertbefore, true).FirstOrDefault() is ToolStripMenuItem tsmiinsert)
				{
					var index = GetIndex(tsmiinsert);

					if (tsmiinsert.GetCurrentParent() is ToolStripDropDownMenu tsddm)
					{
						if (name?.Length == 0)
						{
							tsddm.Items.Insert((int)index, new ToolStripSeparator());
							return null;
						}
						else
						{
							item = new ToolStripMenuItem(name);
							item.Click += Tsmi_Click;
							item.Name = name;
							tsddm.Items.Insert((int)index, item);
						}
					}
					else
					{
						item = new ToolStripMenuItem(name);
						item.Click += Tsmi_Click;
						item.Name = name;
						GetMenu().Items.Insert((int)index, item);
					}
				}
			}
			else
			{
				if (GetMenu().Items.Find(name, true).FirstOrDefault() is ToolStripMenuItem tsmi)
				{
					item = tsmi;
				}
				else if (GetMenu().Items.Add(name) is ToolStripMenuItem tsmi2)
				{
					tsmi2.Click += Tsmi_Click;
					tsmi2.Name = name;
					item = tsmi2;
				}
			}

			if (item != null)
			{
				if (funcorsub is Menu mnu)
				{
					while (mnu.MenuItem.Items.Count > 0)//Must use this because add range doesn't work.
						_ = item.DropDownItems.Add(mnu.MenuItem.Items[0]);
				}
				else if (funcorsub is GenericFunction gf)
				{
					var handler = clickHandlers.GetOrAdd(item);
					handler.ModifyEventHandlers(gf, 1);
				}
				else if (GuiControl.GetDel(funcorsub, null) is GenericFunction del)//Pass null for the eventObj argument because this isn't associated with any particular form.
				{
					var handler = clickHandlers.GetOrAdd(item);
					handler.ModifyEventHandlers(del, 1);
				}

				foreach (var opt in Options.ParseOptions(options))
				{
					var temp = 0;
					var tempbool = false;

					if (Options.TryParse(opt, "P", ref temp)) { }
					else if (Options.TryParse(opt, "Radio", ref tempbool, System.StringComparison.OrdinalIgnoreCase, true, true)) { }
					else if (Options.TryParse(opt, "Right", ref tempbool, System.StringComparison.OrdinalIgnoreCase, true, true))
					{
						item.TextAlign = tempbool ? System.Drawing.ContentAlignment.MiddleRight : System.Drawing.ContentAlignment.MiddleLeft;
					}
					else if (Options.TryParse(opt, "Break", ref tempbool, System.StringComparison.OrdinalIgnoreCase, true, true)) { }
					else if (Options.TryParse(opt, "BarBreak", ref tempbool, System.StringComparison.OrdinalIgnoreCase, true, true)) { }
				}
			}

			return item;
		}

		private void Check(string s, eCheckToggle checktoggle)
		{
			if (GetMenuItem(s) is ToolStripMenuItem item)
			{
				if (checktoggle == eCheckToggle.Check)
					item.Checked = true;
				else if (checktoggle == eCheckToggle.Uncheck)
					item.Checked = false;
				else
					item.Checked = !item.Checked;
			}
		}

		private void Enable(string s, eCheckToggle checktoggle)
		{
			if (GetMenuItem(s) is ToolStripItem item)
			{
				if (checktoggle == eCheckToggle.Check)
					item.Enabled = true;
				else if (checktoggle == eCheckToggle.Uncheck)
					item.Enabled = false;
				else
					item.Enabled = !item.Enabled;
			}
		}

		private void MakeVisible(string s, eCheckToggle vis)
		{
			if (GetMenuItem(s) is ToolStripMenuItem item)
				if (vis == eCheckToggle.Toggle)
					item.Visible = !item.Visible;
				else if (vis == eCheckToggle.Check)
					item.Visible = true;
				else
					item.Visible = false;
		}

		private void subMenuItem1_Click(object sender, EventArgs e)
		{
		}

		private enum eCheckToggle
		{
			Check,
			Uncheck,
			Toggle
		}
	}

	public class MenuBar : Menu
	{
		internal MenuStrip MenuStrip { get; } = new MenuStrip();

		public MenuBar()
		{
			MenuStrip.Dock = DockStyle.Top;
			MenuStrip.ImageScalingSize = new System.Drawing.Size(28, 28);
		}

		public new void Add(params object[] obj)
		{
			var items = obj.L();

			if (items.Count > 0 && items[0] is string s)
			{
				if (items.Count > 1 && items[1] is Menu mnu)
				{
					var temp = GetMenuItem(s);
					ToolStripMenuItem newItem = null;

					if (temp is ToolStripMenuItem tsmi)
					{
						tsmi.DropDownItems.Clear();
						newItem = tsmi;
					}
					else if (MenuStrip.Items.Add(s) is ToolStripMenuItem tsmi2)
					{
						tsmi2.Name = tsmi2.Text;
						newItem = tsmi2;
					}

					if (newItem != null)
						while (mnu.MenuItem.Items.Count > 0)//Must use this because add range doesn't work.
							_ = newItem.DropDownItems.Add(mnu.MenuItem.Items[0]);
				}
				else
				{
					var newItem = MenuStrip.Items.Add(s);
					newItem.Name = newItem.Text;
				}
			}
		}

		protected override long GetIndex(ToolStripItem tsi) => MenuStrip.Items.IndexOf(tsi);

		protected override ToolStrip GetMenu() => MenuStrip;
	}
}