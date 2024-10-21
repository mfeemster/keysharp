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
		internal IntPtr hwnd = IntPtr.Zero;

		internal IntPtr lParam = IntPtr.Zero;
		internal uint message;
		internal object obj;

		//internal System.Drawing.Point pt;
		//internal uint time;
		internal IntPtr wParam = IntPtr.Zero;
	}
}