#if LINUX
#pragma warning disable 649
namespace Keysharp.Core.Linux.X11
{
	internal enum XWindowClass
	{
		InputOutput = 1,
		InputOnly = 2
	}

	internal enum XRequest : byte
	{
		X_CreateWindow = 1,
		X_ChangeWindowAttributes = 2,
		X_GetWindowAttributes = 3,
		X_DestroyWindow = 4,
		X_DestroySubwindows = 5,
		X_ChangeSaveSet = 6,
		X_ReparentWindow = 7,
		X_MapWindow = 8,
		X_MapSubwindows = 9,
		X_UnmapWindow = 10,
		X_UnmapSubwindows = 11,
		X_ConfigureWindow = 12,
		X_CirculateWindow = 13,
		X_GetGeometry = 14,
		X_QueryTree = 15,
		X_InternAtom = 16,
		X_GetAtomName = 17,
		X_ChangeProperty = 18,
		X_DeleteProperty = 19,
		X_GetProperty = 20,
		X_ListProperties = 21,
		X_SetSelectionOwner = 22,
		X_GetSelectionOwner = 23,
		X_ConvertSelection = 24,
		X_SendEvent = 25,
		X_GrabPointer = 26,
		X_UngrabPointer = 27,
		X_GrabButton = 28,
		X_UngrabButton = 29,
		X_ChangeActivePointerGrab = 30,
		X_GrabKeyboard = 31,
		X_UngrabKeyboard = 32,
		X_GrabKey = 33,
		X_UngrabKey = 34,
		X_AllowEvents = 35,
		X_GrabServer = 36,
		X_UngrabServer = 37,
		X_QueryPointer = 38,
		X_GetMotionEvents = 39,
		X_TranslateCoords = 40,
		X_WarpPointer = 41,
		X_SetInputFocus = 42,
		X_GetInputFocus = 43,
		X_QueryKeymap = 44,
		X_OpenFont = 45,
		X_CloseFont = 46,
		X_QueryFont = 47,
		X_QueryTextExtents = 48,
		X_ListFonts = 49,
		X_ListFontsWithInfo = 50,
		X_SetFontPath = 51,
		X_GetFontPath = 52,
		X_CreatePixmap = 53,
		X_FreePixmap = 54,
		X_CreateGC = 55,
		X_ChangeGC = 56,
		X_CopyGC = 57,
		X_SetDashes = 58,
		X_SetClipRectangles = 59,
		X_FreeGC = 60,
		X_ClearArea = 61,
		X_CopyArea = 62,
		X_CopyPlane = 63,
		X_PolyPoint = 64,
		X_PolyLine = 65,
		X_PolySegment = 66,
		X_PolyRectangle = 67,
		X_PolyArc = 68,
		X_FillPoly = 69,
		X_PolyFillRectangle = 70,
		X_PolyFillArc = 71,
		X_PutImage = 72,
		X_GetImage = 73,
		X_PolyText8 = 74,
		X_PolyText16 = 75,
		X_ImageText8 = 76,
		X_ImageText16 = 77,
		X_CreateColormap = 78,
		X_FreeColormap = 79,
		X_CopyColormapAndFree = 80,
		X_InstallColormap = 81,
		X_UninstallColormap = 82,
		X_ListInstalledColormaps = 83,
		X_AllocColor = 84,
		X_AllocNamedColor = 85,
		X_AllocColorCells = 86,
		X_AllocColorPlanes = 87,
		X_FreeColors = 88,
		X_StoreColors = 89,
		X_StoreNamedColor = 90,
		X_QueryColors = 91,
		X_LookupColor = 92,
		X_CreateCursor = 93,
		X_CreateGlyphCursor = 94,
		X_FreeCursor = 95,
		X_RecolorCursor = 96,
		X_QueryBestSize = 97,
		X_QueryExtension = 98,
		X_ListExtensions = 99,
		X_ChangeKeyboardMapping = 100,
		X_GetKeyboardMapping = 101,
		X_ChangeKeyboardControl = 102,
		X_GetKeyboardControl = 103,
		X_Bell = 104,
		X_ChangePointerControl = 105,
		X_GetPointerControl = 106,
		X_SetScreenSaver = 107,
		X_GetScreenSaver = 108,
		X_ChangeHosts = 109,
		X_ListHosts = 110,
		X_SetAccessControl = 111,
		X_SetCloseDownMode = 112,
		X_KillClient = 113,
		X_RotateProperties = 114,
		X_ForceScreenSaver = 115,
		X_SetPointerMapping = 116,
		X_GetPointerMapping = 117,
		X_SetModifierMapping = 118,
		X_GetModifierMapping = 119,
		X_NoOperation = 127
	}

