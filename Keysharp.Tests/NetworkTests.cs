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
			var attr = Files.FileExist(filename);

			if (attr.StartsWith('A') || attr.StartsWith('N'))
				_ = Files.FileDelete(filename);

			_ = Download("http://textfiles.com/art/asciiart.txt", filename);
			Assert.IsTrue(File.Exists(filename));
			Assert.AreEqual(16048L, Files.FileGetSize(filename));
			Thread.Sleep(1000);
			attr = Files.FileExist(filename);

			if (attr.StartsWith('A') || attr.StartsWith('N'))
				_ = Files.FileDelete(filename);

			_ = Download("*0 http://textfiles.com/art/asciiart.txt", filename);
			Assert.IsTrue(File.Exists(filename));
			Assert.AreEqual(16048L, Files.FileGetSize(filename));
			Assert.IsTrue(TestScript("network-download", true));
		}
	}
}