using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
#if WINDOWS
	using Keysharp.Core.Windows;
#endif
using Keysharp.Scripting;

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
			//Any setting of this field causes click events to behave unpredictably, so don't use it, and therefor don't support double clicks.
			//This differs from the documentation which says that double click is supported.
			//This isn't worth implementing since it's behavior that will almost never be desired.
			//SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
		}

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
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
#endif

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
	}

	public class KeysharpEdit : TextBox
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

		public KeysharpEdit(int _add = 0, int _remove = 0)
		{
			addstyle = _add;
			removestyle = _remove;
		}

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
	}

	public class KeysharpForm : Form
	{
		internal object eventObj;
		internal bool showWithoutActivation;
		private bool beenShown = false;

		[System.ComponentModel.Browsable(false)]
		protected override bool ShowWithoutActivation => showWithoutActivation;

		internal bool BeenShown => beenShown;

		public KeysharpForm()
		{
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			//See Gui.Show() for where the remainder of the properties get set, such as scaling values.
			Font = MainWindow.OurDefaultFont;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			KeyPreview = true;
			DoubleBuffered = true;
			SetStyle(ControlStyles.StandardClick, true);
			SetStyle(ControlStyles.StandardDoubleClick, true);
			Shown += (o, e) =>
			{
				beenShown = true;//Will not trigger on initial temporary show/hide in constructor, and instead will only happen after Gui.Show() completes.
			};
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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
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

				_ = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
				ll.Links[ll.Links.IndexOf(e.Link)].Visited = true;
			}

			return null;
		}

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
	}

	public class KeysharpListView : ListView
	{
		public bool uni = false;
		private int sortColumn = -1;

		public KeysharpListView() => ColumnClick += KeysharpListView_ColumnClick;

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
	}

	/// <summary>
	/// Derivation that allows setting the interpolation mode.
	/// Gotten from: https://www.codeproject.com/articles/717312/pixelbox-a-picturebox-with-configurable-interpolat
	/// </summary>
	public class KeysharpPictureBox : PictureBox
	{
		private readonly int addstyle, removestyle;
		private InterpolationMode interpolationMode = InterpolationMode.NearestNeighbor;
		private bool scaleHeight;
		private bool scaleWidth;
		public string Filename { get; private set; }

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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
	}

	/// <summary>
	/// Derivation to allow for custom colors.
	/// Gotten from: https://stackoverflow.com/questions/778678/how-to-change-the-color-of-progressbar-in-c-sharp-net-3-5
	/// and: https://stackoverflow.com/questions/15795205/how-to-make-a-vertical-progress-bar
	/// </summary>
	public class KeysharpProgressBar : ProgressBar
	{
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

		public KeysharpProgressBar(int _add = 0, int _remove = 0)
		{
			AddStyle = _add;
			removestyle = _remove;
			SetStyle(ControlStyles.UserPaint, true);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			const int inset = 1;

			using (var offscreenImage = new Bitmap(Width, Height))
			{
				using (var offscreen = Graphics.FromImage(offscreenImage))
				{
					var rect = new Rectangle(0, 0, Width, Height);
					var scaleFactor = ((double)Value - Minimum) / ((double)Maximum - Minimum);
					var vert = (AddStyle & 0x04) == 0x04;

					if (ProgressBarRenderer.IsSupported)
					{
						if ((AddStyle & 0x04) == 0x04)
							ProgressBarRenderer.DrawVerticalBar(offscreen, rect);
						else
							ProgressBarRenderer.DrawHorizontalBar(offscreen, rect);
					}

					if (vert)
					{
						rect.Width -= 4;
						rect.Height = (int)((rect.Height * scaleFactor) - 4);
					}
					else
					{
						rect.Width = (int)((rect.Width * scaleFactor) - 4);
						rect.Height -= 4;
					}

					if (rect.Width == 0) rect.Width = 1;

					if (rect.Height == 0) rect.Height = 1;

					var brush = new LinearGradientBrush(rect, BackColor, ForeColor, vert ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical);
					offscreen.FillRectangle(brush, inset, vert ? (Height - rect.Height) + inset : inset, rect.Width, rect.Height);
					e.Graphics.DrawImage(offscreenImage, 0, 0);
				}
			}
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			// None... Helps control the flicker.
		}

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
	}

	public class KeysharpRichEdit : RichTextBox
	{
		private readonly int addstyle, removestyle;

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
		}

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

		private void KeysharpRichEdit_KeyPress(object sender, KeyPressEventArgs e)
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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}

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
		internal readonly List<IFuncObj> doubleClickHandlers = new List<IFuncObj>();

		//No WndProc method to override because TSSL is not a Control.

		public KeysharpToolStripStatusLabel(string text = "")
			: base(text)
		{
			DoubleClickEnabled = true;
			DoubleClick += KeysharpToolStripStatusLabel_DoubleClick;
		}

		public void KeysharpToolStripStatusLabel_DoubleClick(object sender, EventArgs e) => doubleClickHandlers.InvokeEventHandlers(this, GetCurrentParent().Items.IndexOf(this) + 1);
	}

	public class KeysharpTrackBar : TrackBar
	{
		public bool inverted = false;
		private readonly int addstyle, removestyle;

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

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
	}

	public class KeysharpTreeView : TreeView
	{
		private readonly int addstyle, removestyle;
		private readonly Dictionary<TreeNode, bool> expandStates = new Dictionary<TreeNode, bool>();

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

		public void DelayedExpandParent(TreeNode node)
		{
			var parent = node.Parent ?? node;

			if (expandStates.TryGetValue(parent, out var b))
				if (b)
					parent.Expand();
		}

		public void MarkForExpansion(TreeNode node) => expandStates[node] = true;

		public void RemoveMarkForExpansion(TreeNode node) => _ = expandStates.Remove(node);

		protected override void WndProc(ref Message m)
		{
			if (!GuiHelper.CallMessageHandler(this, ref m))
				base.WndProc(ref m);
		}
	}
}