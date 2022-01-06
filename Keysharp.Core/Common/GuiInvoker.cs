using System.Windows.Forms;

namespace Keysharp.Core.Common
{
	internal static class GuiInvoker
	{
		// TODO: GuiInvoker class is probably unnecessary

		public static void Invoke(Control c, Action action)
		{
			_ = c.Invoke(action);
		}

		/// <summary>
		/// Threadsave Text Property setter
		/// </summary>
		/// <param name="control"></param>
		/// <param name="text"></param>
		public static void SetText(Control control, Action action)//Need to generalize this.//MATT
		{
			if (control.IsDisposed || control.Disposing || !control.IsHandleCreated)
				return;

			if (control.InvokeRequired)
				Invoke(control, action);
			else
				action();
		}

		public delegate void Action();
	}
}