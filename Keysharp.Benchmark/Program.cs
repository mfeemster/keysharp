using static Keysharp.Core.Accessors;
using static Keysharp.Core.Core;
using static Keysharp.Core.Common.Keyboard.HotstringDefinition;
using static Keysharp.Core.Dialogs;
using static Keysharp.Core.Disk;
using static Keysharp.Core.DllHelper;
using static Keysharp.Core.Env;
using static Keysharp.Core.File;
using static Keysharp.Core.Flow;
using static Keysharp.Core.Function;
using static Keysharp.Core.GuiHelper;
using static Keysharp.Core.Images;
using static Keysharp.Core.ImageLists;
using static Keysharp.Core.Ini;
using static Keysharp.Core.Keyboard;
using static Keysharp.Core.KeysharpObject;
using static Keysharp.Core.Loops;
using static Keysharp.Core.Maths;
using static Keysharp.Core.Menu;
using static Keysharp.Core.Misc;
using static Keysharp.Core.Monitor;
using static Keysharp.Core.Mouse;
using static Keysharp.Core.Network;
using static Keysharp.Core.Options;
using static Keysharp.Core.Processes;
using static Keysharp.Core.Registrys;
using static Keysharp.Core.Screen;
using static Keysharp.Core.Security;
using static Keysharp.Core.SimpleJson;
using static Keysharp.Core.Sound;
using static Keysharp.Core.Strings;
using static Keysharp.Core.ToolTips;
using static Keysharp.Core.Window;
using static Keysharp.Core.Windows.WindowsAPI;
using static Keysharp.Scripting.Script;
using static Keysharp.Scripting.Script.Operator;

[assembly: System.CLSCompliantAttribute(true)]
[assembly: Keysharp.Scripting.AssemblyBuildVersionAttribute("0.0.0.1")]

namespace Keysharp.Main
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Windows.Forms;
	using BenchmarkDotNet.Analysers;
	using BenchmarkDotNet.Attributes;
	using BenchmarkDotNet.Configs;
	using BenchmarkDotNet.Exporters;
	using BenchmarkDotNet.Loggers;
	using BenchmarkDotNet.Running;
	using Keysharp.Core;
	using Keysharp.Scripting;
	using Array = Keysharp.Core.Array;
	using Buffer = Keysharp.Core.Buffer;

	[MemoryDiagnoser]
	public class MapReadBenchmark
	{
		private Dictionary<object, object> dkt;
		private Map map, mapScript;
		private List<string> strings;

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
				dkt.TryGetValue(s, out _);
		}

		[GlobalSetup]
		public void Setup()
		{
			map = Keysharp.Scripting.Script.Map();
			mapScript = Keysharp.Scripting.Script.Map();
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
		private Dictionary<object, object> dkt;
		private Map map, mapScript;
		private List<string> strings;

		[Params(10000)]
		public int Size { get; set; }

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
				_ = SetObject(strings[i], mapScript, System.Array.Empty<object>(), i);
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
			map = Keysharp.Scripting.Script.Map();
			mapScript = Keysharp.Scripting.Script.Map();
			dkt = new Dictionary<object, object>();
			strings = new List<string>();

			for (var i = 0; i < Size; i++)
				strings.Add(i.ToString());
		}
	}

	public sealed class Program
	{
		[System.STAThreadAttribute()]
		public static void Main(string[] args)
		{
			var config = new ManualConfig();
			config.AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
			config.AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
			config.AddDiagnoser(DefaultConfig.Instance.GetDiagnosers().ToArray());
			config.AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray());
			config.AddJob(DefaultConfig.Instance.GetJobs().ToArray());
			config.AddValidator(DefaultConfig.Instance.GetValidators().ToArray());
			config.UnionRule = ConfigUnionRule.AlwaysUseGlobal; // Overriding the default
			var summary = BenchmarkRunner.Run<MapWriteBenchmark>(config);
			var logger = ConsoleLogger.Default;
			MarkdownExporter.Console.ExportToLog(summary, logger);
			ConclusionHelper.Print(logger, summary.BenchmarksCases.First().Config.GetCompositeAnalyser().Analyse(summary).ToList());
			//BenchmarkRunner.Run<MapReadBenchmark>();
			//BenchmarkRunner.Run<MapWriteBenchmark>();
			//var bm = new ByteArrayToHexaBenchmark();
			//bm.Size = 100;
			//bm.Setup();
			//var str = bm.ToHexWithByteManipulation(bm._array);
			//Console.WriteLine(str);
		}
	}
}