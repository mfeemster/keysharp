using static Keysharp.Scripting.Keywords;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		internal static CodeFieldReferenceExpression OperatorAsFieldReference(Script.Operator op)
		{
			//Despite the extreme namespace and type qualification verbosity it causes,
			//we must pass the type so the generated code is not ambiguous with methods whose names are the same as these operators.
			//For example, Add and Add().
			var field = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(Script.Operator)), op.ToString());
			field.UserData.Add(RawData, op);
			return field;
		}

		/// <summary>
		/// CodeDOM does not have built in support for ternary operators and creating our own derived class for it won't work.
		/// So we must manually create the code string for the ternary,
		/// then use a code snippet to hold the string. This is not ideal, but there is no other way.
		/// This is needed to obtain the expected behavior where the false condition does not execute
		/// if the evaluation is true.
		/// </summary>
		/// <param name="eval">The expression to evaluate.</param>
		/// <param name="trueBranch">The expression to execute if eval is true.</param>
		/// <param name="falseBranch">The expression to execute if eval is false.</param>
		/// <returns>A snippet expression of the ternary operation using the passed in expressions.</returns>
		internal CodeSnippetExpression MakeTernarySnippet(CodeExpression eval, CodeExpression trueBranch, CodeExpression falseBranch)
		{
			var evalstr = Ch.CodeToString(eval);
			var tbs = Ch.CodeToString(trueBranch);
			var fbs = Ch.CodeToString(falseBranch);
			return new CodeSnippetExpression($"(_ = {evalstr} ? (object)({tbs}) : (object)({fbs}))");//Must explicitly cast both branches to object in case they are different types such as: x ? 1.0 : 1L (double and long).
		}

		internal static (bool, Script.Operator) OperatorFromString(string code)
		{
			var op = code;

			switch (op[0])
			{
				case Add:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.Add);

						case 2:
							return (true, Script.Operator.Increment);

						default:
							return (false, Script.Operator.Add);
					}

				case Minus:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.Subtract);

						case 2:
							return (true, Script.Operator.Decrement);

						default:
							return (false, Script.Operator.Add);
					}

				case Multiply:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.Multiply);

						case 2:
							return (true, Script.Operator.Power);

						default:
							return (false, Script.Operator.Add);
					}

				case Divide:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.Divide);

						case 2:
							return (true, Script.Operator.FloorDivide);

						default:
							return (false, Script.Operator.Add);
					}

				case Greater:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.GreaterThan);

						case 2:
							if (op[0] == op[1])
								return (true, Script.Operator.BitShiftRight);
							else if (op[1] == Equal)
								return (true, Script.Operator.GreaterThanOrEqual);
							else
								return (false, Script.Operator.Add);

						case 3:
							return op[0] == op[1] && op[1] == op[2] ? (true, Script.Operator.LogicalBitShiftRight) : (false, Script.Operator.Add);

						default:
							return (false, Script.Operator.Add);
					}

				case Less:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.LessThan);

						case 2:
							if (op[1] == op[0])
								return (true, Script.Operator.BitShiftLeft);
							else if (op[1] == Equal)
								return (true, Script.Operator.LessThanOrEqual);
							else if (op[1] == Greater)
								return (true, Script.Operator.ValueInequality);
							else
								return (false, Script.Operator.Add);

						default:
							return (false, Script.Operator.Add);
					}

				case BitAND:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.BitwiseAnd);

						case 2:
							if (op[0] == op[1])
								return (true, Script.Operator.BooleanAnd);
							else
								return (false, Script.Operator.Add);

						default:
							return (false, Script.Operator.Add);
					}

				case BitOR:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.BitwiseOr);

						case 2:
							return op[0] == op[1] ? (true, Script.Operator.BooleanOr) : (false, Script.Operator.Add);

						default:
							return (false, Script.Operator.Add);
					}

				case BitXOR:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.BitwiseXor);

						default:
							return (false, Script.Operator.Add);
					}

				case BitNOT:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.BitwiseNot);

						case 2:
							return op[1] == Equal ? (true, Script.Operator.RegEx) : (false, Script.Operator.Add);

						default:
							return (false, Script.Operator.Add);
					}

				case Equal:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.ValueEquality);

						case 2:
							return op[1] == op[0] ? (true, Script.Operator.IdentityEquality) : (false, Script.Operator.Add);

						default:
							return (false, Script.Operator.Add);
					}

				case Not:
					switch (op.Length)
					{
						case 1:
							return (true, Script.Operator.LogicalNot);

						case 2:
							return op[1] == Equal ? (true, Script.Operator.ValueInequality) : (false, Script.Operator.Add);

						case 3:
							return op[1] == Equal && op[2] == Equal ? (true, Script.Operator.IdentityInequality) : (false, Script.Operator.Add);

						default:
							return (false, Script.Operator.Add);
					}

				case AssignPre:
					return op.Length > 1 && op[1] == Equal ? (true, Script.Operator.Assign) : (true, Script.Operator.TernaryB);

				case Concatenate:
					return (true, Script.Operator.Concat);

				case TernaryA:
					return op.Length > 1 && op[1] == TernaryA ? (true, Script.Operator.NullCoalesce) : (true, Script.Operator.TernaryA);

				default:
					switch (code.ToLowerInvariant())
					{
						case NotTxt:
							return (true, Script.Operator.LogicalNotEx);

						case AndTxt:
							return (true, Script.Operator.BooleanAnd);

						case OrTxt:
							return (true, Script.Operator.BooleanOr);

						case IsTxt:
							return (true, Script.Operator.Is);
					}

					return (false, Script.Operator.Add);
			}
		}

		internal static int OperatorPrecedence(Script.Operator op, CodeLine codeLine)
		{
			switch (op)
			{
				case Script.Operator.Power:
					return -1;

				case Script.Operator.Minus:
				case Script.Operator.LogicalNot:
				case Script.Operator.BitwiseNot:
				case Script.Operator.Address:
				case Script.Operator.Dereference:
					return -2;

				case Script.Operator.Multiply:
				case Script.Operator.Divide:
				case Script.Operator.FloorDivide:
					return -3;

				case Script.Operator.Add:
				case Script.Operator.Subtract:
					return -4;

				case Script.Operator.BitShiftLeft:
				case Script.Operator.BitShiftRight:
				case Script.Operator.LogicalBitShiftRight:
					return -5;

				case Script.Operator.BitwiseAnd:
				case Script.Operator.BitwiseXor:
				case Script.Operator.BitwiseOr:
					return -6;

				case Script.Operator.Concat:
				case Script.Operator.RegEx:
					return -7;

				case Script.Operator.GreaterThan:
				case Script.Operator.LessThan:
				case Script.Operator.GreaterThanOrEqual:
				case Script.Operator.LessThanOrEqual:
					return -8;

				case Script.Operator.Is:
					return -9;

				case Script.Operator.ValueEquality:
				case Script.Operator.IdentityEquality:
				case Script.Operator.ValueInequality:
				case Script.Operator.IdentityInequality:
					return -10;

				case Script.Operator.LogicalNotEx:
					return -11;

				case Script.Operator.BooleanAnd:
					return -12;

				case Script.Operator.BooleanOr:
					return -13;

				case Script.Operator.TernaryA:
				case Script.Operator.TernaryB:
				case Script.Operator.NullCoalesce:
					return -14;

				case Script.Operator.Assign:
					return -15;

				default:
					throw new ParseException($"Operator {op} was not a value which can be used when determining precedence.", codeLine);
			}
		}
	}
}