namespace Keysharp.Core
{
	public static class EditX
	{
		public static long EditGetCurrentCol(object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null) => script.ControlProvider.Manager.EditGetCurrentCol(
													 control,
													 winTitle,
													 winText,
													 excludeTitle,
													 excludeText);

		public static long EditGetCurrentLine(object control,
											  object winTitle = null,
											  object winText = null,
											  object excludeTitle = null,
											  object excludeText = null) => script.ControlProvider.Manager.EditGetCurrentLine(
													  control,
													  winTitle,
													  winText,
													  excludeTitle,
													  excludeText);

		public static string EditGetLine(object n,
										 object control,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null) => script.ControlProvider.Manager.EditGetLine(
											 n.Ai(),
											 control,
											 winTitle,
											 winText,
											 excludeTitle,
											 excludeText);

		public static long EditGetLineCount(object control,
											object winTitle = null,
											object winText = null,
											object excludeTitle = null,
											object excludeText = null) => script.ControlProvider.Manager.EditGetLineCount(
												control,
												winTitle,
												winText,
												excludeTitle,
												excludeText);

		public static string EditGetSelectedText(object control,
				object winTitle = null,
				object winText = null,
				object excludeTitle = null,
				object excludeText = null) => script.ControlProvider.Manager.EditGetSelectedText(
					control,
					winTitle,
					winText,
					excludeTitle,
					excludeText);

		public static object EditPaste(object @string,
									   object control,
									   object winTitle = null,
									   object winText = null,
									   object excludeTitle = null,
									   object excludeText = null)
		{
			script.ControlProvider.Manager.EditPaste(
				@string.As(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}
	}
}