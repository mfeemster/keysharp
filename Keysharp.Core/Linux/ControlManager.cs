#if LINUX

namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of ControlManager for the linux platfrom.
	/// </summary>
	internal class ControlManager : ControlManagerBase
	{
		internal override long ControlAddItem(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var res = 0L;
				var ctrl2 = Control.FromHandle(item.Handle);

				if (ctrl2 is ComboBox cb)
					res = cb.Items.Add(str);
				else if (ctrl2 is ListBox lb)
					res = lb.Items.Add(str);
				else
				{
					//How to do the equivalent of what the Windows derivation does, but on linux?
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
				var ctrl2 = Control.FromHandle(item.Handle);
				n--;

				if (ctrl2 is ComboBox cb)
				{
					if (n >= 0)
						cb.SelectedIndex = n;
					else
						cb.SelectedIndex = -1;
				}
				else if (ctrl2 is ListBox lb)
				{
					if (n >= 0)
					{
						lb.SelectedIndex = n;

						if (lb.GetGuiControl() is GuiControl gc)
							gc._control_DoubleClick(lb, new EventArgs());
					}
					else
						lb.SelectedIndex = -1;
				}
				else if (ctrl2 is TabControl tc)
				{
					tc.SelectedIndex = n;
				}
				else
				{
					//How to do the equivalent of what the Windows derivation does, but on linux?
				}

				WindowItemBase.DoControlDelay();
			}
		}

		internal override long ControlChooseString(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			var index = 0L;

			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var ctrl2 = Control.FromHandle(item.Handle);

				if (ctrl2 is ComboBox cb)
				{
					index = cb.FindString(str);
					cb.SelectedIndex = (int)index;
				}
				else if (ctrl2 is ListBox lb)
				{
					index = lb.FindString(str);
					lb.SelectedIndex = (int)index;

					if (index >= 0)
					{
						if (lb.GetGuiControl() is GuiControl gc)
							gc._control_DoubleClick(lb, new EventArgs());
					}
				}
				else
				{
					//How to do the equivalent of what the Windows derivation does, but on linux?
				}

				WindowItemBase.DoControlDelay();
			}

			return index;
		}

		internal override void ControlClick(object ctrlorpos, object title, string text, string whichButton, int clickCount, string options, string excludeTitle, string excludeText)
		{
		}

		internal override void ControlDeleteItem(int n, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var ctrl2 = Control.FromHandle(item.Handle);
				n--;

				if (ctrl2 is ComboBox cb)
				{
					cb.Items.RemoveAt(n);
					cb.SelectedIndex = -1;//On linux, if the selected item is deleted, it will throw an exception the next time the dropdown is clicked if SelectedIndex is not set to -1.
				}
				else if (ctrl2 is ListBox lb)
				{
					lb.Items.RemoveAt(n);
				}
				else
				{
					//How to do the equivalent of what the Windows derivation does, but on linux?
				}

				WindowItemBase.DoControlDelay();
			}
		}

		internal override long ControlFindItem(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var ctrl2 = Control.FromHandle(item.Handle);

				if (ctrl2 is ComboBox cb)
					return cb.Items.IndexOf(str) + 1L;
				else if (ctrl2 is ListBox lb)
					return lb.Items.IndexOf(str) + 1L;
				else
				{
					//How to do the equivalent of what the Windows derivation does, but on linux?
				}
			}

			return 0L;
		}

		internal override void ControlFocus(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (Control.FromHandle(item.Handle) is Control ctrl2)
					ctrl2.Focus();
				else
					item.Active = true;//Will not work for X11.//TODO
			}
		}

		internal override long ControlGetChecked(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var ctrl2 = Control.FromHandle(item.Handle);

				if (ctrl2 is CheckBox cb)
					return cb.Checked ? 1L : 0L;
				else
				{
					//How to do the equivalent of what the Windows derivation does, but on linux?
				}
			}

			return 0L;
		}

		internal override string ControlGetChoice(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				var ctrl2 = Control.FromHandle(item.Handle);

				if (ctrl2 is ComboBox cb)
					return cb.SelectedItem != null ? cb.SelectedItem.ToString() : "";
				else if (ctrl2 is ListBox lb)
					return lb.SelectedItem != null ? lb.SelectedItem.ToString() : "";
				else
				{
					//How to do the equivalent of what the Windows derivation does, but on linux?
				}

				WindowItemBase.DoControlDelay();
			}

			return "";
		}

		internal override string ControlGetClassNN(object ctrl, object title, string text, string excludeTitle, string excludeText) => "";
		internal override long ControlGetExStyle(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;
		internal override long ControlGetFocus(object title, string text, string excludeTitle, string excludeText) => 1;
		internal override long ControlGetIndex(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;
		internal override Array ControlGetItems(object ctrl, object title, string text, string excludeTitle, string excludeText) => new Array();
		internal override void ControlGetPos(ref object outX, ref object outY, ref object outWidth, ref object outHeight, object ctrl = null, string title = null, string text = null, string excludeTitle = null, string excludeText = null)
		{ }
		internal override long ControlGetStyle(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override string ControlGetText(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			var val = "";

			if (Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
				val = Control.FromHandle(item.Handle) is Control ctrl2 ? ctrl2.Text : item.Title;

			return val;
		}

		internal override long ControlGetVisible(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override void ControlHideDropDown(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		internal override void ControlMove(int x, int y, int width, int height, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		internal override void ControlSend(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		internal override void ControlSendText(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		internal override void ControlSetChecked(object val, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		internal override void ControlSetEnabled(object val, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		internal override void ControlSetExStyle(object val, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		internal override void ControlSetStyle(object val, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		//internal override void ControlSetText(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		//{
		//}
		internal override void ControlShow(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		internal override void ControlShowDropDown(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		internal override long EditGetCurrentCol(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;
		internal override long EditGetCurrentLine(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;
		internal override string EditGetLine(int n, object ctrl, object title, string text, string excludeTitle, string excludeText) => "";
		internal override long EditGetLineCount(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;
		internal override string EditGetSelectedText(object ctrl, object title, string text, string excludeTitle, string excludeText) => "";
		internal override void EditPaste(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		internal override object ListViewGetContent(string options, object ctrl, object title, string text, string excludeTitle, string excludeText) => "";
		internal override void MenuSelect(object title, string text, string menu, string sub1, string sub2, string sub3, string sub4, string sub5, string sub6, string excludeTitle, string excludeText)
		{
		}
		internal override void PostMessage(int msg, int wparam, int lparam, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}
		internal override long SendMessage(int msg, object wparam, object lparam, object ctrl, object title, string text, string excludeTitle, string excludeText, int timeout) => 1;
	}
}

#endif