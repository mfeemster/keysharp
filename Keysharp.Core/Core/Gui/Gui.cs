using static Keysharp.Core.Flow;

namespace Keysharp.Core
{
	public class Gui : KeysharpObject, IEnumerable<(object, object)>
	{
		public TabPage CurrentTab;
		public KeysharpForm form;
		internal static ConcurrentDictionary<long, Gui> allGuiHwnds = new ConcurrentDictionary<long, Gui>();

		internal static Type[] GuiTypes = new Type[]
		{
			typeof(Keysharp.Core.Gui),
			typeof(Keysharp.Core.GuiControl),
			typeof(Keysharp.Core.Menu),
			typeof(System.Windows.Forms.Control)//Add native control and form types just to be safe.
		};

		internal List<IFuncObj> closedHandlers;
		internal List<IFuncObj> contextMenuChangedHandlers;
		internal Dictionary<object, object> controls = new Dictionary<object, object>();
		internal List<IFuncObj> dropFilesHandlers;
		internal List<IFuncObj> escapeHandlers;
		internal MenuBar menuBar;
		internal List<IFuncObj> sizeHandlers;

		private static readonly Dictionary<string, Action<Gui, object>> showOptionsDkt = new Dictionary<string, Action<Gui, object>>
		{
			{
				"AlwaysOnTop", (f, o) => { if (o is bool b) f.form.TopMost = b; }
			},
			{
				"Border", (f, o) =>
				{
					if (o is bool b && b)
						f.form.FormBorderStyle = f.resizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;//No such thing as a resizable single pixel border.
					else
						f.form.FormBorderStyle = f.resizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedDialog;
				}
			},
			{
				"Caption", (f, o) =>
				{
					if (o is bool b && b)
						f.form.FormBorderStyle = f.resizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedDialog;
					else
						f.form.FormBorderStyle = FormBorderStyle.None;
				}
			},
			{
				"Disabled", (f, o) => { if (o is bool b) f.form.Enabled = !b; }
			},
			{
				"DPIScale", (f, o) => { if (o is bool b) f.dpiscaling = b; }
			},
			{
				"LastFound", (f, o) =>
				{
					if (o is bool b)
					{
						f.lastfound = b;
						Keysharp.Scripting.Script.HwndLastUsed = f.Hwnd;
					}
				}
			},
			{
				"MaximizeBox", (f, o) => { if (o is bool b) f.form.MaximizeBox = b; }
			},
			{
				"MinimizeBox", (f, o) => { if (o is bool b) f.form.MinimizeBox = b; }
			},
			{
				"MinSize", (f, o) =>
				{
					if (o is string s)
					{
						if (s?.Length == 0)
						{
							f.form.MinimumSize = new Size(-1, -1);
						}
						else if (s.EndsWith("x", StringComparison.OrdinalIgnoreCase))//Only width was specified.
						{
							if (int.TryParse(s.AsSpan(0, s.Length - 1), out var width))
								f.form.MinimumSize = new Size(width, f.form.MinimumSize.Height);
						}
						else if (s.StartsWith("x", StringComparison.OrdinalIgnoreCase))//Only height was specified.
						{
							if (int.TryParse(s.AsSpan(1), out var height))
								f.form.MinimumSize = new Size(f.form.MinimumSize.Width, height);
						}
						else
						{
							var splits = s.Split('x', StringSplitOptions.RemoveEmptyEntries);

							if (splits.Length == 2)
							{
								if (int.TryParse(splits[0], out var width) && int.TryParse(splits[1], out var height))
									f.form.MinimumSize = new Size(width, height);
							}
						}
					}
				}
			},
			{
				"MaxSize", (f, o) =>
				{
					if (o is string s)
					{
						if (s?.Length == 0)
						{
							f.form.MaximumSize = new Size(-1, -1);
						}
						else if (s.EndsWith("x", StringComparison.OrdinalIgnoreCase))//Only width was specified.
						{
							if (int.TryParse(s.AsSpan(0, s.Length - 1), out var width))
								f.form.MaximumSize = new Size(width, f.form.MaximumSize.Height);
						}
						else if (s.StartsWith("x", StringComparison.OrdinalIgnoreCase))//Only height was specified.
						{
							if (int.TryParse(s.AsSpan(1), out var height))
								f.form.MaximumSize = new Size(f.form.MaximumSize.Width, height);
						}
						else
						{
							var splits = s.Split('x', StringSplitOptions.RemoveEmptyEntries);

							if (splits.Length == 2)
							{
								if (int.TryParse(splits[0], out var width) && int.TryParse(splits[1], out var height))
									f.form.MaximumSize = new Size(width, height);
							}
						}
					}
				}
			},
			{
				"OwnDialogs", (f, o) =>
				{
					if (o is bool b)
					{
						f.owndialogs = b;
						GuiHelper.DialogOwner = b ? f.form : null;
					}
				}
			},
			{
				"Owner", (f, o) =>
				{
					if (o is string s)
					{
						if (int.TryParse(s, out var hwnd))
						{
							if (Control.FromHandle(new IntPtr(hwnd)) is Form theform)
								f.form.Owner = theform;
						}
					}
				}
			},
			{
				"Parent", (f, o) =>
				{
					if (o is string s)
					{
						if (int.TryParse(s, out var hwnd))
						{
							if (Control.FromHandle(new IntPtr(hwnd)) is Form theform)
								f.form.Parent = theform;
						}
					}
				}
			},
			{
				"Resize", (f, o) =>
				{
					if (o is bool b)
					{
						f.resizable = b;
						f.form.FormBorderStyle = b ? FormBorderStyle.Sizable : FormBorderStyle.FixedDialog;
						f.form.MaximizeBox = b;
						f.form.SizeGripStyle = b ? SizeGripStyle.Show : SizeGripStyle.Hide;

						if (b)
							f.form.AutoSize = false;
					}
				}
			},
			{
				"SysMenu", (f, o) => { if (o is bool b) f.form.ControlBox = b; }
			},
			{
				"Theme", (f, o) =>
				{
					Keysharp.Scripting.Script.OutputDebug("Themes are not supported", false);
				}
			},
			{
				"ToolWindow", (f, o) => {
					if (o is bool b && b)
					{
						if (f.form.FormBorderStyle != FormBorderStyle.None)//Only change border if they haven't requested that there be no border.
							f.form.FormBorderStyle = f.resizable ? FormBorderStyle.SizableToolWindow : FormBorderStyle.FixedToolWindow;

						f.form.ShowInTaskbar = false;
					}
					else
					{
						if (f.form.FormBorderStyle != FormBorderStyle.None)
							f.form.FormBorderStyle = f.resizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedDialog;

						f.form.ShowInTaskbar = true;
					}
				}
			}
		};

		private static int windowCount = 0;
		private bool closingFromDestroy;
		private bool dpiscaling = true;
		private bool lastfound = false;
		private bool owndialogs = false;
		private bool resizable = false;

		public object BackColor
		{
			get => form.BackColor.ToArgb().ToString("X").Substring(2, 6);

			set
			{
				if (value is string s)
				{
					if (Keysharp.Core.Conversions.TryParseColor(s, out var c))
						form.BackColor = c;
				}
				else
					form.BackColor = Color.FromArgb((int)(value.Al() | 0xFF000000));
			}
		}

		public Control FocusedCtrl => form.ActiveControl;

		public IntPtr Hwnd => form.Handle;

		public long MarginX
		{
			get => form.Margin.Left;
			set => form.Margin = new Padding((int)value, form.Margin.Top, (int)value, form.Margin.Bottom);
		}

		public long MarginY
		{
			get => form.Margin.Top;
			set => form.Margin = new Padding(form.Margin.Left, (int)value, form.Margin.Right, (int)value);
		}

		public MenuBar MenuBar
		{
			get => menuBar;

			set
			{
				menuBar = value;
				form.TagAndAdd(menuBar.MenuStrip);
				form.MainMenuStrip = menuBar.MenuStrip;
			}
		}

		public object Name
		{
			get => form.Name;
			set => form.Name = value.ToString();
		}

		public KeysharpForm ParentForm => form.FindParent<KeysharpForm>();

		public object Title
		{
			get => form.Text;
			set => form.Text = value.As();
		}

		internal Font Font { get; set; }

		internal Control LastContainer { get; set; }

		internal Control LastControl
		{
			get
			{
				if (LastContainer != null)
				{
					//These must be sorted by tag because the built in Control.Controls collection doesn't keep items in the order they were added.
					var orderedControls = LastContainer.Controls.Cast<Control>().Where(c => c.Tag is GuiTag).OrderBy(c => ((GuiTag)c.Tag).Index);

					for (var i = orderedControls.Count() - 1; i >= 0; i--)
					{
						var ctrl = orderedControls.ElementAt(i);

						if (ctrl != null && !(ctrl is KeysharpStatusStrip))//Don't count a status strip as a last control since its placement is handled manually.
							return ctrl;
					}
				}

				return null;
			}
		}

		internal Point Section { get; set; }

		internal StatusStrip StatusBar { get; set; }

		public Gui(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			__New(obj0, obj1, obj2);
		}

		internal Gui(object obj0 = null, object obj1 = null, object obj2 = null, object obj3 = null)//The last parameter is hidden and is only for internal use for when we wrap the main window in a Gui object.
		{
			if (obj3 is KeysharpForm kf)
			{
				form = kf;

				//This is wrong and needs to add things recursively and keep a count.
				foreach (var ctrl in form.GetAllControlsRecusrvive<Control>())//In order for searches that use allGuiHwnds, we must make all of the child controls point here.
					ctrl.Tag = new GuiControl(this, ctrl, ctrl.Name, true);//Supposed to be name like "label", "edit" etc, but just pass the name since this is only used with the main window.
			}

			LastContainer = form;
			allGuiHwnds[form.Handle.ToInt64()] = this;

			if (lastfound)
				Keysharp.Scripting.Script.HwndLastUsed = Hwnd;
		}

		public IEnumerator<(object, object)> __Enum() => ((IEnumerable<(object, object)>)this).GetEnumerator();

