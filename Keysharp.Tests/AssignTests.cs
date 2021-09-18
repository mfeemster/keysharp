using NUnit.Framework;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Assign")]
		public void AssignDynVarEx() => Assert.IsTrue(TestScript("assign-dyn-var-ex", false));//Dynamic variables only work as global statics, not function variables.

		[Test, Category("Assign")]
		public void AssignEmpty() => Assert.IsTrue(TestScript("assign-empty", true));

		[Test, Category("Assign")]
		public void AssignEmptyEx() => Assert.IsTrue(TestScript("assign-empty-ex", true));

		[Test, Category("Assign")]
		public void AssignFloat() => Assert.IsTrue(TestScript("assign-float", true));

		[Test, Category("Assign")]
		public void AssignFloatEx() => Assert.IsTrue(TestScript("assign-float-ex", true));

		[Test, Category("Assign")]
		public void AssignInt() => Assert.IsTrue(TestScript("assign-int", true));

		[Test, Category("Assign")]
		public void AssignNull() => Assert.IsTrue(TestScript("assign-null", true));

		[Test, Category("Assign")]
		public void AssignNullEx() => Assert.IsTrue(TestScript("assign-null-ex", true));

		[Test, Category("Assign")]
		public void AssignQuoted() => Assert.IsTrue(TestScript("assign-quoted", true));

		[Test, Category("Assign")]
		public void AssignQuotedEx() => Assert.IsTrue(TestScript("assign-quoted-ex", true));

		[Test, Category("Assign")]
		public void AssignStringCaseCompareEx() => Assert.IsTrue(TestScript("assign-string-case-compare-ex", true));

		[Test, Category("Assign")]
		public void AssignStringConcatEx() => Assert.IsTrue(TestScript("assign-string-concat-ex", true));

		[Test, Category("Assign")]
		public void AssignUnquoted() => Assert.IsTrue(TestScript("assign-unquoted", true));

		[Test, Category("Assign")]
		public void AssignVar() => Assert.IsTrue(TestScript("assign-var", true));

		[Test, Category("Assign")]
		public void AssignVarEx() => Assert.IsTrue(TestScript("assign-var-ex", true));

		[Test, Category("Assign")]
		public void AssignCompareDynVarEx() => Assert.IsTrue(TestScript("assign-compare-dyn-var-ex", false));//Dynamic variables only work as global statics, not function variables.

		[Test, Category("Assign")]
		public void AssignIntEx() => Assert.IsTrue(TestScript("assign-int-ex", true));

		[Test, Category("Assign")]
		public void AssignHex() => Assert.IsTrue(TestScript("assign-hex", true));

		[Test, Category("Assign")]
		public void AssignNumerics() => Assert.IsTrue(TestScript("assign-numerics", true));
	}
}