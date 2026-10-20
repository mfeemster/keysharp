namespace Keysharp.Tests;

[TestFixture, NonParallelizable]
public class TimerSchedulerTests : TestRunner
{
	[Test, Category("Threading")]
	public void BlockedAdmission() => Assert.IsTrue(TestScript("timer-blocked-admission", false));

	[Test, Category("Threading")]
	public void BlockedTimer()
	{
		var output = RunScript(string.Concat(path, "timer-blocked-fires-once.ahk"), "timer-blocked-fires-once", true, false);
		Assert.That(output.Trim(), Is.EqualTo("pass"), output);
	}

	[Test, Category("Threading")]
	public void CallbackSleep() => Assert.IsTrue(TestScript("timer-during-callback-sleep", false));

	[Test, Category("Threading")]
	public void LongTimerCoalescing() => Assert.IsTrue(TestScript("timer-long-callback-coalesces", false));

	[TestCase(false), TestCase(true), Category("Threading"), Category("Internal"), Category("Curated")]
	public void MenuVisible(bool runOnce)
	{
		_ = UseQueuedMainContext();
		var scheduler = s.EventScheduler;
		var timers = s.FlowData.timers;
		var calls = 0;
		var callbackThreadId = 0;
		var timer = timers.Upsert(new KeysharpFunc((Func<object>)(() =>
		{
			calls++;
			callbackThreadId = Environment.CurrentManagedThreadId;
			return 0L;
		})), scheduler, 60_000L, runOnce, 0L);
		s.SetMenuVisible(1, true);

		try
		{
			timer.NextDueTick = 0;
			scheduler.PumpThreadQueuedEventsCore();
			Assert.That(calls, Is.EqualTo(1));
			Assert.That(callbackThreadId, Is.EqualTo(Environment.CurrentManagedThreadId));

			if (!runOnce)
				timer.NextDueTick = 0;

			scheduler.PumpThreadQueuedEventsCore();
			Assert.That(calls, Is.EqualTo(runOnce ? 1 : 2));
			Assert.That(s.IsMenuVisible, Is.True);
		}
		finally
		{
			s.SetMenuVisible(1, false);

			if (timer.IsActive)
				timers.DisableOrDelete(timer);
		}
	}

	[Test, Category("Threading"), Category("Internal")]
	public void NotificationDuringPump()
	{
		var context = UseQueuedMainContext();
		var scheduler = s.EventScheduler;
		_ = scheduler.EnqueueCallback(() =>
		{
			scheduler.WakeForTimerCheck();
			scheduler.WakeForTimerCheck();
		});

		scheduler.PumpThreadQueuedEventsCore();
		// The original post is still in the test transport; the active pass must add one follow-up.
		Assert.That(context.PendingCount, Is.EqualTo(2));
		context.DrainAll();
		Assert.That(context.PendingCount, Is.Zero);
	}

	[Test, Category("Threading")]
	public void ShortTimerPeriod() => Assert.IsTrue(TestScript("timer-short-callback-keeps-period", false));
}