		public override object __New(params object[] obj)
		{
			if (form == null)//Don't allow derived classes to init twice.
			{
				var options = obj.Length > 0 ? obj[0].As() : null;
				var caption = obj.Length > 1 ? obj[1].As() : null;
				var eventObj = obj.Length > 2 ? obj[2] : null;
				var newCount = Interlocked.Increment(ref windowCount);
				form = new KeysharpForm
				{
					eventObj = eventObj,
					FormBorderStyle = FormBorderStyle.FixedSingle,//Default to a non-resizeable window, with the maximize box disabled.
					Icon = Keysharp.Core.Properties.Resources.Keysharp_ico,
					Name = $"Keysharp window {newCount}",
					MaximizeBox = false,
					SizeGripStyle = SizeGripStyle.Hide,
					Tag = this,
					Text = caption != "" ? caption : Accessors.A_ScriptName
				};
				//Note that we don't do any Suspend/Resume layout calls when creating controls on the form as would normally
				//be done in designer-generated code. It appears to cause layout problems.
				Opt(options);
				//var formHandle = form.Handle;//Force the creation.
				form.FormClosing += Form_FormClosing;
				form.KeyDown += Form_KeyDown;
				form.Resize += Form_Resize;
				form.MouseDown += Form_MouseDown;
				form.DragDrop += Form_DragDrop;
				form.Load += Form_Load;
				var x = (int)Math.Round(form.Font.Size * 1.25f);//Not really sure if Size is the same as height, like the documentation says.//TODO
				var y = (int)Math.Round(form.Font.Size * 0.75f);
				form.Margin = new Padding(x, y, x, y);
				LastContainer = form;
				allGuiHwnds[form.Handle.ToInt64()] = this;

				if (lastfound)
					Keysharp.Scripting.Script.HwndLastUsed = Hwnd;
			}

			return "";
		}

