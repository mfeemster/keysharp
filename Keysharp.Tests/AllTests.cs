using NUnit.Framework;

namespace Keysharp.Tests
{
	[SetUpFixture]
	public class MySetUpClass
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			//var dir = Path.GetDirectoryName(typeof(MySetUpClass).Assembly.Location);
			//Environment.CurrentDirectory = dir;
			// or
			//Directory.SetCurrentDirectory(dir);
		}
	}

	public partial class Scripting
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void SimplePass() => Assert.IsTrue(TestScript("pass", true));

		[Test]
		public void AssignInt() => Assert.IsTrue(TestScript("assign-int", true));

		[Test]
		public void ExAssignInt() => Assert.IsTrue(TestScript("assign-int-ex", true));

		[Test]
		public void AssignFloat() => Assert.IsTrue(TestScript("assign-float", true));

		[Test]
		public void AssignFloatEx() => Assert.IsTrue(TestScript("assign-float-ex", true));

		[Test]
		public void AssignUnquoted() => Assert.IsTrue(TestScript("assign-unquoted", true));

		[Test]
		public void AssignQuoted() => Assert.IsTrue(TestScript("assign-quoted", true));

		[Test]
		public void AssignQuotedEx() => Assert.IsTrue(TestScript("assign-quoted-ex", true));

		[Test]
		public void AssignNull() => Assert.IsTrue(TestScript("assign-null", true));

		[Test]
		public void AssignNullEx() => Assert.IsTrue(TestScript("assign-null-ex", true));

		[Test]
		public void AssignEmpty() => Assert.IsTrue(TestScript("assign-empty", true));

		[Test]
		public void AssignEmptyEx() => Assert.IsTrue(TestScript("assign-empty-ex", true));

		[Test]
		public void AssignVar() => Assert.IsTrue(TestScript("assign-var", true));

		[Test]
		public void AssignVarEx() => Assert.IsTrue(TestScript("assign-var-ex", true));

		[Test]
		public void AssignDynVarEx() => Assert.IsTrue(TestScript("assign-dyn-var-ex", true));

		[Test]
		public void AssignStringConcat() => Assert.IsTrue(TestScript("assign-string-concat", true));

		[Test]
		public void AssignStringConcatEx() => Assert.IsTrue(TestScript("assign-string-concat-ex", true));

		[Test]
		public void AssignStringCaseCompareEx() => Assert.IsTrue(TestScript("assign-string-case-compare-ex", true));

		[Test]
		public void BetweenNumeric() => Assert.IsTrue(TestScript("between-numeric", true));

		[Test]
		public void BetweenNumericNot() => Assert.IsTrue(TestScript("between-numeric-not", true));

		[Test]
		public void BetweenNumericVar() => Assert.IsTrue(TestScript("between-numeric-var", true));

		[Test]
		public void BetweenNumericVarNot() => Assert.IsTrue(TestScript("between-numeric-var-not", true));

		[Test]
		public void BetweenString() => Assert.IsTrue(TestScript("between-string", true));

		[Test]
		public void BetweenStringNot() => Assert.IsTrue(TestScript("between-string-not", true));

		[Test]
		public void BetweenStringVar() => Assert.IsTrue(TestScript("between-string-var", true));

		[Test]
		public void BetweenStringVarNot() => Assert.IsTrue(TestScript("between-string-var-not", true));

		[Test]
		public void Numerics() => Assert.IsTrue(TestScript("numerics", true));

		[Test]
		public void CompareDynVarEx() => Assert.IsTrue(TestScript("compare-dyn-var-ex", true));

		[Test]
		public void IncDec() => Assert.IsTrue(TestScript("inc-dec-ex", true));

		[Test]
		public void GreaterLessEx() => Assert.IsTrue(TestScript("greater-less-ex", true));

		[Test]
		public void GreaterLessEqualEx() => Assert.IsTrue(TestScript("greater-less-equal-ex", true));

		[Test]
		public void PowerEx() => Assert.IsTrue(TestScript("power-ex", true));

		[Test]
		public void LogicalNotEx() => Assert.IsTrue(TestScript("logical-not-ex", true));

		[Test]
		public void LogicalAndEx() => Assert.IsTrue(TestScript("logical-and-ex", true));

		[Test]
		public void LogicalOrEx() => Assert.IsTrue(TestScript("logical-or-ex", true));

		[Test]
		public void BitwiseNotEx() => Assert.IsTrue(TestScript("bitwise-not-ex", true));

		[Test]
		public void BitwiseAndOrXorEx() => Assert.IsTrue(TestScript("bitwise-and-or-xor-ex", true));

		[Test]
		public void LeftShiftEx() => Assert.IsTrue(TestScript("lsh-ex", true));

		[Test]
		public void RightShiftEx() => Assert.IsTrue(TestScript("rsh-ex", true));

		[Test]
		public void DereferenceEx() => Assert.IsTrue(TestScript("dereference-ex", true));

		[Test]
		public void UnaryMinusEx() => Assert.IsTrue(TestScript("unary-minus-ex", true));

		[Test]
		public void ConcatEx() => Assert.IsTrue(TestScript("concat-ex", true));

		[Test]
		public void CompareStringCaseEx() => Assert.IsTrue(TestScript("compare-string-case-ex", true));

		[Test]
		public void MultiplyEx() => Assert.IsTrue(TestScript("multiply-ex", true));

		[Test]
		public void DivideEx() => Assert.IsTrue(TestScript("divide-ex", true));

		[Test]
		public void TernaryEx() => Assert.IsTrue(TestScript("ternary-ex", true));

		[Test]
		public void CombinedOpAssignEx() => Assert.IsTrue(TestScript("combined-op-assign-ex", true));

		[Test]
		public void MultiStatementEx() => Assert.IsTrue(TestScript("multi-statement-ex", true));

		[Test]
		public void ArrayEx() => Assert.IsTrue(TestScript("array-ex", true));

		[Test]
		public void AssociativeArrayEx() => Assert.IsTrue(TestScript("associative-array-ex", true));

		[Test]
		public void Hex() => Assert.IsTrue(TestScript("hex", true));

		[Test, Category("BuiltInVars")]
		public void PropsSpecialChars() => Assert.IsTrue(TestScript("props-special-chars", true));

		[Test, Category("BuiltInVars")]
		public void PropsScriptProperties() => Assert.IsTrue(TestScript("props-script-properties", false));

		[Test, Category("BuiltInVars")]
		public void PropsDateTime() => Assert.IsTrue(TestScript("props-date-time", true));

		[Test, Category("BuiltInVars")]
		public void PropsScriptSettings() => Assert.IsTrue(TestScript("props-script-settings", true));

		[Test, Category("BuiltInVars")]
		public void Suspend() => Assert.IsTrue(TestScript("suspend", true));
		/*
		            [Test]
		            public void Functions()
		            {
		            Assert.IsTrue(TestScript("functions"));
		            }

		            [Test]
		            public void Comments()
		            {
		            Assert.IsTrue(TestScript("comments"));
		            }

		            [Test]
		            public void Directive()
		            {
		            Assert.IsTrue(TestScript("directive"));
		            }

		            [Test]
		            public void Expressions()
		            {
		            Assert.IsTrue(TestScript("expressions"));
		            }

		            [Test]
		            public void Command()
		            {
		            Assert.IsTrue(TestScript("command"));
		            }

		            [Test]
		            public void Line()
		            {
		            Assert.IsTrue(TestScript("line"));
		            }

		            [Test]
		            public void Loop()
		            {
		            Assert.IsTrue(TestScript("loop"));
		            }

		            [Test]
		            public void Objects()
		            {
		            Assert.IsTrue(TestScript("objects"));
		            }

		            [Test]
		            public void Assign()
		            {
		            Assert.IsTrue(TestScript("assign"));
		            }

		            [Test]
		            public void Goto()
		            {
		            Assert.IsTrue(TestScript("goto"));
		            }

		            [Test]
		            public void Hotkey()
		            {
		            Assert.IsTrue(ValidateScript("hotkey"));
		            }

		            [Test]
		            public void VanillaExpressions()
		            {
		            Assert.IsTrue(ValidateScript("vanilla-Expressions"));
		            }

		            [Test]
		            public void VanillaContinuation()
		            {
		            Assert.IsTrue(ValidateScript("vanilla-Line Continuation"));
		            }

		            [Test]
		            public void VanillaMain()
		            {
		            Assert.IsTrue(ValidateScript("vanilla-MAIN"));
		            }

		            [Test]
		            public void VanillaRegex()
		            {
		            Assert.IsTrue(ValidateScript("vanilla-RegExMatch & RegExReplace"));
		            }*/
	}
}
