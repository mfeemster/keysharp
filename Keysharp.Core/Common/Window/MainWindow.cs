using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Keysharp.Core;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Common.Threading;
using Keysharp.Core.Common.Window;
using Keysharp.Core.Windows;
using static Keysharp.Core.Misc;

namespace Keysharp.Scripting
{
	public partial class MainWindow : KeysharpForm
	{
		public static Font OurDefaultFont = new System.Drawing.Font("Microsoft Sans Serif", 9F);
		internal FormWindowState lastWindowState = FormWindowState.Normal;
		private readonly bool success;
		private AboutBox about;
		private bool callingInternalVars = false;

		public bool IsClosing { get; private set; }

		internal System.Windows.Forms.ToolStripMenuItem SuspendHotkeysToolStripMenuItem => suspendHotkeysToolStripMenuItem;

		public MainWindow()
		{
			InitializeComponent();
			//FormBorderStyle = FormBorderStyle.SizableToolWindow;
			SetStyle(ControlStyles.StandardClick, true);
			SetStyle(ControlStyles.StandardDoubleClick, true);
			SetStyle(ControlStyles.EnableNotifyMessage, true);
#if WINDOWS
			success = WindowsAPI.AddClipboardFormatListener(Handle);//Need a cross platform way to do this.//TODO
#else
			success = true;
#endif
			tpVars.HandleCreated += TpVars_HandleCreated;
			//          ThreadId = WindowsAPI.GetCurrentThreadId();
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
			this.BeginInvoke(() =>//These need to be BeginInvoke(), otherwise they can freeze if called within a COM event.
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
			this.BeginInvoke(() =>
			{
				ShowIfNeeded();
				SetTextInternal(HotkeyDefinition.GetHotkeyDescriptions(), MainFocusedTab.Hotkeys, txtHotkeys, true);
			});
		}

		internal void ShowDebug()
		{
			this.BeginInvoke(() =>
			{
				ShowIfNeeded();
				tcMain.SelectedTab = tpDebug;
			});
		}

		internal void ShowHistory()
		{
			this.BeginInvoke(() =>
			{
				ShowIfNeeded();
				SetTextInternal(Keysharp.Scripting.Script.ListKeyHistory(), MainFocusedTab.History, txtHistory, true);
			});
		}

		internal void ShowInternalVars(bool showTab)
		{
			try
			{
				callingInternalVars = true;//Gets called twice if called before first showing.
				this.BeginInvoke(() =>
				{
					ShowIfNeeded();
					SetTextInternal(Script.GetVars(), MainFocusedTab.Vars, txtVars, showTab);
				});
			}
			finally
			{
				callingInternalVars = false;
			}
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
					_ = Core.Flow.ExitAppInternal((m.Msg & WindowsAPI.ENDSESSION_LOGOFF) != 0 ? Core.Flow.ExitReasons.LogOff : Core.Flow.ExitReasons.Shutdown);
					handled = true;
					break;

				case WindowsAPI.WM_HOTKEY://We will need to find a cross platform way to do this. At the moment, hotkeys appear to be a built in feature in Windows.//TODO
					//Sadly, we cannot make this method async, so this will just be fire and forget.
					var tv = Threads.GetThreadVariables();
					tv.WaitForCriticalToFinish();//Must wait until the previous critical task finished before proceeding.
					Keysharp.Scripting.Script.HookThread.kbdMsSender.ProcessHotkey(m.WParam.ToInt32(), m.LParam.ToInt32(), null, WindowsAPI.WM_HOTKEY);
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

		private void editScriptToolStripMenuItem_Click(object sender, System.EventArgs e) => Script.Edit();

		private void exitToolStripMenuItem_Click(object sender, System.EventArgs e) => Keysharp.Core.Flow.ExitAppInternal(Core.Flow.ExitReasons.Exit);

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

		private System.Windows.Forms.TextBox GetText(MainFocusedTab tab)
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

		private void hotkeysAndTheirMethodsToolStripMenuItem_Click(object sender, System.EventArgs e) => ListHotkeys();

		private void keyHistoryAndScriptInfoToolStripMenuItem_Click(object sender, System.EventArgs e) => ShowHistory();

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

			if (Keysharp.Core.Flow.ExitAppInternal(Keysharp.Core.Flow.ExitReasons.Close))
			{
				IsClosing = false;
				e.Cancel = true;
				return;
			}

			if (success)
				_ = WindowsAPI.RemoveClipboardFormatListener(Handle);

			Keysharp.Core.Gui.DestroyAll();
			about?.Close();
		}

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

		private void refreshToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			if (tcMain.SelectedTab == tpVars)
				ShowInternalVars(true);
			else if (tcMain.SelectedTab == tpHotkeys)
				ListHotkeys();
			else if (tcMain.SelectedTab == tpHistory)
				ShowHistory();
		}

		private void reloadScriptToolStripMenuItem_Click(object sender, System.EventArgs e) => Keysharp.Core.Flow.Reload();

		private void SetTextInternal(string text, MainFocusedTab tab, System.Windows.Forms.TextBox txt, bool focus)
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
			if (WindowState == FormWindowState.Minimized)
			{
				Show();
				WindowState = FormWindowState.Normal;
			}
		}

		private void suspendHotkeysToolStripMenuItem_Click(object sender, System.EventArgs e) => Keysharp.Scripting.Script.SuspendHotkeys();

		private void TpVars_HandleCreated(object sender, System.EventArgs e)
		{
			if (!callingInternalVars)
				ShowInternalVars(false);
		}

		private void userManualToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			Keysharp.Core.Dialogs.MsgBox("This feature is not implemented");
		}

		private void variablesAndTheirContentsToolStripMenuItem_Click(object sender, System.EventArgs e) => ShowInternalVars(true);

		private void windowSpyToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			var path = System.IO.Path.GetDirectoryName(Accessors.A_KeysharpPath);
			var exe = path + "/Keysharp.exe";
			var opt = path + "/Scripts/WindowSpy.ks";
			object pid = 0;
			//Keysharp.Core.Dialogs.MsgBox(exe + "\r\n" + path + "\r\n" + opt);
			_ = Core.Processes.Run("\"" + exe + "\"", path, "", ref pid, "\"" + opt + "\"");
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