using NUnit.Framework;
using static Keysharp.Core.Network;

namespace Keysharp.Tests
{
	/// <summary>
	/// Function tests don't need to also be wrapped in a funciton, so pass false.
	/// </summary>
	public partial class Scripting
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
			Assert.IsTrue(TestScript("network-download", false));
		}
	}
}