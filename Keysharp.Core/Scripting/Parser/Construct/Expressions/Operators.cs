using System;
using System.CodeDom;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private bool IsAssignOp(string code)
		{
			if (!(code.Length >= 2 && code.Length <= 4))
				return false;

			if (code[0] == Equal || code[code.Length - 1] != Equal)
				return false;

			if (code.Length == 3)
			{
				if (code[0] == code[1])
				{
					switch (code[0])
					{
						case Greater:
						case Less:
						case Divide:
							return true;

						default:
							return false;
					}
				}
				else
					return false;
			}
			else if (code.Length == 4)
			{
				if (code[0] == code[1] && code[1] == code[2])
				{
					switch (code[0])
					{
						case Greater:
							return true;

						default:
							return false;
					}
				}
				else
					return false;
			}
			else
			{
				switch (code[0])
				{
					case Greater:
					case Less:
					case Not:
					case BitNOT:
						return false;

					default:
						return true;
				}
			}
		}

		private bool IsOperator(string code)
		{
			try
			{
				if (!IsAssignOp(code))
					_ = OperatorFromString(code);
			}
			catch (ArgumentOutOfRangeException) { return false; }

			return true;
		}

		private bool IsUnaryOperator(Script.Operator op)
		{
			switch (op)
			{
				case Script.Operator.Subtract://Minus doesn't seem to be needed here.
				case Script.Operator.LogicalNot:
				case Script.Operator.LogicalNotEx:
				case Script.Operator.BitwiseNot:
				case Script.Operator.BitwiseAnd:
				case Script.Operator.Dereference:
					return true;

				//TODO
				//This messes up the postfix operator when used in an assignment like y := x++
				//case Script.Operator.Add:
				//return true;

				default:
					return false;
			}
		}

		private CodeFieldReferenceExpression OperatorAsFieldReference(Script.Operator op)
		{
			var field = new CodeFieldReferenceExpression(null, op.ToString());//Not passing a type cuts down on the extreme namespace and type qualification verbosity.
			//field.TargetObject = new CodeTypeReferenceExpression(typeof(Script.Operator), op.ToString());
			field.UserData.Add(RawData, op);
			return field;
		}

		private Script.Operator OperatorFromString(string code)
		{
			var op = code;

			switch (op[0])
			{
				case Add:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.Add;

						case 2:
							return Script.Operator.Increment;

						default:
							throw new ParseException(ExUnexpected);
					}

				case Minus:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.Subtract;

						case 2:
							return Script.Operator.Decrement;

						default:
							throw new ParseException(ExUnexpected);
					}

				case Multiply:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.Multiply;

						case 2:
							return Script.Operator.Power;

						default:
							throw new ParseException(ExUnexpected);
					}

				case Divide:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.Divide;

						case 2:
							return Script.Operator.FloorDivide;

						default:
							throw new ParseException(ExUnexpected);
					}

				case Greater:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.GreaterThan;

						case 2:
							if (op[0] == op[1])
								return Script.Operator.BitShiftRight;
							else if (op[1] == Equal)
								return Script.Operator.GreaterThanOrEqual;
							else
								throw new ParseException(ExUnexpected);

						case 3:
							return op[0] == op[1] && op[1] == op[2] ? Script.Operator.LogicalBitShiftRight : throw new ParseException(ExUnexpected);

						default:
							throw new ParseException(ExUnexpected);
					}

				case Less:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.LessThan;

						case 2:
							if (op[1] == op[0])
								return Script.Operator.BitShiftLeft;
							else if (op[1] == Equal)
								return Script.Operator.LessThanOrEqual;
							else if (op[1] == Greater)
								return Script.Operator.ValueInequality;
							else
								throw new ParseException(ExUnexpected);

						default:
							throw new ParseException(ExUnexpected);
					}

				case BitAND:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.BitwiseAnd;

						case 2:
							if (op[0] == op[1])
								return Script.Operator.BooleanAnd;
							else
								throw new ParseException(ExUnexpected);

						default:
							throw new ParseException(ExUnexpected);
					}

				case BitOR:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.BitwiseOr;

						case 2:
							return op[0] == op[1] ? Script.Operator.BooleanOr : throw new ParseException(ExUnexpected);

						default:
							throw new ParseException(ExUnexpected);
					}

				case BitXOR:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.BitwiseXor;

						default:
							throw new ParseException(ExUnexpected);
					}

				case BitNOT:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.BitwiseNot;

						case 2:
							return op[1] == Equal ? Script.Operator.RegEx : throw new ParseException(ExUnexpected);

						default:
							throw new ParseException(ExUnexpected);
					}

				case Equal:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.ValueEquality;

						case 2:
							return op[1] == op[0] ? Script.Operator.IdentityEquality : throw new ParseException(ExUnexpected);

						default:
							throw new ParseException(ExUnexpected);
					}

				case Not:
					switch (op.Length)
					{
						case 1:
							return Script.Operator.LogicalNot;

						case 2:
							return op[1] == Equal ? Script.Operator.ValueInequality : throw new ParseException(ExUnexpected);

						case 3:
							return op[1] == Equal && op[2] == Equal ? Script.Operator.IdentityInequality : throw new ParseException(ExUnexpected);

						default:
							throw new ParseException(ExUnexpected);
					}

				case AssignPre:
					return op.Length > 1 && op[1] == Equal ? Script.Operator.Assign : Script.Operator.TernaryB;

				case Concatenate:
					return Script.Operator.Concat;

				case TernaryA:
					return op.Length > 1 && op[1] == TernaryA ? Script.Operator.NullAssign : Script.Operator.TernaryA;

				default:
					switch (code.ToLowerInvariant())
					{
						case NotTxt:
							return Script.Operator.LogicalNotEx;

						case AndTxt:
							return Script.Operator.BooleanAnd;

						case OrTxt:
							return Script.Operator.BooleanOr;

						case IsTxt:
							return Script.Operator.Is;
					}

					throw new ArgumentOutOfRangeException();
			}
		}

		private int OperatorPrecedence(Script.Operator op)
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
				case Script.Operator.NullAssign:
					return -14;

				case Script.Operator.Assign:
					return -15;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}