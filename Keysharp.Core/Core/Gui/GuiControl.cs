using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Keysharp.Core.Common;
using Keysharp.Core.Windows;

namespace Keysharp.Core
{
	public class GuiControl : KeysharpObject
	{
		internal MsgMonitorList monitorEvents;
		internal string typename;
		private readonly Control _control;
		private readonly List<IFuncObj> clickHandlers = new List<IFuncObj>();//These all need to be converted to FuncObj.
		private readonly List<IFuncObj> doubleClickHandlers = new List<IFuncObj>();

		//Normal event handlers can't be used becaused they need to return a value.
		//The returned values are then inspected to determine if subsequent handlers should be called or not.
		private List<IFuncObj> changeHandlers;

		private List<IFuncObj> columnClickHandlers;
		private Dictionary<int, List<IFuncObj>> commandHandlers;
		private List<IFuncObj> contextMenuChangedHandlers;
		private bool dpiscaling = true;
		private bool fireEvents = true;//Need to figure out how to enable/disable events.//MATT//TODO
		private long parenthandle;
		private List<IFuncObj> focusedItemChangedHandlers;
		private List<IFuncObj> focusHandlers;
		private List<IFuncObj> itemCheckHandlers;
		private List<IFuncObj> itemEditHandlers;
		private List<IFuncObj> itemExpandHandlers;
		private List<IFuncObj> lostFocusHandlers;
		private int mousecount = 0;
		private Dictionary<int, List<IFuncObj>> notifyHandlers;
		private List<IFuncObj> selectedItemChangedHandlers;
		public bool AltSubmit { get; internal set; } = false;

		public string ClassNN
		{
			get
			{
				var i = 0;

				foreach (Control ctrl in Gui.form.Controls)
				{
					if (Control.Tag is GuiControl gc)
					{
						if (string.Compare(gc.typename, typename, true) == 0)
							i++;

						if (ctrl == _control)
							break;
					}
				}

				return $"{typename}{i}";
			}
		}

		public Control Control => _control;

		public object Enabled
		{
			get => _control.Enabled;
			set => _control.Enabled = Options.OnOff(value) ?? false;
		}

		public object Focused => _control.Focused;

		public Gui Gui { get; private set; }

		public long Hwnd => _control.Handle.ToInt64();

		public object Name
		{
			get => _control.Name;
			set => _control.Name = value.ToString();
		}

		public KeysharpForm Parent
		{
			get
			{
				var parent = _control.Parent;

				do
				{
					if (parent is KeysharpForm kf)
						return kf;

					parent = parent.Parent;
				} while (parent != null);

				return parent as KeysharpForm;
			}
		}

		public object Text
		{
			get
			{
				if (_control is ListBox lb)
				{
					if (lb.SelectionMode == SelectionMode.One)
					{
						if (lb.SelectedIndex > 0 && lb.SelectedItem is string s)
							return s;
					}
					else
						return new Array(lb.SelectedItems);
				}

				if (_control is ComboBox cb)
				{
					if (cb.DropDownStyle == ComboBoxStyle.DropDownList && cb.SelectedIndex > 0 && cb.SelectedItem is string s)
						return s;
					else
						return cb.Text;
				}

				if (_control is StatusStrip ss)
					return ss.Items.Count > 0 ? ss.Items[0].Text : "";

				if (_control is TabControl tc && tc.SelectedTab is TabPage tp)
					return tp.Text;
				else
					return _control.Text;
			}
			set
			{
				if (value is string s)
				{
					if (_control is ListBox lb)
					{
						if (lb.SelectionMode == SelectionMode.One)
						{
							lb.SelectedItem = s;
						}
						else
						{
							for (var i = 0; i < lb.Items.Count; i++)
							{
								if (lb.Items[i] is string item && item == s)
									lb.SetSelected(i, true);
							}
						}
					}
					else if (_control is ComboBox cb)
					{
						if (s?.Length == 0)
							cb.SelectedItem = -1;

						if (cb.DropDownStyle == ComboBoxStyle.DropDownList)
							cb.SelectedItem = s;
						else
							cb.Text = s;
					}
					else if (_control is TabControl tc)
						tc.SelectTab(s);
					else
						_control.Text = s;
				}
			}
		}

		public string Type => typename;

		public object Value
		{
			get
			{
				if (_control is Label lbl)
					return lbl.Text;
				else if (_control is TextBox txt)
					return Strings.NormalizeEol(txt.Text);
				else if (_control is HotkeyBox hk)
					return hk.GetText();
				else if (_control is NumericUpDown nud)
					return nud.Value;
				else if (_control is Button btn)
					return btn.Text;
				else if (_control is CheckBox cb)
				{
					if (cb.CheckState == CheckState.Checked)
						return 1L;
					else if (cb.CheckState == CheckState.Unchecked)
						return 0L;
					else
						return -1L;
				}
				else if (_control is RadioButton rb)
					return rb.Checked ? 1L : 0L;
				else if (_control is ComboBox cmb)
				{
					if (cmb.DropDownStyle == ComboBoxStyle.DropDown)
					{
						var indexof = cmb.Items.IndexOf(cmb.Text);

						if (indexof == -1)
							return 0L;
					}

					return (long)cmb.SelectedIndex + 1;
				}
				else if (_control is ListBox lb)
				{
					return lb.SelectionMode == SelectionMode.One
						   ? (long)lb.SelectedIndex + 1
						   : (object)new Array(lb.SelectedIndices.Cast<int>().Select(x => x + 1).ToList());
				}
				else if (_control is DateTimePicker dtp)
					return Conversions.ToYYYYMMDDHH24MISS(dtp.Value);
				else if (_control is MonthCalendar mc)
					return mc.MaxSelectionCount == 1 ? $"{mc.SelectionStart:yyyyMMdd}" : $"{mc.SelectionStart:yyyyMMdd}-{mc.SelectionEnd:yyyyMMdd}";
				else if (_control is TrackBar tb)
					return tb.Value;
				else if (_control is ProgressBar pb)
					return pb.Value;
				else if (_control is TabControl tc)
					return (long)tc.SelectedIndex + 1;
				else if (_control is StatusStrip ss)
					return ss.Text;//Unsure if this is what's intended.
				else if (_control is KeysharpPictureBox pic)
					return pic.Filename;

				return "";
			}
			set
			{
				var val = value != null ? value.ToString() : "";
				var ival = value.ParseInt().Value;

				if (_control is Label lbl)
					lbl.Text = val;
				else if (_control is TextBox txt)
					txt.Text = Strings.NormalizeEol(val);
				else if (_control is HotkeyBox hk)
					hk.SetText(val);
				else if (_control is NumericUpDown nud)
					nud.Value = value.ParseDecimal().Value;
				else if (_control is CheckBox cb)
				{
					var cbstate = ival;

					if (cbstate == -1)
						cb.CheckState = CheckState.Indeterminate;
					else
						cb.Checked = Options.OnOff(value) ?? false;
				}
				else if (_control is RadioButton rb)
					rb.Checked = Options.OnOff(value) ?? false;
				else if (_control is ComboBox cmb)
					cmb.SelectedIndex = ival - 1;
				else if (_control is ListBox lb)
				{
					if (value is Keysharp.Core.Array ar)
					{
						lb.ClearSelected();

						foreach (var (arval, _) in ar)
							lb.SetSelected(arval.ParseInt().GetValueOrDefault() - 1, true);
					}
					else
						lb.SelectedIndex = ival - 1;
				}
				else if (_control is DateTimePicker dtp)
				{
					if (val?.Length == 0)
						dtp.Checked = false;
					else
						dtp.Value = Conversions.FromYYYYMMDDHH24MISS(val);
				}
				else if (_control is MonthCalendar mc)
				{
					Conversions.ParseRange(val, out var dtlow, out var dthigh);

					if (dtlow == DateTime.MinValue)
						dtlow = dthigh;

					if (dthigh == DateTime.MaxValue)
						dthigh = dtlow;

					mc.SelectionRange = new SelectionRange(dtlow, dthigh);
				}
				else if (_control is TrackBar tb)
					tb.Value = ival;
				else if (_control is ProgressBar pb)
					pb.Value = Math.Clamp(ival, pb.Minimum, pb.Maximum);
				else if (_control is TabControl tc)
					tc.SelectedIndex = ival - 1;
				else if (_control is StatusStrip ss)
					ss.Text = val;
				else if (_control is KeysharpPictureBox pic)
				{
					var splits = val.Split('*', StringSplitOptions.RemoveEmptyEntries);
					var width = int.MinValue;
					var height = int.MinValue;
					var icon = int.MinValue;
					var filename = splits.Last();

					for (var i = 0; i < splits.Length - 1; i++)
					{
						var opt = splits[i];

						if (Options.TryParse(opt, "w", ref width)) { }
						else if (Options.TryParse(opt, "h", ref height)) { }
						else if (Options.TryParse(opt, "icon", ref icon)) { if (icon > 0) --icon; }
					}

					if (pic.SizeMode == PictureBoxSizeMode.Zoom)
					{
						if (ImageHelper.LoadImage(filename, width, height, icon) is Bitmap bmp)
						{
							var ratio = bmp.Height != 0 ? (double)bmp.Width / bmp.Height : 1;

							if (ratio == 0)
								ratio = 1;

							if (width > 0)
								pic.Width = width;

							if (height > 0)
								pic.Height = height;

							if (width < 0)
								pic.Width = (int)(pic.Height * ratio);

							if (height < 0)
								pic.Height = (int)(pic.Width / ratio);

							pic.Image = bmp;
						}
					}
					else
						pic.Load(filename);
				}
			}
		}

