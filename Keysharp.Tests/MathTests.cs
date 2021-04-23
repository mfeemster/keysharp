using System;
using System.Collections;
using Keysharp.Core;
using NUnit.Framework;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Math")]
		public void Abs()
		{
			Assert.AreEqual(1, Maths.Abs(1));
			Assert.AreEqual(1, Maths.Abs(-1));
			Assert.AreEqual(9.81, Maths.Abs(-9.81));
			Assert.AreEqual(0, Maths.Abs(-0));
			//
			Assert.AreEqual(1, Maths.Abs(new object[] { 1 }));
			Assert.AreEqual(1, Maths.Abs(new object[] { -1 }));
			Assert.AreEqual(9.81, Maths.Abs(new object[] { -9.81 }));
			Assert.AreEqual(0, Maths.Abs(new object[] { -0 }));
			//
			Assert.AreEqual(1, Maths.Abs(new ArrayList { 1 }));
			Assert.AreEqual(1, Maths.Abs(new ArrayList { -1 }));
			Assert.AreEqual(9.81, Maths.Abs(new ArrayList { -9.81 }));
			Assert.AreEqual(0, Maths.Abs(new ArrayList { -0 }));
			//
			Assert.IsTrue(TestScript("math-abs", true));
		}

		[Test, Category("Math")]
		public void ACos()
		{
			foreach (var n in new[] { -1, -0.5, 0, -0, 0.5, 1, 0.675 })
			{
				Assert.AreEqual(Math.Acos(n), Maths.ACos(n));
				Assert.AreEqual(Math.Acos(n), Maths.ACos(new object[] { n }));
				Assert.AreEqual(Math.Acos(n), Maths.ACos(new ArrayList { n }));
			}
			var caught = false;

			try
			{
				var x = Maths.ACos(-1.1);
			}
			catch (Exception)
			{
				caught = true;
			}

			Assert.IsTrue(caught);
			caught = false;

			try
			{
				var x = Maths.ACos(-1.1);
			}
			catch (Exception)
			{
				caught = true;
			}

			Assert.IsTrue(caught);
			Assert.IsTrue(TestScript("math-acos", true));
		}

		[Test, Category("Math")]
		public void ASin()
		{
			foreach (var n in new[] { -1, -0.5, 0, -0, 0.5, 1, 0.675 })
			{
				Assert.AreEqual(Math.Asin(n), Maths.ASin(n));
				Assert.AreEqual(Math.Asin(n), Maths.ASin(new object[] { n }));
				Assert.AreEqual(Math.Asin(n), Maths.ASin(new ArrayList { n }));
			}
			var caught = false;

			try
			{
				var x = Maths.ASin(-1.1);
			}
			catch (Exception)
			{
				caught = true;
			}

			Assert.IsTrue(caught);
			caught = false;

			try
			{
				var x = Maths.ASin(-1.1);
			}
			catch (Exception)
			{
				caught = true;
			}

			Assert.IsTrue(caught);
			Assert.IsTrue(TestScript("math-asin", true));
		}

		[Test, Category("Math")]
		public void ATan()
		{
			foreach (var n in new[] { -1, -0.5, 0, -0, 0.5, 1, 0.675 })
			{
				Assert.AreEqual(Math.Atan(n), Maths.ATan(n));
				Assert.AreEqual(Math.Atan(n), Maths.ATan(new object[] { n }));
				Assert.AreEqual(Math.Atan(n), Maths.ATan(new ArrayList { n }));
			}
			Assert.IsTrue(TestScript("math-atan", true));
		}

		[Test, Category("Math")]
		public void Atan2()
		{
			var n = new[] { -1, -0.5, 0, -0, 0.5, 1, 0.675 };

			for (var i = 1; i < n.Length; i++)
			{
				var x = n[i];
				var y = n[i - 1];
				Assert.AreEqual(Math.Atan2(y, x), Maths.ATan2(y, x));
				Assert.AreEqual(Math.Atan2(y, x), Maths.ATan2(new object[] { y, x }));
				Assert.AreEqual(Math.Atan2(y, x), Maths.ATan2(new ArrayList { y, x }));
			}

			Assert.IsTrue(TestScript("math-atan2", true));
		}

		[Test, Category("Math")]
		public void Ceil()
		{
			foreach (var n in new[] { -1, -2.1, 0, -0, 1.000001 })
			{
				Assert.AreEqual(Math.Ceiling(n), Maths.Ceil(n));
				Assert.AreEqual(Math.Ceiling(n), Maths.Ceil(new object[] { n }));
				Assert.AreEqual(Math.Ceiling(n), Maths.Ceil(new ArrayList { n }));
			}
			Assert.IsTrue(TestScript("math-ceil", true));
		}

		[Test, Category("Math")]
		public void Cos()
		{
			foreach (var n in new[] { -1, -0.5, 0, -0, 0.5, 1, 0.675 })
			{
				var v = n * Math.PI;
				Assert.AreEqual(Math.Cos(v), Maths.Cos(v));
				Assert.AreEqual(Math.Cos(v), Maths.Cos(new object[] { v }));
				Assert.AreEqual(Math.Cos(v), Maths.Cos(new ArrayList { v }));
			}
			Assert.IsTrue(TestScript("math-cos", true));
		}

		[Test, Category("Math")]
		public void Cosh()
		{
			foreach (var n in new[] { -1, -0.5, 0, -0, 0.5, 1, 0.675 })
			{
				var v = n * Math.PI;
				Assert.AreEqual(Math.Cosh(v), Maths.Cosh(v));
				Assert.AreEqual(Math.Cosh(v), Maths.Cosh(new object[] { v }));
				Assert.AreEqual(Math.Cosh(v), Maths.Cosh(new ArrayList { v }));
			}
			Assert.IsTrue(TestScript("math-cosh", true));
		}

		[Test, Category("Math")]
		public void Exp()
		{
			foreach (var n in new[] { -1, -0.5, 0, -0, 0.5, 1, 0.675 })
			{
				Assert.AreEqual(Math.Exp(n), Maths.Exp(n));
				Assert.AreEqual(Math.Exp(n), Maths.Exp(new object[] { n }));
				Assert.AreEqual(Math.Exp(n), Maths.Exp(new ArrayList { n }));
			}
			Assert.IsTrue(TestScript("math-exp", true));
		}

		[Test, Category("Math")]
		public void Floor()
		{
			foreach (var n in new[] { -1, -0.5, 0, -0, 0.5, 1, 0.67 })
			{
				Assert.AreEqual(Math.Floor(n), Maths.Floor(n));
				Assert.AreEqual(Math.Floor(n), Maths.Floor(new object[] { n }));
				Assert.AreEqual(Math.Floor(n), Maths.Floor(new ArrayList { n }));
			}
			Assert.IsTrue(TestScript("math-floor", true));
		}

		[Test, Category("Math")]
		public void Integer()
		{
			foreach (var n in new[] { -1, -2.1, 0, -0, 0.5, 1.000001 })
			{
				Assert.AreEqual((double)(long)(n), Maths.Integer(n));
				Assert.AreEqual((double)(long)(n), Maths.Integer(new object[] { n }));
				Assert.AreEqual((double)(long)(n), Maths.Integer(new ArrayList { n }));
			}
			Assert.IsTrue(TestScript("math-integer", true));
		}

		[Test, Category("Math")]
		public void Ln()
		{
			foreach (var n in new[] { 0, -0, 0.5, 1, 0.675 })
			{
				Assert.AreEqual(Math.Log(n), Maths.Ln(n));
				Assert.AreEqual(Math.Log(n), Maths.Ln(new object[] { n }));
				Assert.AreEqual(Math.Log(n), Maths.Ln(new ArrayList { n }));
			}
			var caught = false;

			try
			{
				var x = Maths.Ln(-1);
			}
			catch (Exception)
			{
				caught = true;
			}

			Assert.IsTrue(caught);
			caught = false;

			try
			{
				var x = Maths.Ln(-0.5);
			}
			catch (Exception)
			{
				caught = true;
			}

			Assert.IsTrue(caught);
			Assert.IsTrue(TestScript("math-ln", true));
		}

		[Test, Category("Math")]
		public void Log()
		{
			foreach (var n in new[] { 0, 0.5, 1, 0.675 })
			{
				foreach (var b in new[] { -1, 0, 1, 2, 3, 4 })
				{
					Assert.AreEqual(Math.Log(n, b), Maths.Log(n, b));
					Assert.AreEqual(Math.Log(n, b), Maths.Log(new object[] { n, b }));
					Assert.AreEqual(Math.Log(n, b), Maths.Log(new ArrayList { n, b }));
				}
			}

			foreach (var n in new[] { 0, 0.5, 1, 0.675 })
			{
				Assert.AreEqual(Math.Log10(n), Maths.Log(n));
				Assert.AreEqual(Math.Log10(n), Maths.Log(new object[] { n }));
				Assert.AreEqual(Math.Log10(n), Maths.Log(new ArrayList { n }));
			}
			var caught = false;

			try
			{
				var x = Maths.Log(-1);
			}
			catch (Exception)
			{
				caught = true;
			}

			Assert.IsTrue(caught);
			caught = false;

			try
			{
				var x = Maths.Log(-0.5);
			}
			catch (Exception)
			{
				caught = true;
			}

			Assert.IsTrue(caught);
		}

		[Test, Category("Math")]
		public void Max()
		{
			Assert.AreEqual(-6, Maths.Max(-6, -6));
			Assert.AreEqual(-5, Maths.Max(-6, -5));
			Assert.AreEqual(-4.2, Maths.Max(-4.2, -5.0));
			Assert.AreEqual(0, Maths.Max(0, 0));
			Assert.AreEqual(1, Maths.Max(0, 1));
			Assert.AreEqual(1, Maths.Max(1, 1));
			Assert.AreEqual(2.3, Maths.Max(1.5, 2.3));
			Assert.AreEqual(0.675, Maths.Max(new object[] { -1.0, 0.675 }));
			Assert.AreEqual(1, Maths.Max(new object[] { -1.0, -0.5, 0, 0.5, 1, 0.675 }));
			Assert.AreEqual(2, Maths.Max(new object[] { -1.0, -0.5, 0, 0.5, 1, 0.675, 2.0 }));
			Assert.AreEqual(0.675, Maths.Max(new ArrayList { -1.0, 0.675 }));
			Assert.AreEqual(1, Maths.Max(new ArrayList { -1.0, -0.5, 0, 0.5, 1, 0.675 }));
			Assert.AreEqual(2, Maths.Max(new ArrayList { -1.0, -0.5, 0, 0.5, 1, 0.675, 2.0 }));
			Assert.AreEqual(1, Maths.Max(-1.0, -0.5, 0, 0.5, 1, 0.675));
			Assert.AreEqual(2, Maths.Max(-1.0, -0.5, 0, 0.5, 1, 0.675, 2.0));
			Assert.AreEqual(string.Empty, Maths.Max(new object[] { -1.0, "asdf" }));
			Assert.AreEqual(string.Empty, Maths.Max(new ArrayList { -1.0, "asdf" }));
			Assert.IsTrue(TestScript("math-max", true));
		}

		[Test, Category("Math")]
		public void Min()
		{
			Assert.AreEqual(-6, Maths.Min(-6, -6));
			Assert.AreEqual(-6, Maths.Min(-6, -5));
			Assert.AreEqual(-5.0, Maths.Min(-4.2, -5.0));
			Assert.AreEqual(0, Maths.Min(0, 0));
			Assert.AreEqual(0, Maths.Min(0, 1));
			Assert.AreEqual(1, Maths.Min(1, 1));
			Assert.AreEqual(1.5, Maths.Min(1.5, 2.3));
			Assert.AreEqual(-1.0, Maths.Min(new object[] { -1.0, 0.675 }));
			Assert.AreEqual(-1.0, Maths.Min(new object[] { -1.0, -0.5, 0, 0.5, 1, 0.675 }));
			Assert.AreEqual(-1.0, Maths.Min(new object[] { -1.0, -0.5, 0, 0.5, 1, 0.675, 2.0 }));
			Assert.AreEqual(-1.0, Maths.Min(new ArrayList { -1.0, 0.675 }));
			Assert.AreEqual(-1.0, Maths.Min(new ArrayList { -1.0, -0.5, 0, 0.5, 1, 0.675 }));
			Assert.AreEqual(-1.0, Maths.Min(new ArrayList { -1.0, -0.5, 0, 0.5, 1, 0.675, 2.0 }));
			Assert.AreEqual(-1.0, Maths.Min(-1.0, -0.5, 0, 0.5, 1, 0.675));
			Assert.AreEqual(-1.0, Maths.Min(-1.0, -0.5, 0, 0.5, 1, 0.675, 2.0));
			Assert.AreEqual(string.Empty, Maths.Min(new object[] { -1.0, "asdf" }));
			Assert.AreEqual(string.Empty, Maths.Min(new ArrayList { -1.0, "asdf" }));
			Assert.IsTrue(TestScript("math-min", true));
		}

		[Test, Category("Math")]
		public void Mod()
		{
			foreach (var n in new[] { -1, 0, 1, 2, 3, 4 })
			{
				foreach (var d in new[] { -1, -0.5, 0.5, 1, 0.675 })
				{
					Assert.AreEqual(d == 0 ? 0 : n % d, Maths.Mod(n, d));
					Assert.AreEqual(d == 0 ? 0 : n % d, Maths.Mod(new object[] { n, d }));
					Assert.AreEqual(d == 0 ? 0 : n % d, Maths.Mod(new ArrayList { n, d }));
				}
			}
			var caught = false;

			try
			{
				var x = Maths.Mod(1, 0);
			}
			catch (Exception)
			{
				caught = true;
			}

			Assert.IsTrue(caught);
		}

		[Test, Category("Math")]
		public void Random()
		{
			var ct = 100000;

			for (var i = 0; i < ct; i++)
			{
				var d = Maths.Random();
				Assert.IsTrue(d >= 0 && d <= int.MaxValue);
			}

			for (var i = 0; i < ct; i++)
			{
				var d = Maths.Random(-int.MaxValue, 0);
				Assert.IsTrue(d >= -int.MaxValue && d <= 0);
			}

			for (var i = 0; i < ct; i++)
			{
				var d = Maths.Random(-10, 10);
				Assert.IsTrue(d >= -10 && d <= 10);
			}

			for (var i = 0; i < ct; i++)
			{
				var d = Maths.Random(-5.123, 5.123);
				Assert.IsTrue(d >= -5.123 && d <= 5.123);
			}

			Assert.IsTrue(TestScript("math-random", true));
		}

		[Test, Category("Math")]
		public void Remainder()
		{
			foreach (var n in new[] { -1, -0.5, 0, 0.5, 1, 0.675 })
			{
				foreach (var d in new[] { 2, 1 })
				{
					Assert.AreEqual(Math.IEEERemainder(n, d), Maths.Remainder(n, d));
					Assert.AreEqual(Math.IEEERemainder(n, d), Maths.Remainder(new object[] { n, d }));
					Assert.AreEqual(Math.IEEERemainder(n, d), Maths.Remainder(new ArrayList { n, d }));
				}
			}
			Assert.AreEqual(0, Maths.Remainder(1, 0));
			Assert.AreEqual(0, Maths.Remainder(new object[] { 1, 0 }));
			Assert.AreEqual(0, Maths.Remainder(new ArrayList { 1, 0 }));
		}

		[Test, Category("Math")]
		public void Round()
		{
			Assert.AreEqual(3, Maths.Round(3.14));
			Assert.AreEqual(3.1, Maths.Round(3.14, 1));
			Assert.AreEqual(350, Maths.Round(345, -1));
			Assert.AreEqual(300, Maths.Round(345, -2));
			Assert.AreEqual(-350, Maths.Round(-345, -1));
			Assert.AreEqual(-300, Maths.Round(-345, -2));
			Assert.AreEqual(0, Maths.Round(-0, -2));
			//
			Assert.AreEqual(3.1, Maths.Round(new object[] { 3.14, 1 }));
			Assert.AreEqual(350, Maths.Round(new object[] { 345, -1 }));
			Assert.AreEqual(300, Maths.Round(new object[] { 345, -2 }));
			Assert.AreEqual(-350, Maths.Round(new object[] { -345, -1 }));
			Assert.AreEqual(-300, Maths.Round(new object[] { -345, -2 }));
			Assert.AreEqual(0, Maths.Round(new object[] { -0, -2 }));
			//
			Assert.AreEqual(3.1, Maths.Round(new ArrayList { 3.14, 1 }));
			Assert.AreEqual(350, Maths.Round(new ArrayList { 345, -1 }));
			Assert.AreEqual(300, Maths.Round(new ArrayList { 345, -2 }));
			Assert.AreEqual(-350, Maths.Round(new ArrayList { -345, -1 }));
			Assert.AreEqual(-300, Maths.Round(new ArrayList { -345, -2 }));
			Assert.AreEqual(0, Maths.Round(new ArrayList { -0, -2 }));
			Assert.IsTrue(TestScript("math-round", true));
		}

		[Test, Category("Math")]
		public void Sin()
		{
			foreach (var n in new[] { -1, -0.5, 0, -0, 0.5, 1, 0.675 })
			{
				var v = n * Math.PI;
				Assert.AreEqual(Math.Sin(v), Maths.Sin(v));
				Assert.AreEqual(Math.Sin(v), Maths.Sin(new object[] { v }));
				Assert.AreEqual(Math.Sin(v), Maths.Sin(new ArrayList { v }));
			}
			Assert.IsTrue(TestScript("math-sin", true));
		}

		[Test, Category("Math")]
		public void Sinh()
		{
			foreach (var n in new[] { -1, -0.5, 0, -0, 0.5, 1, 0.675 })
			{
				var v = n * Math.PI;
				Assert.AreEqual(Math.Sinh(v), Maths.Sinh(v));
				Assert.AreEqual(Math.Sinh(v), Maths.Sinh(new object[] { v }));
				Assert.AreEqual(Math.Sinh(v), Maths.Sinh(new ArrayList { v }));
			}
			Assert.IsTrue(TestScript("math-sinh", true));
		}

		[Test, Category("Math")]
		public void Sqrt()
		{
			foreach (var n in new[] { 0, 1, 4, 9, 36, 12769, 8 })
			{
				Assert.AreEqual(Math.Sqrt(n), Maths.Sqrt(n));
				Assert.AreEqual(Math.Sqrt(n), Maths.Sqrt(new object[] { n }));
				Assert.AreEqual(Math.Sqrt(n), Maths.Sqrt(new ArrayList { n }));
			}
			var caught = false;

			try
			{
				var x = Maths.Sqrt(-1);
			}
			catch (Exception)
			{
				caught = true;
			}

			Assert.IsTrue(caught);
			Assert.IsTrue(TestScript("math-sqrt", true));
		}

		[Test, Category("Math")]
		public void Tan()
		{
			foreach (var n in new[] { -1, -0.5, 0, -0, 0.5, 1, 0.675 })
			{
				var v = n * Math.PI;
				Assert.AreEqual(Math.Tan(v), Maths.Tan(v));
				Assert.AreEqual(Math.Tan(v), Maths.Tan(new object[] { v }));
				Assert.AreEqual(Math.Tan(v), Maths.Tan(new ArrayList { v }));
			}
			Assert.IsTrue(TestScript("math-tan", true));
		}

		[Test, Category("Math")]
		public void Tanh()
		{
			foreach (var n in new[] { -1, -0.5, 0, -0, 0.5, 1, 0.675 })
			{
				var v = n * Math.PI;
				Assert.AreEqual(Math.Tanh(v), Maths.Tanh(v));
				Assert.AreEqual(Math.Tanh(v), Maths.Tanh(new object[] { v }));
				Assert.AreEqual(Math.Tanh(v), Maths.Tanh(new ArrayList { v }));
			}
			Assert.IsTrue(TestScript("math-tanh", true));
		}

		[Test, Category("Math")]
		public void Truncate()
		{
			foreach (var n in new[] { -1, -4.5, 0, -0, 2.5, 1, 8.675 })
			{
				Assert.AreEqual(Math.Truncate(n), Maths.Truncate(n));
				Assert.AreEqual(Math.Truncate(n), Maths.Truncate(new object[] { n }));
				Assert.AreEqual(Math.Truncate(n), Maths.Truncate(new ArrayList { n }));
			}
			Assert.IsTrue(TestScript("math-truncate", true));
		}
	}
}