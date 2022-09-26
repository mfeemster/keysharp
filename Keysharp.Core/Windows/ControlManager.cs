using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Common.Window;

namespace Keysharp.Core.Windows
{
	internal class ControlManager : ControlManagerBase
	{
		internal static int ConvertMouseButton(string buf, bool allowWheel = true)
		{
			if (buf?.Length == 0 || buf.StartsWith("L"))
				return WindowsAPI.VK_LBUTTON;

			if (buf.StartsWith("R")) return WindowsAPI.VK_RBUTTON;

			if (buf.StartsWith("M")) return WindowsAPI.VK_MBUTTON;

			if (buf == "X1") return WindowsAPI.VK_XBUTTON1;

			if (buf == "X2") return WindowsAPI.VK_XBUTTON2;

			if (allowWheel)
			{
				if (buf == "WheelUp" || buf == "WU") return WindowsAPI.VK_WHEEL_UP;

				if (buf == "WheelDown" || buf == "WD") return WindowsAPI.VK_WHEEL_DOWN;

				if (buf == "WheelLeft" || buf == "WL") return WindowsAPI.VK_WHEEL_LEFT;

				if (buf == "WheelRight" || buf == "WR") return WindowsAPI.VK_WHEEL_RIGHT;
			}

			return 0;
		}

		internal override long ControlAddItem(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var res = 0L;
				var ctrl2 = Control.FromHandle(item.Handle);

				if (ctrl2 is ComboBox cb)
				{
					res = cb.Items.Add(str);
				}
				else if (ctrl2 is ListBox lb)
				{
					res = lb.Items.Add(str);
				}
				else
				{
					int msg;

					if (item.ClassName.Contains("Combo"))
						msg = WindowsAPI.CB_ADDSTRING;
					else if (item.ClassName.Contains("List"))
						msg = WindowsAPI.LB_ADDSTRING;
					else
						throw new TargetError($"Class name ${item.ClassName} did not contain Combo or List");

					if (WindowsAPI.SendMessageTimeout(item.Handle, (uint)msg, 0, str, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result) == 0)
						throw new TargetError($"Could not add ${str} to combo or list box in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

					res = result.ToInt64();

					if (res == WindowsAPI.CB_ERR || res == WindowsAPI.CB_ERRSPACE)
						throw new Error("Failed");
				}

				WindowItemBase.DoControlDelay();
				return res + 1L;
			}

			return 0L;
		}

