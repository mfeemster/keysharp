using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Keysharp.Core;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Windows;
using static Keysharp.Core.Misc;

namespace Keysharp.Scripting
{
	public partial class MainWindow : KeysharpForm
	{
		public static Font OurDefaultFont = new System.Drawing.Font("Microsoft Sans Serif", 9F);
		internal FormWindowState lastWindowState = FormWindowState.Normal;
		private readonly bool success;

		public bool IsClosing { get; private set; }
		//public uint ThreadId { get; private set; }

		public MainWindow()
		{
			InitializeComponent();
			SetStyle(ControlStyles.StandardClick, true);
			SetStyle(ControlStyles.StandardDoubleClick, true);
			SetStyle(ControlStyles.EnableNotifyMessage, true);
			success = WindowsAPI.AddClipboardFormatListener(Handle);//Need a cross platform way to do this.//TODO
			tpVars.HandleCreated += TpVars_HandleCreated;
			//          ThreadId = WindowsAPI.GetCurrentThreadId();
		}

		public void AddText(string s, MainFocusedTab tab)
		{
			this.Invoke(() =>
			{
				GetText(tab).AppendText($"{s}\r\n");//This should scroll to the bottom, if not, try this:
				//txt.SelectionStart = txt.TextLength;
				//txt.ScrollToCaret();
				tcMain.SelectedTab = GetTab(tab);
			});
		}

		public void ClearText(MainFocusedTab tab) => SetText(string.Empty, tab);

		public void SetText(string s, MainFocusedTab tab)
		{
			this.Invoke(() =>
			{
				GetText(tab).Text = s;
				tcMain.SelectedTab = GetTab(tab);
			});
		}

		//protected override async void OnNotifyMessage(Message m)
		//{
		//  if (GuiHelper.onMessageHandlers.TryGetValue(m.Msg, out var handlers))
		//  {
		//      var res = handlers.InvokeEventHandlers(m.WParam.ToInt64(), m.LParam.ToInt64(), m.Msg, m.HWnd.ToInt64());
		//
		//      if (res.IsNotNullOrEmpty())
		//          return;
		//  }
		//
		//  switch (m.Msg)
		//  {
		//      case WindowsAPI.WM_CLIPBOARDUPDATE:
		//          if (success)
		//              ClipboardUpdate?.Invoke(null);
		//
		//          break;
		//
		//      case WindowsAPI.WM_ENDSESSION:
		//          _ = Core.Flow.ExitAppInternal((m.Msg & WindowsAPI.ENDSESSION_LOGOFF) != 0 ? Core.Flow.ExitReasons.LogOff : Core.Flow.ExitReasons.Shutdown);
		//          break;
		//
		//      case WindowsAPI.WM_HOTKEY://We will need to find a cross platform way to do this. At the moment, hotkeys appear to be a built in feature in Windows.//TODO
		//          _ = await Keysharp.Scripting.Script.HookThread.kbdMsSender.ProcessHotkey(m.WParam.ToInt32(), m.LParam.ToInt32(), WindowsAPI.WM_HOTKEY);
		//          break;
		//  }
		//
		//  //else if (m.Msg == WindowsAPI.WM_DESTROY && m.HWnd == Handle)//AHK looked for WM_DESTROY, but we seem to get many of them, so it probably won't work here. It's ok because it's a corner case anyway.
		//  //{
		//  //  Keysharp.Core.Flow.ExitAppInternal(Keysharp.Core.Flow.ExitReasons.Destroy);
		//  //}
		//}