		public object Visible
		{
			get => _control.Visible;
			set => _control.Visible = Options.OnOff(value) ?? false;
		}

		internal Control Ctrl => _control;

		public GuiControl(Gui gui, Control control, string name)
		{
			Gui = gui;
			typename = name;
			_control = control;
			_control.Tag = this;
			_control.Click += _control_Click;
			_control.DoubleClick += _control_DoubleClick;

			if (_control is StatusStrip ss)
			{
				ss.ItemClicked += Ss_ItemClicked;
			}
			else if (_control is LinkLabel ll)
			{
				ll.LinkClicked += _control_Click;
			}
			else if (_control is ListView lv)
			{
				lv.ColumnClick += Lv_ColumnClick;
				lv.ItemChecked += Lv_ItemChecked;
				lv.BeforeLabelEdit += Lv_BeforeLabelEdit;
				lv.SelectedIndexChanged += Lv_SelectedIndexChanged;
				lv.ItemSelectionChanged += Lv_ItemSelectionChanged;
			}
			else if (_control is TreeView tv)
			{
				tv.AfterCheck += Tv_AfterCheck;
				tv.BeforeLabelEdit += Tv_BeforeLabelEdit;
				tv.AfterExpand += Tv_AfterExpand;
				tv.AfterSelect += Tv_AfterSelect;
				tv.NodeMouseClick += Tv_NodeMouseClick;
			}
			else if (_control is TrackBar tb)
			{
				tb.MouseCaptureChanged += Tb_MouseCaptureChanged;
				tb.ValueChanged += Tb_ValueChanged;
			}
			else if (_control is TabControl tc)
			{
				tc.Selected += Tc_Selected;
			}
			else if (_control is NumericUpDown nud)
			{
				nud.ValueChanged += Nud_ValueChanged;
			}
			else if (_control is HotkeyBox hkb)
			{
				hkb.TextChanged += Hkb_TextChanged;
			}
			else if (_control is MonthCalendar mc)
			{
				mc.DateChanged += Mc_DateChanged;
			}
			else if (_control is DateTimePicker dtp)
			{
				dtp.ValueChanged += Dtp_ValueChanged;
			}
			else if (_control is TextBox txt)
			{
				txt.TextChanged += Txt_TextChanged;
			}
			else if (_control is ListBox lb)
			{
				lb.SelectedIndexChanged += Lb_SelectedIndexChanged;
			}
			else if (_control is ComboBox cmb)
			{
				cmb.SelectedIndexChanged += Cmb_SelectedIndexChanged;
			}

			_control.GotFocus += _control_GotFocus;
			_control.LostFocus += _control_LostFocus;
			_control.KeyDown += _control_KeyDown;
			_control.MouseDown += _control_MouseDown;
		}

		public object Add(params object[] obj)
		{
			if (_control is KeysharpTreeView tv)
			{
				var (name, parent, options) = obj.Sls();
				var first = false;
				var n = int.MinValue;
				TreeNode node;
				TreeNodeCollection nodes = null;
				parenthandle = tv.Handle.ToInt64();//By forcing a read of the parent handle, it causes the controls and their handles to properly be created.

				if (parent == 0)
				{
					nodes = tv.Nodes;
				}
				else
				{
					var top = GuiHelper.TV_FindNode(tv, parent);
					nodes = top == null ? tv.Nodes : top.Nodes;
				}

				foreach (var opt in Options.ParseOptions(options))
				{
					if (int.TryParse(opt, out var index)) { n = index; }
					else if (string.Compare(opt, "First", true) == 0) { first = true; }
				}

				if (first)
					node = nodes.Insert(0, name);
				else if (n != int.MinValue && n < nodes.Count)
					node = nodes.Insert(n, name);
				else
					node = nodes.Add(name);

				tv.DelayedExpandParent(node);
				var id = node.Handle.ToInt64();
				node.Name = id.ToString();
				return GuiHelper.TV_NodeOptions(node, parent, options, false);
			}
			else if (_control is ListView lv)
			{
				var lvo = obj.Length > 0 && obj[0] is string options ? GuiHelper.ParseListViewOptions(options) : new GuiHelper.ListViewOptions();
				var strs = obj.Cast<object>().Skip(1).Flatten().Cast<object>().Select(x => x.Str()).ToList();
				GuiHelper.AddOrInsertListViewItem(lv, lvo, strs, int.MinValue);
			}
			else if (_control is ListBox lb)//Using AddRange() relieves the caller of having to set -Redraw first.
				lb.Items.AddRange(obj.Cast<object>().Flatten().Cast<object>().Select(x => x.Str()).ToArray());
			else if (_control is ComboBox cb)
				cb.Items.AddRange(obj.Cast<object>().Flatten().Cast<object>().Select(x => x.Str()).ToArray());
			else if (_control is TabControl tc)
				tc.TabPages.AddRange(obj.Cast<object>().Flatten().Cast<object>().Select(x => new TabPage(x.Str())).ToArray());

