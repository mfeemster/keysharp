namespace Keysharp.Core
{
	public static class Dialogs
	{
		internal static int nFileDialogs;
		internal static int nFolderDialogs;
		internal static int nMessageBoxes;
		internal static Guid computer = new Guid("0AC0837C-BBF8-452A-850D-79D08E667CA7");//Computer (/).
		internal static Guid desktop = new Guid("B4BFCC3A-DB2C-424C-B029-7FE99A87C641");//Desktop (~/Desktop).
		internal static Guid documents = new Guid("FDD39AD0-238F-46AF-ADB4-6C85480369C7");//Documents (~/Documents).
		internal static Guid downloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");//Downloads (~/Downloads).
		internal static Guid music = new Guid("4BD8D571-6D19-48D3-BE97-422220080E43");//Music (~/Music).
		internal static Guid pictures = new Guid("33E28130-4E1E-4676-835A-98395C3BC3BB");//Pictures (~/Pictures).
		internal static Guid @public = new Guid("DFDF76A2-C82A-4D63-906A-5644AC457385");//Public (~/Public).
		internal static Guid userprofile = new Guid("5E6C858F-0E22-4760-9AFE-EA3317B67173");//User profile root (~/).
		internal static Guid userprofiles = new Guid("0762D272-C50A-4BB0-A382-697DCD729B80");//User profiles (/home).

		/// <summary>
		/// Displays a standard dialog that allows the user to select a folder.
		/// The list of known supported CLSID GUIDS is here: https://docs.microsoft.com/en-us/previous-versions//bb762584(v=vs.85)
		/// Note this does not support the following features that are defined in AHK:
		/// -Restricting which folders the user can browse to.
		/// -Selected folder isn't actually selected in the tree, it's just shown as text in the path text box.
		/// -Showing an edit box, which is not needed because the user can type in the path combo box.
		/// -The New Folder button is always shown.
		/// -Option 7 to omit BIF_NEWDIALOGSTYLE is not supported.
		/// </summary>
		public static string DirSelect(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			var startingFolder = obj0.As();
			var options = obj1.Al();
			var prompt = obj2.As();
			var str = "";
			var select = new FolderBrowserDialog
			{
				ShowNewFolderButton = (options & 1) == 1//The 3 and 5 options seem to not apply to this class in C#.
			};
			select.Description = prompt != "" ? prompt : "Select Folder - " + Accessors.A_ScriptName;
			select.RootFolder = Environment.SpecialFolder.MyComputer;

			if (startingFolder.StartsWith("::"))
			{
				var guidStr = startingFolder.Trim([':', '{', '}']).ToLower();
				var guid = new Guid(guidStr);
#if LINUX

				if (guid == computer)
					select.SelectedPath = "/";
				else if (guid == desktop)
					select.SelectedPath = "~/Desktop";
				else if (guid == documents)
					select.SelectedPath = "~/Documents";
				else if (guid == downloads)
					select.SelectedPath = "~/Downloads";
				else if (guid == music)
					select.SelectedPath = "~/Music";
				else if (guid == pictures)
					select.SelectedPath = "~/Pictures";
				else if (guid == @public)
					select.SelectedPath = "~/Public";
				else if (guid == userprofile)
					select.SelectedPath = "~/";
				else if (guid == userprofiles)
					select.SelectedPath = "/home";

#elif WINDOWS

				if (WindowsAPI.SHGetKnownFolderPath(new Guid(guidStr)) is string s)
					select.SelectedPath = s;

#endif
			}
			else if (Options.TryParseString(startingFolder, "*", ref str))
				select.SelectedPath = str;
			else if (startingFolder.Length != 0)
				select.SelectedPath = startingFolder;

			nFolderDialogs++;
			var selected = Script.mainWindow.CheckedInvoke(() => GuiHelper.DialogOwner == null ? select.ShowDialog() : select.ShowDialog(GuiHelper.DialogOwner), true);
			nFolderDialogs--;
			return selected == DialogResult.OK ? select.SelectedPath : "";
		}

		/// <summary>
		/// Displays a standard dialog that allows the user to open or save files.
		/// </summary>
		/// <param name="OutputVar">The user selected files.</param>
		/// <param name="Options">
		/// <list type="bullet">
		/// <item><term>M</term>: <description>allow muliple files to be selected.</description></item>
		/// <item><term>S</term>: <description>show a save as dialog rather than a file open dialog.</description></item>
		/// <item><term>1</term>: <description>only allow existing file or directory paths.</description></item>
		/// <item><term>8</term>: <description>prompt to create files.</description></item>
		/// <item><term>16:</term>: <description>prompt to overwrite files.</description></item>
		/// <item><term>32</term>: <description>follow the target of a shortcut rather than using the shortcut file itself.</description></item>
		/// </list>
		/// </param>
		/// <param name="RootDir">The file path to initially select.</param>
		/// <param name="Prompt">Text displayed in the window to instruct the user what to do.</param>
		/// <param name="Filter">Indicates which types of files are shown by the dialog, e.g. <c>Audio (*.wav; *.mp2; *.mp3)</c>.</param>
		public static object FileSelect(object obj0 = null, object obj1 = null, object obj2 = null, object obj3 = null)
		{
			var opts = obj0.As();
			var rootdir = obj1.As();
			var title = obj2.As();
			var filter = FixFilters(obj3.As());
			bool save = false, multi = false, check = false, create = false, overwite = false, shortcuts = false, dir = false;
			opts = opts.ToUpperInvariant();
			object files = null;

			if (opts.Contains('M'))
			{
				opts = opts.Replace("M", string.Empty);
				multi = true;
			}

			if (opts.Contains('S'))
			{
				opts = opts.Replace("S", string.Empty);
				save = true;
			}

			if (opts.Contains('D'))
			{
				opts = opts.Replace("D", string.Empty);
				dir = true;
			}

			if (int.TryParse(opts.Trim(), out var result))
			{
				if ((result & 1) == 1 || (result & 2) == 2)
					check = true;

				if (!save && ((result & 8) == 8))
					create = true;

				if ((result & 16) == 16)
				{
					overwite = true;

					if ((result & 8) != 8)
						save = true;
				}

				if ((result & 32) == 32)
					shortcuts = true;

				if ((result & 24) == 24)
				{
					if (!save)
						overwite = true;
					else
						create = true;
				}
			}

			nFileDialogs++;

			if (!filter.Contains("All Files (*.*)|*.*"))
				filter += "|All Files (*.*)|*.*";

			if (save)
			{
				if (title?.Length == 0)
					title = $"Select File - {Accessors.A_ScriptName}";

				var saveas = new SaveFileDialog
				{
					CheckPathExists = check,
					CreatePrompt = create,
					OverwritePrompt = overwite,
					DereferenceLinks = shortcuts,
					Filter = filter,
					Title = title,
					InitialDirectory = Path.GetDirectoryName(rootdir),
					FileName = Path.GetFileName(rootdir)
				};
				var selected = Script.mainWindow.CheckedInvoke(() => GuiHelper.DialogOwner == null ? saveas.ShowDialog() : saveas.ShowDialog(GuiHelper.DialogOwner), true);
				files = selected == DialogResult.OK ? saveas.FileName : "";
			}
			else
			{
				if (dir)
				{
					if (title?.Length == 0)
						title = $"Select Folder - {Accessors.A_ScriptName}";

					var select = new FolderBrowserDialog()
					{
						RootFolder = Environment.SpecialFolder.MyComputer,
						SelectedPath = rootdir + Path.DirectorySeparatorChar,
						Description = title,
						ShowNewFolderButton = true
					};
					var selected = Script.mainWindow.CheckedInvoke(() => GuiHelper.DialogOwner == null ? select.ShowDialog() : select.ShowDialog(GuiHelper.DialogOwner), true);
					files = selected == DialogResult.OK ? select.SelectedPath : "";
				}
				else
				{
					if (title?.Length == 0)
						title = $"Select File - {Accessors.A_ScriptName}";

					var open = new OpenFileDialog
					{
						Multiselect = multi,
						CheckFileExists = check,
						DereferenceLinks = shortcuts,
						Filter = filter,
						Title = title,
						InitialDirectory = Path.GetDirectoryName(rootdir),
						FileName = Path.GetFileName(rootdir)
					};
					var selected = Script.mainWindow.CheckedInvoke(() => GuiHelper.DialogOwner == null ? open.ShowDialog() : open.ShowDialog(GuiHelper.DialogOwner), true);
					files = selected == DialogResult.OK
							? multi ? new Array(open.FileNames.Cast<object>().ToArray()) : open.FileName
							: multi ? new Array() : "";
				}
			}

			nFileDialogs--;
			return files;
		}

		public static string FixFilters(string filter)
		{
			if (filter.Length > 0)
			{
				var splits = filter.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
				var origcount = splits.Count;

				for (var i = 0; i < splits.Count; i++)
				{
					if (i == splits.Count - 1 || (splits[i].EndsWith(')') && !splits[i + 1].StartsWith("*")))
					{
						var paren1 = splits[i].IndexOf('(');
						var paren2 = splits[i].LastIndexOf(')');

						if (paren1 != -1 && paren2 != -1)
						{
							splits.Insert(i + 1, splits[i].Substring(paren1 + 1, paren2 - paren1 - 1).Replace(',', ';'));
							i++;
						}
					}
				}

				return splits.Count == origcount ? filter : string.Join('|', splits);
			}
			else
				return "All Files (*.*)|*.*";
		}

		/// <summary>
		/// Displays an input box to ask the user to enter a string.
		/// </summary>
		/// <param name="OutputVar">The name of the variable in which to store the text entered by the user.</param>
		/// <param name="Title">The title of the input box. If blank or omitted, it defaults to the name of the script.</param>
		/// <param name="Prompt">The text of the input box, which is usually a message to the user indicating what kind of input is expected.</param>
		/// <param name="Hide">If this parameter is the word HIDE, the user's input will be masked, which is useful for passwords.</param>
		/// <param name="Width">If this parameter is blank or omitted, the starting width of the window will be 375.</param>
		/// <param name="Height">If this parameter is blank or omitted, the starting height of the window will be 189.</param>
		/// <param name="X">The X coordinate of the window (use 0,0 to move it to the upper left corner of the desktop). If either coordinate is blank or omitted, the dialog will be centered in that dimension. Either coordinate can be negative to position the window partially or entirely off the desktop.</param>
		/// <param name="Y">The Y coordinate of the window (see <paramref name="X"/>).</param>
		/// <param name="Font">Not yet implemented (leave blank). In the future it might accept something like verdana:8</param>
		/// <param name="Timeout">Timeout in seconds (can contain a decimal point).  If this value exceeds 2147483 (24.8 days), it will be set to 2147483. After the timeout has elapsed, the InputBox window will be automatically closed and ErrorLevel will be set to 2. OutputVar will still be set to what the user entered.</param>
		/// <param name="Default">A string that will appear in the InputBox's edit field when the dialog first appears. The user can change it by backspacing or other means.</param>
		public static DialogResultReturn InputBox(object obj0 = null, object obj1 = null, object obj2 = null, object obj3 = null)
		{
			var prompt = obj0.As();
			var title = obj1.As();
			var options = obj2.As();
			var def = obj3.As();
			var input = new InputDialog
			{
				Default = def,
				Prompt = prompt,
				Title = title?.Length == 0 ? Accessors.A_ScriptName : title
			};
			object wl = null, wt = null, wr = null, wb = null;
			var workarea = Monitor.MonitorGetWorkArea(null, ref wl, ref wt, ref wr, ref wb);
			var w = int.MinValue;
			var h = int.MinValue;
			var x = int.MinValue;
			var y = int.MinValue;
			var pw = "";

			foreach (Range r in options.AsSpan().SplitAny(Keywords.Spaces))
			{
				var temp = 0;
				var opt = options.AsSpan(r).Trim();

				if (opt.Length > 0)
				{
					if (Options.TryParse(opt, "w", ref temp)) { w = temp; }
					else if (Options.TryParse(opt, "h", ref temp)) { h = temp; }
					else if (Options.TryParse(opt, "x", ref temp)) { x = temp; }
					else if (Options.TryParse(opt, "y", ref temp)) { y = temp; }
					else if (Options.TryParse(opt, "t", ref temp)) { input.Timeout = temp; }
					else if (Options.TryParseString(opt, "Password", ref pw, StringComparison.OrdinalIgnoreCase, true)) { input.PasswordChar = pw; }
				}
			}

			input.Load += (oo, ee) =>
			{
				var width = w != int.MinValue ? w : input.ClientSize.Width;
				var height = h != int.MinValue ? h : input.ClientSize.Height;
				input.ClientSize = new Size(width, height);
				input.Left = x != int.MinValue ? x : (((wr.Ai() - wl.Ai()) / 2) - (input.Width / 2));
				input.Top = y != int.MinValue ? y : (((wb.Ai() - wt.Ai()) / 2) - (input.Height / 2));
			};
			nFileDialogs++;
			Script.mainWindow.CheckedInvoke(() =>
			{
				if (GuiHelper.DialogOwner != null)
					_ = input.ShowDialog(GuiHelper.DialogOwner);
				else
					input.Show();
			}, true);

			while (input.Visible)
				Application.DoEvents();

			nFileDialogs--;
			return new DialogResultReturn()
			{
				Value = input.Message,
				Result = input.Result
			};
		}

		/// <summary>
		/// Show a message box.
		/// </summary>
		/// <param name="text">The text to show in the prompt, which can be an object of any kind.</param>
		//public static DialogResult MsgBox(object text)
		//{
		//  var txt = Script.ForceString(text);
		//  //Task.Factory.StartNew(() =>
		//  {
		//      var title = Environment.GetEnvironmentVariable("SCRIPT") ?? "Keysharp";
		//
		//      if (!string.IsNullOrEmpty(title) && System.IO.File.Exists(title))
		//          title = Path.GetFileName(title);
		//
		//      var res = GuiHelper.DialogOwner != null ? MessageBox.Show(GuiHelper.DialogOwner, txt, title) : MessageBox.Show(txt, title);
		//      return res;
		//  }//);
		//}

		/// <summary>
		/// Displays the specified text in a small window containing one or more buttons (such as Yes and No).
		/// </summary>
		/// <param name="Options">
		/// <para>Indicates the type of message box and the possible button combinations. If blank or omitted, it defaults to 0. See the table below for allowed values.</para>
		/// <para>This parameter will not be recognized if it contains an expression or a variable reference such as %option%. Instead, use a literal numeric value.</para>
		/// </param>
		/// <param name="Title">The title of the message box window. If omitted or blank, it defaults to the name of the script (without path).</param>
		/// <param name="Text">
		/// <para>If all the parameters are omitted, the MsgBox will display the text "Press OK to continue." Otherwise, this parameter is the text displayed inside the message box to instruct the user what to do, or to present information.</para>
		/// <para>Escape sequences can be used to denote special characters. For example, `n indicates a linefeed character, which ends the current line and begins a new one. Thus, using text1`n`ntext2 would create a blank line between text1 and text2.</para>
		/// <para>If Text is long, it can be broken up into several shorter lines by means of a continuation section, which might improve readability and maintainability.</para>
		/// </param>
		/// <param name="Timeout">
		/// <para>(optional) Timeout in seconds (can contain a decimal point but cannot be an expression).  If this value exceeds 2147483 (24.8 days), it will be set to 2147483.  After the timeout has elapsed the message box will be automatically closed and the IfMsgBox command will see the value TIMEOUT.</para>
		/// <para>Known limitation: If the MsgBox contains only an OK button, IfMsgBox will think that the OK button was pressed if the MsgBox times out while its own thread is interrupted by another.</para>
		/// </param>
		public static string MsgBox(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			var text = obj0.As();
			var title = obj1.As();
			var buttons = MessageBoxButtons.OK;
			var icon = MessageBoxIcon.None;
			var defaultbutton = MessageBoxDefaultButton.Button1;
#if WINDOWS
			var mbopts = (MessageBoxOptions)WindowsAPI.MB_SETFOREGROUND;//For some reason this constant is not available in C#, but it works and is required to make the message box take the focus.
#else
			var mbopts = MessageBoxOptions.ServiceNotification;
#endif
			//var help = false;
			Control owner = GuiHelper.DialogOwner;// ?? Form.ActiveForm;
			var timeout = 0.0;

			if (title?.Length == 0)
			{
				title = Accessors.A_ScriptName;

				if (text?.Length == 0 && obj2 == null)
					text = "Press OK to continue.";
			}

			void HandleNumericOptions(int itemp)
			{
				switch (itemp & 0xf0000)
				{
					case 524288: mbopts |= MessageBoxOptions.RightAlign; break;

					case 1048576: mbopts |= MessageBoxOptions.RtlReading; break;
				}

				//switch (itemp & 0xf000)
				//{
				//  case 16384: help = true; break;
				//}

				switch (itemp & 0xf00)
				{
					case 256: defaultbutton = MessageBoxDefaultButton.Button2; break;

					case 512: defaultbutton = MessageBoxDefaultButton.Button3; break;
				}

				switch (itemp & 0xf0)
				{
					case 16: icon = MessageBoxIcon.Hand; break;

					case 32: icon = MessageBoxIcon.Question; break;

					case 48: icon = MessageBoxIcon.Exclamation; break;

					case 64: icon = MessageBoxIcon.Asterisk; break;
				}

				switch (itemp & 0xf)
				{
					case 0: buttons = MessageBoxButtons.OK; break;

					case 1: buttons = MessageBoxButtons.OKCancel; break;

					case 2: buttons = MessageBoxButtons.AbortRetryIgnore; break;

					case 3: buttons = MessageBoxButtons.YesNoCancel; break;

					case 4: buttons = MessageBoxButtons.YesNo; break;

					case 5: buttons = MessageBoxButtons.RetryCancel; break;
#if WINDOWS

					case 6: buttons = MessageBoxButtons.CancelTryContinue; break;
#endif
				}

				//System modal dialogs are no longer supported in Windows.
			}

			if (Script.IsNumeric(obj2))
			{
				HandleNumericOptions(obj2.Ai());
			}
			else
			{
				var options = obj2.As();
				var iopt = 0;
				var hadNumeric = false;

				foreach (Range r in options.AsSpan().SplitAny(Keywords.Spaces))
				{
					var opt = options.AsSpan(r).Trim();

					if (opt.Length > 0)
					{
						long hwnd = 0;

						if (Options.TryParse(opt, "Owner", ref hwnd)) { owner = Control.FromHandle(new IntPtr(hwnd)); }
						else if (Options.TryParse(opt, "T", ref timeout)) { }
						else if (int.TryParse(opt, out var itemp))
						{
							hadNumeric = true;
							iopt |= itemp;
						}
						else
						{
							switch (opt)
							{
								case var b when opt.Equals("ok", StringComparison.OrdinalIgnoreCase):
									buttons = MessageBoxButtons.OK;
									break;

								case var b when opt.Equals("okcancel", StringComparison.OrdinalIgnoreCase):
								case var b2 when opt.Equals("o/c", StringComparison.OrdinalIgnoreCase):
								case var b3 when opt.Equals("oc", StringComparison.OrdinalIgnoreCase):
									buttons = MessageBoxButtons.OKCancel;
									break;

								case var b when opt.Equals("abortretryignore", StringComparison.OrdinalIgnoreCase):
								case var b2 when opt.Equals("a/r/i", StringComparison.OrdinalIgnoreCase):
								case var b3 when opt.Equals("ari", StringComparison.OrdinalIgnoreCase):
									buttons = MessageBoxButtons.AbortRetryIgnore;
									break;

								case var b when opt.Equals("yesnocancel", StringComparison.OrdinalIgnoreCase):
								case var b2 when opt.Equals("y/n/c", StringComparison.OrdinalIgnoreCase):
								case var b3 when opt.Equals("ync", StringComparison.OrdinalIgnoreCase):
									buttons = MessageBoxButtons.YesNoCancel;
									break;

								case var b when opt.Equals("yesno", StringComparison.OrdinalIgnoreCase):
								case var b2 when opt.Equals("y/n", StringComparison.OrdinalIgnoreCase):
								case var b3 when opt.Equals("yn", StringComparison.OrdinalIgnoreCase):
									buttons = MessageBoxButtons.YesNo;
									break;

								case var b when opt.Equals("retrycancel", StringComparison.OrdinalIgnoreCase):
								case var b2 when opt.Equals("r/c", StringComparison.OrdinalIgnoreCase):
								case var b3 when opt.Equals("rc", StringComparison.OrdinalIgnoreCase):
									buttons = MessageBoxButtons.RetryCancel;
									break;
#if WINDOWS

								case var b when opt.Equals("canceltryagaincontinue", StringComparison.OrdinalIgnoreCase):
								case var b2 when opt.Equals("c/t/c", StringComparison.OrdinalIgnoreCase):
								case var b3 when opt.Equals("ctc", StringComparison.OrdinalIgnoreCase):
									buttons = MessageBoxButtons.CancelTryContinue;
									break;
#endif

								case var b when opt.Equals("iconx", StringComparison.OrdinalIgnoreCase):
									icon = MessageBoxIcon.Hand;
									break;

								case var b when opt.Equals("icon?", StringComparison.OrdinalIgnoreCase):
									icon = MessageBoxIcon.Question;
									break;

								case var b when opt.Equals("icon!", StringComparison.OrdinalIgnoreCase):
									icon = MessageBoxIcon.Exclamation;
									break;

								case var b when opt.Equals("iconi", StringComparison.OrdinalIgnoreCase):
									icon = MessageBoxIcon.Asterisk;
									break;

								case var b when opt.Equals("default2", StringComparison.OrdinalIgnoreCase):
									defaultbutton = MessageBoxDefaultButton.Button2;
									break;

								case var b when opt.Equals("default3", StringComparison.OrdinalIgnoreCase):
									defaultbutton = MessageBoxDefaultButton.Button3;
									break;
#if WINDOWS

								case var b when opt.Equals("default4", StringComparison.OrdinalIgnoreCase):
									defaultbutton = MessageBoxDefaultButton.Button4;
									break;
#endif
							}
						}
					}
				}

				if (hadNumeric)
					HandleNumericOptions(iopt);
			}

			if (timeout != 0)
			{
				var timeoutclosed = false;
				var w = new Form() { Size = new Size(0, 0) };//Gotten from https://stackoverflow.com/a/26418199
				nMessageBoxes++;
				//No need to show the form, it will work while hidden.
				_ = Task.Delay(TimeSpan.FromSeconds(timeout))
					.ContinueWith(t =>
				{
					w.Invoke(() => w.Close());
					timeoutclosed = true;
				}, TaskScheduler.FromCurrentSynchronizationContext());
				var ret = MessageBox.Show(w, text, title, buttons, icon, defaultbutton, mbopts);
				nMessageBoxes--;
				return timeoutclosed ? "Timeout" : ret.ToString();
			}
			else
			{
				nMessageBoxes++;
				var ret = "";

				if (owner != null)
					_ = owner.Invoke(() => ret = MessageBox.Show(owner, text, title, buttons, icon, defaultbutton, mbopts).ToString());
				else
					ret = Script.mainWindow.CheckedInvoke(() => MessageBox.Show(null, text, title, buttons, icon, defaultbutton, mbopts).ToString(), true);

				nMessageBoxes--;
				return ret;
			}
		}

		internal static void CloseMessageBoxes()
		{
			//Will need a way to do this on linux.//TODO
			var tempn = nMessageBoxes;

			while (tempn > 0)
			{
				IntPtr wnd;

				if ((wnd = WindowsAPI.FindWindow("#32770", null)) != IntPtr.Zero)
				{
					_ = WindowsAPI.SendMessage(wnd, WindowsAPI.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
					tempn--;
				}
			}
		}

		public class DialogResultReturn
		{
			public string Result { get; set; }
			public string Value { get; set; }
		}
	}
}