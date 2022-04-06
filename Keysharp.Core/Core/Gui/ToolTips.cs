using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Keysharp.Core.Common;
using Keysharp.Core.Windows;
using Keysharp.Scripting;

namespace Keysharp.Core
{
	public static class ToolTips
	{
		private static ToolTip[] persistentTooltips = new System.Windows.Forms.ToolTip[20];

		public static void ToolTip(params object[] obj)
		{
			var (text, x, y, id) = obj.L().Si3("", int.MinValue, int.MinValue, 1);
			id--;

			if (text != "")
			{
				var tooltipForm = GuiHelper.DialogOwner ?? Form.ActiveForm;

				if (tooltipForm == null)
					tooltipForm = Script.mainWindow;

				if (tooltipForm == null)
					return;

				tooltipForm.CheckedBeginInvoke(() =>
				{
					var coord = Mouse.Coords.Tooltip;

					if (x == int.MinValue || y == int.MinValue)
					{
						var mouse = System.Windows.Forms.Cursor.Position;
						x = mouse.X;
						y = mouse.Y;
						coord = CoordModeType.Screen;
					}

					if (coord == CoordModeType.Screen)//Consolidate this into a function since it'll be used in several places.//MATT
					{
						var borderWidth = (tooltipForm.Width - tooltipForm.ClientSize.Width) / 2;
						var p = tooltipForm.PointToClient(new Point(x, y));
						x = p.X + borderWidth;
						y = p.Y + (tooltipForm.Height - tooltipForm.ClientSize.Height - borderWidth);
					}

					if (persistentTooltips[id] == null)
						persistentTooltips[id] = new ToolTip { Active = true, AutomaticDelay = 0, InitialDelay = 0, ReshowDelay = 0, ShowAlways = true };

					persistentTooltips[id].Active = true;

					persistentTooltips[id].Show(text, tooltipForm, new Point(x, y));
				});
			}
			else
			{
				if (persistentTooltips[id] != null)
					persistentTooltips[id].Active = false;
			}
		}

		public static void TraySetIcon(params object[] obj)
		{
			var (filename, iconnumber, freeze) = obj.L().Sis("", 1);

			if (Parser.NoTrayIcon)
				return;

			if (freeze != "")
				Accessors.A_IconFrozen = Options.OnOff(freeze) ?? false;

			if (filename != "*")
			{
				var bmp = ImageHelper.LoadImage(filename, 0, 0, --iconnumber);

				if (Script.Tray == null)
					Script.CreateTrayMenu();

				if (bmp != null)
				{
					var ptr = bmp.GetHicon();
					var icon = Icon.FromHandle(ptr);

					if (icon != null)
					{
						Script.Tray.Icon = icon;
						Accessors.A_IconFile = filename;
						Accessors.A_IconNumber = iconnumber;
					}

					_ = WindowsAPI.DestroyIcon(ptr);
				}
			}
			else
			{
				Accessors.A_IconFile = "";
				Accessors.A_IconNumber = 1;
				Script.Tray.Icon = Keysharp.Core.Properties.Resources.Keysharp_ico;
			}
		}

		public static void TrayTip(params object[] obj)
		{
			var (text, title, options) = obj.L().S2o();

			if (Parser.NoTrayIcon)
				return;

			if ((bool)Accessors.A_IconHidden)
				return;

			if (Script.Tray == null)
				Script.CreateTrayMenu();

			if (text?.Length == 0 && title?.Length == 0)
			{
				Script.Tray.Visible = false;
				return;
			}

			var duration = 5;
			var icon = ToolTipIcon.None;
			void HandleInt(object o)
			{
				var i = o.ParseInt();

				if ((i & 4) == 4) { }//tray icon
				else if ((i & 3) == 3) { icon = ToolTipIcon.Error; }
				else if ((i & 2) == 2) { icon = ToolTipIcon.Warning; }
				else if ((i & 1) == 1) { icon = ToolTipIcon.Info; }
				else if ((i & 16) == 16) { }
				else if ((i & 32) == 32) { }
			}

			if (options is string s)
			{
				foreach (var opt in Options.ParseOptions(s.ToLower()))
				{
					var temp = 0;

					if (opt == "iconi") icon = ToolTipIcon.Info;
					else if (opt == "icon!") icon = ToolTipIcon.Warning;
					else if (opt == "iconx") icon = ToolTipIcon.Error;
					else if (opt == "mute") { }
					else if (Options.TryParse(opt, "dur", ref temp)) { duration = temp; }
					else HandleInt(opt);
				}
			}
			else if (options != null)
				HandleInt(options);

			Script.Tray.Visible = true;
			Script.Tray.ShowBalloonTip(duration * 1000, title, text, icon);
		}
	}
}