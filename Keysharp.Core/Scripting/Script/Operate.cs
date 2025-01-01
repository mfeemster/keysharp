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
			var varspan = variable.AsSpan();
			var ret = false;

			switch (op)
			{
				case Between:
				{
					if (subject == null)
						throw new UnsetItemError("Left side operand of between was null.");

					if (test == null)
						throw new UnsetItemError("Right side operand of between was null.");

					var z = test.IndexOf(And, StringComparison.OrdinalIgnoreCase);

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
					if (subject == null)
						throw new UnsetItemError("Left side operand of in was null.");

					if (test == null)
						throw new UnsetItemError("Right side operand of in was null.");

					foreach (Range r in test.AsSpan().Split(Delimiter))
					{
						var sub = test.AsSpan(r);

						if (varspan.Equals(sub, StringComparison.OrdinalIgnoreCase))
							ret = true;
					}

					break;

				case Contains:
					if (subject == null)
						throw new UnsetItemError("Left side operand of contains was null.");

					if (test == null)
						throw new UnsetItemError("Right side operand of contains was null.");

					foreach (Range r in test.AsSpan().Split(Delimiter))
					{
						var sub = test.AsSpan(r);

						if (varspan.IndexOf(sub, StringComparison.OrdinalIgnoreCase) != -1)
							ret = true;
					}

					break;

				case Is:
					if (test == null)
						return subject == null;

					test = test.ToLowerInvariant();

					//Put common cases first.
					switch (test)
					{
						case Integer:
							ret = IsInteger(subject);
							goto done;

						case Float:
							ret = IsFloat(subject);
							goto done;

						case Number:
							ret = IsInteger(subject) || IsFloat(subject);
							goto done;

						case "string":
							ret = subject is string;
							goto done;

						case "unset":
						case "null":
							ret = subject == null;
							goto done;
					}

					//Traverse class hierarchy to see if there is a match.
					if (subject != null)
					{
						var type = subject.GetType();

						if (Parser.IsTypeOrBase(type, test))
						{
							ret = true;
							goto done;
						}
					}

					break;
			}

			done:
			return !not ? ret : !ret;
		}

		public static BoolResult IfTest(object result) => new (ForceBool(result), result);

		public static object Operate(Operator op, object left, object right)
		{
			switch (op)
			{
				case Operator.Add:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of addition was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of addition was null.");

					if (left is long l && right is long r)
						return l + r;

					if (left is IntPtr ipl)
						return new IntPtr(ipl + ForceLong(right));

					if (right is IntPtr ipr)
						return new IntPtr(ipr + ForceLong(left));

					return ForceDouble(left) + ForceDouble(right);
				}

				case Operator.BitShiftLeft:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of arithmetic left shift was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of arithmetic left shift was null.");

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
					if (left == null)
						throw new UnsetItemError("Left side operand of arithmetic right shift was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of arithmetic right shift was null.");

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
					if (left == null)
						throw new UnsetItemError("Left side operand of logical right shift was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of logical right shift was null.");

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
					if (left == null)
						throw new UnsetItemError("Left side operand of bitwise and was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of bitwise and was null.");

					if (left is double)
						throw new TypeError($"Left side operand of bitwise and was not an integer, and instead was of type {left.GetType()}.");

					if (right is double)
						throw new TypeError($"Right side operand of bitwise and was not an integer, and instead was of type {left.GetType()}.");

					return ForceLong(left) & ForceLong(right);
				}

				case Operator.BitwiseOr:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of bitwise or was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of bitwise or was null.");

					if (left is double)
						throw new TypeError($"Left side operand of bitwise or was not an integer, and instead was of type {left.GetType()}.");

					if (right is double)
						throw new TypeError($"Right side operand of bitwise or was not an integer, and instead was of type {left.GetType()}.");

					return ForceLong(left) | ForceLong(right);
				}

				case Operator.BitwiseXor:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of bitwise xor was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of bitwise xor was null.");

					if (left is double)
						throw new TypeError($"Left side operand of bitwise xor was not an integer, and instead was of type {left.GetType()}.");

					if (right is double)
						throw new TypeError($"Right side operand of bitwise xor was not an integer, and instead was of type {left.GetType()}.");

					return ForceLong(left) ^ ForceLong(right);
				}

				case Operator.BooleanAnd:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of boolean and was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of boolean and was null.");

					if (left == null)
						throw new TypeError("Left side operand of boolean and was null.");

					if (right == null)
						throw new TypeError("Right side operand of boolean and was null.");

					var b1 = ForceBool(left);

					if (!b1)
						return left;

					return right;
				}

				case Operator.BooleanOr:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of boolean or was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of boolean or was null.");

					if (left == null)
						throw new TypeError("Left side operand of boolean or was null.");

					if (right == null)
						throw new TypeError("Right side operand of boolean or was null.");

					var b1 = ForceBool(left);

					if (b1)
						return left;

					return right;
				}

				case Operator.Concat:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of concat was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of concat was null.");

					return string.Concat(ForceString(left), ForceString(right));
				}

				case Operator.RegEx:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of regular expression was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of regular expression was null.");

					object outvar = null;
					_ = RegEx.RegExMatch(ForceString(left), ForceString(right), ref outvar, 1);
					return outvar;
				}

				case Operator.FloorDivide:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of floor divide was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of floor divide was null.");

					if (left is double)
						throw new TypeError($"Left side operand of integer divide was not an integer, and instead was of type {left.GetType()}.");

					if (right is double)
						throw new TypeError($"Right side operand of integer divide was not an integer, and instead was of type {left.GetType()}.");

					var r = ForceLong(right);
					return r == 0 ? throw new ZeroDivisionError($"Right side operand of integer divide was 0.0") : ForceLong(left) / r;
				}

				case Operator.IdentityInequality:
				{
					if (left == null)
						return right != null;

					if (right == null)
						return left != null;

					_ = MatchTypes(ref left, ref right);

					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, true) != 0;

					return !left.Equals(right);
				}

				case Operator.IdentityEquality://This is for a double equal sign in a conditional, and uses case sensitive comparison for strings.
				{
					if (left == null)
						return right == null;

					if (right == null)
						return left == null;

					_ = MatchTypes(ref left, ref right);

					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, true) == 0;

					return left.Equals(right);
				}

				case Operator.ValueEquality://This is for a single equal sign in a conditional, and uses the case insensitive comparison type for strings.
				{
					if (left == null)
						return right == null;

					if (right == null)
						return left == null;

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
						return left.Equals(right);//Will go here if both are double or decimal.
				}

				case Operator.LessThan:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of less than was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of less than was null.");

					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, true) < 0;
					else
						return left == null ? right == null : ForceDouble(left) < ForceDouble(right);
				}

				case Operator.LessThanOrEqual:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of less than or equal was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of less than or equal was null.");

					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, true) <= 0;
					else
						return left == null ? right == null : ForceDouble(left) <= ForceDouble(right);
				}

				case Operator.GreaterThan:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of greater than was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of greater than was null.");

					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, true) > 0;
					else
						return left == null ? right == null : ForceDouble(left) > ForceDouble(right);
				}

				case Operator.GreaterThanOrEqual:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of greater than or equal was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of greater than or equal was null.");

					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, true) >= 0;
					else
						return left == null ? right == null : ForceDouble(left) >= ForceDouble(right);
				}

				case Operator.ValueInequality://This is for != or <> in a conditional.
				{
					if (left == null)
						return right != null;

					if (right == null)
						return left != null;

					_ = MatchTypes(ref left, ref right);

					if (left is string s1 && right is string s2)
						return Strings.StrCmp(s1, s2, false) != 0;
					else
						return left == null ? right != null : !left.Equals(right);//Will go here if both are double or decimal.
				}

				case Operator.Modulus:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of modulo was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of modulo was null.");

					if (left is long l && right is long r)
						return l % r;

					return ForceDouble(left) % ForceDouble(right);
				}

				case Operator.Power:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of modulus was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of modulus was null.");

					return Math.Pow(ForceDouble(left), ForceDouble(right));
				}

				case Operator.Minus:
				case Operator.Subtract:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of subtraction or minus was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of subtraction or minus was null.");

					if (left is long l && right is long r)
						return l - r;

					return ForceDouble(left) - ForceDouble(right);
				}

				case Operator.Multiply:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of multiply was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of multiply was null.");

					if (left is long l && right is long r)
						return l * r;

					return ForceDouble(left) * ForceDouble(right);
				}

				case Operator.Divide:
				{
					if (left == null)
						throw new UnsetItemError("Left side operand of divide was null.");

					if (right == null)
						throw new UnsetItemError("Right side operand of divide was null.");

					var r = ForceDouble(right);

					if (r == 0.0)
						throw new ZeroDivisionError($"Right side operand of floating point divide was 0.0");

					return ForceDouble(left) / r;
				}

				case Operator.Is:
				{
					return IfLegacy(left, "is", ForceString(right));
				}

				default:
					throw new ValueError($"Operator {op} cannot be applied to: {left} and {right}");
			}
		}

		public static object OperateTernary(bool result, ExpressionDelegate x, ExpressionDelegate y) => result ? x() : y();

		public static object MultiStatement(params object[] args) => args[ ^ 1];

		public static object OperateUnary(Operator op, object right)
		{
			switch (op)
			{
				case Operator.Minus:
				case Operator.Subtract:
				{
					if (right == null)
						throw new UnsetItemError("Right side operand of subtraction or minus was null.");

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
					if (right == null)
						throw new UnsetItemError("Right side operand of bitwise not was null.");

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
					throw new ValueError($"Operator {op} cannot be applied to: {right}");
			}
		}

		public static int OperateZero(object expression) => 0;

		public static object OrMaybe(object left, object right) => Types.IsSet(left) == 1L ? left : right;

		internal static bool IsFloat(object obj) =>
		obj is double ||
		obj is float ||
		obj is decimal;

		internal static bool IsInteger(object obj) =>
		obj is long ||
		obj is int ||
		obj is ulong ||
		obj is uint ||
		obj is short ||
		obj is ushort ||
		obj is char ||
		obj is sbyte ||
		obj is byte;

		internal static bool IsNumeric(Type type) =>
		type == typeof(long)
		|| type == typeof(double)
		|| type == typeof(int)
		|| type == typeof(uint)
		|| type == typeof(ulong)
		|| type == typeof(float)
		|| type == typeof(decimal)
		|| type == typeof(byte)
		|| type == typeof(sbyte)
		;

		internal static bool IsNumeric(object value) => value != null&& IsNumeric(value.GetType());

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
			NullCoalesce,
		};

		public delegate object ExpressionDelegate();
	}
}