using static Keysharp.Core.Network;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public partial class NetworkTests : TestRunner
	{
		[Test, Category("Network")]
		public void NetDownload()
		{
			var filename = @"./asciiart.txt";

			if (System.IO.File.Exists(filename))
				Keysharp.Core.Files.FileDelete(filename);

			Download("http://textfiles.com/art/asciiart.txt", filename);
			Assert.IsTrue(System.IO.File.Exists(filename));
			Assert.AreEqual(16048L, Keysharp.Core.Files.FileGetSize(filename));
			System.Threading.Thread.Sleep(1000);

			if (System.IO.File.Exists(filename))
				Keysharp.Core.Files.FileDelete(filename);

			Download("*0 http://textfiles.com/art/asciiart.txt", filename);
			Assert.IsTrue(System.IO.File.Exists(filename));
			Assert.AreEqual(16048L, Keysharp.Core.Files.FileGetSize(filename));
			Assert.IsTrue(TestScript("network-download", true));
		}
	}
}