			return null;
		}

		public void Choose(object obj)
		{
			var s = obj as string;
			var i = (int)obj.Al() - 1;
			fireEvents = false;

			if (_control is TabControl tc)
			{
				if (!string.IsNullOrEmpty(s))
				{
					if (tc.FindTab(s, false) is TabPage tp)
						tc.SelectTab(tp);
				}
				else if (i >= 0)
					tc.SelectTab(i);
			}
			else if (_control is ListBox lb)
			{
				if (!string.IsNullOrEmpty(s))
					lb.SelectItem(s);
				else if (i >= 0)
					lb.SetSelected(i, true);
				else
					lb.ClearSelected();
			}
			else if (_control is ComboBox cb)
			{
				if (!string.IsNullOrEmpty(s))
					cb.SelectItem(s);
				else if (i >= 0)
					cb.SelectedIndex = i;
				else if (cb.DropDownStyle != ComboBoxStyle.DropDownList)
				{
					cb.SelectedIndex = -1;
					cb.ResetText();
				}
			}

			fireEvents = true;
		}

		/// <summary>
		/// Deletes the specified entry or all entries of a ListBox, DropDownList, ComboBox, or Tab control.
		/// Note this differs from AHK in that deleting a tab fully removes the tab, and does not associate the controls
		/// of an existing tab to the tab at the index that was deleted.
		/// </summary>
		/// <param name="obj"></param>
		public long Delete(object obj)
		{
			var index = (int)obj.Al() - 1;

			switch (_control)
			{
				case ListBox lb:
					if (index >= 0)
						lb.Items.RemoveAt(index);
					else
						lb.Items.Clear();

					break;

				case ComboBox cb:
					if (index >= 0)
						cb.Items.RemoveAt(index);
					else
						cb.Items.Clear();

					break;

				case TabControl tc:
				{
					if (index < 0)
						tc.TabPages.Clear();
					else if (index < tc.TabPages.Count)
					{
						var ctrls = tc.TabPages[index].Controls;
						tc.TabPages.RemoveAt(index);

						if (index < tc.TabPages.Count)//Extremely bizarre behavior, but the documentation says that if you delete a tab, then its controls are moved to the next tab, replacing whatever was on that tab.
						{
							tc.TabPages[index].Controls.Clear();
							tc.TabPages[index].Controls.AddRange(ctrls.Cast<Control>().ToArray());
						}
					}

					break;
				}

				case TreeView tv:
				{
					var id = obj.Al(long.MinValue);

					if (id == long.MinValue)
					{
						tv.Nodes.Clear();
						return 1L;
					}
					else if (GuiHelper.TV_FindNode(tv, id) is TreeNode node)
					{
						node.Remove();
						return 1L;
					}

					break;
				}

				case ListView lv:
					if (index < 0)
					{
						lv.Items.Clear();
						return 1L;
					}
					else if (index < lv.Items.Count)
					{
						lv.Items.RemoveAt(index);
						return 1L;
					}

					break;
			}

			return 0L;
		}

		public long DeleteCol(object obj)
		{
			if (_control is ListView lv)
			{
				var index = (int)obj.Al() - 1;

				if (index >= 0 && index < lv.Columns.Count)
				{
					lv.Columns.RemoveAt(index);
					return 1L;
				}
			}

			return 0L;
		}

		public void Focus() => _control.Focus();

		public long Get(object obj0, object obj1)
		{
			if (_control is TreeView tv)
			{
				var id = obj0.Al();
				var attr = obj1.As();

				if (GuiHelper.TV_FindNode(tv, id) is TreeNode node)
				{
					if (Options.OptionContains(attr, Core.Keyword_Expand, Core.Keyword_Expanded, Core.Keyword_Expand[0].ToString()) && node.IsExpanded)
						return node.Handle.ToInt64();
					else if (Options.OptionContains(attr, Core.Keyword_Check, Core.Keyword_Checked, Core.Keyword_Checked[0].ToString()) && node.Checked)
						return node.Handle.ToInt64();
					else if (Options.OptionContains(attr, Core.Keyword_Bold, Core.Keyword_Bold[0].ToString()) && node.NodeFont.Bold)
						return node.Handle.ToInt64();
				}
			}

			return 0L;
		}

		public long GetChild(object obj)
		{
			if (_control is TreeView tv)
			{
				var id = obj.Al();
				var node = GuiHelper.TV_FindNode(tv, id);
				return node == null ? 0 : node.Nodes.Count == 0 ? 0L : node.FirstNode.Handle.ToInt64();
			}

			return 0L;
		}

		public long GetCount(object obj)
		{
			if (_control is ListView lv)
			{
				var mode = obj.As();

				if (mode?.Length == 0)
					return lv.Items.Count;
				else if (mode.StartsWith("s", System.StringComparison.OrdinalIgnoreCase))
					return lv.SelectedItems.Count;
				else if (mode.StartsWith("c", System.StringComparison.OrdinalIgnoreCase))
					return lv.Columns.Count;
			}
			else if (_control is TreeView tv)
				return tv.Nodes.Count;

			return 0L;
		}

		public long GetNext(object obj0 = null, object obj1 = null)
		{
			var id = obj0.Al();
			var mode = obj1.As();

			if (_control is TreeView tv)
			{
				none:

				if (string.IsNullOrEmpty(mode))
				{
					if (id == 0)
						return tv.Nodes.Count == 0 ? 0L : tv.Nodes[0].Handle.ToInt64();

					var node = GuiHelper.TV_FindNode(tv, id);
					return node == null || node.NextNode == null ? 0L : node.NextNode.Handle.ToInt64();
				}

				var check = Options.OptionContains(mode, Core.Keyword_Check, Core.Keyword_Checked, Core.Keyword_Checked[0].ToString());
				var full = check || Options.OptionContains(mode, Core.Keyword_Full, Core.Keyword_Full[0].ToString());

				if (!full)
				{
					mode = null;
					goto none;
				}

				for (var i = id == 0 ? 1 : GuiHelper.TV_FindNode(tv, id).Index + 1; i < tv.Nodes.Count; i++)//Add one because it's supposed to look for the "next" node.
				{
					if (check && !tv.Nodes[i].Checked)
						continue;

					return tv.Nodes[i].Handle.ToInt64();
				}
			}
			else if (_control is ListView lv)
			{
				var startrow = (int)(id <= 1 ? 0 : id);//Do not subtract 1 here, because the documentation says to start at the next row, if not zero.
				var c = mode.StartsWith("c", System.StringComparison.OrdinalIgnoreCase);
				var f = mode.StartsWith("f", System.StringComparison.OrdinalIgnoreCase);

				if (c)
				{
					for (var i = startrow; i < lv.Items.Count; i++)
						if (lv.Items[i].Checked)
							return i + 1;
				}
				else if (f)
				{
					for (var i = startrow; i < lv.Items.Count; i++)
						if (lv.Items[i].Focused)
							return i + 1;
				}
				else
				{
					for (var i = startrow; i < lv.Items.Count; i++)
						if (lv.Items[i].Selected)
							return i + 1;
				}
			}

			return 0L;
		}

		public object GetNode(object obj)
		{
			if (_control is TreeView tv)
			{
				var id = obj.Al();
				return GuiHelper.TV_FindNode(tv, id);
			}

