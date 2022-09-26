using System.Drawing;
using Keysharp.Core.Common.Window;

namespace Keysharp.Core.Linux
{
	internal class ControlManager : ControlManagerBase
	{
		internal override long ControlAddItem(string str, object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override void ControlChooseIndex(int n, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}

		internal override long ControlChooseString(string str, object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override void ControlClick(object ctrlorpos, object title, string text, string whichButton, int clickCount, string options, string excludeTitle, string excludeText)
		{
		}

		internal override void ControlDeleteItem(int n, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}

		internal override long ControlFindItem(string str, object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override void ControlFocus(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}

		internal override long ControlGetChecked(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override string ControlGetChoice(object ctrl, object title, string text, string excludeTitle, string excludeText) => "";

		internal override string ControlGetClassNN(object ctrl, object title, string text, string excludeTitle, string excludeText) => "";

		internal override long ControlGetEnabled(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override long ControlGetExStyle(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override long ControlGetFocus(object title, string text, string excludeTitle, string excludeText) => 1;

		internal override long ControlGetHwnd(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override long ControlGetIndex(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override Array ControlGetItems(object ctrl, object title, string text, string excludeTitle, string excludeText) => new Array();

		internal override Keysharp.Core.Map ControlGetPos(object ctrl, object title, string text, string excludeTitle, string excludeText) => new Keysharp.Core.Map();

		internal override long ControlGetStyle(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override string ControlGetText(object ctrl, object title, string text, string excludeTitle, string excludeText) => "";

		internal override long ControlGetVisible(object ctrl, object title, string text, string excludeTitle, string excludeText) => 1;

		internal override void ControlHide(object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}

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

		internal override void ControlSetStyle(object val, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}

		internal override void ControlSetExStyle(object val, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}

		internal override void ControlSetText(string str, object ctrl, object title, string text, string excludeTitle, string excludeText)
		{
		}

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