	internal enum XKeySym : uint
	{
		XK_BackSpace = 0xFF08,
		XK_Tab = 0xFF09,
		XK_Clear = 0xFF0B,
		XK_Return = 0xFF0D,
		XK_Home = 0xFF50,
		XK_Left = 0xFF51,
		XK_Up = 0xFF52,
		XK_Right = 0xFF53,
		XK_Down = 0xFF54,
		XK_Page_Up = 0xFF55,
		XK_Page_Down = 0xFF56,
		XK_End = 0xFF57,
		XK_Begin = 0xFF58,
		XK_Menu = 0xFF67,
		XK_Shift_L = 0xFFE1,
		XK_Shift_R = 0xFFE2,
		XK_Control_L = 0xFFE3,
		XK_Control_R = 0xFFE4,
		XK_Caps_Lock = 0xFFE5,
		XK_Shift_Lock = 0xFFE6,
		XK_Meta_L = 0xFFE7,
		XK_Meta_R = 0xFFE8,
		XK_Alt_L = 0xFFE9,
		XK_Alt_R = 0xFFEA,
		XK_Super_L = 0xFFEB,
		XK_Super_R = 0xFFEC,
		XK_Hyper_L = 0xFFED,
		XK_Hyper_R = 0xFFEE,
	}

	[Flags]
	internal enum KeyMasks
	{
		ShiftMask = (1 << 0),
		LockMask = (1 << 1),
		ControlMask = (1 << 2),
		Mod1Mask = (1 << 3),
		Mod2Mask = (1 << 4),
		Mod3Mask = (1 << 5),
		Mod4Mask = (1 << 6),
		Mod5Mask = (1 << 7),

		ModMasks = Mod1Mask | Mod2Mask | Mod3Mask | Mod4Mask | Mod5Mask
	}

	internal enum XKeys
	{
		LeftControl = 37,
		RightControl = 105,
		LeftShift = 50,
		RightShift = 62,
		LeftAlt = 64,
		RightAlt = 108,
		LeftSuper = 133,
		RightSuper = 134,
		SpaceBar = 65,
		LowerLetter = 24,     // q on qwerty
		UpperLetter = 58,     // m
		BackSpace = 22,
		Return = 36,
		// Missing: "Menu key"

		F1 = 67,
		F2 = 68,
		F3 = 69,
		F4 = 70,
		F5 = 71,
		F6 = 72,
		F7 = 73,
		F8 = 74,
		F9 = 75,
		F10 = 76,

		// Missing: F11
		F12 = 96,

		Escape = 9,
		Tab = 23,
		CapsLock = 66,
		Tilde = 49,
		Backslash = 51,

		// Missing: PrintScrn
		ScrollLock = 78,

		Pause = 127,
		Insert = 118,
		Delete = 119,
		Home = 110,
		End = 115,
		PageUp = 112,
		PageDown = 117,
		NumLock = 77,

		Slash = 61,
		Dot = 60,
		Comma = 59,
		Quote = 48,
		Semicolon = 47,
		OpenSquareBracket = 34,
		CloseSquareBracket = 35,

		ExMark = 10,
		At = 11,
		Hash = 12,
		Dollar = 13,
		Percent = 14,
		Circumflex = 15,
		Ampersand = 16,
		Asterisk = 17,
		OpenParens = 18,
		CloseParens = 19,
		Dash = 20,
		Equals = 21,

		NumpadSlash = 106,
		NumpadAsterisk = 63,
		NumpadMinus = 82,
		NumpadPlus = 86,
		NumpadEnter = 104,
		NumpadDot = 91,

		Left = 113,
		Right = 114,
		Up = 111,
		Down = 116
	}

