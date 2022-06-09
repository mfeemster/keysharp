using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Windows;
using static Keysharp.Core.Misc;

namespace Keysharp.Scripting
{
	public partial class MainWindow : Form
	{
		public static Font OurDefaultFont = new System.Drawing.Font("Microsoft Sans Serif", 9F);
		internal FormWindowState lastWindowState = FormWindowState.Normal;
		private readonly bool success;

		public bool IsClosing { get; private set; }

		public MainWindow()
		{
			InitializeComponent();
			SetStyle(ControlStyles.StandardClick, true);
			SetStyle(ControlStyles.StandardDoubleClick, true);
			success = WindowsAPI.AddClipboardFormatListener(Handle);//Need a cross platform way to do this.//TODO
			tpVars.HandleCreated += TpVars_HandleCreated;
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

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WindowsAPI.WM_CLIPBOARDUPDATE:
					if (success)
						ClipboardUpdate?.Invoke(null);

					break;

				case WindowsAPI.WM_ENDSESSION:
					_ = Core.Flow.ExitAppInternal((m.Msg & WindowsAPI.ENDSESSION_LOGOFF) != 0 ? Core.Flow.ExitReasons.LogOff : Core.Flow.ExitReasons.Shutdown);
					break;

				case WindowsAPI.WM_HOTKEY://We will need to find a cross platform way to do this. At the moment, hotkeys appear to be a built in feature in Windows.//TODO
				{
					var wParamVal = m.WParam.ToInt32() & HotkeyDefinition.HOTKEY_ID_MASK;
					var lParamVal = m.LParam.ToInt32();
					var hkId = wParamVal;

					if (hkId < HotkeyDefinition.shk.Count)//Ensure hotkey ID is valid.
					{
						var hk = HotkeyDefinition.shk[hkId];
						// Check if criterion allows firing.
						// For maintainability, this is done here rather than a little further down
						// past the g_MaxThreadsTotal and thread-priority checks.  Those checks hardly
						// ever abort a hotkey launch anyway.
						//
						// If message is WM_HOTKEY, it's either:
						// 1) A joystick hotkey from TriggerJoyHotkeys(), in which case the lParam is ignored.
						// 2) A hotkey message sent by the OS, in which case lParam contains currently-unused info set by the OS.
						//
						// An incoming WM_HOTKEY can be subject to #HotIf Win. at this stage under the following conditions:
						// 1) Joystick hotkey, because it relies on us to do the check so that the check is done only
						//    once rather than twice.
						// 2) #HotIf Win. keybd hotkeys that were made non-hook because they have a non-suspended, global variant.
						//
						// If message is AHK_HOOK_HOTKEY:
						// Rather than having the hook pass the qualified variant to us, it seems preferable
						// to search through all the criteria again and rediscover it.  This is because conditions
						// may have changed since the message was posted, and although the hotkey might still be
						// eligible for firing, a different variant might now be called for (e.g. due to a change
						// in the active window).  Since most criteria hotkeys have at most only a few criteria,
						// and since most such criteria are #HotIf WinActive rather than Exist, the performance will
						// typically not be reduced much at all.  Furthermore, trading performance for greater
						// reliability seems worth it in this case.
						//
						// The inefficiency of calling HotCriterionAllowsFiring() twice for each hotkey --
						// once in the hook and again here -- seems justified for the following reasons:
						// - It only happens twice if the hotkey a hook hotkey (multi-variant keyboard hotkeys
						//   that have a global variant are usually non-hook, even on NT/2k/XP).
						// - The hook avoids doing its first check of WinActive/Exist if it sees that the hotkey
						//   has a non-suspended, global variant.  That way, hotkeys that are hook-hotkeys for
						//   reasons other than #HotIf Win. (such as mouse, overriding OS hotkeys, or hotkeys
						//   that are too fancy for RegisterHotkey) will not have to do the check twice.
						// - It provides the ability to set the last-found-window for #HotIf WinActive/Exist
						//   (though it's not needed for the "Not" counterparts).  This HWND could be passed
						//   via the message, but that would require malloc-there and free-here, and might
						//   result in memory leaks if its ever possible for messages to get discarded by the OS.
						// - It allows hotkeys that were eligible for firing at the time the message was
						//   posted but that have since become ineligible to be aborted.  This seems like a
						//   good precaution for most users/situations because such hotkey subroutines will
						//   often assume (for scripting simplicity) that the specified window is active or
						//   exists when the subroutine executes its first line.
						// - Most criterion hotkeys use #HotIf WinActive(), which is a very fast call.  Also, although
						//   WinText and/or "SetTitleMatchMode 'Slow'" slow down window searches, those are rarely
						//   used too.
						//
						HotkeyVariant variant = null; // Set default.
						var variant_id = 0;
						/*

						    // For #HotIf hotkey variants, we don't want to evaluate the expression a second time. If the hook
						    // thread determined that a specific variant should fire, it is passed via the high word of wParam:
						    if ((variant_id = Conversions.HighWord((int)wParamVal)) != 0)
						    {
						    // The following relies on the fact that variants can't be removed or re-ordered;
						    // variant_id should always be the variant's one-based index in the linked list:
						    --variant_id; // i.e. index 1 should be mFirstVariant, not mFirstVariant->mNextVariant.

						    for (variant = hk.firstVariant; variant_id != 0; variant = variant.nextVariant, --variant_id)
						    {
						    }
						    }

						    char? dummy = null;

						    if (!(variant != null || (variant = hk.CriterionAllowsFiring(ref criterion_found_hwnd
						                                    , msg.message == (uint)UserMessages.AHK_HOOK_HOTKEY ? KeyboardMouseSender.KeyIgnoreLevel((uint)Conversions.HighWord((int)lParamVal)) : 0, ref dummy)) != null))
						    continue; // No criterion is eligible, so ignore this hotkey event (see other comments).

						    // If this is AHK_HOOK_HOTKEY, criterion was eligible at time message was posted,
						    // but not now.  Seems best to abort (see other comments).
						    // Due to the key-repeat feature and the fact that most scripts use a value of 1
						    // for their #MaxThreadsPerHotkey, this check will often help average performance
						    // by avoiding a lot of unnecessary overhead that would otherwise occur:
						    if (!hk.PerformIsAllowed(variant))
						    {
						    // The key is buffered in this case to boost the responsiveness of hotkeys
						    // that are being held down by the user to activate the keyboard's key-repeat
						    // feature.  This way, there will always be one extra event waiting in the queue,
						    // which will be fired almost the instant the previous iteration of the subroutine
						    // finishes (this above description applies only when MaxThreadsPerHotkey is 1,
						    // which it usually is).
						    variant.RunAgainAfterFinished(); // Wheel notch count (g->EventInfo below) should be okay because subsequent launches reuse the same thread attributes to do the repeats.
						    continue;
						    }

						    // Now that above has ensured variant is non-NULL:
						    var hc = variant.hotCriterion;

						    if (hc == null || hc.type == HotCriterionEnum.IfNotActive || hc.type == HotCriterionEnum.IfNotExist)
						    criterion_found_hwnd = IntPtr.Zero; // For "NONE" and "NOT", there is no last found window.
						    else if (HotkeyDefinition.HOT_IF_REQUIRES_EVAL(hc.type))
						    criterion_found_hwnd = Keysharp.Scripting.Script.hotExprLFW; // For #HotIf WinExist(WinTitle) and similar.

						    priority = variant.priority;
						    SetHotNamesAndTimes(hk.Name);

						    if (IsWheelVK(hk.vk)) // If this is true then also: msg.message==AHK_HOOK_HOTKEY
						    Accessors.A_EventInfo = lParamVal; // v1.0.43.03: Override the thread default of 0 with the number of notches by which the wheel was turned.

						    // Above also works for RunAgainAfterFinished since that feature reuses the same thread attributes set above.
						    Keysharp.Scripting.Script.hWndLastUsed = criterion_found_hwnd; // v1.0.42. Even if the window is invalid for some reason, IsWindow() and such are called whenever the script accesses it (GetValidLastUsedWindow()).
						    Accessors.A_SendLevel = variant.inputLevel;
						    Keysharp.Scripting.Script.hotCriterion = variant.hotCriterion; // v2: Let the Hotkey command use the criterion of this hotkey variant by default.
						    hk.PerformInNewThreadMadeByCaller(variant);
						*/
						break;
					}

					break;
				}
			}

			//else if (m.Msg == WindowsAPI.WM_DESTROY && m.HWnd == Handle)//AHK looked for WM_DESTROY, but we seem to get many of them, so it probably won't work here. It's ok because it's a corner case anyway.
			//{
			//  Keysharp.Core.Flow.ExitAppInternal(Keysharp.Core.Flow.ExitReasons.Destroy);
			//}
			base.WndProc(ref m);
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

			foreach (var item in AppDomain.CurrentDomain.GetAssemblies().Where(assy => assy.FullName.StartsWith("Keysharp.Core,")))
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