using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using Keysharp.Core;
using static Keysharp.Core.Processes;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using static Keysharp.Core.Core;
using Keysharp.Scripting;
using static Keysharp.Core.Sound;
using System.Globalization;
using static Keysharp.Core.Mouse;

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