		public GuiControl Add(object obj0, object obj1 = null, object obj2 = null)
		{
			var typeo = obj0.As();
			var options = obj1.As();
			var o = obj2;//The third argument needs to account for being an array in the case of combo/list boxes.
			var type = typeo.ToLowerInvariant();
			GuiControl holder = null;
			var text = o as string;
			var al = o as Array;
			var dpiscale = !dpiscaling ? 1.0 : Accessors.A_ScaledScreenDPI;
			var dpiinv = 1.0 / dpiscale;
			var opts = ParseOpt(type, text, options);
			Control ctrl = null;

			switch (type)
			{
				case Keywords.Keyword_Text:
				{
					//var lbl = new KeysharpLabel(0x20)
					var lbl = new KeysharpLabel
					{
						Font = Conversions.ConvertFont(form.Font)
					};
					ctrl = lbl;
				}
				break;

				case Keywords.Keyword_Edit:
				{
					var ml = opts.multiline.IsTrue() || opts.rows > 1 || opts.height != int.MinValue;
#if WINDOWS

					if (opts.number)
						opts.addstyle |= WindowsAPI.ES_NUMBER;

					if (opts.limit == int.MinValue && !ml)
						opts.remstyle |= WindowsAPI.WS_HSCROLL | WindowsAPI.ES_AUTOHSCROLL;

#endif
					var txt = new KeysharpEdit(opts.addstyle, opts.remstyle)
					{
						AcceptsTab = opts.wanttab ?? false,
						AcceptsReturn = opts.wantreturn ?? false,
						Multiline = ml,
						ReadOnly = opts.rdonly ?? false,
						WordWrap = ml
					};
#if !WINDOWS

					if (opts.number)
						txt.IsNumeric = true;

#endif

					if (opts.limit != int.MinValue)
						txt.MaxLength = opts.limit;

					if (opts.vscroll.IsTrue() && opts.hscrollamt != int.MinValue)
						txt.ScrollBars = ScrollBars.Both;
					else if (opts.vscroll.IsTrue() || txt.Multiline)
						txt.ScrollBars = ScrollBars.Vertical;
					else if (opts.hscrollamt != int.MinValue)
						txt.ScrollBars = ScrollBars.Horizontal;

					if (opts.lowercase.IsTrue())
						txt.CharacterCasing = CharacterCasing.Lower;
					else if (opts.uppercase.IsTrue())
						txt.CharacterCasing = CharacterCasing.Upper;
					else
						txt.CharacterCasing = CharacterCasing.Normal;

					if (opts.pwd)
					{
						if (opts.pwdch != "")
							txt.PasswordChar = opts.pwdch[0];
						else
							txt.UseSystemPasswordChar = true;
					}

					if (opts.wantctrla.IsFalse())
					{
						txt.PreviewKeyDown += SuppressCtrlAPreviewKeyDown;
						txt.KeyDown += SuppressCtrlAKeyDown;
					}

#if WINDOWS

					if (txt.Multiline && opts.tabstops.Any())
						_ = WindowsAPI.SendMessage(txt.Handle, WindowsAPI.EM_SETTABSTOPS, opts.tabstops.Count, opts.tabstops.ToArray());

#endif
					ctrl = txt;
				}
				break;

				case Keywords.Keyword_Rich_Edit:
				{
					var ml = !opts.multiline.IsFalse();
#if WINDOWS

					if (opts.number)
						opts.addstyle |= WindowsAPI.ES_NUMBER;

					if (opts.limit == int.MinValue && !ml)
						opts.remstyle |= WindowsAPI.WS_HSCROLL | WindowsAPI.ES_AUTOHSCROLL;

#endif
					var txt = new KeysharpRichEdit(opts.addstyle, opts.remstyle)
					{
						AcceptsTab = opts.wanttab ?? false,
						Multiline = ml,
						ReadOnly = opts.rdonly ?? false
					};
#if !WINDOWS

					if (opts.number)
						txt.IsNumeric = true;

#endif

					if (opts.limit != int.MinValue)
						txt.MaxLength = opts.limit;

					if (opts.vscroll.IsTrue() && opts.hscrollamt != int.MinValue)
						txt.ScrollBars = RichTextBoxScrollBars.Both;
					else if (opts.vscroll.IsTrue() || txt.Multiline)
						txt.ScrollBars = RichTextBoxScrollBars.Vertical;
					else if (opts.hscrollamt != int.MinValue)
						txt.ScrollBars = RichTextBoxScrollBars.Horizontal;

					if (opts.lowercase.IsTrue())
						txt.CharacterCasing = CharacterCasing.Lower;
					else if (opts.uppercase.IsTrue())
						txt.CharacterCasing = CharacterCasing.Upper;
					else
						txt.CharacterCasing = CharacterCasing.Normal;

					if (opts.wantctrla.IsFalse())
					{
						txt.PreviewKeyDown += SuppressCtrlAPreviewKeyDown;
						txt.KeyDown += SuppressCtrlAKeyDown;
					}

#if WINDOWS

					if (txt.Multiline && opts.tabstops.Any())
						_ = WindowsAPI.SendMessage(txt.Handle, WindowsAPI.EM_SETTABSTOPS, opts.tabstops.Count, opts.tabstops.ToArray());

#endif
					ctrl = txt;
				}
				break;

				case Keywords.Keyword_UpDown:
				{
					//TODO
					//This is done differently than how the documentation says.
					//There is no such thing as a "buddy". rather, the numeric up down control is entirely self
					//contained. This is because the buddy style control was a remnant of MFC, and C# doesn't support such a control.
					//Also, there is no way to do "wrapping". Users will need to program this themselves.
					//16 has no effect, no way to make it vertical.
					//Horz has no effect.
					//Min and max can't be swapped.
					//Increment is made easier to set with the new "Increment" option.
					//Hex is made easier with the new "Hex" option.
					var nud = new KeysharpNumericUpDown
					{
						Increment = opts.nudinc ?? 1,
						ThousandsSeparator = (opts.addstyle & 0x80) != 0x80,
						UpDownAlign = opts.leftj.IsTrue() ? LeftRightAlignment.Left : LeftRightAlignment.Right,
						Hexadecimal = opts.hex.IsTrue()
					};

					if (opts.nudlow.HasValue)
						nud.Minimum = opts.nudlow.Value;

					if (opts.nudhigh.HasValue)
						nud.Maximum = opts.nudhigh.Value;

					if (obj2 != null)
						nud.Value = (decimal)obj2.Ad();
					else
						nud.Value = Math.Min(nud.Minimum, 0m);

					ctrl = nud;
				}
				break;

				case Keywords.Keyword_Pic:
				case Keywords.Keyword_Picture://No special support for GDI+, instead we just use whatever C# uses under the hood for its PictureBox control. Also, animated gifs do animate.
				{
					var pic = new KeysharpPictureBox(text, 0x20);//Attempt to support transparency.

					if (opts.width < 0 && opts.height < 0)
					{
						pic.SizeMode = PictureBoxSizeMode.AutoSize;
					}
					else if (opts.width < 0 || opts.height < 0)
					{
						if (opts.width < 0)
							pic.ScaleWidth = true;
						else if (opts.height < 0)
							pic.ScaleHeight = true;

						pic.SizeMode = PictureBoxSizeMode.Zoom;
					}
					else
						pic.SizeMode = PictureBoxSizeMode.StretchImage;

					ctrl = pic;
				}
				break;

				case Keywords.Keyword_Button:
				{
#if WINDOWS
					opts.addstyle &= WindowsAPI.BS_NOTIFY;//Documentation says BS_NOTIFY will be automatically added in OnEvent(), which is the only time clicks are handled, so add regardless.
#endif
					ctrl = new KeysharpButton(opts.addstyle, opts.remstyle)
					{
						Name = text
					};

					if (opts.btndef.IsTrue())
						form.AcceptButton = (IButtonControl)ctrl;
				}
				break;

				case Keywords.Keyword_CheckBox:
				{
					var chk = new KeysharpCheckBox(opts.addstyle, opts.remstyle)
					{
						ThreeState = opts.check3
					};

					if (opts.ischecked.HasValue)
					{
						if (opts.ischecked == 0)
							chk.Checked = false;
						else if (opts.ischecked == 1)
							chk.Checked = true;
						else if (opts.ischecked == -1 || opts.checkedgray)
							chk.CheckState = CheckState.Indeterminate;
					}

					if (opts.rightj.HasValue)
						chk.CheckAlign = ContentAlignment.MiddleRight;

					ctrl = chk;
				}
				break;

				case Keywords.Keyword_Radio:
				{
					var rad = new KeysharpRadioButton(opts.addstyle, opts.remstyle)
					{
						AutoSize = true,
						Text = text
					};
					ctrl = rad;
				}
				break;

				case Keywords.Keyword_ComboBox:
				case Keywords.Keyword_DropDownList:
				{
					KeysharpComboBox ddl;

					if (type == Keywords.Keyword_DropDownList)
					{
						ddl = new KeysharpComboBox();
						ddl.DropDownStyle = ComboBoxStyle.DropDownList;
					}
					else
					{
#if WINDOWS
						ddl = new KeysharpComboBox(0, opts.limit != int.MinValue ? WindowsAPI.CBS_AUTOHSCROLL : 0);
#else
						ddl = new KeysharpComboBox(0, 0);
#endif
						ddl.DropDownStyle = opts.cmbsimple.IsTrue() ? ComboBoxStyle.Simple : ComboBoxStyle.DropDown;
					}

					if (opts.sort.IsTrue())
					{
						ddl.Sorted = true;

						if (type == Keywords.Keyword_ComboBox)
						{
							ddl.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
							ddl.AutoCompleteSource = AutoCompleteSource.ListItems;
						}
					}

					ddl.Items.AddRange(al.Cast<(object, object)>().Select(x => x.Item2).Select(x => opts.lowercase.IsTrue() ? x.Str().ToLower() : opts.uppercase.IsTrue() ? x.Str().ToUpper() : x.Str()).ToArray());

					if (opts.choose.Any())
						ddl.SelectedIndex = opts.choose[0];

					ddl.IntegralHeight = true;

					if (opts.rows != float.MinValue)
					{
						ddl.IntegralHeight = false;
						ddl.MaxDropDownItems = (int)opts.rows;
					}
					else if (opts.height != int.MinValue)
					{
						ddl.IntegralHeight = false;
						ddl.DropDownHeight = opts.height;
					}

					ctrl = ddl;
				}
				break;

				case Keywords.Keyword_ListBox:
				{
#if WINDOWS

					if (opts.rdonly.IsTrue())
						opts.addstyle |= WindowsAPI.LBS_NOSEL;

#endif
					var lb = new KeysharpListBox(opts.addstyle, opts.remstyle)
					{
						SelectionMode = opts.multiline.IsTrue() ? SelectionMode.MultiExtended : SelectionMode.One,
						Sorted = opts.sort.IsTrue()//Unsure how to make incremental search work.
					};
					lb.Items.AddRange(al.Cast<(object, object)>().Select(x => x.Item2).Select(x => opts.lowercase.IsTrue() ? x.Str().ToLower() : opts.uppercase.IsTrue() ? x.Str().ToUpper() : x.Str()).ToArray());

					if (opts.vscroll.HasValue)
						lb.ScrollAlwaysVisible = opts.vscroll.Value;

					if (opts.hscrollamt != int.MinValue)
						lb.HorizontalScrollbar = true;

					if (opts.hscrollamt > 0)
						lb.HorizontalExtent = opts.hscrollamt;

					if (opts.choose.Any())
					{
						if (lb.SelectionMode == SelectionMode.MultiExtended)
						{
							foreach (var ch in opts.choose)
								lb.SelectedIndices.Add(ch);
						}
						else if (lb.SelectionMode == SelectionMode.One)
							lb.SelectedIndex = opts.choose[0];
					}

					if (opts.tabstops.Any())
					{
						lb.UseCustomTabOffsets = true;
						lb.CustomTabOffsets.AddRange(opts.tabstops.ToArray());
					}

					if ((opts.addstyle & 0x100) == 0x100)
						lb.IntegralHeight = false;

					ctrl = lb;
				}
				break;

				case Keywords.Keyword_ListView:
				{
					//There is no way to preallocate memory with the "Count" option, so that is ignored.
					var lv = new KeysharpListView();
					lv.Columns.AddRange(al.Cast<(object, object)>().Select(x => x.Item2).Select(x => new ColumnHeader { Text = x.Str() }).ToArray());
					lv.CheckBoxes = opts.ischecked.HasValue && opts.ischecked.Value > 0;
					lv.GridLines = opts.grid.IsTrue();
					lv.LabelEdit = opts.rdonly.IsFalse();
					lv.View = opts.lvview ?? System.Windows.Forms.View.Details;

					if (lv.LabelEdit && !opts.wantf2.IsFalse())//Note that checking !IsFalse() is not the same as IsTrue().
						lv.KeyDown += Tv_Lv_KeyDown;

					if (opts.sort.IsTrue())
						lv.Sorting = SortOrder.Ascending;
					else if (opts.sortdesc.IsTrue())
						lv.Sorting = SortOrder.Descending;
					else
						lv.Sorting = SortOrder.None;

					if (opts.multiline.HasValue)
						lv.MultiSelect = opts.multiline.Value;

					if ((opts.addlvstyle & 0x10) == 0x10)
						lv.AllowColumnReorder = true;
					else if ((opts.remlvstyle & 0x10) == 0x10)
						lv.AllowColumnReorder = false;

					if ((opts.addlvstyle & 0x20) == 0x20)
						lv.FullRowSelect = true;
					else if ((opts.remlvstyle & 0x20) == 0x20)
						lv.FullRowSelect = false;

					if (opts.header.HasValue)
						lv.HeaderStyle = opts.header.IsFalse() ? ColumnHeaderStyle.None : ColumnHeaderStyle.Clickable;
					else if (opts.clickheader.HasValue)
						lv.HeaderStyle = opts.clickheader.IsFalse() ? ColumnHeaderStyle.Nonclickable : ColumnHeaderStyle.Clickable;
					else if (opts.sortheader.HasValue)
						lv.HeaderStyle = opts.sortheader.IsFalse() ? ColumnHeaderStyle.Nonclickable : ColumnHeaderStyle.Clickable;

					lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
					ctrl = lv;
				}
				break;

				case Keywords.Keyword_TreeView:
				{
#if WINDOWS
					var tv = new KeysharpTreeView(!opts.hscroll ? WindowsAPI.TVS_NOHSCROLL : 0, 0);
#else
					var tv = new KeysharpTreeView(0, 0);
#endif

					if (opts.buttons.HasValue)
						tv.ShowPlusMinus = opts.buttons.Value;

					tv.CheckBoxes = opts.ischecked.HasValue && opts.ischecked.Value != 0;
					tv.ShowLines = opts.lines ?? true;
					tv.LabelEdit = opts.rdonly.IsFalse();

					if (tv.LabelEdit && !opts.wantf2.IsFalse())//Note that checking !IsFalse() is not the same as IsTrue().
						tv.KeyDown += Tv_Lv_KeyDown;

					if (opts.ilid != long.MinValue && ImageLists.IL_Get(opts.ilid) is ImageList il)
						tv.ImageList = il;

					ctrl = tv;
				}
				break;

				case Keywords.Keyword_Link:
				{
					var linklabel = new KeysharpLinkLabel(text);
					ctrl = linklabel;
				}
				break;

				case Keywords.Keyword_Hotkey:
				{
					var hk = new HotkeyBox();

					if (opts.limit != int.MinValue)
						hk.Limit = (HotkeyBox.Limits)opts.limit;

					if (!string.IsNullOrEmpty(text))
						hk.SetText(text);

					ctrl = hk;
				}
				break;

				case Keywords.Keyword_DateTime:
				{
					var dtp = new KeysharpDateTimePicker();
					dtp.SetFormat(text);

					if (opts.rightj.IsTrue())
						dtp.DropDownAlign = LeftRightAlignment.Right;

					dtp.ShowUpDown = opts.dtopt1;
					dtp.CalendarForeColor = opts.c;//This will only have an effect if visual styles are disabled.

					if (opts.dtlow != DateTime.MinValue)
						dtp.MinDate = opts.dtlow;

					if (opts.dthigh != DateTime.MaxValue)
						dtp.MaxDate = opts.dthigh;

					if (opts.choosenone)
					{
						dtp.ShowCheckBox = true;
						dtp.Checked = false;
						var origfmt = dtp.Format;
						var origcustomfmt = dtp.CustomFormat;
						dtp.Format = DateTimePickerFormat.Custom;
						dtp.CustomFormat = " ";
						dtp.ValueChanged += (oo, ee) =>
						{
							if (!string.IsNullOrEmpty(origcustomfmt))
								dtp.CustomFormat = origcustomfmt;
							else
								dtp.Format = origfmt;
						};
					}
					else
					{
						dtp.ShowCheckBox = opts.dtopt2;
						dtp.Checked = true;

						if (opts.dtChoose >= dtp.MinDate && opts.dtChoose <= dtp.MaxDate)
							dtp.Value = opts.dtChoose;
					}

					ctrl = dtp;
				}
				break;

				case Keywords.Keyword_MonthCal:
				{
					var cal = new KeysharpMonthCalendar();

					if (opts.dtlow != DateTime.MinValue)//This causes a crash when you scroll past it.
						cal.MinDate = opts.dtlow;

					if (opts.dthigh != DateTime.MaxValue)
						cal.MaxDate = opts.dthigh;

					cal.ShowWeekNumbers = opts.opt4;
					cal.ShowTodayCircle = !opts.opt8;
					cal.ShowToday = !opts.opt16;
					cal.MaxSelectionCount = opts.datemultisel ? 31 : 1;

					if (opts.dtselstart > DateTime.MinValue && opts.dtselend < DateTime.MaxValue)
						cal.SelectionRange = new SelectionRange(opts.dtselstart, opts.dtselend);

					//Note that colors do not work here is visual styles are enabled.
					cal.TitleForeColor = opts.c;

					if (opts.bgcolor.HasValue)
						cal.TitleBackColor = opts.bgcolor.Value;

					ctrl = cal;
				}
				break;

				case Keywords.Keyword_Slider://Buddy controls are not supported.
				{
					var style = 0;
#if WINDOWS

					if (opts.tooltip)
						style |= WindowsAPI.TBS_TOOLTIPS;

					if (opts.thick != int.MinValue)
						style |= WindowsAPI.TBS_FIXEDLENGTH;

#endif
					var slider = new KeysharpTrackBar(style) { Orientation = opts.vertical ? Orientation.Vertical : Orientation.Horizontal };

					if (opts.nudlow.HasValue && opts.nudhigh.HasValue)
					{
						slider.Minimum = opts.nudlow.Value;
						slider.Maximum = opts.nudhigh.Value;
					}
					else//Set the same defaults as AHK.
					{
						slider.Minimum = 0;
						slider.Maximum = 100;
					}

					if (o != null)
						slider.Value = (int)Script.ForceLong(o);

					if (opts.center.IsTrue())
						slider.TickStyle = TickStyle.Both;
					else if (opts.leftj.IsTrue())
						slider.TickStyle = TickStyle.TopLeft;
					else if (opts.noticks.IsTrue())
						slider.TickStyle = TickStyle.None;

					if (opts.tickinterval != int.MinValue)
						slider.TickFrequency = opts.tickinterval;
					else
						slider.TickFrequency = slider.Maximum - slider.Minimum;

					if (opts.line != int.MinValue)
						slider.SmallChange = opts.line;

					if (opts.page != int.MinValue)
						slider.LargeChange = opts.page;

#if WINDOWS

					if (opts.thick != int.MinValue)
						_ = WindowsAPI.SendMessage(slider.Handle, WindowsAPI.TBM_SETTHUMBLENGTH, (uint)opts.thick, 0);

					if (opts.tooltip)
						_ = WindowsAPI.SendMessage(slider.Handle, WindowsAPI.TBM_SETTIPSIDE, (uint)opts.tooltipside, 0);

#endif
					slider.inverted = opts.invert.IsTrue();
					ctrl = slider;
				}
				break;

				case Keywords.Keyword_Progress:
				{
					var prg = new KeysharpProgressBar(opts.bgcolor.HasValue || opts.c != Control.DefaultForeColor
#if WINDOWS
													  , opts.vertical ? 0x04 : 0
#endif
													 );
					prg.Style = opts.smooth.IsTrue() ? ProgressBarStyle.Continuous : ProgressBarStyle.Blocks;

					if (opts.nudlow.HasValue)
						prg.Minimum = opts.nudlow.Value;

					if (opts.nudhigh.HasValue)
						prg.Maximum = opts.nudhigh.Value;

					if (o is long pos)
						prg.Value = (int)pos;

					if (opts.bgcolor.HasValue)
						prg.BackColor = opts.bgcolor.Value;

#if WINDOWS

					if (opts.vertical && opts.width == int.MinValue && opts.height == int.MinValue)
						(prg.Height, prg.Width) = (prg.Width, prg.Height);

#endif
					ctrl = prg;
				}
				break;

				case Keywords.Keyword_GroupBox:
				{
					ctrl = new KeysharpGroupBox() { };
				}
				break;

				case Keywords.Keyword_Tab:
				case Keywords.Keyword_Tab2:
				case Keywords.Keyword_Tab3:
				{
					var kstc = new KeysharpTabControl();//This will also support image lists just like TreeView for setting icons on tabs, instead of using SendMessage().
					kstc.TabPages.AddRange(al.Cast<(object, object)>().Select(x => x.Item2).Select(x => new TabPage(x.Str())).ToArray());

					if (opts.leftj.IsTrue())
						kstc.Alignment = System.Windows.Forms.TabAlignment.Left;
					else if (opts.rightj.IsTrue())
						kstc.Alignment = System.Windows.Forms.TabAlignment.Right;
					else if (opts.bottom)
						kstc.Alignment = System.Windows.Forms.TabAlignment.Bottom;
					else if (opts.top)
						kstc.Alignment = System.Windows.Forms.TabAlignment.Top;

					if (opts.buttons.HasValue)
						kstc.Appearance = TabAppearance.FlatButtons;

					if (opts.choose.Any())
						kstc.SelectedIndex = opts.choose[0];

					if (opts.wordwrap.HasValue)
						kstc.Multiline = opts.wordwrap.IsTrue();

					ctrl = kstc;
				}
				break;

				case Keywords.Keyword_StatusBar:
				{
					var ss = new KeysharpStatusStrip();
					StatusBar = ss;
					ss.AutoSize = false;
					ss.ImageScalingSize = new System.Drawing.Size((int)Math.Round(28 * dpiscale), (int)Math.Round(28 * dpiscale));
					ss.Dock = DockStyle.Bottom;//Docking must be used and must be on the bottom. Don't ever set form.AutoSize = true with this, they are incompatible.
					ss.SizingGrip = false;

					if (!string.IsNullOrEmpty(text))
					{
						var tsl = new KeysharpToolStripStatusLabel(text)
						{
							ForeColor = opts.c,//Contrary to the documentation, the foreground *can* be set.
							AutoSize = true,
							Name = $"AutoToolStripLabel{ss.Items.Count}"
						};

						if (opts.bgcolor.HasValue)
							tsl.BackColor = opts.bgcolor.Value;

						_ = ss.Items.Add(tsl);
					}

					ctrl = ss;
				}
				break;

				case Keywords.Keyword_ActiveX:
				{
				}
				break;

				case Keywords.Keyword_WebBrowser:
				{
					var web = new WebBrowser();
					web.Navigate(text);
					ctrl = web;
				}
				break;

				case Keywords.Keyword_Custom:
				{
				}
				break;
			}

			if (ctrl == null)
				return null;

			if (opts.name != null)
				ctrl.Name = opts.name;

			if (opts.wordwrap.HasValue)
				Reflections.SafeSetProperty(ctrl, "WordWrap", opts.wordwrap.Value);

			if (opts.thinborder.HasValue)
				Reflections.SafeSetProperty(ctrl, "BorderStyle", opts.thinborder.Value ? BorderStyle.FixedSingle : BorderStyle.None);

			if (opts.autosize.HasValue)
				Reflections.SafeSetProperty(ctrl, "AutoSize", opts.autosize.Value);

			if (text != null && !(ctrl is DateTimePicker) && !(ctrl is HotkeyBox) && !(ctrl is LinkLabel))
				ctrl.Text = text;

			if (!(ctrl is KeysharpStatusStrip))//Don't want status strip to have a margin, so it can be placed at the bottom of the form when autosize is true, and have it look exactly like it would if it were docked when autosize is false.
				ctrl.Margin = form.Margin;

			//ctrl.Padding = form.Padding;

			if (opts.visible.HasValue)
				ctrl.Visible = opts.visible.Value;

			if (opts.enabled.HasValue)
				ctrl.Enabled = opts.enabled.Value;

			ctrl.ForeColor = opts.c;

			if (opts.tabstop.HasValue)
				ctrl.TabStop = opts.tabstop.Value;

			if (opts.bgtrans)
				ctrl.BackColor = Color.Transparent;
			else if (opts.bgcolor.HasValue)
				ctrl.BackColor = opts.bgcolor.Value;

			if (opts.center.IsTrue())
				Reflections.SafeSetProperty(ctrl, "TextAlign", ContentAlignment.MiddleCenter);
			else if (opts.leftj.IsTrue())
				Reflections.SafeSetProperty(ctrl, "TextAlign", ContentAlignment.MiddleLeft);
			else if (opts.rightj.IsTrue())
				Reflections.SafeSetProperty(ctrl, "TextAlign", ContentAlignment.MiddleRight);

			holder = new GuiControl(this, ctrl, typeo);
			controls[ctrl.Handle.ToInt64()] = holder;
			var prevParent = LastContainer;
			var ctrlIsLabel = ctrl is KeysharpLabel;

			if (ctrl is KeysharpTabControl ktc)
			{
				if (ktc.TabPages.Count >= 0)
					holder.UseTab(1);//Will set this object's CurrentTab value, as well as the LastContainer values.

				if (opts.bgtrans)
					ktc.SetColor(Color.Transparent);
				else if (opts.bgcolor.HasValue)
					ktc.SetColor(opts.bgcolor.Value);
			}

			if (opts.altsubmit.HasValue)
				holder.AltSubmit = opts.altsubmit.Value;

			var fontpixels = GetFontPixels(ctrl.Font);// * dpiinv;
			var scaledPref = (double)ctrl.PreferredSize.Width;
			var w = scaledPref;
			var lastControl = LastControl;

			if (lastControl is KeysharpRadioButton && !(ctrl is KeysharpRadioButton))//Pop container if we've ended a radio group.
			{
				LastContainer = LastContainer.Parent;
				lastControl = LastControl;//Will retrieve the last control in the LastContainer we just assigned.
			}

			if (opts.autosize.IsTrue())
				goto heightdone;

			if (opts.wp != int.MinValue)
			{
				w = lastControl != null ? lastControl.Width + opts.wp : 0.0;
			}
			else if (opts.width != int.MinValue)
			{
				if (opts.width != -1)
					w = (int)Math.Round(dpiscale * opts.width);
			}
			else if (ctrl is KeysharpProgressBar kpb && ((kpb.AddStyle & 0x04) == 0x04))
				w = fontpixels * 2;
			else if (ctrl is ComboBox || ctrl is HotkeyBox || ctrl is ListBox || ctrl is NumericUpDown || ctrl is ProgressBar || ctrl is TextBox)
				w = fontpixels * 15;
			else if (ctrl is TrackBar trk)
				w = trk.Orientation == Orientation.Horizontal ? fontpixels * 2 : fontpixels * 15;//Documentation didn't mention a default for vertical trackbars, so just make it the same a vertical progress bar.
			else if (ctrl is GroupBox)
				w = fontpixels * 18;
			else if (ctrl is TabPage || ctrl is TabControl)
				w = (fontpixels * 30) + (3 * ctrl.Margin.Left);
			else if (ctrl is ListView || ctrl is TreeView || ctrl is DateTimePicker)//Documentaiton doesn't mention these, but IronAHK handled them this way, so leaving this here.
				w = fontpixels * 30;

			ctrl.Width = opts.width != int.MinValue ? (int)Math.Round(w) : Math.Max((int)w, (int)Math.Round(scaledPref));

			if (opts.hp != int.MinValue)
			{
				ctrl.Height = lastControl != null ? lastControl.Height + opts.hp : 0;
			}
			else
			{
				if (opts.height != int.MinValue)
				{
					if (opts.height != -1)
						ctrl.Height = (int)Math.Round(dpiscale * opts.height);
				}
				else
				{
					var r = 0;

					if (opts.rows != float.MinValue)
						r = (int)Math.Round(opts.rows);
					else if (ctrl is ComboBox || ctrl is ListBox)
						r = 3;
					else if (ctrl is ListView || ctrl is TreeView || (ctrl is KeysharpProgressBar kpb2 && ((kpb2.AddStyle & 0x04) == 0x04)))
						r = 5;
					else if (ctrl is GroupBox || ctrl is ProgressBar)
						r = 2;
					else if (ctrl is TextBox tb)
						r = tb.Multiline ? 3 : 1;
					else if (ctrl is DateTimePicker || ctrl is HotkeyBox)
						r = 1;
					else if (ctrl is TabPage || ctrl is TabControl)
						r = 10;

					var fontRows = (int)(Math.Round(fontpixels + 0.5) * r);//This is a rough attempt to make text boxes tall enough to show the requested number of lines without having the scrollbars appear unnecessarily.
					var defheight = fontRows;//AHK used external leading, but just use fontpixels here because it's close enough.

					if (ctrl is ComboBox cmb)
					{
						cmb.MaxDropDownItems = r;
					}
					else if (ctrl is ListBox lb)
					{
						lb.Height = (int)Math.Round(lb.ItemHeight * r * dpiinv) + (lb.Height - lb.ClientSize.Height) + lb.Margin.Bottom;
					}
					else if (ctrl is TreeView tv)
					{
						tv.Height = (int)Math.Round(tv.ItemHeight * r * dpiinv) - tv.Margin.Bottom;//For some reason, TreeView doesn't appear to need to have DPI scaling applied, and also is a bit too large, so we subtract the margin.
					}
					else if (ctrl is GroupBox gb)
					{
						gb.Height = defheight + ((gb.Margin.Top + gb.Margin.Bottom) * (2 + ((int)(r + 1.5) - 2)));//This odd formula comes straight from the AHK source.
					}
					else if (ctrl is ListView lv)
					{
						lv.Height = defheight + ((lv.Margin.Top + lv.Margin.Bottom) * (2 + ((int)(r + 1.5) - 2)));//ListView doesn't have an ItemHeight property, so attempt to compute here in the same way GroupBox is done above.
					}
					else if (ctrl is TabControl tc2)
					{
						tc2.Height = defheight + (int)Math.Round((tc2.Margin.Top + tc2.Margin.Bottom) *  (2.0 + ((int)(r + 1.5) - 1)));//Same here, but -1.
					}
					else
					{
						if (opts.rows == float.MinValue) //Neither r or h were specified.
						{
							if (ctrl is TrackBar trk && opts.thick == int.MinValue)//Separate check for TrackBar because the documentation specifies it in pixels. Skip this if thickness has been specified.
							{
								ctrl.Height = trk.Orientation == Orientation.Horizontal ? 30 : (int)Math.Round(5 * fontpixels);
								goto heightdone;
							}
							else if (ctrl is KeysharpLabel lbl)
							{
								lbl.MaximumSize = new Size(lbl.Width, 0);//This enforces wrapping at the specified width.
								lbl.AutoSize = true;
								goto heightdone;
							}
						}

						if (r > 1)
						{
							ctrl.Height = ctrl.Height - ctrl.ClientSize.Height + fontRows;
						}
						else
						{
							var ctrlheight = ctrl.PreferredSize.Height;
							ctrlheight += ctrl.Height - ctrl.ClientSize.Height;//Account for the border.
							ctrl.Height = ctrlheight;
						}
					}
				}
			}

			heightdone:
			Point loc;

			if (lastControl != null)
			{
				var xoffset = (double)lastControl.Left;
				var yoffset = (double)lastControl.Top;

				if (opts.xplus != int.MinValue)
					xoffset += lastControl.Width + (opts.xplus * dpiscale);
				else if (opts.xp != int.MinValue)
					xoffset += opts.xp * dpiscale;
				else if (opts.xm != int.MinValue)
					xoffset = form.Margin.Left + (opts.xm * dpiscale);
				else if (opts.xs != int.MinValue)
					xoffset = Section.X + (opts.xs * dpiscale);
				else
					xoffset = int.MinValue;

				if (opts.yplus != int.MinValue)
					yoffset += lastControl.Height + (opts.yplus * dpiscale);
				else if (opts.yp != int.MinValue)
					yoffset += opts.yp * dpiscale;
				else if (opts.ym != int.MinValue)
					yoffset = form.Margin.Top + (opts.ym * dpiscale);
				else if (opts.ys != int.MinValue)
					yoffset = Section.Y + (opts.ys * dpiscale);
				else
					yoffset = int.MinValue;

				loc = new Point((int)Math.Round(xoffset), (int)Math.Round(yoffset));
			}
			else
				loc = new Point(int.MinValue, int.MinValue);

			//Explicitly positioned values override everything.
			if (opts.x != int.MinValue)
				loc.X = (int)Math.Round(opts.x * dpiscale);

			if (opts.y != int.MinValue)
				loc.Y = (int)Math.Round(opts.y * dpiscale);

			//Note we check DockStyle here because if the previous control was docked to a side, then we can't really use its location as a reference to base this control's location off of.
			if (ctrl is KeysharpStatusStrip ksss)//Need to figure out how to do this without resizing all tab controls on every add. Maybe at the end before show(), and also on every add after show()?//TODO
			{
			}
			else if (loc.X != int.MinValue && loc.Y != int.MinValue)//If both x and y were specified, that takes precedence over everything else.
			{
				ctrl.Location = loc;
			}
			else if (lastControl != null && lastControl.Dock == DockStyle.None && loc.X == int.MinValue && loc.Y == int.MinValue)
			{
				var templast = opts.group && lastControl.Parent is Panel panel ? panel : lastControl;
				ctrl.Location = new Point(templast.Location.X, templast.Location.Y + templast.Height + form.Margin.Bottom);
			}
			else if (lastControl != null && lastControl.Dock == DockStyle.None && loc.X == int.MinValue)//Depends on loc.Y not being MinValue, but that's not reliable based on the logic changes above.
				ctrl.Location = new Point(lastControl.Left + lastControl.Width + lastControl.Margin.Right + ctrl.Margin.Left, loc.Y);
			else if (lastControl != null && lastControl.Dock == DockStyle.None && loc.Y == int.MinValue)//Same but for loc.X.
			{
				ctrl.Location = new Point(loc.X, lastControl.Top + lastControl.Height + lastControl.Margin.Bottom + ctrl.Margin.Top);
			}
			else//Final fallback when nothing else has worked.
			{
				var top = (double)prevParent.Margin.Top;

				if (prevParent is Form f && f.MainMenuStrip != null)
					top += f.MainMenuStrip.Height;

				if (loc.Y == int.MinValue && LastContainer is GroupBox gblast)
				{
					//Top needs to be manually adjusted when the container is a GroupBox, we're adding the first control, and they haven't explicitly specified a Y coordinate.
					if (gblast.Controls.Count == 0)
						top += gblast.Margin.Top + gblast.Padding.Bottom;
				}

				ctrl.Location = new Point(opts.x != int.MinValue ? opts.x : prevParent.Margin.Left,
										  opts.y != int.MinValue ? opts.y : (int)Math.Round(top));
			}

			if (ctrl is TabControl tc && CurrentTab is TabPage currtp)
			{
				prevParent.TagAndAdd(holder);

				if (prevParent != form)
					ctrl.Size = new Size(Math.Min(prevParent.Width - (2 * prevParent.Margin.Right), ctrl.Right), Math.Min(prevParent.Height - (2 * prevParent.Margin.Top), ctrl.Bottom));

				LastContainer = currtp;
			}
			else if (ctrl is KeysharpRadioButton krb)
			{
				if (lastControl == null || !(lastControl is KeysharpRadioButton) || opts.group)
				{
					var panel = new Panel();
					var parent = LastContainer;
					//panel.BorderStyle = BorderStyle.FixedSingle;//For debugging so we can see where the panel is.
					panel.Location = new Point(Math.Max(parent.Margin.Left, ctrl.Left), Math.Max(parent.Margin.Top, ctrl.Top));
					parent.TagAndAdd(panel);
					ctrl.Location = new Point(panel.Margin.Left, panel.Margin.Top);
					panel.Size = new System.Drawing.Size(ctrl.Width + panel.Margin.Left + panel.Margin.Right, ctrl.Height + panel.Margin.Top + panel.Margin.Bottom);
					panel.AutoSize = true;
					panel.TagAndAdd(holder);
					LastContainer = panel;
				}
				else if (LastContainer is Panel pnl)
				{
					pnl.TagAndAdd(holder);
				}

				krb.Checked = opts.ischecked.HasValue && opts.ischecked.Value > 0;
			}
			else if (ctrl is GroupBox gb)
			{
				LastContainer.TagAndAdd(holder);
				LastContainer = gb;
			}
			else if (ctrl is KeysharpStatusStrip ksss2)
			{
				form.TagAndAdd(holder);//For status bars, don't add to whatever container we were on, instead always add to the form.
			}
			else
			{
				LastContainer.TagAndAdd(holder);
			}

			if (ctrl is KeysharpPictureBox pbox)
			{
				if (ImageHelper.LoadImage(text, opts.width, opts.height, opts.iconnumber).Item1 is Bitmap bmp)
				{
					if (pbox.SizeMode == PictureBoxSizeMode.Zoom)
					{
						var ratio = bmp.Height != 0 ? (double)bmp.Width / bmp.Height : 1;

						if (ratio == 0)
							ratio = 1;

						if (opts.width < 0)
							pbox.Width = (int)(pbox.Height * ratio);
						else
							pbox.Height = (int)(pbox.Width / ratio);
					}

					pbox.Image = bmp;
					//pbox.BackgroundImage = bmp;
				}
			}

			if (form.BeenShown)//See above for a description of this logic.
			{
				//If it's been shown and is contained anywhere within the hierarchy of a TabControl, then resize
				//every TabControl in the chain.
				foreach (var partc in ctrl.FindParents<KeysharpTabControl>())
					partc.AdjustSize(dpiscale);
			}

			if (opts.section)
				Section = ctrl.Location;

			return holder;
		}

