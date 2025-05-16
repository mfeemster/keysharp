using BenchmarkDotNet.Order;
using static Keysharp.Core.Functions;

namespace Keysharp.Benchmark
{
	public class MathBench : BaseTest
	{
		private double totalCos;
		private IFuncObj fo = Func("Cos");
		private List<double> vals = [];
		private List<object> objvals = [];
		private Keysharp.Scripting.Script _ks_s;

		[Params(500000)]
		public int Size { get; set; }

		[Benchmark(Baseline = true)]
		public void CosNative()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += Math.Cos(vals[i]);

			if (!total.IsAlmostEqual(totalCos))
				throw new Exception($"{total} was not equal to {totalCos}.");
		}

		[Benchmark]
		public void KeysharpCosDouble()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += Maths.Cos(vals[i]);

			if (!total.IsAlmostEqual(totalCos))
				throw new Exception($"{total} was not equal to {totalCos}.");
		}

		[Benchmark]
		public void KeysharpCosFuncDouble()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += (double)fo.Call(vals[i]);

			if (!total.IsAlmostEqual(totalCos))
				throw new Exception($"{total} was not equal to {totalCos}.");
		}

		[Benchmark]
		public void KeysharpCosObj()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += Maths.Cos(objvals[i]);

			if (!total.IsAlmostEqual(totalCos))
				throw new Exception($"{total} was not equal to {totalCos}.");
		}

		[GlobalSetup]
		public void Setup()
		{
			_ks_s  = new ();
			totalCos = 0.0;
			vals = new List<double>(Size);
			objvals = new List<object>(Size);
			fo = Func("Cos");

			for (var i = 0; i < Size; i++)
			{
				var val = Maths.Random();
				var d = val.Ad();
				vals.Add(d);
				objvals.Add(val);
				totalCos += Math.Cos(d);
			}
		}
	}
}