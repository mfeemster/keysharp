using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public partial class SoundTests : TestRunner
	{
		[Test, Category("Sound")]
		public void SoundBeep()
		{
			Sound.SoundBeep();
			Sound.SoundBeep(700, 500);
			Sound.SoundBeep(800, 500);
			Sound.SoundBeep(900, 500);
			Sound.SoundBeep(1000, 500);
			Assert.IsTrue(TestScript("sound-soundbeep", true));
		}
	}
}
