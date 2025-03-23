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

		internal static (bool, Script.Operator) OperatorFromString(string code)
		{
			var op = code;

			switch (op[0])
			{
				case Add:
					return op.Length switch
				{
						1 => (true, Script.Operator.Add),
							2 => (true, Script.Operator.Increment),
							_ => (false, Script.Operator.Add),
					};

				case Minus:
					return op.Length switch
				{
						1 => (true, Script.Operator.Subtract),
							2 => (true, Script.Operator.Decrement),
							_ => (false, Script.Operator.Add),
					};

				case Multiply:
					return op.Length switch
				{
						1 => (true, Script.Operator.Multiply),
							2 => (true, Script.Operator.Power),
							_ => (false, Script.Operator.Add),
					};

				case Divide:
					return op.Length switch
				{
						1 => (true, Script.Operator.Divide),
							2 => (true, Script.Operator.FloorDivide),
							_ => (false, Script.Operator.Add),
					};

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
					return op.Length switch
				{
						1 => (true, Script.Operator.BitwiseOr),
							2 => op[0] == op[1] ? (true, Script.Operator.BooleanOr) : (false, Script.Operator.Add),
							_ => (false, Script.Operator.Add),
					};

				case BitXOR:
					return op.Length switch
				{
						1 => (true, Script.Operator.BitwiseXor),
							_ => (false, Script.Operator.Add),
					};

				case BitNOT:
					return op.Length switch
				{
						1 => (true, Script.Operator.BitwiseNot),
							2 => op[1] == Equal ? (true, Script.Operator.RegEx) : (false, Script.Operator.Add),
							_ => (false, Script.Operator.Add),
					};

				case Equal:
					return op.Length switch
				{
						1 => (true, Script.Operator.ValueEquality),
							2 => op[1] == op[0] ? (true, Script.Operator.IdentityEquality) : (false, Script.Operator.Add),
							_ => (false, Script.Operator.Add),
					};

				case Not:
					return op.Length switch
				{
						1 => (true, Script.Operator.LogicalNot),
							2 => op[1] == Equal ? (true, Script.Operator.ValueInequality) : (false, Script.Operator.Add),
							3 => op[1] == Equal && op[2] == Equal ? (true, Script.Operator.IdentityInequality) : (false, Script.Operator.Add),
							_ => (false, Script.Operator.Add),
					};

				case AssignPre:
					return op.Length > 1 && op[1] == Equal ? (true, Script.Operator.Assign) : (true, Script.Operator.TernaryB);

				case Concatenate:
					return (true, Script.Operator.Concat);

				case TernaryA:
					return op.Length > 1 && op[1] == TernaryA ? (true, Script.Operator.NullCoalesce) : (true, Script.Operator.TernaryA);

				default:
					return code.ToLowerInvariant() switch
				{
						NotTxt => (true, Script.Operator.LogicalNotEx),
							AndTxt => (true, Script.Operator.BooleanAnd),
							OrTxt => (true, Script.Operator.BooleanOr),
							IsTxt => (true, Script.Operator.Is),
							_ => (false, Script.Operator.Add),
					};
			}
		}

		internal static int OperatorPrecedence(Script.Operator op, CodeLine codeLine)
		{

			return op switch
		{
				Script.Operator.Power => -1,
				Script.Operator.Minus or Script.Operator.LogicalNot or Script.Operator.BitwiseNot or Script.Operator.Address or Script.Operator.Dereference => -2,
				Script.Operator.Multiply or Script.Operator.Divide or Script.Operator.FloorDivide => -3,
				Script.Operator.Add or Script.Operator.Subtract => -4,
				Script.Operator.BitShiftLeft or Script.Operator.BitShiftRight or Script.Operator.LogicalBitShiftRight => -5,
				Script.Operator.BitwiseAnd or Script.Operator.BitwiseXor or Script.Operator.BitwiseOr => -6,
				Script.Operator.Concat or Script.Operator.RegEx => -7,
				Script.Operator.GreaterThan or Script.Operator.LessThan or Script.Operator.GreaterThanOrEqual or Script.Operator.LessThanOrEqual => -8,
				Script.Operator.Is => -9,
				Script.Operator.ValueEquality or Script.Operator.IdentityEquality or Script.Operator.ValueInequality or Script.Operator.IdentityInequality => -10,
				Script.Operator.LogicalNotEx => -11,
				Script.Operator.BooleanAnd => -12,
				Script.Operator.BooleanOr => -13,
				Script.Operator.TernaryA or Script.Operator.TernaryB or Script.Operator.NullCoalesce => -14,
				Script.Operator.Assign => -15,
				_ => throw new ParseException($"Operator {op} was not a value which can be used when determining precedence.", codeLine),
			};
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
			var cse = new CodeSnippetExpression($"({evalstr} ? (object)({tbs}) : (object)({fbs}))");//Must explicitly cast both branches to object in case they are different types such as: x ? 1.0 : 1L (double and long).
			cse.UserData["orig"] = new CodeTernaryOperatorExpression(eval, trueBranch, falseBranch);
			return cse;
		}

	}
}