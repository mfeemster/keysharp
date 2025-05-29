namespace Keysharp.Scripting
{
	public partial class MainWindow : KeysharpForm
	{
		public static Font OurDefaultFont = new ("Microsoft Sans Serif", 9F);
		internal FormWindowState lastWindowState = FormWindowState.Normal;
#if WINDOWS
		private readonly bool clipSuccess;
#endif
		private AboutBox about;
		private bool callingInternalVars = false;

#if LINUX
		//private static Gdk.Atom clipAtom = Gdk.Atom.Intern("CLIPBOARD", false);
		//private Gtk.Clipboard gtkClipBoard = Gtk.Clipboard.Get(clipAtom);
#endif

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsClosing { get; private set; }

		internal ToolStripMenuItem SuspendHotkeysToolStripMenuItem => suspendHotkeysToolStripMenuItem;

		public MainWindow()
		{
			InitializeComponent();
			//FormBorderStyle = FormBorderStyle.SizableToolWindow;
			SetStyle(ControlStyles.StandardClick, true);
			SetStyle(ControlStyles.StandardDoubleClick, true);
			SetStyle(ControlStyles.EnableNotifyMessage, true);
#if LINUX
			//gtkClipBoard.OwnerChange += gtkClipBoard_OwnerChange;
#elif WINDOWS
			clipSuccess = WindowsAPI.AddClipboardFormatListener(Handle);//Need a cross platform way to do this.//TODO
#endif
			tpVars.HandleCreated += TpVars_HandleCreated;
			editScriptToolStripMenuItem.Visible = !A_IsCompiled;
		}

		public void AddText(string s, MainFocusedTab tab, bool focus)
		{
			//Use CheckedBeginInvoke() because CheckedInvoke() seems to crash if this is called right as the window is closing.
			//Such as with a hotkey that prints on mouse click, which will cause a print when the X is clicked to close.
			this.CheckedBeginInvoke(() =>
			{
				GetText(tab).AppendText($"{s}\r\n");//This should scroll to the bottom, if not, try this:

				if (focus)
				{
					var sel = GetTab(tab);

					if (sel != null)
						tcMain.SelectedTab = sel;
				}
			}, false, false);
		}

		public void ClearText(MainFocusedTab tab) => SetText(string.Empty, tab, false);

		public void SetText(string s, MainFocusedTab tab, bool focus)
		{
			_ = this.BeginInvoke(() => //These need to be BeginInvoke(), otherwise they can freeze if called within a COM event.
			{
				GetText(tab).Text = s;

				if (focus)
				{
					var sel = GetTab(tab);

					if (sel != null)
						tcMain.SelectedTab = sel;
				}
			});
		}

		internal void ListHotkeys()
		{
			_ = this.BeginInvoke(() =>
			{
				ShowIfNeeded();
				SetTextInternal(HotkeyDefinition.GetHotkeyDescriptions(), MainFocusedTab.Hotkeys, txtHotkeys, true);
			});
		}

		internal void ShowDebug()
		{
			_ = this.BeginInvoke(() =>
			{
				ShowIfNeeded();
				tcMain.SelectedTab = tpDebug;
			});
		}

		internal void ShowHistory()
		{
			_ = this.BeginInvoke(() =>
			{
				ShowIfNeeded();
				SetTextInternal(Core.Debug.ListKeyHistory(), MainFocusedTab.History, txtHistory, true);
			});
		}

		internal void ShowInternalVars(bool showTab)
		{
			callingInternalVars = true;//Gets called twice if called before first showing.
			_ = this.BeginInvoke(() =>
			{
				try
				{
					ShowIfNeeded();
					SetTextInternal(Core.Debug.GetVars(), MainFocusedTab.Vars, txtVars, showTab);
				}
				finally
				{
					callingInternalVars = false;
				}
			});
		}

		protected override void WndProc(ref Message m)
		{
			var handled = false;
#if WINDOWS

			switch (m.Msg)
			{
				case WindowsAPI.WM_CLIPBOARDUPDATE:
					if (clipSuccess)
						ClipboardUpdate?.Invoke(null);

					handled = true;
					break;

				case WindowsAPI.WM_COPYDATA:
				{
					if (Script.TheScript.GuiData.onMessageHandlers.TryGetValue(m.Msg, out var msgMonitor))//Needs to be handled here instead of MessageFilter because that one doesn't seem to intercept it.
					{
						var copyStruct = (WindowsAPI.COPYDATASTRUCT)m.GetLParam(typeof(WindowsAPI.COPYDATASTRUCT));
						_ = msgMonitor.funcs.InvokeEventHandlers(m.WParam.ToInt64(), copyStruct.lpData, m.Msg, m.HWnd.ToInt64());
						handled = true;
					}
				}
				break;

				case WindowsAPI.WM_ENDSESSION:
					_ = Flow.ExitAppInternal((m.Msg & WindowsAPI.ENDSESSION_LOGOFF) != 0 ? Flow.ExitReasons.LogOff : Flow.ExitReasons.Shutdown, null, false);
					handled = true;
					break;

				case WindowsAPI.WM_HOTKEY://We will need to find a cross platform way to do this. At the moment, hotkeys appear to be a built in feature in Windows.//TODO
					_ = Script.TheScript.HookThread.channel.Writer.TryWrite(new KeysharpMsg()
					{
						hwnd = m.HWnd,//Unused, but probably still good to assign.
						message = WindowsAPI.WM_HOTKEY,
						wParam = m.WParam,
						lParam = m.LParam,
					});
					handled = true;
					break;
			}

#endif
			base.WndProc(ref m);

			if (handled)
				m.Result = new nint(1);
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (about == null)
			{
				about = new AboutBox();
				about.FormClosing += (ss, ee) =>
				{
					about = null;
				};
			}

			about.Show();
		}

		private void clearDebugLogToolStripMenuItem_Click(object sender, EventArgs e) => txtDebug.Text = "";

		private void editScriptToolStripMenuItem_Click(object sender, EventArgs e) => Core.Debug.Edit();

		private void exitToolStripMenuItem_Click(object sender, EventArgs e) => _ = Flow.ExitAppInternal(Flow.ExitReasons.Menu, null, false);

		private TabPage GetTab(MainFocusedTab tab)
		{

			return tab switch
		{
				MainFocusedTab.Debug => tpDebug,
				MainFocusedTab.Vars => tpVars,
				MainFocusedTab.Hotkeys => tpHotkeys,
				MainFocusedTab.History => tpHistory,
				_ => tpDebug,
		};
	}

	private TextBox GetText(MainFocusedTab tab)
		{

			return tab switch
		{
				MainFocusedTab.Debug => txtDebug,
				MainFocusedTab.Vars => txtVars,
				MainFocusedTab.Hotkeys => txtHotkeys,
				MainFocusedTab.History => txtHistory,
				_ => txtDebug,
		};
	}

	private void hotkeysAndTheirMethodsToolStripMenuItem_Click(object sender, EventArgs e) => ListHotkeys();

		private void keyHistoryAndScriptInfoToolStripMenuItem_Click(object sender, EventArgs e) => ShowHistory();

		/// <summary>
		/// This will get called if the user manually closes the main window,
		/// or if ExitApp() is called from somewhere within the code, which will also close the main window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (string.IsNullOrEmpty(A_ExitReason as string) && e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
				Hide();
				return;
			}

			IsClosing = true;

			if (Flow.ExitAppInternal(Flow.ExitReasons.Close, null, false))
			{
				IsClosing = false;
				e.Cancel = true;
				return;
			}

#if WINDOWS

			if (clipSuccess)
				_ = WindowsAPI.RemoveClipboardFormatListener(Handle);

#elif LINUX
			//gtkClipBoard.OwnerChange -= gtkClipBoard_OwnerChange;
#endif
			Gui.DestroyAll();
			about?.Close();
		}

