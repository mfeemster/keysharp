using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Keysharp.Core;
using Keysharp.Core.Windows;

namespace Keysharp.Scripting
{
	partial class Script
	{
		internal static Menu trayMenu;

		public static void CreateTrayMenu()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)//Hopefully this is possible on non-windows OSes.//TODO
				return;

			if (Parser.NoTrayIcon)
				return;

			var trayIcon = Core.Core.Tray = new NotifyIcon { ContextMenuStrip = new ContextMenuStrip(), Text = Accessors.A_ScriptName.Substring(0, Math.Min(Accessors.A_ScriptName.Length, 64)) };//System tray icon tooltips have a limit of 64 characters.
			trayMenu = new Menu
			{
				MenuItem = Core.Core.Tray.ContextMenuStrip

			};
			_ = trayMenu.Add("&Open", (GenericFunction)(obj =>
			{
				if (mainWindow != null)
				{
					mainWindow.Show();
					mainWindow.BringToFront();
					mainWindow.WindowState = mainWindow.lastWindowState;
				}

				return "";
			}));
			//Need to properly fill in all of these event handlers when the proper functionality is implemented.
			_ = trayMenu.Add("&Help", (GenericFunction)(obj =>
			{
				return "";
			}));
			_ = trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			_ = trayMenu.Add("&Window Spy", (GenericFunction)(obj =>
			{
				return "";
			}));
			_ = trayMenu.Add("&Reload This Script", (GenericFunction)(obj =>
			{
				Application.Restart();
				return "";
			}));
			_ = trayMenu.Add("&Edit This Script", (GenericFunction)(obj =>
			{
				Edit();
				return "";
			}));
			_ = trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			_ = trayMenu.Add("&Suspend Hotkeys", (GenericFunction)(obj =>
			{
				return "";
			}));
			_ = trayMenu.Add("&Pause Script", (GenericFunction)(obj =>
			{
				return "";
			}));
			_ = trayMenu.Add("&Exit", (GenericFunction)(obj =>
			{
				Accessors.A_ExitReason = "OnExit()";
				Keysharp.Core.Flow.ExitApp();
				return "";
			}));
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

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)//Not sure whether resources work on non-windows implementations of C#.
			{
				PausedIcon = Keysharp.Core.Properties.Resources.Keysharp_p;
				SuspendedIcon = Keysharp.Core.Properties.Resources.Keysharp_s;
			}

			Flow.ApplicationExit += (o, e) =>
			{
				if (Core.Core.Tray != null)
				{
					Core.Core.Tray.Visible = false;
					Core.Core.Tray.Dispose();
				}
			};
			isReadyToExecute = true;//Not sure if this should go somewhere else, but put it here for now.//TODO
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