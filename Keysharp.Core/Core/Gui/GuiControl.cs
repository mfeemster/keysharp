namespace Keysharp.Core
{
	public partial class Gui : KeysharpObject, I__Enum, IEnumerable<(object, object)>
	{
		public class Control : KeysharpObject
		{
			private string typename;
			private WeakReference<Gui> gui;
			private readonly List<IFuncObj> clickHandlers = [];
			private readonly List<IFuncObj> doubleClickHandlers = [];
			private bool DpiScaling => ((Gui)Gui).dpiscaling;
			private System.Windows.Forms.Control _control;

			//Normal event handlers can't be used becaused they need to return a value.
			//The returned values are then inspected to determine if subsequent handlers should be called or not.
			private List<IFuncObj> changeHandlers;
			private List<IFuncObj> columnClickHandlers;
			private Dictionary<int, List<IFuncObj>> commandHandlers;
			private List<IFuncObj> contextMenuChangedHandlers;
			private nint dummyHandle;
			private List<IFuncObj> focusedItemChangedHandlers;
			private List<IFuncObj> focusHandlers;
			private List<IFuncObj> itemCheckHandlers;
			private List<IFuncObj> itemEditHandlers;
			private List<IFuncObj> itemExpandHandlers;
			private List<IFuncObj> lostFocusHandlers;
			private Dictionary<int, List<IFuncObj>> notifyHandlers;
			private long parenthandle;
			private List<IFuncObj> selectedItemChangedHandlers;
			internal Size requestedSize = new (int.MinValue, int.MinValue);
			internal bool eventHandlerActive = true;

			public bool AltSubmit { get; internal set; } = false;

			public string ClassNN => Script.TheScript.WindowProvider.Manager.CreateWindow(_control.Handle) is WindowItemBase wi ? wi.ClassNN : "";

			public System.Windows.Forms.Control Ctrl => _control;

			public object Enabled
			{
				get => _control.Enabled;
				set => _control.Enabled = Options.OnOff(value) ?? false;
			}

			public object Focused => _control.Focused;

			public object Gui => gui.TryGetTarget(out var g) ? g : DefaultErrorObject;

			public long Hwnd => _control.Handle.ToInt64();

			public object Name
			{
				get => _control.Name;
				set => _control.Name = value.ToString();
			}

			public string NetClassNN => Script.TheScript.WindowProvider.Manager.CreateWindow(_control.Handle) is WindowItemBase wi ? wi.NetClassNN : "";

			public object Parent
			{
				get => _control.Parent;

				set
				{
					if (value is Gui.Control gc)
						_control.Parent = gc._control;
					else if (value is System.Windows.Forms.Control c)
						_control.Parent = c;
				}
			}

			public KeysharpForm ParentForm => _control.FindParent<KeysharpForm>();

			public object RichText
			{
				get
				{
					if (_control is RichTextBox rtf)
						return KeysharpEnhancements.NormalizeEol(rtf.Rtf);

					return DefaultErrorObject;
				}
				set
				{
					if (_control is RichTextBox rtf)
						rtf.Rtf = KeysharpEnhancements.NormalizeEol(value);
					else
						_ = Errors.ErrorOccurred($"Can only set RichText on a RichEdit control. Attempted on a {_control.GetType().Name} control.");
				}
			}

			public (Type, object) super => (typeof(KeysharpObject), this);

			public object Text
			{
				get
				{
					if (_control is KeysharpListBox lb)
					{
						if (lb.SelectionMode == SelectionMode.One)
						{
							if (lb.SelectedIndex > 0 && lb.SelectedItem is string s)
								return s;
						}
						else
							return new Array(lb.SelectedItems);
					}

					if (_control is KeysharpComboBox cb)
					{
						if (cb.DropDownStyle == ComboBoxStyle.DropDownList && cb.SelectedIndex > 0 && cb.SelectedItem is string s)
							return s;
						else
							return cb.Text;
					}

					if (_control is KeysharpStatusStrip ss)
						return ss.Items.Count > 0 ? ss.Items[0].Text : "";

					if (_control is KeysharpTabControl tc && tc.SelectedTab is TabPage tp)
						return tp.Text;
					else if (_control is KeysharpGroupBox gb)
						return gb.Text;
					else
						return _control.Text;
				}
				set
				{
					var s = value.As();

					if (_control is KeysharpListBox lb)
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
					else if (_control is KeysharpComboBox cb)
					{
						if (s?.Length == 0)
							cb.SelectedItem = -1;

						if (cb.DropDownStyle == ComboBoxStyle.DropDownList)
							cb.SelectedItem = s;
						else
							cb.Text = s;
					}
					else if (_control is KeysharpTabControl tc)
						tc.SelectTab(s);
					else if (_control is KeysharpGroupBox gb)
						gb.Text = s;
					else
						_control.Text = s;

					if (ParentForm.Visible == true)
						_control.Refresh();
				}
			}

			public string Type => typename;

			public object Value
			{
				get
				{
					if (_control is KeysharpLabel lbl)
						return lbl.Text;
					else if (_control is KeysharpTextBox txt)
						return KeysharpEnhancements.NormalizeEol(txt.Text);
					else if (_control is KeysharpRichEdit rtf)
						return KeysharpEnhancements.NormalizeEol(rtf.Text);
					else if (_control is HotkeyBox hk)
						return hk.GetText();
					else if (_control is KeysharpNumericUpDown nud)
						return nud.Value;
					else if (_control is KeysharpButton btn)
						return btn.Text;
					else if (_control is KeysharpCheckBox cb)
					{
						if (cb.CheckState == CheckState.Checked)
							return 1L;
						else if (cb.CheckState == CheckState.Unchecked)
							return 0L;
						else
							return -1L;
					}
					else if (_control is KeysharpRadioButton rb)
						return rb.Checked ? 1L : 0L;
					else if (_control is KeysharpComboBox cmb)
					{
						if (cmb.DropDownStyle == ComboBoxStyle.DropDown)
						{
							var indexof = cmb.Items.IndexOf(cmb.Text);

							if (indexof == -1)
								return 0L;
						}

						return (long)cmb.SelectedIndex + 1;
					}
					else if (_control is KeysharpListBox lb)
					{
						return lb.SelectionMode == SelectionMode.One
							   ? (long)lb.SelectedIndex + 1
							   : new Array(lb.SelectedIndices.Cast<int>().Select(x => x + 1).ToList());
					}
					else if (_control is KeysharpDateTimePicker dtp)
						return Conversions.ToYYYYMMDDHH24MISS(dtp.Value);
					else if (_control is KeysharpMonthCalendar mc)
					{
						if (mc.MaxSelectionCount == 1)
						{
							return $"{mc.SelectionStart:yyyyMMdd}";
						}
						else
						{
							if (mc.SelectionStart.Date == mc.SelectionEnd.Date)
								return $"{mc.SelectionStart:yyyyMMdd}";
							else
								return $"{mc.SelectionStart:yyyyMMdd}-{mc.SelectionEnd:yyyyMMdd}";
						}
					}
					else if (_control is KeysharpTrackBar tb)
						return tb.Value;
					else if (_control is KeysharpProgressBar pb)
						return pb.Value;
					else if (_control is KeysharpTabControl tc)
						return (long)tc.SelectedIndex + 1;
					else if (_control is KeysharpStatusStrip ss)
						return ss.Text;//Unsure if this is what's intended.
					else if (_control is KeysharpPictureBox pic)
						return pic.Filename;

#if WINDOWS
					else if (_control is KeysharpActiveX kax)
						return kax.Iid;

#endif
					return DefaultObject;
				}
				set
				{
					var val = value != null ? value.ToString() : "";
					var temp = value.ParseInt(false);
					var ival = temp ?? 0;

					if (_control is KeysharpLabel lbl)
						lbl.Text = val;
					else if (_control is KeysharpTextBox txt)
						txt.Text = KeysharpEnhancements.NormalizeEol(val, Environment.NewLine);
					else if (_control is KeysharpRichEdit kre)
						kre.Text = KeysharpEnhancements.NormalizeEol(val, Environment.NewLine);
					else if (_control is HotkeyBox hk)
						hk.SetText(val);
					else if (_control is KeysharpNumericUpDown nud)
						nud.Value = value.ParseDecimal().Value;
					else if (_control is KeysharpCheckBox cb)
					{
						var cbstate = ival;

						if (cbstate == -1)
							cb.CheckState = CheckState.Indeterminate;
						else
							cb.Checked = Options.OnOff(value) ?? false;
					}
					else if (_control is KeysharpRadioButton rb)
						rb.Checked = Options.OnOff(value) ?? false;
					else if (_control is KeysharpComboBox cmb)
						cmb.SelectedIndex = ival - 1;
					else if (_control is KeysharpListBox lb)
					{
						if (value is Array ar)
						{
							lb.ClearSelected();

							foreach (var (arval, _) in ar)
								lb.SetSelected(arval.ParseInt().GetValueOrDefault() - 1, true);
						}
						else
							lb.SelectedIndex = ival - 1;
					}
					else if (_control is KeysharpDateTimePicker dtp)
					{
						if (val?.Length == 0)
							dtp.Checked = false;
						else
							dtp.Value = Conversions.ToDateTime(val);
					}
					else if (_control is KeysharpMonthCalendar mc)
					{
						Conversions.ParseRange(val, out var dtlow, out var dthigh);

						if (dtlow == System.DateTime.MinValue)
							dtlow = dthigh;

						if (dthigh == System.DateTime.MaxValue)
							dthigh = dtlow;

						mc.SelectionRange = new SelectionRange(dtlow, dthigh);
					}
					else if (_control is KeysharpTrackBar tb)
						tb.Value = ival;
					else if (_control is KeysharpProgressBar pb)
						pb.Value = Math.Clamp(ival, pb.Minimum, pb.Maximum);
					else if (_control is KeysharpTabControl tc)
						tc.SelectedIndex = ival - 1;
					else if (_control is KeysharpStatusStrip ss)
						ss.Text = val;
					else if (_control is KeysharpPictureBox pic)
					{
						if (val == "")
						{
							var oldimage = pic.Image;
							pic.Image = null;

							if (oldimage is Bitmap oldbmp)
								oldbmp.Dispose();
						}
						else
						{
							var width = int.MinValue;
							var height = int.MinValue;
							var icon = "";
							object iconnumber = 0L;
							var filename = "";

							foreach (Range r in val.AsSpan().SplitAny(SpaceTabSv))
							{
								var opt = val.AsSpan(r).Trim();

								if (opt.Length > 0)
								{
									if (Options.TryParse(opt, "*w", ref width)) { }
									else if (Options.TryParse(opt, "*h", ref height)) { }
									else if (Options.TryParseString(opt, "*icon", ref icon)) { iconnumber = ImageHelper.PrepareIconNumber(icon); }
									else
									{
										filename = val.Substring(r.Start.Value);
										break;
									}
								}
							}

							//If neither were set, make one set and the other unset to force a resize internally
							//so it will match the dimensions of what's already loaded.
							if (width < 0 && height < 0)
								width = pic.Height;

							if (ImageHelper.LoadImage(filename, width, height, iconnumber).Item1 is Bitmap bmp)
							{
								if (pic.SizeMode == PictureBoxSizeMode.Zoom)
								{
									var ratio = bmp.Height != 0 ? (double)bmp.Width / bmp.Height : 1;

									if (ratio == 0)
										ratio = 1;

									if (width > 0)
										pic.Width = width;

									if (height > 0)
										pic.Height = height;

									if (width < 0 && pic.ScaleWidth)
										pic.Width = (int)(pic.Height * ratio);

									if (height < 0 && pic.ScaleHeight)
										pic.Height = (int)(pic.Width / ratio);
								}

								var oldimage = pic.Image;
								pic.Image = bmp;

								if (oldimage is Bitmap oldbmp)
									oldbmp.Dispose();
							}
						}
					}

					if (ParentForm.Visible == true)
						_control.Refresh();
				}
			}

			public object Visible
			{
				get => _control.Visible;
				set => _control.Visible = Options.OnOff(value) ?? false;
			}

			public object BackColor
			{
				get => (_control.BackColor.ToArgb() & 0x00FFFFFF).ToString("X6");

				set
				{
					if (value is string s)
					{
						if (Conversions.TryParseColor(s, out var c))
							_control.BackColor = c;
					}
					else
						_control.BackColor = Color.FromArgb((int)(value.Al() | 0xFF000000));

					if (ParentForm.Visible == true)
						_control.Refresh();
				}
			}

			public Control(params object[] args) : base(args) { }

			public override object __New(params object[] args)
			{
				if (args.Length == 0) return DefaultErrorObject;

				var g = args[0] as Gui;
				var control = args[1] as System.Windows.Forms.Control;
				var name = args[2].ToString();
				var wrap = args.Length > 3 ? args[3].Ab() : false;
				gui = new WeakReference<Gui>(g);
				typename = name;
				_control = control;
				_control.Tag = new GuiTag()
				{
					GuiControl = this,
					Index = _control.Parent != null ? _control.Parent.Controls.Count : 0
				};

				if (wrap)//Just a holder for the controls in the main window.
					return DefaultObject;

				_control.Click += _control_Click;
				_control.DoubleClick += _control_DoubleClick;

				if (_control is KeysharpStatusStrip ss)
				{
					ss.ItemClicked += Ss_ItemClicked;
				}
				else if (_control is KeysharpLinkLabel ll)
				{
					ll.LinkClicked += _control_Click;
				}
				else if (_control is KeysharpListView lv)
				{
					lv.ColumnClick += Lv_ColumnClick;
					lv.ItemChecked += Lv_ItemChecked;
					lv.AfterLabelEdit += Lv_AfterLabelEdit;
					lv.SelectedIndexChanged += Lv_SelectedIndexChanged;
					lv.ItemSelectionChanged += Lv_ItemSelectionChanged;
				}
				else if (_control is KeysharpTreeView tv)
				{
					tv.AfterCheck += Tv_AfterCheck;
					tv.AfterLabelEdit += Tv_AfterLabelEdit;
					tv.AfterExpand += Tv_AfterExpand;
					tv.AfterSelect += Tv_AfterSelect;
					tv.NodeMouseClick += Tv_NodeMouseClick;
				}
				else if (_control is KeysharpTrackBar tb)
				{
					tb.MouseCaptureChanged += Tb_MouseCaptureChanged;
					tb.ValueChanged += Tb_ValueChanged;
				}
				else if (_control is KeysharpTabControl tc)
				{
					tc.Selected += Tc_Selected;
				}
				else if (_control is KeysharpNumericUpDown nud)
				{
					nud.ValueChanged += Nud_ValueChanged;
				}
				else if (_control is HotkeyBox hkb)
				{
					hkb.TextChanged += Hkb_TextChanged;
				}
				else if (_control is KeysharpMonthCalendar mc)
				{
					mc.DateChanged += Mc_DateChanged;
				}
				else if (_control is KeysharpDateTimePicker dtp)
				{
					dtp.ValueChanged += Dtp_ValueChanged;
				}
				else if (_control is KeysharpTextBox txt)
				{
					txt.TextChanged += Txt_TextChanged;
				}
				else if (_control is KeysharpListBox lb)
				{
					lb.SelectedIndexChanged += Lb_SelectedIndexChanged;
				}
				else if (_control is KeysharpComboBox cmb)
				{
					cmb.SelectedIndexChanged += Cmb_SelectedIndexChanged;
				}

				_control.GotFocus += _control_GotFocus;
				_control.LostFocus += _control_LostFocus;
				_control.KeyDown += _control_KeyDown;
				_control.MouseDown += _control_MouseDown;
				dummyHandle = _control.Handle;//Force creation of the handle.
				return DefaultObject;
			}

			public object Add(params object[] obj)
			{
				object result = DefaultObject;
				eventHandlerActive = false;

				try
				{
					if (_control is KeysharpTreeView tv)
					{
						var (name, parent, options) = obj.Sls();
						var first = false;
						var n = int.MinValue;
						TreeNode node;
						TreeNodeCollection nodes = null;
						parenthandle = tv.Handle.ToInt64();//By forcing a read of the parent handle, it causes the controls and their handles to properly be created. This is the only way nodes will be properly nested before the window is shown.

						if (parent == 0)
						{
							nodes = tv.Nodes;
						}
						else
						{
							var top = TreeViewHelper.TV_FindNode(tv, parent);
							nodes = top == null ? tv.Nodes : top.Nodes;
						}

						foreach (Range r in options.AsSpan().SplitAny(Spaces))
						{
							var opt = options.AsSpan(r).Trim();

							if (opt.Length > 0)
							{
								if (int.TryParse(opt, out var index)) { n = index; }
								else if (opt.Equals("First", StringComparison.OrdinalIgnoreCase)) { first = true; }
							}
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
						result = TreeViewHelper.TV_NodeOptions(node, parent, options, false);
					}
					else if (_control is KeysharpListView lv)
					{
						var lvo = obj.Length > 0 && obj[0] is string options && options.Length > 0 ? ListViewHelper.ParseListViewOptions(options) : new ListViewHelper.ListViewOptions();
						var strs = obj.Cast<object>().Skip(1).Select(x => x.Str()).ToList();
						result = ListViewHelper.AddOrInsertListViewItem(lv, lvo, strs, int.MinValue);
					}
					else
					{
						if (obj.Length > 0 && obj[0] is Array arr)
							obj = arr.array.ToArray();

						if (_control is KeysharpListBox lb)//Using AddRange() relieves the caller of having to set -Redraw first.
							lb.Items.AddRange(obj.Cast<object>().Select(x => x.Str()).ToArray());
						else if (_control is KeysharpComboBox cb)
							cb.Items.AddRange(obj.Cast<object>().Select(x => x.Str()).ToArray());
						else if (_control is KeysharpTabControl tc)
							tc.TabPages.AddRange(obj.Cast<object>().Select(x => new TabPage(x.Str())).ToArray());
					}
				}
				finally
				{
					eventHandlerActive = true;
				}

				return result;
			}

			public object Choose(object value)
			{
				//The documentation says "Unlike ControlChooseIndex, this method does not raise a Change or DoubleClick event."
				//But we don't raise click events anyway here, so it shouldn't matter.
				var s = value as string;
				var i = value.Ai() - 1;

				if (_control is KeysharpTabControl tc)
				{
					if (!string.IsNullOrEmpty(s))
					{
						if (tc.FindTab(s, false) is TabPage tp)
							tc.SelectTab(tp);
					}
					else if (i >= 0)
						tc.SelectTab(i);
				}
				else if (_control is KeysharpListBox lb)
				{
					if (!string.IsNullOrEmpty(s))
						lb.SelectItem(s);
					else if (i >= 0)
						lb.SetSelected(i, true);
					else
						lb.ClearSelected();
				}
				else if (_control is KeysharpComboBox cb)
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

				return DefaultObject;
			}

			/// <summary>
			/// Deletes the specified entry or all entries of a ListBox, DropDownList, ComboBox, or Tab control.
			/// Note this differs from AHK in that deleting a tab fully removes the tab, and does not associate the controls
			/// of an existing tab to the tab at the index that was deleted.
			/// </summary>
			/// <param name="value"></param>
			public long Delete(object value = null)
			{
				var index = value.Ai() - 1;

				switch (_control)
				{
					case KeysharpListBox lb:
						if (index >= 0)
							lb.Items.RemoveAt(index);
						else
							lb.Items.Clear();

						break;

					case KeysharpComboBox cb:
						if (index >= 0)
							cb.Items.RemoveAt(index);
						else
							cb.Items.Clear();

						break;

					case KeysharpTabControl tc:
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
								tc.TabPages[index].Controls.AddRange(ctrls.Cast<System.Windows.Forms.Control>().ToArray());
							}
						}

						break;
					}

					case KeysharpTreeView tv:
					{
						var id = value.Al(long.MinValue);

						if (id == long.MinValue)
						{
							tv.Nodes.Clear();
							return 1L;
						}
						else if (TreeViewHelper.TV_FindNode(tv, id) is TreeNode node)
						{
							node.Remove();
							return 1L;
						}

						break;
					}

					case KeysharpListView lv:
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

			public long DeleteCol(object column)
			{
				if (_control is KeysharpListView lv)
				{
					var index = column.Ai() - 1;

					if (index >= 0 && index < lv.Columns.Count)
					{
						lv.Columns.RemoveAt(index);
						return 1L;
					}
				}

				return 0L;
			}

			public object Focus() => _control.Focus();

			public long Get(object itemID, object attribute)
			{
				if (_control is KeysharpTreeView tv)
				{
					var id = itemID.Al();
					var attr = attribute.As();

					if (TreeViewHelper.TV_FindNode(tv, id) is TreeNode node)
					{
						if (Options.OptionContains(attr, Keyword_Expand, Keyword_Expanded, Keyword_Expand[0].ToString()) && node.IsExpanded)
							return node.Handle.ToInt64();
						else if (Options.OptionContains(attr, Keyword_Check, Keyword_Checked, Keyword_Checked[0].ToString()) && node.Checked)
							return node.Handle.ToInt64();
						else if (Options.OptionContains(attr, Keyword_Bold, Keyword_Bold[0].ToString()) && node.NodeFont.Bold)
							return node.Handle.ToInt64();
					}
				}

				return 0L;
			}

			public long GetChild(object itemID)
			{
				if (_control is KeysharpTreeView tv)
				{
					var id = itemID.Al();
					var node = TreeViewHelper.TV_FindNode(tv, id);
					return node == null ? 0 : node.Nodes.Count == 0 ? 0L : node.FirstNode.Handle.ToInt64();
				}

				return 0L;
			}

			public object GetClientPos([Optional()][DefaultParameterValue(null)] object outX,
									   [Optional()][DefaultParameterValue(null)] object outY,
									   [Optional()][DefaultParameterValue(null)] object outWidth,
									   [Optional()][DefaultParameterValue(null)] object outHeight)
			{
				GetClientPos(_control, DpiScaling, outX, outY, outWidth, outHeight);
				return DefaultObject;
			}

			public long GetCount(object mode = null)
			{
				if (_control is KeysharpListView lv)
				{
					var m = mode.As();

					if (m?.Length == 0)
						return lv.Items.Count;
					else if (m.StartsWith("s", StringComparison.OrdinalIgnoreCase))
						return lv.SelectedItems.Count;
					else if (m.StartsWith("c", StringComparison.OrdinalIgnoreCase))
						return lv.Columns.Count;
				}
				else if (_control is KeysharpTreeView tv)
					return tv.Nodes.Count;

				return 0L;
			}

			public long GetNext(object startingRowNumber = null, object rowType = null)
			{
				var id = startingRowNumber.Al();
				var mode = rowType.As();

				if (_control is KeysharpTreeView tv)
				{
					none:

					if (string.IsNullOrEmpty(mode))
					{
						if (id == 0)
							return tv.Nodes.Count == 0 ? 0L : tv.Nodes[0].Handle.ToInt64();

						var node = TreeViewHelper.TV_FindNode(tv, id);
						return node == null || node.NextNode == null ? 0L : node.NextNode.Handle.ToInt64();
					}

					var check = Options.OptionContains(mode, Keyword_Check, Keyword_Checked, Keyword_Checked[0].ToString());
					var full = check || Options.OptionContains(mode, Keyword_Full, Keyword_Full[0].ToString());

					if (!full)
					{
						mode = null;
						goto none;
					}

					for (var i = id == 0 ? 1 : TreeViewHelper.TV_FindNode(tv, id).Index + 1; i < tv.Nodes.Count; i++)//Add one because it's supposed to look for the "next" node.
					{
						if (check && !tv.Nodes[i].Checked)
							continue;

						return tv.Nodes[i].Handle.ToInt64();
					}
				}
				else if (_control is KeysharpListView lv)
				{
					var startrow = (int)(id <= 1 ? 0 : id);//Do not subtract 1 here, because the documentation says to start at the next row, if not zero.
					var c = mode.StartsWith("c", StringComparison.OrdinalIgnoreCase);
					var f = mode.StartsWith("f", StringComparison.OrdinalIgnoreCase);

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

			public object GetNode(object itemID)
			{
				if (_control is KeysharpTreeView tv)
				{
					var id = itemID.Al();
					return TreeViewHelper.TV_FindNode(tv, id);
				}

				return DefaultErrorObject;
			}

			public long GetParent(object itemID)
			{
				if (_control is KeysharpTreeView tv)
				{
					var id = itemID.Al();
					var node = TreeViewHelper.TV_FindNode(tv, id);
					return node == null || node.Parent == null || !(node.Parent is TreeNode) ? 0L : node.Parent.Handle.ToInt64();
				}

				return DefaultErrorLong;
			}

			public object GetPos([Optional()][DefaultParameterValue(null)] object outX,
								 [Optional()][DefaultParameterValue(null)] object outY,
								 [Optional()][DefaultParameterValue(null)] object outWidth,
								 [Optional()][DefaultParameterValue(null)] object outHeight)
			{
				GetPos(_control, DpiScaling, outX, outY, outWidth, outHeight);
				return DefaultObject;
			}

			public long GetPrev(object itemID)
			{
				if (_control is KeysharpTreeView tv)
				{
					var id = itemID.Al();
					var node = TreeViewHelper.TV_FindNode(tv, id);
					return node == null || node.PrevNode == null ? 0L : node.PrevNode.Handle.ToInt64();
				}

				return DefaultErrorLong;
			}

			public long GetSelection() => _control is KeysharpTreeView tv&& tv.SelectedNode != null ? tv.SelectedNode.Handle.ToInt64() : 0L;

			public string GetText(object rowNumber, object columnNumber = null)
			{
				if (_control is KeysharpTreeView tv)
				{
					var id = rowNumber.Al();
					var node = TreeViewHelper.TV_FindNode(tv, id);

					if (node != null)
						return node.Text;
				}
				else if (_control is KeysharpListView lv)
				{
					var row = rowNumber.Ai();
					var col = columnNumber.Ai(1);
					row--;
					col = Math.Max(col - 1, 0);

					if (row < 0 && col < lv.Columns.Count)
						return lv.Columns[col].Text;
					else if (row < lv.Items.Count && col < lv.Items[row].SubItems.Count)
						return lv.Items[row].SubItems[col].Text;
				}

				return DefaultErrorString;
			}

			public long Insert(object rowNumber, params object[] obj)
			{
				if (_control is KeysharpListView lv)//Note that this index might not actually be where the row is shown, due to sorting.
				{
					var rownumber = rowNumber.Ai();
					string opts = null;

					if (obj.Length > 0)
						opts = obj[0].ToString();

					var lvo = opts is string options ? ListViewHelper.ParseListViewOptions(options) : new ListViewHelper.ListViewOptions();
					var strs = obj.Length > 1 ? obj.Cast<object>().Skip(1).Select(x => x.Str()).ToList() : [];
					return ListViewHelper.AddOrInsertListViewItem(lv, lvo, strs, rownumber - 1) + 1;
				}

				return 0L;
			}

			public long InsertCol(object columnNumber = null, object options = null, object columnTitle = null)
			{
				if (_control is KeysharpListView lv)
				{
					var index = columnNumber.Ai(int.MaxValue);
					var opts = options.As();
					var title = columnTitle.As();
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
						index = lv.Columns.Add(header);

					ListViewHelper.ParseAndApplyListViewColumnOptions(header, opts);
					return index + 1L;
				}

				return -1L;
			}

			public long Modify(object rowNumber, object options = null, params object[] obj)
			{
				var opts = options == null ? null : options.ToString();
				var rownumber = rowNumber.Al();

				if (_control is KeysharpTreeView tv)
				{
					var id = rownumber;
					var name = obj.S1();

					if (TreeViewHelper.TV_FindNode(tv, id) is TreeNode node)
					{
						if (opts?.Length == 0 && name?.Length == 0)
						{
							node.TreeView.SelectedNode = node;
							return node.Handle.ToInt64();
						}
						else if (name != "")
							node.Text = name;

						return TreeViewHelper.TV_NodeOptions(node, node.Parent != null ? node.Parent.Handle.ToInt64() : 0L, opts, true);
					}
				}
				else if (_control is KeysharpListView lv)
				{
					try
					{
						if (rownumber < lv.Items.Count)
						{
							var lvo = opts is string o ? ListViewHelper.ParseListViewOptions(o) : new ListViewHelper.ListViewOptions();
							var strs = obj.Length > 0 ? obj.Cast<object>().Select(x => x.Str()).ToList() : [];
							var start = Math.Max(0, rownumber - 1);
							var end = rownumber == 0 ? lv.Items.Count : Math.Min(rownumber, lv.Items.Count);

							for (rownumber = start; rownumber < end; rownumber++)
							{
								var item = lv.Items[(int)rownumber];

								for (int i = 0, j = lvo.colstart; i < strs.Count && j < item.SubItems.Count; i++, j++)
									item.SubItems[j].Text = strs[i];

								ListViewHelper.ApplyListViewOptions(lv, item, lvo);
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

			public long ModifyCol(object columnNumber = null, object options = null, object columnTitle = null)
			{
				if (_control is KeysharpListView lv)
				{
					var colnumber = columnNumber.Ai();
					var opts = options.As();
					var coltitle = columnTitle.As();

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

						ListViewHelper.ParseAndApplyListViewColumnOptions(col, opts);
						return 1L;
					}
				}

				return 0L;
			}

			public object Move(object x = null, object y = null, object width = null, object height = null)
			{
				var _x = x.Al(long.MinValue);
				var _y = y.Al(long.MinValue);
				var w = width.Al(long.MinValue);
				var h = height.Al(long.MinValue);
				var scale = !DpiScaling ? 1.0 : A_ScaledScreenDPI;
				var hasScrollBars = _control is KeysharpTextBox || _control is KeysharpRichEdit;//Reflections.SafeHasProperty(_control, "ScrollBars") || Reflections.SafeHasProperty(_control, "HorizontalScrollbar") || Reflections.SafeHasProperty(_control, "Scrollable")
				Point offset = Parent == null || Parent.GetControl() is Form ? Point.Empty : Parent.GetControl().GetLocationRelativeToForm();

				if (_x != long.MinValue)
					_control.Left = (int)Math.Round(_x * scale - offset.X);

				if (_y != long.MinValue)
					_control.Top = (int)Math.Round(_y * scale - offset.Y);

				if (w != long.MinValue)//Add extra if the control has scrollbars, even if they are not visible.
					_control.Width = (int)Math.Round(w * scale) - (hasScrollBars ? SystemInformation.VerticalScrollBarWidth : 0);

				if (h != long.MinValue)//Unsure if it's needed here too.
					_control.Height = (int)Math.Round(h * scale) - (hasScrollBars ? SystemInformation.HorizontalScrollBarHeight : 0);

				return DefaultObject;
			}

			public object SetCue(object newText, object showWhenFocused = null)
			{
				string txt = newText.ToString();
				int showOnFocus = ForceBool(showWhenFocused ?? false) ? 1 : 0;

				if (_control is KeysharpTextBox tb)
				{
#if WINDOWS

					if (!tb.Multiline)
						WindowsAPI.SendMessage(tb.Handle, WindowsAPI.EM_SETCUEBANNER, showOnFocus, txt);
					else
						tb.PlaceholderText = txt;

#endif
					return DefaultObject;
				}

#if WINDOWS
				else if (_control is KeysharpComboBox cb)
				{
					// Find the embedded Edit control
					nint editHandle = WindowsAPI.FindWindowEx(cb.Handle, 0, "Edit", null);

					if (editHandle != 0)
					{
						WindowsAPI.SendMessage(editHandle, WindowsAPI.EM_SETCUEBANNER, showOnFocus, txt);
						return DefaultObject;
					}
				}

				return Errors.ValueErrorOccurred($"Only Edit and ComboBox controls implement this method.");
#else
				return Errors.ValueErrorOccurred($"Only Edit controls implement this method.");
#endif
			}

			public object OnCommand(object notifyCode, object callback, object addRemove = null)
			{
				HandleOnCommandNotify(notifyCode.Al(), callback, addRemove.Al(1L), ref commandHandlers);
				return DefaultObject;
			}

			public object OnEvent(object eventName, object callback, object addRemove = null)
			{
				var e = eventName.As().ToLower();
				var h = callback;
				var i = addRemove.Al(1);

				if (gui == null || !gui.TryGetTarget(out var g))
					return DefaultErrorObject;

				var del = Functions.GetFuncObj(h, g.form.eventObj, true);

				if (del != null)
				{
					if (e == "change")
					{
						if (changeHandlers == null)
							changeHandlers = [];

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
							focusHandlers = [];

						focusHandlers.ModifyEventHandlers(del, i);
					}
					else if (e == "losefocus")
					{
						if (lostFocusHandlers == null)
							lostFocusHandlers = [];

						lostFocusHandlers.ModifyEventHandlers(del, i);
					}
					else if (e == "colclick")
					{
						if (_control is KeysharpListView lv)
						{
							if (columnClickHandlers == null)
								columnClickHandlers = [];

							columnClickHandlers.ModifyEventHandlers(del, i);
						}
					}
					else if (e == "itemcheck")
					{
						if (_control is KeysharpTreeView || _control is KeysharpListView)
						{
							if (itemCheckHandlers == null)
								itemCheckHandlers = [];

							itemCheckHandlers.ModifyEventHandlers(del, i);
						}
					}
					else if (e == "itemedit")
					{
						if (_control is KeysharpTreeView || _control is KeysharpListView)
						{
							if (itemEditHandlers == null)
								itemEditHandlers = [];

							itemEditHandlers.ModifyEventHandlers(del, i);
						}
					}
					else if (e == "itemexpand")
					{
						if (_control is KeysharpTreeView)
						{
							if (itemExpandHandlers == null)
								itemExpandHandlers = [];

							itemExpandHandlers.ModifyEventHandlers(del, i);
						}
					}
					else if (e == "itemfocus")
					{
						if (_control is KeysharpListView)
						{
							if (focusedItemChangedHandlers == null)
								focusedItemChangedHandlers = [];

							focusedItemChangedHandlers.ModifyEventHandlers(del, i);
						}
					}
					else if (e == "itemselect")
					{
						if (_control is KeysharpTreeView || _control is KeysharpListView)
						{
							if (selectedItemChangedHandlers == null)
								selectedItemChangedHandlers = [];

							selectedItemChangedHandlers.ModifyEventHandlers(del, i);
						}
					}
					else if (e == "contextmenu")
					{
						if (contextMenuChangedHandlers == null)
							contextMenuChangedHandlers = [];

						if (!(_control is KeysharpTextBox) && !(_control is KeysharpMonthCalendar))
							contextMenuChangedHandlers.ModifyEventHandlers(del, i);
					}
				}

				return DefaultObject;
			}

			public object OnNotify(object notifyCode, object callback, object addRemove = null)
			{
				HandleOnCommandNotify(notifyCode.Al(), callback, addRemove.Al(1L), ref notifyHandlers);
				return DefaultObject;
			}

			public object Opt(object options)
			{
				if (gui == null || !gui.TryGetTarget(out var g))
					return DefaultErrorObject;

				var opts = Core.Gui.ParseOpt(typename, _control.Text, options.As());

				if (opts.redraw.HasValue)
				{
					if (opts.redraw == false)
					{
						_control.SuspendDrawing();
					}
					else
					{
						if (_control is KeysharpListView klv)
							klv.SetListViewColumnSizes();

						_control.ResumeDrawing();
					}
				}

				if (opts.c != _control.ForeColor && opts.c != System.Windows.Forms.Control.DefaultForeColor)
				{
					if (_control is KeysharpDateTimePicker dtp)
						dtp.CalendarForeColor = opts.c;
					else if (_control is KeysharpMonthCalendar mc)
						mc.TitleForeColor = opts.c;

					_control.ForeColor = opts.c;
				}

				if (_control is KeysharpButton)
				{
					if (opts.btndef.HasValue)
						g.form.AcceptButton = opts.btndef == true ? (IButtonControl)_control : null;
				}
				else if (_control is KeysharpListBox lb)
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
				else if (_control is KeysharpComboBox cb)
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

					if (typename != Keyword_DropDownList && opts.cmbsimple.HasValue)
					{
						cb.DropDownStyle = opts.cmbsimple.IsTrue() ? ComboBoxStyle.Simple : ComboBoxStyle.DropDown;
					}
				}
				else if (_control is KeysharpTextBox txt)
				{
					txt.AcceptsTab = opts.wanttab ?? false;
					txt.AcceptsReturn = opts.wantreturn ?? false;

					if (opts.wantctrla.IsFalse())
					{
						txt.PreviewKeyDown += Core.Gui.SuppressCtrlAPreviewKeyDown;
						txt.KeyDown += Core.Gui.SuppressCtrlAKeyDown;
					}
					else if (opts.wantctrla.IsTrue())
					{
						txt.PreviewKeyDown -= Core.Gui.SuppressCtrlAPreviewKeyDown;
						txt.KeyDown -= Core.Gui.SuppressCtrlAKeyDown;
					}

					if (opts.vscroll.IsTrue() && opts.hscrollamt != int.MinValue)
						txt.ScrollBars = ScrollBars.Both;
					else if (opts.vscroll.IsTrue() || txt.Multiline)
						txt.ScrollBars = ScrollBars.Vertical;
					else if (opts.hscrollamt != int.MinValue)
						txt.ScrollBars = ScrollBars.Horizontal;

					if (opts.limit != int.MinValue)
						txt.MaxLength = opts.limit;

#if WINDOWS
					long val;

					if (opts.number)
						val = WindowsAPI.GetWindowLongPtr(txt.Handle, WindowsAPI.GWL_STYLE).ToInt64() | 0x2000;
					else
						val = WindowsAPI.GetWindowLongPtr(txt.Handle, WindowsAPI.GWL_STYLE).ToInt64() & ~0x2000;

					_ = WindowsAPI.SetWindowLongPtr(txt.Handle, WindowsAPI.GWL_STYLE, new nint(val));
#else
					txt.IsNumeric = opts.number;
#endif

					if (opts.lowercase.IsTrue())
						txt.CharacterCasing = CharacterCasing.Lower;
					else if (opts.uppercase.IsTrue())
						txt.CharacterCasing = CharacterCasing.Upper;
					else
						txt.CharacterCasing = CharacterCasing.Normal;
				}
				else if (_control is KeysharpRichEdit rtxt)
				{
					rtxt.AcceptsTab = opts.wanttab ?? false;

					if (opts.wantctrla.IsFalse())
					{
						rtxt.PreviewKeyDown += Core.Gui.SuppressCtrlAPreviewKeyDown;
						rtxt.KeyDown += Core.Gui.SuppressCtrlAKeyDown;
					}
					else if (opts.wantctrla.IsTrue())
					{
						rtxt.PreviewKeyDown -= Core.Gui.SuppressCtrlAPreviewKeyDown;
						rtxt.KeyDown -= Core.Gui.SuppressCtrlAKeyDown;
					}

					if (opts.vscroll.IsTrue() && opts.hscrollamt != int.MinValue)
						rtxt.ScrollBars = RichTextBoxScrollBars.Both;
					else if (opts.vscroll.IsTrue() || rtxt.Multiline)
						rtxt.ScrollBars = RichTextBoxScrollBars.Vertical;
					else if (opts.hscrollamt != int.MinValue)
						rtxt.ScrollBars = RichTextBoxScrollBars.Horizontal;

					if (opts.limit != int.MinValue)
						rtxt.MaxLength = opts.limit;

#if WINDOWS
					long val;

					if (opts.number)
						val = WindowsAPI.GetWindowLongPtr(rtxt.Handle, WindowsAPI.GWL_STYLE).ToInt64() | 0x2000;
					else
						val = WindowsAPI.GetWindowLongPtr(rtxt.Handle, WindowsAPI.GWL_STYLE).ToInt64() & ~0x2000;

					_ = WindowsAPI.SetWindowLongPtr(rtxt.Handle, WindowsAPI.GWL_STYLE, new nint(val));
#else
					rtxt.IsNumeric = opts.number;
#endif

					if (opts.lowercase.IsTrue())
						rtxt.CharacterCasing = CharacterCasing.Lower;
					else if (opts.uppercase.IsTrue())
						rtxt.CharacterCasing = CharacterCasing.Upper;
					else
						rtxt.CharacterCasing = CharacterCasing.Normal;
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

#if WINDOWS

					if (opts.thick != int.MinValue)
						_ = WindowsAPI.SendMessage(tb.Handle, WindowsAPI.TBM_SETTHUMBLENGTH, (uint)opts.thick, 0);

					if (opts.tooltip)
						_ = WindowsAPI.SendMessage(tb.Handle, WindowsAPI.TBM_SETTIPSIDE, (uint)opts.tooltipside, 0);

#endif
				}
				else if (_control is KeysharpTreeView tv)
				{
					if (opts.buttons.HasValue)
						tv.ShowPlusMinus = opts.buttons.Value;

					if (opts.rdonly.HasValue)
						tv.LabelEdit = !opts.rdonly.Value;

					if (opts.lines.HasValue)
						tv.ShowLines = opts.lines.Value;

					if (tv.LabelEdit && opts.wantf2.HasValue)
						if (opts.wantf2.IsTrue())
							tv.KeyDown += Core.Gui.Tv_Lv_KeyDown;
						else
							tv.KeyDown -= Core.Gui.Tv_Lv_KeyDown;
				}
				else if (_control is KeysharpListView lv)
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
							lv.KeyDown += Core.Gui.Tv_Lv_KeyDown;
						else
							lv.KeyDown -= Core.Gui.Tv_Lv_KeyDown;

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
				else if (_control is KeysharpProgressBar pb)
				{
					if (opts.smooth.HasValue)
						pb.Style = opts.smooth.IsTrue() ? ProgressBarStyle.Continuous : ProgressBarStyle.Blocks;
				}
				else if (_control is KeysharpTabControl tc)
				{
					if (opts.buttons.HasValue)
						tc.Appearance = opts.buttons.Value ? TabAppearance.FlatButtons : TabAppearance.Normal;

					if (opts.wordwrap.HasValue)
						tc.Multiline = opts.wordwrap.IsTrue();

					if (opts.leftj.IsTrue())
						tc.Alignment = TabAlignment.Left;
					else if (opts.rightj.IsTrue())
						tc.Alignment = TabAlignment.Right;
					else if (opts.bottom)
						tc.Alignment = TabAlignment.Bottom;
					else if (opts.top)
						tc.Alignment = TabAlignment.Top;

					if (opts.bgtrans)
						tc.SetColor(Color.Transparent);
					else if (opts.bgcolor.HasValue)
						tc.SetColor(opts.bgcolor.Value);
				}
				else if (_control is KeysharpNumericUpDown nud)
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

				return DefaultObject;
			}

			public object Redraw()
			{
				_control.Refresh();
				return DefaultObject;
			}

			public object SetFont(object options = null, object fontName = null)
			{
				_control.SetFont(options, fontName);
				return DefaultObject;
			}

			public object SetFormat(object format)
			{
				(_control as DateTimePicker)?.SetFormat(format);
				return DefaultObject;
			}

			public nint SetIcon(object fileName, object iconNumber = null, object partNumber = null)
			{
				if (_control is KeysharpStatusStrip ss)
				{
					var filename = fileName.As();
					var iconnumber = ImageHelper.PrepareIconNumber(iconNumber);
					var part = partNumber.Ai(1);
					part--;
					(Bitmap, object) ret;

					if (part < ss.Items.Count && (ret = ImageHelper.LoadImage(filename, 0, 0, iconnumber)).Item1 is Bitmap bmp)
					{
						ss.Items[part].Image = bmp;
						return ret.Item2 is Icon icon ? icon.Handle : bmp.GetHicon();
					}
				}

				return 0;
			}

			public long SetImageList(object imageListID, object iconType = null)
			{
				var id = imageListID.Al();
				var type = iconType.Al(-1);
				var oldil = 0L;

				if (ImageLists.IL_Get(id) is ImageList il)
				{
					if (_control is KeysharpTreeView tv)
					{
						oldil = ImageLists.IL_GetId(tv.ImageList);
						tv.ImageList = il;
					}
					else if (_control is KeysharpListView lv)
					{
						var newil = ImageLists.IL_Get(id);

						if (newil != null)
						{
							switch (type)
							{
								case 0:
								{
									oldil = ImageLists.IL_GetId(lv.LargeImageList);
									lv.LargeImageList = newil;
									break;
								}

								case 1:
								{
									oldil = ImageLists.IL_GetId(lv.SmallImageList);
									lv.SmallImageList = newil;
									break;
								}

								case 2://Documentation says state icons don't work, but they do here.
								{
									oldil = ImageLists.IL_GetId(lv.StateImageList);
									lv.StateImageList = newil;
									break;
								}

								default:
								{
									if (il.ImageSize.Width > Env.SysGet(SystemMetric.SM_CXSMICON).Al())
									{
										oldil = ImageLists.IL_GetId(lv.LargeImageList);
										lv.LargeImageList = newil;
									}
									else
									{
										oldil = ImageLists.IL_GetId(lv.SmallImageList);
										lv.SmallImageList = newil;
									}

									break;
								}
							}
						}
					}
					else if (_control is KeysharpTabControl tc)
					{
						oldil = ImageLists.IL_GetId(tc.ImageList);
						tc.ImageList = il;
					}
				}

				return oldil;
			}

			public object SetParts(params object[] widths)
			{
				if (_control is KeysharpStatusStrip ss)
				{
					KeysharpToolStripStatusLabel tssl = null;
					ss.Items.Clear();

					for (var i = 0; i < widths.Length - 1; i++)
					{
						var part = widths[i];

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

				return DefaultObject;
			}

			public object SetTabIcon(object tabIndex, object imageIndex)//New function since the original required SendMessage() to do this.
			{
				if (_control is KeysharpTabControl tc)
				{
					var tabindex = tabIndex.Ai();
					var imageindex = imageIndex.Ai();

					if (tabindex < tc.TabCount && tc.ImageList != null && imageindex < tc.ImageList.Images.Count)
						tc.TabPages[tabindex].ImageIndex = imageindex;
					else
						tc.TabPages[tabindex].ImageIndex = -1;
				}

				return DefaultObject;
			}

			public bool SetText(object newText, object partNumber = null, object style = null)
			{
				if (_control is KeysharpStatusStrip ss)
				{
					var text = newText.As();
					var part = partNumber.Ai(1);
					var s = style.Al(-1);
					part--;

					if (part < ss.Items.Count)
					{
						var item = ss.Items[part];
						item.Text = text;

						if (item is ToolStripStatusLabel tssl)
						{
							if (s == 0)
							{
								tssl.BorderStyle = Border3DStyle.Sunken;
								tssl.BorderSides = ToolStripStatusLabelBorderSides.All;
							}
							else if (s == 1)
							{
								tssl.BorderStyle = Border3DStyle.Flat;
								tssl.BorderSides = ToolStripStatusLabelBorderSides.None;
							}
							else if (s == 2)
							{
								tssl.BorderStyle = Border3DStyle.Raised;
								tssl.BorderSides = ToolStripStatusLabelBorderSides.All;
							}
						}

						return true;
					}
				}

				return false;
			}

			public object UseTab(object value = null, object exactMatch = null)
			{
				if (_control is KeysharpTabControl tc)
				{
					if (gui == null || !gui.TryGetTarget(out var g))
						return DefaultErrorObject;

					var val = value;
					var exact = exactMatch.Ab();

					if (val is string s)
					{
						if (s.Length > 0 && tc.FindTab(s, exact) is TabPage tp)
						{
							g.CurrentTab = tp;
							g.LastContainer = tp;
						}
					}
					else if (val != null)
					{
						var i = (int)val.Al();
						i--;

						if (i >= 0 && i < tc.TabPages.Count)
						{
							var tp = tc.TabPages[i];
							g.CurrentTab = tp;
							g.LastContainer = tp;
						}
					}
					else
					{
						tc.AdjustSize(!DpiScaling ? 1.0 : A_ScaledScreenDPI, requestedSize);
						g.LastContainer = tc.Parent;
					}
				}

				return DefaultObject;
			}

			internal static void GetClientPos(System.Windows.Forms.Control control, bool scaling, [ByRef] object outX, [ByRef] object outY, [ByRef] object outWidth, [ByRef] object outHeight) => GetPosHelper(control, scaling, true, outX, outY, outWidth, outHeight);

			internal static void GetPos(System.Windows.Forms.Control control, bool scaling, [ByRef] object outX, [ByRef] object outY, [ByRef] object outWidth, [ByRef] object outHeight) => GetPosHelper(control, scaling, false, outX, outY, outWidth, outHeight);

			internal static void GetPosHelper(System.Windows.Forms.Control control, bool scaling, bool client, [ByRef] object outX, [ByRef] object outY, [ByRef] object outWidth, [ByRef] object outHeight)
			{
				outX ??= VarRef.Empty; outY ??= VarRef.Empty; outWidth ??= VarRef.Empty; outHeight ??= VarRef.Empty;
				var rect = client ? control.ClientRectangle : control.Bounds;
				if (!client && control?.Parent != null)
				{
					Point p = control.Parent.GetLocationRelativeToForm();
					rect.X += p.X; rect.Y += p.Y;
				}

				if (!scaling)
				{
					Script.SetPropertyValue(outX, "__Value", (long)rect.X);
					Script.SetPropertyValue(outY, "__Value", (long)rect.Y);
					Script.SetPropertyValue(outWidth, "__Value", (long)rect.Width);
					Script.SetPropertyValue(outHeight, "__Value", (long)rect.Height);
				}
				else
				{
					var scale = 1.0 / Accessors.A_ScaledScreenDPI;
					Script.SetPropertyValue(outX, "__Value", (long)(rect.X * scale));
					Script.SetPropertyValue(outY, "__Value", (long)(rect.Y * scale));
					Script.SetPropertyValue(outWidth, "__Value", (long)(rect.Width * scale));
					Script.SetPropertyValue(outHeight, "__Value", (long)(rect.Height * scale));
				}
			}

			internal void _control_Click(object sender, EventArgs e)
			{
				if (!eventHandlerActive)
					return;

				if (_control is KeysharpTreeView tv)
					_ = clickHandlers.InvokeEventHandlers(this, GetSelection());
				else if (_control is KeysharpListView lv)
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
							_ = clickHandlers.InvokeEventHandlers(this, tss.Item1 != "" ? tss.Item1 : ll.Links.IndexOf(lllce.Link) + 1L, tss.Item2);
					}
				}
				//else if (_control is KeysharpButton)
				//{
				//  //mousecount ^= 1;//Button click events get fired twice, because we have double click and standard click enabled, so filter the second click here.
				//  //if (mousecount > 0)
				//  _ = clickHandlers.InvokeEventHandlers(this, 0L);
				//}
				else
					_ = clickHandlers.InvokeEventHandlers(this, 0L);
			}

			internal void _control_DoubleClick(object sender, EventArgs e)
			{
				if (!eventHandlerActive)
					return;

				if (_control is KeysharpTreeView tv)
					_ = doubleClickHandlers.InvokeEventHandlers(this, GetSelection());
				else if (_control is KeysharpListView lv)
				{
					if (lv.SelectedIndices.Count > 0)
						_ = doubleClickHandlers.InvokeEventHandlers(this, lv.SelectedIndices[0] + 1L);
					else
						_ = doubleClickHandlers.InvokeEventHandlers(this, 0L);
				}
				else if (_control is KeysharpListBox lb)
				{
					if (lb.SelectedIndices.Count > 0)
						_ = doubleClickHandlers.InvokeEventHandlers(this, lb.SelectedIndices[0] + 1L);
					else
						_ = doubleClickHandlers.InvokeEventHandlers(this, 0L);
				}
				else
					_ = doubleClickHandlers.InvokeEventHandlers(this, 0L);

				//Status strip items are handled in a separate special handler contained within each item.
			}

			internal void _control_GotFocus(object sender, EventArgs e)
			{
				if (eventHandlerActive)
					_ = (focusHandlers?.InvokeEventHandlers(this, 0L));
			}

			internal void _control_KeyDown(object sender, KeyEventArgs e)
			{
				if (eventHandlerActive && e.KeyCode == Keys.Apps || (e.KeyCode == Keys.F10 && ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift)))
					CallContextMenuChangeHandlers(true, Cursor.Position.X, Cursor.Position.Y);
			}

			internal void _control_LostFocus(object sender, EventArgs e)
			{
				if (eventHandlerActive)
					_ = (lostFocusHandlers?.InvokeEventHandlers(this, 0L));
			}

			internal void _control_MouseDown(object sender, MouseEventArgs e)
			{
				if (eventHandlerActive && e.Button == MouseButtons.Right)
					CallContextMenuChangeHandlers(false, e.X, e.Y);
			}

			internal void CallContextMenuChangeHandlers(bool wasRightClick, int x, int y)
			{
				if (!eventHandlerActive)
					return;

				if (_control is KeysharpListBox lb)
					_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, lb.SelectedIndex + 1L, wasRightClick, x, y));
				else if (_control is KeysharpListView lv)
					_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, lv.SelectedIndices.Count > 0 ? lv.SelectedIndices[0] + 1L : 0L, wasRightClick, x, y));
				else if (_control is KeysharpTreeView tv)
					_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, tv.SelectedNode.Handle.ToInt64(), wasRightClick, x, y));
				else
					_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, _control.Handle.ToInt64().ToString(), wasRightClick, x, y));//Unsure what to pass for Item, so just pass handle.
			}

			internal void Cmb_SelectedIndexChanged(object sender, EventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpComboBox)
					_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
			}

			internal void Dtp_ValueChanged(object sender, EventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpDateTimePicker)
					_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
			}

			internal void HandleOnCommandNotify(long code, object callback, long addremove, ref Dictionary<int, List<IFuncObj>> handlers)
			{
				if (gui == null || !gui.TryGetTarget(out var g))
					return;

				var del = Functions.GetFuncObj(callback, g.form.eventObj, true);

				if (handlers == null)
					handlers = [];

				var h = handlers.GetOrAdd((int)code);
				h.ModifyEventHandlers(del, addremove);
			}

			internal void Hkb_TextChanged(object sender, EventArgs e)
			{
				if (eventHandlerActive && _control is HotkeyBox)
					_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
			}

			internal unsafe object InvokeMessageHandlers(ref Message m)
			{
#if WINDOWS

				if (m.Msg == WindowsAPI.WM_NOTIFY || m.Msg == WindowsAPI.WM_REFLECT + WindowsAPI.WM_NOTIFY)
				{
					if (notifyHandlers != null)
					{
						var nmhdr = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));

						if (notifyHandlers.TryGetValue((int)nmhdr.code, out var handler))
						{
							var ret = handler?.InvokeEventHandlers(this, m.LParam.ToInt64());
							m.Result = ret.IsCallbackResultNonEmpty() ? 1 : 0;
							return true;
						}
					}
				}
				else if (m.Msg == WindowsAPI.WM_COMMAND)
				{
					if (commandHandlers != null)
					{
						var val = (int)((m.WParam.ToInt64() >> 16) & 0xFFFF);
						//KeysharpEnhancements.OutputDebugLine($"Received WM_COMMAND {m.Msg}, with val: {val:X}, with lparam: {m.LParam.ToInt64():X}, wparam: {m.WParam.ToInt64():X}");

						if (commandHandlers.TryGetValue(val, out var handler))
						{
							var ret = handler?.InvokeEventHandlers(this);
							m.Result = ret.IsCallbackResultNonEmpty() ? 1 : 0;
							return true;
						}
					}
				}

