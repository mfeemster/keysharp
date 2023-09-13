using static Keysharp.Core.Accessors;
using static Keysharp.Core.Core;
using static Keysharp.Core.Common.Keyboard.HotstringDefinition;
using static Keysharp.Core.Dialogs;
using static Keysharp.Core.Drive;
using static Keysharp.Core.Dir;
using static Keysharp.Core.DllHelper;
using static Keysharp.Core.Env;
using static Keysharp.Core.KeysharpFile;
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

namespace Keysharp.Benchmark
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

	public sealed class Program
	{
		[System.STAThreadAttribute()]
		public static void Main(string[] args)
		{
			//var ib = new IndexBench();
			//ib.Setup();
			//ib.NativeArray();
			//ib.KeysharpArray();
			//ib.KeysharpArrayIndex();
			//ib.KeysharpArrayIndexNoDefaultIndexer();
			//return;
			//var fb = new FuncBench();
			//fb.Setup();
			//fb.KeysharpNativeLongSimNativeLoopIncrement();
			//return;
			var config = new ManualConfig();
			_ = config.AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
			_ = config.AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
			_ = config.AddDiagnoser(DefaultConfig.Instance.GetDiagnosers().ToArray());
			_ = config.AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray());
			_ = config.AddJob(DefaultConfig.Instance.GetJobs().ToArray());
			_ = config.AddValidator(DefaultConfig.Instance.GetValidators().ToArray());
			config.UnionRule = ConfigUnionRule.AlwaysUseGlobal; // Overriding the default
			//var summary = BenchmarkRunner.Run<MapWriteBenchmark>(config);
			//var summary = BenchmarkRunner.Run<MathBench>(config);
			var summary = BenchmarkRunner.Run<IndexBench>(config);
			//var summary = BenchmarkRunner.Run<IndexBench>();
			//var summary = BenchmarkRunner.Run<FuncBench>(config);
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