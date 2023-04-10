using System;
using System.Runtime.InteropServices;
using Keysharp.Core;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		public static bool IfLegacy(object subject, string op, string test, bool not = false)
		{
			const string Between = "between";
			const string In = "in";
			const string Contains = "contains";
			const string Is = "is";
			const char Delimiter = ',';
			const string And = " and ";
			const string Integer = "integer";
			const string Float = "float";
			const string Number = "number";
			const string Digit = "digit";
			const string Xdigit = "xdigit";
			const string Alpha = "alpha";
			const string Upper = "upper";
			const string Lower = "lower";
			const string Alnum = "alnum";
			const string Space = "space";
			const string Time = "time";
			var variable = ForceString(subject);
			var ret = false;

			switch (op)
			{
				case Between:
				{
					var z = test.IndexOf(And, System.StringComparison.OrdinalIgnoreCase);

					if (z == -1)
						z = variable.Length;

					if (double.TryParse(test.AsSpan(0, z), out var low) && double.TryParse(test.AsSpan(z + And.Length), out var high))
					{
						var d = ForceDouble(subject);
						ret = d >= low && d <= high;
					}
					else if (subject is string s)
					{
						ret = string.Compare(test.Substring(0, z), s) < 0 && string.Compare(s, test.Substring(z + And.Length)) < 0;
					}
				}
				break;

				case In:
					foreach (var sub in test.Split(Delimiter))
						if (variable.Equals(sub, System.StringComparison.OrdinalIgnoreCase))
							ret = true;

					break;

				case Contains:
					foreach (var sub in test.Split(Delimiter))
						if (variable.IndexOf(sub, System.StringComparison.OrdinalIgnoreCase) != -1)
							ret = true;

					break;

				case Is:
					test = test.ToLowerInvariant();

					if (subject != null)
					{
						var type = subject.GetType();

						if (Keysharp.Scripting.Parser.IsTypeOrBase(type, test))
						{
							ret = true;
							goto done;
						}
					}

					switch (test)
					{
						case Integer:
						case Number:
							variable = variable.Trim().TrimStart(new[] { '+', '-' });

						goto case Xdigit;

						case Xdigit:
							if (variable.Length > 3 && variable[0] == '0' && (variable[1] == 'x' || variable[1] == 'X'))
								variable = variable.Substring(2);

							break;

						case "true":
							return ForceBool(subject);

						case "false":
							return !ForceBool(subject);
					}

					switch (test)
					{
						case Float:
							if (!variable.Contains("."))
							{
								ret = false;
								goto done;
							}

						goto case Number;

						case Number:
						{
							var dot = false;

							foreach (var sym in variable)
							{
								if (sym == '.')
								{
									if (dot)
									{
										ret = false;
										goto done;
									}

									dot = true;
								}
								else if (!char.IsDigit(sym))
								{
									ret = false;
									goto done;
								}
							}

							ret = true;
							goto done;
						}

						case Digit:
							foreach (var sym in variable)
								if (!char.IsDigit(sym))
								{
									ret = false;
									goto done;
								}

							ret = true;
							goto done;

						case Integer:
						case Xdigit:
						{
							foreach (var sym in variable)
								if (!(char.IsDigit(sym) || (sym > 'a' - 1 && sym < 'f' + 1) || (sym > 'A' - 1 && sym < 'F' + 1)))
								{
									ret = false;
									goto done;
								}

							ret = true;
							goto done;
						}

						case Alpha:
							foreach (var sym in variable)
								if (!char.IsLetter(sym))
								{
									ret = false;
									goto done;
								}

							ret = true;
							goto done;

						case Upper:
							foreach (var sym in variable)
								if (!char.IsUpper(sym))
								{
									ret = false;
									goto done;
								}

							ret = true;
							goto done;

						case Lower:
							foreach (var sym in variable)
								if (!char.IsLower(sym))
								{
									ret = false;
									goto done;
								}

							ret = true;
							goto done;

						case Alnum:
							foreach (var sym in variable)
								if (!char.IsLetterOrDigit(sym))
								{
									ret = false;
									goto done;
								}

							ret = true;
							goto done;

						case Space:
							foreach (var sym in variable)
								if (!char.IsWhiteSpace(sym))
								{
									ret = false;
									goto done;
								}

							ret = true;
							goto done;

						case Time:
							if (!IsNumeric(variable))
							{
								ret = false;
								goto done;
							}

							ret = ForceLong(variable) < 99991231125959;
							goto done;

						default:
							ret = false;
							goto done;
					}
			}

			done:
			return !not ? ret : !ret;
		}

		public static BoolResult IfTest(object result) => new BoolResult(ForceBool(result), result);

		//if (result is bool b)
		//  return b;
		//else if (IsNumeric(result))
		//  return ForceDouble(result) != 0;
		//else
		//  return result is string s ? !string.IsNullOrEmpty(s) : result != null;

		public static bool IsNumeric(Type type) =>
		type == typeof(int)
		|| type == typeof(uint)
		|| type == typeof(long)
		|| type == typeof(ulong)
		|| type == typeof(float)
		|| type == typeof(double)
		|| type == typeof(decimal)
		|| type == typeof(byte)
		|| type == typeof(sbyte)
		;

		public static bool IsNumeric(object value) => value != null&& IsNumeric(value.GetType());

		public static object Operate(Operator op, object left, object right)
		{
			switch (op)
			{
				case Operator.Add:
				{
					if (left is long l && right is long r)
						return l + r;

					return ForceDouble(left) + ForceDouble(right);
				}

				case Operator.BitShiftLeft:
				{
					if (left is double)
						throw new TypeError($"Left side operand of arithmetic left shift was not an integer, and instead was of type {left.GetType()}.");

					if (right is double)
						throw new TypeError($"Right side operand of arithmetic left shift was not an integer, and instead was of type {left.GetType()}.");

					var r = (int)ForceLong(right);

					if (r < 0 || r > 63)
						throw new Error($"Shift operand of {r} for arithmetic left shift was not in the range of [0-63].");

					return ForceLong(left) << r;
				}

				case Operator.BitShiftRight:
				{
					if (left is double)
						throw new TypeError($"Left side operand of arithmetic right shift was not an integer, and instead was of type {left.GetType()}.");

					if (right is double)
						throw new TypeError($"Right side operand of arithmetic right shift was not an integer, and instead was of type {left.GetType()}.");

					var r = (int)ForceLong(right);

					if (r < 0 || r > 63)
						throw new Error($"Shift operand of {r} for arithmetic right shift was not in the range of [0-63].");

					return ForceLong(left) >> r;
				}

				case Operator.LogicalBitShiftRight:
				{
					if (left is double)
						throw new TypeError($"Left side operand of logical right shift was not an integer, and instead was of type {left.GetType()}.");

					if (right is double)
						throw new TypeError($"Right side operand of logical right shift was not an integer, and instead was of type {left.GetType()}.");

					var r = (int)ForceLong(right);

					if (r < 0 || r > 63)
						throw new Error($"Shift operand of {r} for logical right shift was not in the range of [0-63].");

					return (long)((ulong)ForceLong(left) >> r);
				}

				case Operator.BitwiseAnd:
				{
					if (left is double)
						throw new TypeError($"Left side operand of bitwise and was not an integer, and instead was of type {left.GetType()}.");

					if (right is double)
						throw new TypeError($"Right side operand of bitwise and was not an integer, and instead was of type {left.GetType()}.");

					return ForceLong(left) & ForceLong(right);
				}

				case Operator.BitwiseOr:
				{
					if (left is double)
						throw new TypeError($"Left side operand of bitwise or was not an integer, and instead was of type {left.GetType()}.");

					if (right is double)
						throw new TypeError($"Right side operand of bitwise or was not an integer, and instead was of type {left.GetType()}.");

					return ForceLong(left) | ForceLong(right);
				}

				case Operator.BitwiseXor:
				{
					if (left is double)
						throw new TypeError($"Left side operand of bitwise xor was not an integer, and instead was of type {left.GetType()}.");

					if (right is double)
						throw new TypeError($"Right side operand of bitwise xor was not an integer, and instead was of type {left.GetType()}.");

					return ForceLong(left) ^ ForceLong(right);
				}

				case Operator.BooleanAnd:
				{
					var b1 = ForceBool(left);

					if (!b1)
						return left;

					return right;
					//return ForceBool(left) && ForceBool(right);
				}

				case Operator.BooleanOr:
				{
					var b1 = ForceBool(left);

					if (b1)
						return left;

					return right;
					//return ForceBool(left) || ForceBool(right);
				}

				case Operator.Concat:
					return string.Concat(ForceString(left), ForceString(right));

				case Operator.RegEx:
					return Strings.RegExMatch(ForceString(left), ForceString(right), 1);

				case Operator.FloorDivide:
				{
					if (left is double)
						throw new TypeError($"Left side operand of integer divide was not an integer, and instead was of type {left.GetType()}.");

					if (right is double)
						throw new TypeError($"Right side operand of integer divide was not an integer, and instead was of type {left.GetType()}.");

					var r = ForceLong(right);
					return r == 0 ? throw new ZeroDivisionError($"Right side operand of integer divide was 0.0") : ForceLong(left) / r;
				}

				case Operator.IdentityInequality:
				{
					_ = MatchTypes(ref left, ref right);//Still need this.
					return left == null ? right != null : !left.Equals(right);
				}

				case Operator.IdentityEquality://This is for a double equal sign in a conditional, and uses case sensitive comparison for strings.
				{
					_ = MatchTypes(ref left, ref right);//Still need this.
					return left == null ? right == null : left.Equals(right);
				}

				case Operator.ValueEquality://This is for a single equal sign in a conditional, and uses the global StringCaseSense comparison type for strings.
				{
					_ = MatchTypes(ref left, ref right);

					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, false) == 0;
					else if (left is Core.Array al1 && right is Core.Array al2)
					{
						var len1 = (long)al1.Length;
						var len2 = (long)al2.Length;

						if (len1 != len2)
							return false;

						for (var i = 1; i <= len1; i++)
						{
							if (IsNumeric(al1[i]) && IsNumeric(al2[i]))
							{
								var d1 = Convert.ToDouble(al1[i]);
								var d2 = Convert.ToDouble(al2[i]);

								if (d1 != d2)
									return false;
							}
							else if (!al1[i].Equals(al2[i]))
								return false;
						}

						return true;
					}
					else if (left is Core.Buffer buf1 && right is Core.Buffer buf2)
					{
						var len1 = (long)buf1.Size;
						var len2 = (long)buf2.Size;

						if (len1 != len2)
							return false;

						for (var i = 1; i <= len1; i++)
						{
							if (buf1[i] != buf2[i])
								return false;
						}

						return true;
					}
					else
						return left == null ? right == null : left.Equals(right);//Will go here if both are double or decimal.
				}

				case Operator.LessThan:
				{
					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, false) < 0;
					else
						return left == null ? right == null : ForceDouble(left) < ForceDouble(right);
				}

				case Operator.LessThanOrEqual:
				{
					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, false) <= 0;
					else
						return left == null ? right == null : ForceDouble(left) <= ForceDouble(right);
				}

				case Operator.GreaterThan:
				{
					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, false) > 0;
					else
						return left == null ? right == null : ForceDouble(left) > ForceDouble(right);
				}

				case Operator.GreaterThanOrEqual:
				{
					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, false) >= 0;
					else
						return left == null ? right == null : ForceDouble(left) >= ForceDouble(right);
				}

				case Operator.ValueInequality://This is for != or <> in a conditional.
				{
					_ = MatchTypes(ref left, ref right);

					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, false) != 0;//This uses the global StringCaseSense comparison type for strings.
					else
						return left == null ? right != null : !left.Equals(right);//Will go here if both are double or decimal.
				}

				case Operator.Modulus:
				{
					if (left is long l && right is long r)
						return l % r;

					return ForceDouble(left) % ForceDouble(right);
				}

				case Operator.Power:
					return Math.Pow(ForceDouble(left), ForceDouble(right));

				case Operator.Minus:
				case Operator.Subtract:
				{
					if (left is long l && right is long r)
						return l - r;

					return ForceDouble(left) - ForceDouble(right);
				}

				case Operator.Multiply:
				{
					if (left is long l && right is long r)
						return l * r;

					return ForceDouble(left) * ForceDouble(right);
				}

				case Operator.Divide:
				{
					var r = ForceDouble(right);

					if (r == 0.0)
						throw new ZeroDivisionError($"Right side operand of floating point divide was 0.0");

					return ForceDouble(left) / r;
				}

				case Operator.Is:
					return IfLegacy(left, "is", ForceString(right));

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static object OperateTernary(bool result, ExpressionDelegate x, ExpressionDelegate y) => result ? x() : y();

		public static object OperateUnary(Operator op, object right)
		{
			switch (op)
			{
				case Operator.Minus:
				case Operator.Subtract:
				{
					if (right is long l)
					{
						return -l;
					}
					else
					{
						var value = ForceDouble(right);
						return value == 0d ? right : -value;
					}
				}

				case Operator.LogicalNot:
				case Operator.LogicalNotEx:
					return !IfTest(right);

				case Operator.BitwiseNot:
				{
					if (right is double)
						throw new TypeError($"Unary operand of logical not was not an integer, and instead was of type {right.GetType()}.");

					if (IsNumeric(right))
					{
						var l = ForceLong(right);
						return ~l;
					}

					throw new TypeError($"Unary operand of logical not was not an integer, and instead was of type {right.GetType()}.");
				}

				//Not supporting references at this time.
				//case Operator.Dereference:
				// TODO: dereference operator
				//return null;
				//case Operator.BitwiseAnd:
				//return GCHandle.Alloc(right, GCHandleType.Pinned).AddrOfPinnedObject().ToInt64();//This seems almost certainly wrong, and would need to be freed elsewhere.

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static int OperateZero(object expression) => 0;

		public enum Operator
		{
			Add,
			Subtract,
			Multiply,
			Divide,
			Modulus,
			Assign,
			IdentityInequality,
			IdentityEquality,
			ValueEquality,
			BitwiseOr,
			BitwiseAnd,
			BooleanOr,
			BooleanAnd,
			RegEx,
			LessThan,
			LessThanOrEqual,
			GreaterThan,
			GreaterThanOrEqual,

			Increment,
			Decrement,

			Minus,
			LogicalNot,
			BitwiseNot,
			Address,
			Dereference,

			Power,
			FloorDivide,
			BitShiftRight,
			BitShiftLeft,
			LogicalBitShiftRight,
			BitwiseXor,
			ValueInequality,
			Concat,

			LogicalNotEx,

			TernaryA,
			TernaryB,

			Is,
			NullAssign,
		};

		public delegate object ExpressionDelegate();
	}
}