using Label = System.Windows.Forms.Label;

namespace Keysharp.Core
{
	public class KeysharpButton : Button
	{
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpButton(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
#if LINUX
			FlatStyle = FlatStyle.Flat;
#endif
			//Any setting of this field causes click events to behave unpredictably, so don't use it, and therefor don't support double clicks.
			//This differs from the documentation which says that double click is supported.
			//This isn't worth implementing since it's behavior that will almost never be desired.
			//SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpCheckBox : CheckBox
	{
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		protected override bool ShowFocusCues => false;

		public KeysharpCheckBox(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
#if WINDOWS

			if ((addstyle & WindowsAPI.BS_NOTIFY) == WindowsAPI.BS_NOTIFY)
				SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);

#else
			FlatStyle = FlatStyle.Flat;
#endif
		}

#if WINDOWS

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if ((addstyle & WindowsAPI.BS_NOTIFY) == WindowsAPI.BS_NOTIFY)
				ResetFlagsandPaint();
			else
				base.OnMouseUp(e);
		}

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpComboBox : ComboBox
	{
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpComboBox(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpDateTimePicker : DateTimePicker
	{
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpDateTimePicker(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpEdit : TextBox
	{
		private readonly int addstyle, removestyle;

#if !WINDOWS
		internal bool IsNumeric { get; set; }

#endif

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpEdit(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
#if !WINDOWS
			KeyPress += KeysharpEdit_KeyPress;
			Validating += KeysharpEdit_Validating;
#endif
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#else

		private void KeysharpEdit_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (IsNumeric)
			{
				if (Text.Any(ch => !char.IsDigit(ch)))
					Text = string.Join("", Text.Where(ch => char.IsDigit(ch)));
			}
		}

		private void KeysharpEdit_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (IsNumeric)
			{
				if (ModifierKeys.HasFlag(Keys.Control))
				{
					var vch = '\u0016';

					if (e.KeyChar == vch)
					{
						var text = Clipboard.GetText();

						if (text.Any(ch => !char.IsDigit(ch)))
						{
							Text = string.Join("", Text.Where(ch => char.IsDigit(ch)));
							e.Handled = true;
						}
					}
				}

				if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
				{
					e.Handled = true;
				}
			}
		}
#endif
	}

	public class KeysharpForm : Form
	{
		internal object eventObj;
		internal bool showWithoutActivation;
		private bool beenShown = false;

		public bool AllowShowDisplay = true;

		internal bool BeenShown => beenShown;

		[Browsable(false)]
		protected override bool ShowWithoutActivation => showWithoutActivation;

		public KeysharpForm()
		{
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			//See Gui.Show() for where the remainder of the properties get set, such as scaling values.
			Font = MainWindow.OurDefaultFont;
			StartPosition = FormStartPosition.CenterScreen;
			KeyPreview = true;
			DoubleBuffered = true;
			SetStyle(ControlStyles.StandardClick, true);
			SetStyle(ControlStyles.StandardDoubleClick, true);
			Shown += (o, e) =>
			{
				beenShown = true;//Will not trigger on initial temporary show/hide in constructor, and instead will only happen after Gui.Show() completes.
			};
		}
		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(AllowShowDisplay ? value : AllowShowDisplay);
		}
	}

	public class KeysharpGroupBox : GroupBox
	{
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpGroupBox(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpLabel : Label
	{
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpLabel(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpLinkLabel : LinkLabel
	{
		internal bool clickSet = false;
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpLinkLabel(string text, int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;

			if (text != "")
			{
				var links = GuiHelper.ParseLinkLabelText(text);
				Text = links.Item1;
				Links.Clear();

				foreach (var link in links.Item2)
					_ = Links.Add(link.Item1, link.Item2, link.Item3);
			}
		}

		internal static object OnLinkLabelClicked(object obj0, object obj1)
		{
			if (obj0 is LinkLabel ll && obj1 is LinkLabelLinkClickedEventArgs e)
			{
				var url = e.Link.LinkData is Tuple<string, string> tss ? tss.Item2.ToString() : ll.Text.Substring(e.Link.Start, e.Link.Length);

				if (!url.Contains("://"))
					url = "https://" + url;

#if WINDOWS
				_ = Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
#else
				System.Diagnostics.Process proc = new System.Diagnostics.Process();
				proc.EnableRaisingEvents = false;
				proc.StartInfo.FileName = "xdg-open";
				proc.StartInfo.Arguments = url;
				proc.Start();
#endif
				ll.Links[ll.Links.IndexOf(e.Link)].Visited = true;
			}

			return null;
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpListBox : ListBox
	{
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpListBox(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpListView : ListView
	{
		public bool uni = false;
		private int sortColumn = -1;

		public KeysharpListView() => ColumnClick += KeysharpListView_ColumnClick;

#if LINUX
		//Linux has a bug where it will not draw the headers if the control is not initially shown.
		//Eg: a ListView on a tab that is not selected.
		//We had to make a new public method ListView.RedrawDetails() to expose this functionality
		//so we could call it from here.
		protected override void OnVisibleChanged(EventArgs e)
		{
			RedrawDetails();
			base.OnVisibleChanged(e);
		}
#endif

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif

		/// <summary>
		/// Gotten from https://docs.microsoft.com/en-us/previous-versions/dotnet/articles/ms996467(v=msdn.10)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void KeysharpListView_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			if (Sorting == SortOrder.None)
				return;

			if (e.Column != sortColumn)//Determine whether the column is the same as the last column clicked.
			{
				sortColumn = e.Column;//Set the sort column to the new column.
				Sorting = Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;//Determine what the last sort order was and change it.
			}
			else if (!uni)
				Sorting = Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;//Determine what the last sort order was and change it.
		}
	}

	public class KeysharpMonthCalendar : MonthCalendar
	{
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpMonthCalendar(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpNumericUpDown : NumericUpDown
	{
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpNumericUpDown(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	/// <summary>
	/// Derivation that allows setting the interpolation mode.
	/// Gotten from: https://www.codeproject.com/articles/717312/pixelbox-a-picturebox-with-configurable-interpolat
	/// </summary>
	public class KeysharpPictureBox : PictureBox
	{
		private readonly int addstyle, removestyle;
		private readonly InterpolationMode interpolationMode = InterpolationMode.NearestNeighbor;
		private bool scaleHeight;
		private bool scaleWidth;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Filename { get; private set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ScaleHeight
		{
			get => scaleHeight;

			set
			{
				scaleHeight = value;

				if (scaleHeight)
					scaleWidth = false;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ScaleWidth
		{
			get => scaleWidth;

			set
			{
				scaleWidth = value;

				if (scaleWidth)
					scaleHeight = false;
			}
		}

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpPictureBox(string filename, int _add = 0, int _remove = 0)
		{
			Filename = filename;
			addstyle = _add;
			removestyle = _remove;
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			pe.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
			pe.Graphics.InterpolationMode = interpolationMode;
			base.OnPaint(pe);
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	/// <summary>
	/// Derivation to allow for custom colors.
	/// Gotten from: https://stackoverflow.com/questions/778678/how-to-change-the-color-of-progressbar-in-c-sharp-net-3-5
	/// and: https://stackoverflow.com/questions/15795205/how-to-make-a-vertical-progress-bar
	/// </summary>
	public class KeysharpProgressBar : ProgressBar
	{
		private readonly bool customColors = false;
		private readonly int removestyle;
		public int AddStyle { get; }

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= AddStyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpProgressBar(bool _customColors, int _add = 0, int _remove = 0)
		{
			AddStyle = _add;
			removestyle = _remove;
			customColors = _customColors;
			SetStyle(ControlStyles.UserPaint, customColors);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (customColors)
			{
				const int inset = 1;

				using (var offscreenImage = new Bitmap(Width, Height))
				{
					using (var offscreen = Graphics.FromImage(offscreenImage))
					{
						var rect = new Rectangle(0, 0, Width, Height);
						var scaleFactor = ((double)Value - Minimum) / ((double)Maximum - Minimum);
#if WINDOWS
						var vert = (AddStyle & 0x04) == 0x04;
#endif

						if (ProgressBarRenderer.IsSupported)
						{
#if WINDOWS

							if ((AddStyle & 0x04) == 0x04)
								ProgressBarRenderer.DrawVerticalBar(offscreen, rect);
							else
#endif
								ProgressBarRenderer.DrawHorizontalBar(offscreen, rect);
						}

						var bx = 0;
						var by = 0;
						var bw = 0;
						var bh = 0;
						var fy = inset;
#if WINDOWS

						if (vert)
						{
							rect.Width -= inset * 2;
							rect.Height = (int)((rect.Height - inset) * scaleFactor);
							fy = (Height - rect.Height) - inset;
							bx = inset;
							by = inset;
							bw = rect.Width;
							bh = fy;
						}
						else
#endif
						{
							rect.Width = (int)((rect.Width - inset) * scaleFactor);
							rect.Height -= inset * 2;
							bx = rect.Width + inset;
							by = inset;
							bw = (Width - rect.Width) - inset;
							bh = rect.Height;
						}

						if (rect.Width < 0) rect.Width = 0;

						if (rect.Height < 0) rect.Height = 0;

						var brush = new SolidBrush(ForeColor);
						offscreen.FillRectangle(brush, inset, fy, rect.Width, rect.Height);
						var bkgBrush = new SolidBrush(BackColor);
						offscreen.FillRectangle(bkgBrush, bx, by, bw, bh);
						e.Graphics.DrawImage(offscreenImage, 0, 0);
					}
				}
			}
			else
				base.OnPaint(e);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (!customColors)
				base.OnPaintBackground(e);
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpRadioButton : RadioButton
	{
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpRadioButton(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
#if WINDOWS

			if ((addstyle & WindowsAPI.BS_NOTIFY) == WindowsAPI.BS_NOTIFY)
				SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);

#endif
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpRichEdit : RichTextBox
	{
		private readonly int addstyle, removestyle;

#if !WINDOWS
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal bool IsNumeric { get; set; }
#endif

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal CharacterCasing CharacterCasing { get; set; } = CharacterCasing.Normal;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpRichEdit(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
			KeyPress += KeysharpRichEdit_KeyPress;
#if !WINDOWS
			Validating += KeysharpRichEdit_Validating;
#endif
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#else

		private void KeysharpRichEdit_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (IsNumeric)
			{
				if (Text.Any(ch => !char.IsDigit(ch)))
					Text = string.Join("", Text.Where(ch => char.IsDigit(ch)));
			}
		}
#endif

		private void KeysharpRichEdit_KeyPress(object sender, KeyPressEventArgs e)
		{
#if !WINDOWS

			if (IsNumeric)
			{
				if (ModifierKeys.HasFlag(Keys.Control))
				{
					var vch = '\u0016';

					if (e.KeyChar == vch)
					{
						var text = Clipboard.GetText();

						if (text.Any(ch => !char.IsDigit(ch)))
							e.Handled = true;
					}
				}

				if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
				{
					e.Handled = true;
				}
			}
			else
#endif
			{
				switch (CharacterCasing)
				{
					case CharacterCasing.Normal:
						break;

					case CharacterCasing.Upper:
						e.KeyChar = char.ToUpper(e.KeyChar);
						break;

					case CharacterCasing.Lower:
						e.KeyChar = char.ToLower(e.KeyChar);
						break;

					default:
						break;
				}
			}
		}
	}

	public class KeysharpStatusStrip : StatusStrip
	{
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpStatusStrip(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpTabControl : TabControl
	{
		internal Color? bgcolor;
		private readonly int addstyle, removestyle;

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpTabControl(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
			Click += KeysharpTabControl_Click;
			Enter += KeysharpTabControl_Enter;
			ControlAdded += KeysharpTabControl_ControlAdded;
			//SetStyle(ControlStyles.UserPaint, true);
			//SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			//SetStyle(ControlStyles.DoubleBuffer, true);
			//SetStyle(ControlStyles.ResizeRedraw, true);
			//SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

		internal void AdjustSize(double dpiscale)
		{
			var tempw = 0.0;
			var temph = 0.0;

			foreach (TabPage tp in TabPages)
			{
				(Control right, Control bottom) rb = tp.RightBottomMost();

				if (rb.right != null)
					tempw = Math.Max(tempw, ((this.TabWidth() + rb.right.Right)) + (tp.Margin.Right + Margin.Right));

				if (rb.bottom != null)
					temph = Math.Max(temph, ((this.TabHeight() + rb.bottom.Bottom)) + (tp.Margin.Bottom + (Margin.Bottom * dpiscale)));
			}

			Width  = (int)Math.Round(tempw);
			Height = (int)Math.Round(temph);
		}

		internal void SetColor(Color color)
		{
			bgcolor = color;
			DrawMode = TabDrawMode.OwnerDrawFixed;

			foreach (TabPage tp in TabPages)
				tp.BackColor = color;
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (bgcolor.HasValue)
			{
				e.Graphics.FillRectangle(new SolidBrush(bgcolor.Value), ClientRectangle);

				for (var i = 0; i < TabPages.Count; i++)
				{
					var page = TabPages[i];
					var paddedTabBounds = GetTabRect(i);
					var yOffset = (e.Index == i) ? -2 : 1;
					paddedTabBounds.Offset(1, yOffset);
					TextRenderer.DrawText(e.Graphics, page.Text, page.Font, paddedTabBounds, page.ForeColor);
				}
			}
			else
				base.OnDrawItem(e);
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif

		/// <summary>
		/// Focusing gets rid of the unsightly dotted selection box on the tab.
		/// It may still show for a half second when donig custom drawing with bgcolor.
		/// </summary>
		private void KeysharpTabControl_Click(object sender, EventArgs e) => _ = SelectedTab.Focus();

		private void KeysharpTabControl_ControlAdded(object sender, ControlEventArgs e)
		{
			if (bgcolor.HasValue && e.Control is TabPage tp)
				tp.BackColor = bgcolor.Value;
		}

		private void KeysharpTabControl_Enter(object sender, EventArgs e) => _ = SelectedTab.Focus();
	}

	public class KeysharpToolStripStatusLabel : ToolStripStatusLabel
	{
		internal readonly List<IFuncObj> doubleClickHandlers = [];

		//No WndProc method to override because TSSL is not a Control.

		public KeysharpToolStripStatusLabel(string text = "")
			: base(text)
		{
			DoubleClickEnabled = true;
			DoubleClick += KeysharpToolStripStatusLabel_DoubleClick;
		}

		private void KeysharpToolStripStatusLabel_DoubleClick(object sender, EventArgs e) => doubleClickHandlers.InvokeEventHandlers(this, GetCurrentParent().Items.IndexOf(this) + 1);
	}

	public class KeysharpTrackBar : TrackBar
	{
		public bool inverted = false;
		private readonly int addstyle, removestyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int Value
		{
			get => inverted ? (Maximum - base.Value) + Minimum : base.Value;
			set => base.Value = inverted ? (Maximum - value) + Minimum : value;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpTrackBar(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
		}

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}

	public class KeysharpTreeView : TreeView
	{
		private readonly int addstyle, removestyle;
		private readonly Dictionary<TreeNode, bool> expandStates = [];

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= addstyle;
				cp.Style &= ~removestyle;
				return cp;
			}
		}

		public KeysharpTreeView(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
		}

		internal void DelayedExpandParent(TreeNode node)
		{
			var parent = node.Parent ?? node;

			if (expandStates.TryGetValue(parent, out var b))
				if (b)
					parent.Expand();
		}

		internal void MarkForExpansion(TreeNode node) => expandStates[node] = true;

		internal void RemoveMarkForExpansion(TreeNode node) => _ = expandStates.Remove(node);

#if WINDOWS

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

#endif
	}
}