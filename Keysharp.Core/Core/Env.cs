using static System.Windows.Forms.DataFormats;

namespace Keysharp.Core
{
	public static class Env
	{
		private static IEnumerable<string> dataFormats = typeof(DataFormats).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Select(f => f.Name);

		/// <summary>
		/// Gotten from AHK.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static ClipboardAll ClipboardAll(object obj0 = null, object obj1 = null)
		{
#if LINUX
			return new ClipboardAll();
#elif WINDOWS
			var data = obj0;
			var size = obj1.Al(long.MinValue);

			if (data is ClipboardAll a)
			{
				RestoreClipboardAll(a, size != long.MinValue ? size : (long)a.Size);
				return a;
			}

			using (var ms = new MemoryStream())
			{
				var dibToOmit = 0;
				var bw = new BinaryWriter(ms);
				var dataObject = Clipboard.GetDataObject();

				foreach (var format in dataObject.GetFormats())
				{
					var fi = ClipFormatStringToInt(format);

					switch (fi)
					{
						case WindowsAPI.CF_BITMAP:
						case WindowsAPI.CF_ENHMETAFILE:
						case WindowsAPI.CF_DSPENHMETAFILE:
							continue;//These formats appear to be specific handle types, not always safe to call GlobalSize() for.
					}

					if (fi == WindowsAPI.CF_TEXT || fi == WindowsAPI.CF_OEMTEXT || fi == dibToOmit)
						continue;

					if (dibToOmit == 0)
					{
						if (fi == WindowsAPI.CF_DIB)
							dibToOmit = WindowsAPI.CF_DIBV5;
						else if (fi == WindowsAPI.CF_DIBV5)
							dibToOmit = WindowsAPI.CF_DIB;
					}
				}

				foreach (var format in dataObject.GetFormats())
				{
					var fi = ClipFormatStringToInt(format);
					var nulldata = false;

					switch (fi)
					{
						case WindowsAPI.CF_BITMAP:
						case WindowsAPI.CF_ENHMETAFILE:
						case WindowsAPI.CF_DSPENHMETAFILE:
							// These formats appear to be specific handle types, not always safe to call GlobalSize() for.
							continue;
					}

					if (fi == WindowsAPI.CF_TEXT || fi == WindowsAPI.CF_OEMTEXT || fi == dibToOmit)
						continue;

					var buf = GetClipboardData(fi, ref nulldata);

					if (buf != null)
					{
					}
					else if (nulldata)
						buf = System.Array.Empty<byte>();//This format usually has null data.
					else
						continue;//GetClipboardData() failed: skip this format.

					bw.Write(fi);
					bw.Write(buf.Length);
					bw.Write(buf);
				}

				if (ms.Position > 0)
				{
					bw.Write(0);
					return new ClipboardAll(ms.ToArray());
				}
			}

			return new ClipboardAll(System.Array.Empty<byte>());
#endif
		}

		public static bool ClipWait(object obj0 = null, object obj1 = null)
		{
			//Will need to see if this works in a cross platform way.//TODO
			var timeout = obj0.Ad(double.MinValue);
			var type = obj1.Ab();
			var checktime = timeout != double.MinValue;
			var frequency = 100;
			var time = checktime ? (long)(Math.Abs(timeout) * 1000) : long.MaxValue;

			for (var i = 0L; !checktime || i < time; i += frequency)
			{
				if (!type ? Clipboard.ContainsText()
#if WINDOWS
				 || Clipboard.ContainsFileDropList()
#endif
				 : !IsClipboardEmpty())
					return true;

				Keysharp.Core.Flow.Sleep(frequency);
			}

			return false;
		}

		/// <summary>
		/// Retrieves an environment variable.
		/// </summary>
		/// <param name="name">The name of the environment variable to retrieve.</param>
		/// <returns>The value of the specified environment variable if it exists, else empty string.</returns>
		public static string EnvGet(object obj) => Environment.GetEnvironmentVariable(obj.As()) ?? string.Empty;

