namespace Keysharp.Core.Common.Window
{
	internal class MessageFilter : IMessageFilter
	{
		public bool PreFilterMessage(ref Message m)
		{
			if (GuiHelper.onMessageHandlers.TryGetValue(m.Msg, out var monitor))
			{
				var tv = Threads.GetThreadVariables();

				if (!Threads.AnyThreadsAvailable() || tv.priority > 0)
					return false;

				if (monitor.instanceCount >= monitor.maxInstances)
					return false;

#if WINDOWS
				Script.HwndLastUsed = WindowsAPI.GetNonChildParent(m.HWnd);//Assign parent window as the last found window (it's ok if it's hidden).
#endif
				var now = DateTime.Now;
				Script.lastPeekTime = now;
				A_EventInfo = now;//AHK used msg.time, but the C# version does not have a time field.
				monitor.instanceCount++;
				object res = null;

				try
				{
					res = monitor.funcs.InvokeEventHandlers(m.WParam.ToInt64(), m.LParam.ToInt64(), m.Msg, m.HWnd.ToInt64());
				}
				finally
				{
					monitor.instanceCount--;
				}

				if (res != null && res.IsNotNullOrEmpty())
					return true;
			}

			return false;
		}
	}
}