		public GuiControl AddActiveX(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_ActiveX, obj0, obj1);

		public GuiControl AddButton(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_Button, obj0, obj1);

		public GuiControl AddCheckbox(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_CheckBox, obj0, obj1);

		public GuiControl AddComboBox(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_ComboBox, obj0, obj1);

		public GuiControl AddCustom(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_Custom, obj0, obj1);

		public GuiControl AddDateTime(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_DateTime, obj0, obj1);

		public GuiControl AddDropDownList(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_DropDownList, obj0, obj1);

		public GuiControl AddEdit(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_Edit, obj0, obj1);

		public GuiControl AddGroupBox(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_GroupBox, obj0, obj1);

		public GuiControl AddHotKey(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_Hotkey, obj0, obj1);

		public GuiControl AddLink(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_Link, obj0, obj1);

		public GuiControl AddListBox(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_ListBox, obj0, obj1);

		public GuiControl AddListView(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_ListView, obj0, obj1);

		public GuiControl AddMonthCal(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_MonthCal, obj0, obj1);

		public GuiControl AddPicture(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_Picture, obj0, obj1);

		public GuiControl AddProgress(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_Progress, obj0, obj1);

		public GuiControl AddRadio(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_Radio, obj0, obj1);

		public GuiControl AddSlider(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_Slider, obj0, obj1);