#endif
				return DefaultObject;
			}

			internal void Lb_SelectedIndexChanged(object sender, EventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpListBox)
					_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
			}

			internal void Lv_AfterLabelEdit(object sender, LabelEditEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpListView)
					_ = (itemEditHandlers?.InvokeEventHandlers(this, e.Item + 1L));//The documentation says to pass "item". Not really sure if that means index, or something else.
			}

			internal void Lv_ColumnClick(object sender, ColumnClickEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpListView)
					_ = (columnClickHandlers?.InvokeEventHandlers(this, e.Column + 1L));
			}

			internal void Lv_ItemChecked(object sender, ItemCheckedEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpListView)
					_ = (itemCheckHandlers?.InvokeEventHandlers(this, e.Item.Index + 1L, e.Item.Checked ? 1L : 0L));
			}

			internal void Lv_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpListView)
					_ = (selectedItemChangedHandlers?.InvokeEventHandlers(this, e.Item.Index + 1L, e.Item.Selected ? 1L : 0L));
			}

			internal void Lv_SelectedIndexChanged(object sender, EventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpListView lv)
					_ = (focusedItemChangedHandlers?.InvokeEventHandlers(this, lv.SelectedIndices.Count > 0 ? lv.SelectedIndices[0] + 1L : 0L));
			}

			internal void Mc_DateChanged(object sender, DateRangeEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpMonthCalendar)
					_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
			}

			internal void Nud_ValueChanged(object sender, EventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpNumericUpDown)
					_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
			}

			internal void Ss_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpStatusStrip ss)
					_ = (clickHandlers?.InvokeEventHandlers(this, ss.Items.IndexOf(e.ClickedItem) + 1L));
			}

			internal void Tb_MouseCaptureChanged(object sender, EventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpTrackBar && !AltSubmit)
					_ = (changeHandlers?.InvokeEventHandlers(this, 0L));//Winforms doesn't support the ability to pass the method by which the slider was changed.
			}

			internal void Tb_ValueChanged(object sender, EventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpTrackBar && AltSubmit)
					_ = (changeHandlers?.InvokeEventHandlers(this, 0L));//Winforms doesn't support the ability to pass the method by which the slider was changed.
			}

			internal void Tc_Selected(object sender, TabControlEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpTabControl)
					_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
			}

			internal void Tv_AfterCheck(object sender, TreeViewEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpTreeView)
					_ = (itemCheckHandlers?.InvokeEventHandlers(this, e.Node.Handle.ToInt64(), e.Node.Checked ? 1L : 0L));
			}

			internal void Tv_AfterExpand(object sender, TreeViewEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpTreeView)
					_ = (itemExpandHandlers?.InvokeEventHandlers(this, e.Node.Handle.ToInt64(), e.Node.IsExpanded ? 1L : 0L));
			}

			internal void Tv_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpTreeView)
					_ = (itemEditHandlers?.InvokeEventHandlers(this, e.Node.Handle.ToInt64()));
			}

			internal void Tv_AfterSelect(object sender, TreeViewEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpTreeView)
					_ = (selectedItemChangedHandlers?.InvokeEventHandlers(this, e.Node.Handle.ToInt64()));
			}

			internal void Tv_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpTreeView tv && e.Node == tv.SelectedNode)
					_ = (selectedItemChangedHandlers?.InvokeEventHandlers(this, e.Node.Handle.ToInt64()));
			}

			internal void Txt_TextChanged(object sender, EventArgs e)
			{
				if (eventHandlerActive && _control is KeysharpTextBox)
					_ = (changeHandlers?.InvokeEventHandlers(this, 0L));
			}
		}

	}
	internal class GuiTag
	{
		internal Gui.Control GuiControl { get; set; }
		internal int Index { get; set; }
	}
}