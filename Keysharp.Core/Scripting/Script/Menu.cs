using System;
using System.Drawing;
using System.Windows.Forms;
using Keysharp.Core;
using Keysharp.Core.Common.Keyboard;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		internal static ToolStripMenuItem pauseMenuItem;
		internal static ToolStripMenuItem suspendMenuItem;
		internal static NotifyIcon Tray;
		internal static Menu trayMenu;

		public static void CreateTrayMenu()
		{
			var trayIcon = Tray = new NotifyIcon { ContextMenuStrip = new ContextMenuStrip(), Text = Accessors.A_ScriptName.Substring(0, Math.Min(Accessors.A_ScriptName.Length, 64)) };//System tray icon tooltips have a limit of 64 characters.
			Keysharp.Core.Processes.mainContext = System.Threading.SynchronizationContext.Current;//This must happen after the icon is created.

			if (Environment.OSVersion.Platform != PlatformID.Win32NT)//Hopefully this is possible on non-windows OSes.//TODO
				return;

			if (Parser.NoTrayIcon)
				return;

			trayMenu = new Menu(Tray.ContextMenuStrip);
			var emptyfunc = new Func<object>(() => "");
			var openfunc = new Func<object>(() =>
			{
				if (mainWindow != null)
				{
					mainWindow.Show();
					mainWindow.BringToFront();
					mainWindow.WindowState = mainWindow.lastWindowState;
				}

				return "";
			});
			var reloadfunc = new Func<object>(() =>
			{
				Keysharp.Core.Flow.Reload();
				return "";
			});
			var editfunc = new Func<object>(() =>
			{
				Edit();
				return "";
			});
			var suspend = new Func<object>(() =>
			{
				SuspendHotkeys();
				return "";
			});
			var pause = new Func<object>(() =>
			{
				PauseThread();
				return "";
			});
			var exitfunc = new Func<object>(() =>
			{
				_ = Keysharp.Core.Flow.ExitAppInternal(Core.Flow.ExitReasons.Menu);
				return "";
			});
			_ = trayMenu.Add("&Open", new FuncObj(openfunc.Method, openfunc.Target));
			//Need to properly fill in all of these event handlers when the proper functionality is implemented.
			_ = trayMenu.Add("&Help", new FuncObj(emptyfunc.Method, emptyfunc.Target));
			_ = trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			//_ = trayMenu.Add("&Window Spy", new FuncObj(emptyfunc.Method, emptyfunc.Target));
			_ = trayMenu.Add("&Reload This Script", new FuncObj(reloadfunc.Method, reloadfunc.Target));

			if (!Accessors.A_IsCompiled)
				_ = trayMenu.Add("&Edit This Script", new FuncObj(editfunc.Method, editfunc.Target));

			_ = trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			suspendMenuItem = trayMenu.Add("&Suspend Hotkeys", new FuncObj(suspend.Method, suspend.Target));
			pauseMenuItem = trayMenu.Add("&Pause Script", new FuncObj(pause.Method, pause.Target));
			_ = trayMenu.Add("&Exit", new FuncObj(exitfunc.Method, exitfunc.Target));
			trayMenu.Default = "&Open";
			trayIcon.Tag = trayMenu;
			trayIcon.MouseClick += TrayIcon_MouseClick;
			trayIcon.MouseDoubleClick += TrayIcon_MouseDoubleClick;
			var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

			if (Accessors.A_IsCompiled || icon == null)//For some reason this is needed when running as a compiled exe.
				icon = Keysharp.Core.Properties.Resources.Keysharp_ico;

			if (icon != null)
			{
				trayIcon.Icon = icon;
				trayIcon.Visible = true;
			}

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)//Not sure whether resources work on non-windows implementations of C#.//TODO
			{
				PausedIcon = Keysharp.Core.Properties.Resources.Keysharp_p_ico;
				SuspendedIcon = Keysharp.Core.Properties.Resources.Keysharp_s_ico;
			}
		}

		internal static void PauseThread()
		{
			mainWindow.CheckedInvoke(() =>
			{
				mainWindow.PauseScriptToolStripMenuItem.Checked = !mainWindow.PauseScriptToolStripMenuItem.Checked;
				pauseMenuItem.Checked = Keysharp.Core.Flow.Suspended;
				Keysharp.Core.Flow.Pause(pauseMenuItem.Checked);
			}, false);
		}

		internal static void SuspendHotkeys()
		{
			mainWindow.CheckedInvoke(() =>
			{
				Keysharp.Core.Flow.Suspended = !Keysharp.Core.Flow.Suspended;
				HotstringDefinition.SuspendAll(Keysharp.Core.Flow.Suspended);//Must do this prior to ManifestAllHotkeysHotstringsHooks() to avoid incorrect removal of hook.
				HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();//Update the state of all hotkeys based on the complex interdependencies hotkeys have with each another.
				suspendMenuItem.Checked = Keysharp.Core.Flow.Suspended;
				mainWindow.SuspendHotkeysToolStripMenuItem.Checked = Keysharp.Core.Flow.Suspended;
			}, false);
		}

		private static void TrayIcon_MouseClick(object sender, MouseEventArgs e)
		{
			if (sender is NotifyIcon ni && ni.Tag is Menu mnu)
				if (mnu.ClickCount == 1)
					if (mnu.defaultItem is ToolStripItem tsi)
						mnu.Tsmi_Click(tsi, new EventArgs());
		}

		private static void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (sender is NotifyIcon ni && ni.Tag is Menu mnu)
				if (mnu.ClickCount > 1)
					if (mnu.defaultItem is ToolStripItem tsi)
						mnu.Tsmi_Click(tsi, new EventArgs());
		}
	}
}