		public GuiControl AddStatusBar(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_StatusBar, obj0, obj1);

		public GuiControl AddTab(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_Tab, obj0, obj1);

		public GuiControl AddText(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_Text, obj0, obj1);

		public GuiControl AddTreeView(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_TreeView, obj0, obj1);

		public GuiControl AddUpDown(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_UpDown, obj0, obj1);

		public GuiControl AddWebBrowser(object obj0 = null, object obj1 = null) => Add(Keywords.Keyword_WebBrowser, obj0, obj1);

		public void Destroy()
		{
			closingFromDestroy = true;

			//Do not close the window if the program is already exiting because it will throw
			//an enumeration modified exception because Winforms is internally already iterating over
			//all open windows to close them.
			if (!Script.IsMainWindowClosing)
				form.Close();
		}

		public void Flash(object obj)
		{
#if WINDOWS
			_ = WindowsAPI.FlashWindow(form.Handle, obj.Ab(true));
#endif
		}

		public void GetClientPos(ref object x, ref object y, ref object width, ref object height) => GuiControl.GetClientPos(form, dpiscaling, ref x, ref y, ref width, ref height);

		public IEnumerator<(object, object)> GetEnumerator() => new MapKeyValueIterator(controls);

		public void GetPos(ref object x, ref object y, ref object width, ref object height) => GuiControl.GetPos(form, dpiscaling, ref x, ref y, ref width, ref height);

		public void Hide() => form.Hide();

		public void Maximize() => form.WindowState = FormWindowState.Maximized;

		public void Minimize() => form.WindowState = FormWindowState.Minimized;

		public void Move(object obj0 = null, object obj1 = null, object obj2 = null, object obj3 = null)
		{
			var x = obj0.Ai();
			var y = obj1.Ai();
			var width = obj2.Ai();
			var height = obj3.Ai();
			var scale = !dpiscaling ? 1.0 : Accessors.A_ScaledScreenDPI;
			form.Top = (int)Math.Round(y * scale);
			form.Left = (int)Math.Round(x * scale);
			form.Width = (int)Math.Round(width * scale);
			form.Height = (int)Math.Round(height * scale);
		}

