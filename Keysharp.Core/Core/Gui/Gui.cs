using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Keysharp.Core.Common;
using Keysharp.Core.Common.Threading;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//MATT
using Keysharp.Scripting;

namespace Keysharp.Core
{
	public class Gui : KeysharpObject
	{
		public TabPage CurrentTab;

		public KeysharpForm form;

		internal static ConcurrentDictionary<long, Gui> allGuiHwnds = new ConcurrentDictionary<long, Gui>();
		internal List<GenericFunction> closedHandlers;
		internal List<GenericFunction> contextMenuChangedHandlers;
		internal List<GenericFunction> dropFilesHandlers;
		internal List<GenericFunction> escapeHandlers;
		internal MenuBar menuBar;
		internal MsgMonitorList monitorEvents;
		internal List<GenericFunction> sizeHandlers;

		//Need a way to retrieve contorls based on name, text class or hwnd. That will be hard because it'll require multiple dictionaries.//MATT
		//private static Dictionary<string, Control> controls;
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
				"LastFound", (f, o) => { if (o is bool b) f.lastfound = b; }
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
						f.form.FormBorderStyle = b ? FormBorderStyle.Sizable : FormBorderStyle.FixedDialog;
						f.form.MaximizeBox = b;
					}
				}
			},
			{
				"SysMenu", (f, o) => { if (o is bool b) f.form.ControlBox = b; }
			},
			{
				"Theme", (f, o) =>
				{
					//Not supporting this, perhaps it's from older versions of windows. Should put a log message here noting that.//MATT
				}
			},
			{
				"ToolWindow", (f, o) => {
					if (o is bool b && b)
					{
						f.form.FormBorderStyle = f.resizable ? FormBorderStyle.SizableToolWindow : FormBorderStyle.FixedToolWindow;
						f.form.ShowInTaskbar = false;
					}
					else
					{
						f.form.FormBorderStyle = f.resizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedDialog;
						f.form.ShowInTaskbar = true;
					}
				}
			}
		};

		private static int windowCount = 0;
		private bool dpiscaling = true;
		private bool lastfound = false;

		private bool owndialogs = false;

		private bool resizable = false;

		public object BackColor
		{
			get => form.BackColor.ToArgb().ToString("X").Substring(0, 6);

			set
			{
				if (value is string s)
				{
					if (Keysharp.Core.Conversions.TryParseColor(s, out var c))
						form.BackColor = c;
				}
				else
					form.BackColor = Color.FromArgb((int)(new object[] { value }.L1() | 0xFF000000));
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
			get
			{
				return menuBar;
			}
			set
			{
				menuBar = value;
				form.Controls.Add(menuBar.MenuStrip);
				form.MainMenuStrip = menuBar.MenuStrip;
			}
		}

		public string Name => form.Name;

		public KeysharpForm Parent
		{
			get
			{
				var parent = form.Parent;

				do
				{
					if (parent is KeysharpForm kf)
						return kf;

					parent = parent.Parent;
				} while (parent != null);

				return parent as KeysharpForm;
			}
		}

		public string Title => form.Text;

		public Gui(params object[] obj)
		{
			var (options, caption, eventObj) = obj.L().S2o();//Need to support an EventObj as the third param.//MATT
			form = new KeysharpForm();
			//form.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
			//form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			form.AutoScaleDimensions = new System.Drawing.SizeF(168F, 168F);
			form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			form.ClientSize = new System.Drawing.Size(1042, 792);
			form.eventObj = eventObj;
			form.Icon = Keysharp.Core.Properties.Resources.Keysharp_ico;
			var newCount = Interlocked.Increment(ref windowCount);
			form.Name = $"Keysharp window {newCount}";
			form.KeyPreview = true;
			form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			form.Text = caption != "" ? caption : Accessors.A_ScriptName;
			var x = (int)Math.Round(form.Font.Size * 1.25f);//Not really sure if Size is the same as height, like the documentation says.//MATT
			var y = (int)Math.Round(form.Font.Size * 0.75f);
			form.Margin = new Padding(x, y, x, y);
			form.Tag = new GuiInfo { Delimiter = '|' };
			form.SuspendLayout();//Not sure if we need this.
			Opt(options);
			//var formHandle = form.Handle;//Force the creation.
			form.FormClosing += Form_FormClosing;
			form.KeyDown += Form_KeyDown;
			form.Resize += Form_Resize;
			form.MouseDown += Form_MouseDown;
			form.DragDrop += Form_DragDrop;
			allGuiHwnds[form.Handle.ToInt64()] = this;
			Keysharp.Core.Common.Window.WindowManagerProvider.Instance.LastFound = new WindowItem(form.Handle);
		}

		public static Gui __New(params object[] obj) => New(obj);

		public static Gui New(params object[] obj) => new Gui(obj);

		public GuiControl Add(params object[] obj)
		{
			var (typeo, options, o) = obj.S2o();//The third argument needs to account for being an array in the case of combo/list boxes.
			var type = typeo.ToLowerInvariant();
			GuiControl holder = null;
			var text = o as string;
			var al = o as Array;

			if (!(form.Tag is GuiInfo info))
				return null;

			var opts = ParseOpt(type, text, options);
			Control ctrl = null;

			switch (type)
			{
				case Core.Keyword_Text:
				{
					var lbl = new KeysharpLabel
					{
						Font = form.Font,
					};
					ctrl = lbl;
				}
				break;

				case Core.Keyword_Edit:
				{
					var ml = opts.multiline.IsTrue() || opts.rows > 1;

					if (opts.number)
						opts.addstyle |= WindowsAPI.ES_NUMBER;

					if (opts.limit == int.MinValue && !ml)
						opts.remstyle |= WindowsAPI.WS_HSCROLL | WindowsAPI.ES_AUTOHSCROLL;

					var txt = new KeysharpEdit(opts.addstyle, opts.remstyle)
					{
						AcceptsTab = opts.wanttab ?? false,
						AcceptsReturn = opts.wantreturn ?? false,
						Multiline = ml,
						ReadOnly = opts.rdonly ?? false
					};

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

					if (txt.Multiline && opts.tabstops.Any())
						_ = WindowsAPI.SendMessage(txt.Handle, WindowsAPI.EM_SETTABSTOPS, opts.tabstops.Count, opts.tabstops.ToArray());

					ctrl = txt;
				}
				break;

				case Core.Keyword_UpDown:
				{
					//MATT
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

					ctrl = nud;
				}
				break;

				case Core.Keyword_Pic:
				case Core.Keyword_Picture://No special support for GDI+, instead we just use whatever C# uses under the hood for its PictureBox control. Also, animated gifs do animate.
				{
					var pic = new KeysharpPictureBox(text);

					if (opts.width < 0 && opts.height < 0)
						pic.SizeMode = PictureBoxSizeMode.AutoSize;
					else if (opts.width < 0 || opts.height < 0)
						pic.SizeMode = PictureBoxSizeMode.Zoom;
					else
						pic.SizeMode = PictureBoxSizeMode.StretchImage;

					ctrl = pic;
				}
				break;

				case Core.Keyword_Button:
				{
					opts.addstyle &= WindowsAPI.BS_NOTIFY;//Documentation says BS_NOTIFY will be automatically added in OnEvent(), which is the only time clicks are handled, so add regardless.
					ctrl = new KeysharpButton(opts.addstyle, opts.remstyle)
					{
						Name = text
					};

					if (opts.btndef.IsTrue())
						form.AcceptButton = (IButtonControl)ctrl;
				}
				break;

				case Core.Keyword_CheckBox:
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

					ctrl = chk;
				}
				break;

				case Core.Keyword_Radio:
				{
					var rad = new KeysharpRadioButton(opts.addstyle, opts.remstyle)
					{
						Text = text
					};
					ctrl = rad;
				}
				break;

				case Core.Keyword_ComboBox:
				case Core.Keyword_DropDownList:
				{
					KeysharpComboBox ddl;

					if (type == Core.Keyword_DropDownList)
					{
						ddl = new KeysharpComboBox();
						ddl.DropDownStyle = ComboBoxStyle.DropDownList;
					}
					else
					{
						ddl = new KeysharpComboBox(0, opts.limit != int.MinValue ? WindowsAPI.CBS_AUTOHSCROLL : 0);
						ddl.DropDownStyle = opts.cmbsimple.IsTrue() ? ComboBoxStyle.Simple : ComboBoxStyle.DropDown;
					}

					if (opts.sort.IsTrue())
					{
						ddl.Sorted = true;

						if (type == Core.Keyword_ComboBox)
						{
							ddl.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
							ddl.AutoCompleteSource = AutoCompleteSource.ListItems;
						}
					}

					ddl.Items.AddRange(al.Flatten().Cast<object>().Select(x => opts.lowercase.IsTrue() ? x.Str().ToLower() : opts.uppercase.IsTrue() ? x.Str().ToUpper() : x.Str()).ToArray());

					if (opts.choose.Any())
						ddl.SelectedIndex = opts.choose[0];

					if (opts.rows != float.MinValue)
						ddl.DropDownHeight = (int)Math.Round(ddl.ItemHeight * opts.rows);
					else if (opts.height != int.MinValue)
						ddl.DropDownHeight = opts.height;

					//Documentation says ItemHeight can be set with a message, but it can just be set with the ItemHeight property.
					ddl.IntegralHeight = true;
					ctrl = ddl;
				}
				break;

				case Core.Keyword_ListBox:
				{
					if (opts.rdonly.IsTrue())
						opts.addstyle |= WindowsAPI.LBS_NOSEL;

					var lb = new KeysharpListBox(opts.addstyle, opts.remstyle)
					{
						SelectionMode = opts.multiline.IsTrue() ? SelectionMode.MultiExtended : SelectionMode.One,
						Sorted = opts.sort.IsTrue()//Unsure how to make incremental search work.
					};
					lb.Items.AddRange(al.Flatten().Cast<object>().Select(x => opts.lowercase.IsTrue() ? x.Str().ToLower() : opts.uppercase.IsTrue() ? x.Str().ToUpper() : x.Str()).ToArray());

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

				case Core.Keyword_ListView:
				{
					//There is no way to preallocate memory with the "Count" option, so that is ignored.//MATT
					var lv = new KeysharpListView();
					lv.Columns.AddRange(al.Cast<object>().Select(x => new ColumnHeader { Text = x.Str() }).ToArray());
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

					ctrl = lv;
				}
				break;

				case Core.Keyword_TreeView:
				{
					var tv = new KeysharpTreeView(!opts.hscroll ? WindowsAPI.TVS_NOHSCROLL : 0, 0);

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

				case Core.Keyword_Link:
				{
					var linklabel = new KeysharpLinkLabel(text);
					ctrl = linklabel;
				}
				break;

				case Core.Keyword_Hotkey:
				{
					var hk = new HotkeyBox();

					if (opts.limit != int.MinValue)
						hk.Limit = (HotkeyBox.Limits)opts.limit;

					if (text != "")
						hk.SetText(text);

					ctrl = hk;
				}
				break;

				case Core.Keyword_DateTime:
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

				case Core.Keyword_MonthCal:
				{
					var cal = new KeysharpMonthCalendar();

					if (opts.dtlow != DateTime.MinValue)//This causes a crash when you scroll past it.
						cal.MinDate = opts.dtlow;

					if (opts.dthigh != DateTime.MaxValue)
						cal.MaxDate = opts.dthigh;

					cal.ShowWeekNumbers = opts.opt4;
					cal.ShowTodayCircle = !opts.opt8;
					cal.ShowToday = !opts.opt16;

					if (opts.rangesel.HasValue)
						cal.MaxSelectionCount = opts.rangesel.Value;

					if (opts.dtselstart > DateTime.MinValue && opts.dtselend < DateTime.MaxValue)
						cal.SelectionRange = new SelectionRange(opts.dtselstart, opts.dtselend);

					//Note that colors do not work here is visual styles are enabled.
					cal.TitleForeColor = opts.c;

					if (opts.bgcolor.HasValue)
						cal.TitleBackColor = opts.bgcolor.Value;

					ctrl = cal;
				}
				break;

				case Core.Keyword_Slider://Still need to figure out how to do buddy controls.//MATT
				{
					var style = 0;

					if (opts.tooltip)
						style |= WindowsAPI.TBS_TOOLTIPS;

					if (opts.thick != int.MinValue)
						style |= WindowsAPI.TBS_FIXEDLENGTH;

					var slider = new KeysharpTrackBar(style) { Orientation = opts.vertical ? Orientation.Vertical : Orientation.Horizontal };

					if (opts.nudlow.HasValue && opts.nudhigh.HasValue)
					{
						slider.Minimum = opts.nudlow.Value;
						slider.Maximum = opts.nudhigh.Value;
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

					if (opts.line != int.MinValue)
						slider.SmallChange = opts.line;

					if (opts.page != int.MinValue)
						slider.LargeChange = opts.page;

					if (opts.thick != int.MinValue)
						_ = WindowsAPI.SendMessage(slider.Handle, WindowsAPI.TBM_SETTHUMBLENGTH, (uint)opts.thick, 0);

					if (opts.tooltip)
						_ = WindowsAPI.SendMessage(slider.Handle, WindowsAPI.TBM_SETTIPSIDE, (uint)opts.tooltipside, 0);

					slider.inverted = opts.invert.IsTrue();
					ctrl = slider;
				}
				break;

				case Core.Keyword_Progress:
				{
					var prg = new KeysharpProgressBar(opts.vertical ? 0x04 : 0);
					prg.Style = opts.smooth.IsTrue() ? ProgressBarStyle.Continuous : ProgressBarStyle.Blocks;

					if (opts.nudlow.HasValue)
						prg.Minimum = opts.nudlow.Value;

					if (opts.nudhigh.HasValue)
						prg.Maximum = opts.nudhigh.Value;

					if (o is long pos)
						prg.Value = (int)pos;

					prg.ForeColor = opts.c;//Probably not needed since it's set below.//MATT

					if (opts.bgcolor.HasValue)
						prg.BackColor = opts.bgcolor.Value;

					ctrl = prg;
				}
				break;

				case Core.Keyword_GroupBox:
				{
					ctrl = new KeysharpGroupBox() { };
				}
				break;

				case Core.Keyword_Tab:
				case Core.Keyword_Tab2:
				case Core.Keyword_Tab3:
				{
					var tc = new KeysharpTabControl();//This will also support image lists just like TreeView for setting icons on tabs, instead of using SendMessage().//MATT
					tc.TabPages.AddRange(al.Flatten().Cast<object>().Select(x => new TabPage(x.Str())).ToArray());

					if (opts.leftj.IsTrue())
						tc.Alignment = System.Windows.Forms.TabAlignment.Left;
					else if (opts.rightj.IsTrue())
						tc.Alignment = System.Windows.Forms.TabAlignment.Right;
					else if (opts.bottom)
						tc.Alignment = System.Windows.Forms.TabAlignment.Bottom;
					else if (opts.top)
						tc.Alignment = System.Windows.Forms.TabAlignment.Top;

					if (opts.buttons.HasValue)
						tc.Appearance = TabAppearance.FlatButtons;

					if (opts.choose.Any())
						tc.SelectedIndex = opts.choose[0];

					if (opts.wordwrap.HasValue)
						tc.Multiline = opts.wordwrap.IsTrue();

					ctrl = tc;
				}
				break;

				case Core.Keyword_StatusBar:
				{
					var ss = new KeysharpStatusStrip();
					info.StatusBar = ss;
					ss.ImageScalingSize = new System.Drawing.Size(28, 28);
					ss.Dock = DockStyle.Bottom;

					if (text != "")
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

				case Core.Keyword_ActiveX:
				{
				}
				break;

				case Core.Keyword_WebBrowser:
				{
					var web = new WebBrowser();
					web.Navigate(text);
					ctrl = web;
				}
				break;

				case Core.Keyword_Custom:
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

			if (!(ctrl is DateTimePicker) && !(ctrl is HotkeyBox) && !(ctrl is LinkLabel))
				ctrl.Text = text;

			ctrl.Margin = form.Margin;// form.Padding;//Padding or margin? Unsure.

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

			if (ctrl is KeysharpTabControl ktc)
				if (ktc.TabPages.Count >= 0)
					holder.UseTab(1);//Will set this object's CurrentTab value.

			if (opts.altsubmit.HasValue)
				holder.AltSubmit = opts.altsubmit.Value;

			var fontpixels = GetFontPixels(ctrl.Font);
			float w = ctrl.PreferredSize.Width;

			if (opts.wp != int.MinValue)
				w = info.LastControl != null ? (float)Accessors.A_ScaledScreenDPI * (info.LastControl.Width + opts.wp) : 0.0f;
			else if (opts.width != int.MinValue)
				w = (float)Accessors.A_ScaledScreenDPI * opts.width;
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

			ctrl.Width = opts.width != int.MinValue ? (int)Math.Round(w) : Math.Max((int)w, ctrl.PreferredSize.Width);

			if (opts.hp != int.MinValue)
			{
				ctrl.Height = info.LastControl != null ? (int)Math.Round(Accessors.A_ScaledScreenDPI * (info.LastControl.Height + opts.hp)) : 0;
			}
			else
			{
				if (opts.height != int.MinValue)
				{
					ctrl.Height = (int)Math.Round(Accessors.A_ScaledScreenDPI * opts.height);
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

					var defheight = (int)(fontpixels * Math.Round(r + 0.5));//AHK used external leading, but just use fontpixels here because it's close enough.

					if (ctrl is ComboBox cmb)
					{
						cmb.MaxDropDownItems = r;
					}
					else if (ctrl is ListBox lb)
					{
						lb.Height = (int)(Accessors.A_ScaledScreenDPI * ((lb.ItemHeight * r) + (lb.Height - lb.ClientSize.Height) + lb.Margin.Bottom));
					}
					else if (ctrl is TreeView tv)
					{
						tv.Height = tv.ItemHeight * r - tv.Margin.Bottom;//For some reason, TreeView doesn't appear to need to have DPI scaling applied, and also is a bit too large, so we subtract the margin.
					}
					else if (ctrl is GroupBox gb)
					{
						gb.Height = defheight + ((gb.Margin.Top + gb.Margin.Bottom) * (2 + ((int)(r + 1 + 0.5) - 2)));//This odd formula comes straight from the AHK source.
					}
					else if (ctrl is ListView lv)
					{
						lv.Height = defheight + ((lv.Margin.Top + lv.Margin.Bottom) * (2 + ((int)(r + 1 + 0.5) - 2)));//ListView doesn't have an ItemHeight property, so attempt to compute here in the same way GroupBox is done above.
					}
					else if (ctrl is TabControl tc)
					{
						tc.Height = defheight + ((tc.Margin.Top + tc.Margin.Bottom) * (2 + ((int)(r + 1 + 0.5) - 1)));//Same here, but -1.
					}
					else
					{
						if (opts.rows == float.MinValue) //Neither r or h were specified.
						{
							if (ctrl is TrackBar trk && opts.thick == int.MinValue)//Separate check for TrackBar because the documentation specifies it in pixels. Skip this if thickness has been specified.
							{
								ctrl.Height = trk.Orientation == Orientation.Horizontal ? (int)Math.Round(Accessors.A_ScaledScreenDPI * 30) : (int)Math.Round(5 * fontpixels);
								goto heightdone;
							}
						}

						if (r > 1)
						{
							ctrl.Height = (int)(r * fontpixels);
						}
						else
						{
							var ctrlheight = ctrl.PreferredSize.Height;
							ctrlheight += ctrl.Margin.Top + ctrl.Margin.Bottom + ctrl.Padding.Top + ctrl.Padding.Bottom;
							ctrlheight += ctrl.Height - ctrl.ClientSize.Height;//Account for the border.
							ctrl.Height = ctrlheight;
						}
					}
				}
			}

			heightdone:
			//ctrl.Height = (int)Math.Round(ctrl.Height * Accessors.A_ScaledScreenDPI);
			var last = info.LastControl;
			Point loc;
			(Control right, Control bottom) rb = info.RightBottomMost();

			if (last != null)
			{
				var xoffset = rb.right.Location.X;
				var yoffset = rb.bottom.Location.Y;

				if (opts.xplusm)
					xoffset += form.Margin.Right + ctrl.Margin.Left;
				else if (opts.xplus != int.MinValue)
					xoffset += rb.right.Size.Width + opts.xplus;
				else if (opts.xp != int.MinValue)
					xoffset += opts.xp;
				else if (opts.xm != int.MinValue)
					xoffset = form.Padding.Left;
				else if (opts.xs != int.MinValue)
					xoffset = info.Section.X + opts.xs;
				else
					xoffset = int.MinValue;

				if (opts.yplusm)
					yoffset += form.Margin.Bottom + ctrl.Margin.Top;
				else if (opts.yplus != int.MinValue)
					yoffset += rb.right.Size.Height + opts.yplus;
				else if (opts.yp != int.MinValue)
					yoffset += opts.yp;
				else if (opts.ym != int.MinValue)
					yoffset = form.Padding.Top;
				else if (opts.ys != int.MinValue)
					yoffset = info.Section.Y + opts.ys;
				else
					yoffset = int.MinValue;

				loc = new Point(xoffset, yoffset);
			}
			else
				loc = new Point(int.MinValue, int.MinValue);

			//Explicitly positioned values override everything.
			if (opts.x != int.MinValue)
				loc.X = opts.x;

			if (opts.y != int.MinValue)
				loc.Y = opts.y;

			//if (last != null && last.Parent != null && !(last.Parent is Form))
			//{
			//  last = last.Parent;
			//}

			//Note we check DockStyle here because if the previous control was docked to a side, then we can't really use its location as a reference to base this control's location off of.
			if (loc.X != int.MinValue && loc.Y != int.MinValue)//If both x and y were specified, that takes precedence over everything else.
				ctrl.Location = loc;
			else if (last != null && last.Dock == DockStyle.None && loc.X == int.MinValue && loc.Y == int.MinValue)
			{
				var templast = opts.group && last.Parent is Panel panel ? panel : last;
				ctrl.Location = new Point(templast.Location.X, templast.Location.Y + templast.Height + form.Margin.Bottom + ctrl.Margin.Top);
			}
			else if (rb.right != null && rb.right.Dock == DockStyle.None && loc.X == int.MinValue)
				ctrl.Location = new Point(rb.right.Location.X + rb.right.Width + form.Margin.Right + ctrl.Margin.Left, loc.Y);
			else if (rb.bottom != null && rb.bottom.Dock == DockStyle.None && loc.Y == int.MinValue)
				ctrl.Location = new Point(loc.X, rb.bottom.Location.Y + rb.bottom.Height + rb.bottom.Margin.Bottom + ctrl.Margin.Top);
			else//Final fallback when nothing else has worked.
			{
				var top = form.Margin.Top;

				if (form.MainMenuStrip != null)
					top += form.MainMenuStrip.Height;

				top = (int)Math.Round(top * Accessors.A_ScaledScreenDPI);
				ctrl.Location = new Point(form.Margin.Left, top);
			}

			//ctrl.Location = new Point((int)Math.Round(ctrl.Location.X * Accessors.A_ScaledScreenDPI), (int)Math.Round(ctrl.Location.Y * Accessors.A_ScaledScreenDPI));

			if (CurrentTab != null)
			{
				CurrentTab.Controls.Add(ctrl);
			}
			else if (ctrl is KeysharpRadioButton krb)
			{
				if (last == null || !(last is KeysharpRadioButton) || opts.group)
				{
					var panel = new Panel();
					//panel.BorderStyle = BorderStyle.FixedSingle;
					panel.Location = new Point(Math.Max(form.Margin.Left, ctrl.Left), Math.Max(form.Margin.Top, ctrl.Top));
					form.Controls.Add(panel);
					var screenpt = form.PointToScreen(ctrl.Location);
					var clientpt = panel.PointToClient(screenpt);
					ctrl.Location = new Point(Math.Max(form.Margin.Left, clientpt.X), Math.Max(form.Margin.Top, clientpt.Y));
					panel.Size = new System.Drawing.Size(ctrl.Width + 10, ctrl.Height + 10);
					panel.AutoSize = true;
					panel.Controls.Add(ctrl);
					info.LastContainer = panel;
				}
				else if (info.LastContainer is Panel pnl)
					pnl.Controls.Add(ctrl);

				krb.Checked = opts.ischecked.HasValue && opts.ischecked.Value > 0;
			}
			else
				form.Controls.Add(ctrl);

			if (ctrl is KeysharpPictureBox pbox && System.IO.File.Exists(text))
			{
				if (pbox.SizeMode == PictureBoxSizeMode.Zoom)
				{
					if (ImageHelper.LoadImage(text, opts.width, opts.height, 0) is Bitmap bmp)
					{
						var ratio = bmp.Height != 0 ? (double)bmp.Width / bmp.Height : 1;

						if (ratio == 0)
							ratio = 1;

						if (opts.width < 0)
							pbox.Width = (int)(pbox.Height * ratio);
						else
							pbox.Height = (int)(pbox.Width / ratio);

						pbox.Image = bmp;
					}
				}
				else
					pbox.Load(text);
			}

			if (opts.section)
				info.Section = ctrl.Location;

			info.LastControl = ctrl;
			return holder;
		}

		public GuiControl AddActiveX(params object[] obj) => Add(new object[] { Core.Keyword_ActiveX }.Concat(obj));

		public GuiControl AddButton(params object[] obj) => Add(new object[] { Core.Keyword_Button }.Concat(obj));

		public GuiControl AddCheckbox(params object[] obj) => Add(new object[] { Core.Keyword_CheckBox }.Concat(obj));

		public GuiControl AddComboBox(params object[] obj) => Add(new object[] { Core.Keyword_ComboBox }.Concat(obj));

		public GuiControl AddCustom(params object[] obj) => Add(new object[] { Core.Keyword_Custom }.Concat(obj));

		public GuiControl AddDateTime(params object[] obj) => Add(new object[] { Core.Keyword_DateTime }.Concat(obj));

		public GuiControl AddDropDownList(params object[] obj) => Add(new object[] { Core.Keyword_DropDownList }.Concat(obj));

		public GuiControl AddEdit(params object[] obj) => Add(new object[] { Core.Keyword_Edit }.Concat(obj));

		public GuiControl AddGroupBox(params object[] obj) => Add(new object[] { Core.Keyword_GroupBox }.Concat(obj));

		public GuiControl AddHotKey(params object[] obj) => Add(new object[] { Core.Keyword_Hotkey }.Concat(obj));

		public GuiControl AddLink(params object[] obj) => Add(new object[] { Core.Keyword_Link }.Concat(obj));

		public GuiControl AddListBox(params object[] obj) => Add(new object[] { Core.Keyword_ListBox }.Concat(obj));

		public GuiControl AddListView(params object[] obj) => Add(new object[] { Core.Keyword_ListView }.Concat(obj));

		public GuiControl AddMonthCal(params object[] obj) => Add(new object[] { Core.Keyword_MonthCal }.Concat(obj));

		public GuiControl AddPicture(params object[] obj) => Add(new object[] { Core.Keyword_Picture }.Concat(obj));

		public GuiControl AddProgress(params object[] obj) => Add(new object[] { Core.Keyword_Progress }.Concat(obj));

		public GuiControl AddRadio(params object[] obj) => Add(new object[] { Core.Keyword_Radio }.Concat(obj));

		public GuiControl AddSlider(params object[] obj) => Add(new object[] { Core.Keyword_Slider }.Concat(obj));

		public GuiControl AddStatusBar(params object[] obj) => Add(new object[] { Core.Keyword_StatusBar }.Concat(obj));

		public GuiControl AddTab(params object[] obj) => Add(new object[] { Core.Keyword_Tab }.Concat(obj));

		public GuiControl AddText(params object[] obj) => Add(new object[] { Core.Keyword_Text }.Concat(obj));

		public GuiControl AddTreeView(params object[] obj) => Add(new object[] { Core.Keyword_TreeView }.Concat(obj));

		public GuiControl AddUpDown(params object[] obj) => Add(new object[] { Core.Keyword_UpDown }.Concat(obj));

		public GuiControl AddWebBrowser(params object[] obj) => Add(new object[] { Core.Keyword_WebBrowser }.Concat(obj));

		public void Destroy()
		{
			form.Close();
		}

		public void Flash(params object[] obj)
		{
			var b = obj.L().B1(true);

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				_ = WindowsAPI.FlashWindow(form.Handle, b);
		}

		public Dictionary<string, object> GetClientPos() => GuiControl.GetClientPos(form, dpiscaling);

		public Dictionary<string, object> GetPos() => GuiControl.GetPos(form, dpiscaling);

		public void Hide() => form.Hide();

		public void Maximize() => form.WindowState = FormWindowState.Maximized;

		public void Minimize() => form.WindowState = FormWindowState.Minimized;

		public void Move(params object[] obj)
		{
			var (x, y, width, height) = obj.L().I4();
			var scale = dpiscaling ? 1.0 : Accessors.A_ScaledScreenDPI;
			form.Top = (int)Math.Round(y * scale);
			form.Left = (int)Math.Round(x * scale);
			form.Width = (int)Math.Round(width * scale);
			form.Height = (int)Math.Round(height * scale);
		}

		public void OnEvent(params object[] obj)
		{
			var (e, h, i) = obj.L().Soi("", null, 1);
			e = e.ToLower();
			var del = GuiControl.GetDel(h, form.eventObj);

			if (e == "close")
			{
				if (closedHandlers == null)
					closedHandlers = new List<GenericFunction>();

				closedHandlers.ModifyEventHandlers(del, i);
			}
			else if (e == "contextmenu")
			{
				if (contextMenuChangedHandlers == null)
					contextMenuChangedHandlers = new List<GenericFunction>();

				contextMenuChangedHandlers.ModifyEventHandlers(del, i);
			}
			else if (e == "dropfiles")
			{
				if (dropFilesHandlers == null)
					dropFilesHandlers = new List<GenericFunction>();

				dropFilesHandlers.ModifyEventHandlers(del, i);
			}
			else if (e == "escape")
			{
				if (escapeHandlers == null)
					escapeHandlers = new List<GenericFunction>();

				escapeHandlers.ModifyEventHandlers(del, i);
			}
			else if (e == "size")
			{
				if (sizeHandlers == null)
					sizeHandlers = new List<GenericFunction>();

				sizeHandlers.ModifyEventHandlers(del, i);
			}
		}

		public void Opt(string opt)
		{
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
					else if (showOptionsDkt.TryGetValue(str, out var func))
					{
						if (split[0] == '+')
							func(this, true);
						else if (split[0] == '-')
							func(this, false);
					}
					else//Special style, windows only.//MATT
					{
					}
				}
			}
		}

		public void Restore() => form.WindowState = FormWindowState.Normal;

		public void SetFont(params object[] obj) => form.SetFont(obj);

		public void Show(params object[] obj)
		{
			var s = obj.L().S1();
			bool /*center = false, cX = false, cY = false,*/ auto = false, min = false, max = false, restore = false, hide = false;
			int?[] pos = { null, null, null, null };

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
						case Core.Keyword_AutoSize: auto = true; break;

						case Core.Keyword_Maximize: max = true; break;

						case Core.Keyword_Minimize: min = true; break;

						case Core.Keyword_Restore:
							form.showWithoutActivation = false;
							restore = true;
							break;

						case Core.Keyword_NoActivate:
						case Core.Keyword_NA:
							form.showWithoutActivation = true;
							restore = true;
							break;

						case Core.Keyword_Hide: hide = true; break;
					}
				}
				else
				{
					var modeval = mode.AsSpan(1);

					if (modeval.Equals(Core.Keyword_Center, StringComparison.OrdinalIgnoreCase))
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

			if (auto || (pos[0] == null && pos[1] == null))//Need to figure out how to auto size to fit all controls.
			{
				var status = form.Controls.OfType<StatusStrip>().ToArray();
				var d = status.Length == 0 ? 0 : status[0].Height;
				form.AutoSize = true;
				form.AutoSizeMode = AutoSizeMode.GrowAndShrink;

				if (d > 0)
					form.ClientSize = new Size(form.ClientSize.Width, form.ClientSize.Height + d);
			}
			else
			{
				var size = form.beenShown ? form.Size : new Size(800, 500);//Using this size because PreferredSize is so small it just shows the title bar.

				if (pos[0] != null)
					size.Width = (int)pos[0];

				if (pos[1] != null)
					size.Height = (int)pos[1];

				form.ClientSize = size;
			}

			var location = form.beenShown ? form.Location : new Point();
			var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;

			if (pos[2] != null)
				location.X = (int)pos[2];
			else if (/*cX || center ||*/ !form.beenShown)
				location.X = ((screen.Width - form.Size.Width) / 2) + screen.X;

			if (pos[3] != null)
				location.Y = (int)pos[3];
			else if (/*cY || center ||*/ !form.beenShown)
				location.Y = ((screen.Height - form.Size.Height) / 2) + screen.Y;

			form.StartPosition = FormStartPosition.Manual;
			form.Location = location;

			if (hide)
				form.Hide();
			else
				form.Show();

			form.ResumeLayout(true);

			if (min)
				form.WindowState = FormWindowState.Minimized;
			else if (max)
				form.WindowState = FormWindowState.Maximized;
			else if (restore)
				form.WindowState = FormWindowState.Normal;
		}

		public Dictionary<object, object> Submit(params object[] obj)
		{
			var hide = obj.L().B1(true);
			var panels = new HashSet<Panel>();
			var ctrls = form.Controls.Flatten();
			var dkt = new Dictionary<object, object>();

			foreach (Control control in form.Controls)
			{
				if (control.Name != "" && control.Tag is GuiControl guictrl)
				{
					if (control is TextBox || control is DateTimePicker || control is MonthCalendar)//Just use value because it's the same and consolidates the formatting in one place, despite being slightly slower.
						dkt[control.Name] = guictrl.Value;
					else if (control is NumericUpDown nud)
						dkt[nud.Name] = (double)nud.Value;
					else if (control is CheckBox cb)
						dkt[cb.Name] = cb.Checked ? 1L : 0L;
					else if (control is TabControl tc)
						dkt[tc.Name] = !guictrl.AltSubmit ? tc.SelectedTab != null ? tc.SelectedTab.Text : "" : (object)(long)(tc.SelectedIndex + 1);
					else if (control is ComboBox cmb)
						dkt[cmb.Name] = !guictrl.AltSubmit || cmb.Items.IndexOf(cmb.Text) == -1 ? cmb.Text : (object)(long)(cmb.SelectedIndex + 1);
					else if (control is TrackBar tb)
						dkt[tb.Name] = tb.Value;
					else if (control is ListBox lb)
					{
						dkt[lb.Name] = !guictrl.AltSubmit
									   ? guictrl.Value
									   : lb.SelectionMode == SelectionMode.One
									   ? lb.SelectedItem as string ?? ""
									   : (object)new Array(lb.SelectedItems.Cast<object>().Where(xx => xx is string).Select(x => x as string).ToList());
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

			return dkt;
		}

		internal static float GetFontPixels(Font font) => (float)Accessors.A_ScaledScreenDPI* (font.Size * (font.FontFamily.GetCellAscent(FontStyle.Regular) + font.FontFamily.GetCellDescent(FontStyle.Regular)) / font.FontFamily.GetEmHeight(FontStyle.Regular));

		internal static GuiInfo GuiAssociatedInfo(Control control) => control.FindForm().Tag as GuiInfo;

		internal static GuiOptions ParseOpt(string type, string text, string optionsstr)
		{
			var options = new GuiOptions();
			var opts = Options.ParseOptions(optionsstr);

			if (type == "monthcal" && text != "")
			{
				Conversions.ParseRange(text, out options.dtselstart, out options.dtselend);

				if (options.dtselstart == DateTime.MinValue)
					options.dtselstart = options.dtselend;

				if (options.dtselend == DateTime.MaxValue)
					options.dtselend = options.dtselstart;
			}

			foreach (var opt in opts)
			{
				var tempbool = false;
				var temp = 0;
				var tempcolor = Color.Empty;

				if (type == "datetime")
				{
					if (Options.TryParseDateTime(opt, "Choose", "yyyyMMdd", ref options.dtChoose)) { }
					else if (string.Compare(opt, "ChooseNone", true) == 0) { options.choosenone = true; }
					else if (opt == "1") { options.dtopt1 = true; }
					else if (opt == "2") { options.dtopt2 = true; }
				}
				else if (type == "monthcal")
				{
					if (Options.TryParse(opt, "Multi", ref temp)) { options.rangesel = temp; }
				}
				else
				{
					if (Options.TryParse(opt, "Multi", ref tempbool, StringComparison.OrdinalIgnoreCase, true, true)) { options.multiline = tempbool; }
				}

				if (Options.TryParse(opt, "r", ref options.rows)) { }
				else if (Options.TryParse(opt, "w", ref options.width)) { }
				else if (Options.TryParse(opt, "h", ref options.height)) { }
				else if (Options.TryParse(opt, "x+", ref options.xplus)) { }
				else if (Options.TryParse(opt, "y+", ref options.yplus)) { }
				else if (string.Compare(opt, "x+m", true) == 0) { options.xplusm = true; }
				else if (string.Compare(opt, "y+m", true) == 0) { options.yplusm = true; }
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
						options.rangesel = (int)(options.dthigh - options.dtlow).TotalDays;
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
				else if (Options.TryParse(opt, "wp", ref options.wp, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "hp", ref options.hp, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "xp", ref options.xp, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "yp", ref options.yp, StringComparison.OrdinalIgnoreCase, true)) { }
				else if (Options.TryParse(opt, "xm", ref options.xm, StringComparison.OrdinalIgnoreCase, true)) { options.xmargin = true; }
				else if (Options.TryParse(opt, "ym", ref options.ym, StringComparison.OrdinalIgnoreCase, true)) { options.ymargin = true; }
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
				else if (string.Compare(opt, "Number", true) == 0) { options.number = true; }
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
				else if (opt == "4") { options.opt4 = true; }
				else if (opt == "8") { options.opt8 = true; }
				else if (opt == "16") { options.opt16 = true; }
				else if (Options.TryParse(opt, "+E", ref options.addexstyle)) { }
				else if (Options.TryParse(opt, "E", ref options.addexstyle)) { }
				else if (Options.TryParse(opt, "-E", ref options.remexstyle)) { }
				else if (Options.TryParse(opt, "+LV", ref temp)) { options.addlvstyle |= temp; }
				else if (Options.TryParse(opt, "LV", ref temp)) { options.addlvstyle |= temp; }
				else if (Options.TryParse(opt, "-LV", ref temp)) { options.remlvstyle &= ~temp; }
				else if (Options.TryParse(opt, "-", ref temp)) { options.remstyle = temp; }
				else if (Options.TryParse(opt, "", ref temp)) { options.addstyle = temp; }
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
			var result = closedHandlers?.InvokeEventHandlers(this);

			if (result.IsCallbackResultNonEmpty())
			{
				e.Cancel = true;
				form.Show();
				return;
			}

			allGuiHwnds.TryRemove(form.Handle.ToInt64(), out _);
			form = null;

			if (Core.Debug)//Only for making testing easier, never meant to run in production.
				Script.mainWindow?.Close();
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
				state = 1;
			else if (form.WindowState == FormWindowState.Minimized)
				state = -1;
			else
				state = 0;

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

		public class GuiOptions
		{
			public int addexstyle = 0;
			public int addlvstyle = 0x20;
			public int addstyle = 0;
			public bool? altsubmit;
			public Color? bgcolor;
			public bool bgtrans = false;
			public bool bottom = false;

			//Button.
			public bool? btndef;

			//Tab.
			public bool? buttons;

			public Color c = Control.DefaultForeColor;
			public bool? center;

			//Checkbox.
			public bool check3 = false;

			public bool checkedgray = false;
			public List<int> choose = new List<int>();
			public bool choosenone = false;
			public bool? clickheader;

			//ComboBox.
			public bool? cmbsimple;

			//DateTime.
			public string customdate = "";

			//DropDownList
			public int ddlchoose = int.MinValue;

			public DateTime dtChoose = DateTime.Now;
			public DateTime dthigh = DateTime.MaxValue;
			public DateTime dtlow = DateTime.MinValue;
			public bool dtopt1 = false;
			public bool dtopt2 = false;
			public DateTime dtselend = DateTime.MaxValue;
			public DateTime dtselstart = DateTime.MinValue;
			public bool? enabled;

			//ListView.
			public bool? grid;

			//GroupBox.
			public bool group = false;

			public bool? header;
			public int height = int.MinValue;
			public bool? hex;
			public int hp = int.MinValue;
			public bool hscroll = true;
			public int hscrollamt = int.MinValue;

			//TreeView.
			public long ilid = long.MinValue;

			//Slider.
			public bool? invert;

			public int? ischecked;
			public bool? leftj;

			//Control specific.
			//Edit.
			public int limit = int.MinValue;

			public int line = int.MinValue;
			public bool? lines;
			public bool? lowercase;
			public View? lvview;
			public bool? multiline;
			public string name = null;
			public bool? noticks;
			public int? nudhigh;

			//NumericUpDown.
			public bool nudhorz = false;

			public int? nudinc;
			public bool nudleft = false;
			public int? nudlow;
			public string nudrange = "";
			public bool number = false;
			public bool opt16 = false;

			//MonthCal.
			public bool opt4 = false;

			public bool opt8 = false;
			public int page = int.MinValue;
			public bool pwd = false;
			public string pwdch = "";
			public int? rangesel;
			public bool? rdonly;
			public bool? redraw;
			public int remexstyle = 0;
			public int remlvstyle;
			public int remstyle = 0;
			public bool? rightj;
			public float rows = float.MinValue;
			public bool section = false;

			//Progress.
			public bool? smooth;

			public bool? sort;
			public bool? sortdesc;
			public bool? sortheader;
			public int t = int.MinValue;
			public bool? tabstop;
			public List<int> tabstops = new List<int>();
			public int thick = int.MinValue;
			public bool? thinborder;
			public int tickinterval = int.MinValue;
			public bool tooltip = false;
			public int tooltipside = 0;
			public bool top = false;
			public bool? uppercase;
			public bool vertical = false;
			public bool? visible;
			public bool? vscroll;
			public bool? wantctrla;
			public bool? wantf2;
			public bool? wantreturn;
			public bool? wanttab;
			public int width = int.MinValue;
			public bool? wordwrap;
			public int wp = int.MinValue;
			public int x = int.MinValue;
			public int xm = int.MinValue;
			public bool xmargin = false;
			public int xp = int.MinValue;
			public int xplus = int.MinValue;
			public bool xplusm = false;
			public int xs = int.MinValue;
			public int y = int.MinValue;
			public int ym = int.MinValue;
			public bool ymargin = false;
			public int yp = int.MinValue;
			public int yplus = int.MinValue;
			public bool yplusm = false;
			public int ys = int.MinValue;
		}
	}
}