using static Keysharp.Scripting.Script;

[assembly: Keysharp.Scripting.AssemblyBuildVersionAttribute("0.0.0.1")]

namespace Keysharp.Benchmark
{
	using System.Collections.Generic;
	using BenchmarkDotNet.Attributes;
	using Keysharp.Core;

	[MemoryDiagnoser]
	public class MapReadBenchmark
	{
		private Dictionary<object, object> dkt = new Dictionary<object, object>();
		private Map map = Keysharp.Core.Misc.Map(), mapScript = Keysharp.Core.Misc.Map();
		private List<string> strings = new List<string>();

		[Params(10000)]
		public int Size { get; set; }

		[Benchmark]
		public void Map()
		{
			foreach (var s in strings)
				_ = map[s];
		}

		[Benchmark]
		public void MapScript()
		{
			foreach (var s in strings)
				_ = Index(mapScript, s);
		}

		[Benchmark(Baseline = true)]
		public void NativeDictionaryTryGet()
		{
			foreach (var s in strings)
				_ = dkt.TryGetValue(s, out _);
		}

		[GlobalSetup]
		public void Setup()
		{
			Keysharp.Scripting.Script.Variables.InitGlobalVars();
			map = Keysharp.Core.Misc.Map();
			mapScript = Keysharp.Core.Misc.Map();
			dkt = new Dictionary<object, object>();
			strings = new List<string>();

			for (var i = 0; i < Size; i++)
			{
				var s = i.ToString();
				map[s] = i;
				mapScript[s] = i;
				dkt[s] = i;
				strings.Add(s);
			}
		}
	}

	[MemoryDiagnoser]
	public class MapWriteBenchmark
	{
		private Dictionary<object, object> dkt = new Dictionary<object, object>();
		private Map map = Keysharp.Core.Misc.Map(), mapScript = Keysharp.Core.Misc.Map();
		private List<string> strings = new List<string>();

		[Params(10000)]
		public int Size { get; set; }

		public MapWriteBenchmark()
		{
			Keysharp.Scripting.Script.Variables.InitGlobalVars();
			map = Keysharp.Core.Misc.Map();
			mapScript = Keysharp.Core.Misc.Map();
			dkt = new Dictionary<object, object>();
			strings = new List<string>();
		}

		[Benchmark]
		public void Map()
		{
			map.Clear();

			for (var i = 0; i < Size; i++)
				map[strings[i]] = i;
		}

		[Benchmark]
		public void MapScript()
		{
			mapScript.Clear();

			for (var i = 0; i < Size; i++)
				_ = SetObject(strings[i], mapScript, i);
		}

		[Benchmark(Baseline = true)]
		public void NativeDictionary()
		{
			dkt.Clear();

			for (var i = 0; i < Size; i++)
				dkt[strings[i]] = i;
		}

		[GlobalSetup]
		public void Setup()
		{
			for (var i = 0; i < Size; i++)
				strings.Add(i.ToString());
		}
	}
}