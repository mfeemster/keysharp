using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Keysharp.Core.Windows;

namespace Keysharp.Core
{
	public class MessageFilter : IMessageFilter
	{
		public bool PreFilterMessage(ref Message m)
		{
			if (GuiHelper.onMessageHandlers.TryGetValue(m.Msg, out var handlers))
			{
				Keysharp.Scripting.Script.hWndLastUsed = WindowsAPI.GetNonChildParent(m.HWnd);//Assign parent window as the last found window (it's ok if it's hidden).
				var now = DateTime.Now;

				if (Keysharp.Scripting.Script.HookThread is Keysharp.Core.Common.Threading.HookThread ht &&
						ht.kbdMsSender is Keysharp.Core.Common.Keyboard.KeyboardMouseSender kbd)
				{
					kbd.lastPeekTime = now;
				}

				Accessors.A_EventInfo = now;//AHK used msg.time, but the C# version does not have a time field.
				//AHK seems to launch these in threads, but that seems odd, so just call them inline here.
				var res = handlers.InvokeEventHandlers(m.WParam.ToInt64(), m.LParam.ToInt64(), m.Msg, m.HWnd.ToInt64());

				if (res.IsNotNullOrEmpty())
					return true;
			}

			return false;
		}
	}

	public static class GuiHelper
	{
		internal static ConcurrentDictionary<long, List<IFuncObj>> onMessageHandlers = new ();

		[ThreadStatic]
		private static string defaultGui;

		[ThreadStatic]
		private static Form dialogOwner;

		private static Dictionary<string, Form> guis;

		[ThreadStatic]
		private static long lastFoundForm = 0;

		internal static Form DefaultGui
		{
			get
			{
				if (guis == null)
					return null;

				var key = DefaultGuiId;
				return guis.TryGetValue(key, out var form) ? form : null;
			}
		}

		internal static string DefaultGuiId
		{
			get => defaultGui ?? "1";
			set => defaultGui = value;
		}

		internal static Form DialogOwner
		{
			get => dialogOwner;
			set => dialogOwner = value;
		}

		internal static long LastFoundForm
		{
			get => lastFoundForm;
			set => lastFoundForm = value;
		}

		public static Icon GetIcon(string source, int n)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				var prc = System.Diagnostics.Process.GetCurrentProcess().Handle;
				var icon = WindowsAPI.ExtractIcon(prc, source, n);

				if (icon != IntPtr.Zero)
					return Icon.FromHandle(icon);
			}