			return null;
		}

		public long GetParent(object obj)
		{
			if (_control is TreeView tv)
			{
				var id = obj.Al();
				var node = GuiHelper.TV_FindNode(tv, id);
				return node == null || node.Parent == null || !(node.Parent is TreeNode) ? 0L : node.Parent.Handle.ToInt64();
			}

			return 0L;
		}

		public Map GetPos() => GetPos(_control, dpiscaling);

		public long GetPrev(object obj)
		{
			if (_control is TreeView tv)
			{
				var id = obj.Al();
				var node = GuiHelper.TV_FindNode(tv, id);
				return node == null || node.PrevNode == null ? 0L : node.PrevNode.Handle.ToInt64();
			}

			return 0L;
		}

		public long GetSelection() => _control is TreeView tv&& tv.SelectedNode != null ? tv.SelectedNode.Handle.ToInt64() : 0L;

		public string GetText(object obj0, object obj1 = null)
		{
			if (_control is TreeView tv)
			{
				var id = obj0.Al();
				var node = GuiHelper.TV_FindNode(tv, id);

				if (node != null)
					return node.Text;
			}
			else if (_control is ListView lv)
			{
				var row = (int)obj0.Al();
				var col = (int)obj1.Al(1);
				row--;
				col = Math.Max(col - 1, 0);

				if (row < 0 && col < lv.Columns.Count)
					return lv.Columns[col].Text;
				else if (row < lv.Items.Count && col < lv.Items[row].SubItems.Count)
					return lv.Items[row].SubItems[col].Text;
			}

			return "";
		}

		public void Insert(params object[] obj)
		{
			if (_control is ListView lv)//Note that this index might not actually be where the row is shown, due to sorting.
			{
				var o = obj.L();
				var (rownumber, opts, cols) = o.Is2();
				var lvo = opts is string options ? GuiHelper.ParseListViewOptions(options) : new GuiHelper.ListViewOptions();
				var strs = o.Count > 2 ? o.Cast<object>().Skip(2).Flatten().Cast<object>().Select(x => x.Str()).ToList() : new List<string>();
				GuiHelper.AddOrInsertListViewItem(lv, lvo, strs, rownumber - 1);
			}
		}

		public long InsertCol(object obj0, object obj1 = null, object obj2 = null)
		{
			if (_control is ListView lv)
			{
				var index = (int)obj0.Al();
				var options = obj1.As();
				var title = obj2.As();
				index--;
				var header = new ColumnHeader
				{
					Text = title
				};

				if (index < lv.Columns.Count)
				{
					lv.Columns.Insert(index, header);

					if (index == 0)
					{
						for (var i = 0; i < lv.Items.Count; i++)
						{
							_ = lv.Items[i].SubItems.Add("");

							for (var j = lv.Items[i].SubItems.Count - 1; j > 1; j--)
								lv.Items[i].SubItems[j].Text = lv.Items[i].SubItems[j - 1].Text;

							lv.Items[i].SubItems[1].Text = "";
						}
					}
					else
					{
						for (var i = 0; i < lv.Items.Count; i++)
						{
							_ = lv.Items[i].SubItems.Add("");

							for (var j = lv.Items[i].SubItems.Count - 1; j > index; j--)
								lv.Items[i].SubItems[j].Text = lv.Items[i].SubItems[j - 1].Text;

							lv.Items[i].SubItems[index].Text = "";
						}
					}
				}
				else
					_ = lv.Columns.Add(header);

				GuiHelper.ParseAndApplyListViewColumnOptions(header, options);
				return index + 1L;
			}

			return -1L;
		}

		public long Modify(params object[] obj)
		{
			var o = obj.L();

			if (_control is TreeView tv)
			{
				var (id, options, name) = o.Ls2();

				if (GuiHelper.TV_FindNode(tv, id) is TreeNode node)
				{
					if (options?.Length == 0 && name?.Length == 0)
					{
						node.TreeView.SelectedNode = node;
						return node.Handle.ToInt64();
					}
					else if (name != "")
						node.Text = name;

					return GuiHelper.TV_NodeOptions(node, node.Parent != null ? node.Parent.Handle.ToInt64() : 0L, options, true);
				}
			}
			else if (_control is ListView lv)
			{
				try
				{
					var (rownumber, opts, cols) = o.Is2();

					if (rownumber < lv.Items.Count)
					{
						var lvo = opts is string options ? GuiHelper.ParseListViewOptions(options) : new GuiHelper.ListViewOptions();
						var strs = o.Count > 2 ? o.Cast<object>().Skip(2).Flatten().Cast<object>().Select(x => x.Str()).ToList() : new List<string>();
						var start = Math.Max(0, rownumber - 1);
						var end = rownumber == 0 ? lv.Items.Count : Math.Min(rownumber, lv.Items.Count);

						for (rownumber = start; rownumber < end; rownumber++)
						{
							var item = lv.Items[rownumber];

							for (int i = 0, j = lvo.colstart; i < strs.Count && j < item.SubItems.Count; i++, j++)
								item.SubItems[j].Text = strs[i];

							GuiHelper.ApplyListViewOptions(lv, item, lvo);
						}

						return 1L;
					}
				}
				catch (Exception)// ex)
				{
				}
			}

			return 0L;
		}

		public long ModifyCol(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			if (_control is ListView lv)
			{
				var colnumber = (int)obj0.Al();
				var opts = obj1.As();
				var coltitle = obj2.As();

				if (opts?.Length == 0 && coltitle?.Length == 0)
				{
					if (colnumber == 0)
					{
						lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
						return 1L;
					}
					else if (colnumber > 0)
					{
						lv.AutoResizeColumn(colnumber - 1, ColumnHeaderAutoResizeStyle.HeaderSize);
						return 1L;
					}
				}

				colnumber--;

				if (colnumber < lv.Columns.Count)
				{
					var col = lv.Columns[colnumber];

					if (coltitle != "")
						col.Text = coltitle;

					GuiHelper.ParseAndApplyListViewColumnOptions(col, opts);
					return 1L;
				}
			}

			return 0L;
		}

		public void Move(object obj0 = null, object obj1 = null, object obj2 = null, object obj3 = null)
		{
			var x = obj0.Al(long.MinValue);
			var y = obj1.Al(long.MinValue);
			var width = obj2.Al(long.MinValue);
			var height = obj3.Al(long.MinValue);
			var scale = dpiscaling ? 1.0 : Accessors.A_ScaledScreenDPI;

			if (y != long.MinValue)
				_control.Top = (int)Math.Round(y * scale);

			if (x != long.MinValue)
				_control.Left = (int)Math.Round(x * scale);

			if (width != long.MinValue)
				_control.Width = (int)Math.Round(width * scale);

			if (height != long.MinValue)
				_control.Height = (int)Math.Round(height * scale);
		}

		public void OnCommand(object obj0, object obj1, object obj2 = null) => HandleOnCommandNotify(obj0.Al(), obj1, obj2.Al(1L), ref commandHandlers);

