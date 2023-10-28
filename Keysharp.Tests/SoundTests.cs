using NUnit.Framework;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Sound")]
		public void SoundBeep()
		{
			Keysharp.Core.Sound.SoundBeep();
			Keysharp.Core.Sound.SoundBeep(700, 500);
			Keysharp.Core.Sound.SoundBeep(800, 500);
			Keysharp.Core.Sound.SoundBeep(900, 500);
			Keysharp.Core.Sound.SoundBeep(1000, 500);
			Assert.IsTrue(TestScript("sound-soundbeep", true));
		}
	}
}
