using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public partial class SoundTests : TestRunner
	{
		[Test, Category("Sound")]
		public void SoundBeep()
		{
			_ = Sound.SoundBeep();
			_ = Sound.SoundBeep(700, 500);
			_ = Sound.SoundBeep(800, 500);
			_ = Sound.SoundBeep(900, 500);
			_ = Sound.SoundBeep(1000, 500);
			Assert.IsTrue(TestScript("sound-soundbeep", true));
		}
	}
}