		public void OnEvent(object obj0, object obj1, object obj2 = null)
		{
			var e = obj0.As().ToLower();
			var h = obj1;
			var i = obj2.Al(1);

			if (e == "focus")
				e = "enter";
			else if (e == "losefocus")
				e = "leave";

			var del = GetFuncObj(h, Gui.form.eventObj);

			if (del != null)
			{
				if (e == "change")
				{
					if (changeHandlers == null)
						changeHandlers = new List<IFuncObj>();

					changeHandlers.ModifyEventHandlers(del, i);
				}
				else if (e == "click")
				{
					if (_control is KeysharpLinkLabel ll && !ll.clickSet)
					{
						ll.clickSet = true;
						clickHandlers.Clear();
					}

					clickHandlers.ModifyEventHandlers(del, i);
				}
				else if (e == "doubleclick")
				{
					if (_control is ToolStrip ts)
					{
						foreach (var item in ts.Items)
							if (item is KeysharpToolStripStatusLabel tssl)
								tssl.doubleClickHandlers.ModifyEventHandlers(del, i);
					}
					else
						doubleClickHandlers.ModifyEventHandlers(del, i);
				}
				else if (e == "focus")
				{
					if (focusHandlers == null)
						focusHandlers = new List<IFuncObj>();

					focusHandlers.ModifyEventHandlers(del, i);
				}
				else if (e == "losefocus")
				{
					if (lostFocusHandlers == null)
						lostFocusHandlers = new List<IFuncObj>();

					lostFocusHandlers.ModifyEventHandlers(del, i);
				}
				else if (e == "colclick")
				{
					if (_control is ListView lv)
					{
						if (columnClickHandlers == null)
							columnClickHandlers = new List<IFuncObj>();

						columnClickHandlers.ModifyEventHandlers(del, i);
					}
				}
				else if (e == "itemcheck")
				{
					if (_control is TreeView || _control is ListView)
					{
						if (itemCheckHandlers == null)
							itemCheckHandlers = new List<IFuncObj>();

						itemCheckHandlers.ModifyEventHandlers(del, i);
					}
				}
				else if (e == "itemedit")
				{
					if (_control is TreeView || _control is ListView)
					{
						if (itemEditHandlers == null)
							itemEditHandlers = new List<IFuncObj>();

						itemEditHandlers.ModifyEventHandlers(del, i);
					}
				}
				else if (e == "itemexpand")
				{
					if (_control is TreeView)
					{
						if (itemExpandHandlers == null)
							itemExpandHandlers = new List<IFuncObj>();

						itemExpandHandlers.ModifyEventHandlers(del, i);
					}
				}
				else if (e == "itemfocus")
				{
					if (_control is ListView)
					{
						if (focusedItemChangedHandlers == null)
							focusedItemChangedHandlers = new List<IFuncObj>();

						focusedItemChangedHandlers.ModifyEventHandlers(del, i);
					}
				}
				else if (e == "itemselect")
				{
					if (_control is TreeView || _control is ListView)
					{
						if (selectedItemChangedHandlers == null)
							selectedItemChangedHandlers = new List<IFuncObj>();

						selectedItemChangedHandlers.ModifyEventHandlers(del, i);
					}
				}
				else if (e == "contextmenu")
				{
					if (contextMenuChangedHandlers == null)
						contextMenuChangedHandlers = new List<IFuncObj>();

					if (!(_control is TextBox) && !(_control is MonthCalendar))
						contextMenuChangedHandlers.ModifyEventHandlers(del, i);
				}
			}
		}

		public void OnNotify(object obj0, object obj1, object obj2 = null) => HandleOnCommandNotify(obj0.Al(), obj1, obj2.Al(1L), ref notifyHandlers);

