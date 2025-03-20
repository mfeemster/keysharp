namespace Keysharp.Scripting
{
	public partial class Script
	{
		const string Addition = "addition";
		const string Divide = "division";
		const string Multiply = "multiplication";
		const string Subtraction = "subtraction";
		const string ArLeftShift = "arithmetic left shift";
		const string ArRightShift = "arithmetic right shift";
		const string LogicalRightShift = "logical right shift";
		const string BitwiseAnd = "bitwise and";
		const string BitwiseOr = "bitwise or";
		const string BitwiseXor = "bitwise xor";
		const string LessThan = "less than";
		const string LessThanOrEqual = "less than or equal";
		const string GreaterThan = "greater than";
		const string GreaterThanOrEqual = "greater than or equal";
		const string Modulo = "modulo";
		const string Power = "power";
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
			Error err;
			var variable = ForceString(subject);
			var varspan = variable.AsSpan();
			var ret = false;

			switch (op)
			{
				case Between:
				{
					if (subject == null)
						_ = Errors.ErrorOccurred(err = new UnsetError("Left side operand of between was null.")) ? throw err : "";

					if (test == null)
						_ = Errors.ErrorOccurred(err = new UnsetError("Right side operand of between was null.")) ? throw err : "";

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
						_ = Errors.ErrorOccurred(err = new UnsetError("Left side operand of in was null.")) ? throw err : "";

					if (test == null)
						_ = Errors.ErrorOccurred(err = new UnsetError("Right side operand of in was null.")) ? throw err : "";

					foreach (Range r in test.AsSpan().Split(Delimiter))
					{
						var sub = test.AsSpan(r);

						if (varspan.Equals(sub, StringComparison.OrdinalIgnoreCase))
							ret = true;
					}

					break;

				case Contains:
					if (subject == null)
						_ = Errors.ErrorOccurred(err = new UnsetError("Left side operand of contains was null.")) ? throw err : "";

					if (test == null)
						_ = Errors.ErrorOccurred(err = new UnsetError("Right side operand of contains was null.")) ? throw err : "";

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

		public static object PrefixIncDec(Operator op, object left, object val) => Operate(op, left, val);

		public static object PostfixIncDecProp(object obj, object prop, object val)
		{
			var orig = GetPropertyValue(obj, prop);
			var newval = Operate(Operator.Add, orig, val);
			SetPropertyValue(obj, prop, newval);
			return orig;
		}

		public static object Operate(Operator op, object left, object right)
		{
			Error err;

			switch (op)
			{
				case Operator.Add:
				{
					if (ParseNumericArgs(left, right, Addition, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
						{
							if (secondIsDouble)
								return firstd + secondd;
							else
								return firstd + secondl;
						}
						else
						{
							if (secondIsDouble)
								return firstl + secondd;
							else
							{
								if (left is IntPtr lip)
									return IntPtr.Add(lip, (int)secondl);
								else if (right is IntPtr rip)
									return IntPtr.Add(rip, (int)firstl);
								else
									return firstl + secondl;
							}
						}
					}

					return null;
				}

				case Operator.BitShiftLeft:
				{
					if (ParseNumericArgs(left, right, ArLeftShift, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Left side operand of arithmetic left shift was not an integer, and instead was of type {left.GetType()}.")) ? throw err : null;

						if (secondIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Right side operand of arithmetic left shift was not an integer, and instead was of type {right.GetType()}.")) ? throw err : null;

						var r = (int)secondl;

						if (r < 0 || r > 63)
							return Errors.ErrorOccurred(err = new Error($"Shift operand of {r} for arithmetic left shift was not in the range of [0-63].")) ? throw err : null;

						return firstl << r;
					}

					return null;
				}

				case Operator.BitShiftRight:
				{
					if (ParseNumericArgs(left, right, ArRightShift, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Left side operand of arithmetic right shift was not an integer, and instead was of type {left.GetType()}.")) ? throw err : null;

						if (secondIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Right side operand of arithmetic right shift was not an integer, and instead was of type {right.GetType()}.")) ? throw err : null;

						var r = (int)secondl;

						if (r < 0 || r > 63)
							return Errors.ErrorOccurred(err = new Error($"Shift operand of {r} for arithmetic right shift was not in the range of [0-63].")) ? throw err : null;

						return firstl >> r;
					}

					return null;
				}

				case Operator.LogicalBitShiftRight:
				{
					if (ParseNumericArgs(left, right, LogicalRightShift, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Left side operand of logical right shift was not an integer, and instead was of type {left.GetType()}.")) ? throw err : null;

						if (secondIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Right side operand of logical right shift was not an integer, and instead was of type {right.GetType()}.")) ? throw err : null;

						var r = (int)secondl;

						if (r < 0 || r > 63)
							return Errors.ErrorOccurred(err = new Error($"Shift operand of {r} for logical right shift was not in the range of [0-63].")) ? throw err : null;

						return (long)((ulong)firstl >> r);
					}

					return null;
				}

				case Operator.BitwiseAnd:
				{
					if (ParseNumericArgs(left, right, BitwiseAnd, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Left side operand of bitwise and was not an integer, and instead was of type {left.GetType()}.")) ? throw err : null;

						if (secondIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Right side operand of bitwise and was not an integer, and instead was of type {right.GetType()}.")) ? throw err : null;

						return firstl & secondl;
					}

					return null;
				}

				case Operator.BitwiseOr:
				{
					if (ParseNumericArgs(left, right, BitwiseOr, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Left side operand of bitwise or was not an integer, and instead was of type {left.GetType()}.")) ? throw err : null;

						if (secondIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Right side operand of bitwise or was not an integer, and instead was of type {right.GetType()}.")) ? throw err : null;

						return firstl | secondl;
					}

					return null;
				}

				case Operator.BitwiseXor:
				{
					if (ParseNumericArgs(left, right, BitwiseXor, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Left side operand of bitwise xor was not an integer, and instead was of type {left.GetType()}.")) ? throw err : null;

						if (secondIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Right side operand of bitwise xor was not an integer, and instead was of type {right.GetType()}.")) ? throw err : null;

						return firstl ^ secondl;
					}

					return null;
				}

				case Operator.BooleanAnd:
				{
					if (left == null)
						return Errors.ErrorOccurred(err = new UnsetError("Left side operand of boolean and was null.")) ? throw err : null;

					if (right == null)
						return Errors.ErrorOccurred(err = new UnsetError("Right side operand of boolean and was null.")) ? throw err : null;

					var b1 = ForceBool(left);

						if (!b1)
							return left;

						return right;
					}

				case Operator.BooleanOr:
				{
					if (left == null)
						return Errors.ErrorOccurred(err = new UnsetError("Left side operand of boolean or was null.")) ? throw err : null;

					if (right == null)
						return Errors.ErrorOccurred(err = new UnsetError("Right side operand of boolean or was null.")) ? throw err : null;

						var b1 = ForceBool(left);

						if (b1)
							return left;

						return right;
					}

				case Operator.Concat:
				{
					if (left == null)
						return Errors.ErrorOccurred(err = new UnsetError("Left side operand of concat was null.")) ? throw err : null;

					if (right == null)
						return Errors.ErrorOccurred(err = new UnsetError("Right side operand of concat was null.")) ? throw err : null;

						return string.Concat(ForceString(left), ForceString(right));
					}

				case Operator.RegEx:
				{
					if (left == null)
						return Errors.ErrorOccurred(err = new UnsetError("Left side operand of regular expression was null.")) ? throw err : null;

					if (right == null)
						return Errors.ErrorOccurred(err = new UnsetError("Right side operand of regular expression was null.")) ? throw err : null;

                        Misc.VarRef outvar = new Misc.VarRef(null);
						_ = RegEx.RegExMatch(ForceString(left), ForceString(right), outvar, 1);
						return outvar.__Value;
					}

				case Operator.FloorDivide:
				{
					if (ParseNumericArgs(left, right, BitwiseOr, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Left side operand of floor divide was not an integer, and instead was of type {left.GetType()}.")) ? throw err : null;

						if (secondIsDouble)
							return Errors.ErrorOccurred(err = new TypeError($"Right side operand of floor divide was not an integer, and instead was of type {right.GetType()}.")) ? throw err : null;

						if (secondl == 0L)
							return Errors.ErrorOccurred(err = new ZeroDivisionError($"Right side operand of floor divide was 0")) ? throw err : null;

						return firstl / secondl;
					}

					return null;
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
					if (left is string s1 && right is string s2)
					{
						return Strings.StrCmp(s1, s2, true) < 0;
					}
					else if (ParseNumericArgs(left, right, LessThan, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
						{
							if (secondIsDouble)
								return firstd < secondd;
							else
								return firstd < secondl;
						}
						else
						{
							if (secondIsDouble)
								return firstl < secondd;
							else
								return firstl < secondl;
						}
					}

					return null;
				}

				case Operator.LessThanOrEqual:
				{
					if (left is string s1 && right is string s2)
					{
						return Strings.StrCmp(s1, s2, true) <= 0;
					}
					else if (ParseNumericArgs(left, right, LessThanOrEqual, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
						{
							if (secondIsDouble)
								return firstd <= secondd;
							else
								return firstd <= secondl;
						}
						else
						{
							if (secondIsDouble)
								return firstl <= secondd;
							else
								return firstl <= secondl;
						}
					}

					return null;
				}

				case Operator.GreaterThan:
				{
					if (left is string s1 && right is string s2)
					{
						return Strings.StrCmp(s1, s2, true) > 0;
					}
					else if (ParseNumericArgs(left, right, GreaterThan, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
						{
							if (secondIsDouble)
								return firstd > secondd;
							else
								return firstd > secondl;
						}
						else
						{
							if (secondIsDouble)
								return firstl > secondd;
							else
								return firstl > secondl;
						}
					}

					return null;
				}

				case Operator.GreaterThanOrEqual:
				{
					if (left is string s1 && right is string s2)
					{
						return Strings.StrCmp(s1, s2, true) >= 0;
					}
					else if (ParseNumericArgs(left, right, GreaterThanOrEqual, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
						{
							if (secondIsDouble)
								return firstd >= secondd;
							else
								return firstd >= secondl;
						}
						else
						{
							if (secondIsDouble)
								return firstl >= secondd;
							else
								return firstl >= secondl;
						}
					}

					return null;
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
					if (ParseNumericArgs(left, right, Modulo, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
						{
							if (secondIsDouble)
								return firstd % secondd;
							else
								return firstd % secondl;
						}
						else
						{
							if (secondIsDouble)
								return firstl % secondd;
							else
								return firstl % secondl;
						}
					}

					return null;
				}

				case Operator.Power:
				{
					if (ParseNumericArgs(left, right, Power, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
						{
							if (secondIsDouble)
								return Math.Pow(firstd, secondd);
							else
								return Math.Pow(firstd, secondl);
						}
						else
						{
							if (secondIsDouble)
								return Math.Pow(firstl, secondd);
							else
								return (long)Math.Pow(firstl, secondl);
						}
					}

					return null;
				}

				case Operator.Minus:
				case Operator.Subtract:
				{
					if (ParseNumericArgs(left, right, Subtraction, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
						{
							if (secondIsDouble)
								return firstd - secondd;
							else
								return firstd - secondl;
						}
						else
						{
							if (secondIsDouble)
								return firstl - secondd;
							else
							{
								if (left is IntPtr lip)
									return IntPtr.Subtract(lip, (int)secondl);
								else if (right is IntPtr rip)
									return IntPtr.Subtract(new IntPtr(firstl), (int)secondl);
								else
									return firstl - secondl;
							}
						}
					}

					return null;
				}

				case Operator.Multiply:
				{
					if (ParseNumericArgs(left, right, Multiply, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
						{
							if (secondIsDouble)
								return firstd * secondd;
							else
								return firstd * secondl;
						}
						else
						{
							if (secondIsDouble)
								return firstl * secondd;
							else
								return firstl * secondl;
						}
					}

					return null;
				}

				case Operator.Divide:
				{
					if (ParseNumericArgs(left, right, Divide, out var firstIsDouble, out var secondIsDouble, out var firstd, out var firstl, out var secondd, out var secondl))
					{
						if (firstIsDouble)
						{
							if (secondIsDouble)
							{
								if (secondd == 0.0)
									return Errors.ErrorOccurred(err = new ZeroDivisionError($"Right side operand of floating point division was 0.0")) ? throw err : null;

								return firstd / secondd;
							}
							else
							{
								if (secondl == 0)
									return Errors.ErrorOccurred(err = new ZeroDivisionError($"Right side operand of floating point division was 0")) ? throw err : null;

								return firstd / secondl;
							}
						}
						else
						{
							if (secondIsDouble)
							{
								if (secondd == 0.0)
									return Errors.ErrorOccurred(err = new ZeroDivisionError($"Right side operand of floating point division was 0.0")) ? throw err : null;

								return firstl / secondd;
							}
							else
							{
								if (secondl == 0)
									return Errors.ErrorOccurred(err = new ZeroDivisionError($"Right side operand of floating point division was 0")) ? throw err : null;

								return (double)firstl / secondl;
							}
						}
					}

					return null;
				}

				case Operator.Is:
				{ 
					if (left == null || right == null)
						return left == right;
					return IfLegacy(left, "is", ForceString(right));
				}

				default:
					return Errors.ErrorOccurred(err = new ValueError($"Operator {op} cannot be applied to: {left} and {right}")) ? throw err : null;
			}
		}

		private static bool ParseNumericArgs(object left, object right, string desc, out bool firstIsDouble, out bool secondIsDouble, out double firstd, out long firstl, out double secondd, out long secondl)
		{
			Error err;
			firstIsDouble = false;
			secondIsDouble = false;
			firstd = 0.0;
			firstl = 0L;
			secondd = 0.0;
			secondl = 0L;

			if (left == null)
				return Errors.ErrorOccurred(err = new UnsetError($"Left side operand of {desc} was null.")) ? throw err : false;

			if (right == null)
				return Errors.ErrorOccurred(err = new UnsetError($"Right side operand of {desc} was null.")) ? throw err : false;

			if (left is double ld)//Check non-string types first as a hot path.
			{
				firstIsDouble = true;
				firstd = ld;
			}
			else if (left is long ll)
			{
				firstl = ll;
			}
			else if (left is IntPtr lip)
			{
				firstl = lip.ToInt64();
			}
			else if (left.ParseLong(ref firstl, false, false))
			{
			}
			else if (left.ParseDouble(ref firstd, false, true))
			{
				firstIsDouble = true;
			}
			else
			{
				return Errors.ErrorOccurred(err = new UnsetError($"Left side operand of {desc} could not be converted to a number.")) ? throw err : false;
			}

			if (right is double rd)
			{
				secondIsDouble = true;
				secondd = rd;
			}
			else if (right is long rl)
			{
				secondl = rl;
			}
			else if (right is IntPtr rip)
			{
				secondl = rip.ToInt64();
			}
			else if (right.ParseLong(ref secondl, false, false))
			{
			}
			else if (right.ParseDouble(ref secondd, false, true))
			{
				secondIsDouble = true;
			}
			else
			{
				return Errors.ErrorOccurred(err = new UnsetError($"Right side operand of {desc} could not be converted to a number.")) ? throw err : false;
			}

			return true;
		}

		public static object OperateTernary(bool result, ExpressionDelegate x, ExpressionDelegate y) => result ? x() : y();

		public static object MultiStatement(object arg1) => arg1;
        public static object MultiStatement(object arg1, object arg2) => arg2;
        public static object MultiStatement(object arg1, object arg2, object arg3) => arg3;
        public static object MultiStatement(object arg1, object arg2, object arg3, object arg4) => arg4;
        public static object MultiStatement(object arg1, object arg2, object arg3, object arg4, object arg5) => arg5;
        public static object MultiStatement(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6) => arg6;
        public static object MultiStatement(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7) => arg7;
        public static object MultiStatement(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8) => arg8;

        public static object MultiStatement(params object[] args) => args[ ^ 1];

        public static void InitStaticVariable(ref object variable, string name, Func<object> initFunc)
        {
            if (Flow.initializedUserStaticVariables.Contains(name))
                return;
            Flow.initializedUserStaticVariables.Add(name);
            variable = initFunc();
        }

        public static object OperateUnary(Operator op, object right)
		{
			Error err;

			switch (op)
			{
				case Operator.Minus:
				case Operator.Subtract:
				{
					if (right == null)
						return Errors.ErrorOccurred(err = new UnsetError("Right side operand of subtraction or minus was null.")) ? throw err : null;

					var l = 0L;
					var d = 0.0;

					if (right is double rd)//Check non-string types first as a hot path.
						return rd == 0d ? rd : -rd;
					else if (right is long rl)
						return -rl;
					else if (right.ParseLong(ref l, false, false))
						return -l;
					else if (right.ParseDouble(ref d, false, true))
						return d == 0d ? d : -d;
					else
						_ = Errors.ErrorOccurred(err = new UnsetError("Right side operand of multiply could not be converted to a number.")) ? throw err : "";

					return null;
				}

				case Operator.LogicalNot:
				case Operator.LogicalNotEx:
					return !IfTest(right);

				case Operator.BitwiseNot:
				{
					if (right == null)
						return Errors.ErrorOccurred(err = new UnsetError("Right side operand of bitwise not was null.")) ? throw err : null;

					if (right is double)
						return Errors.ErrorOccurred(err = new TypeError($"Unary operand of logical not was not an integer, and instead was of type {right.GetType()}.")) ? throw err : null;

					var l = 0L;

					if (right.ParseLong(ref l, false, false))
						return ~l;

					return Errors.ErrorOccurred(err = new TypeError($"Unary operand of logical not was not an integer, and instead was of type {right.GetType()}.")) ? throw err : null;
				}

				//Not supporting references at this time.
				//case Operator.Dereference:
				// TODO: dereference operator
				//return null;
				//case Operator.BitwiseAnd:
				//return GCHandle.Alloc(right, GCHandleType.Pinned).AddrOfPinnedObject().ToInt64();//This seems almost certainly wrong, and would need to be freed elsewhere.

				default:
					return Errors.ErrorOccurred(err = new ValueError($"Operator {op} cannot be applied to: {right}")) ? throw err : null;
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