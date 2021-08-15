using Keysharp.Core;
using NUnit.Framework;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Collections")]
		public void ArrayEx() => Assert.IsTrue(TestScript("collections-array-ex", true));

		[Test, Category("Collections")]
		public void AssociativeArrayEx() => Assert.IsTrue(TestScript("collections-associative-array-ex", true));

		[Test, Category("Collections")]
		public void Buffer()
		{
			var buf = new Buffer(5, 10);
			Assert.AreEqual(5L, (long)buf.Size);

			for (var i = 1; i <= (long)buf.Size; i++)
			{
				var p = buf[i];
				Assert.AreEqual(10L, p);
			}

			buf.Size = 10;
			Assert.AreEqual(10L, (long)buf.Size);

			for (var i = 1; i <= 5; i++)//Ensure original values were copied. Subsequent values are undefined.
			{
				var p = buf[i];
				Assert.AreEqual(10L, p);
			}

			Assert.IsTrue(TestScript("collections-buffer", true));
		}
	}
}