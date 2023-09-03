using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Keysharp.Core
{
	/// <summary>
	/// Most functions here do not take variadic parameters so they can run as fast as possible.
	/// Also, an attempt to cast the object argument to a double is first made because it's the
	/// most common and fastest case. If it's not a double, ParseDouble() is used.
	/// </summary>
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

		/// <summary>
		/// Returns the absolute value of a number.
		/// </summary>
		/// <param name="n">Any number.</param>
		/// <returns>The magnitude of <paramref name="n"/>.</returns>
		public static double Abs(object obj) => Math.Abs(obj is double d ? d : obj.Ad());

		/// <summary>
		/// Returns the angle whose cosine is the specified number.
		/// </summary>
		/// <param name="n">A number representing a cosine, where -1 ≤ <paramref name="n"/> ≤ 1.</param>
		/// <returns>An angle, θ, measured in radians, such that 0 ≤ θ ≤ π.</returns>
		public static double ACos(object obj)
		{
			var n = obj is double d ? d : obj.Ad();

			if (n < -1 || n > 1)
				throw new Error($"ACos() argument of {n} was not between -1 and 1 {new StackFrame(0).GetMethod().Name}");

			return Math.Acos(n);
		}

		/// <summary>
		/// Returns the angle whose sine is the specified number.
		/// </summary>
		/// <param name="n">A number representing a sine, where -1 ≤ <paramref name="n"/> ≤ 1.</param>
		/// <returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2.</returns>
		public static double ASin(object obj)
		{
			var n = obj is double d ? d : obj.Ad();

			if (n < -1 || n > 1)
				throw new Error($"ASin() argument of {n} was not between -1 and 1 {new StackFrame(0).GetMethod().Name}");

			return Math.Asin(n);
		}

		/// <summary>
		/// Returns the angle whose tangent is the specified number.
		/// </summary>
		/// <param name="n">A number representing a tangent.</param>
		/// <returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2.</returns>
		public static double ATan(object obj) => Math.Atan(obj is double d ? d : obj.Ad());

		/// <summary>
		/// Returns the angle whose tangent is the y/x number.
		/// </summary>
		/// <param name="n">A number representing a tangent.</param>
		/// <returns>An angle, θ, measured in radians, such that -(y/x)/2 ≤ θ ≤ (y/x)/2.</returns>
		public static double ATan2(object obj0, object obj1) => Math.Atan2(obj0 is double d0 ? d0 : obj0.Ad(), obj1 is double d1 ? d1 : obj1.Ad());

		/// <summary>
		/// Returns the smallest integer greater than or equal to the specified double number.
		/// </summary>
		/// <param name="n">A number.</param>
		/// <returns>The smallest integer greater than or equal to <paramref name="n"/>.</returns>
		public static double Ceil(object obj) => Math.Ceiling(obj is double d ? d : obj.Ad());

		/// <summary>
		/// Returns the cosine of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The cosine of <paramref name="n"/>.</returns>
		public static double Cos(object obj) => Math.Cos(obj is double d ? d : obj.ParseDouble().Value);

		/// <summary>
		/// Returns the hyperbolic cosine of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The hyperbolic cosine of <paramref name="n"/>.</returns>
		public static double Cosh(object obj) => Math.Cosh(obj is double d ? d : obj.Ad());

		public static string DateAdd(object obj0, object obj1, object obj2)
		{
			var s1 = obj0.As();
			var t = obj1.Ad();
			var units = obj2.As();

			if (s1.Length == 0)
				s1 = Accessors.A_Now;

			var d1 = Conversions.ToDateTime(s1);

			if (units.StartsWith("s", StringComparison.OrdinalIgnoreCase))
				d1 = d1.AddSeconds(t);
			else if (units.StartsWith("m", StringComparison.OrdinalIgnoreCase))
				d1 = d1.AddMinutes(t);
			else if (units.StartsWith("h", StringComparison.OrdinalIgnoreCase))
				d1 = d1.AddHours(t);
			else
				d1 = d1.AddDays(t);

			return Conversions.ToYYYYMMDDHH24MISS(d1);
		}

		public static long DateDiff(object obj0, object obj1, object obj2)
		{
			var s1 = obj0.As();
			var s2 = obj1.As();
			var units = obj2.As();

			if (s1.Length == 0)
				s1 = Accessors.A_Now;

			if (s2.Length == 0)
				s2 = Accessors.A_Now;

			var d1 = Conversions.ToDateTime(s1);
			var d2 = Conversions.ToDateTime(s2);
			var diff = d1 - d2;

			if (units.StartsWith("s", StringComparison.OrdinalIgnoreCase))
				return diff.Seconds;
			else if (units.StartsWith("m", StringComparison.OrdinalIgnoreCase))
				return diff.Minutes;
			else if (units.StartsWith("h", StringComparison.OrdinalIgnoreCase))
				return diff.Hours;
			else
				return diff.Days;
		}

		/// <summary>
		/// Returns <c>e</c> raised to the specified power.
		/// </summary>
		/// <param name="n">A number specifying a power.</param>
		/// <returns>The number <c>e</c> raised to the power <paramref name="n"/>.</returns>
		public static double Exp(object obj) => Math.Exp(obj is double d ? d : obj.Ad());

		/// <summary>
		/// Returns the largest integer less than or equal to the specified double number.
		/// </summary>
		/// <param name="n">A number.</param>
		/// <returns>The largest integer less than or equal to <paramref name="n"/>.</returns>
		public static double Floor(object obj) => Math.Floor(obj is double d ? d : obj.Ad());

		/// <summary>
		/// Returns the integer portion of the specified double number.
		/// </summary>
		/// <param name="n">A number.</param>
		/// <returns>The integer portion of <paramref name="n"/>.</returns>
		public static long Integer(object obj) => (long)(obj is double d ? d : obj.Ad());

		/// <summary>
		/// Returns the natural (base e) logarithm of a specified number.
		/// </summary>
		/// <param name="n">A number whose logarithm is to be found.</param>
		/// <returns>The natural logarithm of <paramref name="n"/> if it's positive, else an exception is thrown.</returns>
		public static double Ln(object obj)
		{
			var n = obj is double d ? d : obj.Ad();

			if (n < 0)
				throw new Error($"Ln() argument {n} was negative {new StackFrame(0).GetMethod().Name}");

			return Math.Log(n);
		}

		/// <summary>
		/// Returns the logarithm of a specified number in a specified base.
		/// </summary>
		/// <param name="n">A number whose logarithm is to be found.</param>
		/// <param name="b">The base of the logarithm. If unspecified this is <c>10</c>.</param>
		/// <returns>The logarithm of <paramref name="n"/> to base <paramref name="b"/> if n is positive, else an exception is thrown.</returns>
		public static double Log(object obj0, object obj1 = null)
		{
			var n = obj0.Ad(double.MinValue);
			var b = obj1.Ad(double.MinValue);

			if (n < 0)
				throw new Error($"Log() argument {n} was negative {new StackFrame(0).GetMethod().Name}");

			if (b != double.MinValue)
				return b == 10 ? Math.Log10(n) : Math.Log(n, b);
			else if (n != double.MinValue)
				return Math.Log10(n);

			return 0.0;
		}

		/// <summary>
		/// Returns the greatest of a list of numbers.
		/// </summary>
		/// <param name="dividend">The first number to compare.</param>
		/// <param name="divisor">The second number to compare.</param>
		/// <returns>The larger of the two numbers, or the empty string if either number is not numeric.</returns>
		public static object Max(params object[] obj)
		{
			var o = obj.L();//Flatten here to find the max of anything contained in any sub item.

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
		/// Returns the least of a list of numbers.
		/// </summary>
		/// <param name="dividend">The first number to compare.</param>
		/// <param name="divisor">The second number to compare.</param>
		/// <returns>The lesser of the two numbers, or the empty string if either number is not numeric.</returns>
		public static object Min(params object[] obj)
		{
			var o = obj.L();//Flatten here to find the max of anything contained in any sub item.

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
		public static object Mod(object obj0, object obj1)
		{
			if (obj0 is double || obj1 is double)
			{
				var dividend = obj0.Ad();
				var divisor = obj1.Ad();

				if (divisor == 0)
					throw new ZeroDivisionError($"Mod() divisor argument of {divisor} was 0 {new StackFrame(0).GetMethod().Name}");

				//return Math.IEEERemainder(dividend, divisor);
				return dividend % divisor;
			}
			else
			{
				var dividend = obj0.Al();
				var divisor = obj1.Al();

				if (divisor == 0)
					throw new Error($"Mod() divisor argument of {divisor} was 0 {new StackFrame(0).GetMethod().Name}");

				return dividend % divisor;
			}
		}

		public static object Number(object obj0)
		{
			if (obj0 is long l)
				return l;
			else if (obj0 is double d)
				return d;
			else
			{
				var s = obj0.As();

				if (s.Contains('.'))
				{
					var val = s.ParseDouble(false);

					if (val.HasValue)
						return val.Value;
				}
				else
				{
					var val = s.ParseLong(false);

					if (val.HasValue)
						return val.Value;
				}

				throw new TypeError($"Could not convert {obj0.As()} to an integer or float.");
			}
		}

		/// <summary>
		/// Returns a random number within a specified range.
		/// </summary>
		/// <param name="result">The name of the variable in which to store the result.</param>
		/// <param name="min">The inclusive lower bound of the random number returned. Default: 0.</param>
		/// <param name="max">The exclusive upper bound of the random number returned. Default 1.</param>
		/// <returns>A random number in the range of <c><paramref name="min"/> - (<paramref name="max/> Q)</c>,
		/// <remarks>If <paramref name="min"/> and <paramref name="max"/> are both integers <paramref name="result"/> will also be an integer.
		/// Otherwise <paramref name="result"/> can be a floating point number.</remarks>
		public static object Random(object obj0 = null, object obj1 = null)
		{
			var r = RandomGenerator;

			if (obj0 is null && obj1 is null)
				return r.NextDouble();

			if (obj0 is long l0)
			{
				if (obj1 is long l1)
				{
					var min = Math.Min(l0, l1);
					var max = Math.Max(l0, l1);
					return r.NextInt64(min, max + 1L);//Integer ranges include the max number.
				}
				else if (obj1 is null)
				{
					var min = Math.Min(0L, l0);
					var max = Math.Max(0L, l0);
					return r.NextInt64(min, max + 1L);//If one param is omitted, it defaults to 0.
				}
			}
			else if (obj1 is long l11)
			{
				var min = Math.Min(0L, l11);
				var max = Math.Max(0L, l11);
				return r.NextInt64(min, max + 1L);//If one param is omitted, it defaults to 0.
			}

			var mind = obj0.Ad();
			var maxd = obj1.Ad();
			var lower = Math.Min(mind, maxd);
			var upper = Math.Max(mind, maxd);
			return r.NextDouble(lower, upper);
		}

		public static void RandomSeed(object obj) => randomGenerator = new Random(obj.Ai());

		/// <summary>
		/// Rounds a number to a specified number of fractional digits.
		/// </summary>
		/// <param name="number">A double number to be rounded.</param>
		/// <param name="n">The number of double places in the return value.</param>
		/// <returns>The number nearest to <paramref name="number"/> that contains a number of fractional digits equal to <paramref name="n"/>.</returns>
		public static double Round(object obj0, object obj1 = null)
		{
			var number = obj0.Ad();
			var n = obj1.Al();
			var mult = n != 0 ? Math.Pow(10, n) : 1;//Code taken from AHK.
			return (number >= 0.0 ? Math.Floor(number * mult + 0.5) : Math.Ceiling((number * mult) - 0.5)) / mult;
		}

		/// <summary>
		/// Returns the sine of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The sine of <paramref name="n"/>.</returns>
		public static double Sin(object obj) => Math.Sin(obj is double d ? d : obj.Ad());

		/// <summary>
		/// Returns the hyperbolic sine of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The hyperbolic sine of <paramref name="n"/>.</returns>
		public static double Sinh(object obj) => Math.Sinh(obj is double d ? d : obj.Ad());

		/// <summary>
		/// Returns the square root of a specified number.
		/// </summary>
		/// <param name="n">A number.</param>
		/// <returns>The positive square root of <paramref name="n"/> if positive, else an exception is thrown.</returns>
		public static double Sqrt(object obj)
		{
			var n = obj is double d ? d : obj.Ad();

			if (n < 0)
				throw new Error($"Sqrt() argument of {n} was negative {new StackFrame(0).GetMethod().Name}");

			return Math.Sqrt(n);
		}

		/// <summary>
		/// Returns the tangent of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The tangent of <paramref name="n"/>.</returns>
		public static double Tan(object obj) => Math.Tan(obj is double d ? d : obj.Ad());

		/// <summary>
		/// Returns the hyperbolic tangent of the specified angle.
		/// </summary>
		/// <param name="n">An angle, measured in radians.</param>
		/// <returns>The hyperbolic tangent of <paramref name="n"/>.</returns>
		public static double Tanh(object obj) => Math.Tanh(obj is double d ? d : obj.Ad());

		/// <summary>
		/// Calculates the integral part of a specified number.
		/// </summary>
		/// <param name="n">A number to truncate.</param>
		/// <returns>The integral part of <paramref name="n"/>; that is, the number that remains after any fractional digits have been discarded.</returns>
		public static double Truncate(object obj) => Math.Truncate(obj is double d ? d : obj.Ad());
	}
}