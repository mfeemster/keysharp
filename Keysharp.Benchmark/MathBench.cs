using static Keysharp.Core.Function;

namespace Keysharp.Benchmark
{
	[MemoryDiagnoser]
	public class MathBench
	{
		private Dictionary<object, object> dkt = new Dictionary<object, object>();
		private IFuncObj fo = Func("Cos");
		private Map map = Keysharp.Core.Misc.Map(), mapScript = Keysharp.Core.Misc.Map();
		private List<object> objvals = new List<object>();
		private double totalCos;
		private List<double> vals = new List<double>();

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
			Keysharp.Scripting.Script.Variables.InitGlobalVars();
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