using Keysharp.Core;
using NUnit.Framework;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("External")]
		public void NumPutNumGet() => Assert.IsTrue(TestScript("external-numput-numget", true));

	}
}