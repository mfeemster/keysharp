using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Scripting;

namespace Keysharp.Core.Common.Threading
{
	public class ThreadVariables : IClearable
	{
		internal Task<object> task = null;
		internal bool allowThreadToBeInterrupted = true;
		internal long controlDelay = 20L;
		internal CoordModes coords;
		internal System.Windows.Forms.Timer currentTimer;
		internal string defaultGui;
		internal long defaultMouseSpeed = 2L;
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
		internal long lastFoundForm = 0L;
		internal long mouseDelay = 10L;
		internal long mouseDelayPlay = -1L;
		internal long peekFrequency = 5L;
		internal long priority;
		internal Random randomGenerator;
		internal StringBuilder regsb = null;
		internal long regView = 64L;
		internal uint sendLevel;
		internal SendModes sendMode = SendModes.Input;
		internal bool storeCapsLockMode = true;
		internal int threadId;
		internal DateTime threadStartTime = DateTime.MinValue;
		internal object titleMatchMode = 2L;
		internal bool titleMatchModeSpeed = true;
		internal int uninterruptibleDuration = 17;
		internal long winDelay = 100L;

		internal bool IsCriticalAndRunning()
		{
			var tempTask = task;
			return tempTask != null && !tempTask.IsCompleted && !allowThreadToBeInterrupted && isCritical;
		}

		internal void WaitForCriticalToFinish()
		{
			if (IsCriticalAndRunning())
			{
				var tempTask = task;
				tempTask?.Wait();
			}
		}

		/// <summary>
		/// The fields in this function must be kept in sync with the fields declared above.
		/// </summary>
		public void Clear()
		{
			task = null;
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
			formatNumeric = null;
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
			regView = 64L;
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
			task = null;
			isCritical = false;
			allowThreadToBeInterrupted = true;
			uninterruptibleDuration = Keysharp.Scripting.Script.uninterruptibleTime;
			threadStartTime = DateTime.MinValue;
			controlDelay = Accessors.A_ControlDelay.Al();
			coords = null;
			currentTimer = null;
			defaultGui = null;
			defaultMouseSpeed = Accessors.A_DefaultMouseSpeed.Al();
			detectHiddenText = Accessors.A_DetectHiddenText.Ab();
			detectHiddenWindows = Accessors.A_DetectHiddenWindows.Ab();
			dialogOwner = null;
			eventInfo = null;
			fileEncoding = Files.GetEncoding(Accessors.A_FileEncoding.ToString());
			formatNumeric = null;
			hotCriterion = null;
			hwndLastUsed = IntPtr.Zero;
			keyDelay = Accessors.A_KeyDelay.Al();
			keyDelayPlay = Accessors.A_KeyDelayPlay.Al();
			keyDuration = Accessors.A_KeyDuration.Al();
			keyDurationPlay = Accessors.A_KeyDurationPlay.Al();
			lastFoundForm = IntPtr.Zero;
			mouseDelay = Accessors.A_MouseDelay.Al();
			mouseDelayPlay = Accessors.A_MouseDelayPlay.Al();
			peekFrequency = Accessors.A_PeekFrequency.Al();
			priority = Accessors.A_Priority.Al();
			randomGenerator = null;
			_ = (regsb?.Clear());
			regView = Accessors.A_RegView.Al();
			sendLevel = Accessors.A_SendLevel.Aui();

			if (Enum.TryParse<SendModes>(Accessors.A_SendMode.As(), out var temp))
				sendMode = temp;

			storeCapsLockMode = Accessors.A_StoreCapsLockMode.Ab();
			threadId = 0;
			titleMatchMode = Accessors.A_TitleMatchMode;
			titleMatchModeSpeed = Accessors.A_TitleMatchModeSpeed.ToString() == Keywords.Keyword_Fast;
			winDelay = Accessors.A_WinDelay.Al();
		}
	}
}