	internal enum XAtom : uint
	{
		AnyPropertyType = 0,
		XA_PRIMARY = 1,
		XA_SECONDARY = 2,
		XA_ARC = 3,
		XA_ATOM = 4,
		XA_BITMAP = 5,
		XA_CARDINAL = 6,
		XA_COLORMAP = 7,
		XA_CURSOR = 8,
		XA_CUT_BUFFER0 = 9,
		XA_CUT_BUFFER1 = 10,
		XA_CUT_BUFFER2 = 11,
		XA_CUT_BUFFER3 = 12,
		XA_CUT_BUFFER4 = 13,
		XA_CUT_BUFFER5 = 14,
		XA_CUT_BUFFER6 = 15,
		XA_CUT_BUFFER7 = 16,
		XA_DRAWABLE = 17,
		XA_FONT = 18,
		XA_INTEGER = 19,
		XA_PIXMAP = 20,
		XA_POINT = 21,
		XA_RECTANGLE = 22,
		XA_RESOURCE_MANAGER = 23,
		XA_RGB_COLOR_MAP = 24,
		XA_RGB_BEST_MAP = 25,
		XA_RGB_BLUE_MAP = 26,
		XA_RGB_DEFAULT_MAP = 27,
		XA_RGB_GRAY_MAP = 28,
		XA_RGB_GREEN_MAP = 29,
		XA_RGB_RED_MAP = 30,
		XA_STRING = 31,
		XA_VISUALID = 32,
		XA_WINDOW = 33,
		XA_WM_COMMAND = 34,
		XA_WM_HINTS = 35,
		XA_WM_CLIENT_MACHINE = 36,
		XA_WM_ICON_NAME = 37,
		XA_WM_ICON_SIZE = 38,
		XA_WM_NAME = 39,
		XA_WM_NORMAL_HINTS = 40,
		XA_WM_SIZE_HINTS = 41,
		XA_WM_ZOOM_HINTS = 42,
		XA_MIN_SPACE = 43,
		XA_NORM_SPACE = 44,
		XA_MAX_SPACE = 45,
		XA_END_SPACE = 46,
		XA_SUPERSCRIPT_X = 47,
		XA_SUPERSCRIPT_Y = 48,
		XA_SUBSCRIPT_X = 49,
		XA_SUBSCRIPT_Y = 50,
		XA_UNDERLINE_POSITION = 51,
		XA_UNDERLINE_THICKNESS = 52,
		XA_STRIKEOUT_ASCENT = 53,
		XA_STRIKEOUT_DESCENT = 54,
		XA_ITALIC_ANGLE = 55,
		XA_X_HEIGHT = 56,
		XA_QUAD_WIDTH = 57,
		XA_WEIGHT = 58,
		XA_POINT_SIZE = 59,
		XA_RESOLUTION = 60,
		XA_COPYRIGHT = 61,
		XA_NOTICE = 62,
		XA_FONT_NAME = 63,
		XA_FAMILY_NAME = 64,
		XA_FULL_NAME = 65,
		XA_CAP_HEIGHT = 66,
		XA_WM_CLASS = 67,
		XA_WM_TRANSIENT_FOR = 68,

		XA_LAST_PREDEFINED = 68
	}

	internal enum NA
	{
		WM_PROTOCOLS,
		WM_DELETE_WINDOW,
		WM_TAKE_FOCUS,

		_NET_SUPPORTED,
		_NET_CLIENT_LIST,
		_NET_NUMBER_OF_DESKTOPS,
		_NET_DESKTOP_GEOMETRY,
		_NET_DESKTOP_VIEWPORT,
		_NET_CURRENT_DESKTOP,
		_NET_DESKTOP_NAMES,
		_NET_ACTIVE_WINDOW,
		_NET_WORKAREA,
		_NET_SUPPORTING_WM_CHECK,
		_NET_VIRTUAL_ROOTS,
		_NET_DESKTOP_LAYOUT,
		_NET_SHOWING_DESKTOP,

		_NET_CLOSE_WINDOW,
		_NET_MOVERESIZE_WINDOW,
		_NET_WM_MOVERESIZE,
		_NET_RESTACK_WINDOW,
		_NET_REQUEST_FRAME_EXTENTS,

