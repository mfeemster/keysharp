namespace Keysharp.Benchmark
{
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
			//var summary = BenchmarkRunner.Run<IndexBench>(config);
			//var summary = BenchmarkRunner.Run<ListAddBench>(config);
			var summary = BenchmarkRunner.Run<NestedMapPopulatorBenchmark>(config);
			//var summary = BenchmarkRunner.Run<IndexBench>();
			//var summary = BenchmarkRunner.Run<FuncBench>(config);
			var logger = ConsoleLogger.Default;
			MarkdownExporter.Console.ExportToLog(summary, logger);
			ConclusionHelper.Print(logger, summary.BenchmarksCases.First().Config.GetCompositeAnalyser().Analyse(summary).ToList());
			summary = BenchmarkRunner.Run<NestedMapRetrieverBenchmark>(config);
			//var summary = BenchmarkRunner.Run<IndexBench>();
			//var summary = BenchmarkRunner.Run<FuncBench>(config);
			MarkdownExporter.Console.ExportToLog(summary, logger);
			ConclusionHelper.Print(logger, summary.BenchmarksCases.First().Config.GetCompositeAnalyser().Analyse(summary).ToList());
		}
	}
}