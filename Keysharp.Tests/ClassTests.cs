using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public class ClassTests : TestRunner
	{
		[Test, Category("Class")]
		public void ClassBasic() => Assert.IsTrue(TestScript("class", false));

		[Test, Category("Class")]
		public void ClassWithStaticVar() => Assert.IsTrue(TestScript("class-static", false));

		[Test, Category("Class")]
		public void ClassWithMemberFuncs() => Assert.IsTrue(TestScript("class-member-funcs", false));

		[Test, Category("Class")]
		public void ClassExtends() => Assert.IsTrue(TestScript("class-extends", false));

		[Test, Category("Class")]
		public void ClassParams() => Assert.IsTrue(TestScript("class-params", false));

		[Test, Category("Class")]
		public void ClassProperties() => Assert.IsTrue(TestScript("class-props", false));

		[Test, Category("Class")]
		public void ClassOwnProperties() => Assert.IsTrue(TestScript("class-ownprops", false));

		[Test, Category("Class")]
		public void ClassSpecialFunctions() => Assert.IsTrue(TestScript("class-special-funcs", false));
	}
}