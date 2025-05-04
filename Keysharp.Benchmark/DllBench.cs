using BenchmarkDotNet.Order;
using static Keysharp.Core.Functions;

namespace Keysharp.Benchmark
{
	[IterationCount(5)]
	[InvocationCount(5)]
	[WarmupCount(15)]
	public class DllBench : BaseTest
	{
		private static object mcode_e = Keysharp.Core.Collections.Map("1", 4L, "2", 1L);
		private static object mcode_c = (_ = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Accessors.A_PtrSize, 8L)) ? (object)(_ = "x64") : (object)(_ = "x86"));
		private static object p = 0L, ptr = 0L, result = 0L;

		public static object CallbackTwoArgs(object arg1, object arg2)
		{
			return _ = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, arg1, arg2);
		}

		public static object MCode(object mcode)
		{
			object v = null;
			object m = new VarRef(() => v, value => v = value);
			object s = null;
			object p = null;
			object op = null;

			if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Core.RegEx.RegExMatch(mcode, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "^([0-9]+),(", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, mcode_c, ":|.*?,"), mcode_c), ":)([^,]+)")), m))))
			{
				return _ = "";
			}

			s = 0L;

			if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Scripting.Script.InvokeWithRefs(Keysharp.Scripting.Script.GetMethodOrProperty(null, "DllCall", 15), "crypt32\\CryptStringToBinary", "str", Keysharp.Scripting.Script.GetPropertyValue(m, "3"), "uint", 0L, "uint", Keysharp.Scripting.Script.Index(mcode_e, Keysharp.Scripting.Script.GetPropertyValue(m, "1")), "ptr", 0L, "uint*", Misc.Mrh(10, s, v => s = v), "ptr", 0L, "ptr", 0L))))
			{
				return _ = "";
			}

			p = Keysharp.Core.Dll.DllCall("GlobalAlloc", "uint", 0L, "ptr", s, "ptr");

			if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, mcode_c, "x64")))
			{
				op = 0L;
				Keysharp.Scripting.Script.InvokeWithRefs(Keysharp.Scripting.Script.GetMethodOrProperty(null, "DllCall", 9), "VirtualProtect", "ptr", p, "ptr", s, "uint", 64L, "uint*", Misc.Mrh(8, op, v => op = v));
			}

			if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.InvokeWithRefs(Keysharp.Scripting.Script.GetMethodOrProperty(null, "DllCall", 15), "crypt32\\CryptStringToBinary", "str", Keysharp.Scripting.Script.GetPropertyValue(m, "3"), "uint", 0L, "uint", Keysharp.Scripting.Script.Index(mcode_e, Keysharp.Scripting.Script.GetPropertyValue(m, "1")), "ptr", p, "uint*", Misc.Mrh(10, s, v => s = v), "ptr", 0L, "ptr", 0L)))
			{
				return _ = p;
			}

			Keysharp.Core.Dll.DllCall("GlobalFree", "ptr", p);
			return "";
		}

		[Params(100000)]
		public int Size { get; set; }

		[Benchmark(Baseline = true)]
		public void CreateCallbackThenFree()
		{
			try
			{
				Keysharp.Core.Loops.Push(Keysharp.Core.LoopType.Normal);

				for (System.Collections.IEnumerator _ks_e0 = Keysharp.Core.Loops.Loop(Size).GetEnumerator(); Keysharp.Core.Flow.IsTrueAndRunning(_ks_e0.MoveNext());
					)
				{
					p = Keysharp.Core.Dll.CallbackCreate(Keysharp.Core.Functions.Func(CallbackTwoArgs));
					result = Keysharp.Core.Dll.DllCall(ptr, "ptr", p, "int", Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Subtract, 1L), "int", 4L);
					Keysharp.Core.Dll.CallbackFree(p);
					_ks_e1:
					;
				}
			}
			finally
			{
				Keysharp.Core.Loops.Pop();
			}
		}

		[GlobalSetup]
		public void Setup()
		{
			Scripting.Script.Variables.InitGlobalVars();
			ptr = MCode("2,x64:SInIidFEicJI/+A=");
		}
	}
}