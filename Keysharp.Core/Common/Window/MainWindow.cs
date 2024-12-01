namespace Keysharp.Scripting
{
	public partial class MainWindow : KeysharpForm
	{
		public static Font OurDefaultFont = new ("Microsoft Sans Serif", 9F);
		internal FormWindowState lastWindowState = FormWindowState.Normal;
		private readonly bool success;
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
			success = true;
#elif WINDOWS
			success = WindowsAPI.AddClipboardFormatListener(Handle);//Need a cross platform way to do this.//TODO
#endif
			tpVars.HandleCreated += TpVars_HandleCreated;
			editScriptToolStripMenuItem.Visible = !Accessors.A_IsCompiled;
		}

		public void AddText(string s, MainFocusedTab tab, bool focus)
		{
			//Use CheckedBeginInvoke() because CheckedInvoke() seems to crash if this is called right as the window is closing.
			//Such as with a hotkey that prints on mouse click, which will cause a print when the X is clicked to close.
			this.CheckedBeginInvoke(() =>
			{
				GetText(tab).AppendText($"{s}\r\n");//This should scroll to the bottom, if not, try this:

				//txt.SelectionStart = txt.TextLength;
				//txt.ScrollToCaret();
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
				SetTextInternal(Script.ListKeyHistory(), MainFocusedTab.History, txtHistory, true);
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
					SetTextInternal(Script.GetVars(), MainFocusedTab.Vars, txtVars, showTab);
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
					if (success)
						ClipboardUpdate?.Invoke(null);

					handled = true;
					break;

				case WindowsAPI.WM_COPYDATA:
				{
					if (GuiHelper.onMessageHandlers.TryGetValue(m.Msg, out var msgMonitor))//Needs to be handled here instead of MessageFilter because that one doesn't seem to intercept it.
					{
						var copyStruct = (WindowsAPI.COPYDATASTRUCT)m.GetLParam(typeof(WindowsAPI.COPYDATASTRUCT));
						_ = msgMonitor.funcs.InvokeEventHandlers(m.WParam.ToInt64(), copyStruct.lpData, m.Msg, m.HWnd.ToInt64());
						handled = true;
					}
				}
				break;

				case WindowsAPI.WM_ENDSESSION:
					_ = Flow.ExitAppInternal((m.Msg & WindowsAPI.ENDSESSION_LOGOFF) != 0 ? Flow.ExitReasons.LogOff : Flow.ExitReasons.Shutdown);
					handled = true;
					break;

				case WindowsAPI.WM_HOTKEY://We will need to find a cross platform way to do this. At the moment, hotkeys appear to be a built in feature in Windows.//TODO
					//Sadly, we cannot make this method async, so this will just be fire and forget.
					var tv = Threads.GetThreadVariables();
					tv.WaitForCriticalToFinish();//Must wait until the previous critical task finished before proceeding.
					Script.HookThread.kbdMsSender.ProcessHotkey(m.WParam.ToInt32(), m.LParam.ToInt32(), null, WindowsAPI.WM_HOTKEY);
					handled = true;
					break;
			}

#endif
			base.WndProc(ref m);

			if (handled)
				m.Result = new IntPtr(1);
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

		private void editScriptToolStripMenuItem_Click(object sender, EventArgs e) => Script.Edit();

		private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Flow.ExitAppInternal(Flow.ExitReasons.Exit);

		private TabPage GetTab(MainFocusedTab tab)
		{
			switch (tab)
			{
				case MainFocusedTab.Debug:
					return tpDebug;

				case MainFocusedTab.Vars:
					return tpVars;

				case MainFocusedTab.Hotkeys:
					return tpHotkeys;

				case MainFocusedTab.History:
					return tpHistory;

				default:
					return tpDebug;
			}
		}

		private TextBox GetText(MainFocusedTab tab)
		{
			switch (tab)
			{
				case MainFocusedTab.Debug:
					return txtDebug;

				case MainFocusedTab.Vars:
					return txtVars;

				case MainFocusedTab.Hotkeys:
					return txtHotkeys;

				case MainFocusedTab.History:
					return txtHistory;

				default:
					return txtDebug;
			}
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
			if (string.IsNullOrEmpty(Accessors.A_ExitReason as string) && e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
				Hide();
				return;
			}

			IsClosing = true;

			if (Flow.ExitAppInternal(Flow.ExitReasons.Close))
			{
				IsClosing = false;
				e.Cancel = true;
				return;
			}

#if WINDOWS

			if (success)
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
				this.Hide();
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
			var lineHeight = TextRenderer.MeasureText("X", txtVars.Font).Height;
			var linesPerPage = 1.0 * txtVars.ClientSize.Height / lineHeight;
			var oldCharIndex = txtVars.GetCharIndexFromPosition(new Point(0, 0));//Magic number, it scrolls backward on each update with smaller numbers.
			var oldLineIndex = txtVars.GetLineFromCharIndex(oldCharIndex);//Magic number, it scrolls backward on each update with smaller numbers.
			SetText(text, tab, focus);
			var newCharIndex = oldLineIndex == 0 ? 0 : txtVars.GetFirstCharIndexFromLine(Math.Max(0, oldLineIndex + (int)linesPerPage));
			txt.Select(Math.Max(0, newCharIndex), 0);
			txt.ScrollToCaret();
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
			var path = Path.GetDirectoryName(Accessors.A_KeysharpPath);
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
}