		/// <summary>
		/// Writes a value to a variable contained in the environment.
		/// </summary>
		/// <param name="name">Name of the environment variable to use, e.g. <c>PATH</c>.</param>
		/// <param name="value">Value to set the environment variable to.</param>
		public static void EnvSet(object obj0, object obj1 = null) => Environment.SetEnvironmentVariable(obj0.As(), obj1 as string);

		/// <summary>
		/// Notifies the operating system and all running applications that environment variables have changed.
		/// </summary>
		public static void EnvUpdate()
		{
#if LINUX
			"source ~/.bashrc".Bash();
#elif WINDOWS

			//SendMessage() freezes when running in a unit test. PostMessage seems to work. Use SendMessageTimeout().
			try { _ = WindowsAPI.SendMessageTimeout(new IntPtr(WindowsAPI.HWND_BROADCAST), WindowsAPI.WM_SETTINGCHANGE, 0u, IntPtr.Zero, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 1000, out var result); }
			catch (Exception ex) { throw new OSError(ex); }

#endif
		}

		public static string FindCommandLineArg(string arg, bool startswith = true)
		{
			if (startswith)
				return Environment.GetCommandLineArgs().FirstOrDefault(x => (x.StartsWith('-')
						|| x.StartsWith('/')) && x.Trim(Keywords.DashSlash).StartsWith(arg, StringComparison.OrdinalIgnoreCase));
			else
				return Environment.GetCommandLineArgs().FirstOrDefault(x => (x.StartsWith('-')
						|| x.StartsWith('/')) && x.Trim(Keywords.DashSlash).Contains(arg, StringComparison.OrdinalIgnoreCase));
		}

		public static string FindCommandLineArgVal(string arg, bool startswith = true)
		{
			var args = Environment.GetCommandLineArgs();

			for (var i = 0; i < args.Length; i++)
			{
				if ((args[i].StartsWith('-') || args[i].StartsWith('/')) && args[i].StartsWith(arg, StringComparison.OrdinalIgnoreCase))
					if (i < args.Length - 1)
						return args[i + 1];
			}

			return null;
		}

		public static void HandleCommandLineParams(string[] args) => Keysharp.Core.Accessors.A_Args.AddRange(args);

		/// <summary>
		/// The clipboard object doesn't provide a way to determine if it's truly empty or not.
		/// This method taken from: https://www.codeproject.com/questions/1089557/check-with-csharp-if-the-clipboard-is-really-empty
		/// attempts to provide such functionality.
		/// </summary>
		/// <returns>True if empty, else false.</returns>
		public static bool IsClipboardEmpty() => !dataFormats.Any(Clipboard.ContainsData);

		public static void OnClipboardChange(object obj0, object obj1 = null)
		{
			if (obj0 is IFuncObj fo)
				Script.ClipFunctions.ModifyEventHandlers(fo, obj1.Al(1));
			else
				throw new TypeError("Object passed to OnClipboardChange() was not a function object.");
		}

		/// <summary>
		/// Retrieves screen resolution, multi-monitor info, dimensions of system objects, and other system properties.
		/// </summary>
		/// <param name="output">The variable to store the result.</param>
		/// <param name="command"></param>
		/// <param name="param"></param>
		public static object SysGet(object obj)
		{
#if LINUX
			var sm = obj is Keysharp.Core.Common.Platform.SystemMetric en ? en : (SystemMetric)obj.Ai();

