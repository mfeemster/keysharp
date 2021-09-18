using NUnit.Framework;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Operator")]
		public void DereferenceEx() => Assert.IsTrue(TestScript("op-dereference-ex", true));

		[Test, Category("Operator")]
		public void UnaryMinusEx() => Assert.IsTrue(TestScript("op-unary-minus-ex", true));

		[Test, Category("Operator")]
		public void MultiStatementEx() => Assert.IsTrue(TestScript("op-multi-statement-ex", true));

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
		public void BitwiseAndOrXorEx() => Assert.IsTrue(TestScript("op-bitwise-and-or-xor-ex", true));

		[Test, Category("Operator")]
		public void BitwiseNotEx() => Assert.IsTrue(TestScript("op-bitwise-not-ex", true));

		[Test, Category("Operator")]
		public void CombinedAssignEx() => Assert.IsTrue(TestScript("op-combined-assign-ex", true));

		[Test, Category("Operator")]
		public void DivideEx() => Assert.IsTrue(TestScript("op-divide-ex", true));

		[Test, Category("Operator")]
		public void GreaterLessEqualEx() => Assert.IsTrue(TestScript("op-greater-less-equal-ex", true));

		[Test, Category("Operator")]
		public void GreaterLessEx() => Assert.IsTrue(TestScript("op-greater-less-ex", true));

		[Test, Category("Operator")]
		public void IncDec() => Assert.IsTrue(TestScript("op-inc-dec-ex", true));

		[Test, Category("Operator")]
		public void LeftShiftEx() => Assert.IsTrue(TestScript("op-lsh-ex", true));

		[Test, Category("Operator")]
		public void LogicalAndEx() => Assert.IsTrue(TestScript("op-logical-and-ex", true));

		[Test, Category("Operator")]
		public void LogicalNotEx() => Assert.IsTrue(TestScript("op-logical-not-ex", true));

		[Test, Category("Operator")]
		public void LogicalOrEx() => Assert.IsTrue(TestScript("op-logical-or-ex", true));

		[Test, Category("Operator")]
		public void MultiplyEx() => Assert.IsTrue(TestScript("op-multiply-ex", true));

		[Test, Category("Operator")]
		public void PowerEx() => Assert.IsTrue(TestScript("op-power-ex", true));

		[Test, Category("Operator")]
		public void RightShiftEx() => Assert.IsTrue(TestScript("op-rsh-ex", false));

		[Test, Category("Operator")]
		public void TernaryEx() => Assert.IsTrue(TestScript("op-ternary-ex", true));
	}
}