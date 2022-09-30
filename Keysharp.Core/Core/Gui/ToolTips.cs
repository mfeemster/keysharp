using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Keysharp.Core.Common;
using Keysharp.Core.Windows;
using Keysharp.Scripting;

namespace Keysharp.Core
{
	public static class ToolTips
	{
		private static ToolTip[] persistentTooltips = new ToolTip[20];

		/// <summary>
		/// Derivation to help with proper positioning to prevent the tooltip from going off screen.
		/// Gotten from: https://stackoverflow.com/questions/65526197/tooltip-text-going-off-screen-winforms
		/// </summary>
		/*
		    private class KeysharpTooltip : ToolTip
		    {
		    private static readonly TextFormatFlags flags = TextFormatFlags.VerticalCenter
		            | TextFormatFlags.Left
		            | TextFormatFlags.LeftAndRightPadding
		            | TextFormatFlags.NoClipping
		            | TextFormatFlags.WordBreak;

		    public KeysharpTooltip()
		    {
		        Popup += KeysharpTooltip_Popup;
		        Draw += KeysharpTooltip_Draw;
		    }

		    private void KeysharpTooltip_Popup(object sender, PopupEventArgs e)
		    {
		        var t = sender as ToolTip;
		        var h = (IntPtr)t.GetType().GetProperty("Handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(t);
		        var toolTipText = t.GetToolTip(e.AssociatedControl);
		        var screen = System.Windows.Forms.Screen.FromControl(e.AssociatedControl).WorkingArea;
		        //if (e.
		        //WindowsAPI.MoveWindow(h, 2, 2, e.Bounds.Width - 2, e.Bounds.Height, false);
		        //using (var g = e.AssociatedControl.CreateGraphics())
		        //  using (var font = new Font("MS Sans Serif", 16))
		        //  {
		        //      var sz = TextRenderer.MeasureText(g, toolTipText, font, screen.Size, flags);
		        //      e.ToolTipSize = new Size(screen.Width - 2, sz.Height + 10);
		        //  }
		    }

		    private void KeysharpTooltip_Draw(object sender, DrawToolTipEventArgs e)
		    {
		        var t = sender as ToolTip;
		        var h = (IntPtr)t.GetType().GetProperty("Handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(t);
		        var screen = System.Windows.Forms.Screen.FromControl(e.AssociatedControl).WorkingArea;
		        var movetox = e.Bounds.Left;
		        var movetoy = e.Bounds.Top;
		        var needsmove = false;

		        if (e.Bounds.Left + e.Bounds.Width > screen.Width)
		        {
		            needsmove = true;
		            movetox = screen.Width - e.Bounds.Width - 1;
		        }

		        if (e.Bounds.Top + e.Bounds.Height > screen.Height)
		        {
		            needsmove = true;
		            movetoy = screen.Height - e.Bounds.Height - 1;
		        }

		        //e.DrawBackground();
		        //e.DrawBorder();
		        var newrect = new Rectangle(movetox, movetoy, e.Bounds.Width, e.Bounds.Height);
		        //using (var font = new Font("MS Sans Serif", 16))
		        //  TextRenderer.DrawText(e.Graphics, e.ToolTipText, font, newrect, Color.Black, flags);
		        var right = movetox + e.Bounds.Width;
		        var bottom = movetoy + e.Bounds.Height;
		        var mouse = System.Windows.Forms.Cursor.Position;

		        if (mouse.X >= movetox && mouse.X <= right &&
		                mouse.Y >= movetoy && mouse.Y <= bottom)
		        {
		            needsmove = true;
		            movetox = mouse.X - e.Bounds.Width - 3;
		            movetoy = mouse.Y - e.Bounds.Height - 3;
		        }

		        if (needsmove)
		        {
		            _ = WindowsAPI.MoveWindow(h, movetox, movetoy, e.Bounds.Width, e.Bounds.Height, false);//Will need a cross platform way to do this.//TODO
		        }
		    }
		    }
		*/

		public static object ToolTip(object obj0 = null, object obj1 = null, object obj2 = null, object obj3 = null)
		{
			var text = obj0.As();
			var x = (int)obj1.Al(int.MinValue);
			var y = (int)obj2.Al(int.MinValue);
			var id = (int)obj3.Al(1);
			id--;

