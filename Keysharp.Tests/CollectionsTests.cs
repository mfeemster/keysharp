using System.Collections.Generic;
using Keysharp.Core;
using NUnit.Framework;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Collections")]
		public void Array()
		{
			var arr = new Keysharp.Core.Array(new object[]
			{
				10L,
				20L,
				30L
			});
			var index = 0;

			foreach (var (i, v) in (IEnumerable<(object, object)>)arr)
			{
				if (index == 0)
				{
					Assert.AreEqual(i, 1L);
					Assert.AreEqual(v, 10L);
				}
				else if (index == 1)
				{
					Assert.AreEqual(i, 2L);
					Assert.AreEqual(v, 20L);
				}
				else if (index == 2)
				{
					Assert.AreEqual(i, 3L);
					Assert.AreEqual(v, 30L);
				}

				index++;
			}

			index = 0;

			foreach (var (i, _) in (IEnumerable<(object, object)>)arr)
			{
				if (index == 0)
					Assert.AreEqual(i, 1L);
				else if (index == 1)
					Assert.AreEqual(i, 2L);
				else if (index == 2)
					Assert.AreEqual(i, 3L);

				index++;
			}

			index = 0;

			foreach (var (_, v) in (IEnumerable<(object, object)>)arr)
			{
				if (index == 0)
					Assert.AreEqual(v, 10L);
				else if (index == 1)
					Assert.AreEqual(v, 20L);
				else if (index == 2)
					Assert.AreEqual(v, 30L);

				index++;
			}

			Assert.AreEqual(arr.ToString(), "[10, 20, 30]");
			Assert.IsTrue(TestScript("collections-array", true));
		}

		[Test, Category("Collections")]
		public void AssociativeArray()
		{
			var arr = Keysharp.Core.Misc.Dictionary(new object[]
			{
				"one",
				"two",
				"three"
			}, new object[]
			{
				1L,
				2L,
				3L
			});

			foreach (var (k, v) in (IEnumerable<(object, object)>)arr)
			{
				if ((string)k == "one")
				{
					Assert.AreEqual(v, 1L);
				}
				else if ((string)k == "two")
				{
					Assert.AreEqual(v, 2L);
				}
				else if ((string)k == "three")
				{
					Assert.AreEqual(v, 3L);
				}
				else
					Assert.IsTrue(false);
			}

			foreach (var (_, v) in (IEnumerable<(object, object)>)arr)
			{
				if ((long)v == 1L)
				{
				}
				else if ((long)v == 2L)
				{
				}
				else if ((long)v == 3L)
				{
				}
				else
					Assert.IsTrue(false);
			}

			foreach (var (k, _) in (IEnumerable<(object, object)>)arr)
			{
				if ((string)k == "one")
				{
				}
				else if ((string)k == "two")
				{
				}
				else if ((string)k == "three")
				{
				}
				else
					Assert.IsTrue(false);
			}

			Assert.AreEqual(arr.ToString(), "{\"one\": 1, \"two\": 2, \"three\": 3}");
			Assert.IsTrue(TestScript("collections-associative-array", true));
		}

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