using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using Keysharp.Core;
using static Keysharp.Core.Core;
using static Keysharp.Core.Env;
using static Keysharp.Core.Loops;
using Keysharp.Scripting;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Flow")]
		public void FlowIf() => Assert.IsTrue(TestScript("flow-if", true));

		[Test, Category("Flow")]
		public void FlowLoop()
		{
			const long n = 10L;
			var x = 0L;
			Assert.AreEqual(0L, Accessors.A_Index);

			foreach (long i in Loop(n))
			{
				Assert.AreEqual(++x, i);
				Assert.AreEqual(i, Accessors.A_Index);
			}

			Pop();//Caller is always required to do this.
			Assert.AreEqual(x, n);
			Assert.AreEqual(0L, Accessors.A_Index);
			x = 0;

			foreach (long i in Loop(n))
			{
				x++;

				if (i > 5)
					break;
			}

			Pop();//Caller is always required to do this.
			Assert.AreEqual(x, 6L);
			Assert.AreEqual(0L, Accessors.A_Index);
			Assert.IsTrue(TestScript("flow-loop", true));
		}

		[Test, Category("Flow")]
		public void FlowWhile()
		{
			const long n = 10L;
			var x = 0L;
			Assert.AreEqual(0L, Accessors.A_Index);

			foreach (long i in Loop(n))
			{
				Assert.AreEqual(++x, i);
				Assert.AreEqual(i, Accessors.A_Index);
			}

			Pop();//Caller is always required to do this.
			Assert.AreEqual(x, n);
			Assert.AreEqual(0L, Accessors.A_Index);
			Assert.IsTrue(TestScript("flow-while", true));
		}

		[Test, Category("Flow")]
		public void FlowLoopParse()
		{
			Assert.IsTrue(TestScript("flow-loop-parse", true));
		}

		[Test, Category("Flow")]
		public void FlowUntil()
		{
			Assert.IsTrue(TestScript("flow-until", true));
		}

		[Test, Category("Flow")]
		public void FlowLoopRead()
		{
			Assert.IsTrue(TestScript("flow-loop-read", true));
		}

		[Test, Category("Flow")]
		public void FlowSwitch()
		{
			Assert.IsTrue(TestScript("flow-switch", true));
		}

		[Test, Category("Flow")]
		public void FlowLoopReg()
		{
			Registrys.RegDeleteKey(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest");
			//
			Registrys.RegWrite("ksdefval", "REG_SZ", @"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "");
			var val = Registrys.RegRead(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "");
			Assert.AreEqual("ksdefval", val);
			//
			Registrys.RegWrite("ksval", "REG_SZ", @"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "testval");
			val = Registrys.RegRead(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "testval");
			Assert.AreEqual("ksval", val);
			//
			Registrys.RegWrite("stringone\nstringtwo\nstringthree", "REG_MULTI_SZ", @"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1", "");
			val = Registrys.RegRead(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1", "");
			Assert.AreEqual(new string[] { "stringone", "stringtwo", "stringthree" }, val);
			//
			Registrys.RegWrite(1, "REG_DWORD", @"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "dword1");
			val = Registrys.RegRead(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "dword1");
			Assert.AreEqual(1, val);
			//
			Registrys.RegWrite(2, "REG_QWORD", @"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "qword1");
			val = Registrys.RegRead(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "qword1");
			Assert.AreEqual(2, val);
			//
			Registrys.RegWrite("AABBCCDD", "REG_BINARY", @"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub2", "bin1");
			val = Registrys.RegRead(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub2", "bin1");
			Assert.AreEqual(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }, val);
			//
			var i = 0;

			//
			foreach (var reg in Loops.LoopRegistry(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "kvr"))
			{
				val = Registrys.RegRead();

				//
				if (i == 0)
					Assert.AreEqual("ksval", val);
				else if (i == 1)
					Assert.AreEqual("ksdefval", val);
				else if (i == 2)
					Assert.AreEqual(null, val);
				else if (i == 3)
					Assert.AreEqual(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }, val);
				else if (i == 4)
					Assert.AreEqual(new string[] { "stringone", "stringtwo", "stringthree" }, val);
				else if (i == 5)
					Assert.AreEqual(new string[] { "stringone", "stringtwo", "stringthree" }, val);
				else if (i == 6)
					Assert.AreEqual(null, val);
				else if (i == 7)
					Assert.AreEqual(2, val);
				else if (i == 8)
					Assert.AreEqual(1, val);

				i++;
			}

			_ = Pop();
			//
			Registrys.RegDelete(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "testval");
			Registrys.RegDelete(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1", "");
			Registrys.RegDelete(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "dword1");
			Registrys.RegDelete(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "qword1");
			Registrys.RegDelete(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub2", "bin1");
			Registrys.RegDeleteKey(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub2");
			Registrys.RegDeleteKey(@"HKEY_CURRENT_USER\SOFTWARE\KeysharpTest");
			Assert.IsTrue(TestScript("flow-loop-reg", true));
		}

		[Test, Category("Flow")]
		public void FlowTryCatch()
		{
			Assert.IsTrue(TestScript("flow-trycatch", true));
		}
	}
}