		public void Opt(object obj)
		{
			var optionsstr = obj.As();
			var opts = Gui.ParseOpt(typename, _control.Text, optionsstr);

			if (opts.redraw.HasValue)
			{
				if (opts.redraw == true)
					_control.SuspendDrawing();
				else
					_control.ResumeDrawing();
			}

			if (opts.c != _control.ForeColor && opts.c != Control.DefaultForeColor)
			{
				if (_control is DateTimePicker dtp)
					dtp.CalendarForeColor = opts.c;
				else if (_control is MonthCalendar mc)
					mc.TitleForeColor = opts.c;

				_control.ForeColor = opts.c;
			}

			if (_control is Button)
			{
				if (opts.btndef.HasValue)
					Gui.form.AcceptButton = opts.btndef == true ? (IButtonControl)_control : null;
			}
			else if (_control is ListBox lb)
			{
				if (opts.vscroll.HasValue)
					lb.ScrollAlwaysVisible = opts.vscroll.Value;

				if (opts.hscrollamt != int.MinValue)
					lb.HorizontalScrollbar = true;

				if (opts.hscrollamt > 0)
					lb.HorizontalExtent = opts.hscrollamt;

				if (opts.sort.HasValue)
					lb.Sorted = opts.sort.Value;
			}
			else if (_control is ComboBox cb)
			{
				if (opts.sort.IsTrue())
				{
					cb.Sorted = true;

					if (cb.DropDownStyle != ComboBoxStyle.DropDownList)
					{
						cb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
						cb.AutoCompleteSource = AutoCompleteSource.ListItems;
					}
				}
				else if (opts.sort.IsFalse())
				{
					cb.Sorted = false;

					if (cb.DropDownStyle != ComboBoxStyle.DropDownList)
					{
						cb.AutoCompleteMode = AutoCompleteMode.None;
						cb.AutoCompleteSource = AutoCompleteSource.None;
					}
				}

				if (typename != Core.Keyword_DropDownList && opts.cmbsimple.HasValue)
				{
					cb.DropDownStyle = opts.cmbsimple.IsTrue() ? ComboBoxStyle.Simple : ComboBoxStyle.DropDown;
				}
			}
			else if (_control is TextBox txt)
			{
				txt.AcceptsTab = opts.wanttab ?? false;
				txt.AcceptsReturn = opts.wantreturn ?? false;

				if (opts.wantctrla.IsFalse())
				{
					txt.PreviewKeyDown += Gui.SuppressCtrlAPreviewKeyDown;
					txt.KeyDown += Gui.SuppressCtrlAKeyDown;
				}
				else if (opts.wantctrla.IsTrue())
				{
					txt.PreviewKeyDown -= Gui.SuppressCtrlAPreviewKeyDown;
					txt.KeyDown -= Gui.SuppressCtrlAKeyDown;
				}

				if (opts.vscroll.IsTrue() && opts.hscrollamt != int.MinValue)
					txt.ScrollBars = ScrollBars.Both;
				else if (opts.vscroll.IsTrue() || txt.Multiline)
					txt.ScrollBars = ScrollBars.Vertical;
				else if (opts.hscrollamt != int.MinValue)
					txt.ScrollBars = ScrollBars.Horizontal;

				if (opts.limit != int.MinValue)
					txt.MaxLength = opts.limit;

				if (opts.lowercase.IsTrue())
					txt.CharacterCasing = CharacterCasing.Lower;
				else if (opts.uppercase.IsTrue())
					txt.CharacterCasing = CharacterCasing.Upper;
				else
					txt.CharacterCasing = CharacterCasing.Normal;
			}
			else if (_control is KeysharpTrackBar tb)
			{
				if (opts.center.IsTrue())
					tb.TickStyle = TickStyle.Both;
				else if (opts.leftj.IsTrue())
					tb.TickStyle = TickStyle.TopLeft;
				else if (opts.noticks.IsTrue())
					tb.TickStyle = TickStyle.None;

				if (opts.invert.HasValue)
					tb.inverted = opts.invert.Value;

				if (opts.tickinterval != int.MinValue)
					tb.TickFrequency = opts.tickinterval;

				if (opts.line != int.MinValue)
					tb.SmallChange = opts.line;

				if (opts.page != int.MinValue)
					tb.LargeChange = opts.page;

				if (opts.thick != int.MinValue)
					_ = WindowsAPI.SendMessage(tb.Handle, WindowsAPI.TBM_SETTHUMBLENGTH, (uint)opts.thick, 0);

				if (opts.tooltip)
					_ = WindowsAPI.SendMessage(tb.Handle, WindowsAPI.TBM_SETTIPSIDE, (uint)opts.tooltipside, 0);
			}
			else if (_control is TreeView tv)
			{
				if (opts.buttons.HasValue)
					tv.ShowPlusMinus = opts.buttons.Value;

				if (opts.rdonly.HasValue)
					tv.LabelEdit = !opts.rdonly.Value;

				if (opts.lines.HasValue)
					tv.ShowLines = opts.lines.Value;

				if (tv.LabelEdit && opts.wantf2.HasValue)
					if (opts.wantf2.IsTrue())
						tv.KeyDown += Gui.Tv_Lv_KeyDown;
					else
						tv.KeyDown -= Gui.Tv_Lv_KeyDown;
			}
			else if (_control is ListView lv)
			{
				if (opts.ischecked.HasValue)
					lv.CheckBoxes = opts.ischecked.Value > 0;

				if (opts.rdonly.HasValue)
					lv.LabelEdit = !opts.rdonly.Value;

				if (opts.grid.HasValue)
					lv.GridLines = opts.grid.IsTrue();

				if (opts.multiline.HasValue)
					lv.MultiSelect = opts.multiline.Value;

				if (lv.LabelEdit && opts.wantf2.HasValue)
					if (opts.wantf2.IsTrue())
						lv.KeyDown += Gui.Tv_Lv_KeyDown;
					else
						lv.KeyDown -= Gui.Tv_Lv_KeyDown;

				if (opts.lvview.HasValue)
					lv.View = opts.lvview.Value;

				if ((opts.addlvstyle & 0x10) == 0x10)
					lv.AllowColumnReorder = true;
				else if ((opts.remlvstyle & 0x10) == 0x10)
					lv.AllowColumnReorder = false;

				if ((opts.addlvstyle & 0x20) == 0x20)
					lv.FullRowSelect = true;
				else if ((opts.remlvstyle & 0x20) == 0x20)
					lv.FullRowSelect = false;

				if (opts.sort.IsTrue())
					lv.Sorting = SortOrder.Ascending;
				else if (opts.sortdesc.IsTrue())
					lv.Sorting = SortOrder.Descending;
				else if (opts.sort.IsFalse() || opts.sortdesc.IsFalse())//If either were reset, just set to none.
					lv.Sorting = SortOrder.None;

				if (opts.header.HasValue)
					lv.HeaderStyle = opts.header.IsFalse() ? ColumnHeaderStyle.None : ColumnHeaderStyle.Clickable;
				else if (opts.clickheader.HasValue)
					lv.HeaderStyle = opts.clickheader.IsFalse() ? ColumnHeaderStyle.Nonclickable : ColumnHeaderStyle.Clickable;
				else if (opts.sortheader.HasValue)
					lv.HeaderStyle = opts.sortheader.IsFalse() ? ColumnHeaderStyle.Nonclickable : ColumnHeaderStyle.Clickable;
			}
			else if (_control is ProgressBar pb)
			{
				if (opts.smooth.HasValue)
					pb.Style = opts.smooth.IsTrue() ? ProgressBarStyle.Continuous : ProgressBarStyle.Blocks;
			}
			else if (_control is TabControl tc)
			{
				if (opts.buttons.HasValue)
					tc.Appearance = opts.buttons.Value ? TabAppearance.FlatButtons : TabAppearance.Normal;

				if (opts.wordwrap.HasValue)
					tc.Multiline = opts.wordwrap.IsTrue();

				if (opts.leftj.IsTrue())
					tc.Alignment = System.Windows.Forms.TabAlignment.Left;
				else if (opts.rightj.IsTrue())
					tc.Alignment = System.Windows.Forms.TabAlignment.Right;
				else if (opts.bottom)
					tc.Alignment = System.Windows.Forms.TabAlignment.Bottom;
				else if (opts.top)
					tc.Alignment = System.Windows.Forms.TabAlignment.Top;
			}
			else if (_control is NumericUpDown nud)
			{
				if (opts.leftj.HasValue)
					nud.UpDownAlign = opts.leftj.Value ? LeftRightAlignment.Left : LeftRightAlignment.Right;

				if (opts.nudinc.HasValue)
					nud.Increment = opts.nudinc.Value;

				if (opts.hex.HasValue)
					nud.Hexadecimal = opts.hex.Value;

				if (opts.nudlow.HasValue)
					nud.Minimum = opts.nudlow.Value;

				if (opts.nudhigh.HasValue)
					nud.Maximum = opts.nudhigh.Value;

				nud.ThousandsSeparator = (opts.addstyle & 0x80) != 0x80;
			}

			if (opts.center.IsTrue())
				Reflections.SafeSetProperty(_control, "TextAlign", ContentAlignment.MiddleCenter);
			else if (opts.leftj.IsTrue())
				Reflections.SafeSetProperty(_control, "TextAlign", ContentAlignment.MiddleLeft);
			else if (opts.rightj.IsTrue())
				Reflections.SafeSetProperty(_control, "TextAlign", ContentAlignment.MiddleRight);

			if (opts.bgtrans)
				_control.BackColor = Color.Transparent;
			else if (opts.bgcolor.HasValue)
				_control.BackColor = opts.bgcolor.Value;

			if (opts.altsubmit.HasValue)
				AltSubmit = opts.altsubmit.Value;

			if (opts.visible.HasValue)
				_control.Visible = opts.visible.Value;

			if (opts.enabled.HasValue)
				_control.Enabled = opts.enabled.Value;

			if (opts.tabstop.HasValue)
				_control.TabStop = opts.tabstop.Value;

			if (opts.wordwrap.HasValue)
				Reflections.SafeSetProperty(_control, "WordWrap", opts.wordwrap.Value);

			if (opts.thinborder.HasValue)
				Reflections.SafeSetProperty(_control, "BorderStyle", opts.thinborder.Value ? BorderStyle.FixedSingle : BorderStyle.None);
		}

		public void Redraw() => _control.Refresh();

		public void SetFont(object obj0 = null, object obj1 = null) => _control.SetFont(obj0, obj1);

		public void SetFormat(object obj) => (_control as DateTimePicker)?.SetFormat(obj);

		public IntPtr SetIcon(object obj0, object obj1 = null, object obj2 = null)
		{
			if (_control is StatusStrip ss)
			{
				var filename = obj0.As();
				var iconnumber = (int)obj1.Al(1L);
				var part = (int)obj2.Al(1L);
				part--;

				if (iconnumber > 0)
					iconnumber--;

				if (part < ss.Items.Count && ImageHelper.LoadImage(filename, 0, 0, iconnumber) is Bitmap bmp)
				{
					ss.Items[part].Image = bmp;
					return bmp.GetHicon();
				}
			}

			return IntPtr.Zero;
		}

