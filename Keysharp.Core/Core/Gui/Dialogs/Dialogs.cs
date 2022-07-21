using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Core.Windows;

namespace Keysharp.Core
{
	public static class Dialogs
	{
		internal static int nFileDialogs;
		internal static int nFolderDialogs;
		internal static int nMessageBoxes;
		internal static Dictionary<int, ProgressDialog> ProgressDialogs { get; set; }

		internal static Dictionary<int, SplashDialog> SplashDialogs { get; set; }

		// TODO: organise Dialogs.cs
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
				if (WindowsAPI.SHGetKnownFolderPath(new Guid(startingFolder.Trim(new char[] { ':', '{', '}' }))) is string s)
					select.SelectedPath = s;
			}
			else if (Keysharp.Core.Options.TryParseString(startingFolder, "*", ref str))
				select.SelectedPath = str;
			else if (startingFolder.Length != 0)
				select.SelectedPath = startingFolder;

			nFolderDialogs++;
			var selected = GuiHelper.DialogOwner == null ? select.ShowDialog() : select.ShowDialog(GuiHelper.DialogOwner);
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

			if (opts.Contains("M"))
			{
				opts = opts.Replace("M", string.Empty);
				multi = true;
			}

			if (opts.Contains("S"))
			{
				opts = opts.Replace("S", string.Empty);
				save = true;
			}

			if (opts.Contains("D"))
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
				var selected = GuiHelper.DialogOwner == null ? saveas.ShowDialog() : saveas.ShowDialog(GuiHelper.DialogOwner);
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
					var selected = GuiHelper.DialogOwner == null ? select.ShowDialog() : select.ShowDialog(GuiHelper.DialogOwner);
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
					var selected = GuiHelper.DialogOwner == null ? open.ShowDialog() : open.ShowDialog(GuiHelper.DialogOwner);
					files = selected == DialogResult.OK
							? multi ? new Array(open.FileNames.Cast<object>().ToArray()) : (object)open.FileName
							: multi ? new Array() : (object)"";
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
					if (i == splits.Count - 1 || splits[i].EndsWith(')') && !splits[i + 1].StartsWith("*"))
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
			var workarea = Monitor.MonitorGetWorkArea();
			var w = int.MinValue;
			var h = int.MinValue;
			var x = int.MinValue;
			var y = int.MinValue;
			var pw = "";

			foreach (var opt in Options.ParseOptions(options))
			{
				var temp = 0;

				if (Options.TryParse(opt, "w", ref temp)) { w = temp; }
				else if (Options.TryParse(opt, "h", ref temp)) { h = temp; }
				else if (Options.TryParse(opt, "x", ref temp)) { x = temp; }
				else if (Options.TryParse(opt, "y", ref temp)) { y = temp; }
				else if (Options.TryParse(opt, "t", ref temp)) { input.Timeout = temp; }
				else if (Options.TryParseString(opt, "Password", ref pw, StringComparison.OrdinalIgnoreCase, true)) { input.PasswordChar = pw; }
			}

			input.Load += (oo, ee) =>
			{
				var width = w != int.MinValue ? w : input.ClientSize.Width;
				var height = h != int.MinValue ? h : input.ClientSize.Height;
				input.ClientSize = new Size(width, height);
				input.Left = x != int.MinValue ? x : (((workarea["Right"].Ai() - workarea["Left"].Ai()) / 2) - (input.Width / 2));
				input.Top = y != int.MinValue ? y : (((workarea["Bottom"].Ai() - workarea["Top"].Ai()) / 2) - (input.Height / 2));
			};
			nFileDialogs++;

			if (GuiHelper.DialogOwner != null)
				_ = input.ShowDialog(GuiHelper.DialogOwner);
			else
				input.Show();

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
			var options = obj2.As();
			var buttons = MessageBoxButtons.OK;
			var icon = MessageBoxIcon.None;
			var defaultbutton = MessageBoxDefaultButton.Button1;
			var mbopts = (MessageBoxOptions)WindowsAPI.MB_SETFOREGROUND;//For some reason this constant is not available in C#, but it works and is required to make the message box take the focus.
			//var help = false;
			Control owner = GuiHelper.DialogOwner;// ?? Form.ActiveForm;
			var timeout = 0.0;