			switch (sm)
			{
				case SystemMetric.SM_CXSCREEN:
					return (long)SystemInformation.WorkingArea.Width;

				case SystemMetric.SM_CYSCREEN:
					return (long)SystemInformation.WorkingArea.Height;

				case SystemMetric.SM_CXVSCROLL:
					return (long)SystemInformation.VerticalScrollBarWidth;

				case SystemMetric.SM_CYHSCROLL:
					return (long)SystemInformation.HorizontalScrollBarHeight;

				case SystemMetric.SM_CYCAPTION:
					return (long)SystemInformation.CaptionHeight;

				case SystemMetric.SM_CXBORDER:
					return (long)SystemInformation.BorderSize.Width;

				case SystemMetric.SM_CYBORDER:
					return (long)SystemInformation.BorderSize.Height;

				case SystemMetric.SM_CXDLGFRAME:
					return (long)SystemInformation.FixedFrameBorderSize.Width;

				//case SystemMetric.SM_CXFIXEDFRAME:
				//break;
				case SystemMetric.SM_CYDLGFRAME:
					return (long)SystemInformation.FixedFrameBorderSize.Height;

				//case SystemMetric.SM_CYFIXEDFRAME:
				//break;
				case SystemMetric.SM_CYVTHUMB:
					return 0L;

				case SystemMetric.SM_CXHTHUMB:
					return 0L;

				case SystemMetric.SM_CXICON:
					return (long)SystemInformation.IconSize.Width;

				case SystemMetric.SM_CYICON:
					return (long)SystemInformation.IconSize.Height;

				case SystemMetric.SM_CXCURSOR:
					return (long)SystemInformation.CursorSize.Width;

				case SystemMetric.SM_CYCURSOR:
					return (long)SystemInformation.CursorSize.Height;

				case SystemMetric.SM_CYMENU:
					return (long)SystemInformation.MenuHeight;

				case SystemMetric.SM_CXFULLSCREEN:
					return 0L;

				case SystemMetric.SM_CYFULLSCREEN:
					return 0L;

				case SystemMetric.SM_CYKANJIWINDOW:
					return 0L;

				case SystemMetric.SM_MOUSEPRESENT:
					return SystemInformation.MousePresent ? 1L : 0L;

				case SystemMetric.SM_CYVSCROLL:
					return (long)SystemInformation.VerticalScrollBarArrowHeight;

				case SystemMetric.SM_CXHSCROLL:
					return (long)SystemInformation.HorizontalScrollBarArrowWidth;

				case SystemMetric.SM_DEBUG:
					return 0L;

				case SystemMetric.SM_SWAPBUTTON:
					return MouseButtonsSwapped() ? 1L : 0L;

				case SystemMetric.SM_CXMIN:
					return (long)SystemInformation.MinimumWindowSize.Width;

				case SystemMetric.SM_CYMIN:
					return (long)SystemInformation.MinimumWindowSize.Height;

				case SystemMetric.SM_CXSIZE:
					return (long)SystemInformation.CaptionButtonSize.Width;

				case SystemMetric.SM_CYSIZE:
					return (long)SystemInformation.CaptionButtonSize.Height;

				case SystemMetric.SM_CXSIZEFRAME:
					return (long)SystemInformation.HorizontalResizeBorderThickness;

				//case SystemMetric.SM_CXFRAME:
				//break;
				case SystemMetric.SM_CYSIZEFRAME:
					return (long)SystemInformation.VerticalResizeBorderThickness;

				//case SystemMetric.SM_CYFRAME:
				//break;
				case SystemMetric.SM_CXMINTRACK:
					return (long)SystemInformation.MinWindowTrackSize.Width;

				case SystemMetric.SM_CYMINTRACK:
					return (long)SystemInformation.MinWindowTrackSize.Height;

				case SystemMetric.SM_CXDOUBLECLK:
					return (long)SystemInformation.DoubleClickSize.Width;

				case SystemMetric.SM_CYDOUBLECLK:
					return (long)SystemInformation.DoubleClickSize.Height;

				case SystemMetric.SM_CXICONSPACING:
					return 0L;

				case SystemMetric.SM_CYICONSPACING:
					return 0L;

				case SystemMetric.SM_MENUDROPALIGNMENT:
					return 0L;

				case SystemMetric.SM_PENWINDOWS:
					return 0L;

				case SystemMetric.SM_DBCSENABLED:
					return 1L;

				case SystemMetric.SM_CMOUSEBUTTONS:
					return MouseButtonCount();

				case SystemMetric.SM_SECURE:
					return 1L;

				case SystemMetric.SM_CXEDGE:
					return (long)SystemInformation.Border3DSize.Width;

				case SystemMetric.SM_CYEDGE:
					return (long)SystemInformation.Border3DSize.Height;

				case SystemMetric.SM_CXMINSPACING:
					return (long)SystemInformation.MinimizedWindowSpacingSize.Width;

				case SystemMetric.SM_CYMINSPACING:
					return (long)SystemInformation.MinimizedWindowSpacingSize.Height;

				case SystemMetric.SM_CXSMICON:
					return (long)SystemInformation.SmallIconSize.Width;

				case SystemMetric.SM_CYSMICON:
					return (long)SystemInformation.SmallIconSize.Height;

				case SystemMetric.SM_CYSMCAPTION:
					return (long)SystemInformation.ToolWindowCaptionHeight;

				case SystemMetric.SM_CXSMSIZE:
					return (long)SystemInformation.ToolWindowCaptionButtonSize.Width;

				case SystemMetric.SM_CYSMSIZE:
					return (long)SystemInformation.ToolWindowCaptionButtonSize.Height;

				case SystemMetric.SM_CXMENUSIZE:
					return (long)SystemInformation.MenuBarButtonSize.Width;

				case SystemMetric.SM_CYMENUSIZE:
					return (long)SystemInformation.MenuBarButtonSize.Height;

				case SystemMetric.SM_ARRANGE:
					return 0L;

				case SystemMetric.SM_CXMINIMIZED:
					return (long)SystemInformation.MinimizedWindowSize.Width;

				case SystemMetric.SM_CYMINIMIZED:
					return (long)SystemInformation.MinimizedWindowSize.Height;

				case SystemMetric.SM_CXMAXTRACK:
					return (long)SystemInformation.MaxWindowTrackSize.Width;

				case SystemMetric.SM_CYMAXTRACK:
					return (long)SystemInformation.MaxWindowTrackSize.Height;

				case SystemMetric.SM_CXMAXIMIZED:
					return System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;

				case SystemMetric.SM_CYMAXIMIZED:
					return System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

				case SystemMetric.SM_NETWORK:
					return NetworkUp() ? 1L : 0L;

				case SystemMetric.SM_CLEANBOOT:
				{
					var boots = "last reboot".Bash().SplitLines().ToList();

					if (boots.Count > 0)
					{
						if (boots[0].Contains("recovery", StringComparison.OrdinalIgnoreCase))
							return NetworkUp() ? 2L : (object)1L;
					}

					return 0L;
				}

				case SystemMetric.SM_CXDRAG:
					return (long)SystemInformation.DragSize.Width;

				case SystemMetric.SM_CYDRAG:
					return (long)SystemInformation.DragSize.Height;

				case SystemMetric.SM_SHOWSOUNDS:
					return 0L;

				case SystemMetric.SM_CXMENUCHECK:
					return 0L;

				case SystemMetric.SM_CYMENUCHECK:
					return 0L;

				case SystemMetric.SM_SLOWMACHINE:
					return 0L;

				case SystemMetric.SM_MIDEASTENABLED:
					return 0L;

				case SystemMetric.SM_MOUSEWHEELPRESENT:
					return "xinput --list --long".Bash().Contains("button wheel", StringComparison.OrdinalIgnoreCase) ? 1L : 0L;

				case SystemMetric.SM_XVIRTUALSCREEN:
					return (long)SystemInformation.VirtualScreen.Left;

				case SystemMetric.SM_YVIRTUALSCREEN:
					return (long)SystemInformation.VirtualScreen.Top;

				case SystemMetric.SM_CXVIRTUALSCREEN:
					return (long)SystemInformation.VirtualScreen.Width;

				case SystemMetric.SM_CYVIRTUALSCREEN:
					return (long)SystemInformation.VirtualScreen.Height;

				case SystemMetric.SM_CMONITORS:
					return (long)SystemInformation.MonitorCount;

				case SystemMetric.SM_SAMEDISPLAYFORMAT:
					return 1L;

				case SystemMetric.SM_IMMENABLED:
					return 0L;

				case SystemMetric.SM_CXFOCUSBORDER:
					return 0L;

				case SystemMetric.SM_CYFOCUSBORDER:
					return 0L;

				case SystemMetric.SM_TABLETPC:
					return 0L;

				case SystemMetric.SM_MEDIACENTER:
					return 0L;

				case SystemMetric.SM_STARTER:
					return 0L;

				case SystemMetric.SM_SERVERR2:
					return 0L;

				case SystemMetric.SM_MOUSEHORIZONTALWHEELPRESENT:
					return 0L;

				case SystemMetric.SM_CXPADDEDBORDER:
					return 0L;

				case SystemMetric.SM_DIGITIZER:
					return 0L;

				case SystemMetric.SM_MAXIMUMTOUCHES:
					return 0L;

				case SystemMetric.SM_REMOTESESSION:
					return "echo $SSH_TTY".Bash() != "" ? 1L : 0L;

				case SystemMetric.SM_SHUTTINGDOWN:
					return "systemctl is-system-running".Bash().Contains("stopping", StringComparison.OrdinalIgnoreCase) ? 1L : 0L;

				case SystemMetric.SM_REMOTECONTROL:
					return "echo $SSH_TTY".Bash() != "" ? 1L : 0L;

				case SystemMetric.SM_CONVERTIBLESLATEMODE:
					return 0L;

				case SystemMetric.SM_SYSTEMDOCKED:
					return 0L;

				default:
					break;
			}

