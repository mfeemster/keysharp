#define USEFORMSTIMER
namespace Keysharp.Core.Common.Threading
{
#if USEFORMSTIMER
	internal class TimerWithTag : System.Windows.Forms.Timer
#else
	internal class TimerWithTag : System.Timers.Timer
#endif
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object Tag { get; set; }
		public TimerWithTag()
			: base() { }

		public TimerWithTag(double interval)
#if USEFORMSTIMER
		{
			Interval = (int)interval;
		}
#else
			: base(interval) { }
#endif
	}
}