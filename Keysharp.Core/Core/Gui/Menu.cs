using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Keysharp.Core.Common;

namespace Keysharp.Core
{
	public class Menu
	{
		internal ToolStripItem defaultItem;
		protected long dummyHandle;
		private static int menuCount = 0;
		private Dictionary<ToolStripItem, List<IFuncObj>> clickHandlers = new Dictionary<ToolStripItem, List<IFuncObj>>();
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

		public long Handle => GetMenu().Handle.ToInt64();
		public long MenuItemCount => GetMenu().Items.Count;
		internal ContextMenuStrip MenuItem { get; set; }

		public Menu(ContextMenuStrip strip = null)
		{
			MenuItem = strip ?? new ContextMenuStrip();
			//GetMenu().ImageScalingSize = new System.Drawing.Size(28, 28);//Don't set scaling, it makes the checked icons look funny.
			var newCount = Interlocked.Increment(ref menuCount);
			GetMenu().Name = $"Menu_{newCount}";
			dummyHandle = Handle;
		}

		public static Menu New() => new Menu();

		public ToolStripMenuItem Add(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			if (obj0 == null && obj1 == null && obj2 == null)
			{
				_ = GetMenu().Items.Add(new ToolStripSeparator());
				return null;
			}

			return AddOrInsert("", obj0.As(), obj1, obj2.As());
		}

		public void AddStandard()//Need to make this actually od something//TODO
		{
			ToolStripMenuItem item;

			if (Accessors.A_AllowMainWindow is bool b && b)
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

		public void Check(object obj) => Check(obj.As(), eCheckToggle.Check);

		public void Delete(object obj)
		{
			var s = obj.As();

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

		public void Disable(object obj) => Enable(obj.As(), eCheckToggle.Uncheck);

		public void Enable(object obj) => Enable(obj.As(), eCheckToggle.Check);

		public void HideItem(object obj) => MakeVisible(obj.As(), eCheckToggle.Uncheck);

		public ToolStripMenuItem Insert(object obj0 = null, object obj1 = null, object obj2 = null, object obj3 = null) => AddOrInsert(obj0.As(), obj1.As(), obj2, obj3.As());

		public string MenuItemId(object obj) => GetMenuItem(obj.As()) is ToolStripMenuItem tsmi ? tsmi.Name : "";

		public void Rename(object obj0, object obj1 = null)
		{
			var name = obj0.As();
			var newname = obj1.As("-");
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

		public void SetColor(object obj0 = null, object obj1 = null) => HandleColor(GetMenu(), obj0.As(), obj1.Ab(true), true);

		public void SetForeColor(object obj0 = null, object obj1 = null) => HandleColor(GetMenu(), obj0.As(), obj1.Ab(true), false);

		public void SetIcon(object obj0, object obj1, object obj2 = null, object obj3 = null)
		{
			var name = obj0.As();
			var filename = obj1.As();
			var iconnumber = ImageHelper.PrepareIconNumber(obj2);
			var width = (int)obj3.Al();

			if (GetMenuItem(name) is ToolStripItem tsmi)
			{
				if (ImageHelper.LoadImage(filename, width, 0, iconnumber) is Bitmap bmp)
					tsmi.Image = bmp;
			}
		}

		public void Show(object obj0 = null, object obj1 = null)
		{
			var x = (int)obj0.Al(Cursor.Position.X);
			var y = (int)obj1.Al(Cursor.Position.Y);
			var pt = new Point(x, y);

			if (Mouse.Coords.Menu == CoordModeType.Screen)
				if (Form.ActiveForm is Form form)
					pt = form.PointToClient(pt);

			MenuItem.Show(pt);
		}

		public void ShowItem(object obj) => MakeVisible(obj.As(), eCheckToggle.Check);

		public void ToggleCheck(object obj) => Check(obj.As(), eCheckToggle.Toggle);

		public void ToggleEnable(object obj) => Enable(obj.As(), eCheckToggle.Toggle);

		public void ToggleItemVis(object obj) => MakeVisible(obj.As(), eCheckToggle.Toggle);

		public void UnCheck(object obj) => Check(obj.As(), eCheckToggle.Uncheck);

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

		protected internal virtual ToolStrip GetMenu() => MenuItem;

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
				else if (funcorsub is IFuncObj fo)
					clickHandlers.GetOrAdd(item).ModifyEventHandlers(fo, 1);
				else if (funcorsub is string s && s.Length > 0)
					clickHandlers.GetOrAdd(item).ModifyEventHandlers(new FuncObj(s), 1);

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

		public MenuBar(ContextMenuStrip strip = null)
			: base(strip)
		{
			MenuStrip.Dock = DockStyle.Top;
		}

		protected internal override ToolStrip GetMenu() => MenuStrip;

		protected override long GetIndex(ToolStripItem tsi) => MenuStrip.Items.IndexOf(tsi);
	}
}