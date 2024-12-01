namespace Keysharp.Core
{
	public class Menu
	{
		internal ToolStripItem defaultItem;
		protected long dummyHandle;
		private static int menuCount = 0;
		private readonly Dictionary<ToolStripItem, List<IFuncObj>> clickHandlers = [];

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

		public static object MenuFromHandle(object obj)
		{
			var handle = new IntPtr(obj.Al());
			var menu = Control.FromHandle(handle);

			if (menu != null)
				return menu;

			if ((menu = Control.FromHandle(handle)) != null)
				return menu;

			return "";
		}

		public static Menu New() => new ();

		public ToolStripMenuItem Add(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			if (obj0 == null && obj1 == null && obj2 == null)
			{
				_ = GetMenu().Items.Add(new ToolStripSeparator());
				return null;
			}

			return AddOrInsert("", obj0.As(), obj1, obj2.As());
		}

		public void AddStandard()
		{
			var menu = GetMenu();
			var emptyfunc = new Func<object>(() => "");
			var openfunc = new Func<object>(() =>
			{
				var mainWindow = Script.mainWindow;

				if (mainWindow != null && Accessors.A_AllowMainWindow.Ab())
				{
					mainWindow.AllowShowDisplay = true;
					mainWindow.Show();
					mainWindow.BringToFront();
					mainWindow.WindowState = mainWindow.lastWindowState;
				}

				return "";
			});
			var reloadfunc = new Func<object>(() =>
			{
				_ = Flow.Reload();
				return "";
			});
			var suspend = new Func<object>(() =>
			{
				Script.SuspendHotkeys();
				return "";
			});
			var exitfunc = new Func<object>(() =>
			{
				_ = Flow.ExitAppInternal(Flow.ExitReasons.Menu);
				return "";
			});
			//Won't be a gui target, so won't be marked as IsGui internally, but it's ok because it's only ever called on the gui thread in response to gui events.
			Script.openMenuItem = Add("&Open", new FuncObj(openfunc.Method, openfunc.Target));

			if (!Accessors.A_AllowMainWindow.Ab())
				Script.openMenuItem.Visible = false;

			//Need to fill in the event handlers for help and window spy when the proper functionality is implemented.//TODO
			//_ = Add("&Help", new FuncObj(emptyfunc.Method, emptyfunc.Target));
			if (menu.Items.Cast<ToolStripItem>().Any(tsi => tsi.Visible))
				_ = menu.Items.Add(new ToolStripSeparator());

			//_ = Add("&Window Spy", new FuncObj(emptyfunc.Method, emptyfunc.Target));
			_ = Add("&Reload Script", new FuncObj(reloadfunc.Method, reloadfunc.Target));

			if (!Accessors.A_IsCompiled)
			{
				var editfunc = new Func<object>(() =>
				{
					Script.Edit();
					return "";
				});
				_ = Add("&Edit Script", new FuncObj(editfunc.Method, editfunc.Target));
			}

			_ = menu.Items.Add(new ToolStripSeparator());
			Script.suspendMenuItem = Add("&Suspend Hotkeys", new FuncObj(suspend.Method, suspend.Target));
			_ = Add("&Exit", new FuncObj(exitfunc.Method, exitfunc.Target));
		}

		public bool Check(object obj) => Check(obj.As(), eCheckToggle.Check);

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

		public bool Disable(object obj) => Enable(obj.As(), eCheckToggle.Uncheck);

		public bool Enable(object obj) => Enable(obj.As(), eCheckToggle.Check);

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
			var width = obj3.Ai();

			if (GetMenuItem(name) is ToolStripItem tsmi)
			{
				if (ImageHelper.LoadImage(filename, width, 0, iconnumber).Item1 is Bitmap bmp)
					tsmi.Image = bmp;
			}
		}

		public void Show(object obj0 = null, object obj1 = null)
		{
			var x = obj0.Ai(Cursor.Position.X);
			var y = obj1.Ai(Cursor.Position.Y);
			var pt = new Point(x, y);

			if (Mouse.Coords.Menu == CoordModeType.Screen)
				if (Form.ActiveForm is Form form)
					pt = form.PointToClient(pt);

			MenuItem.Show(pt);
		}

		public void ShowItem(object obj) => MakeVisible(obj.As(), eCheckToggle.Check);

		public bool ToggleCheck(object obj) => Check(obj.As(), eCheckToggle.Toggle);

		public bool ToggleEnable(object obj) => Enable(obj.As(), eCheckToggle.Toggle);

		public void ToggleItemVis(object obj) => MakeVisible(obj.As(), eCheckToggle.Toggle);

		public bool UnCheck(object obj) => Check(obj.As(), eCheckToggle.Uncheck);

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
				if (string.IsNullOrEmpty(Default) && item.Text == "&Open")
				{
					Default = "&Open";
				}

				if (funcorsub is Menu mnu)
				{
					var fromMenuItems = mnu.GetMenu().Items;

					while (fromMenuItems.Count > 0)//Must use this because add range doesn't work.
					{
						var moveItem = fromMenuItems[0];
#if WINDOWS
						_ = item.DropDownItems.Add(moveItem);
#else
						//Windows automatically removes a menu item from one collection when it is added to another, but linux doesn't.
						//So it must be done manually here by reassigning the owner.
						moveItem.Owner = item.DropDown;
#endif
					}
				}
				else
					clickHandlers.GetOrAdd(item).ModifyEventHandlers(Functions.GetFuncObj(funcorsub, null, true), 1);

				foreach (Range r in options.AsSpan().SplitAny(Spaces))
				{
					var opt = options.AsSpan(r).Trim();

					if (opt.Length > 0)
					{
						var temp = 0;
						var tempbool = false;

						if (Options.TryParse(opt, "P", ref temp)) { }
						else if (Options.TryParse(opt, "Radio", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { }
						else if (Options.TryParse(opt, "Right", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true))
						{
							item.TextAlign = tempbool ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
						}
						else if (Options.TryParse(opt, "Break", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { }
						else if (Options.TryParse(opt, "BarBreak", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { }
					}
				}
			}

			return item;
		}

		private bool Check(string s, eCheckToggle checktoggle)
		{
			if (GetMenuItem(s) is ToolStripMenuItem item)
			{
				if (checktoggle == eCheckToggle.Check)
					item.Checked = true;
				else if (checktoggle == eCheckToggle.Uncheck)
					item.Checked = false;
				else
					item.Checked = !item.Checked;

				return item.Checked;
			}

			return false;
		}

		private bool Enable(string s, eCheckToggle checktoggle)
		{
			if (GetMenuItem(s) is ToolStripItem item)
			{
				if (checktoggle == eCheckToggle.Check)
					item.Enabled = true;
				else if (checktoggle == eCheckToggle.Uncheck)
					item.Enabled = false;
				else
					item.Enabled = !item.Enabled;

				return item.Enabled;
			}

			return false;
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