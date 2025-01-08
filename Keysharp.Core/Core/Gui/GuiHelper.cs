namespace Keysharp.Core
{
	public static class GuiHelper
	{
		internal static ConcurrentDictionary<long, MsgMonitor> onMessageHandlers = new ();

		internal static string DefaultGuiId
		{
			get => Threads.GetThreadVariables().defaultGui ?? "1";
			set => Threads.GetThreadVariables().defaultGui = value;
		}

		internal static Form DialogOwner
		{
			get => Threads.GetThreadVariables().dialogOwner;
			set => Threads.GetThreadVariables().dialogOwner = value;
		}

		public static Gui Gui(object obj0 = null, object obj1 = null, object obj2 = null) => new (obj0, obj1, obj2);

		public static object GuiCtrlFromHwnd(object obj)
		{
			if (Control.FromHandle(new IntPtr(obj.Al())) is Control c)
				if (c.GetGuiControl() is GuiControl gui)
					return gui;

			return "";
		}

		public static object GuiFromHwnd(object obj0, object obj1 = null)
		{
			var hwnd = obj0.Al();
			var recurse = obj1.Ab();

			if (Core.Gui.allGuiHwnds.TryGetValue(hwnd, out var gui))
				return gui;

			if (recurse)
			{
				if (Control.FromHandle(new IntPtr(hwnd)) is Control c)
				{
					while (c.Parent is Control cp)
					{
						if (Core.Gui.allGuiHwnds.TryGetValue(cp.Handle.ToInt64(), out gui))
							return gui;

						c = cp;
					}
				}
			}

			return "";
		}

		public static Menu Menu() => new ();

		public static MenuBar MenuBar() => new ();

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

		internal static bool CallMessageHandler(Control control, ref Message m)
		{
			if (Control.FromHandle(m.HWnd) == control)
			{
				if (control.GetGuiControl() is GuiControl ctrl)
				{
					var ret = ctrl.InvokeMessageHandlers(ref m);

					if (ret != null)
						return true;
				}
			}

			return false;
		}

		internal static Icon GetIcon(string source, int n)
		{
#if WINDOWS
			var prc = Process.GetCurrentProcess().Handle;
			var icon = WindowsAPI.ExtractIcon(prc, source, n);

			if (icon != IntPtr.Zero)
				return Icon.FromHandle(icon);

			return Icon.ExtractAssociatedIcon(source);
#else
			return null;
#endif
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

		internal static (string, List<Tuple<int, int, Tuple<string, string>>>) ParseLinkLabelText(string txt)
		{
			var sb = new StringBuilder(txt.Length);
			var splits = txt.Split(["<a", "</a>"], StringSplitOptions.RemoveEmptyEntries);//Do not trim splits here.
			var links = new List<Tuple<int, int, Tuple<string, string>>>();
			var quotes = new char[] { '\'', '\"' };

			foreach (var split in splits)
			{
				var trimSplit = split.TrimStart();

				if (trimSplit.StartsWith("href=") || trimSplit.StartsWith("id="))
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

		/// <summary>
		/// The Windows API functions have serious limitations when it comes to loading icons.
		/// They can't load any of size 256 or larger, plus they are platform specific.
		/// This loads the desired size and is cross platform.
		/// Gotten from https://www.codeproject.com/Articles/26824/Extract-icons-from-EXE-or-DLL-files
		/// </summary>
		/// <param name="icon"></param>
		/// <returns></returns>
		internal static List<Icon> SplitIcon(Icon icon)
		{
			Error err;

			if (icon == null)
				return Errors.ErrorOccurred(err = new UnsetError("Icon was null")) ? throw err : null;

			try
			{
				// Get an .ico file in memory, then split it into separate icons.
				byte[] src = null;

				using (var stream = new MemoryStream())
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
				return Errors.ErrorOccurred(err = new Error($"Error splitting icon: {e.Message}")) ? throw err : null;
			}
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
	}
}