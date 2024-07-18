using static Keysharp.Core.Flow;
using static Keysharp.Core.Loops;
using static Keysharp.Scripting.Script;
using static Keysharp.Scripting.Script.Operator;

namespace Keysharp.Benchmark
{
	[MemoryDiagnoser]
	public class FuncBench
	{
		private myclass? cl;
		private long totalSum;

		[Params(1000000)]
		public static long Size { get; set; }

		public static object? x { get; set; }

		public static object IncFunc()
		{
			x = Operate(Add, x, 1L);
			return "";
		}

		[Benchmark]
		public void KeysharpClassFuncLoopIncrement()
		{
			cl.x = 0;
			_ = cl.ClassIncTestFunc();

			if ((long)cl.x != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpFuncLoopIncrement()
		{
			x = 0L;

			for (System.Collections.IEnumerator e3 = Loop(Size).GetEnumerator(); IsTrueAndRunning(e3.MoveNext());
				)
			{
				_ = IncFunc();
				e4:
				;
			}

			e5:
			_ = Pop();

			if ((long)x != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpLoopIncrement()
		{
			x = 0L;

			for (System.Collections.IEnumerator e0 = Loop(Size).GetEnumerator(); IsTrueAndRunning(e0.MoveNext());
				)
			{
				x = Operate(Add, x, 1L);
				e1:
				;
			}

			e2:
			_ = Pop();

			if ((long)x != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpNativeLongLoopIncrement()
		{
			var total = 0L;

			for (System.Collections.IEnumerator e3 = Loop(Size).GetEnumerator(); IsTrueAndRunning(e3.MoveNext());
				)
			{
				total++;
				e4:
				;
			}

			e5:
			_ = Pop();

			if (total != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpNativeObjectLoopIncrement()
		{
			x = 0L;

			for (System.Collections.IEnumerator e3 = Loop(Size).GetEnumerator(); IsTrueAndRunning(e3.MoveNext());
				)
			{
				x = (long)x + 1L;
				e4:
				;
			}

			e5:
			_ = Pop();

			if ((long)x != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark(Baseline = true)]
		public void NativeLoopIncrement()
		{
			var total = 0L;

			for (var i = 0; i < Size; i++)
				total++;

			if (total != totalSum)
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[GlobalSetup]
		public void Setup()
		{
			Keysharp.Scripting.Script.Variables.InitGlobalVars();
			Size = 1000000;
			totalSum = Size;
			cl = new myclass();
		}
	}

	[MemoryDiagnoser]
	public class myclass : KeysharpObject
	{
		public static string @__Class
		{
			get
			{
				return "myclass";
			}
		}

		public object? x { get; set; }

		public myclass()
		{
			__Init();
		}

		public static myclass Call()
		{
			return new myclass();
		}

		public object ClassInc()
		{
			x = Operate(Add, x, 1L);
			return "";
		}

		public object ClassIncTestFunc()
		{
			x = 0L;

			for (System.Collections.IEnumerator e8 = Loop(FuncBench.Size).GetEnumerator(); IsTrueAndRunning(e8.MoveNext());
				)
			{
				_ = Invoke(GetMethodOrProperty(this, "ClassInc", 0));
				e9:
				;
			}

			e10:
			_ = Pop();
			return "";
		}

		private void @__Init()
		{
			x = 0L;
		}
	}
}