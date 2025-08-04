namespace Keysharp.Core.Common.Window
{
	internal class MessageFilter : IMessageFilter
	{
		Script script;
		internal Message lastMsg;
		internal MessageFilter(Script associatedScript)
		{
			script = associatedScript;
		}

		internal bool CallEventHandlers(ref Message m, bool isPreFilter = true)
		{
			if (script.GuiData.onMessageHandlers.TryGetValue(m.Msg, out var monitor))
			{
				var tv = script.Threads.CurrentThread;

				if (!script.Threads.AnyThreadsAvailable() || tv.priority > 0)
					return false;

				if (monitor.instanceCount >= monitor.maxInstances)
					return false;

#if WINDOWS
				script.HwndLastUsed = WindowsAPI.GetNonChildParent(m.HWnd);//Assign parent window as the last found window (it's ok if it's hidden).
#endif
				var now = DateTime.UtcNow;
				script.lastPeekTime = now;
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

		public bool PreFilterMessage(ref Message m)
		{
			if (m.HWnd != 0)
			{
				var ctl = Control.FromHandle(m.HWnd);
				if (ctl == null || !(ctl.FindForm() is KeysharpForm))
					return false;
			}
			lastMsg = m;
			return CallEventHandlers(ref m);
		}
	}
}