		internal override void ControlChooseIndex(int n, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				uint msg = 0, x_msg = 0, y_msg = 0;
				n--;

				if (item.ClassName.Contains("Combo"))
				{
					msg = WindowsAPI.CB_SETCURSEL;
					x_msg = WindowsAPI.CBN_SELCHANGE;
					y_msg = WindowsAPI.CBN_SELENDOK;
				}
				else if (item.ClassName.Contains("List"))
				{
					msg = (WindowsAPI.GetWindowLongPtr(item.Handle, WindowsAPI.GWL_STYLE).ToInt64() & (WindowsAPI.LBS_EXTENDEDSEL | WindowsAPI.LBS_MULTIPLESEL)) != 0
						  ? WindowsAPI.LB_SETSEL
						  : (uint)WindowsAPI.LB_SETCURSEL;
					x_msg = WindowsAPI.LBN_SELCHANGE;
					y_msg = WindowsAPI.LBN_DBLCLK;
				}
				else if (item.ClassName.Contains("Tab"))
				{
					if (!WindowsAPI.ControlSetTab(item.Handle, n))
						throw new TargetError($"Could not set tab index to ${n} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
				}
				else
					throw new TargetError($"Class name ${item.ClassName} did not contain Combo, List or Tab");

				IntPtr result;

				if (msg == WindowsAPI.LB_SETSEL)//Multi-select, so use the cumulative method.
				{
					if (WindowsAPI.SendMessageTimeout(item.Handle, msg, n != -1 ? 1u : 0u, new IntPtr(n), SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out result) == 0)
						throw new TargetError($"Could not set list box selection index to ${n} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
				}
				else//ComboBox or single-select ListBox.
				{
					if (WindowsAPI.SendMessageTimeout(item.Handle, msg, (uint)n, IntPtr.Zero, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out result) == 0)
						throw new TargetError($"Could not set combo or single selection list box index to ${n} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
				}

				if (result.ToInt64() == WindowsAPI.CB_ERR && n != -1)//CB_ERR == LB_ERR
					throw new Error("Failed");

				NotifyParent(item.Handle, x_msg, y_msg);
				WindowItemBase.DoControlDelay();
			}
		}

		internal override long ControlChooseString(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				uint msg = 0, x_msg = 0, y_msg = 0;

				if (item.ClassName.Contains("Combo"))
				{
					msg = WindowsAPI.CB_SELECTSTRING;
					x_msg = WindowsAPI.CBN_SELCHANGE;
					y_msg = WindowsAPI.CBN_SELENDOK;
				}
				else if (item.ClassName.Contains("List"))
				{
					msg = (WindowsAPI.GetWindowLongPtr(item.Handle, WindowsAPI.GWL_STYLE).ToInt64() & (WindowsAPI.LBS_EXTENDEDSEL | WindowsAPI.LBS_MULTIPLESEL)) != 0
						  ? WindowsAPI.LB_FINDSTRING
						  : (uint)WindowsAPI.LB_SELECTSTRING;
					x_msg = WindowsAPI.LBN_SELCHANGE;
					y_msg = WindowsAPI.LBN_DBLCLK;
				}
				else
					throw new TargetError($"Class name ${item.ClassName} did not contain Combo or List");

				IntPtr item_index;

				if (msg == WindowsAPI.LB_FINDSTRING)//Multi-select ListBox (LB_SELECTSTRING is not supported by these).
				{
					if (WindowsAPI.SendMessageTimeout(item.Handle, msg, -1, str, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out item_index) == 0)
						throw new TargetError($"Could not set list box selection index to ${str} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

					if (item_index.ToInt64() == WindowsAPI.LB_ERR)
						throw new TargetError($"Erroneous item index when setting list box selection index to ${str} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

					if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.LB_SETSEL, 1, item_index, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result) == 0)
						throw new TargetError($"Could not set list box selection index to ${item_index.ToInt64()} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

					if (result.ToInt64() == WindowsAPI.LB_ERR)
						throw new TargetError($"Erroneous item index when setting list box selection index to ${item_index.ToInt64()} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
				}
				else//ComboBox or single-select ListBox.
				{
					if (WindowsAPI.SendMessageTimeout(item.Handle, msg, -1, str, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out item_index) == 0)
						throw new TargetError($"Could not set combo box selection index to ${str} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

					if (item_index.ToInt64() == WindowsAPI.CB_ERR) // CB_ERR == LB_ERR
						throw new TargetError($"Erroneous item index when setting combo box selection index to ${str} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
				}

				NotifyParent(item.Handle, x_msg, y_msg);
				WindowItemBase.DoControlDelay();
				return item_index.ToInt64() + 1;
			}

			return 0L;
		}


		internal override void ControlClick(object ctrlorpos, object title, string text, string whichButton, int clickCount, string options, string excludeTitle, string excludeText)
		{
			var winx = int.MinValue;
			var winy = int.MinValue;
			var ctrlx = int.MinValue;
			var ctrly = int.MinValue;
			var vk = ConvertMouseButton(whichButton);
			var posoverride = options.Contains("pos", StringComparison.OrdinalIgnoreCase);
			bool d = false, u = false, na = false;
			var optsplits = options.Split(Keysharp.Core.Core.Spaces, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			for (var i = 0; i < optsplits.Length; i++)
			{
				var opt = optsplits[i].ToLower();

				if (opt == "d")
					d = true;
				else if (opt == "u")
					u = true;
				else if (opt == "na")
					na = true;
				else if (Options.TryParse(opt, "x", ref ctrlx)) { }
				else if (Options.TryParse(opt, "y", ref ctrly)) { }
			}

			if (d)
				u = false;

			if (u)
				d = false;

			if (ctrlorpos is string s && s.StartsWith("x", StringComparison.OrdinalIgnoreCase) && s.Contains(' ') && s.Contains('y', StringComparison.OrdinalIgnoreCase))
			{
				var possplits = s.Split(Keysharp.Core.Core.Spaces, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

				for (var i = 0; i < possplits.Length; i++)
				{
					var opt = optsplits[i].ToLower();

					if (Options.TryParse(opt, "x", ref winx)) { }
					else if (Options.TryParse(opt, "y", ref winy)) { }
				}
			}

			WindowItemBase item = null;
			var getctrlbycoords = false;

			if (ctrlorpos.IsNullOrEmpty())//No control or coordinates, so just find the window.
			{
				item = Window.SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true);
			}
			else if (!posoverride)//Don't override, so try ctrlorpos first, and if it doesn't work, then try as an x/y.
			{
				item = Window.SearchControl(ctrlorpos, title, text, excludeTitle, excludeText, false);

				if (item == null)
				{
					if (winx != int.MinValue && winy != int.MinValue)
					{
						getctrlbycoords = false;
					}
					else
					{
						throw new TargetError($"Could not get control {ctrlorpos} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
					}
				}
			}
			else//Override, so always treat ctrlorpos as an x/y.
			{
				if (winx != int.MinValue && winy != int.MinValue)
				{
					getctrlbycoords = false;
				}
			}

			if (getctrlbycoords)
			{
				item = Window.SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true);
				var rect = new Point(winx, winx);
				_ = WindowsAPI.ClientToScreen(item.Handle, ref rect);
				var newx = rect.X;
				var newy = rect.Y;
				var pah = new PointAndHwnd(new POINT
				{
					x = newx,
					y = newy
				});
				item.ChildFindPoint(pah);
				item = pah.hwndFound != IntPtr.Zero ? new WindowItem(pah.hwndFound) : item;
				//rect = new Point(newx, newy);
				//_ = WindowsAPI.ScreenToClient(item.Handle, ref rect);
				//ctrlx = rect.X;
				//ctrly = rect.Y;
			}

			//if (x != int.MinValue && y != int.MinValue)
			//  if (!(Window.SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true) is WindowItem win))
			//      return;

			//if (!posoverride)
			//  item = Window.SearchControl(win, ctrlorpos, false);

			//if (item == null && ctrlorpos is string s)
			//{
			//  var rect = new Point(x, y);
			//  _ = WindowsAPI.ClientToScreen(win.Handle, ref rect);
			//  x = rect.X;
			//  y = rect.Y;
			//}

			//if (item == null)
			//{
			//  if (x != int.MinValue && y != int.MinValue)
			//  {
			//      var pah = new PointAndHwnd(new POINT
			//      {
			//          x = x,
			//          y = y
			//      });
			//      win.ChildFindPoint(pah);
			//      item = pah.hwndFound != IntPtr.Zero ? new WindowItem(pah.hwndFound) : win;
			//      var rect = new Point(x, y);
			//      _ = WindowsAPI.ScreenToClient(item.Handle, ref rect);
			//      x = rect.X;
			//      y = rect.Y;
			//  }
			//}

			if (clickCount < 1)
				return;

			if (ctrlx == int.MinValue || ctrly == int.MinValue)
			{
				var temprect = new RECT();

				if (!WindowsAPI.GetWindowRect(item.Handle, out temprect))
					throw new TargetError($"Could not get control rect in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				if (ctrlx == int.MinValue)
					ctrlx = (temprect.Right - temprect.Left) / 2;

				if (ctrly == int.MinValue)
					ctrly = (temprect.Bottom - temprect.Top) / 2;
			}

			var lparam = KeyboardUtils.MakeLong((short)ctrlx, (short)ctrly);
			uint msg_down = 0, msg_up = 0;
			uint wparam = 0, wparam_up = 0;
			var vk_is_wheel = vk == WindowsAPI.VK_WHEEL_UP || vk == WindowsAPI.VK_WHEEL_DOWN;
			var vk_is_hwheel = vk == WindowsAPI.VK_WHEEL_LEFT || vk == WindowsAPI.VK_WHEEL_RIGHT;

			if (vk_is_wheel)
			{
				wparam = (uint)(clickCount * ((vk == WindowsAPI.VK_WHEEL_UP) ? WindowsAPI.WHEEL_DELTA : -WindowsAPI.WHEEL_DELTA)) << 16;  // High order word contains the delta.
				msg_down = WindowsAPI.WM_MOUSEWHEEL;
			}
			else if (vk_is_hwheel)  // Lexikos: Support horizontal scrolling in Windows Vista and later.
			{
				wparam = (uint)(clickCount * ((vk == WindowsAPI.VK_WHEEL_LEFT) ? -WindowsAPI.WHEEL_DELTA : WindowsAPI.WHEEL_DELTA)) << 16;
				msg_down = WindowsAPI.WM_MOUSEHWHEEL;
			}
			else
			{
				switch (vk)
				{
					case WindowsAPI.VK_LBUTTON: msg_down = WindowsAPI.WM_LBUTTONDOWN; msg_up = WindowsAPI.WM_LBUTTONUP; wparam = WindowsAPI.MK_LBUTTON; break;

					case WindowsAPI.VK_RBUTTON: msg_down = WindowsAPI.WM_RBUTTONDOWN; msg_up = WindowsAPI.WM_RBUTTONUP; wparam = WindowsAPI.MK_RBUTTON; break;

					case WindowsAPI.VK_MBUTTON: msg_down = WindowsAPI.WM_MBUTTONDOWN; msg_up = WindowsAPI.WM_MBUTTONUP; wparam = WindowsAPI.MK_MBUTTON; break;

					case WindowsAPI.VK_XBUTTON1: msg_down = WindowsAPI.WM_XBUTTONDOWN; msg_up = WindowsAPI.WM_XBUTTONUP; wparam_up = WindowsAPI.VK_XBUTTON1 << 16; wparam = WindowsAPI.MK_XBUTTON1 | wparam_up; break;

					case WindowsAPI.VK_XBUTTON2: msg_down = WindowsAPI.WM_XBUTTONDOWN; msg_up = WindowsAPI.WM_XBUTTONUP; wparam_up = WindowsAPI.XBUTTON2 << 16; wparam = WindowsAPI.MK_XBUTTON2 | wparam_up; break;
				}
			}

			(bool, uint) thinfo = (false, 0);

			if (!na)
			{
				thinfo = WindowsAPI.AttachThreadInput(item.Handle, true);
			}

			if (vk_is_wheel || vk_is_hwheel) // v1.0.48: Lexikos: Support horizontal scrolling in Windows Vista and later.
			{
				_ = WindowsAPI.PostMessage(item.Handle, msg_down, wparam, lparam);
				WindowItemBase.DoControlDelay();
			}
			else
			{
				for (var i = 0; i < clickCount; ++i)
				{
					if (!u) // It's either down-only or up-and-down so always to the down-event.
					{
						_ = WindowsAPI.PostMessage(item.Handle, msg_down, wparam, lparam);
						WindowItemBase.DoControlDelay();
					}

					if (!d) // It's either up-only or up-and-down so always to the up-event.
					{
						_ = WindowsAPI.PostMessage(item.Handle, msg_up, wparam_up, lparam);
						WindowItemBase.DoControlDelay();
					}
				}
			}

			if (!na)
				WindowsAPI.DetachThreadInput(thinfo.Item1, thinfo.Item2);
		}

		internal override void ControlDeleteItem(int n, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				uint msg;// = 0, x_msg = 0, y_msg = 0;
				n--;
				var ctrl2 = Control.FromHandle(item.Handle);

				if (ctrl2 is ComboBox cb)
				{
					cb.Items.RemoveAt(n);
				}
				else if (ctrl2 is ListBox lb)
				{
					lb.Items.RemoveAt(n);
				}
				else
				{
					if (item.ClassName.Contains("Combo"))
						msg = WindowsAPI.CB_DELETESTRING;
					else if (item.ClassName.Contains("List"))
						msg = WindowsAPI.LB_DELETESTRING;
					else
						throw new TargetError($"Class name ${item.ClassName} did not contain Combo or List");

					if (WindowsAPI.SendMessageTimeout(item.Handle, msg, (uint)n, IntPtr.Zero, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result) == 0)
						throw new TargetError($"Could not delete combo or list box index ${n} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

					if (result.ToInt64() == WindowsAPI.CB_ERR) // CB_ERR == LB_ERR
						throw new TargetError($"Erroneous item index when deleting combo or list box selection index to ${n} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
				}

				WindowItemBase.DoControlDelay();
			}
		}

		internal override long ControlFindItem(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				uint msg = 0;

				if (item.ClassName.Contains("Combo"))
					msg = WindowsAPI.CB_FINDSTRINGEXACT;
				else if (item.ClassName.Contains("List"))
					msg = WindowsAPI.LB_FINDSTRINGEXACT;
				else
					throw new TargetError($"Class name ${item.ClassName} did not contain Combo or List");

				if (WindowsAPI.SendMessageTimeout(item.Handle, msg, -1, str, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var index) == 0 || index.ToInt64() == WindowsAPI.CB_ERR) // CB_ERR == LB_ERR
					throw new Error($"Could not search for combo or list box item string {str} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				WindowItemBase.DoControlDelay();
				return index.ToInt64() + 1;
			}

			return 0L;
		}

		internal override void ControlFocus(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var thinfo = WindowsAPI.AttachThreadInput(item.Handle, true);
				_ = WindowsAPI.SetFocus(item.Handle);
				WindowItemBase.DoControlDelay();
				WindowsAPI.DetachThreadInput(thinfo.Item1, thinfo.Item2);
			}
		}

		internal override long ControlGetChecked(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				//Using SendMessage() with BM_GETCHECK does *not* work on Winforms checkboxes. So we must use this custom automation function gotten from Stack Overflow.
				return WindowsAPI.IsChecked(item.Handle) ? 1 : 0;
			}

			return 0L;
		}

		internal override string ControlGetChoice(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				uint msg = 0, x_msg = 0, y_msg = 0;

				if (item.ClassName.Contains("Combo"))
				{
					msg = WindowsAPI.CB_GETCURSEL;
					x_msg = WindowsAPI.CB_GETLBTEXTLEN;
					y_msg = WindowsAPI.CB_GETLBTEXT;
				}
				else if (item.ClassName.Contains("List"))
				{
					msg = WindowsAPI.LB_GETCURSEL;
					x_msg = WindowsAPI.LB_GETTEXTLEN;
					y_msg = WindowsAPI.LB_GETTEXT;
				}
				else
					throw new TargetError($"Class name ${item.ClassName} did not contain Combo or List");

				if (WindowsAPI.SendMessageTimeout(item.Handle, msg, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var index) == 0
						|| index.ToInt64() == WindowsAPI.CB_ERR  // CB_ERR == LB_ERR.  There is no selection (or very rarely, some other type of problem).
						|| WindowsAPI.SendMessageTimeout(item.Handle, x_msg, index.ToInt32(), null, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var length) == 0
						|| length.ToInt64() == WindowsAPI.CB_ERR)  // CB_ERR == LB_ERR
				{
					throw new Error($"Could not get selected item string for combo or list box in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
				}

				var sb = new StringBuilder(length.ToInt32());

				if (WindowsAPI.SendMessageTimeout(item.Handle, y_msg, (uint)index.ToInt32(), sb, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out length) == 0
						|| length.ToInt64() == WindowsAPI.CB_ERR)//Probably impossible given the way it was called above. Also, CB_ERR == LB_ERR. Relies on short-circuit boolean order.
				{
					throw new Error($"Could not get selected item string for combo or list box in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
				}

				return sb.ToString();
			}

			return "";
		}

		internal override string ControlGetClassNN(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
				return item.ClassNN;

			return "";
		}

		internal override long ControlGetEnabled(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
				return item.Enabled ? 1L : 0L;

			return 0L;
		}

		internal override long ControlGetExStyle(object ctrl, object title, string text, string excludeTitle, string excludeText) => Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item ? item.ExStyle : 0L;

		internal override long ControlGetFocus(object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true) is WindowItem win)
			{
				var h = WindowsAPI.GetWindowThreadProcessId(win.Handle, out var pid);
				var info = GUITHREADINFO.Default;//Must be initialized this way because the size field must be populated.

				if (!WindowsAPI.GetGUIThreadInfo(h, out info))
					throw new OSError("", $"Could not retrieve GUI thread info.");

				//Use IsChild() to ensure the focused control actually belongs to this window.
				//Otherwise, a HWND will be returned if any window in the same thread has focus,
				//including the target window itself (typically when it has no controls).
				if (!WindowsAPI.IsChild(win.Handle, info.hwndFocus))
					return 0L;//As documented, if "none of the target window's controls has focus, the return value is 0".

				return info.hwndFocus.ToInt64();
			}
			return 0L;
		}

		internal override long ControlGetHwnd(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
				return item.Handle.ToInt64();
			else
				return 0L;
		}

		internal override long ControlGetIndex(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				uint msg = 0;

				if (item.ClassName.Contains("Combo"))
					msg = WindowsAPI.CB_GETCURSEL;
				else if (item.ClassName.Contains("List"))
					msg = WindowsAPI.LB_GETCURSEL;
				else if (item.ClassName.Contains("Tab"))
					msg = WindowsAPI.TCM_GETCURSEL;
				else
					throw new TargetError($"Class name ${item.ClassName} did not contain Combo, List or Tab");

				if (WindowsAPI.SendMessageTimeout(item.Handle, msg, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var index) == 0)
					throw new Error($"Could not get selected item index for combo box, list box or tab control in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				return index.ToInt64() + 1L;
			}

			return 0L;
		}

		internal override Array ControlGetItems(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				uint msg = 0, x_msg = 0;

				if (item.ClassName.Contains("Combo"))
				{
					msg = WindowsAPI.CB_GETCOUNT;
					x_msg = WindowsAPI.CB_GETLBTEXT;
				}
				else if (item.ClassName.Contains("List"))
				{
					msg = WindowsAPI.LB_GETCOUNT;
					x_msg = WindowsAPI.LB_GETTEXT;
				}
				else
					throw new TargetError($"Class name ${item.ClassName} did not contain Combo or List");

				var cnt = (int)WindowsAPI.SendMessage(item.Handle, msg, 0, 0);
				var listBoxContent = new Array(cnt);

				for (var i = 0; i < cnt; i++)
				{
					var sb = new StringBuilder(256);
					_ = WindowsAPI.SendMessage(item.Handle, x_msg, i, sb);
					_ = listBoxContent.Add(sb.ToString());
				}

				return listBoxContent;
			}

			return new Array();
		}

		internal override Keysharp.Core.Map ControlGetPos(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (!WindowsAPI.GetWindowRect(item.Handle, out var rect))
					throw new Error($"Could not get rect control in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				if (WindowsAPI.MapWindowPoints(IntPtr.Zero, WindowsAPI.GetParent(item.Handle), ref rect, 2) == 0)
					throw new Error($"Could not map rect from screen to window in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				return rect.ToPos();
			}

			return new Keysharp.Core.Map();
		}

		internal override long ControlGetStyle(object ctrl, object title, string text, string excludeTitle, string excludeText) => Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item ? item.Style : 0;

		internal override string ControlGetText(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			return Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item
				   ? Control.FromHandle(item.Handle) is Control ctrl2 ? ctrl2.Text : WindowsAPI.GetWindowTextTimeout(item.Handle, 5000)
				   : "";
		}

		internal override long ControlGetVisible(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (Control.FromHandle(item.Handle) is Control ctrl2)
					return ctrl2.Visible ? 1 : 0;
				else
					_ = item.Visible;
			}

			return 0L;
		}

		internal override void ControlHide(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (Control.FromHandle(item.Handle) is Control ctrl2)
					ctrl2.Visible = false;
				else
					_ = item.Hide();

				WindowItemBase.DoControlDelay();
			}
		}

		internal override void ControlHideDropDown(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.CB_SHOWDROPDOWN, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var _) == 0)
					throw new Error($"Could not hide combo box drop down for in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				WindowItemBase.DoControlDelay();
			}
		}

		internal override void ControlMove(int x, int y, int width, int height, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (Control.FromHandle(item.Handle) is Control ctrl2)
				{
					ctrl2.Location = new Point(x == int.MinValue ? ctrl2.Location.X : x, y == int.MinValue ? ctrl2.Location.Y : y);
					ctrl2.Size = new Size(width == int.MinValue ? ctrl2.Size.Width : width, height == int.MinValue ? ctrl2.Size.Height : height);
				}
				else
				{
					item.Location = new Rectangle(x == int.MinValue ? item.Location.X : x, y == int.MinValue ? item.Location.Y : y, 0, 0);//Width and height are ignored.
					item.Size = new Size(width == int.MinValue ? item.Size.Width : width, height == int.MinValue ? item.Size.Height : height);
				}

				WindowItemBase.DoControlDelay();
			}
		}

