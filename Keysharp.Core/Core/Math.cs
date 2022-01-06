using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Keysharp.Core
{
	public static class Maths
	{
		[ThreadStatic]
		private static Random randomGenerator;

		private static Random RandomGenerator
		{
			get
			{
				if (randomGenerator == null)
					randomGenerator = new Random((int)(DateTime.Now.Ticks & 0xFFFFFFFF));

				return randomGenerator;
			}
		}

		public static void RandomSeed(params object[] obj) => randomGenerator = new Random(obj.L().I1());

		/// <summary>
		/// Returns the absolute value of a number.
		/// </summary>
		/// <param name="n">Any number.</param>
		/// <returns>The magnitude of <paramref name="n"/>.</returns>

		public static double Abs(params object[] obj) => Math.Abs(obj.L().D1());

		/// <summary>
		/// Returns the angle whose cosine is the specified number.
		/// </summary>
		/// <param name="n">A number representing a cosine, where -1 ≤ <paramref name="n"/> ≤ 1.</param>
		/// <returns>An angle, θ, measured in radians, such that 0 ≤ θ ≤ π.</returns>
		public static double ACos(params object[] obj)
		{
			var n = obj.L().D1();

			if (n < -1 || n > 1)
				throw new Exception($"ACos() argument of {n} was not between -1 and 1 {new StackFrame(0).GetMethod().Name}");

			return Math.Acos(n);
		}

		/// <summary>
		/// Returns the angle whose sine is the specified number.
		/// </summary>
		/// <param name="n">A number representing a sine, where -1 ≤ <paramref name="n"/> ≤ 1.</param>
		/// <returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2.</returns>
		public static double ASin(params object[] obj)
		{
			var n = obj.L().D1();

			if (n < -1 || n > 1)
				throw new Exception($"ASin() argument of {n} was not between -1 and 1 {new StackFrame(0).GetMethod().Name}");

			return Math.Asin(n);
		}

		/// <summary>
		/// Returns the angle whose tangent is the specified number.
		/// </summary>
		/// <param name="n">A number representing a tangent.</param>
		/// <returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2.</returns>

		public static double ATan(params object[] obj) => Math.Atan(obj.L().D1());

		/// <summary>
		/// Returns the angle whose tangent is the y/x number.
		/// </summary>
		/// <param name="n">A number representing a tangent.</param>
		/// <returns>An angle, θ, measured in radians, such that -(y/x)/2 ≤ θ ≤ (y/x)/2.</returns>
		public static double ATan2(params object[] obj)
		{
			var (y, x) = obj.L().D2();
			return Math.Atan2(y, x);
		}

		/// <summary>
		/// Returns the smallest integer greater than or equal to the specified double number.
		/// </summary>
		/// <param name="n">A number.</param>
		/// <returns>The smallest integer greater than or equal to <paramref name="n"/>.</returns>
		public static double Ceil(params object[] obj) => Math.Ceiling(obj.L().D1());


		/// <summary>
		/// Returns the integer portion of the specified double number.
		/// </summary>
		/// <param name="n">A number.</param>
		/// <returns>The integer portion of <paramref name="n"/>.</returns>
		public static long Integer(params object[] obj) => (long)obj.L().D1();

		/// <summary>
		/// Returns the cosine of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The cosine of <paramref name="n"/>.</returns>
		public static double Cos(params object[] obj) => Math.Cos(obj.L().D1());

		public static double CosNoFlatten(params object[] obj) => Math.Cos(obj.D1());

		public static double CosSingleObj(object obj) => Math.Cos(obj.ParseDouble().Value);

		public static double CosDirectPass(double d) => Math.Cos(d);

		public static double CosDirectPassObject(object d) => Math.Cos((double)d);

		/// <summary>
		/// Returns the hyperbolic cosine of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The hyperbolic cosine of <paramref name="n"/>.</returns>
		public static double Cosh(params object[] obj) => Math.Cosh(obj.L().D1());

		/// <summary>
		/// Add a value to a variable using numeric or date-time arithmetic.
		/// </summary>
		/// <param name="var">A variable.</param>
		/// <param name="value">A number.</param>
		/// <param name="units">
		/// To use date arithmetic specify one of the following words:
		/// <c>seconds</c> (<c>s</c>), <c>minutes</c> (<c>m</c>), <c>hours</c> (<c>h</c>), <c>days</c> (<c>d</c>), <c>months</c> or <c>years</c> (<c>y</c>).
		/// If this parameter is blank the functions performs a numeric addition.
		/// </param>
		public static void EnvAdd(ref double var, double value, string units = null)
		{
			if (string.IsNullOrEmpty(units))
			{
				var += value;
				return;
			}

			var time = Conversions.ToDateTime(((int)var).ToString());

			switch (units.ToLowerInvariant())
			{
				case Core.Keyword_Seconds:
				case "s":
					time = time.AddSeconds(value);
					break;

				case Core.Keyword_Minutes:
				case "m":
					time = time.AddMinutes(value);
					break;

				case Core.Keyword_Hours:
				case "h":
					time = time.AddHours(value);
					break;

				case Core.Keyword_Days:
				case "d":
					time = time.AddDays(value);
					break;

				case Core.Keyword_Months:
				case "mn":
					time = time.AddMonths((int)value);
					break;

				case Core.Keyword_Years:
				case "y":
					time = time.AddYears((int)value);
					break;
			}

			var = Conversions.FromTime(time);
		}

		/// <summary>
		/// See <see cref="EnvAdd"/>.
		/// </summary>
		/// <param name="variable">A variable.</param>
		/// <param name="value">A value.</param>
		/// <param name="units">A numeric unit.</param>
		[Obsolete]
		public static void EnvSub(ref double variable, double value, string units = null) => EnvAdd(ref variable, -value, units);

		/// <summary>
		/// Returns <c>e</c> raised to the specified power.
		/// </summary>
		/// <param name="n">A number specifying a power.</param>
		/// <returns>The number <c>e</c> raised to the power <paramref name="n"/>.</returns>
		public static double Exp(params object[] obj) => Math.Exp(obj.L().D1());

		/// <summary>
		/// Returns the largest integer less than or equal to the specified double number.
		/// </summary>
		/// <param name="n">A number.</param>
		/// <returns>The largest integer less than or equal to <paramref name="n"/>.</returns>
		public static double Floor(params object[] obj) => Math.Floor(obj.L().D1());

		/// <summary>
		/// Returns the natural (base e) logarithm of a specified number.
		/// </summary>
		/// <param name="n">A number whose logarithm is to be found.</param>
		/// <returns>The natural logarithm of <paramref name="n"/> if it's positive, else an exception is thrown.</returns>
		public static double Ln(params object[] obj)
		{
			var n = obj.L().D1();

			if (n < 0)
				throw new Exception($"Ln() argument {n} was negative {new StackFrame(0).GetMethod().Name}");

			return Math.Log(n);
		}

		/// <summary>
		/// Returns the logarithm of a specified number in a specified base.
		/// </summary>
		/// <param name="n">A number whose logarithm is to be found.</param>
		/// <param name="b">The base of the logarithm. If unspecified this is <c>10</c>.</param>
		/// <returns>The logarithm of <paramref name="n"/> to base <paramref name="b"/> if n is positive, else an exception is thrown.</returns>
		public static double Log(params object[] obj)
		{
			var (n, b) = obj.L().D2(double.MinValue, double.MinValue);

			if (n < 0)
				throw new Exception($"Log() argument {n} was negative {new StackFrame(0).GetMethod().Name}");

			if (b != double.MinValue)
				return b == 10 ? Math.Log10(n) : Math.Log(n, b);
			else if (n != double.MinValue)
				return Math.Log10(n);

			return 0;
		}

		/// <summary>
		/// Returns the greater of two numbers.
		/// </summary>
		/// <param name="dividend">The first number to compare.</param>
		/// <param name="divisor">The second number to compare.</param>
		/// <returns>The larger of the two numbers, or the empty string if either number is not numeric.</returns>
		public static object Max(params object[] obj)
		{
			var o = obj.L();

			if (o.Count > 1)
			{
				var max = double.MinValue;

				for (var i = 1; i < o.Count; i++)
				{
					if (!Scripting.Script.IsNumeric(o[i]) || !Scripting.Script.IsNumeric(o[i - 1]))
						return string.Empty;

					var x = Convert.ToDouble(o[i]);
					var y = Convert.ToDouble(o[i - 1]);
					var z = Math.Max(x, y);

					if (z is double dz)
						max = Math.Max(max, dz);
				}

				if (max != double.MinValue)
					return max;
			}

			return string.Empty;
		}

		/// <summary>
		/// Returns the lesser of two numbers.
		/// </summary>
		/// <param name="dividend">The first number to compare.</param>
		/// <param name="divisor">The second number to compare.</param>
		/// <returns>The lesser of the two numbers, or the empty string if either number is not numeric.</returns>
		public static object Min(params object[] obj)
		{
			var o = obj.L();

			if (o.Count > 1)
			{
				var min = double.MaxValue;

				for (var i = 1; i < o.Count; i++)
				{
					if (!Scripting.Script.IsNumeric(o[i]) || !Scripting.Script.IsNumeric(o[i - 1]))
						return string.Empty;

					var x = Convert.ToDouble(o[i]);
					var y = Convert.ToDouble(o[i - 1]);
					var z = Math.Min(x, y);

					if (z is double dz)
						min = Math.Min(min, dz);
				}

				if (min != double.MaxValue)
					return min;
			}

			return string.Empty;
		}

		/// <summary>
		/// Returns the remainder after dividing two numbers.
		/// Throws an exception if divisor is 0.
		/// </summary>
		/// <param name="dividend">The dividend.</param>
		/// <param name="divisor">The divisor.</param>
		/// <returns>The remainder after dividing <paramref name="dividend"/> by <paramref name="divisor"/>.</returns>
		public static double Mod(params object[] obj)
		{
			var (dividend, divisor) = obj.L().D2();

			if (divisor == 0)
				throw new Exception($"Mod() divisor argument of {divisor} was 0 {new StackFrame(0).GetMethod().Name}");

			return dividend % divisor;
		}

		/// <summary>
		/// Returns a random number within a specified range.
		/// </summary>
		/// <param name="result">The name of the variable in which to store the result.</param>
		/// <param name="min">The inclusive lower bound of the random number returned. Default: 0.</param>
		/// <param name="max">The exclusive upper bound of the random number returned. Default int.MaxValue.</param>
		/// <returns>A random number in the range of <c><paramref name="min"/> - (<paramref name="max/> Q)</c>,
		/// <remarks>If <paramref name="min"/> and <paramref name="max"/> are both integers <paramref name="result"/> will also be an integer.
		/// Otherwise <paramref name="result"/> can be a floating point number.</remarks>
		public static double Random(params object[] obj)
		{
			var (min, max) = obj.L().D2(0.0, (double)int.MaxValue);
			var r = RandomGenerator;
			var x = Math.IEEERemainder(min, 1);
			var y = Math.IEEERemainder(max, 1);
			var z = (double)r.Next((int)min, (int)max);

			if (x != 0 || y != 0)
				z += (r.NextDouble() % Math.Abs(y - x)) + x;

			return z;
		}

		/// <summary>
		/// Returns the remainder resulting from the division of a specified number by another specified number.
		/// </summary>
		/// <param name="x">A dividend.</param>
		/// <param name="y">A divisor.</param>
		/// <returns>A number equal to <c><paramref name="x"/> - (<paramref name="y"/> Q)</c>,
		/// where <c>Q</c> is the quotient of <c><paramref name="x"/> / <paramref name="y"/></c> rounded to
		/// the nearest integer (if <c><paramref name="x"/> / y</c> falls halfway between two integers, the even integer is returned).
		///
		/// If <c><paramref name="x"/> - (<paramref name="y"/> Q)</c> is zero, the value <c>0</c> is returned.
		///
		/// If <c><paramref name="y"/> = 0</c>, <c>0</c> is returned.</returns>
		public static double Remainder(params object[] obj)
		{
			var (x, y) = obj.L().D2();
			return y == 0 ? 0 : Math.IEEERemainder(x, y);
		}

		/// <summary>
		/// Rounds a number to a specified number of fractional digits.
		/// </summary>
		/// <param name="n">A double number to be rounded.</param>
		/// <param name="doubles">The number of double places in the return value.</param>
		/// <returns>The number nearest to <paramref name="n"/> that contains a number of fractional digits equal to <paramref name="doubles"/>.</returns>
		public static double Round(params object[] obj)
		{
			var (n, doubles) = obj.L().Di();
			var mult = doubles != 0 ? Math.Pow(10, doubles) : 1;//Code taken from AHK.
			return (n >= 0.0 ? Math.Floor(n * mult + 0.5) : Math.Ceiling((n * mult) - 0.5)) / mult;
		}

		/// <summary>
		/// Returns the sine of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The sine of <paramref name="n"/>.</returns>
		public static double Sin(params object[] obj) => Math.Sin(obj.L().D1());

		/// <summary>
		/// Returns the hyperbolic sine of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The hyperbolic sine of <paramref name="n"/>.</returns>
		public static double Sinh(params object[] obj) => Math.Sinh(obj.L().D1());

		/// <summary>
		/// Returns the square root of a specified number.
		/// </summary>
		/// <param name="n">A number.</param>
		/// <returns>The positive square root of <paramref name="n"/> if positive, else an exception is thrown.</returns>
		public static double Sqrt(params object[] obj)
		{
			var n = obj.L().D1();

			if (n < 0)
				throw new Exception($"Sqrt() argument of {n} was negative {new StackFrame(0).GetMethod().Name}");

			return Math.Sqrt(n);
		}

		/// <summary>
		/// Returns the tangent of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The tangent of <paramref name="n"/>.</returns>
		public static double Tan(params object[] obj) => Math.Tan(obj.L().D1());

		/// <summary>
		/// Returns the hyperbolic tangent of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The hyperbolic tangent of <paramref name="n"/>.</returns>
		public static double Tanh(params object[] obj) => Math.Tanh(obj.L().D1());

		/// <summary>
		/// Calculates the integral part of a specified number.
		/// </summary>
		/// <param name="n">A number to truncate.</param>
		/// <returns>The integral part of <paramref name="n"/>; that is, the number that remains after any fractional digits have been discarded.</returns>
		public static double Truncate(params object[] obj) => Math.Truncate(obj.L().D1());
	}
}