			return Icon.ExtractAssociatedIcon(source);
		}

		public static object GuiCtrlFromHwnd(object obj) => Control.FromHandle(new IntPtr(obj.Al())) is Control c&& c.Tag is GuiControl gui ? gui : "";

		public static object GuiFromHwnd(object obj0, object obj1 = null)
		{
			var hwnd = obj0.Al();
			var recurse = obj1.Ab();

			if (Gui.allGuiHwnds.TryGetValue(hwnd, out var gui))
				return gui;

			if (recurse)
			{
				if (Control.FromHandle(new IntPtr(hwnd)) is Control c)
				{
					while (c.Parent is Control cp)
					{
						if (Gui.allGuiHwnds.TryGetValue(cp.Handle.ToInt64(), out gui))
							return gui;

						c = cp;
					}
				}
			}

			return "";
		}

		/// <summary>
		/// The Windows API funcitons have serious limitations when it comes to loading icons.
		/// They can't load any of size 256 or larger, plus they are platform specific.
		/// This loads the desired size and is cross platform.
		/// Gotten from https://www.codeproject.com/Articles/26824/Extract-icons-from-EXE-or-DLL-files
		/// </summary>
		/// <param name="icon"></param>
		/// <returns></returns>
		public static List<Icon> SplitIcon(Icon icon)
		{
			if (icon == null)
				throw new ArgumentNullException("icon");

			try
			{
				// Get an .ico file in memory, then split it into separate icons.
				byte[] src = null;

				using (var stream = new System.IO.MemoryStream())
				{
					icon.Save(stream);
					src = stream.ToArray();
				}

				int count = BitConverter.ToUInt16(src, 4);
				var splitIcons = new List<Icon>(count);
				//var sb = new StringBuilder(1024 * 1024);

				for (var i = 0; i < count; i++)
				{
					var length = BitConverter.ToInt32(src, 6 + (16 * i) + 8); //ICONDIRENTRY.dwBytesInRes
					var offset = BitConverter.ToInt32(src, 6 + (16 * i) + 12);//ICONDIRENTRY.dwImageOffset

					using (var dst = new BinaryWriter(new MemoryStream(6 + 16 + length)))
					{
						dst.Write(src, 0, 4);//Copy ICONDIR and set idCount to 1.
						dst.Write((short)1);
						//Copy ICONDIRENTRY and set dwImageOffset to 22.
						dst.Write(src, 6 + (16 * i), 12);//ICONDIRENTRY except dwImageOffset.
						dst.Write(22);                 //ICONDIRENTRY.dwImageOffset.
						//var pixindex = 0;
						var start = dst.BaseStream.Position + 40;
						var end = dst.BaseStream.Position + length;

						for (var ii = start; ii < end; ii += 4)
						{
							//sb.AppendLine($"{pixindex}: a: {src[ii + 3]}");
							//sb.AppendLine($"{pixindex}: r: {src[ii + 2]}");
							//sb.AppendLine($"{pixindex}: g: {src[ii + 1]}");
							//sb.AppendLine($"{pixindex}: b: {src[ii]}");
							var adouble = src[ii + 3] / 255.0;
							src[ii + 2] = (byte)Math.Round(adouble * src[ii + 2]);
							src[ii + 1] = (byte)Math.Round(adouble * src[ii + 1]);
							src[ii] = (byte)Math.Round(adouble * src[ii]);
							//pixindex++;
						}

						dst.Write(src, offset, length);//Copy an image.
						_ = dst.BaseStream.Seek(0, SeekOrigin.Begin);//Create an icon from the in-memory file.
						splitIcons.Add(new Icon(dst.BaseStream));
					}
				}

				//System.IO.File.WriteAllText($"./file{imageindex++}out.txt", sb.ToString());
				return splitIcons;
			}
			catch (Exception e)
			{
				throw new Error($"Error splitting icon: {e.Message}");
			}
		}

		internal static void AddOrInsertListViewItem(ListView lv, GuiHelper.ListViewOptions lvo, List<string> strs, int insert)
		{
			var item = new ListViewItem();

			while (item.SubItems.Count < lv.Columns.Count)
				_ = item.SubItems.Add("");

			for (int i = 0, j = lvo.colstart; i < strs.Count && j < item.SubItems.Count; i++, j++)
				item.SubItems[j].Text = strs[i];

			_ = insert >= 0 ? lv.Items.Insert(Math.Min(insert, lv.Items.Count), item) : lv.Items.Add(item);
			ApplyListViewOptions(lv, item, lvo);

			if (lv.Items.Count == 1)//Resize on the first item, don't do it for subsequent items because it takes too long. It will be done again when setting opts to +Redraw.
				lv.SetListViewColumnSizes();
		}

		internal static void ApplyListViewOptions(ListView lv, ListViewItem item, ListViewOptions lvo)
		{
			item.Focused = lvo.focused;
			item.Selected = lvo.select;
			item.Checked = lvo.ischecked;

			if (lvo.vis)
				item.EnsureVisible();

			if (lvo.icon >= 0 && lv.SmallImageList != null && lvo.icon < lv.SmallImageList.Images.Count)
				item.ImageIndex = lvo.icon;
		}

		internal static bool CallMessageHandler(Control control, ref Message m)
		{
			if (Control.FromHandle(m.HWnd) == control)
			{
				if (control.Tag is GuiControl ctrl)
				{
					var ret = ctrl.InvokeMessageHandlers(ref m);

					if (ret != null)
						return true;
				}
			}

			return false;
		}

		internal static Bitmap GetScreen(Rectangle rect)
		{
			PixelFormat pFormat;

			switch (System.Windows.Forms.Screen.PrimaryScreen.BitsPerPixel)
			{
				case 8:
				case 16:
					pFormat = PixelFormat.Format16bppRgb565;
					break;

				case 24: pFormat = PixelFormat.Format24bppRgb; break;

				case 32: pFormat = PixelFormat.Format32bppArgb; break;

				default: pFormat = PixelFormat.Format32bppArgb; break;
			}

			try
			{
				var bmp = new Bitmap(rect.Width, rect.Height, pFormat);
				var g = Graphics.FromImage(bmp);
				g.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
				return bmp;
			}
			catch
			{
				var bmp2 = new Bitmap(0, 0, PixelFormat.Format24bppRgb);
				return bmp2;
			}
		}

		internal static string GuiId(ref string command)
		{
			var id = DefaultGuiId;

			if (command.Length == 0)
				return id;

			var z = command.IndexOf(':');
			var pre = string.Empty;

			if (z != -1)
			{
				pre = command.Substring(0, z).Trim();
				z++;
				command = z == command.Length ? string.Empty : command.Substring(z);
			}

			return pre.Length == 0 ? id : pre;
		}

		internal static void LV_ColOptions(ref ColumnHeader col, string options)
		{
		}

		internal static void LV_RowOptions(ref ListViewItem row, string options)
		{
			var opts = options.Split(new[] { ' ', '\t' }, StringSplitOptions.TrimEntries);

			for (var i = 0; i < opts.Length; i++)
			{
				var enable = true;
				var state = opts[i][0];

				if (state == '-')
					enable = false;

				if (!enable || state == '+')
					opts[i] = opts[i].Substring(1);

				var mode = opts[i].ToLowerInvariant();

				switch (mode)
				{
					case Core.Keyword_Check: row.Checked = enable; break;

					case Core.Keyword_Focus: row.Focused = enable; break;

					case Core.Keyword_Icon: row.ImageIndex = int.Parse(mode.AsSpan(4)); break;

					case Core.Keyword_Select: row.Selected = enable; break;

					case Core.Keyword_Vis: row.EnsureVisible(); break;
				}
			}
		}

		internal static void ParseAndApplyListViewColumnOptions(ColumnHeader col, string options)
		{
			var lvco = GuiHelper.ParseListViewColumnOptions(options);
			var lv = col.ListView as KeysharpListView;

			if (lvco.width.HasValue)
				col.Width = lvco.width.Value;

			if (lvco.auto.HasValue)
				lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

			if (lvco.autohdr.HasValue)
				lv.SetListViewColumnSizes();

			if (lvco.icon.HasValue)
				col.ImageIndex = lvco.icon.Value == 0 ? -1 : lvco.icon.Value;

			if (lvco.iconright.HasValue)
			{
				var colflags = new LV_COLUMN();
				colflags.mask = WindowsAPI.LVCF_FMT;
				_ = WindowsAPI.SendLVColMessage(lv.Handle, WindowsAPI.LVM_GETCOLUMN, (uint)col.Index, ref colflags);
				// Set the new format flags
				colflags.mask = WindowsAPI.LVCF_FMT | WindowsAPI.LVCF_IMAGE;

				if (lvco.iconright.Value)
					colflags.fmt |= WindowsAPI.LVCFMT_IMAGE | WindowsAPI.LVCFMT_BITMAP_ON_RIGHT;
				else
					colflags.fmt |= WindowsAPI.LVCFMT_IMAGE & ~WindowsAPI.LVCFMT_BITMAP_ON_RIGHT;

				_ = WindowsAPI.SendLVColMessage(lv.Handle, WindowsAPI.LVM_SETCOLUMN, (uint)col.Index, ref colflags);
			}

			if (lvco.inttype.HasValue)
			{
				lv.ListViewItemSorter = lvco.inttype.Value ? new ListViewIntegerComparer(col) : (System.Collections.IComparer)new ListViewCaseInsensitiveComparer(col);
				col.TextAlign = HorizontalAlignment.Right;
			}

			if (lvco.floattype.HasValue)
			{
				lv.ListViewItemSorter = lvco.floattype.Value ? new ListViewFloatComparer(col) : (System.Collections.IComparer)new ListViewCaseInsensitiveComparer(col);
				col.TextAlign = HorizontalAlignment.Right;
			}

			if (lvco.texttype.IsTrue())
				lv.ListViewItemSorter = new ListViewCaseInsensitiveComparer(col);

			//Note that due to a limitation in the underlying control, the first column will always be left aligned.
			if (lvco.center.HasValue)//These will also change the alignment of the header, but unsure how to separate the two.
				col.TextAlign = lvco.center.Value ? HorizontalAlignment.Center : HorizontalAlignment.Left;

			if (lvco.left.HasValue)
				col.TextAlign = lvco.left.Value ? HorizontalAlignment.Left : HorizontalAlignment.Right;

			if (lvco.right.HasValue)
				col.TextAlign = lvco.right.Value ? HorizontalAlignment.Right : HorizontalAlignment.Left;

			if (lvco.casesensitive.IsTrue())
				lv.ListViewItemSorter = new ListViewCaseSensitiveComparer(col);
			else if (lvco.caselocale.IsTrue())
				lv.ListViewItemSorter = new ListViewCaseLocaleComparer(col);
			else if (lvco.logical.IsTrue())
				lv.ListViewItemSorter = new ListViewLogicalComparer(col);
			else
				lv.ListViewItemSorter = new ListViewCaseInsensitiveComparer(col);

			if (lvco.desc.HasValue)
				lv.Sorting = lvco.desc.Value ? SortOrder.Descending : SortOrder.Ascending;

			if (lvco.nosort.HasValue)
				lv.Sorting = lvco.nosort.Value ? SortOrder.None : SortOrder.Descending;

			if (lvco.sort.IsTrue())
				lv.Sorting = SortOrder.Ascending;

			if (lvco.sortdesc.IsTrue())
				lv.Sorting = SortOrder.Descending;

			if (lvco.uni.HasValue)
				lv.uni = lvco.uni.Value;
		}

		internal static (string, List<Tuple<int, int, Tuple<string, string>>>) ParseLinkLabelText(string txt)
		{
			var sb = new StringBuilder(txt.Length);
			var splits = txt.Split(new string[] { "<a", "</a>" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var links = new List<Tuple<int, int, Tuple<string, string>>>();
			var quotes = new char[] { '\'', '\"' };

			foreach (var split in splits)
			{
				if (split.StartsWith("href=") || split.StartsWith("id="))
				{
					var id = "";
					var url = "";
					var pos = split.NthIndexOf("id=", 0, 1, StringComparison.OrdinalIgnoreCase);

					if (pos >= 0)
					{
						var idstartindex = split.NthIndexOfAny(quotes, pos, 1);
						var idstopindex = split.NthIndexOfAny(quotes, pos, 2);

						if (idstartindex >= 0 && idstopindex >= 0)
						{
							idstartindex++;
							id = split.Substring(idstartindex, idstopindex - idstartindex);
						}
					}

					var index1 = split.IndexOf('>') + 1;
					var linktext = split.Substring(index1);
					pos = split.NthIndexOf("href=", 0, 1, StringComparison.OrdinalIgnoreCase);
					index1 = split.NthIndexOfAny(quotes, pos, 1);
					var index2 = split.NthIndexOfAny(quotes, pos, 2);

					if (index1 >= 0 && index2 >= 0)
					{
						index1++;
						url = split.Substring(index1, index2 - index1);
					}

					links.Add(new Tuple<int, int, Tuple<string, string>>(sb.Length, linktext.Length, new Tuple<string, string>(id, url)));
					_ = sb.Append(linktext);
				}
				else
					_ = sb.Append(split);
			}

			var newtxt = sb.ToString();
			return (newtxt, links);
		}

		internal static ListViewColumnOptions ParseListViewColumnOptions(string options)
		{
			var lvco = new ListViewColumnOptions();

			foreach (var opt in Options.ParseOptions(options))
			{
				var temp = 0;
				var b = false;

				if (int.TryParse(opt, out var width)) { lvco.width = width; }
				else if (Options.TryParse(opt, "AutoHdr", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.autohdr = b; }
				else if (Options.TryParse(opt, "Auto", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.auto = b; }
				else if (string.Compare(opt, "-Icon", true) == 0) { lvco.icon = 0; }
				else if (Options.TryParse(opt, "IconRight", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.iconright = b; }
				else if (Options.TryParse(opt, "Icon", ref temp)) { lvco.icon = temp; }
				else if (Options.TryParse(opt, "Float", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.floattype = b; }
				else if (Options.TryParse(opt, "Integer", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.inttype = b; }
				else if (Options.TryParse(opt, "Text", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.texttype = b; }
				else if (Options.TryParse(opt, "Center", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.center = b; }
				else if (Options.TryParse(opt, "Left", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.left = b; }
				else if (Options.TryParse(opt, "Right", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.right = b; }
				else if (Options.TryParse(opt, "CaseLocale", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.caselocale = b; }
				else if (Options.TryParse(opt, "Case", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.casesensitive = b; }
				else if (Options.TryParse(opt, "Desc", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.desc = b; }
				else if (Options.TryParse(opt, "Logical", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.logical = b; }
				else if (Options.TryParse(opt, "NoSort", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.nosort = b; }
				else if (Options.TryParse(opt, "SortDesc", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.sortdesc = b; }
				else if (Options.TryParse(opt, "Sort", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.sort = b; }
				else if (Options.TryParse(opt, "Uni", ref b, StringComparison.OrdinalIgnoreCase, true, true)) { lvco.uni = b; }
			}

			return lvco;
		}

		internal static ListViewOptions ParseListViewOptions(string options)
		{
			var lvo = new ListViewOptions();

			foreach (var opt in Options.ParseOptions(options))
			{
				var temp = 0;

				if (Options.TryParse(opt, "Check", ref lvo.ischecked, StringComparison.OrdinalIgnoreCase, true, true)) { }
				else if (Options.TryParse(opt, "Focus", ref lvo.focused, StringComparison.OrdinalIgnoreCase, true, true)) { }
				else if (Options.TryParse(opt, "Select", ref lvo.select, StringComparison.OrdinalIgnoreCase, true, true)) { }
				else if (Options.TryParse(opt, "Col", ref temp)) { lvo.colstart = temp - 1; }
				else if (Options.TryParse(opt, "Icon", ref lvo.icon)) { }
				else if (string.Compare(opt, "Vis", true) == 0) { lvo.vis = true; }
			}

			return lvo;
		}

		internal static TreeNode TV_FindNode(TreeView parent, long id)
		{
			if (id == 0)
				return parent.TopNode;

			var match = parent.Nodes.Find(id.ToString(), true);
			return match.Length == 0 ? null : match[0];
		}

		internal static long TV_NodeOptions(TreeNode node, long parent, string options, bool modify)
		{
			var ret = node.Handle.ToInt64();

			if (string.IsNullOrEmpty(options))
				return ret;

			if (node.TreeView is KeysharpTreeView tv)
			{
				var bold = false;
				var ischecked = 0;
				var expanded = 0;
				//var selected = 0;
				var icon = int.MinValue;

				foreach (var opt in Options.ParseOptions(options))
				{
					if (string.Compare(opt, "-Bold", true) == 0) { node.NodeFont = new Font(tv.Font, FontStyle.Regular); }
					else if (Options.TryParse(opt, "Bold", ref bold)) { node.NodeFont = new Font(tv.Font, bold ? FontStyle.Bold : FontStyle.Regular); }
					else if (string.Compare(opt, "-Check", true) == 0) { node.Checked = false; }
					else if (Options.TryParse(opt, "Check", ref ischecked, StringComparison.OrdinalIgnoreCase, true)) { node.Checked = ischecked != 0; }
					else if (string.Compare(opt, "-Expand", true) == 0) { node.Collapse(); tv.RemoveMarkForExpansion(node); }
					else if (Options.TryParse(opt, "Expand", ref expanded, StringComparison.OrdinalIgnoreCase, true, 1))
					{
						if (expanded != 0)
						{
							if (modify)
							{
								if (node.Nodes.Count == 0)
									ret = 0;
								else
									node.Expand();
							}

							tv.MarkForExpansion(node);
						}
						else
						{
							node.Collapse();
							tv.RemoveMarkForExpansion(node);
						}
					}
					else if (string.Compare(opt, "Select ", true) == 0) { tv.SelectedNode = node; }
					else if (string.Compare(opt, "Vis", true) == 0) { node.EnsureVisible(); }
					else if (string.Compare(opt, "VisFirst", true) == 0) { node.EnsureVisible(); tv.TopNode = node; }
					else if (Options.TryParse(opt, "Icon", ref icon))
					{
						if (tv.ImageList != null)
						{
							if (icon < tv.ImageList.Images.Count)
							{
								node.ImageIndex = icon - 1;
								node.SelectedImageIndex = node.ImageIndex;
							}
							else
							{
								node.ImageIndex = -1;
								node.SelectedImageIndex = node.ImageIndex;
							}
						}
					}
					else if (string.Compare(opt, "Sort", true) == 0)
					{
						if (node.NextNode == null)
							ret = 0;
						else
							_ = tv.BeginInvoke(new MethodInvoker(tv.Sort));//BeginInvoke() is needed to avoid an infinte loop: https://stackoverflow.com/questions/808696/c-sharp-windows-form-treeview-sort-after-labeledit
					}
				}
			}

			return ret;
		}

		private static Control GuiControlGetFocused(Control parent)
		{
			foreach (Control child in parent.Controls)
			{
				if (child.Focused)
					return child;
				else if (child.Controls.Count != 0)
				{
					var item = GuiControlGetFocused(child);

					if (item != null)
						return item;
				}
			}

			return null;
		}

		//private static Control GuiFindControl(string name, Form gui)
		//{
		//  if (gui == null)
		//      return null;

		//  foreach (Control control in gui.Controls)
		//      if (control.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
		//          return control;

		//  return null;
		//}

		//private static void OnEditKeyPress(object sender, KeyPressEventArgs e)
		//{
		//  if (!(char.IsDigit(e.KeyChar)
		//          || char.IsNumber(e.KeyChar)
		//          || e.KeyChar == '.'
		//          || e.KeyChar == ','
		//          || (int)e.KeyChar == 8
		//          || (int)e.KeyChar == 58
		//          || (int)e.KeyChar == 59))
		//  {
		//      e.Handled = true;
		//  }
		//}

		internal static void SetListViewColumnSizes(this ListView lv, int width = -2)
		{
			foreach (ColumnHeader col in lv.Columns)
				col.Width = width;
		}

		internal class ListViewColumnOptions
		{
			public bool? auto;
			public bool? autohdr;
			public bool? caselocale;
			public bool? casesensitive;
			public bool? center;
			public bool? desc;
			public bool? floattype;
			public int? icon;
			public bool? iconright;
			public bool? inttype;
			public bool? left;
			public bool? logical;
			public bool? nosort;
			public bool? right;
			public bool? sort;
			public bool? sortdesc;
			public bool? texttype;
			public bool? uni;
			public int? width;
		}

		internal abstract class ListViewComparer : System.Collections.IComparer
		{
			protected ColumnHeader col;

			public ListViewComparer()
			{
			}

			public ListViewComparer(ColumnHeader column) => col = column;

			public abstract int Compare(object x, object y);

			public int SortOrder() => col.ListView.Sorting == System.Windows.Forms.SortOrder.Descending ? -1 : 1;
		}

		internal class ListViewOptions
		{
			public int colstart = 0;
			public bool focused = false;
			public int icon = int.MinValue;
			public bool ischecked = false;
			public bool select = false;
			public bool vis = false;
		}

		private class ListViewCaseInsensitiveComparer : ListViewComparer
		{
			public ListViewCaseInsensitiveComparer(ColumnHeader column)
				: base(column)
			{
			}

			public override int Compare(object l, object r) =>
			l is ListViewItem x&& r is ListViewItem y&& col.Index < x.SubItems.Count&& col.Index < y.SubItems.Count
			? SortOrder()* string.Compare(x.SubItems[col.Index].Text, y.SubItems[col.Index].Text, true)
			: 0;
		}

		private class ListViewCaseLocaleComparer : ListViewComparer
		{
			public ListViewCaseLocaleComparer(ColumnHeader column)
				: base(column)
			{
			}

			public override int Compare(object l, object r) =>
			l is ListViewItem x&& r is ListViewItem y&& col.Index < x.SubItems.Count&& col.Index < y.SubItems.Count
			? SortOrder()* string.Compare(x.SubItems[col.Index].Text, y.SubItems[col.Index].Text, StringComparison.CurrentCultureIgnoreCase)
			: 0;
		}

		private class ListViewCaseSensitiveComparer : ListViewComparer
		{
			public ListViewCaseSensitiveComparer(ColumnHeader column)
				: base(column)
			{
			}

			public override int Compare(object l, object r) =>
			l is ListViewItem x&& r is ListViewItem y&& col.Index < x.SubItems.Count&& col.Index < y.SubItems.Count
			? SortOrder()* string.Compare(x.SubItems[col.Index].Text, y.SubItems[col.Index].Text, StringComparison.Ordinal)
			: 0;
		}

		private class ListViewFloatComparer : ListViewComparer
		{
			public ListViewFloatComparer(ColumnHeader column)
				: base(column)
			{
			}

			public override int Compare(object l, object r)
			{
				if (l is ListViewItem x && r is ListViewItem y && col.Index < x.SubItems.Count && col.Index < y.SubItems.Count)
					if (double.TryParse(x.SubItems[col.Index].Text, out var i1) && double.TryParse(y.SubItems[col.Index].Text, out var i2))
						return (i1 < i2 ? -1 : i1 > i2 ? 1 : 0) * SortOrder();

				return 0;
			}
		}

		private class ListViewIntegerComparer : ListViewComparer
		{
			public ListViewIntegerComparer(ColumnHeader column)
				: base(column)
			{
			}

			public override int Compare(object l, object r)
			{
				if (l is ListViewItem x && r is ListViewItem y && col.Index < x.SubItems.Count && col.Index < y.SubItems.Count)
				{
					var i1 = x.SubItems[col.Index].Text.ParseLong(false);
					var i2 = y.SubItems[col.Index].Text.ParseLong(false);
					return (i1 < i2 ? -1 : i1 > i2 ? 1 : 0) * SortOrder();
				}

				return 0;
			}
		}

		private class ListViewLogicalComparer : ListViewComparer
		{
			public ListViewLogicalComparer(ColumnHeader column)
				: base(column)
			{
			}

			public override int Compare(object l, object r) =>
			l is ListViewItem x&& r is ListViewItem y&& col.Index < x.SubItems.Count&& col.Index < y.SubItems.Count
			? SortOrder()* NaturalComparer.NaturalCompare(x.SubItems[col.Index].Text, y.SubItems[col.Index].Text)
			: 0;
		}
	}
}