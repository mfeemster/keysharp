namespace Keysharp.Core.Common.Threading
{
	internal class TimerWithTag : System.Timers.Timer
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object Tag { get; set; }
		public TimerWithTag()
			: base() { }

		public TimerWithTag(double interval)
			: base(interval) { }
	}
}