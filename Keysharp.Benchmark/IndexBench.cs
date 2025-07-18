﻿using static Keysharp.Scripting.Script;
using Array = Keysharp.Core.Array;

namespace Keysharp.Benchmark
{
	public class IndexBench : BaseTest
	{
		private dynamic dynamickeysharparray = new Array();
		private Array keysharparray = Collections.Array();
		private object[] nativearray = System.Array.Empty<object>();
		private double[] nativedoublearray = System.Array.Empty<double>();
		private double totalSum;
		private Keysharp.Scripting.Script? _ks_s;

		[Params(500000)]
		public int Size { get; set; }

		[Benchmark]
		public void KeysharpArrayIndexRead()
		{
			var total = 0.0;

			for (var i = 1; i <= Size; i++)
				total += (double)keysharparray[i];

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpArrayIndexEnumeratorRead()
		{
			var total = 0.0;
			var e0 = keysharparray;
			var e2 = Loops.MakeEnumerator(e0, 1);
			_ = Loops.Push();

			for (object? val = null
							   ; Flow.IsTrueAndRunning(e2.Call(ref val));
				)
			{
				_ = Keysharp.Core.Loops.Inc();
				total += (double)val;
				e3:
				;
			}

			e4:
			_ = Loops.Pop();

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpArrayIndexMethodRead()
		{
			var total = 0.0;

			for (long i = 1; i <= Size; i++)
				total += (double)Index(keysharparray, i);

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpDynamicArrayIndexRead()
		{
			var total = 0.0;

			for (var i = 1; i <= Size; i++)
				total += (double)dynamickeysharparray[i];

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void NativeDoubleArrayRead()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += nativedoublearray[i];

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark(Baseline = true)]
		public void NativeObjectArrayRead()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += (double)nativearray[i];

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public unsafe void NativeUnsafeDoubleArrayRead()
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
			_ks_s  = new ();
			totalSum = 0;
			Size = 1000000;
			nativearray = new object[Size];
			nativedoublearray = new double[Size];
			keysharparray = new Array();
			keysharparray.Capacity = Size;
			dynamickeysharparray = new Array();
			dynamickeysharparray.Capacity = Size;

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

	public class ListAddBench : BaseTest
	{
		private Keysharp.Core.Array keysharparray = Collections.Array();
		private List<object> nativelist = [];
		private readonly object o = 123L;
		private Keysharp.Scripting.Script? _ks_s;

		[Params(500000)]
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
				_ = Invoke(GetMethodOrProperty(keysharparray, "Add", 1), o);

			if (keysharparray.Count != Size)
				throw new Exception($"Native list size of {keysharparray.Count} was not equal to Size {Size}.");
		}

		[Benchmark]
		public void KeysharpArrayScriptAddWithPrealloc()
		{
			keysharparray.Clear();
			keysharparray.Capacity = Size;

			for (var i = 0; i < Size; i++)
				_ = Invoke(GetMethodOrProperty(keysharparray, "Add", 1), o);

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
			_ks_s = new ();
			Size = 500000;
			nativelist = [];
			keysharparray = Collections.Array();
		}
	}
}