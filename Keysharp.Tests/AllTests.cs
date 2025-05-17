using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	[SetUpFixture]
	public class MySetUpClass
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
		}
	}
}