		public void OnEvent(object obj0, object obj1, object obj2 = null)
		{
			var e = obj0.As();
			var h = obj1;
			var i = obj2.Al(1);
			e = e.ToLower();
			var del = Function.GetFuncObj(h, form.eventObj, true);

			if (e == "close")
			{
				if (closedHandlers == null)
					closedHandlers = new List<IFuncObj>();

				closedHandlers.ModifyEventHandlers(del, i);
			}
			else if (e == "contextmenu")
			{
				if (contextMenuChangedHandlers == null)
					contextMenuChangedHandlers = new List<IFuncObj>();

				contextMenuChangedHandlers.ModifyEventHandlers(del, i);
			}
			else if (e == "dropfiles")
			{
				if (dropFilesHandlers == null)
					dropFilesHandlers = new List<IFuncObj>();

				dropFilesHandlers.ModifyEventHandlers(del, i);
			}
			else if (e == "escape")
			{
				if (escapeHandlers == null)
					escapeHandlers = new List<IFuncObj>();

				escapeHandlers.ModifyEventHandlers(del, i);
			}
			else if (e == "size")
			{
				if (sizeHandlers == null)
					sizeHandlers = new List<IFuncObj>();

				sizeHandlers.ModifyEventHandlers(del, i);
			}
		}

		public void Opt(object obj)
		{
			var opt = obj.As();

			foreach (var split in Options.ParseOptions(opt))
			{
				var str = split.Substring(1);

				if (str.Length > 0)
				{
					var val = "";

					if (str.StartsWith("MinSize"))
					{
						if (split[0] == '+')
							val = str.Substring(7);

						if (showOptionsDkt.TryGetValue("MinSize", out var func))
							func(this, val);
					}
					else if (str.StartsWith("MaxSize"))
					{
						if (split[0] == '+')
							val = str.Substring(7);

						if (showOptionsDkt.TryGetValue("MaxSize", out var func))
							func(this, val);
					}
					else if (str.StartsWith("Owner"))
					{
						if (split[0] == '+')
							val = str.Substring(5);

						if (showOptionsDkt.TryGetValue("Owner", out var func))
							func(this, val);
					}
					else if (str.StartsWith("Parent"))
					{
						if (split[0] == '+')
							val = str.Substring(6);

						if (showOptionsDkt.TryGetValue("Owner", out var func))
							func(this, val);
					}
					else if (showOptionsDkt.TryGetValue(str, out var func))//Used +/-.
					{
						if (split[0] == '+')
							func(this, true);
						else if (split[0] == '-')
							func(this, false);
					}
					else if (showOptionsDkt.TryGetValue(split, out var func2))//No +/-, so just try as is.
					{
						func2(this, true);//No +/- so just assume true.
					}
					else//Special style, windows only. Need to figure out how to make this cross platform.//TODO
					{
#if WINDOWS
						var temp = 0;
						var handle = this.form.Handle;

						if (Options.TryParse(split, "+E", ref temp))
						{
							_ = WindowsAPI.SetWindowLongPtr(handle, WindowsAPI.GWL_EXSTYLE, new IntPtr(WindowsAPI.GetWindowLongPtr(handle, WindowsAPI.GWL_EXSTYLE).ToInt64() | (long)temp));
						}
						else if (Options.TryParse(split, "E", ref temp))
						{
							_ = WindowsAPI.SetWindowLongPtr(handle, WindowsAPI.GWL_EXSTYLE, new IntPtr(WindowsAPI.GetWindowLongPtr(handle, WindowsAPI.GWL_EXSTYLE).ToInt64() | (long)temp));
						}
						else if (Options.TryParse(split, "-E", ref temp))
						{
							_ = WindowsAPI.SetWindowLongPtr(handle, WindowsAPI.GWL_EXSTYLE, dwNewLong: new IntPtr(WindowsAPI.GetWindowLongPtr(handle, WindowsAPI.GWL_EXSTYLE).ToInt64() & ~temp));
						}
						else if (Options.TryParse(split, "-", ref temp))
						{
							_ = WindowsAPI.SetWindowLongPtr(handle, WindowsAPI.GWL_STYLE, new IntPtr(WindowsAPI.GetWindowLongPtr(handle, WindowsAPI.GWL_STYLE).ToInt64() & ~temp));
						}
						else if (Options.TryParse(split, "+", ref temp))
						{
							_ = WindowsAPI.SetWindowLongPtr(handle, WindowsAPI.GWL_STYLE, new IntPtr(WindowsAPI.GetWindowLongPtr(handle, WindowsAPI.GWL_STYLE).ToInt64() | (long)temp));
						}
						else if (Options.TryParse(split, "", ref temp))
						{
							_ = WindowsAPI.SetWindowLongPtr(handle, WindowsAPI.GWL_STYLE, new IntPtr(WindowsAPI.GetWindowLongPtr(handle, WindowsAPI.GWL_STYLE).ToInt64() | (long)temp));
						}

#endif
					}
				}
			}
		}

		public void Restore() => form.WindowState = FormWindowState.Normal;

		public void SetFont(object obj0 = null, object obj1 = null) => form.SetFont(obj0, obj1);

		public void Show(object obj = null)
		{
			var s = obj.As();
			bool /*center = false, cX = false, cY = false,*/ auto = false, min = false, max = false, restore = false, hide = false;
			int?[] pos = { null, null, null, null };
			var dpiscale = !dpiscaling ? 1.0 : Accessors.A_ScaledScreenDPI;

			foreach (var opt in Options.ParseOptions(s))
			{
				var mode = opt.ToLowerInvariant();
				var select = -1;

				switch (mode[0])
				{
					case 'w': select = 0; break;

					case 'h': select = 1; break;

					case 'x': select = 2; break;

					case 'y': select = 3; break;
				}

				if (select == -1)
				{
					switch (mode)
					{
						//case Core.Keyword_Center: center = true; break;
						case Keywords.Keyword_AutoSize: auto = true; break;

						case Keywords.Keyword_Maximize: max = true; break;

						case Keywords.Keyword_Minimize: min = true; break;

						case Keywords.Keyword_Restore:
							form.showWithoutActivation = false;
							restore = true;
							break;

						case Keywords.Keyword_NoActivate:
						case Keywords.Keyword_NA:
							form.showWithoutActivation = true;
							restore = true;
							break;

						case Keywords.Keyword_Hide: hide = true; break;
					}
				}
				else
				{
					var modeval = mode.AsSpan(1);

					if (modeval.Equals(Keywords.Keyword_Center, StringComparison.OrdinalIgnoreCase))
					{
						//if (select == 2)
						//  cX = true;
						//else
						//  cY = true;
					}
					else if (modeval.Length != 0 && int.TryParse(modeval, out var n))
						pos[select] = n;
				}
			}

			ResizeTabControls();
			var status = form.Controls.OfType<KeysharpStatusStrip>().ToArray();
			(int, int) FixStatusStrip(KeysharpStatusStrip ss)
			{
				var maxx = 0;
				var maxy = 0;

				foreach (Control ctrl in form.Controls)
				{
					if (ctrl != ss)
					{
						var yval = ctrl.Bottom;

						if (yval > maxy)
							maxy = yval;

						var xval = ctrl.Right;

						if (xval > maxx)
							maxx = xval;
					}
				}

				return (maxx, maxy);
			}

			if (auto || (!form.BeenShown && pos[0] == null && pos[1] == null))//The caluclations in this block are not exact, but are as close as we can possibly get in a generic way.
			{
				//AHK always autosizes on first show when no dimensions are specified.
				KeysharpStatusStrip ss = null;
				var ssHeight = 0;

				if (status.Length > 0)
				{
					ss = status[0];
					ssHeight = ss.Height;
				}

				var (maxx, maxy) = FixStatusStrip(ss);
				form.ClientSize = new Size(maxx + form.Margin.Left,
										   maxy + ssHeight + form.Margin.Bottom);//Need to manually include the height of the status strip when it's docked.
			}
			else
			{
				var size = form.BeenShown ? form.Size : new Size(800, 500);//Using this size because PreferredSize is so small it just shows the title bar.

				if (pos[0] != null)//Dimensions must be scaled by DPI.//TODO
					size.Width = (int)pos[0];

				if (pos[1] != null)
					size.Height = (int)pos[1];

				form.Size = size;
			}

			var location = form.BeenShown ? form.Location : new Point();
			var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;

			if (pos[2] != null)//Strangely, the position does not need to be scaled by DPI.
				location.X = (int)pos[2];
			else if (/*cX || center ||*/ !form.BeenShown)
				location.X = ((screen.Width - form.Size.Width) / 2) + screen.X;

			if (pos[3] != null)
				location.Y = (int)pos[3];
			else if (/*cY || center ||*/ !form.BeenShown)
				location.Y = ((screen.Height - form.Size.Height) / 2) + screen.Y;

			form.StartPosition = FormStartPosition.Manual;
			form.Location = location;

			if (hide)
				form.Hide();
			else
				form.Show();

			if (min)
				form.WindowState = FormWindowState.Minimized;
			else if (max)
				form.WindowState = FormWindowState.Maximized;
			else if (restore)
				form.WindowState = FormWindowState.Normal;

			form.Update();//Required for the very first state of the form to always be displayed.
		}

		public Map Submit(object obj = null)
		{
			var hide = obj.Ab(true);
			var panels = new HashSet<Panel>();
			var ctrls = form.Controls.Flatten();
			var dkt = new Dictionary<object, object>();

			foreach (Control control in form.Controls)
			{
				if (control.Name != "" && control.GetGuiControl() is GuiControl guictrl)
				{
					if (control is TextBox || control is DateTimePicker || control is MonthCalendar)//Just use value because it's the same and consolidates the formatting in one place, despite being slightly slower.
						dkt[control.Name] = guictrl.Value;
					else if (control is RichTextBox)
						dkt[control.Name] = !guictrl.AltSubmit ? guictrl.Value : guictrl.RichText;
					else if (control is NumericUpDown nud)
						dkt[nud.Name] = (double)nud.Value;
					else if (control is CheckBox cb)
						dkt[cb.Name] = cb.Checked ? 1L : 0L;
					else if (control is TabControl tc)
						dkt[tc.Name] = !guictrl.AltSubmit ? tc.SelectedTab != null ? tc.SelectedTab.Text : "" : (long)(tc.SelectedIndex + 1);
					else if (control is ComboBox cmb)
						dkt[cmb.Name] = !guictrl.AltSubmit || cmb.Items.IndexOf(cmb.Text) == -1 ? cmb.Text : (long)(cmb.SelectedIndex + 1);
					else if (control is TrackBar tb)
						dkt[tb.Name] = tb.Value;
					else if (control is ListBox lb)
					{
						dkt[lb.Name] = !guictrl.AltSubmit
									   ? guictrl.Value
									   : lb.SelectionMode == SelectionMode.One
									   ? lb.SelectedItem as string ?? ""
									   : new Array(lb.SelectedItems.Cast<object>().Where(xx => xx is string).Select(x => x as string).ToList());
					}
					else if (control is RadioButton rb)//This is supposed to do something special if it's part of a group, but unsure how to determine that.
					{
						if (rb.Parent is Panel pnl && !panels.Contains(pnl))
						{
							_ = panels.Add(pnl);
							var rbs = pnl.Controls.Cast<Control>().Where(pc => pc is RadioButton pcrb).Cast<RadioButton>().ToList();
							var named = rbs.Where(rr => rr.Name != "").ToList();

							if (named.Count == 1)
							{
								for (var i = 0; i < rbs.Count(); i++)
								{
									if (rbs[i].Checked)
									{
										dkt[named[0].Name] = (long)(i + 1);
										goto DoneAssigning;
									}
								}

								dkt[named[0].Name] = 0L;
							}
						}
					}
				}
			}

			DoneAssigning:

			if (hide)
				Hide();

			return new Map(dkt);
		}
		public void UseGroup(object obj0 = null)
		{
			if (obj0 is GuiControl gctrl && gctrl.Control is GroupBox gb)
				LastContainer = gb;
			else
				LastContainer = form;
		}
		IEnumerator IEnumerable.GetEnumerator() => __Enum();
		internal static bool AnyExistingVisibleWindows() => allGuiHwnds.Values.Any(g => g.form != Keysharp.Scripting.Script.mainWindow && g.form.Visible);

