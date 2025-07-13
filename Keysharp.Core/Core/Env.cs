using static System.Windows.Forms.DataFormats;

namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for environment-related functions.
	/// </summary>
	public static class Env
	{
		/// <summary>
		/// Creates an object containing everything on the clipboard (such as pictures and formatting).
		/// Omit both parameters to retrieve the current contents of the clipboard.<br/>
		/// Otherwise, specify one or both parameters to create an object containing the given binary clipboard data.
		/// </summary>
		/// <param name="data">A Buffer-like object or a pure integer which is the address of the binary data.<br/>
		/// The data must be in a specific format, so typically originates from a previous call to ClipboardAll.
		/// </param>
		/// <param name="size">The number of bytes of data to use. This is optional when data is an object.</param>
		/// <returns>A <see cref="ClipboardAll"/> object containing the clipboard data.</returns>
		public static ClipboardAll ClipboardAll(object data = null, object size = null)
		{
			//Need to see if this should be put on the main thread like A_Clipboard.//TODO
#if LINUX
			return new ClipboardAll();
#elif WINDOWS
			var d = data;
			var s = size.Al(long.MinValue);

			if (d is ClipboardAll a)
			{
				RestoreClipboardAll(a, s != long.MinValue ? s : (long)a.Size);
				return a;
			}

			using (var ms = new MemoryStream())
			{
				var dibToOmit = 0;
				var bw = new BinaryWriter(ms);
				var dataObject = Clipboard.GetDataObject();

				if (dataObject != null)
				{
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
							buf = [];//This format usually has null data.
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
			}

			return new ClipboardAll(System.Array.Empty<byte>());
#else
			return null;
#endif
		}

		/// <summary>
		/// Waits until the clipboard contains data.
		/// </summary>
		/// <param name="tiemout">If omitted, the function will wait indefinitely. Otherwise, it will wait no longer than this many seconds.<br/>
		/// To wait for a fraction of a second, specify a floating-point number, for example, 0.25 to wait for a maximum of 250 milliseconds.
		/// </param>
		/// <param name="waitFor">If omitted, it defaults to 0 (wait only for text or files).<br/>
		/// Otherwise, specify one of the following numbers to indicate what to wait for:<br/>
		/// 0: The function is more selective, waiting specifically for text or files to appear("text" includes anything that would produce text when you paste into Notepad).<br/>
		/// 1: The function waits for data of any kind to appear on the clipboard.
		/// </param>
		/// <returns>True if it did not time out, else false.</returns>
		public static bool ClipWait(object tiemout = null, object waitFor = null)
		{
			//Need to see if this should be put on the main thread like A_Clipboard.//TODO
			var to = tiemout.Ad(double.MinValue);
			var type = waitFor.Ab();
			var checktime = to != double.MinValue;
			var frequency = 100;
			var time = checktime ? (long)(Math.Abs(to) * 1000) : long.MaxValue;

			for (var i = 0L; !checktime || i < time; i += frequency)
			{
				if (!type ? Clipboard.ContainsText()
#if WINDOWS
						|| Clipboard.ContainsFileDropList()
#endif
						: !KeysharpEnhancements.IsClipboardEmpty())
					return true;

				Flow.Sleep(frequency);
			}

			return false;
		}

		/// <summary>
		/// Retrieves the value of the specified environment variable.
		/// </summary>
		/// <param name="name">The name of the environment variable to retrieve.</param>
		/// <returns>The value of the specified environment variable if it exists, else empty string.</returns>
		public static string EnvGet(object name) => Environment.GetEnvironmentVariable(name.As()) ?? string.Empty;

		/// <summary>
		/// Writes a value to the specified environment variable.
		/// </summary>
		/// <param name="name">The name of the environment variable.</param>
		/// <param name="value">If omitted, the environment variable will be deleted. Otherwise, specify the value to write.</param>
		/// <exception cref="OSError">An <see cref="OSError"/> exception is thrown if any failure is detected.</exception>
		public static object EnvSet(object name, object value = null)
		{
			try
			{
				Environment.SetEnvironmentVariable(name.As(), value as string);
				return DefaultObject;
			}
			catch (Exception ex)
			{
				return Errors.OSErrorOccurred(ex, $@"Error setting environment variable {name} to value ""{value}"".");
			}
		}

		/// <summary>
		/// Notifies the operating system and all running applications that environment variables have changed.
		/// </summary>
		/// <exception cref="OSError">An <see cref="OSError"/> exception is thrown on Windows if any failure is detected.</exception>
		public static object EnvUpdate()
		{
#if LINUX
			"source ~/.bashrc".Bash();
#elif WINDOWS

			//SendMessage() freezes when running in a unit test. PostMessage seems to work. Use SendMessageTimeout().
			try { _ = WindowsAPI.SendMessageTimeout(new nint(WindowsAPI.HWND_BROADCAST), WindowsAPI.WM_SETTINGCHANGE, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 1000, out var result); }
			catch (Exception ex)
			{
				return Errors.OSErrorOccurred(ex, "Error updating environment variables.");
			}

#endif
			return DefaultObject;
		}

		/// <summary>
		/// Assign command line arguments to <see cref="A_Args"/>.
		/// This should never be called directly by a Script.TheScript.<br/>
		/// Instead, it's used by the parser in the generated C# code.
		/// </summary>
		/// <param name="args">The command line arguments to process.</param>
		public static object HandleCommandLineParams(string[] args)
		{
			if (args.Length > 0 && args[0].TrimStart(Keywords.DashSlash).ToUpper() == "SCRIPT")
			{
				string[] newArgs = new string[args.Length - 1];
				System.Array.Copy(args, 1, newArgs, 0, args.Length - 1);
				Environment.ExitCode = Runner.Run(args);
				throw new Flow.UserRequestedExitException();
			}

			_ = A_Args.AddRange(args);
			return DefaultObject;
		}

		/// <summary>
		/// Compiles and executes a C# script dynamically in a separate process.
		/// </summary>
		/// <param name="obj0">The script source result (as any object with a valid string representation).</param>
		/// <param name="obj1">Whether to run the process as async (provide non-unset non-zero value) or not.
		/// <param name="obj2">An optional name for the dynamically generated program; defaults to "DynamicScript".</param>
		/// <param name="obj3">Optional executable path used to run the generated assembly; defaults to the currently running process.</param>
		/// If provided a callback function then it's considered async and the function <c>Call</c> method will be
		/// invoked when the process exits with the ProcessInfo as the only argument.</param>
		/// <returns>
		/// Returns a <see cref="ProcessInfo"/> wrapper around the spawned process.
		/// If compilation fails without a flagged error, returns <c>null</c>.
		/// </returns>
		/// <exception cref="Error">Throws any compilation as <see cref="Error"/>.</exception>
		public static object RunScript(object obj0, object obj1 = null, object obj2 = null, object obj3 = null)
		{
			string script = obj0.As();
			IFuncObj cb = null;

			if (obj1 != null)
				cb = Functions.Func(obj1);

			string name = obj2.As("DynamicScript");
			string result = null;
			byte[] compiledBytes = null;
			var ch = new CompilerHelper();
			(compiledBytes, result) = ch.CompileCodeToByteArray([script], name);

			if (compiledBytes == null)
				return Errors.ErrorOccurred(result);

			var scriptProcess = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = obj3 == null ? Path.GetFileName(Environment.ProcessPath) : obj3.As(),
					Arguments = "--script --assembly *",
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};
			var info = new ProcessInfo(scriptProcess);
			scriptProcess.EnableRaisingEvents = true;
			scriptProcess.Exited += (object sender, EventArgs e) => cb?.Call(info);
			_ = scriptProcess.Start();

			using (var writer = new BinaryWriter(scriptProcess.StandardInput.BaseStream, Encoding.UTF8, leaveOpen: true))
			{
				writer.Write(compiledBytes.Length);
				writer.Write(compiledBytes);
				writer.Flush();
			}

			if (!ForceBool(obj1 ?? false))
				scriptProcess.WaitForExit();

			return info;
		}

		/// <summary>
		/// Registers a function to be called automatically whenever the clipboard's content changes.
		/// </summary>
		/// <param name="callback">The callback to call, which has a single parameter specifying which data type the clipboard contains.<br/>
		/// 0: Clipboard is now empty.<br/>
		/// 1: Clipboard contains something that can be expressed as text(this includes files copied from an Explorer window).<br/>
		/// 2: Clipboard contains something entirely non-text such as a picture.<br/>
		/// </param>
		/// <param name="addRemove">If omitted, it defaults to 1. Otherwise, specify one of the following numbers:<br/>
		///  1: Call the callback after any previously registered callbacks.<br/>
		/// -1: Call the callback before any previously registered callbacks.<br/>
		///  0: Do not call the callback.
		/// </param>
		/// <exception cref="TypeError">A <see cref="TypeError"/> exception is thrown if callback is not of type <see cref="FuncObj"/>.</exception>
		public static object OnClipboardChange(object callback, object addRemove = null)
		{
			if (callback is IFuncObj fo)
			{
				Script.TheScript.ClipFunctions.ModifyEventHandlers(fo, addRemove.Al(1));
				return DefaultObject;
			}
			else
				return Errors.TypeErrorOccurred(callback, typeof(FuncObj), DefaultErrorObject);
		}

		/// <summary>
		/// Retrieves dimensions of system objects, and other system properties.
		/// </summary>
		/// <param name="property">The variable to store the result.</param>
		/// <returns>This function returns the value of the specified system property.</returns>
		public static object SysGet(object property)
		{
#if LINUX
			var sm = property is Keysharp.Core.Common.Platform.SystemMetric en ? en : (SystemMetric)property.Ai();

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

			if (property is SystemMetric en)
				return WindowsAPI.GetSystemMetrics(en);

			return WindowsAPI.GetSystemMetrics((SystemMetric)property.Ai());
#else
			return 0L;
#endif
		}

		/// <summary>
		/// Internal helper to convert a clipboard integer format to a string.
		/// </summary>
		/// <param name="fmt">The format to convert.</param>
		/// <returns>The string representation of the specified clipboard format.</returns>
		internal static int ClipFormatStringToInt(string fmt) => GetFormat(fmt) is Format d ? d.Id : 0;

		/// <summary>
		/// Internal helper to search the command line arguments for a specified string.
		/// </summary>
		/// <param name="arg">The argument to search for.</param>
		/// <param name="startsWith">True to require the argument to start with arg, else it must contain arg.</param>
		/// <returns>The matched argument if found, else null.</returns>
		internal static string FindCommandLineArg(string arg, bool startsWith = true)
		{
			if (startsWith)
				return Script.TheScript.KeysharpArgs.FirstOrDefault(x => (x.StartsWith('-')
						|| x.StartsWith('/')) && x.Trim(DashSlash).StartsWith(arg, StringComparison.OrdinalIgnoreCase));
			else
				return Script.TheScript.KeysharpArgs.FirstOrDefault(x => (x.StartsWith('-')
						|| x.StartsWith('/')) && x.Trim(DashSlash).Contains(arg, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Internal helper to search the command line argument values for a specified string.
		/// </summary>
		/// <param name="arg">The argument to search for.</param>
		/// <param name="startsWith">True to require the argument to start with arg, else it must contain arg.</param>
		/// <returns>The matched argument if found, else null.</returns>
		/// <returns>The matched value if found, else null.</returns>
		internal static string FindCommandLineArgVal(string arg, bool startsWith = true)
		{
			var args = Script.TheScript.KeysharpArgs;

			for (var i = 0; i < args.Length; i++)
			{
				if ((args[i].StartsWith('-') || args[i].StartsWith('/')) && (startsWith ? args[i].TrimStart(DashSlash).StartsWith(arg, StringComparison.OrdinalIgnoreCase) : args[i].Contains(arg, StringComparison.OrdinalIgnoreCase)))
					if (i < args.Length - 1)
						return args[i + 1];
			}

			return DefaultObject;
		}

#if WINDOWS

		/// <summary>
		/// Internal helper to get the data on the clipboard in the specified format.
		/// Gotten from: http://pinvoke.net/default.aspx/user32/GetClipboardData.html
		/// </summary>
		/// <param name="format">The numerical format of the data to retireve.</param>
		/// <param name="nullData">True if null data is ok, else false.</param>
		/// <returns>The retrieved data as a byte array, else null on error.</returns>
		internal static byte[] GetClipboardData(int format, ref bool nullData)
		{
			if (format != 0)
			{
				if (WindowsAPI.OpenClipboard(A_ClipboardTimeout.Al()))
				{
					byte[] buf;
					nint gLock = 0;

					try
					{
						var clipdata = WindowsAPI.GetClipboardData(format, ref nullData);//Get pointer to clipboard data in the selected format.
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
			}

			return null;
		}

#endif

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

			foreach (Range r in inputStr.AsSpan().SplitAny(CrLf))
			{
				var split = inputStr.AsSpan(r).Trim();

				if (split.Contains("Buttons supported:", StringComparison.OrdinalIgnoreCase))
				{
					var splitct = 0;

					foreach (Range r2 in split.Split(':'))
					{
						var btnSplit = split[r2].Trim();

						if (btnSplit.Length > 0)
						{
							if (splitct > 0)
							{
								if (long.TryParse(btnSplit, out var btnCount))
								{
									count = Math.Min(count, btnCount);
									break;
								}
							}

							splitct++;
						}
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
			//  KeysharpEnhancements.OutputDebugLine($"{name}");
			var deviceIds = "xinput list --id-only".Bash().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			//foreach (var id in deviceIds)
			//  KeysharpEnhancements.OutputDebugLine($"{id}");

			if (deviceNames.Length == deviceIds.Length)
			{
				for (var i = 0; i < deviceNames.Length && !swapped; i++)
				{
					if (!deviceNames[i].Contains("xtest", StringComparison.OrdinalIgnoreCase))
					{
						var buttonStr = $"xinput get-button-map {deviceIds[i]}".Bash();
						var buttonStrSplits = buttonStr.Split(SpaceTab, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

						if (buttonStrSplits.All(sp => int.TryParse(sp, out var _)))
						{
							//KeysharpEnhancements.OutputDebugLine($"Device {deviceIds[i]}: {deviceNames[i]} with buttons {buttonStr} getting examined.");
							for (var j = 0; j < 3 && j < buttonStrSplits.Length; j++)
							{
								if (int.TryParse(buttonStrSplits[j], out var btn))
								{
									if (btn != j + 1)
									{
										swapped = true;
										//KeysharpEnhancements.OutputDebugLine($"\tWas swapped.");
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
		/// <summary>
		/// Internal helper to restore data contained in a <see cref="ClipboardAll"/> object to the clipboard.
		/// </summary>
		/// <param name="clip">The object whose data will be restored.</param>
		/// <param name="length">The length of the data to restore.</param>
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
					if (WindowsAPI.OpenClipboard(A_ClipboardTimeout.Al()))//Need to leave it open for it to work when using the Windows API.
					{
						wasOpened = true;
						var ptr = (nint)clip.Ptr;
						length = Math.Min(Math.Max(0U, length), (long)clip.Size);

						for (var index = 0; index < length;)
						{
							var cliptype = Unsafe.Read<uint>((void*)nint.Add(ptr, index));

							if (cliptype == 0)
								break;

							index += 4;
							var size = Unsafe.Read<int>((void*)nint.Add(ptr, index));
							index += 4;

							if (index + size < length)
							{
								var hglobal = Marshal.AllocHGlobal(size);
								System.Buffer.MemoryCopy((void*)nint.Add(ptr, index), hglobal.ToPointer(), size, size);
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
	/// Gotten from: https://stackoverflow.com/questions/6262454/c-sharp-backing-up-and-restoring-clipboard
	/// </summary>
	public class ClipboardAll : KeysharpObject
	{
		Dictionary<string, object> backup = new Dictionary<string, object>();
		IDataObject dataObject = Clipboard.GetDataObject();
		string[] formats;

		public new (Type, object) super => (typeof(KeysharpObject), this);

		public ClipboardAll(params object[] args) => _ = __New(args);

		public new object __New(params object[] args)
		{
			Save();
			return DefaultObject;
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

	/// <summary>
	/// A class that represents clipboard data.
	/// This is just a thin derivation of <see cref="Buffer"/>.
	/// </summary>
	public class ClipboardAll : Buffer
	{
		/// <summary>
		/// Constructor that just passes the data to the base.
		/// </summary>
		/// <param name="obj">The data to pass to the base.</param>
		//public ClipboardAll(byte[] obj)
		//  : base(obj)
		//{
		//}

		/// <summary>
		/// The implementation for <see cref="KeysharpObject.super"/> for this class to return this type.
		/// </summary>
		public new (Type, object) super => (typeof(KeysharpObject), this);

		public ClipboardAll(params object[] args) => _ = __New(args);
	}
#else
	public class ClipboardAll
	{
	}
#endif
}