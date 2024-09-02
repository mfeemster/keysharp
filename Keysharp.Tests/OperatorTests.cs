using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public partial class OperatorTests : TestRunner
	{
		//[Test, Category("Operator")]
		//public void Dereference() => Assert.IsTrue(TestScript("op-dereference", true));//Probably will never implement this.

		[Test, Category("Operator")]
		public void UnaryMinus() => Assert.IsTrue(TestScript("op-unary-minus", true));

		[Test, Category("Operator")]
		public void MultiStatement() => Assert.IsTrue(TestScript("op-multi-statement", true));

		[Test, Category("Operator")]
		public void BetweenNumeric() => Assert.IsTrue(TestScript("op-between-numeric", true));

		[Test, Category("Operator")]
		public void BetweenNumericNot() => Assert.IsTrue(TestScript("op-between-numeric-not", true));

		[Test, Category("Operator")]
		public void BetweenNumericVar() => Assert.IsTrue(TestScript("op-between-numeric-var", true));

		[Test, Category("Operator")]
		public void BetweenNumericVarNot() => Assert.IsTrue(TestScript("op-between-numeric-var-not", true));

		[Test, Category("Operator")]
		public void BetweenString() => Assert.IsTrue(TestScript("op-between-string", true));

		[Test, Category("Operator")]
		public void BetweenStringNot() => Assert.IsTrue(TestScript("op-between-string-not", true));

		[Test, Category("Operator")]
		public void BetweenStringVar() => Assert.IsTrue(TestScript("op-between-string-var", true));

		[Test, Category("Operator")]
		public void BetweenStringVarNot() => Assert.IsTrue(TestScript("op-between-string-var-not", true));

		[Test, Category("Operator")]
		public void BitwiseAndOrXor() => Assert.IsTrue(TestScript("op-bitwise-and-or-xor", true));

		[Test, Category("Operator")]
		public void BitwiseNot() => Assert.IsTrue(TestScript("op-bitwise-not", true));

		[Test, Category("Operator")]
		public void CombinedAssign() => Assert.IsTrue(TestScript("op-combined-assign", true));

		[Test, Category("Operator")]
		public void Divide() => Assert.IsTrue(TestScript("op-divide", true));

		[Test, Category("Operator")]
		public void GreaterLessEqual() => Assert.IsTrue(TestScript("op-greater-less-equal", true));

		[Test, Category("Operator")]
		public void GreaterLess() => Assert.IsTrue(TestScript("op-greater-less", true));

		[Test, Category("Operator")]
		public void IncDec() => Assert.IsTrue(TestScript("op-inc-dec", true));

		[Test, Category("Operator")]
		public void LeftShift() => Assert.IsTrue(TestScript("op-lsh", true));

		[Test, Category("Operator")]
		public void LogicalAnd() => Assert.IsTrue(TestScript("op-logical-and", true));

		[Test, Category("Operator")]
		public void LogicalNot() => Assert.IsTrue(TestScript("op-logical-not", true));

		[Test, Category("Operator")]
		public void LogicalOr() => Assert.IsTrue(TestScript("op-logical-or", true));

		[Test, Category("Operator")]
		public void Multiply() => Assert.IsTrue(TestScript("op-multiply", true));

		[Test, Category("Operator")]
		public void Power() => Assert.IsTrue(TestScript("op-power", true));

		[Test, Category("Operator")]
		public void RightShift() => Assert.IsTrue(TestScript("op-rsh", false));

		[Test, Category("Operator")]
		public void Ternary() => Assert.IsTrue(TestScript("op-ternary", true));

		[Test, Category("Operator")]
		public void NullAssign() => Assert.IsTrue(TestScript("op-null-assign", false));
	}
}