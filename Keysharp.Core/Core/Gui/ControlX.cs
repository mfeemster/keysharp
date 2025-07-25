﻿namespace Keysharp.Core
{
	public static class ControlX
	{
		public static long ControlAddItem(object @string,
										  object control,
										  object winTitle = null,
										  object winText = null,
										  object excludeTitle = null,
										  object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlAddItem(
											  @string.As(),
											  control,
											  winTitle,
											  winText,
											  excludeTitle,
											  excludeText);

		public static object ControlChooseIndex(object n,
												object control,
												object winTitle = null,
												object winText = null,
												object excludeTitle = null,
												object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlChooseIndex(
				n.Ai(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static long ControlChooseString(object @string,
											   object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlChooseString(
													   @string.As(),
													   control,
													   winTitle,
													   winText,
													   excludeTitle,
													   excludeText);

		public static object ControlClick(object ctrlOrPos = null,
										  object title = null,
										  object text = null,
										  object whichButton = null,
										  object clickCount = null,
										  object options = null,
										  object excludeTitle = null,
										  object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlClick(
				ctrlOrPos,
				title,
				text,
				whichButton.As(),
				clickCount.Ai(1),
				options.As(),
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlDeleteItem(object n,
											   object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlDeleteItem(
				n.Ai(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static long ControlFindItem(object @string,
										   object control,
										   object winTitle = null,
										   object winText = null,
										   object excludeTitle = null,
										   object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlFindItem(
											   @string.As(),
											   control,
											   winTitle,
											   winText,
											   excludeTitle,
											   excludeText);

		public static object ControlFocus(object control,
										  object winTitle = null,
										  object winText = null,
										  object excludeTitle = null,
										  object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlFocus(
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static long ControlGetChecked(object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetChecked(
													 control,
													 winTitle,
													 winText,
													 excludeTitle,
													 excludeText);

		public static string ControlGetChoice(object control,
											  object winTitle = null,
											  object winText = null,
											  object excludeTitle = null,
											  object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetChoice(
													  control,
													  winTitle,
													  winText,
													  excludeTitle,
													  excludeText);

		public static string ControlGetClassNN(object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetClassNN(
													   control,
													   winTitle,
													   winText,
													   excludeTitle,
													   excludeText);

		public static long ControlGetEnabled(object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetEnabled(
													 control,
													 winTitle,
													 winText,
													 excludeTitle,
													 excludeText);

		public static long ControlGetExStyle(object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetExStyle(
													 control,
													 winTitle,
													 winText,
													 excludeTitle,
													 excludeText);

		public static long ControlGetFocus(object winTitle = null,
										   object winText = null,
										   object excludeTitle = null,
										   object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetFocus(
											   winTitle,
											   winText,
											   excludeTitle,
											   excludeText);

		public static long ControlGetHwnd(object control,
										  object winTitle = null,
										  object winText = null,
										  object excludeTitle = null,
										  object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetHwnd(
											  control,
											  winTitle,
											  winText,
											  excludeTitle,
											  excludeText);

		public static long ControlGetIndex(object control,
										   object winTitle = null,
										   object winText = null,
										   object excludeTitle = null,
										   object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetIndex(
											   control,
											   winTitle,
											   winText,
											   excludeTitle,
											   excludeText);

		public static object ControlGetItems(object control,
											object winTitle = null,
											object winText = null,
											object excludeTitle = null,
											object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetItems(
												control,
												winTitle,
												winText,
												excludeTitle,
												excludeText);

		public static object ControlGetPos([Optional()][DefaultParameterValue(0)] ref object outX,
										   [Optional()][DefaultParameterValue(0)] ref object outY,
										   [Optional()][DefaultParameterValue(0)] ref object outWidth,
										   [Optional()][DefaultParameterValue(0)] ref object outHeight,
										   object ctrl = null,
										   object title = null,
										   object text = null,
										   object excludeTitle = null,
										   object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlGetPos(
				ref outX,
				ref outY,
				ref outWidth,
				ref outHeight,
				ctrl,
				title,
				text,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static long ControlGetStyle(object control,
										   object winTitle = null,
										   object winText = null,
										   object excludeTitle = null,
										   object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetStyle(
											   control,
											   winTitle,
											   winText,
											   excludeTitle,
											   excludeText);

		public static string ControlGetText(object control,
											object winTitle = null,
											object winText = null,
											object excludeTitle = null,
											object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetText(
												control,
												winTitle,
												winText,
												excludeTitle,
												excludeText);

		public static long ControlGetVisible(object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null) => Script.TheScript.ControlProvider.Manager.ControlGetVisible(
													 control,
													 winTitle,
													 winText,
													 excludeTitle,
													 excludeText);

		public static object ControlHide(object control,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlHide(
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlHideDropDown(object control,
				object winTitle = null,
				object winText = null,
				object excludeTitle = null,
				object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlHideDropDown(
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlMove(object x = null,
										 object y = null,
										 object width = null,
										 object height = null,
										 object control = null,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlMove(
				x.Ai(int.MinValue),
				y.Ai(int.MinValue),
				width.Ai(int.MinValue),
				height.Ai(int.MinValue),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlSend(object keys,
										 object control = null,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlSend(
				keys.As(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlSendText(object keys,
											 object control = null,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlSendText(
				keys.As(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlSetChecked(object newSetting,
											   object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlSetChecked(
				newSetting,
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlSetEnabled(object newSetting,
											   object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlSetEnabled(
				newSetting,
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlSetExStyle(object value,
											   object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlSetExStyle(
				value,
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlSetStyle(object value,
											 object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlSetStyle(
				value,
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlSetText(object newText,
											object control,
											object winTitle = null,
											object winText = null,
											object excludeTitle = null,
											object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlSetText(
				newText.As(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlShow(object control,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlShow(
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}

		public static object ControlShowDropDown(object control,
				object winTitle = null,
				object winText = null,
				object excludeTitle = null,
				object excludeText = null)
		{
			Script.TheScript.ControlProvider.Manager.ControlShowDropDown(
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return DefaultObject;
		}
	}
}