		public long SetImageList(object obj0, object obj1 = null)
		{
			var id = obj0.Al();
			var type = obj1.Al();//Type is for something called "state icons", which the documentation says are not supported yet, so we ignore for now.
			var oldil = 0L;

			if (ImageLists.IL_Get(id) is ImageList il)
			{
				if (_control is TreeView tv)
				{
					oldil = ImageLists.IL_GetId(tv.ImageList);
					tv.ImageList = il;
				}
				else if (_control is ListView lv)
				{
					switch (type)
					{
						case 1:
						{
							var newil = ImageLists.IL_Get(id);

							if (newil != null)
							{
								oldil = ImageLists.IL_GetId(lv.SmallImageList);
								lv.SmallImageList = newil;
							}

							break;
						}

						case 2://Documentation says state icons don't work, but they do here.//NEW
						{
							var newil = ImageLists.IL_Get(id);

							if (newil != null)
							{
								oldil = ImageLists.IL_GetId(lv.StateImageList);
								lv.StateImageList = newil;
							}

							break;
						}

						case 0://Documentation says default is deduced, but unsure how, so default to large.//MATT
						default:
						{
							var newil = ImageLists.IL_Get(id);

							if (newil != null)
							{
								oldil = ImageLists.IL_GetId(lv.LargeImageList);
								lv.LargeImageList = newil;
							}

							break;
						}
					}
				}
				else if (_control is TabControl tc)
				{
					oldil = ImageLists.IL_GetId(tc.ImageList);
					tc.ImageList = il;
				}
			}

			return oldil;
		}

		public void SetParts(params object[] obj)
		{
			if (_control is StatusStrip ss)
			{
				var parts = obj.L();
				KeysharpToolStripStatusLabel tssl = null;
				ss.Items.Clear();

				for (var i = 0; i < parts.Count - 1; i++)
				{
					var part = parts[i];

					if (part != null)
					{
						var width = part.ParseInt().Value;
						tssl = new KeysharpToolStripStatusLabel
						{
							AutoSize = false,
							Name = $"tss{width}_{i + 1}",
							Width = width,
							Alignment = ToolStripItemAlignment.Left
						};
						_ = ss.Items.Add(tssl);
					}
				}

				tssl = new KeysharpToolStripStatusLabel
				{
					Name = "tssspring_last",
					Alignment = ToolStripItemAlignment.Left,
					Spring = true
				};
				_ = ss.Items.Add(tssl);
			}
		}

		public void SetTabIcon(object obj0, object obj1)//New function since the original required SendMessage() to do this.
		{
			if (_control is TabControl tc)
			{
				var tabindex = (int)obj0.Al();
				var imageindex = (int)obj1.Al();

				if (tabindex < tc.TabCount && tc.ImageList != null && imageindex < tc.ImageList.Images.Count)
					tc.TabPages[tabindex].ImageIndex = imageindex;
				else
					tc.TabPages[tabindex].ImageIndex = -1;
			}
		}

		public bool SetText(object obj0, object obj1 = null, object obj2 = null)
		{
			if (_control is StatusStrip ss)
			{
				var text = obj0.As();
				var part = (int)obj1.Al(1L);
				var style = obj2.Al();
				part--;

				if (part < ss.Items.Count)
				{
					var item = ss.Items[part];
					item.Text = text;//Documentation says tabs get expanded, but that doesn't seem to work here.//MATT

					if (item is ToolStripStatusLabel tssl)
					{
						if (style == 0)
						{
							tssl.BorderStyle = Border3DStyle.Sunken;
							tssl.BorderSides = ToolStripStatusLabelBorderSides.All;
						}
						else if (style == 1)
						{
							tssl.BorderStyle = Border3DStyle.Flat;
							tssl.BorderSides = ToolStripStatusLabelBorderSides.None;
						}
						else if (style == 2)
						{
							tssl.BorderStyle = Border3DStyle.Raised;
							tssl.BorderSides = ToolStripStatusLabelBorderSides.All;
						}
						else
							return false;
					}

					return true;
				}
			}

			return false;
		}

		public void UseTab(object obj0 = null, object obj1 = null)
		{
			if (_control is KeysharpTabControl tc)
			{
				var val = obj0;
				var exact = obj1.Ab();

				if (val is string s)
				{
					if (tc.FindTab(s, exact) is TabPage tp)
						Gui.CurrentTab = tp;
				}
				else if (val != null)
				{
					var i = (int)val.Al();
					i--;

					if (i >= 0 && i < tc.TabPages.Count)
						Gui.CurrentTab = tc.TabPages[i];
				}
				else
					Gui.CurrentTab = null;
			}
		}

		internal static Map GetClientPos(Control control, bool scaling)
		{
			var scale = scaling ? 1.0 : Accessors.A_ScaledScreenDPI;
			var dkt = new Map(new Dictionary<string, object>
			{
				{ "X", control.ClientRectangle.X * scale },
				{ "Y", control.ClientRectangle.Y * scale },
				{ "Width", control.ClientRectangle.Width * scale },
				{ "Height", control.ClientRectangle.Height * scale }
			});
			return dkt;
		}

		internal static IFuncObj GetFuncObj(object h, object eventObj)
		{
			IFuncObj del = null;

			if (h is string s)
				del = new FuncObj(s, eventObj);//If eventObj is not null, then s is assumed to be a method on that object, else s is assumed to be a global function.
			else if (h is IFuncObj fo)
				del = fo;

			return del;
		}

		internal static Map GetPos(Control control, bool scaling)
		{
			var scale = scaling ? 1.0 : Accessors.A_ScaledScreenDPI;
			var dkt = new Map(new Dictionary<string, object>
			{
				{ "X", control.Location.X * scale },
				{ "Y", control.Location.Y * scale },
				{ "Width", control.Size.Width * scale },
				{ "Height", control.Size.Height * scale }
			});
			return dkt;
		}

		internal object InvokeMessageHandlers(ref Message m)
		{
			if (m.Msg == WindowsAPI.WM_NOTIFY || m.Msg == WindowsAPI.WM_REFLECT + WindowsAPI.WM_NOTIFY)
			{
				if (notifyHandlers != null)
				{
					var nmhdr = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));

					if (notifyHandlers.TryGetValue((int)nmhdr.code, out var handler))
					{
						var ret = handler?.InvokeEventHandlers(this, nmhdr);
						m.Result = (IntPtr)(ret.IsCallbackResultNonEmpty() ? 1 : 0);
						return true;
					}
				}
			}
			else if (m.Msg == WindowsAPI.WM_COMMAND)
			{
				if (commandHandlers != null)
				{
					var val = (int)m.WParam;

					if (commandHandlers.TryGetValue(val, out var handler))
					{
						var ret = handler?.InvokeEventHandlers(this);
						m.Result = (IntPtr)(ret.IsCallbackResultNonEmpty() ? 1 : 0);
						return true;
					}
				}
			}