			return 0L;
#elif WINDOWS

			if (obj is Keysharp.Core.Common.Platform.SystemMetric en)
				return Keysharp.Core.Windows.WindowsAPI.GetSystemMetrics(en);

			return Keysharp.Core.Windows.WindowsAPI.GetSystemMetrics((SystemMetric)obj.Ai());
#else
			return 0L;
#endif
		}

		internal static int ClipFormatStringToInt(string fmt) => DataFormats.GetFormat(fmt) is DataFormats.Format d ? d.Id : 0;

		/// <summary>
		/// Gets the data on the clipboard in the specified format.
		/// Gotten from: http://pinvoke.net/default.aspx/user32/GetClipboardData.html
		/// </summary>
		/// <param name="format"></param>
		/// <param name="nulldata"></param>
		/// <returns></returns>
		internal static byte[] GetClipboardData(int format, ref bool nulldata)
		{
			if (format != 0)
			{
#if WINDOWS

				if (WindowsAPI.OpenClipboard((long)Accessors.A_ClipboardTimeout))
				{
					byte[] buf;
					var gLock = IntPtr.Zero;

					try
					{
						var clipdata = WindowsAPI.GetClipboardData(format, ref nulldata);//Get pointer to clipboard data in the selected format.
						var length = (int)WindowsAPI.GlobalSize(clipdata);
						gLock = WindowsAPI.GlobalLock(clipdata);
						buf = new byte[length];

						if (length != 0)
							Marshal.Copy(gLock, buf, 0, length);
					}
					finally
					{
						_ = WindowsAPI.GlobalUnlock(gLock);
						_ = WindowsAPI.CloseClipboard();
					}

					return buf;
				}

#elif LINUX
				throw new NotImplementedException();
#endif
			}

			return null;
		}

