using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keysharp.Core.Common.Keyboard
{
	/// <summary>
	/// Command which holds the current Thread until requested Key was pressed
	/// </summary>
	public class KeyWaitCommand
	{
		private readonly KeyboardMouseSender kbdMsSender;

		private Keys _endKey;
		private System.Threading.Timer _timeoutTimer;
		private bool abort = false;
		private object abortLock = new object();

		/// <summary>
		/// Define the Timeout when the wait process should timeout.
		/// Default is null, means no timeout.
		/// </summary>
		public int? TimeOutVal { get; set; } = null;

		/// <summary>
		/// Trigger on the key Down Event. Default is false, what means Key Up event must be fired.
		/// </summary>
		public bool TriggerOnKeyDown { get; set; } = false;

		internal KeyWaitCommand(KeyboardMouseSender sender) => kbdMsSender = sender ?? throw new ArgumentNullException("keyboardHook");

		/// <summary>
		/// Wait until the key gets pressed.
		/// </summary>
		/// <param name="k"></param>
		public void Wait(Keys k)
		{
			if (k == Keys.None)
				throw new ArgumentException("Must submit valid Key definition!");

			_endKey = k;

			if (TimeOutVal.HasValue)
			{
				_timeoutTimer = new System.Threading.Timer(new System.Threading.TimerCallback(OnTimoutTick));
				_ = _timeoutTimer.Change(TimeOutVal.Value, Timeout.Infinite);
			}

			kbdMsSender.KeyEvent += OnKeyEvent;

			while (true)
			{
				lock (abortLock)
				{
					if (abort)
						break;
				}

				Keysharp.Core.Flow.Sleep(2);
			}

			kbdMsSender.KeyEvent -= OnKeyEvent;
		}

		private void Abort()
		{
			lock (abortLock)
			{
				abort = true;
			}
		}

		/// <summary>
		/// Occurs when a Key is pressed/released
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnKeyEvent(object sender, KeyEventArgs e)
		{
			if (e.Block || e.Handled)
				return;

			if ((e.Keys == _endKey) && (e.Down == TriggerOnKeyDown))
			{
				Abort();
			}
		}

		private void OnTimoutTick(object state)
		{
			var timeoutTimer = (System.Threading.Timer)state;
			timeoutTimer.Dispose();
			Abort();
		}
	}
}