		internal override void ControlSend(string str, object ctrl, object title, string text, string excludeTitle, string excludeText) => ControlSendHelper(str, ctrl, title, text, excludeTitle, excludeText, SendRawModes.NotRaw);

		internal void ControlSendHelper(string str, object ctrl, object title, string text, string excludeTitle, string excludeText, SendRawModes mode)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var ht = Keysharp.Scripting.Script.HookThread;
				var kbdMouseSender = ht.kbdMsSender;
				kbdMouseSender.SendKeys(str, mode, SendModes.Event, item.Handle);
			}
		}

		internal override void ControlSendText(string str, object ctrl, object title, string text, string excludeTitle, string excludeText) => ControlSendHelper(str, ctrl, title, text, excludeTitle, excludeText, SendRawModes.RawText);

		internal override void ControlSetChecked(object val, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var onoff = Options.ConvertOnOffToggle(val);
				var ctrl2 = Control.FromHandle(item.Handle);

				if (ctrl2 is CheckBox cb)
					cb.Checked = onoff == ToggleValueType.Toggle ? !cb.Checked : onoff == ToggleValueType.On;
				else if (ctrl2 is RadioButton rb)
					rb.Checked = onoff == ToggleValueType.Toggle ? !rb.Checked : onoff == ToggleValueType.On;
				else
				{
					var ischecked = WindowsAPI.IsChecked(item.Handle);//Check to see if it's already in the desired state.

					if (onoff == ToggleValueType.On && ischecked)
						return;
					else if (onoff == ToggleValueType.Off && !ischecked)
						return;

					var thinfo = WindowsAPI.AttachThreadInput(item.Handle, false);//Pass false because the SetActiveWindow() call below is more specific.
					_ = WindowsAPI.SetActiveWindow(item.Handle.ToInt64() == item.Handle.ToInt64() ? WindowsAPI.GetNonChildParent(item.Handle) : item.Handle);//Account for when the target window might be the control itself (e.g. via ahk_id %ControlHWND%).

					if (!WindowsAPI.GetWindowRect(item.Handle, out var rect))
						rect.Bottom = rect.Left = rect.Right = rect.Top = 0;

					var lparam = KeyboardUtils.MakeLong((short)((rect.Right - rect.Left) / 2), (short)((rect.Bottom - rect.Top) / 2));
					_ = WindowsAPI.PostMessage(item.Handle, WindowsAPI.WM_LBUTTONDOWN, WindowsAPI.MK_LBUTTON, lparam);
					_ = WindowsAPI.PostMessage(item.Handle, WindowsAPI.WM_LBUTTONUP, 0, lparam);
					WindowItemBase.DoControlDelay();
					WindowsAPI.DetachThreadInput(thinfo.Item1, thinfo.Item2);
				}
			}
		}

		internal override void ControlSetEnabled(object val, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var onoff = Options.ConvertOnOffToggle(val);

				if (Control.FromHandle(item.Handle) is Control ctrl2)
					ctrl2.Enabled = onoff == ToggleValueType.Toggle ? !ctrl2.Enabled : onoff == ToggleValueType.On;
				else if (!WindowsAPI.EnableWindow(item.Handle, onoff == ToggleValueType.Toggle ? !WindowsAPI.IsWindowEnabled(item.Handle) : onoff == ToggleValueType.On))
					throw new Error($"Could not enable control in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				WindowItemBase.DoControlDelay();
			}
		}

		internal override void ControlSetStyle(object val, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (val is int i)
					item.Style = i;
				else if (val is uint ui)
					item.Style = ui;
				else if (val is long l)
					item.Style = l;
				else if (val is double d)
					item.Style = (long)d;
				else if (val is string s)
				{
					long temp = 0;

					if (Options.TryParse(s, "+", ref temp)) { item.Style |= temp; }
					else if (Options.TryParse(s, "-", ref temp)) { item.Style &= ~temp; }
					else if (Options.TryParse(s, "^", ref temp)) { item.Style ^= temp; }
					else item.Style = val.ParseLong(true).Value;
				}
			}
		}

		internal override void ControlSetExStyle(object val, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (val is int i)
					item.ExStyle = i;
				else if (val is uint ui)
					item.ExStyle = ui;
				else if (val is long l)
					item.ExStyle = l;
				else if (val is double d)
					item.ExStyle = (long)d;
				else if (val is string s)
				{
					long temp = 0;

					if (Options.TryParse(s, "+", ref temp)) { item.ExStyle |= temp; }
					else if (Options.TryParse(s, "-", ref temp)) { item.ExStyle &= ~temp; }
					else if (Options.TryParse(s, "^", ref temp)) { item.ExStyle ^= temp; }
					else item.ExStyle = val.ParseLong(true).Value;
				}
			}
		}

		internal override void ControlSetText(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (Control.FromHandle(item.Handle) is Control ctrl2)//No matter what I've tried, sending WM_SETTEXT will not work with buttons, however this method works flawlessly.
					ctrl2.Text = str;
				else
					item.Title = str;//Just in case... it seems to work on text boxes.

				WindowItemBase.DoControlDelay();
			}
		}

		internal override void ControlShow(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (Control.FromHandle(item.Handle) is Control ctrl2)
					ctrl2.Visible = true;
				else
					_ = item.Show();

				WindowItemBase.DoControlDelay();
			}
		}

		internal override void ControlShowDropDown(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.CB_SHOWDROPDOWN, 1, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var _) == 0)
					throw new Error($"Could not hide combo box drop down for in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				WindowItemBase.DoControlDelay();
			}
		}

		internal override long EditGetCurrentCol(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				_ = WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.EM_GETSEL, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result);
				var val = result.ToInt64() & 0xFFFF;

				if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.EM_LINEFROMCHAR, (uint)val, 0u, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000u, out var result2) == 0)
					throw new Error($"Could not get line form character position for text box in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				var resint = result2.ToInt32();

				if (resint == 0)//Since we're on line zero, the column number is simply start+1.
					return val + 1;

				if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.EM_LINEINDEX, (uint)result2.ToInt32(), 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var line_start) == 0)
					throw new Error($"Could not get line line index from character position for text box in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				return val - line_start.ToInt64() + 1L;
			}

			return 0L;
		}

		internal override long EditGetCurrentLine(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.EM_LINEFROMCHAR, -1, null, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result) == 0)
					throw new Error($"Could not get current line index for text box in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				return result.ToInt64() + 1L;
			}

			return 0L;
		}

		internal override string EditGetLine(int n, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				n--;
				var buffer = new StringBuilder(32767);
				buffer.Append((char)buffer.Capacity);

				if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.EM_GETLINE, (uint)n, buffer, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var _) == 0)
					throw new Error($"Could not get line for text box in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				if (buffer.Length == 0)
				{
					if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.EM_GETLINECOUNT, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var linecount) == 0)
						throw new Error($"Could not get line count for text box in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

					if (n + 1 > linecount.ToInt32())
						throw new Error($"Requested line of {n + 1} is greater than the number of lines ({linecount.ToInt32()}) in the text box in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
				}

				return buffer.ToString();
			}

			return "";
		}

		internal override long EditGetLineCount(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				//Don't try to cast to TextBox control here because it handles lines differently: wordwrapping doesn't count as a new line, whereas it does with EM_GETLINECOUNT;
				if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.EM_GETLINECOUNT, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result) == 0)
					throw new Error($"Could not get line count for text box in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				return result.ToInt64();
			}

			return 0L;
		}

		internal override string EditGetSelectedText(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (Control.FromHandle(item.Handle) is TextBox ctrl2)
					return ctrl2.SelectedText;

				_ = WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.EM_GETSEL, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result);
				var res = result.ToInt32();
				var start = Conversions.LowWord(res);
				var end = Conversions.HighWord(res);

				if (start == end)
					return "";

				if (WindowsAPI.GetWindowTextTimeout(item.Handle, 2000) is string s)
					return s[start..end];
			}

			return "";
		}

		internal override void EditPaste(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.EM_REPLACESEL, 1, str, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result) == 0)
					throw new Error($"Could not paste into text box in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				WindowItemBase.DoControlDelay();
			}
		}

		internal override object ListViewGetContent(string options, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var opts = Options.ParseOptions(options);
				var focused = false;
				var count = false;
				var sel = false;
				var countcol = false;
				var col = int.MinValue;
				object ret = null;

				foreach (var opt in opts)
				{
					if (string.Compare(opt, "focused", true) == 0) { focused = true; }
					else if (string.Compare(opt, "count", true) == 0) { count = true; }
					else if (string.Compare(opt, "selected", true) == 0) { sel = true; }
					else if (string.Compare(opt, "col", true) == 0) { countcol = true; }
					else if (Options.TryParse(opt, "col", ref col)) { col--; }
				}

				if (Control.FromHandle(item.Handle) is ListView lv)
				{
					if (count && sel)
						ret = lv.SelectedItems.Count;
					else if (count && focused)
						ret = lv.FocusedItem is ListViewItem lvi ? lvi.Index + 1 : (object)0;
					else if (count && countcol)
						ret = lv.Columns.Count;
					else if (count)
						ret = lv.Items.Count;
					else
					{
						var sb = new StringBuilder(1024);
						var items = new List<ListViewItem>();

						if (focused)
						{
							if (lv.FocusedItem is ListViewItem lvi)
								items.Add(lvi);
						}
						else if (sel)
							items.AddRange(lv.SelectedItems.Cast<ListViewItem>());
						else
							items.AddRange(lv.Items.Cast<ListViewItem>());

						if (col >= 0)
						{
							if (col >= lv.Columns.Count)
								throw new ValueError($"Column ${col + 1} is greater than list view column count of {lv.Columns.Count} in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

							items.ForEach(templvi => sb.AppendLine(templvi.SubItems[col].Text));
						}
						else
							items.ForEach(templvi => sb.AppendLine(string.Join('\t', templvi.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(x => x.Text))));

						ret = sb.ToString();
					}
				}
				else
				{
					if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.LVM_GETITEMCOUNT, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var row_count) == 0)
						throw new TargetError($"Could not get row count for list view in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

					var col_count = new IntPtr(-1);  // Fix for v1.0.37.01: Use -1 to indicate "undetermined col count".

					if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.LVM_GETHEADER, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var header_control) != 0
							&& header_control.ToInt64() != 0) // Relies on short-circuit boolean order.
						_ = WindowsAPI.SendMessageTimeout(header_control, WindowsAPI.HDM_GETITEMCOUNT, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out col_count);

					var rowct = row_count.ToInt64();
					var colct = col_count.ToInt64();

					if (count)
					{
						if (focused) // Listed first so that it takes precedence over include_selected_only.
						{
							if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.LVM_GETNEXTITEM, -1, WindowsAPI.LVNI_FOCUSED, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result) == 0)
								throw new TargetError($"Could not get next item for list view in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

							ret = result.ToInt64() + 1;
						}
						else if (sel)
						{
							if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.LVM_GETSELECTEDCOUNT, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result) == 0)
								throw new TargetError($"Could not get selected item count for list view in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

							ret = result.ToInt64();
						}
						else if (countcol) // "Count Col" returns the number of columns.
							ret = colct;
						else // Total row count.
							ret = rowct;

						return ret;
					}

					if (rowct < 1 || colct == 0) // But don't return when col_count == -1 (i.e. always make the attempt when col count is undetermined).
						return "";  // No text in the control, so indicate success.

					// allocate buffer for a string to store the text of the list view item we wanted
					var remotetext = IntPtr.Zero;
					var remotelvi = IntPtr.Zero;

					if ((remotetext = WindowsAPI.AllocInterProcMem(WindowsAPI.LV_REMOTE_BUF_SIZE, item.Handle, ProcessAccessTypes.PROCESS_QUERY_INFORMATION, out var prochandle)) == IntPtr.Zero)
						throw new TargetError($"Could not allocate inter process string memory for list view in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

					// this is the LVITEM we need to inject
					var lvItem = new LVITEM
					{
						mask = 0x0001,
						cchTextMax = WindowsAPI.LV_REMOTE_BUF_SIZE - 1,
						pszText = remotetext,
						iItem = 0,//itemId,
						iSubItem = 1,//subItemId
					};
					long i, total_length;
					var next = IntPtr.Zero;
					var is_selective = focused || sel;
					var single_col_mode = col > -1 || colct == -1;// Get only one column in these cases.
					var lvItemSize = Marshal.SizeOf(lvItem);
					var lvItemLocalPtr = Marshal.AllocHGlobal(lvItemSize);
					var sb = new StringBuilder(1024);

					if ((remotelvi = WindowsAPI.AllocInterProcMem((uint)lvItemSize, item.Handle, ProcessAccessTypes.PROCESS_QUERY_INFORMATION, out _)) == IntPtr.Zero)
						throw new TargetError($"Could not allocate inter process list view item memory for list view in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

					for (i = 0, next = new IntPtr(-1), total_length = 0; i < rowct; ++i) // For each row:
					{
						if (is_selective)
						{
							// Fix for v1.0.37.01: Prevent an infinite loop that might occur if the target control no longer
							// exists (perhaps having been closed in the middle of the operation) or is permanently hung.
							// If GetLastError() were to return zero after the below, it would mean the function timed out.
							// However, rather than checking and retrying, it seems better to abort the operation because:
							// 1) Timeout should be quite rare.
							// 2) Reduces code size.
							// 3) Having a retry really should be accompanied by SLEEP_WITHOUT_INTERRUPTION because all this
							//    time our thread would not pumping messages (and worse, if the keyboard/mouse hooks are installed,
							//    mouse/key lag would occur).
							if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.LVM_GETNEXTITEM, next.ToInt32(), focused ? WindowsAPI.LVNI_FOCUSED : WindowsAPI.LVNI_SELECTED,
															  SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out next) == 0
									|| next.ToInt32() == -1) // No next item.  Relies on short-circuit boolean order.
								break; // End of estimation phase (if estimate is too small, the text retrieval below will truncate it).
						}
						else
							next = new IntPtr(i);

						for (lvItem.iSubItem = (col > -1) ? col : 0 // iSubItem is which field to fetch. If it's zero, the item vs. subitem will be fetched.
											   ; colct == -1 || lvItem.iSubItem < colct // If column count is undetermined (-1), always make the attempt.
								; ++lvItem.iSubItem) // For each column:
						{
							Marshal.StructureToPtr(lvItem, lvItemLocalPtr, false);

							if (WindowsAPI.WriteProcessMemory(prochandle, remotelvi, lvItemLocalPtr, lvItemSize, out _)
									&& WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.LVM_GETITEMTEXT, next, remotelvi, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var itemlen) != 0)
								total_length += itemlen.ToInt64();

							//else timed out or failed, don't include the length in the estimate.  Instead, the
							// text-fetching routine below will ensure the text doesn't overflow the var capacity.
							if (single_col_mode)
								break;
						}
					}

					// Add to total_length enough room for one linefeed per row, and one tab after each column
					// except the last (formula verified correct, though it's inflated by 1 for safety). "i" contains the
					// actual number of rows that will be transcribed, which might be less than rowct if is_selective==true.
					total_length += i * (single_col_mode ? 1 : colct);
					var capacity = total_length; // LRESULT avoids signed vs. unsigned compiler warnings.

					if (capacity > 0) // For maintainability, avoid going negative.
						--capacity; // Adjust to exclude the zero terminator, which simplifies things below.

					// RETRIEVE THE TEXT FROM THE REMOTE LISTVIEW
					// Start total_length at zero in case actual size is greater than estimate, in which case only a partial set of text along with its '\t' and '\n' chars will be written.
					for (i = 0, next = new IntPtr(-1), total_length = 0; i < rowct; ++i) // For each row:
					{
						if (is_selective)
						{
							// Fix for v1.0.37.01: Prevent an infinite loop (for details, see comments in the estimation phase above).
							if (WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.LVM_GETNEXTITEM, next.ToInt32(), focused ? WindowsAPI.LVNI_FOCUSED : WindowsAPI.LVNI_SELECTED
															  , SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out next) == 0
									|| next.ToInt32() == -1) // No next item.
								break; // See comment above for why unconditional break vs. continue.
						}
						else // Retrieve every row, so the "next" row becomes the "i" index.
							next = new IntPtr(i);

						// Insert a linefeed before each row except the first:
						if (i != 0 && total_length < capacity) // If we're at capacity, it will exit the loops when the next field is read.
						{
							_ = sb.AppendLine();
							++total_length;
						}

						var localTextBuffer = new byte[WindowsAPI.LV_TEXT_BUF_SIZE];

						// iSubItem is which field to fetch. If it's zero, the item vs. subitem will be fetched:
						for (lvItem.iSubItem = (col > -1) ? col : 0
											   ; colct == -1 || lvItem.iSubItem < colct // If column count is undetermined (-1), always make the attempt.
								; ++lvItem.iSubItem) // For each column:
						{
							// Insert a tab before each column except the first and except when in single-column mode:
							if (!single_col_mode && lvItem.iSubItem != 0 && total_length < capacity)  // If we're at capacity, it will exit the loops when the next field is read.
							{
								_ = sb.Append('\t');
								++total_length;
							}

							Marshal.StructureToPtr(lvItem, lvItemLocalPtr, false);

							if (!WindowsAPI.WriteProcessMemory(prochandle, remotelvi, lvItemLocalPtr, lvItemSize, out _)
									|| WindowsAPI.SendMessageTimeout(item.Handle, WindowsAPI.LVM_GETITEMTEXT, next, remotelvi, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var templen) == 0)
								continue; // Timed out or failed. It seems more useful to continue getting text rather than aborting the operation.

							var length = (uint)templen.ToInt32();

							// Otherwise, the message was successfully sent.
							if (length > 0)
							{
								if (total_length + length > capacity)
									goto break_both; // "goto" for simplicity and code size reduction.

								// Otherwise:
								// READ THE TEXT FROM THE REMOTE PROCESS
								// Although MSDN has the following comment about LVM_GETITEM, it is not present for
								// LVM_GETITEMTEXT. Therefore, to improve performance (by avoiding a second call to
								// ReadProcessMemory) and to reduce code size, we'll take them at their word until
								// proven otherwise.  Here is the MSDN comment about LVM_GETITEM: "Applications
								// should not assume that the text will necessarily be placed in the specified
								// buffer. The control may instead change the pszText member of the structure
								// to point to the new text, rather than place it in the buffer."
								if (WindowsAPI.ReadProcessMemory(prochandle, remotetext, localTextBuffer, length * 2, out var bytesread))
								{
									var itemtext = System.Text.Encoding.Unicode.GetString(localTextBuffer.AsSpan().Slice(0, (int)bytesread));
									_ = sb.Append(itemtext);
									total_length += length; // Recalculate length in case its different than the estimate (for any reason).
								}

								//else it failed; but even so, continue on to put in a tab (if called for).
							}

							//else length is zero; but even so, continue on to put in a tab (if called for).
							if (single_col_mode)
								break;
						}
					}

					break_both:
					// finally free all the memory we allocated, and close the process handle we opened
					_ = WindowsAPI.VirtualFreeEx(prochandle, remotelvi, 0, VirtualAllocExTypes.MEM_RELEASE);
					_ = WindowsAPI.VirtualFreeEx(prochandle, remotetext, 0, VirtualAllocExTypes.MEM_RELEASE);
					Marshal.FreeHGlobal(lvItemLocalPtr);
					_ = WindowsAPI.CloseHandle(prochandle);
					ret = sb.ToString();
				}

				WindowItemBase.DoControlDelay();
				return ret;
			}

			return "";
		}

		internal override void MenuSelect(object title, string text, string menu, string sub1, string sub2, string sub3, string sub4, string sub5, string sub6, string excludeTitle, string excludeText)
		{
			if (Window.SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true) is WindowItem win)
			{
				var hmenu = win.GetMenuItemId(menu, sub1, sub2, sub3, sub4, sub5, sub6);

				if (hmenu != 0)
				{
					_ = WindowsAPI.PostMessage(win.Handle, WindowsAPI.WM_COMMAND, hmenu, 0u);
					WindowItemBase.DoWinDelay();
				}
			}
		}

		internal void NotifyParent(IntPtr handle, uint x_msg, uint y_msg)
		{
			var immediate_parent = WindowsAPI.GetParent(handle);

			if (immediate_parent == IntPtr.Zero)
				throw new TargetError($"Parent is null");

			WindowsAPI.SetLastError(0);//Must be done to differentiate between success and failure when control has ID 0.
			var control_id = WindowsAPI.GetDlgCtrlID(handle);

			if (control_id == 0 && WindowsAPI.GetLastError() != 0)
				throw new OSError("", $"Last error was not zero");

			// Both conditions must be checked (see above).

			// Proceed even if control_id == 0, since some applications are known to
			// utilize the notification in that case (e.g. Notepad's Save As dialog).
			if (WindowsAPI.SendMessageTimeout(immediate_parent, WindowsAPI.WM_COMMAND, (uint)KeyboardUtils.MakeLong((short)control_id, (short)x_msg)
											  , handle, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result) == 0)
				throw new TargetError($"Could not send WM_COMMAND message of {x_msg} to ${immediate_parent.ToInt64()}");

			if (WindowsAPI.SendMessageTimeout(immediate_parent, WindowsAPI.WM_COMMAND, (uint)KeyboardUtils.MakeLong((short)control_id, (short)y_msg)
											  , handle, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 2000, out var result2) == 0)
				throw new TargetError($"Could not send WM_COMMAND message of {y_msg} to ${immediate_parent.ToInt64()}");
		}

		internal override void PostMessage(int msg, int wparam, int lparam, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			var thehandle = title.ParseLong(false);
			var item = ctrl != null
					   ? Window.SearchControl(ctrl, title, text, excludeTitle, excludeText)
					   : Window.SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true);

			if (item is WindowItem)
			{
				if (thehandle != WindowsAPI.HWND_BROADCAST)
					thehandle = item.Handle.ToInt64();

				if (!WindowsAPI.PostMessage(new IntPtr(thehandle.Value), (uint)msg, new IntPtr(wparam), new IntPtr(lparam)))
					throw new Error($"Could not post message with values msg: {msg}, lparam: {lparam}, wparam: {wparam} to control in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
			}

			WindowItemBase.DoControlDelay();
		}

		internal override long SendMessage(int msg, object wparam, object lparam, object ctrl, object title, string text, string excludeTitle, string excludeText, int timeout)
		{
			var thehandle = title.ParseLong(false);
			var wbuf = Reflections.SafeGetProperty<Keysharp.Core.Buffer>(wparam, "Ptr");
			var wptr = wbuf != null ? wbuf.Ptr : wparam != null ? new IntPtr(wparam.ParseLong().Value) : IntPtr.Zero;
			var item = ctrl != null
					   ? Window.SearchControl(ctrl, title, text, excludeTitle, excludeText)
					   : Window.SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true);

			if (thehandle == WindowsAPI.HWND_BROADCAST)
			{
			}
			else
				thehandle = item.Handle.ToInt64();

			long ret;

			if (lparam is string s)
			{
				if (WindowsAPI.SendMessageTimeout(new IntPtr(thehandle.Value), (uint)msg, wptr, s, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, (uint)timeout, out var result) == 0)
					throw new OSError("", $"Could not send message with values msg: {msg}, lparam: {lparam}, wparam: {wparam} to control in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				ret = result.ToInt64();
			}
			else
			{
				var lbuf = Reflections.SafeGetProperty<Keysharp.Core.Buffer>(lparam, "Ptr");
				var lptr = lbuf != null ? lbuf.Ptr : new IntPtr(lparam.ParseLong().Value);

				if (WindowsAPI.SendMessageTimeout(new IntPtr(thehandle.Value), (uint)msg, wptr, lptr, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, (uint)timeout, out var result) == 0)
					throw new OSError("", $"Could not send message with values msg: {msg}, lparam: {lparam}, wparam: {wparam} to control in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				ret = result.ToInt64();
			}

			WindowItemBase.DoControlDelay();
			return ret;
		}
	}
}