		_NET_WM_NAME,
		_NET_WM_VISIBLE_NAME,
		_NET_WM_ICON_NAME,
		_NET_WM_VISIBLE_ICON_NAME,
		_NET_WM_DESKTOP,
		_NET_WM_WINDOW_TYPE,
		_NET_WM_STATE,
		_NET_WM_ALLOWED_ACTIONS,
		_NET_WM_STRUT,
		_NET_WM_STRUT_PARTIAL,
		_NET_WM_ICON_GEOMETRY,
		_NET_WM_ICON,
		_NET_WM_PID,
		_NET_WM_HANDLED_ICONS,
		_NET_WM_USER_TIME,
		_NET_FRAME_EXTENTS,

		_NET_WM_PING,
		_NET_WM_SYNC_REQUEST,

		_NET_SYSTEM_TRAY_S,
		_NET_SYSTEM_TRAY_ORIENTATION,
		_NET_SYSTEM_TRAY_OPCODE,

		_NET_WM_STATE_MAXIMIZED_HORZ,
		_NET_WM_STATE_MAXIMIZED_VERT,

		_XEMBED,
		_XEMBED_INFO,

		_MOTIF_WM_HINTS,

		_NET_WM_STATE_NO_TASKBAR,
		_NET_WM_STATE_ABOVE,
		_NET_WM_STATE_MODAL,
		_NET_WM_STATE_HIDDEN,
		_NET_WM_CONTEXT_HELP,

		_NET_WM_WINDOW_OPACITY,

		_NET_WM_WINDOW_TYPE_DESKTOP,
		_NET_WM_WINDOW_TYPE_DOCK,
		_NET_WM_WINDOW_TYPE_TOOLBAR,
		_NET_WM_WINDOW_TYPE_MENU,
		_NET_WM_WINDOW_TYPE_UTILITY,
		_NET_WM_WINDOW_TYPE_SPLASH,
		_NET_WM_WINDOW_TYPE_DIALOG,
		_NET_WM_WINDOW_TYPE_NORMAL,

		CLIPBOARD,
		DIB,
		OEMTEXT,
		UNICODETEXT,
		TARGETS,

		LAST_NET_ATOM
	}

	internal enum StackMode
	{
		Above = 0,
		Below = 1,
		TopIf = 2,
		BottomIf = 3,
		Opposite = 4
	}

	[Flags]
	internal enum SetWindowValuemask
	{
		Nothing = 0,
		BackPixmap = 1,
		BackPixel = 2,
		BorderPixmap = 4,
		BorderPixel = 8,
		BitGravity = 16,
		WinGravity = 32,
		BackingStore = 64,
		BackingPlanes = 128,
		BackingPixel = 256,
		OverrideRedirect = 512,
		SaveUnder = 1024,
		EventMask = 2048,
		DontPropagate = 4096,
		ColorMap = 8192,
		Cursor = 16384
	}

	internal enum NotifyDetail
	{
		NotifyAncestor = 0,
		NotifyVirtual = 1,
		NotifyInferior = 2,
		NotifyNonlinear = 3,
		NotifyNonlinearVirtual = 4,
		NotifyPointer = 5,
		NotifyPointerRoot = 6,
		NotifyDetailNone = 7
	}

	internal enum NotifyMode
	{
		NotifyNormal = 0,
		NotifyGrab = 1,
		NotifyUngrab = 2
	}

	internal enum MapState
	{
		IsUnmapped = 0,
		IsUnviewable = 1,
		IsViewable = 2
	}

	internal enum Gravity
	{
		ForgetGravity = 0,
		NorthWestGravity = 1,
		NorthGravity = 2,
		NorthEastGravity = 3,
		WestGravity = 4,
		CenterGravity = 5,
		EastGravity = 6,
		SouthWestGravity = 7,
		SouthGravity = 8,
		SouthEastGravity = 9,
		StaticGravity = 10
	}

	internal enum GrabMode
	{
		GrabModeSync = 0,
		GrabModeAsync = 1
	}

	internal enum CursorFontShape
	{
		XC_X_cursor = 0,
		XC_arrow = 2,
		XC_based_arrow_down = 4,
		XC_based_arrow_up = 6,
		XC_boat = 8,
		XC_bogosity = 10,
		XC_bottom_left_corner = 12,
		XC_bottom_right_corner = 14,
		XC_bottom_side = 16,
		XC_bottom_tee = 18,
		XC_box_spiral = 20,
		XC_center_ptr = 22,