		protected override void WndProc(ref Message m)
		{
			//  switch (m.Msg)
			//  {
			//      case WindowsAPI.WM_CLIPBOARDUPDATE:
			//          if (success)
			//              ClipboardUpdate?.Invoke(null);
			//          break;
			//      case WindowsAPI.WM_ENDSESSION:
			//          _ = Core.Flow.ExitAppInternal((m.Msg & WindowsAPI.ENDSESSION_LOGOFF) != 0 ? Core.Flow.ExitReasons.LogOff : Core.Flow.ExitReasons.Shutdown);
			//          break;
			//      case WindowsAPI.WM_HOTKEY://We will need to find a cross platform way to do this. At the moment, hotkeys appear to be a built in feature in Windows.//TODO
			//                                //_ = await Keysharp.Scripting.Script.HookThread.kbdMsSender.ProcessHotkey(m.WParam.ToInt32(), m.LParam.ToInt32(), WindowsAPI.WM_HOTKEY);
			//          break;
			//  }
			//else if (m.Msg == WindowsAPI.WM_DESTROY && m.HWnd == Handle)//AHK looked for WM_DESTROY, but we seem to get many of them, so it probably won't work here. It's ok because it's a corner case anyway.
			//  {
			//      Keysharp.Core.Flow.ExitAppInternal(Keysharp.Core.Flow.ExitReasons.Destroy);
			//  }
			//if (GuiHelper.onMessageHandlers.TryGetValue(m.Msg, out var handlers))
			//{
			//  var res = handlers.InvokeEventHandlers(m.WParam.ToInt64(), m.LParam.ToInt64(), m.Msg, m.HWnd.ToInt64());
			//  if (res.IsNotNullOrEmpty())
			//      return;
			//}
			var handled = false;

			switch (m.Msg)
			{
				case WindowsAPI.WM_CLIPBOARDUPDATE:
					if (success)
						ClipboardUpdate?.Invoke(null);

					handled = true;
					break;

				case WindowsAPI.WM_COPYDATA:
				{
					if (GuiHelper.onMessageHandlers.TryGetValue(m.Msg, out var handlers))//Needs to be handled here instead of MessageFilter because that one doesn't seem to intercept it.
					{
						var mystr = (WindowsAPI.COPYDATASTRUCT)m.GetLParam(typeof(WindowsAPI.COPYDATASTRUCT));
						_ = handlers.InvokeEventHandlers(m.WParam.ToInt64(), mystr.lpData, m.Msg, m.HWnd.ToInt64());
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
					_ = Keysharp.Scripting.Script.HookThread.kbdMsSender.ProcessHotkey(m.WParam.ToInt32(), m.LParam.ToInt32(), WindowsAPI.WM_HOTKEY);
					handled = true;
					break;
			}

			base.WndProc(ref m);

			if (handled)
				m.Result = new IntPtr(1);
		}

		private void editScriptToolStripMenuItem_Click(object sender, System.EventArgs e) => Script.Edit();
		private void exitToolStripMenuItem_Click(object sender, System.EventArgs e) => Keysharp.Core.Flow.ExitAppInternal(Core.Flow.ExitReasons.Exit);
		private TabPage GetTab(MainFocusedTab tab)
		{
			switch (tab)
			{
				case MainFocusedTab.Main:
					return tpMain;

				case MainFocusedTab.Debug:
					return tpDebug;

				case MainFocusedTab.Vars:
					return tpVars;

				default:
					return tpMain;
			}
		}
		private TextBox GetText(MainFocusedTab tab)
		{
			switch (tab)
			{
				case MainFocusedTab.Main:
					return txtMain;

				case MainFocusedTab.Debug:
					return txtDebug;

				case MainFocusedTab.Vars:
					return txtVars;

				default:
					return txtMain;
			}
		}
		private void hotkeysAndTheirMethodsToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
		}
		private void keyHistoryAndScriptInfoToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
		}
		/// <summary>
		/// This will get called if the user manually closes the main window,
		/// or if ExitApp() is called from somewhere within the code, which will also close the main window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			IsClosing = true;

			if (Keysharp.Core.Flow.ExitAppInternal(Keysharp.Core.Flow.ExitReasons.Close))
			{
				IsClosing = false;
				e.Cancel = true;
				return;
			}

