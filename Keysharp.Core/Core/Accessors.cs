using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//MATT
using Keysharp.Scripting;

namespace Keysharp.Core
{
	public static class Accessors
	{
		internal static long ClipboardTimeout = 1000;

		private static bool allowMainWindow = true;

		[ThreadStatic]
		private static long? controlDelay;

		[ThreadStatic]
		private static long? defaultMouseSpeed;

		[ThreadStatic]
		private static bool? detectHiddenText;

		[ThreadStatic]
		private static bool? detectHiddenWindows;

		[ThreadStatic]
		private static object eventInfo;

		[ThreadStatic]
		private static Encoding fileEncoding;

		[ThreadStatic]
		private static string formatNumeric;

		private static long hotkeyModifierTimeout = 50L;

		private static long hotkeyThrottleInterval = 2000L;

		private static bool? iconFrozen;

		private static bool iconHidden;

		private static string initialWorkingDir = Environment.CurrentDirectory;

		[ThreadStatic]
		private static long? keyDelay;

		[ThreadStatic]
		private static long? keyDelayPlay;

		[ThreadStatic]
		private static long? keyDuration;

		[ThreadStatic]
		private static long? keyDurationPlay;

		private static long maxHotkeysPerInterval = 2000L;

		private static string menuMaskKey = "";

		[ThreadStatic]
		private static long? mouseDelay;

		[ThreadStatic]
		private static long? mouseDelayPlay;

		[ThreadStatic]
		private static bool? noTimers;

		[ThreadStatic]
		private static long? peekFrequency;

		private static Icon prevTrayIcon;

		[ThreadStatic]
		private static long? priority;

		[ThreadStatic]
		private static long? sendLevel;

		[ThreadStatic]
		private static SendModes? sendMode;

		[ThreadStatic]
		private static bool? storeCapsLockMode;

		[ThreadStatic]
		private static long? titleMatchMode;

		[ThreadStatic]
		private static bool? titleMatchModeSpeed;

		private static bool? winActivateForce;

		[ThreadStatic]
		private static long? winDelay;

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
					if (!val.Value)
						Keysharp.Scripting.Script.trayMenu.HideItem("&Open");
					else
						Keysharp.Scripting.Script.trayMenu.ShowItem("&Open");