		internal static void DestroyAll()
		{
			//Destroy everything but the main window, which will destroy itself.
			foreach (var gui in allGuiHwnds.Values.Where(g => g.form != Keysharp.Scripting.Script.mainWindow).ToArray())
			{
				try
				{
					gui.Destroy();
				}
				catch
				{
				}
			}

			allGuiHwnds.Clear();
		}

		internal static float GetFontPixels(Font font) => font.GetHeight((float)Accessors.A_ScreenDPI);
		internal static bool IsGuiType(Type type) => GuiTypes.Any(t => t.IsAssignableFrom(type));
		internal static GuiOptions ParseOpt(string type, string text, string optionsstr)
		{
			var options = new GuiOptions();
			var opts = Options.ParseOptions(optionsstr);

			if (type == "monthcal" && !string.IsNullOrEmpty(text))
			{
				Conversions.ParseRange(text, out options.dtselstart, out options.dtselend);

				if (options.dtselstart == DateTime.MinValue)
					options.dtselstart = options.dtselend;

				if (options.dtselend == DateTime.MaxValue)
					options.dtselend = options.dtselstart;

				if (options.dtselstart != DateTime.MinValue && options.dtselend != DateTime.MaxValue &&
						(options.dtselend - options.dtselstart).TotalDays > 1)
					options.datemultisel = true;
			}

			foreach (var opt in opts)
			{
				var tempbool = false;
				var temp = 0;
				var tempcolor = Color.Empty;
				var tempstr = "";

				if (type == "datetime")
				{
					if (Options.TryParseDateTime(opt, "Choose", "yyyyMMdd", ref options.dtChoose)) { continue; }
					else if (string.Compare(opt, "ChooseNone", true) == 0) { options.choosenone = true; continue; }
					else if (opt == "1") { options.dtopt1 = true; continue; }
					else if (opt == "2") { options.dtopt2 = true; continue; }
				}
				else if (type == "monthcal")
				{
					if (Options.TryParse(opt, "Multi", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.datemultisel = true; continue; }
				}
				else
				{
					if (Options.TryParse(opt, "Multi", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.multiline = tempbool; continue; }
				}

				if (Options.TryParse(opt, "r", ref options.rows)) { }
				else if (Options.TryParse(opt, "w", ref options.width)) { }
				else if (Options.TryParse(opt, "h", ref options.height)) { }
				else if (Options.TryParse(opt, "x+", ref options.xplus)) { }
				else if (Options.TryParse(opt, "y+", ref options.yplus)) { }
				//else if (string.Compare(opt, "x+m", true) == 0) { options.xplusm = true; }
				//else if (string.Compare(opt, "y+m", true) == 0) { options.yplusm = true; }
				else if (Options.TryParse(opt, "x", ref options.x)) { }
				else if (Options.TryParse(opt, "y", ref options.y)) { }
				else if (Options.TryParse(opt, "t", ref options.t)) { options.tabstops.Add(options.t); }
				else if (Options.TryParse(opt, "Redraw", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.redraw = tempbool; }
				//Checkbox.
				else if (string.Compare(opt, "Check3", true) == 0) { options.check3 = true; }//Needs to come before any option starting with a 'c'.
				else if (string.Compare(opt, "CheckedGray ", true) == 0) { options.checkedgray = true; }
				else if (Options.TryParse(opt, "Checked", ref temp, StringComparison.OrdinalIgnoreCase, true, 1)) { options.ischecked = temp; }
				else if (Options.TryParse(opt, "Center", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.center = tempbool; }
				else if (Options.TryParseString(opt, "Range", ref options.nudrange))
				{
					if (type == "datetime" || type == "monthcal")
					{
						Conversions.ParseRange(options.nudrange, out options.dtlow, out options.dthigh);
					}
					else if (type == "updown" || type == "slider" || type == "progress")
					{
						var splits = options.nudrange.Split('-', StringSplitOptions.None);
						var vals = Conversions.ParseRange(splits);

						if (vals.Count > 0)
							options.nudlow = vals[0];

						if (vals.Count > 1)
							options.nudhigh = vals[1];
					}
				}
				else if (Options.TryParse(opt, "Choose", ref options.ddlchoose)) { options.ddlchoose--; options.choose.Add(options.ddlchoose); }
				//
				else if (Options.TryParse(opt, "c", ref options.c)) { }
				else if (string.Compare(opt, "Vertical", true) == 0) { options.vertical = true; }
				else if (Options.TryParseString(opt, "v", ref options.name)) { }
				else if (Options.TryParse(opt, "Disabled", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.enabled = !tempbool; }
				else if (Options.TryParse(opt, "Hidden", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.visible = !tempbool; }
				else if (Options.TryParse(opt, "Autosize", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.autosize = tempbool; }
				else if (Options.TryParse(opt, "wp", ref options.wp, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "hp", ref options.hp, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "xp", ref options.xp, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "yp", ref options.yp, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "xm", ref options.xm, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "ym", ref options.ym, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "xs", ref options.xs, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "ys", ref options.ys, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "AltSubmit", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.altsubmit = tempbool; }
				else if (Options.TryParse(opt, "Left", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.leftj = tempbool; }
				else if (Options.TryParse(opt, "Right", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.rightj = tempbool; }
				else if (string.Compare(opt, "Section", true) == 0) { options.section = true; }
				else if (Options.TryParse(opt, "Tabstop", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.tabstop = tempbool; }
				else if (Options.TryParse(opt, "Wrap", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.wordwrap = tempbool; }
				else if (Options.TryParse(opt, "VScroll", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.vscroll = tempbool; }
				else if (string.Compare(opt, "-HScroll", true) == 0) { options.hscroll = false; }
				else if (Options.TryParse(opt, "HScroll", ref options.hscrollamt, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "Increment", ref temp)) { options.nudinc = temp; }
				else if (Options.TryParse(opt, "Hex", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.hex = tempbool; }
				else if (string.Compare(opt, "BackgroundTrans", true) == 0) { options.bgtrans = true; }
				else if (string.Compare(opt, "-Background", true) == 0) { options.bgcolor = Control.DefaultBackColor; }
				else if (string.Compare(opt, "Background", true) == 0) { options.bgcolor = Control.DefaultBackColor; }
				else if (string.Compare(opt, "BackgroundDefault", true) == 0) { options.bgcolor = Control.DefaultBackColor; }
				else if (Options.TryParse(opt, "Background", ref tempcolor, StringComparison.OrdinalIgnoreCase, true)) { options.bgcolor = tempcolor; }
				else if (Options.TryParse(opt, "Border", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.thinborder = tempbool; }
				//Control specific.
				//Edit.
				else if (Options.TryParse(opt, "limit", ref options.limit, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "Lowercase", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.lowercase = tempbool; }
				else if (Options.TryParse(opt, "Uppercase", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.uppercase = tempbool; }
				else if (Options.TryParse(opt, "Number", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.number = tempbool; }
				else if (Options.TryParseString(opt, "Password", ref options.pwdch, StringComparison.OrdinalIgnoreCase)) { options.pwd = true; }
				else if (Options.TryParse(opt, "ReadOnly", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.rdonly = tempbool; }
				else if (Options.TryParse(opt, "WantCtrlA", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.wantctrla = tempbool; }
				else if (Options.TryParse(opt, "WantReturn", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.wantreturn = tempbool; }
				else if (Options.TryParse(opt, "WantTab", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.wanttab = tempbool; }
				//GrouBox.
				else if (string.Compare(opt, "Group", true) == 0) { options.group = true; }
				//UpDown.
				else if (string.Compare(opt, "Horz", true) == 0) { options.nudhorz = true; }
				else if (string.Compare(opt, "Left", true) == 0) { options.nudleft = true; }
				//16
				//0x80
				//None unit inc/dec
				//Button.
				else if (Options.TryParse(opt, "Default", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.btndef = tempbool; }
				//DropDownList.
				else if (Options.TryParse(opt, "Sort", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.sort = tempbool; }
				//ComboBox.
				else if (Options.TryParse(opt, "Simple", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.cmbsimple = tempbool; }
				else if (Options.TryParse(opt, "Invert", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.invert = tempbool; }
				else if (Options.TryParse(opt, "Line", ref options.line)) { }
				else if (Options.TryParse(opt, "NoTicks", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.noticks = tempbool; }
				else if (Options.TryParse(opt, "Page", ref options.page)) { }
				else if (Options.TryParse(opt, "Thick", ref options.thick)) { }
				else if (Options.TryParse(opt, "TickInterval", ref options.tickinterval)) { }
				else if (string.Compare(opt, "ToolTip", true) == 0) { options.tooltip = true; }
				else if (string.Compare(opt, "ToolTipTop", true) == 0) { options.tooltipside = 0; }
				else if (string.Compare(opt, "ToolTipLeft", true) == 0) { options.tooltipside = 1; }
				else if (string.Compare(opt, "ToolTipBottom", true) == 0) { options.tooltipside = 2; }
				else if (string.Compare(opt, "ToolTipRight", true) == 0) { options.tooltipside = 3; }
				else if (Options.TryParse(opt, "Smooth", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.smooth = tempbool; }
				else if (Options.TryParse(opt, "Buttons", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.buttons = tempbool; }
				else if (string.Compare(opt, "Bottom", true) == 0) { options.bottom = true; }
				else if (string.Compare(opt, "Top", true) == 0) { options.top = true; }
				else if (Options.TryParse(opt, "ImageList", ref options.ilid)) { }
				else if (Options.TryParse(opt, "Lines", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.lines = tempbool; }
				else if (Options.TryParse(opt, "WantF2", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.wantf2 = tempbool; }
				//ListView.
				else if (Options.TryParse(opt, "SortDesc", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.sortdesc = tempbool; }
				else if (Options.TryParse(opt, "Grid", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.grid = tempbool; }
				else if (Options.TryParse(opt, "Hdr", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.header = tempbool; }
				else if (Options.TryParse(opt, "NoSortHdr", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.clickheader = !tempbool; }
				else if (Options.TryParse(opt, "NoSort", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.sortheader = !tempbool; }
				else if (Options.TryParse(opt, "Icon", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { if (tempbool) options.lvview = View.LargeIcon; }
				else if (Options.TryParse(opt, "Tile", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { if (tempbool) options.lvview = View.Tile; }
				else if (Options.TryParse(opt, "IconSmall", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { if (tempbool) options.lvview = View.SmallIcon; }
				else if (Options.TryParse(opt, "List", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { if (tempbool) options.lvview = View.List; }
				else if (Options.TryParse(opt, "Report", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { if (tempbool) options.lvview = View.Details; }
				//PictureBox.
				else if (Options.TryParseString(opt, "Icon", ref tempstr)) { options.iconnumber = ImageHelper.PrepareIconNumber(tempstr); }
				//Other.
				else if (opt == "4") { options.opt4 = true; }
				else if (opt == "8") { options.opt8 = true; }
				else if (opt == "16") { options.opt16 = true; }
				else if (Options.TryParse(opt, "+E", ref options.addexstyle)) { }
				else if (Options.TryParse(opt, "E", ref options.addexstyle)) { }
				else if (Options.TryParse(opt, "-E", ref options.remexstyle)) { }
				else if (Options.TryParse(opt, "+LV", ref temp)) { options.addlvstyle |= temp; }
				else if (Options.TryParse(opt, "LV", ref temp)) { options.addlvstyle |= temp; }
				else if (Options.TryParse(opt, "-LV", ref temp)) { options.remlvstyle &= ~temp; }
				else if (Options.TryParse(opt, "-", ref options.remstyle)) { }
				else if (Options.TryParse(opt, "+", ref options.addstyle)) { }
				else if (Options.TryParse(opt, "", ref options.addstyle)) { }
			}

			return options;
		}
		internal static void SuppressCtrlAKeyDown(object o, KeyEventArgs e)
		{
			if (e.KeyData == (Keys.Control | Keys.A))
				e.SuppressKeyPress = true;
		}
		internal static void SuppressCtrlAPreviewKeyDown(object o, PreviewKeyDownEventArgs e)
		{
			if (e.KeyData == (Keys.Control | Keys.A))
				e.IsInputKey = true;
		}
		internal void CallContextMenuChangeHandlers(bool wasRightClick, int x, int y)
		{
			var control = form.ActiveControl;

			if (control is ListBox lb)
				_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, control, lb.SelectedIndex + 1L, wasRightClick, x, y));
			else if (control is ListView lv)
				_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, control, lv.SelectedIndices.Count > 0 ? lv.SelectedIndices[0] + 1L : 0L, wasRightClick, x, y));
			else if (control is TreeView tv)
				_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, control, tv.SelectedNode.Handle, wasRightClick, x, y));
			else
				_ = (contextMenuChangedHandlers?.InvokeEventHandlers(this, control, control != null ? control.Handle.ToInt64().ToString() : "", wasRightClick, x, y));//Unsure what to pass for Item, so just pass handle.
		}
		internal void Form_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var coords = form.PointToClient(new Point(e.X, e.Y));
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);
				_ = dropFilesHandlers?.InvokeEventHandlers(this, form.ActiveControl, new Array(files), coords.X, coords.Y);
			}
		}
		internal void Form_FormClosing(object sender, FormClosingEventArgs e)
		{
			var handle = form.Handle.ToInt64();
			var result = closedHandlers?.InvokeEventHandlers(this);

			if (!closingFromDestroy)
			{
				e.Cancel = true;

				if (result.IsCallbackResultNonEmpty())
					return;

				form.Hide();
			}
			else if (form != null)
			{
				_ = allGuiHwnds.TryRemove(handle, out _);
				form = null;
			}

			//If there is nothing else keeping the program alive, and the program is not already exiting, close it.
			if (!Keysharp.Scripting.Script.IsMainWindowClosing && !Keysharp.Scripting.Script.AnyPersistent())
			{
				Accessors.A_ExitReason = ExitReasons.Exit.ToString();
				Script.mainWindow?.Close();
			}
		}
		internal void Form_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Apps || (e.KeyCode == Keys.F10 && ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)))
				CallContextMenuChangeHandlers(true, System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
			else if (e.KeyCode == Keys.Escape)
				_ = escapeHandlers?.InvokeEventHandlers(this);
		}
		internal void Form_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				CallContextMenuChangeHandlers(false, e.X, e.Y);
		}
		internal void Form_Resize(object sender, EventArgs e)
		{
			long state;

			if (form.WindowState == FormWindowState.Maximized)
				state = 1L;
			else if (form.WindowState == FormWindowState.Minimized)
				state = -1L;
			else
				state = 0L;

			_ = sizeHandlers?.InvokeEventHandlers(this, state, (long)form.Width, (long)form.Height);
		}
		internal void Tv_Lv_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F2)
			{
				if (sender is TreeView tv)
					tv.SelectedNode?.BeginEdit();
				else if (sender is ListView lv && lv.SelectedItems.Count > 0)
					lv.SelectedItems[0].BeginEdit();
			}
		}
		private void Form_Load(object sender, EventArgs e)
		{
			//form.Visible = false;
		}

		private void ResizeTabControls()
		{
			var dpiscale = !dpiscaling ? 1.0 : Accessors.A_ScaledScreenDPI;
			var tabControls = controls.Values.OfType<GuiControl>().Select(gc => gc.Control).OfType<KeysharpTabControl>().ToHashSet();

			foreach (var tc in tabControls)
				tc.AdjustSize(dpiscale);
		}

		public object this[object controlname]
		{
			get
			{
				var handle = controlname.ParseLong(false);

				if (handle.HasValue)
				{
					if (controls.TryGetValue(handle.Value, out var val) && val is GuiControl gc)
						return gc;
				}

				if (controlname is string s)
				{
					foreach (var ctrlkv in controls)
					{
						if (ctrlkv.Value is GuiControl gc)
						{
							if (string.Compare(gc.Name as string, s, true) == 0)
								return gc;

							if (string.Compare(gc.Text as string, s, true) == 0)
								return gc;
						}
					}

					//Put the ClassNN searches in a separate loop to be done as a last resort because they're very slow.
					foreach (var ctrlkv in controls)
					{
						if (ctrlkv.Value is GuiControl gc)
						{
							if (string.Compare(gc.ClassNN, s, true) == 0)
								return gc;
						}
					}

					foreach (var ctrlkv in controls)
					{
						if (ctrlkv.Value is GuiControl gc)
						{
							if (string.Compare(gc.NetClassNN, s, true) == 0)
								return gc;
						}
					}
				}

				throw new Error($"No controls matched the handle, name, text, ClassNN or NetClassNN {controlname}.");
			}
		}

		internal class GuiOptions
		{
			internal int addexstyle = 0;
			internal int addlvstyle = 0x20;
			internal int addstyle = 0;
			internal bool? altsubmit;
			internal Color? bgcolor;
			internal bool bgtrans = false;
			internal bool bottom = false;
			internal bool? autosize;

			//Control specific.
			//Button.
			internal bool? btndef;

			//Tab.
			internal bool? buttons;

			internal Color c = Control.DefaultForeColor;
			internal bool? center;

			//Checkbox.
			internal bool check3 = false;

			internal bool checkedgray = false;
			internal List<int> choose = new List<int>();
			internal bool choosenone = false;
			internal bool? clickheader;

			//ComboBox.
			internal bool? cmbsimple;

			//DateTime.
			internal string customdate = "";

			internal bool datemultisel;

			//DropDownList
			internal int ddlchoose = int.MinValue;

			internal DateTime dtChoose = DateTime.Now;
			internal DateTime dthigh = DateTime.MaxValue;
			internal DateTime dtlow = DateTime.MinValue;
			internal bool dtopt1 = false;
			internal bool dtopt2 = false;
			internal DateTime dtselend = DateTime.MaxValue;
			internal DateTime dtselstart = DateTime.MinValue;
			internal bool? enabled;

			//ListView.
			internal bool? grid;

			//GroupBox.
			internal bool group = false;

			internal bool? header;
			internal int height = int.MinValue;
			internal bool? hex;
			internal int hp = int.MinValue;
			internal bool hscroll = true;
			internal int hscrollamt = int.MinValue;

			//PictureBox.
			internal object iconnumber = 0;

			//TreeView.
			internal long ilid = long.MinValue;

			//Slider.
			internal bool? invert;

			internal int? ischecked;
			internal bool? leftj;

			//Edit.
			internal int limit = int.MinValue;

			internal int line = int.MinValue;
			internal bool? lines;
			internal bool? lowercase;
			internal View? lvview;
			internal bool? multiline;
			internal string name = null;
			internal bool? noticks;
			internal int? nudhigh;

			//NumericUpDown.
			internal bool nudhorz = false;

			internal int? nudinc;
			internal bool nudleft = false;
			internal int? nudlow;
			internal string nudrange = "";
			internal bool number = false;
			internal bool opt16 = false;

			//MonthCal.
			internal bool opt4 = false;

			internal bool opt8 = false;
			internal int page = int.MinValue;
			internal bool pwd = false;
			internal string pwdch = "";
			internal bool? rdonly;
			internal bool? redraw;
			internal int remexstyle = 0;
			internal int remlvstyle;
			internal int remstyle = 0;
			internal bool? rightj;
			internal float rows = float.MinValue;
			internal bool section = false;

			//Progress.
			internal bool? smooth;

			internal bool? sort;
			internal bool? sortdesc;
			internal bool? sortheader;
			internal int t = int.MinValue;
			internal bool? tabstop;
			internal List<int> tabstops = new List<int>();
			internal int thick = int.MinValue;
			internal bool? thinborder;
			internal int tickinterval = int.MinValue;
			internal bool tooltip = false;
			internal int tooltipside = 0;
			internal bool top = false;
			internal bool? uppercase;
			internal bool vertical = false;
			internal bool? visible;
			internal bool? vscroll;
			internal bool? wantctrla;
			internal bool? wantf2;
			internal bool? wantreturn;
			internal bool? wanttab;
			internal int width = int.MinValue;
			internal bool? wordwrap;
			internal int wp = int.MinValue;
			internal int x = int.MinValue;
			internal int xm = int.MinValue;
			internal int xp = int.MinValue;
			internal int xplus = int.MinValue;
			internal int xs = int.MinValue;
			internal int y = int.MinValue;
			internal int ym = int.MinValue;
			internal int yp = int.MinValue;
			internal int yplus = int.MinValue;
			internal int ys = int.MinValue;
		}
	}
}