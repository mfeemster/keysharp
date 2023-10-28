using System;
using System.Drawing;
using System.Windows.Forms;
using Keysharp.Core;
using Keysharp.Core.Common.Keyboard;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		internal static ToolStripMenuItem openMenuItem;
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

			if (NoTrayIcon)
				return;

			trayMenu = new Menu(Tray.ContextMenuStrip);
			trayMenu.AddStandard();
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
				PausedIcon = Keysharp.Core.Properties.Resources.Keysharp_p_ico;//Pause isn't really needed since pausing is not supported. Perhaps it can be used for something else some day.
				SuspendedIcon = Keysharp.Core.Properties.Resources.Keysharp_s_ico;
			}
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