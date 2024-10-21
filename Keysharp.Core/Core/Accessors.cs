namespace Keysharp.Core
{
	public static class Accessors
	{
		internal static long hotkeyModifierTimeout = 50L;
		internal static long hotkeyThrottleInterval = 2000L;
		internal static long maxHotkeysPerInterval = 2000L;
		private static readonly string initialWorkingDir = Environment.CurrentDirectory;
		private static bool allowMainWindow = true;
		private static long controlDelay = 20L;
		private static long defaultMouseSpeed = 2L;
		private static object detectHiddenText = true;
		private static object detectHiddenWindows = false;
		private static Encoding fileEncoding = Encoding.Default;
		private static bool? iconFrozen;
		private static bool iconHidden;
		private static uint inputLevel;
		private static long keyDelay = 10L;
		private static long keyDelayPlay = -1L;
		private static long keyDuration = -1L;
		private static long keyDurationPlay = -1L;
		private static string menuMaskKey = "";
		private static long mouseDelay = 10L;
		private static long mouseDelayPlay = -1L;
		private static long peekFrequency = 5L;
		private static Icon prevTrayIcon;
		private static object regView = 64L;
		private static object sendLevel = 0L;
		private static SendModes sendMode = SendModes.Input;
		private static object storeCapsLockMode = true;
		private static object titleMatchMode = 2L;
		private static object titleMatchModeSpeed = true;
		private static long winDelay = 100L;

		/// <summary>
		/// The version of the assembly that was used to compile the script that is currently running.
		/// This queries the AssemblyBuildVersionAttribute of the generated code, which is automatically added by the script compiler
		/// to match the version of the program the script compiler is being run from.
		/// This allows for a consistent value whether compiling and running a script, or running a compilex exe.
		/// </summary>
		public static string A_AhkBuildVersion
		{
			get
			{
				var ver = GetAssembly().GetCustomAttribute<AssemblyBuildVersionAttribute>();
				return ver != null ? ver.Version : "";
			}
		}

		/// <summary>
		/// The full path of the assembly that is currently executing.
		/// </summary>
		public static string A_AhkPath => Application.ExecutablePath;

		/// <summary>
		/// The version of Keysharp.Core.dll
		/// </summary>
		public static string A_AhkVersion
		{
			get
			{
				var ver = typeof(Accessors).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
				return ver != null ? ver.Version : "";
			}
		}

		public static object A_AllowMainWindow
		{
			get => allowMainWindow;

			set
			{
				var val = Options.OnOff(value);

				if (val.HasValue)
				{
					allowMainWindow = val.Value;
					Script.openMenuItem.Visible = val.Value;

					if (!A_AllowMainWindow.Ab())
						Script.openMenuItem.Visible = false;
					else
						Script.trayMenu.Default = "&Open";
				}
			}
		}

		public static object A_AllowTimers { get; set; } = true;

		/// <summary>
		/// The full path and name of the folder containing the current user's application-specific data. For example: <code>C:\Documents and Settings\Username\Application Data</code>
		/// </summary>
		public static string A_AppData =>
#if WINDOWS
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

#else//On linux, ApplicationData maps to home/.config which is a file, not a folder. So just map it to home instead.
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#endif

		/// <summary>
		/// The full path and name of the folder containing the all-users application-specific data.
		/// </summary>
		public static string A_AppDataCommon => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

		public static Array A_Args { get; internal set; } = [];

		/// <summary>
		/// The company of the assembly that is currently executing, specified with the #ASSEMBLYCOMPANY directive.
		/// </summary>
		public static string A_AsmCompany
		{
			get
			{
				var ver = GetAssembly().GetCustomAttribute<AssemblyCompanyAttribute>();
				return ver != null ? ver.Company : "";
			}
		}

		/// <summary>
		/// The configuration of the assembly that is currently executing, specified with the #ASSEMBLYCONFIGURATION directive.
		/// </summary>
		public static string A_AsmConfiguration
		{
			get
			{
				var ver = GetAssembly().GetCustomAttribute<AssemblyConfigurationAttribute>();
				return ver != null ? ver.Configuration : "";
			}
		}

		/// <summary>
		/// The copyright of the assembly that is currently executing, specified with the #ASSEMBLYCOPYRIGHT directive.
		/// </summary>
		public static string A_AsmCopyright
		{
			get
			{
				var ver = GetAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>();
				return ver != null ? ver.Copyright : "";
			}
		}

		/// <summary>
		/// The description of the assembly that is currently executing, specified with the #ASSEMBLYDESCRIPTION directive.
		/// </summary>
		public static string A_AsmDescription
		{
			get
			{
				var ver = GetAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>();
				return ver != null ? ver.Description : "";
			}
		}

		/// <summary>
		/// The product of the assembly that is currently executing, specified with the #ASSEMBLYPRODUCT directive.
		/// </summary>
		public static string A_AsmProduct
		{
			get
			{
				var ver = GetAssembly().GetCustomAttribute<AssemblyProductAttribute>();
				return ver != null ? ver.Product : "";
			}
		}

		/// <summary>
		/// The title of the assembly that is currently executing, specified with the #ASSEMBLYTITLE directive.
		/// </summary>
		public static string A_AsmTitle
		{
			get
			{
				var ver = GetAssembly().GetCustomAttribute<AssemblyTitleAttribute>();
				return ver != null ? ver.Title : "";
			}
		}

		/// <summary>
		/// The trademark of the assembly that is currently executing, specified with the #ASSEMBLYTRADEMARK directive.
		/// </summary>
		public static string A_AsmTrademark
		{
			get
			{
				var ver = GetAssembly().GetCustomAttribute<AssemblyTrademarkAttribute>();
				return ver != null ? ver.Trademark : "";
			}
		}

		/// <summary>
		/// The version of the assembly that is currently executing, specified with the #ASSEMBLVERSION directive.
		/// </summary>
		public static string A_AsmVersion
		{
			get
			{
				var ver = GetAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>();//For some reason it likes AssemblyFileVersionAttribute better than AssemblyVersionAttribute.
				return ver != null ? ver.Version : "";
			}
		}

		/// <summary>
		/// The current X coordinate of the caret (text insertion point). The coordinates are relative to the active window unless CoordMode is used to make them relative to the entire screen. If there is no active window or the caret position cannot be determined, these variables are blank.
		/// </summary>
		public static string A_CaretX => "";

		/// <summary>
		/// The current Y coordinate of the caret (text insertion point). The coordinates are relative to the active window unless CoordMode is used to make them relative to the entire screen. If there is no active window or the caret position cannot be determined, these variables are blank.
		/// </summary>
		public static string A_CaretY => "";

		public static object A_Clipboard
		{
			get
			{
				//Even if we're on an STA thread, this can sometimes fail. So it's best to always
				//make sure it's on the main thread.
				var act = () =>
				{
#if WINDOWS

					if (WindowsAPI.OpenClipboard((long)A_ClipboardTimeout))//Will need a cross platform version of this.//TODO
					{
						_ = WindowsAPI.CloseClipboard();//Need to close it for it to work
#endif

						if (Clipboard.GetData(DataFormats.Text) is string text)
							return text;

						if (Clipboard.GetData(DataFormats.Html) is string html)
							return html;

						if (Clipboard.GetData(DataFormats.Rtf) is string rtf)
							return rtf;

						if (Clipboard.GetData(DataFormats.SymbolicLink) is string sym)
							return sym;

						if (Clipboard.GetData(DataFormats.UnicodeText) is string uni)
							return uni;

						if (Clipboard.GetData(DataFormats.OemText) is string oem)
							return oem;

						if (Clipboard.GetData(DataFormats.CommaSeparatedValue) is string csv)
							return csv;

						if (Clipboard.GetData(DataFormats.FileDrop) is string[] files)
							return string.Join(Environment.NewLine, files);

#if WINDOWS
					}

#endif
					return "";
				};
				var ret = "";
				Script.mainWindow.CheckedInvoke(() => ret = act(), true);
				return ret;
			}
			set
			{
				Script.mainWindow.CheckedInvoke(() =>
				{
#if LINUX

					if (value == null || (value is string s && s?.Length == 0))
					{
						//Clipboard.Clear();//For some reason this doesn't work on linux. Bug reported here: https://github.com/DanielVanNoord/System.Windows.Forms/issues/17
						Clipboard.SetDataObject("", true);
					}
					else if (value is ClipboardAll arr)
						Env.RestoreClipboardAll(arr, 0L);
					else
						Clipboard.SetDataObject(value.ToString(), true);

#elif WINDOWS

					if (WindowsAPI.OpenClipboard((long)A_ClipboardTimeout))
					{
						_ = WindowsAPI.CloseClipboard();//Need to close it for it to work

						if (value == null || (value is string s && s?.Length == 0))
							Env.MyClearClip();
						else if (value is ClipboardAll arr)
							Env.RestoreClipboardAll(arr, (long)arr.Size);
						else
							Clipboard.SetDataObject(value.ToString(), true);
					}

#endif
				}, true);
			}
		}

		public static object A_ClipboardTimeout { get; set; } = 1000L;

		public static string A_CommandLine
		{
			get
			{
				var exe = Application.ExecutablePath;

				if (exe.Contains(' '))
				{
					if (!exe.StartsWith('"'))
						exe = '"' + exe;

					if (!exe.EndsWith('"'))
						exe += '"';
				}

				var args = new List<string>();

				foreach (var arg in Environment.GetCommandLineArgs().Skip(1))
				{
					var quotedArg = arg;

					if (quotedArg.Contains(' '))
					{
						if (!quotedArg.StartsWith('"'))
							quotedArg = '"' + quotedArg;

						if (!quotedArg.EndsWith('"'))
							quotedArg += '"';
					}

					args.Add(quotedArg);
				}

				return args.Count > 0 ? exe + " " + string.Join(' ', args) : exe;
			}
		}

		/// <summary>
		/// The name of the computer as seen on the network.
		/// </summary>
		public static string A_ComputerName => Environment.MachineName;

		public static string A_ComSpec => Environment.GetEnvironmentVariable("COMSPEC");

		/// <summary>
		/// The delay in milliseconds that will occur after each control-modifying command.
		/// </summary>
		public static object A_ControlDelay
		{
			get => controlDelay;
			set => controlDelay = ThreadAccessors.A_ControlDelay = value.Al();
		}

		public static string A_CoordModeCaret => Mouse.Coords.Caret.ToString();
		public static string A_CoordModeMenu => Mouse.Coords.Menu.ToString();
		public static string A_CoordModeMouse => Mouse.Coords.Mouse.ToString();
		public static string A_CoordModePixel => Mouse.Coords.Pixel.ToString();
		public static string A_CoordModeToolTip => Mouse.Coords.Tooltip.ToString();

		/// <summary>
		/// The type of mouse cursor currently being displayed. It will be one of the following words: AppStarting, Arrow, Cross, Help, IBeam, Icon, No, Size, SizeAll, SizeNESW, SizeNS, SizeNWSE, SizeWE, UpArrow, Wait, Unknown. The acronyms used with the size-type cursors are compass directions, e.g. NESW = NorthEast+SouthWest. The hand-shaped cursors (pointing and grabbing) are classfied as Unknown.
		/// </summary>
		public static string A_Cursor =>
		Cursor.Current is Cursor cur ?
#if LINUX
		cur.ToString().Trim(Keywords.BothBrackets).Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[1]
#elif WINDOWS
		cur.ToString().Trim(Keywords.BothBrackets).Split(' ', StringSplitOptions.RemoveEmptyEntries)[1].Replace("Cursor", "")
#endif
		: "Default---";

		/// <summary>
		/// See <see cref="A_MDay"/>.
		/// </summary>
		public static string A_DD => A_MDay;

		/// <summary>
		/// Current day of the week's 3-letter abbreviation in the current user's language, e.g. <code>Sun</code>.
		/// </summary>
		public static string A_DDD => DateTime.Now.ToString("ddd");

		/// <summary>
		/// Current day of the week's full name in the current user's language, e.g. <code>Sunday</code>.
		/// </summary>
		public static string A_DDDD => DateTime.Now.ToString("dddd");

		public static bool A_DefaultHotstringCaseSensitive => HotstringManager.hsCaseSensitive;
		public static bool A_DefaultHotstringConformToCase => HotstringManager.hsConformToCase;
		public static bool A_DefaultHotstringDetectWhenInsideWord => HotstringManager.hsDetectWhenInsideWord;
		public static bool A_DefaultHotstringDoBackspace => HotstringManager.hsDoBackspace;
		public static bool A_DefaultHotstringDoReset => HotstringManager.hsDoReset;
		public static bool A_DefaultHotstringEndCharRequired => HotstringManager.hsEndCharRequired;
		public static string A_DefaultHotstringEndChars => HotstringManager.defEndChars;
		public static long A_DefaultHotstringKeyDelay => HotstringManager.hsKeyDelay;
		public static bool A_DefaultHotstringOmitEndChar => HotstringManager.hsOmitEndChar;
		public static long A_DefaultHotstringPriority => HotstringManager.hsPriority;
		public static string A_DefaultHotstringSendMode => HotstringManager.hsSendMode.ToString();
		public static string A_DefaultHotstringSendRaw => HotstringManager.hsSendRaw.ToString();

		/// <summary>
		/// Sets the mouse speed that will be used if unspecified in <see cref="Click"/>.
		/// </summary>
		public static object A_DefaultMouseSpeed
		{
			get => defaultMouseSpeed;
			set => defaultMouseSpeed = ThreadAccessors.A_DefaultMouseSpeed = value.Al();
		}

		/// <summary>
		/// The full path and name of the folder containing the current user's desktop files.
		/// </summary>
		public static string A_Desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

		/// <summary>
		/// The full path and name of the folder containing the all-users desktop files.
		/// </summary>
		public static string A_DesktopCommon => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

		/// <summary>
		/// Determines whether invisible text in a window is "seen" for the purpose of finding the window. This affects commands such as <see cref="WinExist"/> and <see cref="WinActivate"/>.
		/// </summary>
		public static object A_DetectHiddenText
		{
			get => detectHiddenText;

			set
			{
				var val = Options.OnOff(value);

				if (val != null)
					detectHiddenText = ThreadAccessors.A_DetectHiddenText = val.Value.Ab();
			}
		}

		/// <summary>
		/// Determines whether invisible windows are "seen".
		/// </summary>
		public static object A_DetectHiddenWindows
		{
			get => detectHiddenWindows;

			set
			{
				var val = Options.OnOff(value);

				if (val != null)
					detectHiddenWindows = ThreadAccessors.A_DetectHiddenWindows = val.Value.Ab();
			}
		}

		/// <summary>
		/// The native directory separator string, i.e. "/" on linux, "\" on Windows.
		/// </summary>
		public static string A_DirSeparator => Path.DirectorySeparatorChar.ToString();

		/// <summary>
		/// Represents the natural logarithmic base, specified by the constant, e.
		/// </summary>
		public static double A_E => Math.E;

		/// <summary>
		/// The ending character that was pressed by the user to trigger the most recent non-auto-replace hotstring. If no ending character was required (due to the * option), this variable will be blank.
		/// </summary>
		public static object A_EndChar
		{
			get;
			internal set;
		}

		/// <summary>
		/// Contains event information from various commands.
		/// </summary>
		public static object A_EventInfo
		{
			get => Threads.GetThreadVariables().eventInfo;
			set => Threads.GetThreadVariables().eventInfo = value;
		}

		public static object A_FileEncoding
		{
			get
			{
				var val = fileEncoding.BodyName;

				if (fileEncoding is UnicodeEncoding ue)
				{
					if (ue.GetPreamble().Length == 0)
						val += "-raw";
				}
				else if (fileEncoding is UTF8Encoding u8)
				{
					if (u8.GetPreamble().Length == 0)
						val += "-raw";
				}

				return val;
			}
			set
			{
				fileEncoding = Files.GetEncoding(value.ToString());
				ThreadAccessors.A_FileEncoding = value;
			}
		}

		/// <summary>
		/// The current numeric format.
		/// </summary>
		public static object A_FormatNumeric
		{
			get
			{
				var tv = Threads.GetThreadVariables();

				if (tv.formatNumeric != null)
					return tv.formatNumeric;

				Script.SetInitialFloatFormat();
				return tv.formatNumeric = "f";
			}

			set => Threads.GetThreadVariables().formatNumeric = value.ToString();
		}

		public static bool A_HasExited => Flow.hasExited;

		public static object A_HotIfTimeout { get; set; } = 1000L;

		public static object A_HotkeyInterval
		{
			get => hotkeyThrottleInterval;
			set => hotkeyThrottleInterval = value.Al();
		}

		public static object A_HotkeyModifierTimeout
		{
			get => hotkeyModifierTimeout;
			set => hotkeyModifierTimeout = value.Al();
		}

		public static object A_HotstringNoMouse => !Script.hsResetUponMouseClick;

		/// <summary>
		/// Current 2-digit hour (00-23) in 24-hour time (for example, 17 is 5pm).
		/// </summary>
		public static string A_Hour => DateTime.Now.ToString("HH");

		/// <summary>
		/// Blank unless a custom tray icon has been specified via Menu, tray, icon -- in which case it's the full path and name of the icon's file.
		/// </summary>
		public static string A_IconFile { get; internal set; } = "";

		/// <summary>
		/// Contains true if the tray icon is currently hidden or false otherwise. The icon can be hidden via #NoTrayIcon or the Menu command.
		/// </summary>
		public static object A_IconHidden
		{
			get => iconHidden;

			set
			{
				if (Script.NoTrayIcon)
					return;

				var val = Options.OnOff(value);

				if (val.HasValue)
				{
					if (!val.Value && iconHidden)//Was true, switching to false, so show.
					{
						if (Script.Tray != null)
						{
							Script.Tray.Icon = prevTrayIcon;
							prevTrayIcon = null;
							iconHidden = val.Value;
						}
					}
					else if (val.Value && !iconHidden)//Was false, switching to true, so hide.
					{
						if (Script.Tray != null)
						{
							prevTrayIcon = Script.Tray.Icon;
							Script.Tray.Icon = null;
							iconHidden = val.Value;
						}
					}
				}
			}
		}

		/// <summary>
		/// Blank if A_IconFile is blank. Otherwise, it's the number of the icon in A_IconFile (typically 1) or a string name of an icon in a resource in a .NET DLL.
		/// </summary>
		public static object A_IconNumber { get; internal set; } = 1L;

		/// <summary>
		/// Blank unless a custom tooltip for the tray icon has been specified via Menu, Tray, Tip -- in which case it's the text of the tip.
		/// </summary>
		public static object A_IconTip
		{
			get => Script.Tray != null ? Script.Tray.Text : "";

			set
			{
				if (Script.Tray != null)
					Script.Tray.Text = value.ToString();
			}
		}

		/// <summary>
		/// The number of the current loop iteration.
		/// </summary>
		public static object A_Index
		{
			get
			{
				var s = Loops.LoopStack;
				return s.Count > 0 ? s.Peek().index : default;
			}
			set
			{
				var s = Loops.LoopStack;

				if (s.Count > 0)
					s.Peek().index = value.Al();
			}
		}

		public static string A_InitialWorkingDir => initialWorkingDir;

		public static object A_InputLevel
		{
			get => inputLevel;
			set => inputLevel = (uint)Math.Clamp(value.Al(), 0L, 100L);
		}

		/// <summary>
		/// The IP addresses of the network adapters in the computer.
		/// </summary>
		public static Array A_IPAddress
		{
			get
			{
				var addr = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
				var ips = new Array(addr.Length);

				for (var i = 0; i < addr.Length; i++)
					if (addr[i].AddressFamily == AddressFamily.InterNetwork)
						_ = ips.Add(addr[i].ToString());

				return ips;
			}
		}

		public static bool A_Is64bitOS => true;

		/// <summary>
		/// <code>true</code> if the current user has administrator rights, <code>false</code> otherwise.
		/// </summary>
		public static bool A_IsAdmin
		{
			get
			{
#if LINUX
				return Xlib.geteuid() == 0;
#elif WINDOWS
				using var id = WindowsIdentity.GetCurrent();
				return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
#endif
			}
		}

		/// <summary>
		/// <code>true</code> if the current executing assembly is a compiled script, <code>false</code> otherwise;
		/// </summary>
		public static bool A_IsCompiled
		{
			get
			{
				var path = Path.GetFileName(Application.ExecutablePath).ToLowerInvariant();
				return path != "keysharp.dll" && path != "keysharp.exe" && path != "testhost.exe"
					   && path != "keysharp" && path != "testhost" && path != "testhost.dll";
			}
		}

		/// <summary>
		/// <c>1</c> if the current thread is marked as critical, <c>0</c> otherwise.
		/// </summary>
		public static long A_IsCritical
		{
			get
			{
				var tv = Threads.GetThreadVariables();
				return tv.isCritical ? tv.peekFrequency : 0L;
			}
		}

		/// <summary>
		/// <code>true</code> if the script is suspended, <code>false</code> otherwise;
		/// </summary>
		public static bool A_IsSuspended => Flow.Suspended;

		/// <summary>
		/// Only for compatibility with AHK, C# programs are always unicode.
		/// </summary>
		public static bool A_IsUnicode => true;

		/// <summary>
		/// Returns either 0, 1, 2 or 3:
		///     0: No keyboard hook is installed.
		///     1: Only our keyboard hook is installed.
		///     2: Only another keyboard hook is installed.
		///     3: Ours and another keyboard hook are installed.
		/// </summary>
		public static long A_KeybdHookInstalled => Script.HookThread is HookThread ht
		? (ht.HasKbdHook() ? 1L : 0L) | (ht.SystemHasAnotherKeybdHook() ? 2L : 0L)
		: 0L;

		/// <summary>
		/// The delay that will occur after each keystroke sent by <see cref="Send"/> and <see cref="ControlSend"/>.
		/// </summary>
		public static object A_KeyDelay
		{
			get => keyDelay;
			set => keyDelay = ThreadAccessors.A_KeyDelay = value.Al();
		}

		/// <summary>
		/// The delay that will occur in SendPlay mode after each keystroke sent by <see cref="Send"/> and <see cref="ControlSend"/>.
		/// </summary>
		public static object A_KeyDelayPlay
		{
			get => keyDelayPlay;
			set => keyDelayPlay = ThreadAccessors.A_KeyDelayPlay = value.Al();
		}

		/// <summary>
		/// The delay between the press of a key and before its release, used with <see cref="A_KeyDelay"/>.
		/// </summary>
		public static object A_KeyDuration
		{
			get => keyDuration;
			set => keyDuration = ThreadAccessors.A_KeyDuration = value.Al();
		}

		/// <summary>
		/// The delay in SendPlay mode between the press of a key and before its release, used with <see cref="A_KeyDelayPlay"/>.
		/// </summary>
		public static object A_KeyDurationPlay
		{
			get => keyDurationPlay;
			set => keyDurationPlay = ThreadAccessors.A_KeyDurationPlay = value.Al();
		}

		public static string A_KeysharpCorePath => Assembly.GetAssembly(typeof(Accessors)).Location;
		public static string A_KeysharpPath => A_AhkPath;

		/// <summary>
		/// The system's default language code.
		/// </summary>
		public static long A_Language => Thread.CurrentThread.CurrentCulture.LCID;

		/// <summary>
		/// The result from Windows <code>GetLastError()</code> function.
		/// </summary>
		public static long A_LastError => Marshal.GetLastWin32Error();

		/// <summary>
		/// The current element of a loop.
		/// </summary>
		public static object A_LoopField
		{
			get
			{
				var s = Loops.LoopStack;

				if (s.Count == 0)
					return null;

				foreach (var l in s)//Since loop is a stack, this goes in reverse order, which is what we want.
				{
					switch (l.type)
					{
						case LoopType.Parse:
							return l.result;

						case LoopType.Each:
						{
							if (!(l.result is object[] so))
								return null;

							return so.Length > 0 ? so[1] : null;
						}
					}
				}

				return null;
			}
		}

		/// <summary>
		/// The attributes of the file currently retrieved.
		/// </summary>
		public static string A_LoopFileAttrib
		{
			get
			{
				if (A_LoopFileFullPath is string s && (File.Exists(s) || Directory.Exists(s)))
				{
					var val = "";
					var attr = File.GetAttributes(s);

					if (attr.HasFlag(FileAttributes.ReadOnly))
						val += "R";

					if (attr.HasFlag(FileAttributes.Archive))
						val += "A";

					if (attr.HasFlag(FileAttributes.System))
						val += "S";

					if (attr.HasFlag(FileAttributes.Hidden))
						val += "H";

					if (attr.HasFlag(FileAttributes.Normal))
						val += "N";

					if (attr.HasFlag(FileAttributes.Directory))
						val += "D";

					if (attr.HasFlag(FileAttributes.Offline))
						val += "O";

					if (attr.HasFlag(FileAttributes.Compressed))
						val += "C";

					if (attr.HasFlag(FileAttributes.Temporary))
						val += "T";

					return val;
				}

				return "";
			}
		}

		/// <summary>
		/// The full path of the directory in which A_LoopFileName resides. However, if FilePattern contains a relative path rather than an absolute path, the path here will also be relative. A root directory will not contain a trailing backslash. For example: C:
		/// </summary>
		public static string A_LoopFileDir
		{
			get
			{
				var loop = Loops.GetDirLoop();

				if (loop != null && loop.file is string s)
					return Path.GetDirectoryName(s);

				return null;
			}
		}

		/// <summary>
		/// The file's extension (e.g. TXT, DOC, or EXE). The period (.) is not included.
		/// </summary>
		public static string A_LoopFileExt
		{
			get
			{
				var file = Loops.GetDirLoopFilename();
				return file != null ? Path.GetExtension(file).TrimStart('.') : "";
			}
		}

		/// <summary>
		/// This is different than A_LoopFilePath in the following ways:
		/// 1) It always contains the absolute/complete path of the file even if FilePattern contains a relative path
		/// 2) Any short (8.3) folder names in FilePattern itself are converted to their long names
		/// 3) Characters in FilePattern are converted to uppercase or lowercase to match the case stored in the file system.
		/// This is useful for converting file names -- such as those passed into a script as command line parameters -- to their exact path names as shown by Explorer.
		/// </summary>
		public static string A_LoopFileFullPath
		{
			get
			{
				var loop = Loops.GetDirLoop();
				return loop != null && loop.file is string s ? Loops.GetExactPath(s) : "";//This gives exact case.
			}
		}

		/// <summary>
		/// This is just a synonym for A_LoopFileFullPath.
		/// </summary>
		public static string A_LoopFileLongPath => A_LoopFileFullPath;

		/// <summary>
		/// The name of the file or folder currently retrieved (without the path).
		/// </summary>
		public static string A_LoopFileName
		{
			get
			{
				var file = Loops.GetDirLoopFilename();
				return file != null ? Path.GetFileName(file) : "";
			}
		}

		/// <summary>
		/// The path and name of the file/folder currently retrieved. If FilePattern contains a relative path rather than an absolute path, the path here will also be relative.
		/// </summary>
		public static string A_LoopFilePath
		{
			get
			{
				var loop = Loops.GetDirLoop();

				if (loop != null && loop.file is string s)
				{
					var fullpath = Path.GetFullPath(s);
					var isrel = !Path.IsPathFullyQualified(loop.path);
					return isrel ? Path.GetRelativePath(A_WorkingDir as string, fullpath) : fullpath;
				}

				return "";
			}
		}

		/// <summary>
		/// The 8.3 short name, or alternate name of the file. If the file doesn't have one (due to the long name being shorter than 8.3 or perhaps because short-name generation is disabled on an NTFS file system), A_LoopFileName will be retrieved instead.
		/// </summary>
		public static string A_LoopFileShortName => A_LoopFileShortPath is string s ? Path.GetFileName(s) : null;

		/// <summary>
		/// The 8.3 short path and name of the file/folder currently retrieved. For example: C:\MYDOCU~1\ADDRES~1.txt. However, if FilePattern contains a relative path rather than an absolute path, the path here will also be relative.
		/// </summary>
		public static string A_LoopFileShortPath
		{
			get
			{
				var loop = Loops.GetDirLoop();
				return loop != null && loop.file is string s ? Loops.GetShortPath(s) : "";
			}
		}

		/// <summary>
		/// The size in bytes of the file currently retrieved. Files larger than 4 gigabytes are also supported.
		/// </summary>
		public static long A_LoopFileSize
		{
			get
			{
				var file = Loops.GetDirLoopFilename();
				return file != null && File.Exists(file) ? new FileInfo(file).Length : 0L;
			}
		}

		/// <summary>
		/// The size in Kbytes of the file currently retrieved, rounded down to the nearest integer.
		/// </summary>
		public static long A_LoopFileSizeKB => A_LoopFileSize / 1024L;

		/// <summary>
		/// The size in Mbytes of the file currently retrieved, rounded down to the nearest integer.
		/// </summary>
		public static long A_LoopFileSizeMB => A_LoopFileSize / (1024L * 1024L);

		/// <summary>
		/// The time the file was last accessed. Format YYYYMMDDHH24MISS.
		/// </summary>
		public static string A_LoopFileTimeAccessed
		{
			get
			{
				var file = Loops.GetDirLoopFilename();
				return !string.IsNullOrEmpty(file) ? Conversions.ToYYYYMMDDHH24MISS(File.GetLastAccessTime(file)) : "";
			}
		}

		/// <summary>
		/// The time the file was created. Format YYYYMMDDHH24MISS.
		/// </summary>
		public static string A_LoopFileTimeCreated
		{
			get
			{
				var file = Loops.GetDirLoopFilename();
				return !string.IsNullOrEmpty(file) ? Conversions.ToYYYYMMDDHH24MISS(File.GetCreationTime(file)) : "";
			}
		}

		/// <summary>
		/// The time the file was last modified. Format YYYYMMDDHH24MISS.
		/// </summary>
		public static string A_LoopFileTimeModified
		{
			get
			{
				var file = Loops.GetDirLoopFilename();
				return !string.IsNullOrEmpty(file) ? Conversions.ToYYYYMMDDHH24MISS(File.GetLastWriteTime(file)) : "";
			}
		}

		/// <summary>
		/// The current object key in an each-loop.
		/// </summary>
		public static object A_LoopKey
		{
			get
			{
				var s = Loops.LoopStack;

				if (s.Count == 0)
					return null;

				foreach (var l in s)
				{
					switch (l.type)
					{
						case LoopType.Each:
						{
							if (!(l.result is object[]))
								return null;

							var pair = (object[])l.result;
							return pair[0];
						}
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Contains the contents of the current line excluding the carriage return and linefeed (`r`n) that marks the end of the line.
		/// </summary>
		public static string A_LoopReadLine
		{
			get
			{
				var s = Loops.LoopStack;

				if (s.Count == 0)
					return "";

				foreach (var l in s)
				{
					switch (l.type)
					{
						case LoopType.File:
							return l.line;
					}
				}

				return "";
			}
		}

#if WINDOWS

		/// <summary>
		/// The name of the root key being accessed (HKEY_LOCAL_MACHINE, HKEY_USERS, HKEY_CURRENT_USER, HKEY_CLASSES_ROOT, or HKEY_CURRENT_CONFIG). For remote registry access, this value will not include the computer name.
		/// </summary>
		public static string A_LoopRegKey
		{
			get
			{
				var s = Loops.LoopStack;

				if (s.Count == 0)
					return "";

				foreach (var l in s)//Since loop is a stack, this goes in reverse order, which is what we want.
				{
					switch (l.type)
					{
						case LoopType.Registry:
							return l.regKeyName;
					}
				}

				return "";
			}
		}

		/// <summary>
		/// Name of the currently retrieved item, which can be either a value name or the name of a subkey. Value names displayed by Windows RegEdit as "(Default)" will be retrieved if a value has been assigned to them, but A_LoopRegName will be blank for them.
		/// </summary>
		public static string A_LoopRegName
		{
			get
			{
				var s = Loops.LoopStack;

				if (s.Count == 0)
					return "";

				foreach (var l in s)
				{
					switch (l.type)
					{
						case LoopType.Registry:
							return l.regName != "(Default)" ? l.regName : "";
					}
				}

				return "";
			}
		}

		/// <summary>
		/// The time the current subkey or any of its values was last modified. Format YYYYMMDDHH24MISS. This variable will be empty if the currently retrieved item is not a subkey (i.e. A_LoopRegType is not the word KEY) or if the operating system is Win9x (since Win9x does not track this info).
		/// </summary>
		public static object A_LoopRegTimeModified
		{
			get
			{
				var s = Loops.LoopStack;

				if (s.Count == 0)
					return "";

				foreach (var l in s)
				{
					switch (l.type)
					{
						case LoopType.Registry:
							return l.regDate;
					}
				}

				return "";
			}
		}

		/// <summary>
		/// The type of the currently retrieved item, which is one of the following words: KEY (i.e. the currently retrieved item is a subkey not a value), REG_SZ, REG_EXPAND_SZ, REG_MULTI_SZ, REG_DWORD, REG_QWORD, REG_BINARY, REG_LINK, REG_RESOURCE_LIST, REG_FULL_RESOURCE_DESCRIPTOR, REG_RESOURCE_REQUIREMENTS_LIST, REG_DWORD_BIG_ENDIAN (probably rare on most Windows hardware). It will be empty if the currently retrieved item is of an unknown type.
		/// </summary>
		public static string A_LoopRegType
		{
			get
			{
				var s = Loops.LoopStack;

				if (s.Count == 0)
					return "";

				foreach (var l in s)
				{
					switch (l.type)
					{
						case LoopType.Registry:
							return l.regType;
					}
				}

				return "";
			}
		}

		public static object A_LoopRegValue
		{
			get
			{
				var s = Loops.LoopStack;

				if (s.Count == 0)
					return "";

				foreach (var l in s)
				{
					switch (l.type)
					{
						case LoopType.Registry:
							return l.regVal;
					}
				}

				return "";
			}
		}

#endif

		public static object A_MaxHotkeysPerInterval
		{
			get => maxHotkeysPerInterval;
			set => maxHotkeysPerInterval = value.Al();
		}

		public static object A_MaxThreads => Script.MaxThreadsTotal;
		public static object A_MaxThreadsBuffer { get; set; }
		public static object A_MaxThreadsPerHotkey { get; set; } = 1L;

		/// <summary>
		/// Current 2-digit day of the month (01-31).
		/// </summary>
		public static string A_MDay => DateTime.Now.ToString("dd");

		public static object A_MenuMaskKey
		{
			get => menuMaskKey;
			set => menuMaskKey = value.ToString();
		}

		/// <summary>
		/// Current 2-digit minute (00-59).
		/// </summary>
		public static string A_Min => DateTime.Now.ToString("mm");

		/// <summary>
		/// Current 2-digit month (01-12). Synonymous with <see cref="A_Mon"/>.
		/// </summary>
		public static string A_MM => A_Mon;

		/// <summary>
		/// Current month's abbreviation in the current user's language, e.g. <code>Jul</code>.
		/// </summary>
		public static string A_MMM => DateTime.Now.ToString("MMM");

		/// <summary>
		/// Current month's full name in the current user's language, e.g. <code>July</code>.
		/// </summary>
		public static string A_MMMM => DateTime.Now.ToString("MMMM");

		/// <summary>
		/// Current 2-digit month (01-12).
		/// </summary>
		public static string A_Mon => DateTime.Now.ToString("MM");

		/// <summary>
		/// Sets the delay that will occur after each mouse movement or click.
		/// </summary>
		public static object A_MouseDelay
		{
			get => mouseDelay;
			set => mouseDelay = ThreadAccessors.A_MouseDelay = value.Al();
		}

		/// <summary>
		/// Sets the delay that will occur in SendPlay mode after each mouse movement or click.
		/// </summary>
		public static object A_MouseDelayPlay
		{
			get => mouseDelayPlay;
			set => mouseDelayPlay = ThreadAccessors.A_MouseDelayPlay = value.Al();
		}

		/// <summary>
		/// Returns either 0, 1, 2 or 3:
		///     0: No mouse hook is installed.
		///     1: Only our mouse hook is installed.
		///     2: Only another mouse hook is installed.
		///     3: Ours and another mouse hook are installed.
		/// </summary>
		public static long A_MouseHookInstalled => Script.HookThread is HookThread ht
		? (ht.HasMouseHook() ? 1L : 0L) | (ht.SystemHasAnotherMouseHook() ? 2L : 0L)
		: 0L;

		/// <summary>
		/// Current 3-digit millisecond (000-999).
		/// </summary>
		public static string A_MSec => DateTime.Now.ToString("fff");

		/// <summary>
		/// The full path and name of the current user's "My Documents" folder.
		/// Need a cross platform way to do this.//TODO
		/// </summary>
		public static string A_MyDocuments => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

		/// <summary>
		/// The native newline string string, i.e. "\n" on linux, "\r\n" on Windows.
		/// </summary>
		public static string A_NewLine => Environment.NewLine;

		public static bool A_NoTrayIcon => Script.NoTrayIcon;

		/// <summary>
		/// The current local time in YYYYMMDDHH24MISS format.
		/// </summary>
		public static string A_Now => Conversions.ToYYYYMMDDHH24MISS(DateTime.Now);

		/// <summary>
		/// The current Coordinated Universal Time (UTC) in YYYYMMDDHH24MISS format.
		/// </summary>
		public static string A_NowUTC => Conversions.ToYYYYMMDDHH24MISS(DateTime.Now.ToUniversalTime());

		/// <summary>
		/// The type of Operating System being run, e.g. <code>WIN32_WINDOWS</code> for Windows 95/98/ME or <code>WIN32_NT</code> for Windows NT4/2000/XP/2003/Vista.
		/// </summary>
		public static string A_OSType => Conversions.ToOSType(Environment.OSVersion.Platform);

		/// <summary>
		/// The Operating System version, e.g. <code>WIN_VISTA</code>, <code>WIN_2003</code>, <code>WIN_XP</code>, <code>WIN_2000</code>, <code>WIN_NT4</code>, <code>WIN_95</code>, <code>WIN_98</code>, <code>WIN_ME</code>.
		/// </summary>
		public static string A_OSVersion => Environment.OSVersion.VersionString;

		public static object A_PeekFrequency
		{
			get => peekFrequency;
			set => peekFrequency = ThreadAccessors.A_PeekFrequency = value.Al();
		}

		/// <summary>
		/// Represents the ratio of the circumference of a circle to its diameter, specified by the constant, π.
		/// </summary>
		public static double A_PI => Math.PI;

		/// <summary>
		/// The key name of the previously executed hotkey or hotstring.
		/// </summary>
		public static string A_PriorHotkey => Script.priorHotkeyName;

		public static object A_Priority { get; set; } = 0L;

		public static string A_PriorKey => Script.HookThread is HookThread ht ? ht.keyHistory.PriorKey() : "";

		/// <summary>
		/// The Program Files directory (e.g. <code>C:\Program Files</code>).
		/// </summary>
		public static string A_ProgramFiles => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

		/// <summary>
		/// The full path and name of the Programs folder in the current user's Start Menu.
		/// </summary>
		public static string A_Programs => Environment.GetFolderPath(Environment.SpecialFolder.Programs);

		/// <summary>
		/// The full path and name of the Programs folder in the all-users Start Menu.
		/// </summary>
		public static string A_ProgramsCommon => Environment.GetFolderPath(Environment.SpecialFolder.Startup);

		public static long A_PtrSize => 8L;

#if WINDOWS

		public static object A_RegView
		{
			get => regView;

			set
			{
				regView = value is string s && s.ToLower() == "default" ? 64L : value.Al() == 32L ? 32L : 64L;
				ThreadAccessors.A_RegView = regView.Al();
			}
		}

#endif

		/// <summary>
		/// Number of pixels per logical inch along the screen width. In a system with multiple display monitors,
		/// this value is the same for all monitors. On most systems this is 96; it depends on the system's text size (DPI) setting.
		/// See also the GUI's -DPIScale option.
		/// </summary>
		public static double A_ScreenDPI
		{
			get
			{
				using (var graphics = Graphics.FromHwnd(IntPtr.Zero))//This will only get the DPI for the first screen.
				{
					var x = graphics.DpiX;
					//var y = graphics.DpiY;
					return x;
				}
			}
		}

		public static long A_ScreenHeight => System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
		public static long A_ScreenWidth => System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;

		public static string A_ScriptDir
		{
			get
			{
				if (A_IsCompiled)
					return Path.GetDirectoryName(GetAssembly().Location);
				else if (Script.scriptName == "*")
					return A_WorkingDir as string;
				else
					return Path.GetDirectoryName(Script.scriptName);
			}
		}

		public static string A_ScriptFullPath => A_IsCompiled ? A_AhkPath : Script.scriptName;

		/// <summary>
		/// The unique ID (HWND/handle) of the script's hidden main window.
		/// </summary>
		public static long A_ScriptHwnd => Script.MainWindowHandle.ToInt64();

		public static string A_ScriptName => Path.GetFileName(Script.scriptName);

		/// <summary>
		/// Current 2-digit second (00-59).
		/// </summary>
		public static string A_Sec => DateTime.Now.ToString("ss");

		public static object A_SendLevel
		{
			get => sendLevel;

			set
			{
				sendLevel = (uint)Math.Clamp(value.Al(), 0L, 100L);
				ThreadAccessors.A_SendLevel = sendLevel.Aui();
			}
		}

		public static object A_SendMode
		{
			get => sendMode.ToString();

			set
			{
				if (Enum.TryParse<SendModes>(value.As(), out var temp))
					sendMode = ThreadAccessors.A_SendMode = temp;
			}
		}

		/// <summary>
		/// This variable contains a single space character.
		/// </summary>
		public static string A_Space => " ";

		/// <summary>
		/// The full path and name of the current user's Start Menu folder.
		/// </summary>
		public static string A_StartMenu => Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);

		/// <summary>
		/// The full path and name of the all-users Start Menu folder.
		/// </summary>
		public static string A_StartMenuCommon => Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);

		/// <summary>
		/// The full path and name of the Startup folder in the current user's Start Menu.
		/// </summary>
		public static string A_Startup => Environment.GetFolderPath(Environment.SpecialFolder.Startup);

		/// <summary>
		/// The full path and name of the Startup folder in the all-users Start Menu.
		/// </summary>
		public static string A_StartupCommon => Environment.GetFolderPath(Environment.SpecialFolder.Startup);

		public static object A_StoreCapsLockMode
		{
			get => storeCapsLockMode;

			set
			{
				var val = Options.OnOff(value);

				if (val != null)
					storeCapsLockMode = ThreadAccessors.A_StoreCapsLockMode = val.Value;
			}
		}

		public static object A_SuspendExempt
		{
			get => HotstringManager.hsSuspendExempt;
			set => HotstringManager.hsSuspendExempt = value.Ab();
		}

		/// <summary>
		/// This variable contains a single tab character.
		/// </summary>
		public static string A_Tab => "\t";

		/// <summary>
		/// The full path and name of the folder designated to hold temporary files.
		/// </summary>
		public static string A_Temp => Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);

		/// <summary>
		/// Temporary file name.
		/// </summary>
		public static string A_TempFile => Path.GetTempFileName();

		public static string A_ThisFunc => new StackFrame(1).GetMethod().Name;

		/// <summary>
		/// The key name of the most recently executed hotkey or hotstring.
		/// </summary>
		public static string A_ThisHotkey => Script.thisHotkeyName;

		/// <summary>
		/// The name of the menu from which A_ThisMenuItem was selected.
		/// </summary>
		public static string A_ThisMenu => null;

		/// <summary>
		/// The name of the most recently selected custom menu item (blank if none).
		/// </summary>
		public static string A_ThisMenuItem => null;

		/// <summary>
		/// A number indicating the current position of A_ThisMenuItem within A_ThisMenu. The first item in the menu is 1, the second is 2, and so on. Menu separator lines are counted. This variable is blank if A_ThisMenuItem is blank or no longer exists within A_ThisMenu. It is also blank if A_ThisMenu itself no longer exists.
		/// </summary>
		public static string A_ThisMenuItemPos => null;

		/// <summary>
		/// The number of milliseconds since the computer was rebooted.
		/// </summary>
		public static long A_TickCount => Environment.TickCount64;

		/// <summary>
		/// The number of milliseconds that have elapsed since the system last received keyboard, mouse, or other input. This is useful for determining whether the user is away. This variable will be blank unless the operating system is Windows 2000, XP, or beyond. Physical input from the user as well as artificial input generated by any program or script (such as the Send or MouseMove commands) will reset this value back to zero. Since this value tends to increase by increments of 10, do not check whether it is equal to another value. Instead, check whether it is greater or less than another value. For example: IfGreater, A_TimeIdle, 600000, MsgBox, The last keyboard or mouse activity was at least 10 minutes ago.
		/// </summary>
		public static long A_TimeIdle
		{
			get
			{
#if WINDOWS
				var lii = LASTINPUTINFO.Default;
				return WindowsAPI.GetLastInputInfo(ref lii) ? Environment.TickCount - lii.dwTime : 0L;
#else
				return "xprintidle".Bash().Al();
#endif
			}
		}

		public static long A_TimeIdleKeyboard
		{
			get
			{
				return Script.HookThread is HookThread ht && ht.HasKbdHook()
					   ? (long)(DateTime.Now - Script.timeLastInputKeyboard).TotalMilliseconds
					   : A_TimeIdle;
			}
		}

		public static long A_TimeIdleMouse
		{
			get
			{
				return Script.HookThread is HookThread ht && ht.HasMouseHook()
					   ? (long)(DateTime.Now - Script.timeLastInputMouse).TotalMilliseconds
					   : A_TimeIdle;
			}
		}

		/// <summary>
		/// Same as above but ignores artificial keystrokes and/or mouse clicks whenever the corresponding hook (keyboard or mouse) is installed. If neither hook is installed, this variable is equivalent to A_TimeIdle. If only one hook is present, only that one type of artificial input will be ignored. A_TimeIdlePhysical may be more useful than A_TimeIdle for determining whether the user is truly present.
		/// </summary>
		public static long A_TimeIdlePhysical
		{
			get
			{
				return Script.HookThread is HookThread ht && ht.HasEitherHook()
					   ? (long)(DateTime.Now - Script.timeLastInputPhysical).TotalMilliseconds
					   : A_TimeIdle;
			}
		}

		/// <summary>
		/// Time in ms that have elapsed since <see cref="A_PriorHotkey"/> was pressed. It will be -1 whenever <see cref="A_PriorHotkey"/> is blank.
		/// </summary>
		public static long A_TimeSincePriorHotkey => string.IsNullOrEmpty(Script.priorHotkeyName) ? -1L : (long)(DateTime.Now - Script.priorHotkeyStartTime).TotalMilliseconds;

		/// <summary>
		/// Time in ms that have elapsed since <see cref="A_ThisHotkey"/> was pressed. It will be -1 whenever <see cref="A_ThisHotkey"/> is blank.
		/// </summary>
		public static long A_TimeSinceThisHotkey => string.IsNullOrEmpty(Script.thisHotkeyName) ? -1L : (long)(DateTime.Now - Script.thisHotkeyStartTime).TotalMilliseconds;

		/// <summary>
		/// The current mode set by <code>SetTitleMatchMode</code>: <code>1</code>, <code>2</code>, <code>3</code>, or <code>RegEx</code>.
		/// </summary>
		public static object A_TitleMatchMode
		{
			get
			{
				var l = titleMatchMode.Al();
				return l == 4L ? Keywords.Keyword_RegEx : l;
			}
			set
			{
				switch (value.ToString().ToLowerInvariant())
				{
					case "1": titleMatchMode = 1L; break;

					case "2": titleMatchMode = 2L; break;

					case "3": titleMatchMode = 3L; break;

					case Keywords.Keyword_RegEx: titleMatchMode = 4L; break;
				}
			}
		}

		/// <summary>
		/// The current match speed (<code>fast</code> or <code>slow</code>) set by <code>SetTitleMatchMode</code>.
		/// </summary>
		public static object A_TitleMatchModeSpeed
		{
			get => titleMatchModeSpeed.Ab() ? Keywords.Keyword_Fast : Keywords.Keyword_Slow;

			set
			{
				switch (value.ToString().ToLowerInvariant())
				{
					case Keywords.Keyword_Fast: titleMatchModeSpeed = true; break;

					case Keywords.Keyword_Slow: titleMatchModeSpeed = false; break;
				}
			}
		}

		public static long A_TotalScreenHeight => SystemInformation.VirtualScreen.Height;
		public static long A_TotalScreenWidth => SystemInformation.VirtualScreen.Width;
		public static Menu A_TrayMenu => Script.trayMenu;
		public static object A_UseHook { get; set; }

		/// <summary>
		/// The logon name of the current user.
		/// </summary>
		public static string A_UserName => Environment.UserName;

		/// <summary>
		/// Current 1-digit day of the week (1-7). 1 is Sunday in all locales.
		/// </summary>
		public static long A_WDay => (int)DateTime.Now.DayOfWeek + 1;

		public static object A_WinActivateForce => Script.WinActivateForce;

		/// <summary>
		/// The current delay set by <code>SetWinDelay</code>.
		/// </summary>
		public static object A_WinDelay
		{
			get => winDelay;
			set => winDelay = ThreadAccessors.A_WinDelay = value.Al();
		}

		/// <summary>
		/// The Windows directory. For example: <code>C:\Windows</code>.
		/// </summary>
		public static string A_WinDir => Environment.GetFolderPath(Environment.SpecialFolder.Windows);

		public static long A_WorkAreaHeight => System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
		public static long A_WorkAreaWidth => System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;

		/// <summary>
		/// The script's current working directory, which is where files will be accessed by default.
		/// </summary>
		public static object A_WorkingDir
		{
			get => Environment.CurrentDirectory;

			set
			{
				var str = value.ToString();

				if (Directory.Exists(str))
					Environment.CurrentDirectory = str;
			}
		}

		/// <summary>
		/// Current day of the year (1-366).
		/// </summary>
		public static long A_YDay => DateTime.Now.DayOfYear;

		/// <summary>
		/// Current 4-digit year (e.g. 2004).
		/// </summary>
		public static long A_Year => DateTime.Now.Year;

		/// <summary>
		/// Current year and week number (e.g. <code>200453</code>) according to ISO 8601.
		/// </summary>
		public static string A_YWeek
		{
			get
			{
				var now = DateTime.Now;
				var cal = new GregorianCalendar(GregorianCalendarTypes.Localized);
				var week = cal.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
				return $"{now:yyyy}{week:D2}";
			}
		}

		/// <summary>
		/// See <see cref="A_Year"/>.
		/// </summary>
		public static long A_YYYY => A_Year;

		public static long False => 0L;
		public static long True => 1L;

		/// <summary>
		/// The most recent reason the script was asked to terminate. This variable is blank unless the script has an OnExit subroutine and that subroutine is currently running or has been called at least once by an exit attempt. See OnExit for details.
		/// </summary>
		internal static object A_ExitReason
		{
			get;
			set;
		} = "";

		internal static object A_IconFrozen
		{
			get => iconFrozen ?? (iconFrozen = false).Value;
			set => iconFrozen = value.ParseBool();
		}

		/// <summary>
		/// Helper to provide the DPI as a percentage.
		/// </summary>
		internal static double A_ScaledScreenDPI => A_ScreenDPI / 96.0;

		//if (A_IsCompiled != 0)//  return Path.GetFileName(GetAssembly().Location);//else if (scriptName == "*")//  return "*";//else//  return Path.GetFileName(scriptName);

		internal static Assembly GetAssembly() => CompilerHelper.compiledasm ?? Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
	}

	internal static class ThreadAccessors
	{
		internal static long A_ControlDelay
		{
			get => Threads.GetThreadVariables().controlDelay;
			set => Threads.GetThreadVariables().controlDelay = value;
		}

		internal static long A_DefaultMouseSpeed
		{
			get => Threads.GetThreadVariables().defaultMouseSpeed;
			set => Threads.GetThreadVariables().defaultMouseSpeed = value;
		}

		internal static bool A_DetectHiddenText
		{
			get => Threads.GetThreadVariables().detectHiddenText;
			set => Threads.GetThreadVariables().detectHiddenText = value;
		}

		internal static bool A_DetectHiddenWindows
		{
			get => Threads.GetThreadVariables().detectHiddenWindows;
			set => Threads.GetThreadVariables().detectHiddenWindows = value;
		}

		internal static object A_FileEncoding
		{
			get
			{
				var fileEncoding = A_FileEncodingRaw;
				var val = fileEncoding.BodyName;

				if (fileEncoding is UnicodeEncoding ue)
				{
					if (ue.GetPreamble().Length == 0)
						val += "-raw";
				}
				else if (fileEncoding is UTF8Encoding u8)
				{
					if (u8.GetPreamble().Length == 0)
						val += "-raw";
				}

				return val;
			}
			set
			{
				Threads.GetThreadVariables().fileEncoding = Files.GetEncoding(value.ToString());
			}
		}

		internal static Encoding A_FileEncodingRaw => Threads.GetThreadVariables().fileEncoding;

		internal static long A_KeyDelay
		{
			get => Threads.GetThreadVariables().keyDelay;
			set => Threads.GetThreadVariables().keyDelay = value;
		}

		internal static long A_KeyDelayPlay
		{
			get => Threads.GetThreadVariables().keyDelayPlay;
			set => Threads.GetThreadVariables().keyDelayPlay = value;
		}

		internal static long A_KeyDuration
		{
			get => Threads.GetThreadVariables().keyDuration;
			set => Threads.GetThreadVariables().keyDuration = value;
		}

		internal static long A_KeyDurationPlay
		{
			get => Threads.GetThreadVariables().keyDurationPlay;
			set => Threads.GetThreadVariables().keyDurationPlay = value;
		}

		internal static long A_MouseDelay
		{
			get => Threads.GetThreadVariables().mouseDelay;
			set => Threads.GetThreadVariables().mouseDelay = value;
		}

		internal static long A_MouseDelayPlay
		{
			get => Threads.GetThreadVariables().mouseDelayPlay;
			set => Threads.GetThreadVariables().mouseDelayPlay = value;
		}

		internal static long A_PeekFrequency
		{
			get => Threads.GetThreadVariables().peekFrequency;
			set => Threads.GetThreadVariables().peekFrequency = value;
		}

#if WINDOWS

		internal static long A_RegView
		{
			get => Threads.GetThreadVariables().regView;
			set => Threads.GetThreadVariables().regView = value;
		}

#endif

		internal static uint A_SendLevel
		{
			get => Threads.GetThreadVariables().sendLevel;
			set => Threads.GetThreadVariables().sendLevel = value;
		}

		internal static SendModes A_SendMode
		{
			get => Threads.GetThreadVariables().sendMode;
			set => Threads.GetThreadVariables().sendMode = value;
		}

		internal static bool A_StoreCapsLockMode
		{
			get => Threads.GetThreadVariables().storeCapsLockMode;
			set => Threads.GetThreadVariables().storeCapsLockMode = value;
		}

		internal static long A_WinDelay
		{
			get => Threads.GetThreadVariables().winDelay;
			set => Threads.GetThreadVariables().winDelay = value;
		}
	}
}