			return null;
		}

		private void _control_Click(object sender, EventArgs e)
		{
			if (_control is TreeView tv)
				_ = clickHandlers.InvokeEventHandlers(this, GetSelection());
			else if (_control is ListView lv)
			{
				if (lv.SelectedIndices.Count > 0)
					_ = clickHandlers.InvokeEventHandlers(this, lv.SelectedIndices[0] + 1L);
				else
					_ = clickHandlers.InvokeEventHandlers(this, 0L);
			}
			else if (_control is KeysharpLinkLabel ll)
			{
				if (e is LinkLabelLinkClickedEventArgs lllce)
				{
					if (!ll.clickSet)
						_ = KeysharpLinkLabel.OnLinkLabelClicked(ll, lllce);
					else if (lllce.Link.LinkData is Tuple<string, string> tss)
						_ = clickHandlers.InvokeEventHandlers(this, tss.Item1 != "" ? tss.Item1 : (object)(ll.Links.IndexOf(lllce.Link) + 1L), tss.Item2);
				}
			}
			else if (_control is KeysharpButton)
			{
				//mousecount ^= 1;//Button click events get fired twice, because we have double click and standard click enabled, so filter the second click here.
				//if (mousecount > 0)
				_ = clickHandlers.InvokeEventHandlers(this, null);
			}
			else
				_ = clickHandlers.InvokeEventHandlers(this, null);
		}

		private void _control_DoubleClick(object sender, EventArgs e)
		{
			if (_control is TreeView tv)
				_ = doubleClickHandlers.InvokeEventHandlers(this, GetSelection());
			else if (_control is ListView lv)
			{
				if (lv.SelectedIndices.Count > 0)
					_ = doubleClickHandlers.InvokeEventHandlers(this, lv.SelectedIndices[0] + 1L);
				else
					_ = doubleClickHandlers.InvokeEventHandlers(this, 0L);
			}
			else if (_control is ListBox lb)
			{
				if (lb.SelectedIndices.Count > 0)
					_ = doubleClickHandlers.InvokeEventHandlers(this, lb.SelectedIndices[0] + 1L);
				else
					_ = doubleClickHandlers.InvokeEventHandlers(this, 0L);
			}
			else
				_ = doubleClickHandlers.InvokeEventHandlers(this, null);

			//Status strip items are handled in a separate special handler contained within each item.
		}

		private void _control_GotFocus(object sender, EventArgs e) => focusHandlers?.InvokeEventHandlers(this, 0L);

		private void _control_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Apps || (e.KeyCode == Keys.F10 && ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)))
				CallContextMenuChangeHandlers(true, System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
		}

		private void _control_LostFocus(object sender, EventArgs e) => lostFocusHandlers?.InvokeEventHandlers(this, 0L);

		private void _control_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				CallContextMenuChangeHandlers(false, e.X, e.Y);
		}

		private void CallContextMenuChangeHandlers(bool wasRightClick, int x, int y)
		{
			if (_control is ListBox lb)
				_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, lb.SelectedIndex + 1L, wasRightClick, x, y));
			else if (_control is ListView lv)
				_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, lv.SelectedIndices.Count > 0 ? lv.SelectedIndices[0] + 1L : 0L, wasRightClick, x, y));
			else if (_control is TreeView tv)
				_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, tv.SelectedNode.Handle, wasRightClick, x, y));
			else
				_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, _control.Handle.ToInt64().ToString(), wasRightClick, x, y));//Unsure what to pass for Item, so just pass handle.
		}

		private void Cmb_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_control is ComboBox)
				_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
		}

		private void Dtp_ValueChanged(object sender, EventArgs e)
		{
			if (_control is DateTimePicker)
				_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
		}

		private void HandleOnCommandNotify(long code, object callback, long addremove, ref Dictionary<int, List<IFuncObj>> handlers)
		{
			var del = GetFuncObj(callback, Gui.form.eventObj);

			if (del != null)
			{
				if (handlers == null)
					handlers = new Dictionary<int, List<IFuncObj>>();

				var h = handlers.GetOrAdd((int)code);
				h.ModifyEventHandlers(del, addremove);
			}
		}

		private void Hkb_TextChanged(object sender, EventArgs e)
		{
			if (_control is HotkeyBox)
				_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
		}

		private void Lb_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_control is ListBox)
				_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
		}

		private void Lv_BeforeLabelEdit(object sender, LabelEditEventArgs e)
		{
			if (_control is ListView lv)
				_ = (itemEditHandlers?.InvokeEventHandlers(this, e.Item + 1L));//The documentation says to pass "item". Not really sure if that means index, or something else.
		}

		private void Lv_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			if (_control is ListView lv)
				_ = (columnClickHandlers?.InvokeEventHandlers(this, e.Column + 1L));
		}

		private void Lv_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			if (_control is ListView)
				_ = (itemCheckHandlers?.InvokeEventHandlers(this, e.Item.Index + 1L, e.Item.Checked ? 1L : 0L));
		}

		private void Lv_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (_control is ListView lv)
				_ = (selectedItemChangedHandlers?.InvokeEventHandlers(this, e.Item.Index + 1L, e.Item.Selected ? 1L : 0L));
		}

		private void Lv_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_control is ListView lv)
				_ = (focusedItemChangedHandlers?.InvokeEventHandlers(this, lv.SelectedIndices.Count > 0 ? lv.SelectedIndices[0] + 1L : 0L));
		}

		private void Mc_DateChanged(object sender, DateRangeEventArgs e)
		{
			if (_control is MonthCalendar)
				_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
		}

		private void Nud_ValueChanged(object sender, EventArgs e)
		{
			if (_control is NumericUpDown)
				_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
		}

		private void Ss_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (_control is StatusStrip ss)
				_ = (clickHandlers?.InvokeEventHandlers(this, ss.Items.IndexOf(e.ClickedItem) + 1L));
		}

		private void Tb_MouseCaptureChanged(object sender, EventArgs e)
		{
			if (_control is TrackBar tb && !AltSubmit)
				_ = (changeHandlers?.InvokeEventHandlers(this, 0L));//Need to figure out how to pass the method by which the slider was changed.//MATT
		}

		private void Tb_ValueChanged(object sender, EventArgs e)
		{
			if (_control is TrackBar tb && AltSubmit)
				_ = (changeHandlers?.InvokeEventHandlers(this, 0L));//Need to figure out how to pass the method by which the slider was changed.//MATT
		}

		private void Tc_Selected(object sender, TabControlEventArgs e)
		{
			if (_control is TabControl)
				_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
		}

		private void Tv_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (_control is TreeView)
				_ = (itemCheckHandlers?.InvokeEventHandlers(this, e.Node.Handle, e.Node.Checked ? 1L : 0L));
		}

		private void Tv_AfterExpand(object sender, TreeViewEventArgs e)
		{
			if (_control is TreeView)
				_ = (itemExpandHandlers?.InvokeEventHandlers(this, e.Node.Handle, e.Node.IsExpanded ? 1L : 0L));
		}

		private void Tv_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (_control is TreeView tv)
				_ = (selectedItemChangedHandlers?.InvokeEventHandlers(this, e.Node.Handle));
		}

		private void Tv_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (_control is TreeView)
				_ = (itemEditHandlers?.InvokeEventHandlers(this, e.Node.Handle));
		}

		private void Tv_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (_control is TreeView tv && e.Node == tv.SelectedNode)
				_ = (selectedItemChangedHandlers?.InvokeEventHandlers(this, e.Node.Handle));
		}

		private void Txt_TextChanged(object sender, EventArgs e)
		{
			if (_control is TextBox)
				_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
		}
	}
}