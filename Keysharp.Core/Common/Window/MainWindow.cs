using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Keysharp.Core.Common.Threading;
using Keysharp.Core.Windows;

namespace Keysharp.Scripting
{
	public partial class MainWindow : Form
	{
		public static Font OurDefaultFont = new System.Drawing.Font("Microsoft Sans Serif", 9F);
		internal FormWindowState lastWindowState = FormWindowState.Normal;
		private readonly bool success;
		//Unsure if this is what we want because Control already has a DefaultFont member, also will need a different one for linux.//MATT

		public MainWindow()
		{
			InitializeComponent();
			SetStyle(ControlStyles.StandardClick, true);
			SetStyle(ControlStyles.StandardDoubleClick, true);
			success = WindowsAPI.AddClipboardFormatListener(Handle);
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
			if (m.Msg == WindowsAPI.WM_CLIPBOARDUPDATE)
			{
				if (success)
				{
					ClipboardUpdate?.Invoke(null);
				}
			}

			base.WndProc(ref m);
		}

		private void editScriptToolStripMenuItem_Click(object sender, System.EventArgs e) => Script.Edit();

		private void exitToolStripMenuItem_Click(object sender, System.EventArgs e) => Keysharp.Core.Flow.ExitApp();

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

		private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
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

		public event ClipUpdateDel ClipboardUpdate;
	}
}