			if (text != "")
			{
				var tooltipForm = GuiHelper.DialogOwner ?? Form.ActiveForm;
				var one_or_both_coords_specified = x != int.MinValue || y != int.MinValue;
				var one_or_both_coords_unspecified = x == int.MinValue || y == int.MinValue;

				if (tooltipForm == null)
				{
					tooltipForm = Application.OpenForms.Cast<Form>().LastOrDefault(f => f != Keysharp.Scripting.Script.mainWindow);//Get the last created one, which is not necessarily the last focused one, even though that's really what we want.

					if (tooltipForm == null)
						tooltipForm = Script.mainWindow;
				}

				if (tooltipForm == null)
					return "";

				tooltipForm.CheckedBeginInvoke(() =>
				{
					if (persistentTooltips[id] == null)
						persistentTooltips[id] = new ToolTip { Active = true, AutomaticDelay = 0, InitialDelay = 0, ReshowDelay = 0, ShowAlways = true };

					var tt = persistentTooltips[id];

					tt.Active = true;

					if (one_or_both_coords_unspecified)
					{
						var temppt = System.Windows.Forms.Cursor.Position;
						temppt.X += 10;
						temppt.Y += 10;
						var m = tt.GetType().GetMethod("SetTool", BindingFlags.Instance | BindingFlags.NonPublic);
						_ = m.Invoke(tt, new object[] { tooltipForm, text, 2, temppt });
					}
					else
					{
						var tempx = 0;
						var tempy = 0;

						if (one_or_both_coords_specified)
						{
							var coordMode = Mouse.Coords.GetCoordMode(CoordMode.Tooltip);

							if (x != int.MinValue)
								tempx = x;

							if (y != int.MinValue)
								tempy = y;

							if (Mouse.Coords.Tooltip == CoordModeType.Screen)
							{
								var m = tt.GetType().GetMethod("SetTool", BindingFlags.Instance | BindingFlags.NonPublic);
								_ = m.Invoke(tt, new object[] { tooltipForm, text, 2, new Point(tempx, tempy) });
							}
							else
							{
								//This is the hard case. They've specified coordinates relative to a window, however if that window
								//is minimized, then it's coordinates are impossible to get. Attempt to use the RestoreBounds property, but that is usually
								//wrong.
								if (tooltipForm.WindowState == FormWindowState.Minimized)
								{
									var actualbounds = tooltipForm.RestoreBounds;
									tempx += actualbounds.X;
									tempy += actualbounds.Y;
									var m = tt.GetType().GetMethod("SetTool", BindingFlags.Instance | BindingFlags.NonPublic);
									_ = m.Invoke(tt, new object[] { tooltipForm, text, 2, new Point(tempx, tempy) });
								}
								else// if (tooltipForm.Visible && tooltipForm.Focused)//The coord is relative to a window, and the window is not minimized and is active.
								{
									var pt = tooltipForm.PointToScreen(new Point(tempx, tempy));
									var m = tt.GetType().GetMethod("SetTool", BindingFlags.Instance | BindingFlags.NonPublic);
									_ = m.Invoke(tt, new object[] { tooltipForm, text, 2, pt });
								}

								//else//The coord is relative to a window, and the window is not minimized but is also not active.
								//{
								//  var pt = tooltipForm.PointToScreen(new Point(tempx, tempy));
								//  var m = tt.GetType().GetMethod("SetTool", BindingFlags.Instance | BindingFlags.NonPublic);
								//  _ = m.Invoke(tt, new object[] { tooltipForm, text, 2, pt });
								//}
							}
						}
					}

					//AHK did a large amount of work to make sure the tooltip didn't go off screen
					//and also to ensure it was not behind the mouse cursor. This seems like overkill
					//for two reasons.
					//1: That code is likely legacy. The Winforms ToolTip class already moves the tooltip
					//to be entirely on the screen if any portion of it would have been off the screen.
					//2: If the user needs to move the mouse out of the way, they can just do it.
				});
				return tooltipForm.Handle.ToInt64();
			}
			else
			{
				if (id < persistentTooltips.Length && persistentTooltips[id] != null)
				{
					persistentTooltips[id].Active = false;
					persistentTooltips[id] = null;
				}
			}

			return "";
		}

		public static void TraySetIcon(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			var filename = obj0.As();
			var iconnumber = ImageHelper.PrepareIconNumber(obj1);

			if (Parser.NoTrayIcon)
				return;

			if (obj2 != null)
				Accessors.A_IconFrozen = obj2.Ab();

			if (filename != "*")
			{
				var bmp = ImageHelper.LoadImage(filename, 0, 0, iconnumber);

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
						Accessors.A_IconNumber = obj1;
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

		public static void TrayTip(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			var text = obj0.As();
			var title = obj1.As();
			var options = obj2;

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