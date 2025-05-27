using static Keysharp.Core.Flow;
using static Keysharp.Core.Loops;
using static Keysharp.Scripting.Script;
using static Keysharp.Scripting.Script.Operator;

namespace Keysharp.Benchmark
{
	public class FuncBench : BaseTest
	{
		private myclass cll = new ();
		private long totalSum;
		private Keysharp.Scripting.Script? _ks_s;

		[Params(500000L)]
		public long Size { get; set; }

		public object? x { get; set; }

		public object IncFunc()
		{
			x = Operate(Add, x, 1L);
			return "";
		}

		[Benchmark]
		public void KeysharpClassFuncLoopIncrement()
		{
			cl.x = 0;
			_ = cl.ClassIncTestFuncScript();

			if ((long)cl.x != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpFuncLoopIncrement()
		{
			x = 0L;
			_ = Push(Keysharp.Core.LoopType.Normal);

			for (System.Collections.IEnumerator e0 = Loop(Size).GetEnumerator();
					IsTrueAndRunning(e0.MoveNext());
				)
			{
				_ = IncFunc();
				e1:
				;
			}

			e2:
			_ = Pop();

			if ((long)x != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpLoopIncrement()
		{
			x = 0L;
			_ = Push(Keysharp.Core.LoopType.Normal);

			for (System.Collections.IEnumerator e0 = Loop(Size).GetEnumerator();
					IsTrueAndRunning(e0.MoveNext());
				)
			{
				{
					x = Operate(Add, x, 1L);
					e1:
					;
				}
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
			_ = Push(Keysharp.Core.LoopType.Normal);

			for (System.Collections.IEnumerator e0 = Loop(Size).GetEnumerator();
					IsTrueAndRunning(e0.MoveNext());
				)
			{
				total++;
				e1:
				;
			}

			e2:
			_ = Pop();

			if (total != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpNativeObjectLoopIncrement()
		{
			x = 0L;
			_ = Push(Keysharp.Core.LoopType.Normal);

			for (System.Collections.IEnumerator e0 = Loop(Size).GetEnumerator();
					IsTrueAndRunning(e0.MoveNext());
				)
			{
				x = (long)x + 1L;
				e1:
				;
			}

			e2:
			_ = Pop();

			if ((long)x != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark(Baseline = true)]
		public void NativeLoopIncrement()
		{
			var total = 0L;

			for (var i = 0L; i < Size; i++)
				total++;

			if (total != totalSum)
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[GlobalSetup]
		public void Setup()
		{
			_ks_s = new ();
			Size = 500000L;
			totalSum = Size;
			cl = new myclass();
		}
	}

	//[MemoryDiagnoser]
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
			x = 0L;
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

		public object ClassIncTestFuncScript()
		{
			var size = 500000L;
			x = 0L;
			_ = Push(Keysharp.Core.LoopType.Normal);

			for (System.Collections.IEnumerator e0 = Loop(size).GetEnumerator();
					IsTrueAndRunning(e0.MoveNext());
				)
			{
				_ = Invoke(GetMethodOrProperty(this, "ClassInc", 0));
				e1:
				;
			}

			e2:
			_ = Pop();
			return "";
		}
	}
}