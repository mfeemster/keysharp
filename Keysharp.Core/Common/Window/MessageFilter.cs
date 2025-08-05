namespace Keysharp.Core.Common.Window
{
	internal class MessageFilter : IMessageFilter
	{
		Script script;
		internal Message? handledMsg;
		internal MessageFilter(Script associatedScript)
		{
			script = associatedScript;
		}

		internal bool CallEventHandlers(ref Message m)
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

				m.Result = (nint)Script.ForceLong(res);

				if (m.Result != 0)
					return true;
			}

			return false;
		}

		public bool PreFilterMessage(ref Message m)
		{
			if (m.HWnd != 0)
			{
				// Ignore IME windows and other helper forms
				var ctl = Control.FromHandle(m.HWnd);
				if (ctl == null || !(ctl.FindForm() is KeysharpForm))
					return false;
			}
			// Stash the message for later comparison in WndProc to determine whether it's already
			// been handled here. See more thorough description in KeysharpForm.cs WndProc.
			handledMsg = m;
			return CallEventHandlers(ref m);
		}
	}
}