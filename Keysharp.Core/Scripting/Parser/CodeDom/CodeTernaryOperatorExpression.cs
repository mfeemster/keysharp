using System.CodeDom;

namespace Keysharp.Scripting
{
	internal class CodeTernaryOperatorExpression : CodeExpression
	{
		internal CodeExpression Condition { get; set; }

		internal CodeExpression FalseBranch { get; set; }

		internal CodeExpression TrueBranch { get; set; }

		internal CodeTernaryOperatorExpression()
		{
		}

		internal CodeTernaryOperatorExpression(CodeExpression condition, CodeExpression trueBranch, CodeExpression falseBranch)
		{
			Condition = condition;
			TrueBranch = trueBranch;
			FalseBranch = falseBranch;
		}
	}
}