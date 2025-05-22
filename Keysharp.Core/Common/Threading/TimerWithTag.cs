using Keysharp.Scripting;

namespace Keysharp.Core.Common.Threading
{
	internal class TimerWithTag : System.Windows.Forms.Timer
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new object Tag { get; set; }

		// When the timer was last (re)started
		private DateTime _lastStart;

		// Guard so we never queue more than one pending invoke
		private bool _pushPending;

		// Whether we’re currently paused
		private bool _isPaused;

		public TimerWithTag() : base()
		{
		}

		public TimerWithTag(double interval) : this()
		{
			Interval = (int)interval;
		}

		/// <summary>
		/// Start (or restart) the timer at the full interval.
		/// </summary>
		public new void Start()
		{
			_lastStart = DateTime.UtcNow;
			_isPaused = false;
			base.Start();
		}

		/// <summary>
		/// Stops entirely and clears any pause state.
		/// </summary>
		public new void Stop()
		{
			base.Stop();
			_isPaused = false;
			_pushPending = false;
		}

		/// <summary>
		/// Pause delivery of Tick events without stopping the internal clock.
		/// </summary>
		public void Pause()
		{
			if (!_isPaused && Enabled)
				_isPaused = true;
		}

		/// <summary>
		/// Resume Tick delivery. If the elapsed time already exceeded the interval,
		/// a tick will fire immediately.
		/// </summary>
		public void Resume()
		{
			if (_isPaused)
			{
				_isPaused = false;
				var elapsed = (DateTime.UtcNow - _lastStart).TotalMilliseconds;
				if (elapsed >= Interval)
					PushToMessageQueue();
			}
		}

		/// <summary>
		/// Pushes a Tick to the main thread message queue immediately.
		/// </summary>
		public void PushToMessageQueue()
		{
			if (_pushPending || !Enabled)
				return;

			_pushPending = true;
			base.Stop(); //Prevent timer from pushing another tick onto the message queue while we are queued

			Script.TheScript.mainWindow.CheckedBeginInvoke(() =>
			{
				_pushPending = false; //Delegate is now running → clear the pending-flag
				OnTick(EventArgs.Empty);
				base.Start(); //Reset the internal counter, because we called the tick manually
			}, false, true);
		}

		/// <summary>
		/// Suppress ticks while paused, otherwise raise them and afterwards reset the start time.
		/// </summary>
		protected override void OnTick(EventArgs e)
		{
			if (_isPaused)
				return;

			base.OnTick(e);
			_lastStart = DateTime.UtcNow;
		}

		/// <summary>
		/// Whether the timer has been paused.
		/// </summary>
		public bool IsPaused => _isPaused;
	}
}