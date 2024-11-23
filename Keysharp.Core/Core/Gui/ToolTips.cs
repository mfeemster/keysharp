namespace Keysharp.Core
{
    public static class ToolTips
    {
        public static readonly int MaxToolTips = 20;
        private static readonly ToolTip[] persistentTooltips = new ToolTip[MaxToolTips];
        private static Point?[] persistentTooltipsPositions = new Point?[MaxToolTips];

        public static object ToolTip(object obj0 = null, object obj1 = null, object obj2 = null, object obj3 = null)
        {
            var text = obj0.As();
            var x = obj1.Ai(int.MinValue);
            var y = obj2.Ai(int.MinValue);
            var id = obj3.Ai(1);
            id--;

            if (text == "") // Clear tooltip and return
            {
                if (id < persistentTooltips.Length && persistentTooltips[id] != null)
                {
                    persistentTooltips[id].Active = false;
                    persistentTooltips[id] = null;
                    persistentTooltipsPositions[id] = null;
                }
                return "";
            }

            var tooltipInvokerForm = GuiHelper.DialogOwner ?? Form.ActiveForm;
            var focusedWindow = IntPtr.Zero;
            var one_or_both_coords_specified = x != int.MinValue || y != int.MinValue;
            var one_or_both_coords_unspecified = x == int.MinValue || y == int.MinValue;

            if (tooltipInvokerForm == null)
            {
                tooltipInvokerForm = Application.OpenForms.Cast<Form>().LastOrDefault(f => f != Script.mainWindow);//Get the last created one, which is not necessarily the last focused one, even though that's really what we want.

                if (tooltipInvokerForm == null)
                    tooltipInvokerForm = Script.mainWindow;
            }

            if (tooltipInvokerForm == null)
                return "";

            var handle = 0L;
            ToolTip tt = null;
            Point? ttp = persistentTooltipsPositions[id];
            tooltipInvokerForm.CheckedInvoke(() =>
            {
                if (persistentTooltips[id] == null)
                    persistentTooltips[id] = new ToolTip
                    {
                        Active = true,
#if WINDOWS
                        AutomaticDelay = 0,//Delay of 0 throws an exception on linux.
#endif
                        InitialDelay = 0,
                        ReshowDelay = 0,
                        ShowAlways = true,
                        UseFading = false,
                        UseAnimation = false
                    };

                tt = persistentTooltips[id];

#if WINDOWS
                var h = tt.GetType().GetProperty("Handle", BindingFlags.Instance | BindingFlags.NonPublic);

                handle = ((IntPtr)h.GetValue(tt)).ToInt64();

#elif LINUX
				var ttwndField = tt.GetType().GetField("tooltip_window", BindingFlags.Instance | BindingFlags.NonPublic);

				var ttwnd = ttwndField.GetValue(tt);

				var hprop = ttwnd.GetType().GetProperty("Handle", BindingFlags.Instance | BindingFlags.Public);

				handle = ((IntPtr)hprop.GetValue(ttwnd)).ToInt64();

#endif
            }, false);
            tooltipInvokerForm.CheckedBeginInvoke(() =>
            {
                tt.Active = true;
#if LINUX
				tt.SetToolTip(tooltipInvokerForm, text);//Setting position is not possible on linux.
#elif WINDOWS

                var mSetTrackPosition = tt.GetType().GetMethod("SetTrackPosition", BindingFlags.Instance | BindingFlags.NonPublic);
                var mSetTool = tt.GetType().GetMethod("SetTool", BindingFlags.Instance | BindingFlags.NonPublic);

                if (one_or_both_coords_unspecified)
                {
                    //We use SetTool() via reflection in this function because it bypasses ToolTip.Show()'s check for whether or not the window
                    //is active.
                    var temppt = Cursor.Position;
                    temppt.X += 10;
                    temppt.Y += 10;

                    if (ttp != null && ttp?.X == temppt.X && ttp?.Y == temppt.Y && tt.GetToolTip(tooltipInvokerForm) == text)
                        return;
                    persistentTooltipsPositions[id] = new Point(temppt.X, temppt.Y);

                    _ = mSetTrackPosition.Invoke(tt, [temppt.X, temppt.Y]);
                    _ = mSetTool.Invoke(tt, [tooltipInvokerForm, text, 2, persistentTooltipsPositions[id]]);
                }
                else
                {
                    var tempx = 0;
                    var tempy = 0;

                    if (one_or_both_coords_specified)
                    {
                        //var coordMode = Mouse.Coords.GetCoordMode(CoordMode.Tooltip);
                        if (x != int.MinValue)
                            tempx = x;

                        if (y != int.MinValue)
                            tempy = y;

                        if (Mouse.Coords.Tooltip == CoordModeType.Screen)
                        {
                            if (ttp != null && ttp?.X == tempx && ttp?.Y == tempy && tt.GetToolTip(tooltipInvokerForm) == text)
                                return;
                            persistentTooltipsPositions[id] = new Point(tempx, tempy);

                            _ = mSetTrackPosition.Invoke(tt, [tempx, tempy]);
                            _ = mSetTool.Invoke(tt, [tooltipInvokerForm, text, 2, persistentTooltipsPositions[id]]);
                        }
                        else
                        {
                            var foreground = WindowProvider.Manager.ActiveWindow;

                            if (foreground.Handle != IntPtr.Zero)
                                PlatformProvider.Manager.CoordToScreen(ref tempx, ref tempy, CoordMode.Tooltip);

                            if (ttp != null && ttp?.X == tempx && ttp?.Y == tempy && tt.GetToolTip(tooltipInvokerForm) == text)
                                return;
                            persistentTooltipsPositions[id] = new Point(tempx, tempy);

                            //This is the hard case. They've specified coordinates relative to a window, however if that window
                            //is minimized, then its coordinates are impossible to get. Attempt to use the RestoreBounds property, but that is usually
                            //wrong.
                            //if (tooltipInvokerForm.WindowState == FormWindowState.Minimized)
                            //{
                            //  var actualbounds = tooltipInvokerForm.RestoreBounds;
                            //  tempx += actualbounds.X;
                            //  tempy += actualbounds.Y;
                            //  var m = tt.GetType().GetMethod("SetTool", BindingFlags.Instance | BindingFlags.NonPublic);
                            //  _ = m.Invoke(tt, new object[] { tooltipInvokerForm, text, 2, new Point(tempx, tempy) });
                            //}
                            //else// if (tooltipForm.Visible && tooltipForm.Focused)//The coord is relative to a window, and the window is not minimized and is active.
                            {
                                //var pt = tooltipForm.PointToScreen(new Point(tempx, tempy));
                                //var pt = tooltipForm.PointToClient(new Point(tempx, tempy));
                                _ = mSetTrackPosition.Invoke(tt, [tempx, tempy]);
                                _ = mSetTool.Invoke(tt, [tooltipInvokerForm, text, 2, persistentTooltipsPositions[id]]);
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

#endif
                //AHK did a large amount of work to make sure the tooltip didn't go off screen
                //and also to ensure it was not behind the mouse cursor. This seems like overkill
                //for two reasons.
                //1: That code is likely legacy. The Winforms ToolTip class already moves the tooltip
                //to be entirely on the screen if any portion of it would have been off the screen.
                //2: If the user needs to move the mouse out of the way, they can just do it.
            }, false, false);
            return handle;
        }

        public static void TraySetIcon(object obj0 = null, object obj1 = null, object obj2 = null)
        {
            var filename = obj0.As();
            var iconnumber = ImageHelper.PrepareIconNumber(obj1);

            if (Script.NoTrayIcon)
                return;

            if (obj2 != null)
                Accessors.A_IconFrozen = obj2.Ab();

            if (filename != "*")
            {
                var (bmp, temp) = ImageHelper.LoadImage(filename, 0, 0, iconnumber);

                if (Script.Tray == null)
                    Script.CreateTrayMenu();

                if (bmp != null)
                {
                    var ptr = bmp.GetHicon();

                    try
                    {
                        var icon = temp as Icon;

                        if (icon == null)
                            icon = Icon.FromHandle(ptr);

                        if (icon != null)
                        {
                            Accessors.A_IconFile = filename;
                            Accessors.A_IconNumber = obj1;
                            Script.mainWindow.CheckedBeginInvoke(() =>
                            {
                                Script.Tray.Icon = Script.mainWindow.Icon = icon;
                            }, false, false);
                        }
                    }
                    finally
                    {
                        _ = PlatformProvider.Manager.DestroyIcon(ptr);
                    }
                }
            }
            else
            {
                Accessors.A_IconFile = "";
                Accessors.A_IconNumber = 1;
                Script.mainWindow.CheckedBeginInvoke(() =>
                {
                    Script.Tray.Icon = Script.mainWindow.Icon = Properties.Resources.Keysharp_ico;
                }, false, false);
            }
        }

        public static void TrayTip(object obj0 = null, object obj1 = null, object obj2 = null)
        {
            var text = obj0.As();
            var title = obj1.As();
            var options = obj2;

            if (Script.NoTrayIcon)
                return;

            if ((bool)Accessors.A_IconHidden)
                return;

            if (Script.Tray == null)
                Script.CreateTrayMenu();

            //As passing an empty string hides the TrayTip (or does nothing on Windows 10),
            //pass a space to ensure the TrayTip is shown.  Testing showed that Windows 10
            //will size the notification to fit only the title, as if there was no text.
            if (title.Length > 0 && text.Length == 0)
            {
                text = " ";
            }

            if (text.Length == 0 && title.Length == 0)
            {
                Script.Tray.Visible = false;
                Script.Tray.Visible = true;
                return;
            }

            var icon = ToolTipIcon.None;
            void HandleInt(int? i)
            {
                if ((i & 4) == 4) { }//tray icon
                else if ((i & 3) == 3) { icon = ToolTipIcon.Error; }
                else if ((i & 2) == 2) { icon = ToolTipIcon.Warning; }
                else if ((i & 1) == 1) { icon = ToolTipIcon.Info; }
                else if ((i & 16) == 16) { }
                else if ((i & 32) == 32) { }
            }

            if (options is string s)
            {
                foreach (Range r in s.AsSpan().SplitAny(Keywords.Spaces))
                {
                    var opt = s.AsSpan(r).Trim();

                    if (opt.Length > 0)
                    {
                        if (opt.Equals("iconi", StringComparison.OrdinalIgnoreCase)) icon = ToolTipIcon.Info;
                        else if (opt.Equals("icon!", StringComparison.OrdinalIgnoreCase)) icon = ToolTipIcon.Warning;
                        else if (opt.Equals("iconx", StringComparison.OrdinalIgnoreCase)) icon = ToolTipIcon.Error;
                        else if (opt.Equals("mute", StringComparison.OrdinalIgnoreCase)) { }
                        else HandleInt(int.Parse(opt));
                    }
                }
            }
            else if (options != null)
                HandleInt(options.ParseInt());

            Script.Tray.Visible = true;
            Script.Tray.ShowBalloonTip(1000, title, text, icon);//Duration is now ignored by Windows.
        }
    }
}