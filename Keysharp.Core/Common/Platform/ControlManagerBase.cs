namespace Keysharp.Core.Common.Platform
{
	internal abstract class ControlManagerBase
	{
		internal abstract long ControlAddItem(string str, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void ControlChooseIndex(int n, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract long ControlChooseString(string str, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void ControlClick(object ctrlorpos, object title, string text, string whichButton, int clickCount, string options, string excludeTitle, string excludeText);

		internal abstract void ControlDeleteItem(int n, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract long ControlFindItem(string str, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void ControlFocus(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract long ControlGetChecked(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract string ControlGetChoice(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract string ControlGetClassNN(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal virtual long ControlGetEnabled(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Keysharp.Core.Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
				return item.Enabled ? 1L : 0L;

			return 0L;
		}

		internal abstract long ControlGetExStyle(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract long ControlGetFocus(object title, string text, string excludeTitle, string excludeText);

		internal virtual long ControlGetHwnd(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Keysharp.Core.Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
				return item.Handle.ToInt64();
			else
				return 0L;
		}

		internal abstract long ControlGetIndex(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract Array ControlGetItems(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void ControlGetPos(ref object outX, ref object outY, ref object outWidth, ref object outHeight, object ctrl = null, string title = null, string text = null, string excludeTitle = null, string excludeText = null);

		internal abstract long ControlGetStyle(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract string ControlGetText(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal virtual long ControlGetVisible(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Keysharp.Core.Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (Control.FromHandle(item.Handle) is Control ctrl2)
					return ctrl2.Visible ? 1L : 0L;
				else
					_ = item.Visible;
			}

			return 0L;
		}

		internal virtual void ControlHide(object ctrl, object title, string text, string excludeTitle, string excludeText) =>
		ShowHideHelper(false, ctrl, title, text, excludeTitle, excludeText);

		internal abstract void ControlHideDropDown(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void ControlMove(int x, int y, int width, int height, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void ControlSend(string str, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void ControlSendText(string str, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void ControlSetChecked(object val, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void ControlSetEnabled(object val, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void ControlSetExStyle(object val, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void ControlSetStyle(object val, object ctrl, object title, string text, string excludeTitle, string excludeText);

		//internal abstract void ControlSetText(string str, object ctrl, object title, string text, string excludeTitle, string excludeText);
		internal virtual void ControlSetText(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Keysharp.Core.Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (Control.FromHandle(item.Handle) is Control ctrl2)//No matter what I've tried, sending WM_SETTEXT will not work with buttons, however this method works flawlessly.
					ctrl2.Text = str;
				else
					item.Title = str;//Just in case... it seems to work on text boxes.

				WindowItemBase.DoControlDelay();
			}
		}

		internal virtual void ControlShow(object ctrl, object title, string text, string excludeTitle, string excludeText) =>
		ShowHideHelper(true, ctrl, title, text, excludeTitle, excludeText);

		internal abstract void ControlShowDropDown(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract long EditGetCurrentCol(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract long EditGetCurrentLine(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract string EditGetLine(int n, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract long EditGetLineCount(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract string EditGetSelectedText(object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void EditPaste(string str, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract object ListViewGetContent(string options, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract void MenuSelect(object title, string text, string menu, string sub1, string sub2, string sub3, string sub4, string sub5, string sub6, string excludeTitle, string excludeText);

		internal abstract void PostMessage(int msg, int wparam, int lparam, object ctrl, object title, string text, string excludeTitle, string excludeText);

		internal abstract long SendMessage(int msg, object wparam, object lparam, object ctrl, object title, string text, string excludeTitle, string excludeText, int timeout);

		private static void ShowHideHelper(bool val, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
			if (Keysharp.Core.Window.SearchControl(ctrl, title, text, excludeTitle, excludeText) is WindowItem item)
			{
				if (Control.FromHandle(item.Handle) is Control ctrl2)
					ctrl2.Visible = val;
				else
					_ = val ? item.Show() : item.Hide();

				WindowItemBase.DoControlDelay();
			}
		}
	}
}