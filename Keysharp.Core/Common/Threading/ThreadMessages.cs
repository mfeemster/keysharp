namespace Keysharp.Core.Common.Threading
{
	internal class HotstringMsg
	{
		internal CaseConformModes caseMode = CaseConformModes.None;
		internal char endChar = (char)0;

		//Might want to add skipchars here.//TODO
		internal HotstringDefinition hs = null;
	}

	internal class KeysharpMsg
	{
		//internal bool completed;
		internal nint hwnd = 0;

		internal nint lParam = 0;
		internal uint message;
		internal object obj;

		//internal System.Drawing.Point pt;
		//internal uint time;
		internal nint wParam = 0;
	}
}