			if (title?.Length == 0)
			{
				title = Accessors.A_ScriptName;

				if (text?.Length == 0 && options?.Length == 0)
					text = "Press OK to continue.";
			}

			foreach (var opt in Options.ParseOptions(options))
			{
				long hwnd = 0;
				double temp = 0;
				var itemp = 0;

				if (Options.TryParse(opt, "Owner", ref hwnd)) { owner = Control.FromHandle(new IntPtr(hwnd)); }
				else if (Options.TryParse(opt, "T", ref temp)) { timeout = temp; }
				else if (int.TryParse(opt, out itemp))
				{
					switch (itemp & 0xf0000)
					{
						case 524288: mbopts |= MessageBoxOptions.RightAlign; continue;

						case 1048576: mbopts |= MessageBoxOptions.RtlReading; continue;
					}

					//switch (itemp & 0xf000)
					//{
					//  case 16384: help = true; continue;
					//}

					switch (itemp & 0xf00)
					{
						case 256: defaultbutton = MessageBoxDefaultButton.Button2; continue;

						case 512: defaultbutton = MessageBoxDefaultButton.Button3; continue;
					}

					switch (itemp & 0xf0)
					{
						case 16: icon = MessageBoxIcon.Hand; continue;

						case 32: icon = MessageBoxIcon.Question; continue;

						case 48: icon = MessageBoxIcon.Exclamation; continue;

						case 64: icon = MessageBoxIcon.Asterisk; continue;
					}

					switch (itemp & 0xf)
					{
						case 0: buttons = MessageBoxButtons.OK; continue;

						case 1: buttons = MessageBoxButtons.OKCancel; continue;

						case 2: buttons = MessageBoxButtons.AbortRetryIgnore; continue;

						case 3: buttons = MessageBoxButtons.YesNoCancel; continue;

						case 4: buttons = MessageBoxButtons.YesNo; continue;

						case 5: buttons = MessageBoxButtons.RetryCancel; continue;

						case 6: buttons = MessageBoxButtons.CancelTryContinue; continue;
					}

					//System modal dialogs are no longer supported in Windows.
				}
				else
				{
					switch (opt)
					{
						case "OK": buttons = MessageBoxButtons.OK; break;

						case "OKCancel":
						case "O/C":
						case "OC":
							buttons = MessageBoxButtons.OKCancel; break;

						case "AbortRetryIgnore":
						case "A/R/I":
						case "ARI":
							buttons = MessageBoxButtons.AbortRetryIgnore; break;

						case "YesNoCancel":
						case "Y/N/C":
						case "YNC":
							buttons = MessageBoxButtons.YesNoCancel; break;

						case "YesNo":
						case "Y/N":
						case "YN":
							buttons = MessageBoxButtons.YesNo; break;

						case "RetryCancel":
						case "R/C":
						case "RC":
							buttons = MessageBoxButtons.RetryCancel; break;

						case "CancelTryAgainContinue":
						case "C/T/C":
						case "CTC":
							buttons = MessageBoxButtons.CancelTryContinue; break;

						case "Iconx": icon = MessageBoxIcon.Hand; break;

						case "Icon?": icon = MessageBoxIcon.Question; break;

						case "Icon!": icon = MessageBoxIcon.Exclamation; break;

						case "Iconi": icon = MessageBoxIcon.Asterisk; break;

						case "Default2": defaultbutton = MessageBoxDefaultButton.Button2; break;

						case "Default3": defaultbutton = MessageBoxDefaultButton.Button3; break;

						case "Default4": defaultbutton = MessageBoxDefaultButton.Button4; break;
					}
				}
			}