#if LINUX
		//private void gtkClipBoard_OwnerChange(object o, Gtk.OwnerChangeArgs args)
		//{
		//  ClipboardUpdate?.Invoke(null);
		//}
#endif

		private void MainWindow_Load(object sender, EventArgs e)
		{
			Visible = false;
			WindowState = FormWindowState.Minimized;
		}

		private void MainWindow_Shown(object sender, EventArgs e)
		{
		}

		private void MainWindow_SizeChanged(object sender, EventArgs e)
		{
			//Cannot call ShowInTaskbar at all here because it causes a full re-creation of the window.
			//So anything that previously used the window handle, including hotkeys, will no longer work.
			if (WindowState == FormWindowState.Minimized)
				Hide();
			else
				lastWindowState = WindowState;
		}

		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (tcMain.SelectedTab == tpVars)
				ShowInternalVars(true);
			else if (tcMain.SelectedTab == tpHotkeys)
				ListHotkeys();
			else if (tcMain.SelectedTab == tpHistory)
				ShowHistory();
		}

		private void reloadScriptToolStripMenuItem_Click(object sender, EventArgs e) => Flow.Reload();

		private void SetTextInternal(string text, MainFocusedTab tab, TextBox txt, bool focus)
		{
			//This can sometimes scroll the textbox on each update due to a fractional line being displayed.
			//This is an artifact of how the Winforms textbox works. You can see this by sizing the window
			//such that pressing F5 in the Vars tab keeps scrolling the textbox.
			//Then, click on the last line of text, you will see it scroll one line each time you click.
			var lineHeight = TextRenderer.MeasureText("X", txtVars.Font).Height;
			var linesPerPage = (double)txt.ClientSize.Height / lineHeight;
			var oldCharIndex = txt.GetCharIndexFromPosition(new Point(0, 0));
			var oldLineIndex = txt.GetLineFromCharIndex(oldCharIndex);
			SetText(text, tab, focus);
			var newCharIndex = oldLineIndex == 0 ? 0 : txt.GetFirstCharIndexFromLine(Math.Max(0, oldLineIndex + (int)linesPerPage));
			//txtDebug.Text += $"lineHeight: {lineHeight}, linesPerPage: {linesPerPage}, oldCharIndex: {oldCharIndex}, oldLineIndex: {oldLineIndex}, newCharIndex: {newCharIndex}\r\n";
			//This must be done with BeginInvoke() or else it won't reposition the scroll bars.
			_ = this.BeginInvoke(() =>
			{
				txt.Select(Math.Max(0, newCharIndex), 0);
				txt.ScrollToCaret();
			});
		}

		private void ShowIfNeeded()
		{
			if (!AllowShowDisplay || WindowState == FormWindowState.Minimized)
			{
				AllowShowDisplay = true;
				Show();
				BringToFront();
				WindowState = FormWindowState.Normal;
			}
		}

		private void suspendHotkeysToolStripMenuItem_Click(object sender, EventArgs e) => Script.SuspendHotkeys();

		private void TpVars_HandleCreated(object sender, EventArgs e)
		{
			if (!callingInternalVars)
				ShowInternalVars(false);
		}

		private void userManualToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_ = Dialogs.MsgBox("This feature is not implemented");
		}

		private void variablesAndTheirContentsToolStripMenuItem_Click(object sender, EventArgs e) => ShowInternalVars(true);

		private void windowSpyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var path = Path.GetDirectoryName(A_KeysharpPath);
#if WINDOWS
			var exe = path + "/Keysharp.exe";
#else
			var exe = path + "/Keysharp";
#endif
			var opt = path + "/Scripts/WindowSpy.ks";
			object pid = 0;
			//Keysharp.Core.Dialogs.MsgBox(exe + "\r\n" + path + "\r\n" + opt);
			_ = Processes.Run("\"" + exe + "\"", path, "", ref pid, "\"" + opt + "\"");
		}

		public enum MainFocusedTab
		{
			Debug,
			Vars,
			Hotkeys,
			History
		}

		public event VariadicAction ClipboardUpdate;
	}

	/// <summary>
	/// Text boxes have a long standing behavior which is undesirable.
	/// They select all text whenever they get the focus.
	/// In order to prevent that, make a small derivation to do
	/// nothing on focus.
	/// https://github.com/dotnet/winforms/issues/5406
	/// </summary>
	internal class NonFocusTextBox : TextBox
	{
		protected override void OnGotFocus(EventArgs e)
		{
			return;
		}
	}
}