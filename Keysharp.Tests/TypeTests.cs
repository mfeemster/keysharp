using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public partial class TypeTests : TestRunner
	{
		/// <summary>
		/// Ensure the type hierarchy matches the documentation exactly.
		/// </summary>
		[Test, Category("Types")]
		public void TestTypes()
		{
			Assert.IsTrue(typeof(Keysharp.Core.KeysharpException).IsAssignableTo(typeof(System.Exception)));
			Assert.IsTrue(typeof(Keysharp.Core.Error).IsAssignableTo(typeof(Keysharp.Core.KeysharpException)));
			Assert.IsTrue(typeof(Keysharp.Core.ParseException).IsAssignableTo(typeof(Keysharp.Core.Error)));
			Assert.IsTrue(typeof(Keysharp.Core.IndexError).IsAssignableTo(typeof(Keysharp.Core.Error)));
			Assert.IsTrue(typeof(Keysharp.Core.KeyError).IsAssignableTo(typeof(Keysharp.Core.Error)));
			Assert.IsTrue(typeof(Keysharp.Core.MemberError).IsAssignableTo(typeof(Keysharp.Core.UnsetError)));
			Assert.IsTrue(typeof(Keysharp.Core.UnsetItemError).IsAssignableTo(typeof(Keysharp.Core.UnsetError)));
			Assert.IsTrue(typeof(Keysharp.Core.MemoryError).IsAssignableTo(typeof(Keysharp.Core.Error)));
			Assert.IsTrue(typeof(Keysharp.Core.MethodError).IsAssignableTo(typeof(Keysharp.Core.MemberError)));
			Assert.IsTrue(typeof(Keysharp.Core.PropertyError).IsAssignableTo(typeof(Keysharp.Core.MemberError)));
			Assert.IsTrue(typeof(Keysharp.Core.OSError).IsAssignableTo(typeof(Keysharp.Core.Error)));
			Assert.IsTrue(typeof(Keysharp.Core.TargetError).IsAssignableTo(typeof(Keysharp.Core.Error)));
			Assert.IsTrue(typeof(Keysharp.Core.TimeoutError).IsAssignableTo(typeof(Keysharp.Core.Error)));
			Assert.IsTrue(typeof(Keysharp.Core.TypeError).IsAssignableTo(typeof(Keysharp.Core.Error)));
			Assert.IsTrue(typeof(Keysharp.Core.ValueError).IsAssignableTo(typeof(Keysharp.Core.Error)));
			Assert.IsTrue(typeof(Keysharp.Core.ZeroDivisionError).IsAssignableTo(typeof(Keysharp.Core.Error)));
#if LINUX
			Assert.IsTrue(typeof(Keysharp.Core.ClipboardAll).IsAssignableTo(typeof(Keysharp.Core.Common.ObjectBase.KeysharpObject)));
#elif WINDOWS
			Assert.IsTrue(typeof(Keysharp.Core.ClipboardAll).IsAssignableTo(typeof(Keysharp.Core.Buffer)));
#endif
			Assert.IsTrue(typeof(Keysharp.Core.Buffer).IsAssignableTo(typeof(Keysharp.Core.Common.ObjectBase.KeysharpObject)));
			Assert.IsTrue(typeof(Keysharp.Core.Array).IsAssignableTo(typeof(Keysharp.Core.Common.ObjectBase.KeysharpObject)));
			Assert.IsTrue(typeof(Keysharp.Core.Map).IsAssignableTo(typeof(Keysharp.Core.Common.ObjectBase.KeysharpObject)));
			Assert.IsTrue(typeof(Keysharp.Core.Common.File.KeysharpFile).IsAssignableTo(typeof(Keysharp.Core.Common.ObjectBase.KeysharpObject)));
		}
	}
}