		XC_circle = 24,
		XC_clock = 26,
		XC_coffee_mug = 28,
		XC_cross = 30,
		XC_cross_reverse = 32,
		XC_crosshair = 34,
		XC_diamond_cross = 36,
		XC_dot = 38,
		XC_dotbox = 40,
		XC_double_arrow = 42,
		XC_draft_large = 44,
		XC_draft_small = 46,

		XC_draped_box = 48,
		XC_exchange = 50,
		XC_fleur = 52,
		XC_gobbler = 54,
		XC_gumby = 56,
		XC_hand1 = 58,
		XC_hand2 = 60,
		XC_heart = 62,
		XC_icon = 64,
		XC_iron_cross = 66,
		XC_left_ptr = 68,
		XC_left_side = 70,

		XC_left_tee = 72,
		XC_left_button = 74,
		XC_ll_angle = 76,
		XC_lr_angle = 78,
		XC_man = 80,
		XC_middlebutton = 82,
		XC_mouse = 84,
		XC_pencil = 86,
		XC_pirate = 88,
		XC_plus = 90,
		XC_question_arrow = 92,
		XC_right_ptr = 94,

		XC_right_side = 96,
		XC_right_tee = 98,
		XC_rightbutton = 100,
		XC_rtl_logo = 102,
		XC_sailboat = 104,
		XC_sb_down_arrow = 106,
		XC_sb_h_double_arrow = 108,
		XC_sb_left_arrow = 110,
		XC_sb_right_arrow = 112,
		XC_sb_up_arrow = 114,
		XC_sb_v_double_arrow = 116,
		XC_sb_shuttle = 118,

		XC_sizing = 120,
		XC_spider = 122,
		XC_spraycan = 124,
		XC_star = 126,
		XC_target = 128,
		XC_tcross = 130,
		XC_top_left_arrow = 132,
		XC_top_left_corner = 134,
		XC_top_right_corner = 136,
		XC_top_side = 138,
		XC_top_tee = 140,
		XC_trek = 142,

		XC_ul_angle = 144,
		XC_umbrella = 146,
		XC_ur_angle = 148,
		XC_watch = 150,
		XC_xterm = 152,
		XC_num_glyphs = 154
	}
	[Flags]
	internal enum ChangeWindowFlags
	{
		CWX = 1 << 0,
		CWY = 1 << 1,
		CWWidth = 1 << 2,
		CWHeight = 1 << 3,
		CWBorderWidth = 1 << 4,
		CWSibling = 1 << 5,
		CWStackMode = 1 << 6
	}

	internal enum CreateWindowArgs
	{
		CopyFromParent = 0,
		ParentRelative = 1,
		InputOutput = 1,
		InputOnly = 2
	}

	internal enum PropertyMode
	{
		Replace = 0,
		Prepend = 1,
		Append = 2
	}

	internal enum NetWindowManagerState
	{
		Remove = 0,
		Add = 1,
		Toggle = 2
	}

	internal enum RevertTo
	{
		None = 0,
		PointerRoot = 1,
		Parent = 2
	}

	internal enum SystrayRequest
	{
		SYSTEM_TRAY_REQUEST_DOCK = 0,
		SYSTEM_TRAY_BEGIN_MESSAGE = 1,
		SYSTEM_TRAY_CANCEL_MESSAGE = 2
	}

	internal enum XInitialState
	{
		DontCareState = 0,
		NormalState = 1,
		ZoomState = 2,
		IconicState = 3,
		InactiveState = 4
	}

	[Flags]
	internal enum XIMProperties
	{
		XIMPreeditArea = 0x0001,
		XIMPreeditCallbacks = 0x0002,
		XIMPreeditPosition = 0x0004,
		XIMPreeditNothing = 0x0008,
		XIMPreeditNone = 0x0010,
		XIMStatusArea = 0x0100,
		XIMStatusCallbacks = 0x0200,
		XIMStatusNothing = 0x0400,
		XIMStatusNone = 0x0800,
	}

	[Flags]
	internal enum WindowType
	{
		Client = 1,
		Whole = 2,
		Both = 3
	}

	[Flags]
	internal enum SelectionNotifyMask : long
	{
		XFixesSetSelectionOwnerNotifyMask = 1L << 0,
		XFixesSelectionWindowDestroyNotifyMask = 1L << 1,
		XFixesSelectionClientCloseNotifyMask = 1L << 2
	}
}
#endif