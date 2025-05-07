using Timer = Keysharp.Core.Common.Threading.TimerWithTag;

namespace Keysharp.Core.Common.Threading
{
	public class ThreadVariables
	{
		//internal Task<object> task = null;
		internal bool task = false;
		internal bool allowThreadToBeInterrupted = true;
		internal long controlDelay = 20L;
		internal Timer currentTimer;
		internal string defaultGui;
		internal long defaultMouseSpeed = 2L;
		internal static long DefaultPeekFrequency = 5L;
		internal static long DefaultUninterruptiblePeekFrequency = 16L;
		internal bool detectHiddenText = true;
		internal bool detectHiddenWindows;
		internal Form dialogOwner;
		internal object eventInfo;
		internal Encoding fileEncoding = Encoding.Default;
		internal string formatNumeric;
		internal IFuncObj hotCriterion;
		internal IntPtr hwndLastUsed = IntPtr.Zero;
		internal bool isCritical = false;
		internal long keyDelay = 10L;
		internal long keyDelayPlay = -1L;
		internal long keyDuration = -1L;
		internal long keyDurationPlay = -1L;
		internal long lastFoundForm;
		internal long mouseDelay = 10L;
		internal long mouseDelayPlay = -1L;
		internal long peekFrequency = 5L;
		internal long priority;
#if WINDOWS
		internal long regView = 64L;
#endif
		internal uint sendLevel;
		internal SendModes sendMode = SendModes.Input;
		internal bool storeCapsLockMode = true;
		internal int threadId;
		internal DateTime threadStartTime = DateTime.MinValue;
		internal object titleMatchMode = 2L;
		internal bool titleMatchModeSpeed = true;
		internal int uninterruptibleDuration = 17;
		internal long winDelay = 100L;
		private CoordModes coords;
		private Random randomGenerator;
		private StringBuilder regsb = null;

		internal CoordModes Coords => coords != null ? coords : coords = new CoordModes();

		internal Random RandomGenerator
		{
			get => randomGenerator != null ? randomGenerator : randomGenerator = new Random((int)(DateTime.UtcNow.Ticks & 0xFFFFFFFF));
			set => randomGenerator = value;
		}

		internal StringBuilder RegSb => regsb != null ? regsb : regsb = new StringBuilder(1024);

		internal bool IsCriticalAndRunning()
		{
			var tempTask = task;
			//return tempTask != null && !tempTask.IsCompleted && !allowThreadToBeInterrupted && isCritical;
			return tempTask && !allowThreadToBeInterrupted && isCritical;
		}

		internal void WaitForCriticalToFinish()
		{
			//if (IsCriticalAndRunning())
			while (IsCriticalAndRunning())
			{
				//var tempTask = task;
				//tempTask?.Wait();
				Thread.Sleep(50);
			}
		}

		/// <summary>
		/// The fields in this function must be kept in sync with the fields declared above.
		/// </summary>
		public void Clear()
		{
			task = false;// null;
			isCritical = false;
			allowThreadToBeInterrupted = true;
			uninterruptibleDuration = 17;
			threadStartTime = DateTime.MinValue;
			controlDelay = 20L;
			coords = null;
			currentTimer = null;
			defaultGui = null;
			defaultMouseSpeed = 2L;
			detectHiddenText = true;
			detectHiddenWindows = false;
			dialogOwner = null;
			eventInfo = null;
			fileEncoding = Encoding.Default;
			hotCriterion = null;
			hwndLastUsed = IntPtr.Zero;
			keyDelay = 10L;
			keyDelayPlay = -1L;
			keyDuration = -1L;
			keyDurationPlay = -1L;
			lastFoundForm = 0L;
			mouseDelay = 10L;
			mouseDelayPlay = -1L;
			peekFrequency = 5L;
			priority = 0L;
			randomGenerator = null;
			_ = (regsb?.Clear());
#if WINDOWS
			regView = 64L;
#endif
			sendLevel = 0;
			sendMode = SendModes.Input;
			storeCapsLockMode = true;
			threadId = 0;
			titleMatchMode = 2L;
			titleMatchModeSpeed = true;
			winDelay = 100L;
		}

		public void Init()
		{
			task = false;// null;
			isCritical = false;
			allowThreadToBeInterrupted = true;
			uninterruptibleDuration = Script.uninterruptibleTime;
			threadStartTime = DateTime.MinValue;
			controlDelay = ControlDelayDefault;
			coords = null;
			currentTimer = null;
			defaultGui = null;
			defaultMouseSpeed = DefaultMouseSpeedDefault;
			detectHiddenText = DetectHiddenTextDefault;
			detectHiddenWindows = DetectHiddenWindowsDefault;
			dialogOwner = null;
			eventInfo = null;
			fileEncoding = FileEncodingDefault;
			hotCriterion = null;
			hwndLastUsed = IntPtr.Zero;
			keyDelay = KeyDelayDefault;
			keyDelayPlay = KeyDelayPlayDefault;
			keyDuration = KeyDurationDefault;
			keyDurationPlay = KeyDurationPlayDefault;
			lastFoundForm = IntPtr.Zero;
			mouseDelay = MouseDelayDefault;
			mouseDelayPlay = MouseDelayPlayDefault;
			peekFrequency = PeekFrequencyDefault;
			priority = A_Priority.Al();
			randomGenerator = null;
			_ = (regsb?.Clear());
#if WINDOWS
			regView = RegViewDefault;
#endif
			sendLevel = SendLevelDefault;
			sendMode = SendModeDefault;
			storeCapsLockMode = StoreCapsLockModeDefault;
			threadId = 0;
			titleMatchMode = TitleMatchModeDefault;
			titleMatchModeSpeed = TitleMatchModeSpeedDefault;
			winDelay = WinDelayDefault;
		}
	}
}