			if (timeout != 0)
			{
				var timeoutclosed = false;
				var w = new Form() { Size = new Size(0, 0), Location = new Point(-10, -10) };//Gotten from https://stackoverflow.com/a/26418199
				nMessageBoxes++;
				w.Show();
				w.BringToFront();
				_ = Task.Delay(TimeSpan.FromSeconds(timeout))
					.ContinueWith(t =>
				{
					w.Close();
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
				//_ = owner.BeginInvoke(() => ret = MessageBox.Show(owner, text, title, buttons, icon, defaultbutton, mbopts).ToString());
				else
					ret = MessageBox.Show(null, text, title, buttons, icon, defaultbutton, mbopts).ToString();

				nMessageBoxes--;
				return ret;
			}
		}

		/// <summary>
		/// Creates or updates a window containing a progress bar or an image.
		/// </summary>
		/// <param name="ProgressParam1">
		/// <para>If the progress window already exists: If Param1 is the word OFF, the window is destroyed. If Param1 is the word SHOW, the window is shown if it is currently hidden.</para>
		/// <para>Otherwise, if Param1 is an pure number, its bar's position is changed to that value. If Param1 is blank, its bar position will be unchanged but its text will be updated to reflect any new strings provided in SubText, MainText, and WinTitle. In both of these modes, if the window doesn't yet exist, it will be created with the defaults for all options.</para>
		/// <para>If the progress window does not exist: A new progress window is created (replacing any old one), and Param1 is a string of zero or more options from the list below.</para>
		/// </param>
		/// <param name="SubText">The text to display below the image or bar indicator. Although word-wrapping will occur, to begin a new line explicitly, use linefeed (`n). To set an existing window's text to be blank, specify %A_Space%. For the purpose of auto-calculating the window's height, blank lines can be reserved in a way similar to MainText below.</param>
		/// <param name="MainText">
		/// <para>The text to display above the image or bar indicator (its font is semi-bold). Although word-wrapping will occur, to begin a new line explicitly, use linefeed (`n).</para>
		/// <para>If blank or omitted, no space will be reserved in the window for MainText. To reserve space for single line to be added later, or to set an existing window's text to be blank, specify %A_Space%. To reserve extra lines beyond the first, append one or more linefeeds (`n).</para>
		/// <para>Once the height of MainText's control area has been set, it cannot be changed without recreating the window.</para>
		/// </param>
		/// <param name="WinTitle">The text to display below the image or bar indicator. Although word-wrapping will occur, to begin a new line explicitly, use linefeed (`n). To set an existing window's text to be blank, specify %A_Space%. For the purpose of auto-calculating the window's height, blank lines can be reserved in a way similar to MainText below.</param>
		/// <param name="FontName">
		/// <para>The name of the font to use for both MainText and SubText. The font table lists the fonts included with the various versions of Windows. If unspecified or if the font cannot be found, the system's default GUI font will be used.</para>
		/// <para>See the options section below for how to change the size, weight, and color of the font.</para>
		/// </param>
		public static void Progress(string ProgressParam1, string SubText, string MainText, string WinTitle, string FontName)
		{
			InitDialogs();
			var progressOptions = new ComplexDlgOptions()
			{
				SubText = SubText,
				MainText = MainText,
				WinTitle = WinTitle,
			};
			progressOptions.ParseGuiID(ProgressParam1);
			progressOptions.ParseComplexOptions(ProgressParam1);
			ProgressAsync(progressOptions);
		}

		/// <summary>
		/// Creates or updates a window containing a progress bar or an image.
		/// </summary>
		/// <param name="ImageFile">
		/// <para>If this is the word OFF, the window is destroyed. If this is the word SHOW, the window is shown if it is currently hidden.</para>
		/// <para>Otherwise, this is the file name of the BMP, GIF, or JPG image to display (to display other file formats such as PNG, TIF, and ICO, consider using the Gui command to create a window containing a picture control).</para>
		/// <para>ImageFile is assumed to be in %A_WorkingDir% if an absolute path isn't specified. If ImageFile and Options are blank and the window already exists, its image will be unchanged but its text will be updated to reflect any new strings provided in SubText, MainText, and WinTitle.</para>
		/// <para>For newly created windows, if ImageFile is blank or there is a problem loading the image, the window will be displayed without the picture.</para>
		/// </param>
		/// <param name="Options">A string of zero or more options from the list further below.</param>
		/// <param name="SubText">The text to display below the image or bar indicator. Although word-wrapping will occur, to begin a new line explicitly, use linefeed (`n). To set an existing window's text to be blank, specify %A_Space%. For the purpose of auto-calculating the window's height, blank lines can be reserved in a way similar to MainText below.</param>
		/// <param name="MainText">
		/// <para>The text to display above the image or bar indicator (its font is semi-bold). Although word-wrapping will occur, to begin a new line explicitly, use linefeed (`n).</para>
		/// <para>If blank or omitted, no space will be reserved in the window for MainText. To reserve space for single line to be added later, or to set an existing window's text to be blank, specify %A_Space%. To reserve extra lines beyond the first, append one or more linefeeds (`n).</para>
		/// <para>Once the height of MainText's control area has been set, it cannot be changed without recreating the window.</para>
		/// </param>
		/// <param name="WinTitle">The title of the window. If omitted and the window is being newly created, the title defaults to the name of the script (without path). If the B (borderless) option has been specified, there will be no visible title bar but the window can still be referred to by this title in commands such as WinMove.</param>
		/// <param name="FontName">
		/// <para>The name of the font to use for both MainText and SubText. The font table lists the fonts included with the various versions of Windows. If unspecified or if the font cannot be found, the system's default GUI font will be used.</para>
		/// <para>See the options section below for how to change the size, weight, and color of the font.</para>
		/// </param>
		public static void SplashImage(string ImageFile, string Options, string SubText, string MainText, string WinTitle, string FontName)
		{
			InitDialogs();

			if (string.IsNullOrEmpty(ImageFile))
				return;

			var splashOptions = new ComplexDlgOptions()
			{
				SubText = SubText,
				MainText = MainText,
				WinTitle = WinTitle,
			};
			splashOptions.ParseGuiID(ImageFile);
			splashOptions.ParseComplexOptions(Options);
			SplashImageAsync(splashOptions);
		}

		private static void InitDialogs()
		{
			if (ProgressDialogs == null)
				ProgressDialogs = new Dictionary<int, ProgressDialog>();

			if (SplashDialogs == null)
				SplashDialogs = new Dictionary<int, SplashDialog>();
		}

		private static void ProgressAsync(ComplexDlgOptions Options)
		{
			ProgressDialog thisProgress = null;

			if (ProgressDialogs.ContainsKey(Options.GUIID))
			{
				thisProgress = ProgressDialogs[Options.GUIID];

				if (thisProgress.InvokeRequired)
				{
					_ = thisProgress.Invoke(new AsyncCallDlgOptions(ProgressAsync), Options);
				}
			}

			if (thisProgress != null)
			{
				Options.AppendShowHideTo(thisProgress);
			}
			else
			{
				thisProgress = new ProgressDialog();
				ProgressDialogs.Add(Options.GUIID, thisProgress);
			}

			Options.AppendTo(thisProgress);

			if (!short.TryParse(Options.Param1, out var num))
			{
				num = 0;
			}

			thisProgress.ProgressValue = num;

			if (!Options.Hide && !thisProgress.Visible)
				thisProgress.Show();
		}

		private static void SplashImageAsync(ComplexDlgOptions Options)
		{
			SplashDialog thisSplash = null;
			System.Drawing.Image thisImage = null;

			if (SplashDialogs.ContainsKey(Options.GUIID))
			{
				thisSplash = SplashDialogs[Options.GUIID];

				if (thisSplash.InvokeRequired)
				{
					_ = thisSplash.Invoke(new AsyncCallDlgOptions(SplashImageAsync), Options);
				}
			}

			if (thisSplash != null)
			{
				Options.AppendShowHideTo(thisSplash);
			}
			else
			{
				thisSplash = new SplashDialog();
				SplashDialogs.Add(Options.GUIID, thisSplash);
			}

			Options.AppendTo(thisSplash);

			if (System.IO.File.Exists(Options.Param1))
			{
				try
				{
					thisImage = System.Drawing.Bitmap.FromFile(Options.Param1);
				}
				catch (Exception ex)
				{
					throw new Error(ex.Message);
				}

				if (thisImage != null)
				{
					thisSplash.Image = thisImage;
				}
			}

			if (!Options.Hide && !thisSplash.Visible)
				thisSplash.Show();
		}

		public class DialogResultReturn
		{
			public string Result { get; set; }
			public string Value { get; set; }
		}

		public delegate void AsyncCallDlgOptions(ComplexDlgOptions options);

		public delegate void AsyncComplexDialoge(IComplexDialog complexDlg);
	}
}