					allowMainWindow = val.Value;
				}
			}
		}

		/// <summary>
		/// The full path and name of the folder containing the current user's application-specific data. For example: <code>C:\Documents and Settings\Username\Application Data</code>
		/// </summary>
		public static string A_AppData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

		/// <summary>
		/// The full path and name of the folder containing the all-users application-specific data.
		/// </summary>
		public static string A_AppDataCommon => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

		public static Array A_Args { get; internal set; }

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
		/// (synonymous with A_NumBatchLines) The current value as set by SetBatchLines. Examples: 200 or 10ms (depending on format).
		/// </summary>
		[Obsolete]
		public static string A_BatchLines => null;

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
				if (WindowsAPI.OpenClipboard(Accessors.ClipboardTimeout))
				{
					_ = WindowsAPI.CloseClipboard();//Need to close it for it to work

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
				}

				return "";
			}
			set
			{
				if (value != null)
				{
					if (WindowsAPI.OpenClipboard(Accessors.ClipboardTimeout))
					{
						_ = WindowsAPI.CloseClipboard();//Need to close it for it to work

						if (value is ClipboardAll arr)
							Env.RestoreClipboardAll(arr, arr.Count);
						else if (value is string s && s?.Length == 0)
							Clipboard.Clear();
						else
							Clipboard.SetDataObject(value.ParseObject().ToString(), true);
					}
				}
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
			get => controlDelay ?? (controlDelay = 20).Value;
			set => controlDelay = value.ParseLong();
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
		Cursor.Current is Cursor cur
		? cur.ToString().Trim(new char[] { '[', ']' }).Split(' ', StringSplitOptions.RemoveEmptyEntries)[1].Replace("Cursor", "")
		: "Default";

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

		/// <summary>
		/// Sets the mouse speed that will be used if unspecified in <see cref="Click"/>.
		/// </summary>
		public static object A_DefaultMouseSpeed
		{
			get => defaultMouseSpeed ?? (defaultMouseSpeed = 2).Value;
			set => defaultMouseSpeed = value.ParseLong();
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
			get => detectHiddenText ?? (detectHiddenText = true).Value;

			set
			{
				var val = Options.OnOff(value);

				if (val != null)
					detectHiddenText = val.Value ? true : false;
			}
		}

		/// <summary>
		/// Determines whether invisible windows are "seen".
		/// </summary>
		public static object A_DetectHiddenWindows
		{
			get => detectHiddenWindows ?? (detectHiddenWindows = false).Value;

			set
			{
				var val = Options.OnOff(value);

				if (val != null)
					detectHiddenWindows = val.Value ? true : false;
			}
		}

		/// <summary>
		/// The native directory seperator string, i.e. "/" on Unix, "\" on Windows.
		/// </summary>
		public static string A_DirSeperator => Path.DirectorySeparatorChar.ToString();

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
		/// Indicates the success or failure of a command.
		/// </summary>
		[Obsolete]
		public static long A_ErrorLevel//Need to get rid of all usages of this.//TODO
		{
			get;
			set;
		}

		/// <summary>
		/// Contains event information from various commands.
		/// </summary>
		public static object A_EventInfo
		{
			get => eventInfo;
			set => eventInfo = value;
		}

		/// <summary>
		/// The most recent reason the script was asked to terminate. This variable is blank unless the script has an OnExit subroutine and that subroutine is currently running or has been called at least once by an exit attempt. See OnExit for details.
		/// </summary>
		public static object A_ExitReason
		{
			get;
			internal set;
		} = "";

		public static object A_FileEncoding
		{
			get
			{
				var val = FileEncoding.BodyName;

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

			set => fileEncoding = File.GetEncoding(value.ParseObject().ToString());
		}

		/// <summary>
		/// The current floating point number format.
		/// </summary>
		[Obsolete]
		public static object A_FormatFloat
		{
			get
			{
				if (A_FormatNumeric is string s)
				{
					if (s.IndexOf("e", System.StringComparison.OrdinalIgnoreCase) != -1)
						return s;

					if (s.IndexOf("f", System.StringComparison.OrdinalIgnoreCase) != -1)
					{
						var format = s.Replace("f", "").Replace("F", "");
						return string.Concat(format.Length == 0 ? "0" : int.Parse(format).ToString(), ".",
											 System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalDigits.ToString());
					}
				}

				return "";
			}
			set
			{
				var e = false;
				var str = value.ParseObject().ToString();

				foreach (var exp in new[] { str.IndexOf('e'), str.IndexOf('E') })
				{
					if (exp == -1)
					{
						continue;
					}

					A_FormatNumeric = str.Substring(exp);
					str = str.Substring(0, exp);
					e = true;
				}
				var parts = str.Split(new[] { '.' }, 2);

				if (!e && int.TryParse(parts[0], out var n) && n != 0)
					A_FormatNumeric = "f" + n;

				if (parts.Length > 1 && int.TryParse(parts[1], out n))
				{
					var t = System.Threading.Thread.CurrentThread;
					var ci = new CultureInfo(t.CurrentCulture.LCID);
					ci.NumberFormat.NumberDecimalDigits = n;
					t.CurrentCulture = ci;
				}
			}
		}

		/// <summary>
		/// The current integer format, either <c>H</c> or <c>D</c>.
		/// </summary>
		[Obsolete]
		public static object A_FormatInteger
		{
			get => (string)A_FormatNumeric == "f" ? "D" : (string)A_FormatNumeric == "x" ? "H" : "";

			set
			{
				switch (value.ParseObject().ToString().ToLowerInvariant())
				{
					case Core.Keyword_Hex:
					case Core.Keyword_FormatHex:
						A_FormatNumeric = "x";
						break;

					case Core.Keyword_FormatDecimal:
						A_FormatNumeric = "f";
						break;
				}
			}
		}

		/// <summary>
		/// The current numeric format.
		/// </summary>
		public static object A_FormatNumeric
		{
			get
			{
				if (formatNumeric != null)
					return formatNumeric;

				var t = System.Threading.Thread.CurrentThread;
				var ci = new CultureInfo(t.CurrentCulture.LCID);
				ci.NumberFormat.NumberDecimalDigits = 6;
				t.CurrentCulture = ci;
				return formatNumeric = "f";
			}

			set => formatNumeric = value.ParseObject().ToString();
		}

		/// <summary>
		/// The GUI window number that launched the current thread. This variable is blank unless a Gui control, menu bar item, or event such as GuiClose/GuiEscape launched the current thread.
		/// </summary>
		[Obsolete]
		public static string A_Gui => null;

		/// <summary>
		/// The name of the variable associated with the GUI control that launched the current thread. If that control lacks an associated variable, A_GuiControl instead contains the first 63 characters of the control's text/caption (this is most often used to avoid giving each button a variable name). A_GuiControl is blank whenever: 1) A_Gui is blank; 2) a GUI menu bar item or event such as GuiClose/GuiEscape launched the current thread; 3) the control lacks an associated variable and has no caption; or 4) The control that originally launched the current thread no longer exists (perhaps due to Gui Destroy).
		/// </summary>
		[Obsolete]
		public static string A_GuiControl => null;

		/// <summary>
		/// See <see cref="A_GuiEvent"/>.
		/// </summary>
		[Obsolete]
		public static string A_GuiControlEvent => null;

		[Obsolete]
		public static string A_GuiEvent => null;

		[Obsolete]
		public static string A_GuiHeight => null;

		[Obsolete]
		public static string A_GuiWidth => null;

		[Obsolete]
		public static string A_GuiX => null;

		[Obsolete]
		public static string A_GuiY => null;

		public static object A_HotkeyModifierTimeout
		{
			get => hotkeyModifierTimeout;
			set => hotkeyModifierTimeout = value.ParseLong().Value;
		}

		public static object A_HotkeyThrottleInterval
		{
			get => hotkeyThrottleInterval;
			set => hotkeyThrottleInterval = value.ParseLong().Value;
		}

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
				if (Parser.NoTrayIcon)
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
		/// Blank if A_IconFile is blank. Otherwise, it's the number of the icon in A_IconFile (typically 1).
		/// </summary>
		public static long A_IconNumber { get; internal set; } = 1L;

		/// <summary>
		/// Blank unless a custom tooltip for the tray icon has been specified via Menu, Tray, Tip -- in which case it's the text of the tip.
		/// </summary>
		public static object A_IconTip
		{
			get => Script.Tray != null ? Script.Tray.Text : "";

			set
			{
				if (Script.Tray != null)
					Script.Tray.Text = value.ParseObject().ToString();
			}
		}

		/// <summary>
		/// The number of the current loop iteration.
		/// </summary>
		public static long A_Index => Loops.loops.Count > 0 ? Loops.loops.Peek().index + 1 : default;

		public static string A_InitialWorkingDir => initialWorkingDir;

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

		/// <summary>
		/// The IP address of the first network adapter in the computer.
		/// </summary>
		[Obsolete]
		public static string A_IPAddress1 => GetIpFromIndex(1);

		/// <summary>
		/// The IP address of the second network adapter in the computer.
		/// </summary>
		[Obsolete]
		public static string A_IPAddress2 => GetIpFromIndex(2);

		/// <summary>
		/// The IP address of the third network adapter in the computer.
		/// </summary>
		[Obsolete]
		public static string A_IPAddress3 => GetIpFromIndex(3);

		/// <summary>
		/// The IP address of the fourth network adapter in the computer.
		/// </summary>
		[Obsolete]
		public static string A_IPAddress4 => GetIpFromIndex(4);

		public static bool A_Is64bitOS => true;

		/// <summary>
		/// <code>true</code> if the current user has administrator rights, <code>false</code> otherwise.
		/// </summary>
		public static bool A_ISAdmin => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

		/// <summary>
		/// <code>true</code> if the current executing assembly is a compiled script, <code>false</code> otherwise;
		/// </summary>
		public static bool A_IsCompiled
		{
			get
			{
				var path = Path.GetFileName(Application.ExecutablePath).ToLowerInvariant();
				return path != "keysharp.exe" && path != "testhost.exe";
			}
		}

		/// <summary>
		/// <c>1</c> if the current thread is marked as critical, <c>0</c> otherwise.
		/// </summary>
		public static bool A_IsCritical => System.Threading.Thread.CurrentThread.Priority == ThreadPriority.Highest;

		/// <summary>
		/// <code>true</code> if the script is suspended, <code>false</code> otherwise;
		/// </summary>
		public static bool A_IsSuspended => Flow.Suspended;

		/// <summary>
		/// Only for compatibility with AHK, C# programs are always unicode.
		/// </summary>
		public static bool A_IsUnicode => true;

		/// <summary>
		/// The delay that will occur after each keystroke sent by <see cref="Send"/> and <see cref="ControlSend"/>.
		/// </summary>
		public static object A_KeyDelay
		{
			get => keyDelay ?? (keyDelay = 10).Value;
			set => keyDelay = value.ParseLong();
		}

		/// <summary>
		/// The delay that will occur in SendPlay mode after each keystroke sent by <see cref="Send"/> and <see cref="ControlSend"/>.
		/// </summary>
		public static object A_KeyDelayPlay
		{
			get => keyDelayPlay ?? (keyDelayPlay = -1).Value;
			set => keyDelayPlay = value.ParseLong();
		}

		/// <summary>
		/// The delay between the press of a key and before its release, used with <see cref="A_KeyDelay"/>.
		/// </summary>
		public static object A_KeyDuration
		{
			get => keyDuration ?? (keyDuration = -1).Value;
			set => keyDuration = value.ParseLong();
		}

		/// <summary>
		/// The delay in SendPlay mode between the press of a key and before its release, used with <see cref="A_KeyDelayPlay"/>.
		/// </summary>
		public static object A_KeyDurationPlay
		{
			get => keyDurationPlay ?? (keyDurationPlay = -1).Value;
			set => keyDurationPlay = value.ParseLong();
		}

		public static string A_KeysharpPath => A_AhkPath;

		/// <summary>
		/// The system's default language code.
		/// </summary>
		public static long A_Language => System.Threading.Thread.CurrentThread.CurrentCulture.LCID;

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
				if (Loops.loops.Count == 0)
					return null;

				//var stack = loops.ToArray();

				foreach (var l in Loops.loops)//Since loop is a stack, this goes in reverse order, which is what we want.
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
				if (A_LoopFileFullPath is string s && (System.IO.File.Exists(s) || Directory.Exists(s)))
				{
					var val = "";
					var attr = System.IO.File.GetAttributes(s);

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
				return file != null && System.IO.File.Exists(file) ? new System.IO.FileInfo(file).Length : 0L;
			}
		}

		/// <summary>
		/// The size in Kbytes of the file currently retrieved, rounded down to the nearest integer.
		/// </summary>
		public static long A_LoopFileSizeKB => A_LoopFileSize / 1024;

		/// <summary>
		/// The size in Mbytes of the file currently retrieved, rounded down to the nearest integer.
		/// </summary>
		public static long A_LoopFileSizeMB => A_LoopFileSize / (1024 * 1024);

		/// <summary>
		/// The time the file was last accessed. Format YYYYMMDDHH24MISS.
		/// </summary>
		public static string A_LoopFileTimeAccessed
		{
			get
			{
				var file = Loops.GetDirLoopFilename();
				return !string.IsNullOrEmpty(file) ? Conversions.ToYYYYMMDDHH24MISS(System.IO.File.GetLastAccessTime(file)) : "";
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
				return !string.IsNullOrEmpty(file) ? Conversions.ToYYYYMMDDHH24MISS(System.IO.File.GetCreationTime(file)) : "";
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
				return !string.IsNullOrEmpty(file) ? Conversions.ToYYYYMMDDHH24MISS(System.IO.File.GetLastWriteTime(file)) : "";
			}
		}

		/// <summary>
		/// The current object key in an each-loop.
		/// </summary>
		public static object A_LoopKey
		{
			get
			{
				if (Loops.loops.Count == 0)
					return null;

				var stack = Loops.loops.ToArray();

				for (var i = 0; i < stack.Length; i++)
				{
					switch (stack[i].type)
					{
						case LoopType.Each:
						{
							if (!(stack[i].result is object[]))
								return null;

							var pair = (object[])stack[i].result;
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
				if (Loops.loops.Count == 0)
					return "";

				foreach (var l in Loops.loops)
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

		/// <summary>
		/// The name of the root key being accessed (HKEY_LOCAL_MACHINE, HKEY_USERS, HKEY_CURRENT_USER, HKEY_CLASSES_ROOT, or HKEY_CURRENT_CONFIG). For remote registry access, this value will not include the computer name.
		/// </summary>
		public static string A_LoopRegKey
		{
			get
			{
				if (Loops.loops.Count == 0)
					return "";

				foreach (var l in Loops.loops)//Since loop is a stack, this goes in reverse order, which is what we want.
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
				if (Loops.loops.Count == 0)
					return "";

				foreach (var l in Loops.loops)
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
				if (Loops.loops.Count == 0)
					return "";

				foreach (var l in Loops.loops)
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
				if (Loops.loops.Count == 0)
					return "";

				foreach (var l in Loops.loops)
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
				if (Loops.loops.Count == 0)
					return "";

				foreach (var l in Loops.loops)
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

		public static long A_MaxHotkeysPerInterval
		{
			get => maxHotkeysPerInterval;
			set => maxHotkeysPerInterval = value.ParseLong().Value;
		}

		/// <summary>
		/// Current 2-digit day of the month (01-31).
		/// </summary>
		public static string A_MDay => DateTime.Now.ToString("dd");

		public static object A_MenuMaskKey
		{
			get => menuMaskKey;
			set => menuMaskKey = value.ParseObject().ToString();
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
			get => mouseDelay ?? (mouseDelay = 10).Value;
			set => mouseDelay = value.ParseLong();
		}

		/// <summary>
		/// Sets the delay that will occur in SendPlay mode after each mouse movement or click.
		/// </summary>
		public static object A_MouseDelayPlay
		{
			get => mouseDelayPlay ?? (mouseDelayPlay = -1).Value;
			set => mouseDelayPlay = value.ParseLong();
		}

		/// <summary>
		/// Current 3-digit millisecond (000-999).
		/// </summary>
		public static string A_MSec => DateTime.Now.ToString("fff");

		/// <summary>
		/// The full path and name of the current user's "My Documents" folder.
		/// </summary>
		public static string A_MyDocuments => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

		public static object A_NoTimers
		{
			get => noTimers ?? (noTimers = false).Value;
			set => noTimers = value.ParseBool();
		}

		/// <summary>
		/// The current local time in YYYYMMDDHH24MISS format.
		/// </summary>
		public static string A_Now => Conversions.ToYYYYMMDDHH24MISS(DateTime.Now);

		/// <summary>
		/// The current Coordinated Universal Time (UTC) in YYYYMMDDHH24MISS format.
		/// </summary>
		public static string A_NowUTC => Conversions.ToYYYYMMDDHH24MISS(DateTime.Now.ToUniversalTime());

		/// <summary>
		/// See <see cref="A_BatchLines"/>.
		/// </summary>
		[Obsolete]
		public static string A_NumBatchLines => A_BatchLines;

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
			get => peekFrequency ?? (peekFrequency = 5L).Value;
			set => peekFrequency = value.ParseLong().Value;
		}

		/// <summary>
		/// Represents the ratio of the circumference of a circle to its diameter, specified by the constant, π.
		/// </summary>
		public static double A_PI => Math.PI;

		/// <summary>
		/// The key name of the previously executed hotkey or hotstring.
		/// </summary>
		public static string A_PriorHotkey => Keysharp.Scripting.Script.HookThread.kbdMsSender.PriorHotkey;

		public static object A_Priority
		{
			get => priority ?? (priority = 0).Value;
			set => priority = value.ParseLong();
		}

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

		public static long A_RegView { get; internal set; } = 64L;

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

		public static string A_ScriptFullPath => A_IsCompiled ? GetAssembly().Location : Script.scriptName;

		/// <summary>
		/// The unique ID (HWND/handle) of the script's hidden main window.
		/// </summary>
		public static long A_ScriptHwnd => Script.mainWindow != null ? Script.mainWindow.Handle.ToInt64() : 0L;

		public static string A_ScriptName => Path.GetFileName(Script.scriptName);

		/// <summary>
		/// Current 2-digit second (00-59).
		/// </summary>
		public static string A_Sec => DateTime.Now.ToString("ss");

		//if (A_IsCompiled != 0)//  return Path.GetFileName(GetAssembly().Location);//else if (scriptName == "*")//  return "*";//else//  return Path.GetFileName(scriptName);
		public static object A_SendLevel
		{
			get => sendLevel ?? (sendLevel = 0).Value;
			set => sendLevel = Math.Clamp(value.ParseLong().Value, 0L, 100L);
		}

		public static object A_SendMode
		{
			get => SendMode.ToString();

			set
			{
				if (Enum.TryParse<SendModes>(value.ParseObject().ToString(), out var temp))
					sendMode = temp;
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
			get => storeCapsLockMode ?? (storeCapsLockMode = true).Value;

			set
			{
				var val = Options.OnOff(value);

				if (val != null)
					storeCapsLockMode = val.Value;
			}
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
		public static string A_ThisHotkey => Keysharp.Scripting.Script.HookThread.kbdMsSender.CurrentHotkey;

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
		public static string A_TimeIdle => null;

		/// <summary>
		/// Same as above but ignores artificial keystrokes and/or mouse clicks whenever the corresponding hook (keyboard or mouse) is installed. If neither hook is installed, this variable is equivalent to A_TimeIdle. If only one hook is present, only that one type of artificial input will be ignored. A_TimeIdlePhysical may be more useful than A_TimeIdle for determining whether the user is truly present.
		/// </summary>
		public static string A_TimeIdlePhysical => null;

		/// <summary>
		/// Time in ms that have elapsed since <see cref="A_PriorHotkey"/> was pressed. It will be -1 whenever <see cref="A_PriorHotkey"/> is blank.
		/// </summary>
		public static int A_TimeSincePriorHotkey => Keysharp.Scripting.Script.HookThread.kbdMsSender.PriorHotkey == null ? -1 : Environment.TickCount - Keysharp.Scripting.Script.HookThread.kbdMsSender.PriorHotkeyTime;

		/// <summary>
		/// Time in ms that have elapsed since <see cref="A_ThisHotkey"/> was pressed. It will be -1 whenever <see cref="A_ThisHotkey"/> is blank.
		/// </summary>
		public static int A_TimeSinceThisHotkey => Keysharp.Scripting.Script.HookThread.kbdMsSender.CurrentHotkey == null ? -1 : Environment.TickCount - Keysharp.Scripting.Script.HookThread.kbdMsSender.CurrentHotkeyTime;

		/// <summary>
		/// The current mode set by <code>SetTitleMatchMode</code>: <code>1</code>, <code>2</code>, <code>3</code>, or <code>RegEx</code>.
		/// </summary>
		public static object A_TitleMatchMode
		{
			get
			{
				if (titleMatchMode == null)
					titleMatchMode = 2;

				return titleMatchMode.Value == 4 ? Core.Keyword_RegEx : titleMatchMode.Value;
			}
			set
			{
				switch (value.ParseObject().ToString().ToLowerInvariant())
				{
					case "1": titleMatchMode = 1; break;

					case "2": titleMatchMode = 2; break;

					case "3": titleMatchMode = 3; break;

					case Core.Keyword_RegEx: titleMatchMode = 4; break;
				}
			}
		}

		/// <summary>
		/// The current match speed (<code>fast</code> or <code>slow</code>) set by <code>SetTitleMatchMode</code>.
		/// </summary>
		public static object A_TitleMatchModeSpeed
		{
			get
			{
				if (titleMatchModeSpeed == null)
					titleMatchModeSpeed = true;

				return titleMatchModeSpeed.Value ? Core.Keyword_Fast : Core.Keyword_Slow;
			}
			set
			{
				switch (value.ParseObject().ToString().ToLowerInvariant())
				{
					case Core.Keyword_Fast: titleMatchModeSpeed = true; break;

					case Core.Keyword_Slow: titleMatchModeSpeed = false; break;
				}
			}
		}

		public static long A_TotalScreenHeight => SystemInformation.VirtualScreen.Height;

		public static long A_TotalScreenWidth => SystemInformation.VirtualScreen.Width;

		public static Menu A_TrayMenu => Keysharp.Scripting.Script.trayMenu;

		/// <summary>
		/// The logon name of the current user.
		/// </summary>
		public static string A_UserName => Environment.UserName;

		/// <summary>
		/// Current 1-digit day of the week (1-7). 1 is Sunday in all locales.
		/// </summary>
		public static long A_WDay => (int)DateTime.Now.DayOfWeek + 1;

		/// <summary>
		/// The current delay set by <code>SetWinDelay</code>.
		/// </summary>
		public static object A_WinDelay
		{
			get => winDelay ?? (winDelay = 100).Value;
			set => winDelay = value.ParseLong();
		}

		/// <summary>
		/// The Windows directory. For example: <code>C:\Windows</code>.
		/// </summary>
		public static string A_WinDir => Environment.GetFolderPath(Environment.SpecialFolder.System);

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
				var str = value.ParseObject().ToString();

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
		public static string A_YWeek => DateTime.Now.ToString("yyyy") + Math.Floor((double)(DateTime.Now.DayOfYear / 12));

		/// <summary>
		/// See <see cref="A_Year"/>.
		/// </summary>
		public static long A_YYYY => A_Year;

		public static long False => 0L;

		public static long True => 1L;

		public static object WinActivateForce
		{
			get => winActivateForce ?? (winActivateForce = false).Value;
			set => winActivateForce = value.ParseBool();
		}

		internal static object A_IconFrozen
		{
			get => iconFrozen ?? (iconFrozen = false).Value;
			set => iconFrozen = value.ParseBool();
		}

		/// <summary>
		/// Helper to provide the DPI as a percentage.
		/// </summary>
		internal static double A_ScaledScreenDPI => A_ScreenDPI / 96.0;

		internal static Encoding FileEncoding => fileEncoding ?? (fileEncoding = Encoding.Default);

		internal static SendModes SendMode
		{
			get => sendMode ?? (sendMode = SendModes.Input).Value;
			set => sendMode = value;
		}

		public static void SetControlDelay(params object[] obj) => Accessors.A_ControlDelay = obj.L().L1();

		public static void SetWinDelay(params object[] obj) => Accessors.A_WinDelay = obj.L().L1();

		internal static Assembly GetAssembly() => CompilerHelper.compiledasm ?? Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

		private static string GetIpFromIndex(int index)
		{
			var addr = A_IPAddress;
			return (long)addr.Length > index - 1 ? addr[index] as string : "";
		}
	}
}