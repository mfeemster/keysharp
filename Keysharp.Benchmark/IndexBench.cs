using static Keysharp.Core.Loops;
using static Keysharp.Scripting.Script;
using Array = Keysharp.Core.Array;

namespace Keysharp.Benchmark
{
	[MemoryDiagnoser]
	public class IndexBench
	{
		private dynamic? dynamickeysharparray;
		private Array keysharparray = Keysharp.Core.Misc.Array();
		private object[] nativearray = System.Array.Empty<object>();
		private double[] nativedoublearray = System.Array.Empty<double>();
		private double totalSum;

		[Params(1000000)]
		public int Size { get; set; }

		[Benchmark]
		public void KeysharpArray()
		{
			var total = 0.0;

			for (var i = 1; i <= Size; i++)
				total += (double)keysharparray[i];

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpArrayIndexEnumerator()
		{
			var total = 0.0;
			var e0 = keysharparray;
			var e2 = MakeEnumerator(e0);
			_ = Push();

			for (
				; e2.MoveNext();
			)
			{
				_ = Inc();
				var (_, val) = e2.Current;
				total += (double)val;
				//e3:
				//;
			}

			//e4:
			_ = Pop();

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpArrayIndexMethod()
		{
			var total = 0.0;

			for (long i = 1; i <= Size; i++)
				total += (double)Index(keysharparray, i);

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpDynamicArray()
		{
			var total = 0.0;

			for (var i = 1; i <= Size; i++)
				total += (double)dynamickeysharparray[i];

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void NativeDoubleArray()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += nativedoublearray[i];

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark(Baseline = true)]
		public void NativeObjectArray()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += (double)nativearray[i];

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public unsafe void NativeUnsafeDoubleArray()
		{
			var total = 0.0;

			fixed (double* ptr = nativedoublearray)
			{
				for (var i = 0; i < Size; i++)
					total += ptr[i];
			}

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[GlobalSetup]
		public void Setup()
		{
			Keysharp.Scripting.Script.Variables.InitGlobalVars();
			totalSum = 0;
			Size = 1000000;
			nativearray = new object[Size];
			nativedoublearray = new double[Size];
			keysharparray = new Array(Size);
			dynamickeysharparray = new Array(Size);

			for (var i = 0; i < Size; i++)
			{
				var val = Maths.Random();
				var d = val.Ad();
				nativearray[i] = d;
				nativedoublearray[i] = d;
				_ = keysharparray.Add(d);
				_ = dynamickeysharparray.Add(d);
				totalSum += d;
			}
		}
	}

	[MemoryDiagnoser]
	public class ListAddBench
	{
		private Keysharp.Core.Array keysharparray = Keysharp.Core.Misc.Array();
		private List<object> nativelist = new List<object>();
		private object o = 123L;

		[Params(1000000)]
		public int Size { get; set; }

		[Benchmark]
		public void KeysharpArrayDirectAdd()
		{
			keysharparray.Clear();

			for (var i = 0; i < Size; i++)
				_ = keysharparray.Add(o);

			if (keysharparray.Count != Size)
				throw new Exception($"Native list size of {keysharparray.Count} was not equal to Size {Size}.");
		}

		[Benchmark]
		public void KeysharpArrayDirectAddWithPrealloc()
		{
			keysharparray.Clear();
			keysharparray.Capacity = Size;

			for (var i = 0; i < Size; i++)
				_ = keysharparray.Add(o);

			if (keysharparray.Count != Size)
				throw new Exception($"Native list size of {keysharparray.Count} was not equal to Size {Size}.");
		}

		[Benchmark]
		public void KeysharpArrayScriptAdd()
		{
			keysharparray.Clear();

			for (var i = 0; i < Size; i++)
				_ = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetMethodOrProperty(keysharparray, "Add", 1), o);

			if (keysharparray.Count != Size)
				throw new Exception($"Native list size of {keysharparray.Count} was not equal to Size {Size}.");
		}

		[Benchmark]
		public void KeysharpArrayScriptAddWithPrealloc()
		{
			keysharparray.Clear();
			keysharparray.Capacity = Size;

			for (var i = 0; i < Size; i++)
				_ = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetMethodOrProperty(keysharparray, "Add", 1), o);

			if (keysharparray.Count != Size)
				throw new Exception($"Native list size of {keysharparray.Count} was not equal to Size {Size}.");
		}

		[Benchmark(Baseline = true)]
		public void NativeListAdd()
		{
			nativelist.Clear();

			for (var i = 0; i < Size; i++)
				nativelist.Add(o);

			if (nativelist.Count != Size)
				throw new Exception($"Native list size of {nativelist.Count} was not equal to Size {Size}.");
		}

		[Benchmark]
		public void NativeListAddWithPrealloc()
		{
			nativelist.Clear();
			nativelist.Capacity = Size;

			for (var i = 0; i < Size; i++)
				nativelist.Add(o);

			if (nativelist.Count != Size)
				throw new Exception($"Native list size of {nativelist.Count} was not equal to Size {Size}.");
		}

		[GlobalSetup]
		public void Setup()
		{
			Keysharp.Scripting.Script.Variables.InitGlobalVars();
			Size = 1000000;
			nativelist = new List<object>();
			keysharparray = Keysharp.Core.Misc.Array();
		}
	}
}