			if (success)
				_ = WindowsAPI.RemoveClipboardFormatListener(Handle);
		}
		private void MainWindow_Load(object sender, EventArgs e)
		{
		}
		private void MainWindow_Shown(object sender, EventArgs e)
		{
		}
		private void MainWindow_SizeChanged(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				ShowInTaskbar = false;
			}
			else
			{
				lastWindowState = WindowState;
				ShowInTaskbar = true;
			}
		}
		private void pauseScriptToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
		}
		private void refreshToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			if (tcMain.SelectedTab == tpVars)
			{
				ShowInternalVars();
			}
		}
		private void reloadScriptToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
		}
		private void ShowInternalVars()
		{
			var sb = new StringBuilder();
			var lineHeight = TextRenderer.MeasureText("X", txtVars.Font).Height;
			var linesPerPage = 1.0 * txtVars.ClientSize.Height / lineHeight;
			var oldCharIndex = txtVars.GetCharIndexFromPosition(new Point(0, 0));//Magic number, it scrolls backward on each update with smaller numbers.
			var oldLineIndex = txtVars.GetLineFromCharIndex(oldCharIndex);//Magic number, it scrolls backward on each update with smaller numbers.

			foreach (var item in Reflections.loadedAssemblies.Values.Where(assy => assy.FullName.StartsWith("Keysharp.Core,")))
			{
				foreach (var type in item.GetTypes().Where(t => t.IsPublic && t.IsClass && t.Namespace != null && t.Namespace.StartsWith("Keysharp.Core")))
				{
					var props = type.GetProperties(BindingFlags.Public | BindingFlags.Static);

					if (props.Length <= 0)
						continue;

					var list = new SortedDictionary<string, PropertyInfo>();

					foreach (var p in props)
						list[p.Name] = p;

					_ = sb.AppendLine($"{type.Name}:");
					_ = sb.AppendLine();

					foreach (var prop in list.Values)
					{
						var val = prop.GetValue(null);
						//var proptype = val != null ? val.GetType().Name : prop.PropertyType.Name;//If you ever want to see the types, add this back in.

						if (val is Keysharp.Core.Array arr)
						{
							var tempsb = new StringBuilder(arr.Count * 100);

							for (var a = 1; a <= arr.Count; a++)
							{
								var tempstr = arr[a].ToString();
								_ = tempsb.Append(tempstr);

								if (a < arr.Count)
									_ = tempsb.Append(", ");
							}

							_ = sb.AppendLine($"\t{prop.Name}: {(tempsb.Length > 0 ? tempsb.ToString() : "Empty")}");
						}
						else if (val is Keysharp.Core.Map map)
						{
							var a = 0;
							var tempsb = new StringBuilder(map.Count * 100);
							_ = tempsb.Append('{');

							foreach (var kv in map.map)
							{
								var tempstr = kv.Value.ToString();
								_ = tempsb.Append($"{kv.Key} : {tempstr}");

								if (++a < map.Count)
									_ = tempsb.Append(", ");
							}

							_ = tempsb.Append('}');
							_ = sb.AppendLine($"\t{prop.Name}: {(tempsb.Length > 0 ? tempsb.ToString() : "Empty")}");
						}
						else
							_ = sb.AppendLine($"\t{prop.Name}: {val}");
					}

					_ = sb.AppendLine("--------------------------------------------------");
					_ = sb.AppendLine();
				}
			}

			SetText(sb.ToString(), MainFocusedTab.Vars);
			var newCharIndex = txtVars.GetFirstCharIndexFromLine(Math.Max(0, oldLineIndex + (int)linesPerPage));
			txtVars.Select(Math.Max(0, newCharIndex), 0);
			txtVars.ScrollToCaret();
		}

		private void suspendHotkeysToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			Keysharp.Scripting.Script.SuspendHotkeys();
		}
		private void TpVars_HandleCreated(object sender, System.EventArgs e) => ShowInternalVars();
		private void userManualToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
		}
		private void variablesAndTheirContentsToolStripMenuItem_Click(object sender, System.EventArgs e) => Script.ListVars();
		private void websiteToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
		}
		private void windowSpyToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
		}
		public enum MainFocusedTab
		{
			Main,
			Debug,
			Vars
		}
		public event VariadicAction ClipboardUpdate;
	}
}