		//internal static DataFormats.Format IntToClipFormat(int i) => DataFormats.GetFormat(i);

#if LINUX
		/// <summary>
		/// Get the number of buttons on the mouse.
		/// This tries to find the device with the least number of buttons and assumes that is the mouse.
		/// XTEST devices are considered but it shouldn't matter because they won't have less buttons than
		/// the actual mouse.
		/// </summary>
		/// <returns>The number of mouse buttons detected</returns>
		internal static long MouseButtonCount()
		{
			var count = long.MaxValue;
			var inputStr = "xinput list --long".Bash();
			var inputStrSplits = inputStr.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			foreach (var split in inputStrSplits)
			{
				if (split.Contains("Buttons supported:"))
				{
					var btnSplit = split.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

					if (btnSplit.Length > 1 && long.TryParse(btnSplit[1], out var btnCount))
					{
						//count = Math.Max(count, btnCount);
						count = Math.Min(count, btnCount);
					}
				}
			}

			return count;
		}

		/// <summary>
		/// Detect whether the buttons on any mouse are swapped. This function is needed because Mono's Winforms
		/// hard codes SystemInformation.MouseButtonsSwapped to false for linux.
		/// This will break on the first device with buttons swapped, which may or may not be the actual mouse.
		/// Unsure how to find the "actual" mouse device though. Should be good enough for our purposes.
		/// This also excludes any device with "XTEST" in the name.
		/// </summary>
		/// <returns>Whether any mouse was found to have any buttons swapped</returns>
		internal static bool MouseButtonsSwapped()
		{
			var swapped = false;
			var deviceNames = "xinput list --name-only".Bash().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			//foreach (var name in deviceNames)
			//  Keysharp.Scripting.Script.OutputDebug($"{name}");
			var deviceIds = "xinput list --id-only".Bash().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			//foreach (var id in deviceIds)
			//  Keysharp.Scripting.Script.OutputDebug($"{id}");

			if (deviceNames.Length == deviceIds.Length)
			{
				for (var i = 0; i < deviceNames.Length && !swapped; i++)
				{
					if (!deviceNames[i].Contains("xtest", StringComparison.OrdinalIgnoreCase))
					{
						var buttonStr = $"xinput get-button-map {deviceIds[i]}".Bash();
						var buttonStrSplits = buttonStr.Split(Keywords.SpaceTab, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

						if (buttonStrSplits.All(sp => int.TryParse(sp, out var _)))
						{
							//Keysharp.Scripting.Script.OutputDebug($"Device {deviceIds[i]}: {deviceNames[i]} with buttons {buttonStr} getting examined.");
							for (var j = 0; j < 3 && j < buttonStrSplits.Length; j++)
							{
								if (int.TryParse(buttonStrSplits[j], out var btn))
								{
									if (btn != j + 1)
									{
										swapped = true;
										//Keysharp.Scripting.Script.OutputDebug($"\tWas swapped.");
										break;
									}
								}
								else
									break;
							}
						}
					}
				}
			}

			return swapped;
		}

		internal static bool NetworkUp() => "ip link show".Bash().Contains("state up", StringComparison.OrdinalIgnoreCase);

#endif

		internal static void RestoreClipboardAll(ClipboardAll clip, long length)
		{
			unsafe
			{
#if LINUX
				clip.Restore();
#elif WINDOWS
				var wasOpened = false;

				try
				{
					if (WindowsAPI.OpenClipboard((long)Accessors.A_ClipboardTimeout))//Need to leave it open for it to work when using the Windows API.
					{
						wasOpened = true;
						var ptr = clip.Ptr;
						length = Math.Min(Math.Max(0U, length), (long)clip.Size);

						for (var index = 0; index < length;)
						{
							var cliptype = (uint)Marshal.ReadInt32(ptr, index);

							if (cliptype == 0)
								break;

							index += 4;
							var size = Marshal.ReadInt32(ptr, index);
							index += 4;

							if (index + size < length)
							{
								var hglobal = Marshal.AllocHGlobal(size);
								System.Buffer.MemoryCopy((ptr + index).ToPointer(), hglobal.ToPointer(), size, size);
								_ = WindowsAPI.SetClipboardData(cliptype, hglobal);
								//Do not free hglobal here.
								index += size;
							}
						}
					}
				}
				finally
				{
					if (wasOpened)
						_ = WindowsAPI.CloseClipboard();
				}


#endif
			}
		}
	}

#if LINUX
	/// <summary>
	/// Taken from: https://stackoverflow.com/questions/6262454/c-sharp-backing-up-and-restoring-clipboard
	/// </summary>
	public class ClipboardAll : KeysharpObject
	{
		Dictionary<string, object> backup = new Dictionary<string, object>();
		IDataObject dataObject = Clipboard.GetDataObject();
		string[] formats;

		public ClipboardAll()
		{
			Save();
		}

		internal void Save()
		{
			backup = new Dictionary<string, object>();
			dataObject = Clipboard.GetDataObject();
			formats = dataObject.GetFormats(false);

			foreach (var lFormat in formats)
				backup.Add(lFormat, dataObject.GetData(lFormat, false));
		}

		internal void Restore()
		{
			dataObject = new DataObject();

			foreach (var format in formats)
				dataObject.SetData(format, backup[format]);

			Clipboard.SetDataObject(dataObject);
		}
	}
#elif WINDOWS
	public class ClipboardAll : Buffer
	{
		public ClipboardAll(byte[] obj)
			: base(obj)
		{
		}
	}
#endif

}