using static Keysharp.Core.Functions;

namespace Keysharp.Benchmark
{
	[MemoryDiagnoser]
	public class MathBench
	{
		private readonly Dictionary<object, object> dkt = [];
		private IFuncObj fo = Func("Cos");
		private readonly Map map = Collections.Map(), mapScript = Collections.Map();
		private List<object> objvals = [];
		private double totalCos;
		private List<double> vals = [];

		[Params(100000)]
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
		public void KeysharpCos()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += Maths.Cos(vals[i]);

			if (!total.IsAlmostEqual(totalCos))
				throw new Exception($"{total} was not equal to {totalCos}.");
		}

		//[Benchmark]
		//public void KeysharpCosFunc()
		//{
		//  var total = 0.0;

		//  for (var i = 0; i < Size; i++)
		//      total += (double)fo.Call(vals[i]);

		//  if (!total.IsAlmostEqual(totalCos))
		//      throw new Exception($"{total} was not equal to {totalCos}.");
		//}

		[Benchmark]
		public void KeysharpCosObj()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += Maths.Cos(objvals[i]);

			if (!total.IsAlmostEqual(totalCos))
				throw new Exception($"{total} was not equal to {totalCos}.");
		}

		/*
		            [Benchmark]
		            public void KeysharpCosDirectPass()
		            {
		            var total = 0.0;

		            for (var i = 0; i < Size; i++)
		                total += Maths.CosDirectPass(vals[i]);

		            if (!total.IsAlmostEqual(totalCos))
		                throw new Exception($"{total} was not equal to {totalCos}.");
		            }

		            [Benchmark]
		            public void KeysharpCosDirectPassObject()
		            {
		            var total = 0.0;

		            for (var i = 0; i < Size; i++)
		                total += Maths.CosDirectPassObject(vals[i]);

		            if (!total.IsAlmostEqual(totalCos))
		                throw new Exception($"{total} was not equal to {totalCos}.");
		            }
		*/

		[GlobalSetup]
		public void Setup()
		{
			Scripting.Script.Variables.InitGlobalVars();
			totalCos = 0;
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