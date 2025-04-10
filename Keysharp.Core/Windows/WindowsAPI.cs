#if WINDOWS

namespace Keysharp.Core.Windows
{
	internal enum GetAncestorFlags
	{
		/// <summary>
		/// Retrieves the parent window. This does not include the owner, as it does with the GetParent function.
		/// </summary>
		GetParent = 1,

		/// <summary>
		/// Retrieves the root window by walking the chain of parent windows.
		/// </summary>
		GetRoot = 2,

		/// <summary>
		/// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
		/// </summary>
		GetRootOwner = 3
	}

	[Flags]
	internal enum KnownFolderFlag : uint
	{
		None = 0x0,
		CREATE = 0x8000,
		DONT_VERFIY = 0x4000,
		DONT_UNEXPAND = 0x2000,
		NO_ALIAS = 0x1000,
		INIT = 0x800,
		DEFAULT_PATH = 0x400,
		NOT_PARENT_RELATIVE = 0x200,
		SIMPLE_IDLIST = 0x100,
		ALIAS_ONLY = 0x80000000
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct NMHDR
	{
		internal IntPtr hwndFrom;
		internal UIntPtr idFrom;
		internal uint code;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct LVITEM
	{
		internal uint mask;
		internal int iItem;
		internal int iSubItem;
		internal uint state;
		internal uint stateMask;
		internal IntPtr pszText;
		internal int cchTextMax;
		internal int iImage;
		internal IntPtr lParam;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct JOYCAPS
	{
		internal ushort wMid;
		internal ushort wPid;
		internal string szPname;
		internal uint wXmin;
		internal uint wXmax;
		internal uint wYmin;
		internal uint wYmax;
		internal uint wZmin;
		internal uint wZmax;
		internal uint wNumButtons;
		internal uint wPeriodMin;
		internal uint wPeriodMax;

		internal uint wRmin;               /* minimum r position value */
		internal uint wRmax;               /* maximum r position value */
		internal uint wUmin;               /* minimum u (5th axis) position value */
		internal uint wUmax;               /* maximum u (5th axis) position value */
		internal uint wVmin;               /* minimum v (6th axis) position value */
		internal uint wVmax;               /* maximum v (6th axis) position value */
		internal uint wCaps;               /* joystick capabilites */
		internal uint wMaxAxes;            /* maximum number of axes supported */
		internal uint wNumAxes;            /* number of axes in use */
		internal uint wMaxButtons;         /* maximum number of buttons supported */
		private readonly string szRegKey;/* registry key */
		private readonly string szOEMVxD; /* OEM VxD in use */
	}

	[Serializable, StructLayout(LayoutKind.Sequential)]
	internal struct JOYINFOEX
	{
		internal static JOYINFOEX Default
		{
			get
			{
				var jie = new JOYINFOEX();
				jie.dwSize = (uint)Marshal.SizeOf(jie);
				return jie;
			}
		}

		internal uint dwSize;                /* size of structure */
		internal uint dwFlags;               /* flags to indicate what to return */
		internal uint dwXpos;                /* x position */
		internal uint dwYpos;                /* y position */
		internal uint dwZpos;                /* z position */
		internal uint dwRpos;                /* rudder/4th axis position */
		internal uint dwUpos;                /* 5th axis position */
		internal uint dwVpos;                /* 6th axis position */
		internal uint dwButtons;             /* button states */
		internal uint dwButtonNumber;        /* current button number pressed */
		internal int dwPOV;                 /* point of view state */
		internal uint dwReserved1;           /* reserved for communication between winmm & driver */
		internal uint dwReserved2;           /* reserved for future expansion */
	}

	//This abbreviated definition is based on the actual definition in kbd.h (Windows DDK).
	[StructLayout(LayoutKind.Sequential)]
	internal struct KBDTABLES64
	{
		internal ulong pCharModifiers;
		internal ulong pVkToWcharTable;
		internal ulong pDeadKey;
		internal ulong pKeyNames;
		internal ulong pKeyNamesExt;
		internal ulong pKeyNamesDead;
		internal ulong pusVSCtoVK;
		internal byte bMaxVSCtoVK;
		internal ulong pVSCtoVK_E0;
		internal ulong pVSCtoVK_E1;

		// This is the one we want:
		internal uint fLocaleFlags;

		// Struct definition truncated.
	}

	internal enum DWMWINDOWATTRIBUTE
	{
		DWMWA_NCRENDERING_ENABLED = 1,
		DWMWA_NCRENDERING_POLICY,
		DWMWA_TRANSITIONS_FORCEDISABLED,
		DWMWA_ALLOW_NCPAINT,
		DWMWA_CAPTION_BUTTON_BOUNDS,
		DWMWA_NONCLIENT_RTL_LAYOUT,
		DWMWA_FORCE_ICONIC_REPRESENTATION,
		DWMWA_FLIP3D_POLICY,
		DWMWA_EXTENDED_FRAME_BOUNDS,
		DWMWA_HAS_ICONIC_BITMAP,
		DWMWA_DISALLOW_PEEK,
		DWMWA_EXCLUDED_FROM_PEEK,
		DWMWA_CLOAK,
		DWMWA_CLOAKED,
		DWMWA_FREEZE_REPRESENTATION,
		DWMWA_LAST
	};

	internal enum AccessProtectionFlags
	{
		PAGE_NOACCESS = 0x001,
		PAGE_READONLY = 0x002,
		PAGE_READWRITE = 0x004,
		PAGE_WRITECOPY = 0x008,
		PAGE_EXECUTE = 0x010,
		PAGE_EXECUTE_READ = 0x020,
		PAGE_EXECUTE_READWRITE = 0x040,
		PAGE_EXECUTE_WRITECOPY = 0x080,
		PAGE_GUARD = 0x100,
		PAGE_NOCACHE = 0x200,
		PAGE_WRITECOMBINE = 0x400
	}

	[Flags]
	internal enum VirtualAllocExTypes
	{
		WRITE_WATCH_FLAG_RESET = 0x00000001, // Win98 only
		MEM_COMMIT = 0x00001000,
		MEM_RESERVE = 0x00002000,
		MEM_COMMIT_OR_RESERVE = 0x00003000,
		MEM_DECOMMIT = 0x00004000,
		MEM_RELEASE = 0x00008000,
		MEM_FREE = 0x00010000,
		MEM_internal = 0x00020000,
		MEM_MAPPED = 0x00040000,
		MEM_RESET = 0x00080000, // Win2K only
		MEM_TOP_DOWN = 0x00100000,
		MEM_WRITE_WATCH = 0x00200000, // Win98 only
		MEM_PHYSICAL = 0x00400000, // Win2K only
		SEC_IMAGE = 0x01000000,
		MEM_LARGE_PAGES = 0x20000000,
		MEM_IMAGE = SEC_IMAGE
	}

	internal enum ProcessAccessTypes
	{
		PROCESS_TERMINATE = 0x00000001,
		PROCESS_CREATE_THREAD = 0x00000002,
		PROCESS_SET_SESSIONID = 0x00000004,
		PROCESS_VM_OPERATION = 0x00000008,
		PROCESS_VM_READ = 0x00000010,
		PROCESS_VM_WRITE = 0x00000020,
		PROCESS_DUP_HANDLE = 0x00000040,
		PROCESS_CREATE_PROCESS = 0x00000080,
		PROCESS_SET_QUOTA = 0x00000100,
		PROCESS_SET_INFORMATION = 0x00000200,
		PROCESS_QUERY_INFORMATION = 0x00000400,
		STANDARD_RIGHTS_REQUIRED = 0x000F0000,
		SYNCHRONIZE = 0x00100000,

		PROCESS_ALL_ACCESS = PROCESS_TERMINATE | PROCESS_CREATE_THREAD | PROCESS_SET_SESSIONID | PROCESS_VM_OPERATION |
							 PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_DUP_HANDLE | PROCESS_CREATE_PROCESS | PROCESS_SET_QUOTA |
							 PROCESS_SET_INFORMATION | PROCESS_QUERY_INFORMATION | STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE
	}

	[Flags]
	internal enum SendMessageTimeoutFlags : uint
	{
		SMTO_NORMAL = 0x0,
		SMTO_BLOCK = 0x1,
		SMTO_ABORTIFHUNG = 0x2,
		SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
		SMTO_ERRORONEXIT = 0x20
	}

	/// <summary>
	/// Gotten from https://stackoverflow.com/questions/12777395/using-column-imagekey-in-a-listview-to-simulate-windows-explorer-style-sorting
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct LV_COLUMN
	{
		internal uint mask;
		internal int fmt;
		internal int cx;
		internal string pszText;
		internal int cchTextMax;
		internal int iSubItem;
		internal int iImage;
		internal int iOrder;
	}

	[Flags]
	public enum MOUSEEVENTF : uint
	{
		MOVE = 0x0001,  // mouse move
		LEFTDOWN = 0x0002,  // left button down
		LEFTUP = 0x0004,  // left button up
		RIGHTDOWN = 0x0008,  // right button down
		RIGHTUP = 0x0010,  // right button up
		MIDDLEDOWN = 0x0020,  // middle button down
		MIDDLEUP = 0x0040,  // middle button up
		XDOWN = 0x0080,  // x button down
		XUP = 0x0100,  // x button down
		WHEEL = 0x0800,  // wheel button rolled
		VIRTUALDESK = 0x4000,  // map to entire virtual desktop
		ABSOLUTE = 0x8000,  // absolute move
		HWHEEL = 0x01000, // hwheel button rolled
		MOVE_NOCOALESCE = 0x2000//do not coalesce mouse moves.
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct FILETIME
	{
		internal uint DateTimeLow;
		internal uint DateTimeHigh;
	}

	[Serializable, StructLayout(LayoutKind.Sequential)]
	internal struct GUITHREADINFO
	{
		internal static GUITHREADINFO Default
		{
			get
			{
				var info = new GUITHREADINFO();
				info.cbSize = (uint)Marshal.SizeOf(info);
				return info;
			}
		}

		internal uint cbSize;
		internal uint flags;
		internal IntPtr hwndActive;
		internal IntPtr hwndFocus;
		internal IntPtr hwndCapture;
		internal IntPtr hwndMenuOwner;
		internal IntPtr hwndMoveSize;
		internal IntPtr hwndCaret;
		internal RECT rcCaret;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct HARDWAREINPUT
	{
		internal uint uMsg;
		internal ushort wParamL;
		internal ushort wParamH;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct INPUT
	{
		internal uint type;
		internal INPUTDATA i;
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct INPUTDATA
	{
		[FieldOffset(0)]
		internal MOUSEINPUT m;

		[FieldOffset(0)]
		internal KEYBDINPUT k;

		[FieldOffset(0)]
		internal HARDWAREINPUT h;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct KEYBDINPUT
	{
		internal ushort wVk;
		internal ushort wScan;
		internal uint dwFlags;
		internal uint time;
		internal ulong dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct MOUSEINPUT
	{
		internal int dx;
		internal int dy;
		internal uint mouseData;
		internal uint dwFlags;
		internal uint time;
		internal ulong dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct KBDLLHOOKSTRUCT
	{
		internal uint vkCode;
		internal uint scanCode;
		internal uint flags;
		internal uint time;
		internal ulong dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct MSDLLHOOKSTRUCT
	{
		internal POINT pt;
		internal int mouseData;//Pinvoke.net claims this must be int and not uint.
		internal uint flags;
		internal int time;
		internal UIntPtr dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct POINT
	{
		internal int x;
		internal int y;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT//Only public for testing purposes.
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}

	[Serializable, StructLayout(LayoutKind.Sequential)]
	internal struct WINDOWPLACEMENT
	{
		internal uint length;
		internal uint flags;
		internal uint showCmd;
		internal POINT ptMinPosition;
		internal POINT ptMaxPosition;
		internal RECT rcNormalPosition;
		internal RECT rcDevice;

		internal static WINDOWPLACEMENT Default
		{
			get
			{
				var result = new WINDOWPLACEMENT();
				result.length = (uint)Marshal.SizeOf(result);
				return result;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Msg
	{
		internal IntPtr hwnd;
		internal uint message;
		internal IntPtr wParam;
		internal IntPtr lParam;
		internal uint time;
		internal POINT pt;//Apparently we can also use System.Drawing.Point, change if needed.
#if _MAC
		internal DWORD lPrivate;
#endif
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct LASTINPUTINFO
	{
		internal static LASTINPUTINFO Default
		{
			get
			{
				var result = new LASTINPUTINFO();
				result.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
				return result;
			}
		}

		[MarshalAs(UnmanagedType.U4)]
		public uint cbSize;

		[MarshalAs(UnmanagedType.U4)]
		public uint dwTime;
	}

	internal struct EventMsg
	{
		internal uint message;
		internal uint paramL;
		internal uint paramH;
		internal uint time;
		internal IntPtr hwnd;
	}

	internal enum gaFlags : uint
	{
		GA_PARENT = 1,
		GA_ROOT = 2,
		GA_ROOTOWNER = 3
	}

	internal enum OBJID : uint
	{
		WINDOW = 0x00000000,
		SYSMENU = 0xFFFFFFFF,
		TITLEBAR = 0xFFFFFFFE,
		MENU = 0xFFFFFFFD,
		CLIENT = 0xFFFFFFFC,
		VSCROLL = 0xFFFFFFFB,
		HSCROLL = 0xFFFFFFFA,
		SIZEGRIP = 0xFFFFFFF9,
		CARET = 0xFFFFFFF8,
		CURSOR = 0xFFFFFFF7,
		ALERT = 0xFFFFFFF6,
		SOUND = 0xFFFFFFF5,
	}

	public static class WindowsAPI
	{
		// File / Device IO
		internal const uint GENERICREAD = 0x80000000;

		internal const int GW_HWNDFIRST = 0;
		internal const int GW_HWNDLAST = 1;
		internal const int GW_HWNDNEXT = 2;
		internal const int GW_HWNDPREV = 3;
		internal const int GW_OWNER = 4;
		internal const int GW_CHILD = 5;
		internal const int GW_ENABLEDPOPUP = 6;
		internal const int GW_MAX = 6;

		internal const int GWL_EXSTYLE = -20;

		internal const int GWL_STYLE = -16;

		internal const int HWND_BOTTOM = 1;

		internal const int HWND_BROADCAST = 0xffff;

		internal const int HWND_NOTOPMOST = -2;

		internal const int HWND_TOP = 0;

		internal const int HWND_TOPMOST = -1;

		internal const int INPUT_HARDWARE = 2;

		internal const int INPUT_KEYBOARD = 1;

		internal const int INPUT_MOUSE = 0;

		internal const int INVALID_HANDLE = -1;

		internal const uint IOCTL_STORAGE_EJECT_MEDIA = 2967560;

		internal const uint IOCTL_STORAGE_EJECTION_CONTROL = 0x2d0940;

		internal const uint IOCTL_STORAGE_LOAD_MEDIA = 0x2d480c;

		internal const int KEYEVENTF_EXTENDEDKEY = 1;
		internal const int KEYEVENTF_KEYUP = 2;
		internal const int KEYEVENTF_UNICODE = 4;
		internal const int KEYEVENTF_SCANCODE = 8;

		internal const int LWA_ALPHA = 0x2;

		internal const int LWA_COLORKEY = 0x1;

		internal const int MAPVK_VK_TO_VSC = 0;
		internal const int MAPVK_VSC_TO_VK = 1;
		internal const int MAPVK_VK_TO_CHAR = 2;
		internal const int MAPVK_VSC_TO_VK_EX = 3;
		internal const int MAPVK_VK_TO_VSC_EX = 4;

		internal const int MF_BYCOMMAND = 0;

		internal const int MF_BYPOSITION = 0x400;

		internal const uint OPENEXISTING = 3;

		internal const int PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF;

		internal const int SC_CLOSE = 0xF060;

		internal const int SHERB_NOCONFIRMATION = 0x1;

		internal const int SHERB_NOPROGRESSUI = 0x2;

		internal const int SHERB_NOSOUND = 0x4;

		internal const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;

		internal const int SW_FORCEMINIMIZE = 11;

		internal const int SW_HIDE = 0;

		internal const int SW_MAX = 11;

		internal const int SW_MAXIMIZE = 3;

		internal const int SW_MINIMIZE = 6;

		internal const int SW_NORMAL = 1;

		internal const int SW_RESTORE = 9;

		internal const int SW_SHOW = 5;

		internal const int SW_SHOWDEFAULT = 10;

		internal const int SW_SHOWMAXIMIZED = 3;

		internal const int SW_SHOWMINIMIZED = 2;

		internal const int SW_SHOWMINNOACTIVE = 7;

		internal const int SW_SHOWNA = 8;

		internal const int SW_SHOWNOACTIVATE = 4;

		internal const int SW_SHOWNORMAL = 1;

		internal const int SWP_NOACTIVATE = 0x10;

		internal const int SWP_NOMOVE = 2;

		internal const int SWP_NOSIZE = 1;

		internal const int SYNCHRONIZE = 0x00100000;

		internal const int LV_REMOTE_BUF_SIZE = 1024;// 8192 (below) seems too large in hindsight, given that an LV can only display the first 260 chars in a field.
		internal const int LV_TEXT_BUF_SIZE = 8192;// Max amount of text in a ListView sub-item.  Somewhat arbitrary: not sure what the real limit is, if any.

		internal const uint WHEEL_DELTA = 120;
		internal const uint XBUTTON1 = 0x0001;
		internal const uint XBUTTON2 = 0x0002;


		internal const int WM_HOTKEY = 0x0312;
		internal const int WM_KEYDOWN = 0x0100;
		internal const int WM_KEYUP = 0x0101;
		internal const int WM_SYSKEYDOWN = 0x0104;
		internal const int WM_SYSKEYUP = 0x0105;
		internal const int WM_CLOSE = 0x0010;
		internal const int WM_QUIT = 0x0012;
		internal const int WM_COMMAND = 0x0111;
		internal const int WM_SETREDRAW = 0x000B;
		internal const int WM_SETTEXT = 0x000C;
		internal const int WM_GETTEXT = 0x000D;
		internal const int WM_GETTEXTLENGTH = 0x000E;
		internal const int WM_SETTINGCHANGE = 0x001A;
		internal const int WM_SYSCOMMAND = 0x0112;
		internal const int WM_USER = 0x0400;
		internal const int WM_KEYFIRST = 0x0100;
		internal const int WM_CHAR = 0x0102;
		internal const int WM_DEADCHAR = 0x0103;
		internal const int WM_SYSCHAR = 0x0106;
		internal const int WM_SYSDEADCHAR = 0x0107;
		internal const int WM_UNICHAR = 0x0109;
		internal const int WM_KEYLAST = 0x0109;
		internal const int WM_MOUSEFIRST = 0x0200;
		internal const int WM_MOUSEMOVE = 0x0200;
		internal const int WM_LBUTTONDOWN = 0x0201;
		internal const int WM_LBUTTONUP = 0x0202;
		internal const int WM_LBUTTONDBLCLK = 0x0203;
		internal const int WM_RBUTTONDOWN = 0x0204;
		internal const int WM_RBUTTONUP = 0x0205;
		internal const int WM_RBUTTONDBLCLK = 0x0206;
		internal const int WM_MBUTTONDOWN = 0x0207;
		internal const int WM_MBUTTONUP = 0x0208;
		internal const int WM_MBUTTONDBLCLK = 0x0209;
		internal const int WM_MOUSEWHEEL = 0x020A;
		internal const int WM_MOUSEHWHEEL = 0x020E;
		internal const int WM_XBUTTONDOWN = 0x020B;
		internal const int WM_NCXBUTTONDOWN = 0x00AB;
		internal const int WM_XBUTTONUP = 0x020C;
		internal const int WM_NCXBUTTONUP = 0x00AC;
		internal const int WM_XBUTTONDBLCLK = 0x020D;
		internal const int WM_MOUSELAST = 0x020E;
		internal const int WM_CLIPBOARDUPDATE = 0x031D;
		internal const int WM_NCCREATE = 0x0081;
		internal const int WM_NCDESTROY = 0x0082;
		internal const int WM_NCCALCSIZE = 0x0083;
		internal const int WM_NCHITTEST = 0x0084;
		internal const int WM_NCPAINT = 0x0085;
		internal const int WM_NCACTIVATE = 0x0086;
		internal const int WM_GETDLGCODE = 0x0087;
		internal const int WM_ENDSESSION = 0x0016;
		internal const int WM_DESTROY = 0x0002;
		internal const int WM_COPYDATA = 0x004A;
		internal const int WM_PAINT = 0x000F;

		internal const uint ENDSESSION_LOGOFF = 0x80000000;

		internal const int HTERROR = -2;
		internal const int HTTRANSPARENT = -1;
		internal const int HTNOWHERE = 0;
		internal const int HTCLIENT = 1;
		internal const int HTCAPTION = 2;
		internal const int HTSYSMENU = 3;
		internal const int HTGROWBOX = 4;
		internal const int HTSIZE = HTGROWBOX;
		internal const int HTMENU = 5;
		internal const int HTHSCROLL = 6;
		internal const int HTVSCROLL = 7;
		internal const int HTMINBUTTON = 8;
		internal const int HTMAXBUTTON = 9;
		internal const int HTLEFT = 10;
		internal const int HTRIGHT = 11;
		internal const int HTTOP = 12;
		internal const int HTTOPLEFT = 13;
		internal const int HTTOPRIGHT = 14;
		internal const int HTBOTTOM = 15;
		internal const int HTBOTTOMLEFT = 16;
		internal const int HTBOTTOMRIGHT = 17;
		internal const int HTBORDER = 18;
		internal const int HTREDUCE = HTMINBUTTON;
		internal const int HTZOOM = HTMAXBUTTON;
		internal const int HTSIZEFIRST = HTLEFT;
		internal const int HTSIZELAST = HTBOTTOMRIGHT;
		internal const int HTOBJECT = 19;

		internal const int HTCLOSE = 20;
		internal const int HTHELP = 21;

		internal const uint PM_NOREMOVE = 0x0000;
		internal const uint PM_REMOVE = 0x0001;
		internal const uint PM_NOYIELD = 0x0002;

		internal const int MK_LBUTTON = 0x0001;
		internal const int MK_RBUTTON = 0x0002;
		internal const int MK_SHIFT = 0x0004;
		internal const int MK_CONTROL = 0x0008;
		internal const int MK_MBUTTON = 0x0010;
		internal const int MK_XBUTTON1 = 0x0020;
		internal const int MK_XBUTTON2 = 0x0040;

		internal const int HOTKEYF_SHIFT = 0x01;
		internal const int HOTKEYF_CONTROL = 0x02;
		internal const int HOTKEYF_ALT = 0x04;

		internal const int WS_EX_LAYERED = 0x80000;
		internal const int WS_EX_TOPMOST = 8;
		internal const int WS_EX_NOACTIVATE = 0x08000000;
		internal const int WS_EX_TOOLWINDOW = 0x00000080;
		internal const int WS_EX_APPWINDOW = 0x00040000;
		internal const int WS_OVERLAPPED = 0x00000000;
		internal const uint WS_POPUP = 0x80000000;
		internal const int WS_CHILD = 0x40000000;
		internal const int WS_MINIMIZE = 0x20000000;
		internal const int WS_VISIBLE = 0x10000000;
		internal const int WS_DISABLED = 0x08000000;
		internal const int WS_CLIPSIBLINGS = 0x04000000;
		internal const int WS_CLIPCHILDREN = 0x02000000;
		internal const int WS_MAXIMIZE = 0x01000000;
		internal const int WS_CAPTION = 0x00C00000;     /* WS_BORDER | WS_DLGFRAME  */
		internal const int WS_BORDER = 0x00800000;
		internal const int WS_DLGFRAME = 0x00400000;
		internal const int WS_VSCROLL = 0x00200000;
		internal const int WS_HSCROLL = 0x00100000;
		internal const int WS_SYSMENU = 0x00080000;
		internal const int WS_THICKFRAME = 0x00040000;
		internal const int WS_GROUP = 0x00020000;
		internal const int WS_TABSTOP = 0x00010000;
		internal const int WS_MINIMIZEBOX = 0x00020000;
		internal const int WS_MAXIMIZEBOX = 0x00010000;
		internal const int WS_TILED = WS_OVERLAPPED;
		internal const int WS_ICONIC = WS_MINIMIZE;
		internal const int WS_SIZEBOX = WS_THICKFRAME;

		internal const int ES_LEFT = 0x0000;
		internal const int ES_CENTER = 0x0001;
		internal const int ES_RIGHT = 0x0002;
		internal const int ES_MULTILINE = 0x0004;
		internal const int ES_UPPERCASE = 0x0008;
		internal const int ES_LOWERCASE = 0x0010;
		internal const int ES_PASSWORD = 0x0020;
		internal const int ES_AUTOVSCROLL = 0x0040;
		internal const int ES_AUTOHSCROLL = 0x0080;
		internal const int ES_NOHIDESEL = 0x0100;
		internal const int ES_OEMCONVERT = 0x0400;
		internal const int ES_READONLY = 0x0800;
		internal const int ES_WANTRETURN = 0x1000;
		internal const int ES_NUMBER = 0x2000;
		internal const int CBS_AUTOHSCROLL = 0x0040;
		internal const int BS_NOTIFY = 0x4000;
		internal const uint TBM_SETTHUMBLENGTH = WM_USER + 27;
		internal const uint TBM_SETTIPSIDE = WM_USER + 31;
		internal const int TBS_FIXEDLENGTH = 0x0040;
		internal const int TBS_TOOLTIPS = 0x0100;
		internal const int TVS_NOHSCROLL = 0x8000;
		internal const uint SB_GETTEXT = WM_USER + 13;
		internal const uint SB_SETPARTS = WM_USER + 4;
		internal const uint SB_GETPARTS = WM_USER + 6;
		internal const uint SB_GETTEXTLENGTH = WM_USER + 12;

		internal const int EM_GETSEL = 0x00B0;
		internal const int EM_SETSEL = 0x00B1;
		internal const int EM_GETRECT = 0x00B2;
		internal const int EM_SETRECT = 0x00B3;
		internal const int EM_SETRECTNP = 0x00B4;
		internal const int EM_SCROLL = 0x00B5;
		internal const int EM_LINESCROLL = 0x00B6;
		internal const int EM_SCROLLCARET = 0x00B7;
		internal const int EM_GETMODIFY = 0x00B8;
		internal const int EM_SETMODIFY = 0x00B9;
		internal const int EM_GETLINECOUNT = 0x00BA;
		internal const int EM_LINEINDEX = 0x00BB;
		internal const int EM_SETHANDLE = 0x00BC;
		internal const int EM_GETHANDLE = 0x00BD;
		internal const int EM_GETTHUMB = 0x00BE;
		internal const int EM_LINELENGTH = 0x00C1;
		internal const int EM_REPLACESEL = 0x00C2;
		internal const int EM_GETLINE = 0x00C4;
		internal const int EM_LIMITTEXT = 0x00C5;
		internal const int EM_CANUNDO = 0x00C6;
		internal const int EM_UNDO = 0x00C7;
		internal const int EM_FMTLINES = 0x00C8;
		internal const int EM_LINEFROMCHAR = 0x00C9;
		internal const int EM_SETTABSTOPS = 0x00CB;
		internal const int EM_SETPASSWORDCHAR = 0x00CC;
		internal const int EM_EMPTYUNDOBUFFER = 0x00CD;
		internal const int EM_GETFIRSTVISIBLELINE = 0x00CE;
		internal const int EM_SETREADONLY = 0x00CF;
		internal const int EM_SETWORDBREAKPROC = 0x00D0;
		internal const int EM_GETWORDBREAKPROC = 0x00D1;
		internal const int EM_GETPASSWORDCHAR = 0x00D2;
		internal const int EM_SETMARGINS = 0x00D3;
		internal const int EM_GETMARGINS = 0x00D4;
		internal const int EM_SETLIMITTEXT = EM_LIMITTEXT;   //win40 Name change
		internal const int EM_GETLIMITTEXT = 0x00D5;
		internal const int EM_POSFROMCHAR = 0x00D6;
		internal const int EM_CHARFROMPOS = 0x00D7;
		internal const int EM_SETIMESTATUS = 0x00D8;
		internal const int EM_GETIMESTATUS = 0x00D9;
		internal const int EM_ENABLEFEATURE = 0x00DA;

		internal const int BM_GETCHECK = 0x00F0;
		internal const int BM_SETCHECK = 0x00F1;
		internal const int BM_GETSTATE = 0x00F2;
		internal const int BM_SETSTATE = 0x00F3;
		internal const int BM_SETSTYLE = 0x00F4;
		internal const int BM_CLICK = 0x00F5;
		internal const int BM_GETIMAGE = 0x00F6;
		internal const int BM_SETIMAGE = 0x00F7;
		internal const int BM_SETDONTCLICK = 0x00F8;
		internal const int BST_UNCHECKED = 0x0000;
		internal const int BST_CHECKED = 0x0001;
		internal const int BST_INDETERMINATE = 0x0002;
		internal const int BST_PUSHED = 0x0004;
		internal const int BST_FOCUS = 0x0008;

		internal const int CBN_ERRSPACE = -1;
		internal const int CBN_SELCHANGE = 1;
		internal const int CBN_DBLCLK = 2;
		internal const int CBN_SETFOCUS = 3;
		internal const int CBN_KILLFOCUS = 4;
		internal const int CBN_EDITCHANGE = 5;
		internal const int CBN_EDITUPDATE = 6;
		internal const int CBN_DROPDOWN = 7;
		internal const int CBN_CLOSEUP = 8;
		internal const int CBN_SELENDOK = 9;
		internal const int CBN_SELENDCANCEL = 10;

		internal const int CB_ERR = -1;
		internal const int CB_ERRSPACE = -2;
		internal const int CB_GETEDITSEL = 0x0140;
		internal const int CB_LIMITTEXT = 0x0141;
		internal const int CB_SETEDITSEL = 0x0142;
		internal const int CB_ADDSTRING = 0x0143;
		internal const int CB_DELETESTRING = 0x0144;
		internal const int CB_DIR = 0x0145;
		internal const int CB_GETCOUNT = 0x0146;
		internal const int CB_GETCURSEL = 0x0147;
		internal const int CB_GETLBTEXT = 0x0148;
		internal const int CB_GETLBTEXTLEN = 0x0149;
		internal const int CB_INSERTSTRING = 0x014A;
		internal const int CB_RESETCONTENT = 0x014B;
		internal const int CB_FINDSTRING = 0x014C;
		internal const int CB_SELECTSTRING = 0x014D;
		internal const int CB_SETCURSEL = 0x014E;
		internal const int CB_SHOWDROPDOWN = 0x014F;
		internal const int CB_GETITEMDATA = 0x0150;
		internal const int CB_SETITEMDATA = 0x0151;
		internal const int CB_GETDROPPEDCONTROLRECT = 0x0152;
		internal const int CB_SETITEMHEIGHT = 0x0153;
		internal const int CB_GETITEMHEIGHT = 0x0154;
		internal const int CB_SETEXTENDEDUI = 0x0155;
		internal const int CB_GETEXTENDEDUI = 0x0156;
		internal const int CB_GETDROPPEDSTATE = 0x0157;
		internal const int CB_FINDSTRINGEXACT = 0x0158;
		internal const int CB_SETLOCALE = 0x0159;
		internal const int CB_GETLOCALE = 0x015A;
		internal const int CB_GETTOPINDEX = 0x015b;
		internal const int CB_SETTOPINDEX = 0x015c;
		internal const int CB_GETHORIZONTALEXTENT = 0x015d;
		internal const int CB_SETHORIZONTALEXTENT = 0x015e;
		internal const int CB_GETDROPPEDWIDTH = 0x015f;
		internal const int CB_SETDROPPEDWIDTH = 0x0160;
		internal const int CB_INITSTORAGE = 0x0161;

		internal const int LB_OKAY = 0;
		internal const int LB_ERR = -1;
		internal const int LB_ERRSPACE = -2;

		internal const int LB_ADDSTRING = 0x0180;
		internal const int LB_INSERTSTRING = 0x0181;
		internal const int LB_DELETESTRING = 0x0182;
		internal const int LB_SELITEMRANGEEX = 0x0183;
		internal const int LB_RESETCONTENT = 0x0184;
		internal const int LB_SETSEL = 0x0185;
		internal const int LB_SETCURSEL = 0x0186;
		internal const int LB_GETSEL = 0x0187;
		internal const int LB_GETCURSEL = 0x0188;
		internal const int LB_GETTEXT = 0x0189;
		internal const int LB_GETTEXTLEN = 0x018A;
		internal const int LB_GETCOUNT = 0x018B;
		internal const int LB_SELECTSTRING = 0x018C;
		internal const int LB_DIR = 0x018D;
		internal const int LB_GETTOPINDEX = 0x018E;
		internal const int LB_FINDSTRING = 0x018F;
		internal const int LB_GETSELCOUNT = 0x0190;
		internal const int LB_GETSELITEMS = 0x0191;
		internal const int LB_SETTABSTOPS = 0x0192;
		internal const int LB_GETHORIZONTALEXTENT = 0x0193;
		internal const int LB_SETHORIZONTALEXTENT = 0x0194;
		internal const int LB_SETCOLUMNWIDTH = 0x0195;
		internal const int LB_ADDFILE = 0x0196;
		internal const int LB_SETTOPINDEX = 0x0197;
		internal const int LB_GETITEMRECT = 0x0198;
		internal const int LB_GETITEMDATA = 0x0199;
		internal const int LB_SETITEMDATA = 0x019A;
		internal const int LB_SELITEMRANGE = 0x019B;
		internal const int LB_SETANCHORINDEX = 0x019C;
		internal const int LB_GETANCHORINDEX = 0x019D;
		internal const int LB_SETCARETINDEX = 0x019E;
		internal const int LB_GETCARETINDEX = 0x019F;
		internal const int LB_SETITEMHEIGHT = 0x01A0;
		internal const int LB_GETITEMHEIGHT = 0x01A1;
		internal const int LB_FINDSTRINGEXACT = 0x01A2;
		internal const int LB_SETLOCALE = 0x01A5;
		internal const int LB_GETLOCALE = 0x01A6;
		internal const int LB_SETCOUNT = 0x01A7;
		internal const int LB_INITSTORAGE = 0x01A8;
		internal const int LB_ITEMFROMPOINT = 0x01A9;

		internal const int LBN_ERRSPACE = -2;
		internal const int LBN_SELCHANGE = 1;
		internal const int LBN_DBLCLK = 2;
		internal const int LBN_SELCANCEL = 3;
		internal const int LBN_SETFOCUS = 4;
		internal const int LBN_KILLFOCUS = 5;

		internal const int LBS_NOTIFY = 0x0001;
		internal const int LBS_SORT = 0x0002;
		internal const int LBS_NOREDRAW = 0x0004;
		internal const int LBS_MULTIPLESEL = 0x0008;
		internal const int LBS_OWNERDRAWFIXED = 0x0010;
		internal const int LBS_OWNERDRAWVARIABLE = 0x0020;
		internal const int LBS_HASSTRINGS = 0x0040;
		internal const int LBS_USETABSTOPS = 0x0080;
		internal const int LBS_NOINTEGRALHEIGHT = 0x0100;
		internal const int LBS_MULTICOLUMN = 0x0200;
		internal const int LBS_WANTKEYBOARDINPUT = 0x0400;
		internal const int LBS_EXTENDEDSEL = 0x0800;
		internal const int LBS_DISABLENOSCROLL = 0x1000;
		internal const int LBS_NODATA = 0x2000;
		internal const int LBS_NOSEL = 0x4000;
		internal const int LBS_COMBOBOX = 0x8000;
		internal const int LBS_STANDARD = LBS_NOTIFY | LBS_SORT | WS_VSCROLL | WS_BORDER;

		internal const int LVM_FIRST = 0x1000;//ListView messages
		internal const int TV_FIRST = 0x1100;//TreeView messages
		internal const int LVM_GETITEMCOUNT = LVM_FIRST + 4;
		internal const int LVM_GETHEADER = LVM_FIRST + 31;
		internal const int LVM_GETNEXTITEM = LVM_FIRST + 12;
		internal const int LVM_GETITEMTEXT = LVM_FIRST + 115;

		internal const int LVNI_FOCUSED = 0x0001;
		internal const int LVNI_SELECTED = 0x0002;
		internal const int LVM_GETSELECTEDCOUNT = LVM_FIRST + 50;

		internal const int HDM_FIRST = 0x1200;//Header messages
		internal const int TCM_FIRST = 0x1300;//Tab control messages
		internal const int TVM_SORTCHILDREN = TV_FIRST + 19;
		internal const int TCM_SETCURFOCUS = (TCM_FIRST + 48);
		internal const int TCM_GETCURSEL = (TCM_FIRST + 11);

		internal const int HDM_GETITEMCOUNT = HDM_FIRST;

		internal const int TCS_SCROLLOPPOSITE = 0x0001;// assumes multiline tab
		internal const int TCS_BOTTOM = 0x0002;
		internal const int TCS_RIGHT = 0x0002;
		internal const int TCS_MULTISELECT = 0x0004;// allow multi-select in button mode
		internal const int TCS_FLATBUTTONS = 0x0008;
		internal const int TCS_FORCEICONLEFT = 0x0010;
		internal const int TCS_FORCELABELLEFT = 0x0020;
		internal const int TCS_HOTTRACK = 0x0040;
		internal const int TCS_VERTICAL = 0x0080;
		internal const int TCS_TABS = 0x0000;
		internal const int TCS_BUTTONS = 0x0100;
		internal const int TCS_SINGLELINE = 0x0000;
		internal const int TCS_MULTILINE = 0x0200;
		internal const int TCS_RIGHTJUSTIFY = 0x0000;
		internal const int TCS_FIXEDWIDTH = 0x0400;
		internal const int TCS_RAGGEDRIGHT = 0x0800;
		internal const int TCS_FOCUSONBUTTONDOWN = 0x1000;
		internal const int TCS_OWNERDRAWFIXED = 0x2000;
		internal const int TCS_TOOLTIPS = 0x4000;
		internal const int TCS_FOCUSNEVER = 0x8000;

		internal const uint LVM_GETCOLUMN = LVM_FIRST + 95;
		internal const uint LVM_SETCOLUMN = LVM_FIRST + 96;
		internal const uint LVCF_FMT = 0x1;
		internal const uint LVCF_IMAGE = 0x10;
		internal const int LVCFMT_IMAGE = 0x800;
		internal const int LVCFMT_BITMAP_ON_RIGHT = 0x1000;

		internal const int NM_FIRST = 0;
		internal const int NM_CLICK = NM_FIRST - 2;
		internal const int WM_REFLECT = 0x2000;
		internal const int WM_NOTIFY = 0x004e;

		internal const int WINDOW_TEXT_SIZE = 32767;

		internal const int ALTERNATE = 1;
		internal const int WINDING = 2;

		internal const int UNCHECKED = 1048576;
		internal const int CHECKED = 1048592;
		internal const int UNCHECKED_FOCUSED = 1048580; // if control is focused
		internal const int CHECKED_FOCUSED = 1048596; // if control is focused

		internal const int CF_TEXT = 1;
		internal const int CF_BITMAP = 2;
		internal const int CF_METAFILEPICT = 3;
		internal const int CF_SYLK = 4;
		internal const int CF_DIF = 5;
		internal const int CF_TIFF = 6;
		internal const int CF_OEMTEXT = 7;
		internal const int CF_DIB = 8;
		internal const int CF_PALETTE = 9;
		internal const int CF_PENDATA = 10;
		internal const int CF_RIFF = 11;
		internal const int CF_WAVE = 12;
		internal const int CF_UNICODETEXT = 13;
		internal const int CF_ENHMETAFILE = 14;
		internal const int CF_HDROP = 15;
		internal const int CF_LOCALE = 16;
		internal const int CF_DIBV5 = 17;
		internal const int CF_OWNERDISPLAY = 0x0080;
		internal const int CF_DSPTEXT = 0x0081;
		internal const int CF_DSPBITMAP = 0x0082;
		internal const int CF_DSPMETAFILEPICT = 0x0083;
		internal const int CF_DSPENHMETAFILE = 0x008E;
		internal const int CF_PRIVATEFIRST = 0x0200;
		internal const int CF_PRIVATELAST = 0x02FF;
		internal const int CF_GDIOBJFIRST = 0x0300;
		internal const int CF_GDIOBJLAST = 0x03FF;

		internal const int HC_ACTION = 0;
		internal const int HC_GETNEXT = 1;
		internal const int HC_SKIP = 2;
		internal const int HC_NOREMOVE = 3;
		internal const int HC_NOREM = HC_NOREMOVE;
		internal const int HC_SYSMODALON = 4;
		internal const int HC_SYSMODALOFF = 5;

		//WM_KEYUP/DOWN/CHAR HIWORD(lParam) flags
		public const int KF_ALTDOWN = 0x2000;

		public const int KF_DLGMODE = 0x0800;
		public const int KF_EXTENDED = 0x0100;
		public const int KF_MENUMODE = 0x1000;
		public const int KF_REPEAT = 0x4000;
		public const int KF_UP = 0x8000;
		public const int LLKHF_ALTDOWN = (KF_ALTDOWN >> 8);

		//Low level hook flags
		public const uint LLKHF_EXTENDED = (KF_EXTENDED >> 8);

		public const uint LLKHF_INJECTED = 0x00000010;
		public const uint LLKHF_LOWER_IL_INJECTED = 0x00000002;
		public const uint LLKHF_UP = (KF_UP >> 8);//0x00000020
		public const uint LLMHF_INJECTED = 0x00000001;//0x00000080
		public const uint LLMHF_LOWER_IL_INJECTED = 0x00000002;

		public const int STILL_ACTIVE = 0x00000103;
		public const int THREAD_BASE_PRIORITY_LOWRT = 15;  // value that gets a thread to LowRealtime-1
		public const int THREAD_BASE_PRIORITY_MAX = 2;   // maximum thread base priority boost
		public const int THREAD_BASE_PRIORITY_MIN = (-2);  // minimum thread base priority boost
		public const int THREAD_BASE_PRIORITY_IDLE = (-15); // value that gets a thread to idle

		public const int THREAD_PRIORITY_LOWEST = THREAD_BASE_PRIORITY_MIN;
		public const int THREAD_PRIORITY_BELOW_NORMAL = (THREAD_PRIORITY_LOWEST + 1);
		public const int THREAD_PRIORITY_NORMAL = 0;
		public const int THREAD_PRIORITY_HIGHEST = THREAD_BASE_PRIORITY_MAX;
		public const int THREAD_PRIORITY_ABOVE_NORMAL = (THREAD_PRIORITY_HIGHEST - 1);
		public const int THREAD_PRIORITY_ERROR_RETURN = (0x7fffffff);
		public const int THREAD_PRIORITY_TIME_CRITICAL = THREAD_BASE_PRIORITY_LOWRT;
		public const int THREAD_PRIORITY_IDLE = THREAD_BASE_PRIORITY_IDLE;
		public const int THREAD_MODE_BACKGROUND_BEGIN = 0x00010000;
		public const int THREAD_MODE_BACKGROUND_END = 0x00020000;
		public const int CT_CTYPE1 = 0x00000001;  // ctype 1 information
		public const int CT_CTYPE2 = 0x00000002;  // ctype 2 information
		public const int CT_CTYPE3 = 0x00000004;  // ctype 3 information
		public const int C3_NONSPACING = 0x0001;// nonspacing character
		public const int C3_DIACRITIC = 0x0002;// diacritic mark
		public const int C3_VOWELMARK = 0x0004;// vowel mark
		public const int C3_SYMBOL = 0x0008;// symbols
		public const int C3_KATAKANA = 0x0010;// katakana character
		public const int C3_HIRAGANA = 0x0020;// hiragana character
		public const int C3_HALFWIDTH = 0x0040;// half width character
		public const int C3_FULLWIDTH = 0x0080;// full width character
		public const int C3_IDEOGRAPH = 0x0100;// ideographic character
		public const int C3_KASHIDA = 0x0200;// Arabic kashida character
		public const int C3_LEXICAL = 0x0400;// lexical character
		public const int C3_HIGHSURROGATE = 0x0800;// high surrogate code unit
		public const int C3_LOWSURROGATE = 0x1000;// low surrogate code unit
		public const int C3_ALPHA = 0x8000;// any linguistic char (C1_ALPHA)
		public const int C3_NOTAPPLICABLE = 0x0000;// ctype 3 is not applicable

		public const int WH_MIN = -1;
		public const int WH_MSGFILTER = -1;
		public const int WH_JOURNALRECORD = 0;
		public const int WH_JOURNALPLAYBACK = 1;
		public const int WH_KEYBOARD = 2;
		public const int WH_GETMESSAGE = 3;
		public const int WH_CALLWNDPROC = 4;
		public const int WH_CBT = 5;
		public const int WH_SYSMSGFILTER = 6;
		public const int WH_MOUSE = 7;

		public const int WH_HARDWARE = 8;
		public const int WH_DEBUG = 9;
		public const int WH_SHELL = 10;
		public const int WH_FOREGROUNDIDLE = 11;
		public const int WH_CALLWNDPROCRET = 12;
		public const int WH_KEYBOARD_LL = 13;
		public const int WH_MOUSE_LL = 14;
		public const int WH_MAX = 14;
		public const int WH_MINHOOK = WH_MIN;
		public const int WH_MAXHOOK = WH_MAX;
		public const int JOYERR_BASE = 160;
		public const int JOYERR_NOERROR = 0;
		public const int JOYERR_PARMS = JOYERR_BASE + 5;
		public const int JOYERR_NOCANDO = JOYERR_BASE + 6;
		public const int JOYERR_UNPLUGGED = JOYERR_BASE + 7;

		public const int JOYCAPS_HASZ = 0x0001;
		public const int JOYCAPS_HASR = 0x0002;
		public const int JOYCAPS_HASU = 0x0004;
		public const int JOYCAPS_HASV = 0x0008;
		public const int JOYCAPS_HASPOV = 0x0010;
		public const int JOYCAPS_POV4DIR = 0x0020;
		public const int JOYCAPS_POVCTS = 0x0040;

		public const int JOY_POVCENTERED = -1;
		public const int JOY_POVFORWARD = 0;
		public const int JOY_POVRIGHT = 9000;
		public const int JOY_POVBACKWARD = 18000;
		public const int JOY_POVLEFT = 27000;

		public const int JOY_RETURNX = 0x00000001;
		public const int JOY_RETURNY = 0x00000002;
		public const int JOY_RETURNZ = 0x00000004;
		public const int JOY_RETURNR = 0x00000008;
		public const int JOY_RETURNU = 0x00000010;     /* axis 5 */
		public const int JOY_RETURNV = 0x00000020;     /* axis 6 */
		public const int JOY_RETURNPOV = 0x00000040;
		public const int JOY_RETURNBUTTONS = 0x00000080;
		public const int JOY_RETURNRAWDATA = 0x00000100;
		public const int JOY_RETURNPOVCTS = 0x00000200;
		public const int JOY_RETURNCENTERED = 0x00000400;
		public const int JOY_USEDEADZONE = 0x00000800;

		public const int JOY_RETURNALL = (JOY_RETURNX | JOY_RETURNY | JOY_RETURNZ |
										  JOY_RETURNR | JOY_RETURNU | JOY_RETURNV |
										  JOY_RETURNPOV | JOY_RETURNBUTTONS);

		public const long MB_SETFOREGROUND = 0x00010000L;

		public const uint MSG_OFFSET_MOUSE_MOVE = 0x80000000;
		public const uint GET_MODULE_HANDLE_EX_FLAG_PIN = 1;

		internal const long ERROR_ALREADY_EXISTS = 183L;
		internal const long ERROR_INVALID_HOOK_HANDLE = 1404L;

		internal const string dwmapi = "dwmapi.dll",
							  kernel32 = "kernel32.dll",
							  shell32 = "shell32.dll",
							  user32 = "user32.dll",
							  gdi32 = "gdi32.dll",
							  version = "version.dll",
							  winmm = "winmm.dll",
							  advapi = "advapi32.dll",
							  ole32 = "ole32.dll",
							  oleacc = "oleacc.dll",
							  oleaut = "oleaut32.dll";

		internal static Point ToPoint(this RECT rect) => new (rect.Left, rect.Top);

		internal static Map ToPos(this RECT rect, double scale = 1.0) => new (new Dictionary<object, object>()
		{
			{ "X", rect.Left * scale },
			{ "Y", rect.Top * scale },
			{ "Width", (rect.Right - rect.Left)* scale },
			{ "Height", (rect.Bottom - rect.Top)* scale },
		});

		[DllImport(oleacc, CharSet = CharSet.Unicode)]
		internal static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint id, ref Guid iid, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);

		//[DllImport(oleaut)]
		//public static extern int SafeArrayGetDim([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] System.Array arr);
		//public static extern int SafeArrayGetDim([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] IntPtr arr);

		/// <summary>
		/// Gotten from https://stackoverflow.com/questions/29392625/check-if-a-winform-checkbox-is-checked-through-winapi-only
		/// </summary>
		internal static bool IsChecked(IntPtr handle)
		{
			var guid = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
			object obj = null;
			var retValue = AccessibleObjectFromWindow(handle, (uint)OBJID.CLIENT, ref guid, ref obj);

			if (obj is IAccessible accObj)
			{
				var result = accObj.get_accState(0);

				if (result is int state)
					return state == CHECKED || state == CHECKED_FOCUSED;
			}

			return false;
		}

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool PeekMessage(out Msg message, IntPtr handle, uint filterMin, uint filterMax, uint flags);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool CloseClipboard();

		internal static bool OpenClipboard(long ms)
		{
			bool open;
			var dtStart = DateTime.Now;

			while (!(open = OpenClipboard(IntPtr.Zero)))
			{
				if (ms == -1)
					break;

				if (ms == 0)
					break;

				if ((DateTime.Now - dtStart).TotalMilliseconds > ms)
					break;

				_ = Flow.Sleep(100);
			}

			return open;
		}

		internal static IntPtr GetClipboardData(int format, ref bool nullIsOkay)
		{
			nullIsOkay = false;
			var formatName = "";

			if (format < 0xC000 || format > 0xFFFF) // It's a registered format (you're supposed to verify in-range before calling GetClipboardFormatName()).  Also helps performance.
			{
			}
			else
			{
				var fmt = DataFormats.GetFormat(format);

				if (fmt != null)
					formatName = fmt.Name;

				if (formatName.StartsWith("Link Source", StringComparison.OrdinalIgnoreCase)
						|| formatName.StartsWith("ObjectLink", StringComparison.OrdinalIgnoreCase)
						|| formatName.StartsWith("OwnerLink", StringComparison.OrdinalIgnoreCase)
						|| formatName.StartsWith("Native", StringComparison.OrdinalIgnoreCase)
						|| formatName.StartsWith("Embed Source", StringComparison.OrdinalIgnoreCase))
					return IntPtr.Zero;

				if (formatName.StartsWith("MSDEVColumnSelect", StringComparison.OrdinalIgnoreCase)
						|| formatName.StartsWith("MSDEVLineSelect", StringComparison.OrdinalIgnoreCase))
				{
					nullIsOkay = true;
					return IntPtr.Zero;
				}
			}

			return GetClipboardData((uint)format);
		}

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GlobalSize(IntPtr handle);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GlobalLock(IntPtr hMem);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool GlobalUnlock(IntPtr hMem);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetClipboardData(uint uFormat);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

		[DllImport(dwmapi, CharSet = CharSet.Unicode)]
		internal static extern uint DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbsize);

		internal static bool IsWindowCloaked(IntPtr hwnd)
		{
			var cloaked = 0;
			return DwmGetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, ref cloaked, 4) >= 0 && cloaked >= 0;
		}

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetLastActivePopup(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetShellWindow();

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool BlockInput(bool fBlockIt);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref EventMsg lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref MSDLLHOOKSTRUCT lParam);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool CloseHandle(IntPtr hObject);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int CloseWindow(IntPtr hWnd);

		[DllImport(kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr CreateFile(string fileName,
				uint desiredAccess,
				uint shareMode,
				IntPtr attributes,
				uint creationDisposition,
				uint flagsAndAttributes,
				IntPtr templateFile);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool DestroyWindow(IntPtr hwnd);

		[DllImport(kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool DeviceIoControl(
			IntPtr hDevice,
			uint dwIoControlCode,
			ref long InBuffer,
			int nInBufferSize,
			ref long OutBuffer,
			int nOutBufferSize,
			ref int pBytesReturned,
			[In] ref NativeOverlapped lpOverlapped);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool DestroyIcon(IntPtr handle);

		//[DllImport(kernel32, SetLastError = true)]
		//internal static extern bool DeviceIoControl(IntPtr driveHandle,
		//      uint IoControlCode,
		//      IntPtr lpInBuffer,
		//      uint inBufferSize,
		//      IntPtr lpOutBuffer,
		//      uint outBufferSize,
		//      out uint lpBytesReturned,
		//      IntPtr lpOverlapped);
		[DllImport(winmm, CharSet = CharSet.Unicode)]
		internal static extern int mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

		[DllImport(winmm, CharSet = CharSet.Unicode)]
		internal static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji);

		[DllImport(winmm, CharSet = CharSet.Unicode)]
		internal static extern int joyGetDevCaps(IntPtr id, ref JOYCAPS lpCaps, uint uSize);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool SetProcessDPIAware();

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool IsHungAppWindow(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool EnumChildWindows(IntPtr hwndParent, _EnumWindowsProc lpEnumFunc, object lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int EnumWindows(_EnumWindowsProc lpEnumFunc, int lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

		[DllImport(shell32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		[PublicForTestOnly]
		public static extern IntPtr FindWindow(string className, string windowName);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool FreeLibrary(IntPtr hModule);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern uint GetCurrentThreadId();

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		internal static (bool, uint) AttachThreadInput(IntPtr targetWindow, bool setActive)
		{
			var threadsAreAttached = false;
			var targetThread = GetWindowThreadProcessId(targetWindow, out _);

			if (targetThread != 0 && targetThread != Processes.CurrentThreadID && !IsHungAppWindow(targetWindow))
				threadsAreAttached = AttachThreadInput(Processes.CurrentThreadID, targetThread, true);

			if (setActive)
				_ = SetActiveWindow(targetWindow);

			return (threadsAreAttached, targetThread);
		}

		internal static void DetachThreadInput(bool threadsAreAttached, uint targetThread)
		{
			if (threadsAreAttached)
				_ = AttachThreadInput(Processes.CurrentThreadID, targetThread, false);
		}

		internal static IntPtr AllocInterProcMem(uint size, IntPtr hwnd, ProcessAccessTypes extraAccess, out IntPtr handle)
		// aHandle is an output parameter that receives the process handle.
		// Returns NULL on failure (in which case caller should ignore the value of aHandle).
		{
			var mem = IntPtr.Zero;
			_ = GetWindowThreadProcessId(hwnd, out var pid);

			// Even if the PID is our own, open the process anyway to simplify the code. After all, it would be
			// pretty silly for a script to access its own ListViews via this method.
			if ((handle = OpenProcess(ProcessAccessTypes.PROCESS_VM_OPERATION | ProcessAccessTypes.PROCESS_VM_READ | ProcessAccessTypes.PROCESS_VM_WRITE | extraAccess, false, pid.ToInt32())) == IntPtr.Zero)
				return mem;

			// Reason for using VirtualAllocEx(): When sending LVITEM structures to a control in a remote process, the
			// structure and its pszText buffer must both be memory inside the remote process rather than in our own.
			mem = VirtualAllocEx(handle, IntPtr.Zero, size, VirtualAllocExTypes.MEM_RESERVE | VirtualAllocExTypes.MEM_COMMIT, AccessProtectionFlags.PAGE_READWRITE);

			//
			if (mem == IntPtr.Zero)
				_ = CloseHandle(handle); // Caller should ignore the value of aHandle when return value is NULL.

			//else leave the handle open.  It's the caller's responsibility to close it.
			return mem;
		}

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetActiveWindow(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetActiveWindow();

		[DllImport(user32, CharSet = CharSet.Unicode, ExactSpelling = true)]
		internal static extern IntPtr GetAncestor(IntPtr hWnd, gaFlags gaFlags);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		internal static string GetClassName(IntPtr hwnd)
		{
			var buf = new StringBuilder(64);
			_ = GetClassName(hwnd, buf, buf.Capacity);
			return buf.ToString();
		}

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool GetCursorPos(out Point lpPoint);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		[PublicForTestOnly]
		public static extern IntPtr GetDesktopWindow();

		[DllImport(version, CharSet = CharSet.Unicode)]
		internal static extern bool GetFileVersionInfo(string sFileName, int handle, int size, byte[] infoBuffer);

		[DllImport(version, CharSet = CharSet.Unicode)]
		internal static extern int GetFileVersionInfoSize(string sFileName, out int handle);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetFocus();

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetParent(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetForegroundWindow();

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool GetGUIThreadInfo(uint idThread, out GUITHREADINFO lpgui);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int ActivateKeyboardLayout(IntPtr hkl, uint Flags);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetKeyboardLayout(uint idThread);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool GetKeyboardLayoutName([Out] StringBuilder pwszKLID);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool GetKeyboardState(byte[] lpKeyState);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool SetKeyboardState(byte[] lpKeyState);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern short GetKeyState(int nVirtKey);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern short GetAsyncKeyState(int nVirtKey);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetMenu(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool revert);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int GetMenuItemCount(IntPtr hMenu);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int GetMenuString(IntPtr hMenu, uint uIDItem, [Out] StringBuilder lpString, int nMaxCount, uint uFlag);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern unsafe bool IsDialogMessage(IntPtr hDlg, Msg lpMsg);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int GetMessage(out Msg lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetMessageExtraInfo();

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern int GetModuleHandleEx(uint dwFlags, string lpModuleName, out IntPtr phModule);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern uint GetPrivateProfileSection(string lpAppName, StringBuilder lpszReturnBuffer, uint nSize, string lpFileName);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern uint GetPrivateProfileSectionNames(StringBuilder lpszReturnBuffer, uint nSize, string lpFileName);

		/// <summary>
		/// Set this to use ExactSpelling since GetProcAddress() only is available in ANSI.
		/// </summary>
		/// <param name="hModule"></param>
		/// <param name="procName"></param>
		/// <returns></returns>
		[DllImport(kernel32, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern uint GetMenuItemID(IntPtr hMenu, int nPos);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern void SetLastError(uint dwErrCode);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern uint GetLastError();

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int GetDlgCtrlID(IntPtr hwndCtl);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool GetLayeredWindowAttributes(IntPtr hwnd, out uint crKey, out byte bAlpha, out uint dwFlags);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		[PublicForTestOnly]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In, Out] ref RECT rect, [MarshalAs(UnmanagedType.U4)] int cPoints);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		//void CoordToScreen(POINT &aPoint, int aWhichMode)
		//// For convenience. See function above for comments.
		//{
		//  CoordToScreen((int &)aPoint.x, (int &)aPoint.y, aWhichMode);
		//}

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

		/// <summary>
		/// See http://msdn.microsoft.com/en-us/library/ms632599%28VS.85%29.aspx#message_only
		/// </summary>
		/// <param name="hwnd"></param>
		/// <returns></returns>
		[DllImport(user32, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool AddClipboardFormatListener(IntPtr hwnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

		internal static string GetWindowText(IntPtr hwnd)
		{
            int textLength = WindowsAPI.GetWindowTextLength(hwnd);
            if (textLength == 0)
                return string.Empty;
            StringBuilder outText = new StringBuilder(textLength + 1);
            int a = GetWindowText(hwnd, outText, outText.Capacity);
            return outText.ToString();
        }

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool IsWindow(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsWindowEnabled(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool SetDllDirectory(string lpPathName);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		//[DllImport(kernel32, CharSet = CharSet.Unicode)]
		//internal static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern void OutputDebugString(string lpOutputString);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern void PostQuitMessage(int nExitCode);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool PostThreadMessage(uint threadId, uint msg, UIntPtr wParam, IntPtr lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool PostMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool PostMessage(IntPtr hWnd, uint msg, string wParam, IntPtr lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr RealChildWindowFromPoint(IntPtr hwndParent, Point ptParentClientCoords);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern uint SendMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, int[] lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr SendLVColMessage(IntPtr hWnd, uint msg, uint wParam, ref LV_COLUMN lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool IsIconic(IntPtr Hwnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool IsZoomed(IntPtr hWnd);

		[DllImport(shell32, CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int SHGetKnownFolderPath(
			[MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
			uint dwFlags,
			IntPtr hToken,
			out IntPtr pszPath);

		/// <summary>
		/// Gotten from https://www.codeproject.com/Questions/1224984/How-to-get-quick-access-folder-path-in-windows-usi
		/// </summary>
		/// <param name="rfid"></param>
		/// <returns></returns>
		internal static string SHGetKnownFolderPath(Guid guid)
		{
			var pPath = IntPtr.Zero;
			string path = null;

			try
			{
				var hr = SHGetKnownFolderPath(guid, (uint)KnownFolderFlag.None, IntPtr.Zero, out pPath);

				if (hr == 0)
				{
					//throw Marshal.GetExceptionForHR(hr);
					path = Marshal.PtrToStringUni(pPath);
				}
			}
			finally
			{
				if (pPath != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(pPath);
					pPath = IntPtr.Zero;
				}
			}

			return path;
		}

		/// <summary>
		/// Gotten from https://gist.github.com/BoyCook/5075907
		/// </summary>
		internal struct COPYDATASTRUCT
		{
			public IntPtr dwData;
			public int cbData;

			[MarshalAs(UnmanagedType.LPStr)]
			public string lpData;
		}

		//For use with WM_COPYDATA and COPYDATASTRUCT
		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int SendMessageTimeout(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam,
				SendMessageTimeoutFlags flags,
				uint timeout,
				out IntPtr result);

		//For use with WM_COPYDATA and COPYDATASTRUCT
		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

		[DllImport(user32, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int SendMessageTimeout(
			IntPtr hWnd,
			uint Msg,
			object wParam,
			string lParam,
			SendMessageTimeoutFlags flags,
			uint timeout,
			out IntPtr result);

		[DllImport(user32, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int SendMessageTimeout(
			IntPtr hWnd,
			uint Msg,
			int wParam,
			string lParam,
			SendMessageTimeoutFlags flags,
			uint timeout,
			out IntPtr result);

		[DllImport(user32, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int SendMessageTimeout(
			IntPtr hWnd,
			uint Msg,
			ref int wParam,
			ref int lParam,
			SendMessageTimeoutFlags flags,
			uint timeout,
			out IntPtr result);

		[DllImport(user32, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int SendMessageTimeout(
			IntPtr hWnd,
			uint Msg,
			int wParam,
			int lParam,
			SendMessageTimeoutFlags flags,
			uint timeout,
			out IntPtr result);

		[DllImport(user32, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int SendMessageTimeout(
			IntPtr hWnd,
			uint Msg,
			uint wParam,
			IntPtr lParam,
			SendMessageTimeoutFlags flags,
			uint timeout,
			out IntPtr result);

		[DllImport(user32, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int SendMessageTimeout(
			IntPtr hWnd,
			uint Msg,
			IntPtr wParam,
			IntPtr lParam,
			SendMessageTimeoutFlags flags,
			uint timeout,
			out IntPtr result);

		[DllImport(user32, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int SendMessageTimeout(
			IntPtr hWnd,
			uint Msg,
			uint wParam,
			uint lParam,
			SendMessageTimeoutFlags flags,
			uint timeout,
			out IntPtr result);

		[DllImport(user32, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int SendMessageTimeout(
			IntPtr hWnd,
			uint Msg,
			uint wParam,
			StringBuilder lParam,
			SendMessageTimeoutFlags flags,
			uint timeout,
			out IntPtr result);

		[DllImport(user32, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int SendMessageTimeout(
			IntPtr hWnd,
			uint Msg,
			uint wParam,
			string lParam,
			SendMessageTimeoutFlags flags,
			uint timeout,
			out IntPtr result);

		[DllImport(advapi, CharSet = CharSet.Unicode)]
		internal static extern int RegQueryInfoKey(
			SafeRegistryHandle hKey,
			[Out()] StringBuilder lpClass,
			ref uint lpcchClass,
			IntPtr lpReserved,
			IntPtr lpcSubKeys,
			IntPtr lpcbMaxSubKeyLen,
			IntPtr lpcbMaxClassLen,
			IntPtr lpcValues,
			IntPtr lpcbMaxValueNameLen,
			IntPtr lpcbMaxValueLen,
			IntPtr lpcbSecurityDescriptor,
			out long lpftLastWriteTime);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern int GetShortPathName(string longPath, StringBuilder shortPath, int bufSize);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int SetActiveWindow(IntPtr handle);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr SetFocus(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		[PublicForTestOnly]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

		//[DllImport(user32, CharSet = CharSet.Unicode)]
		//internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr SetWindowsHookEx(int idHook, PlaybackProc lpfn, IntPtr hMod, uint dwThreadId);

		//[DllImport(user32, CharSet = CharSet.Unicode)]
		//internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, int wParam, [In] KBDLLHOOKSTRUCT lParam);
		//[DllImport(user32, CharSet = CharSet.Unicode)]
		//internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, int wParam, [In] MSDLLHOOKSTRUCT lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool SetWindowText(IntPtr hWnd, string lpString);

		[DllImport(shell32)]
		internal static extern int SHEmptyRecycleBin(IntPtr hWnd, string pszRootPath, uint dwFlags);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

		internal static int ToUnicodeOrAsciiEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, uint wFlags, IntPtr dwhkl) =>
		ToUnicodeEx(wVirtKey, wScanCode, lpKeyState, pwszBuff, pwszBuff.Capacity, wFlags, dwhkl);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport(version, CharSet = CharSet.Unicode)]
		internal static extern bool VerQueryValue(byte[] pBlock, string pSubBlock, out string pValue, out uint len);

		[DllImport(winmm, CharSet = CharSet.Unicode)]
		internal static extern uint waveOutGetVolume(IntPtr hwo, out uint dwVolume);

		[DllImport(winmm, CharSet = CharSet.Unicode)]
		internal static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr WindowFromPoint(Point Point);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int SendMessage(IntPtr hwnd, uint msg, int wParam, StringBuilder sb);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool RegisterHotKey(IntPtr hWnd, uint id, KeyModifiers fsModifiers, uint vk);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool UnregisterHotKey(IntPtr hWnd, uint id);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern bool IsCharAlphaNumeric(char ch);

		/// <summary>
		/// Returns the first ancestor of aWnd that isn't itself a child.  aWnd itself is returned if
		/// it is not a child.  Returns NULL only if aWnd is NULL.  Also, it should always succeed
		/// based on the axiom that any window with the WS_CHILD style (aka WS_CHILDWINDOW) must have
		/// a non-child ancestor somewhere up the line.
		/// This function doesn't do anything special with owned vs. unowned windows.  Despite what MSDN
		/// says, GetParent() does not return the owner window, at least in some cases on Windows XP
		/// (e.g. BulletProof FTP Server). It returns NULL instead. In any case, it seems best not to
		/// worry about owner windows for this function's caller (MouseGetPos()), since it might be
		/// desirable for that command to return the owner window even though it can't actually be
		/// activated.  This is because attempts to activate an owner window should automatically cause
		/// the OS to activate the topmost owned window instead.  In addition, the owner window may
		/// contain the actual title or text that the user is interested in.  UPDATE: Due to the fact
		/// that this function retrieves the first parent that's not a child window, it's likely that
		/// that window isn't its owner anyway (since the owner problem usually applies to a parent
		/// window being owned by some controlling window behind it).
		/// </summary>
		/// <param name="hwnd"></param>
		/// <returns></returns>
		internal static IntPtr GetNonChildParent(IntPtr hwnd)
		{
			if (hwnd == IntPtr.Zero) return hwnd;

			IntPtr parent, parent_prev;

			for (parent_prev = hwnd; ; parent_prev = parent)
			{
				if ((GetWindowLongPtr(parent_prev, GWL_STYLE).ToInt64() & WS_CHILD) == 0)  // Found the first non-child parent, so return it.
					return parent_prev;

				parent = GetAncestor(parent_prev, gaFlags.GA_PARENT);

				if (parent == IntPtr.Zero)
					return parent_prev;  // This will return aWnd if aWnd has no parents.
			}
		}

		internal static string GetWindowTextTimeout(IntPtr hwnd, uint timeout)
		{
			_ = SendMessageTimeout(hwnd, WM_GETTEXTLENGTH, 0u, IntPtr.Zero, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, timeout, out var length);

			if (length == IntPtr.Zero)
				return null;

			var val = length.ToInt32();

			if (val == 0)
			{
				return null;
			}

			var sb = new StringBuilder(val + 1);  // leave room for null-terminator
			var ptr = IntPtr.Zero;

			if (SendMessageTimeout(hwnd, WM_GETTEXT, (uint)sb.Capacity, sb, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, timeout, out ptr) == 0)
				return null;

			return sb.ToString();
		}

		internal static bool ControlSetTab(IntPtr hwnd, int tabIndex)
		{
			// MSDN: "If the tab control does not have the TCS_BUTTONS style, changing the focus also changes
			// the selected tab. In this case, the tab control sends the TCN_SELCHANGING and TCN_SELCHANGE
			// notification codes to its parent window."
			if (SendMessageTimeout(hwnd, TCM_SETCURFOCUS, (uint)tabIndex, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result) == 0)
				return false;

			// Tab controls with the TCS_BUTTONS style need additional work:
			if ((GetWindowLongPtr(hwnd, GWL_STYLE).ToInt64() & TCS_BUTTONS) != 0)
			{
				// Problem:
				//  TCM_SETCURFOCUS does not change the selected tab if TCS_BUTTONS is set.
				//
				// False solution #1 (which used to be recommended in the docs):
				//  Send a TCM_SETCURSEL method afterward.  TCM_SETCURSEL changes the selected tab,
				//  but doesn't notify the control's parent, so it doesn't update the tab's contents.
				//
				// False solution #2:
				//  Send a WM_NOTIFY message to the parent window to notify it.  Can't be done.
				//  MSDN says: "For Windows 2000 and later systems, the WM_NOTIFY message cannot
				//  be sent between processes."
				//
				// Solution #1:
				//  Send VK_LEFT/VK_RIGHT as many times as needed.
				//
				// Solution #2:
				//  Set the focus to an adjacent tab and then send VK_LEFT/VK_RIGHT.
				//   - Must choose an appropriate tab index and vk depending on which tab is being
				//     selected, since VK_LEFT/VK_RIGHT don't wrap around.
				//   - Ends up tempting optimisations which increase code size, such as to avoid
				//     TCM_SETCURFOCUS if an adjacent tab is already focused.
				//   - Still needs VK_SPACE afterward to actually select the tab.
				//
				// Solution #3 (the one below):
				//  Set the focus to the appropriate tab and then send VK_SPACE.
				//   - Since we've already set the focus, all we need to do is send VK_SPACE.
				//   - If the tab index is invalid and the user has focused but not selected
				//     another tab, that tab will be selected.  This seems harmless enough.
				//
				_ = PostMessage(hwnd, WM_KEYDOWN, VirtualKeys.VK_SPACE, 0x00000001);
				_ = PostMessage(hwnd, WM_KEYUP, VirtualKeys.VK_SPACE, 0xC0000001);
			}

			return true;
		}

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool GetStringTypeEx(uint Locale, uint dwInfoType, string lpSrcStr, int cchSrc, [Out] ushort[] lpCharType);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool SetThreadPriority(IntPtr hThread, int priority);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr OpenProcess(ProcessAccessTypes desiredAccess, bool inheritHandle, int processId);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr address, uint size, VirtualAllocExTypes allocationType, AccessProtectionFlags flags);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr address, uint size, VirtualAllocExTypes dwFreeType);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr baseAddress, byte[] buffer, uint dwSize, out uint numberOfBytesRead);

		[DllImport(kernel32, CharSet = CharSet.Unicode)]
		internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

		[DllImport(gdi32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr CreateEllipticRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

		[DllImport(gdi32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

		[DllImport(gdi32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

		[DllImport(gdi32, CharSet = CharSet.Unicode)]
		internal static extern IntPtr CreatePolygonRgn(POINT[] lppt, int cPoints, int fnPolyFillMode);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

		[DllImport(gdi32, CharSet = CharSet.Unicode)]
		internal static extern bool DeleteObject(IntPtr hObject);

		internal delegate bool _EnumWindowsProc(IntPtr hwnd, int lParam);//Add an underscore to this name because some sample programs use EnumWindowsProc as a function name.

		internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

		internal delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, ref MSDLLHOOKSTRUCT lParam);

		internal delegate IntPtr PlaybackProc(int nCode, IntPtr wParam, ref EventMsg lParam);

		[DllImport(user32, CharSet = CharSet.Unicode)]
		internal static extern int GetSystemMetrics(SystemMetric smIndex);
	}
}
#endif