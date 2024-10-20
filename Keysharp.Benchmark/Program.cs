
using System.Security.Cryptography;

namespace Keysharp.Benchmark
{
	public sealed class Program
	{
		[System.STAThreadAttribute()]
		//public static void Main(string[] args)
		//{
		//  //var ib = new IndexBench();
		//  //ib.Setup();
		//  //ib.NativeArray();
		//  //ib.KeysharpArray();
		//  //ib.KeysharpArrayIndex();
		//  //ib.KeysharpArrayIndexNoDefaultIndexer();
		//  //return;
		//  //var fb = new FuncBench();
		//  //fb.Setup();
		//  //fb.KeysharpNativeLongSimNativeLoopIncrement();
		//  //return;
		//  var config = new ManualConfig();
		//  _ = config.AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
		//  _ = config.AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
		//  _ = config.AddDiagnoser(DefaultConfig.Instance.GetDiagnosers().ToArray());
		//  _ = config.AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray());
		//  _ = config.AddJob(DefaultConfig.Instance.GetJobs().ToArray());
		//  _ = config.AddValidator(DefaultConfig.Instance.GetValidators().ToArray());
		//  config.UnionRule = ConfigUnionRule.AlwaysUseGlobal; // Overriding the default
		//  //var summary = BenchmarkRunner.Run<MapWriteBenchmark>(config);
		//  //var summary = BenchmarkRunner.Run<MathBench>(config);
		//  //var summary = BenchmarkRunner.Run<IndexBench>(config);
		//  var summary = BenchmarkRunner.Run<ListAddBench>(config);
		//  //var summary = BenchmarkRunner.Run<NestedMapPopulatorBenchmark>(config);
		//  //var summary = BenchmarkRunner.Run<HexBench>(config);
		//  //var summary = BenchmarkRunner.Run<HexBench>();
		//  //var summary = BenchmarkRunner.Run<IndexBench>();
		//  //var summary = BenchmarkRunner.Run<FuncBench>(config);
		//  var logger = ConsoleLogger.Default;
		//  MarkdownExporter.Console.ExportToLog(summary, logger);
		//  ConclusionHelper.Print(logger, summary.BenchmarksCases.First().Config.GetCompositeAnalyser().Analyse(summary).ToList());
		//  //summary = BenchmarkRunner.Run<NestedMapRetrieverBenchmark>(config);
		//  //var summary = BenchmarkRunner.Run<IndexBench>();
		//  //var summary = BenchmarkRunner.Run<FuncBench>(config);
		//  MarkdownExporter.Console.ExportToLog(summary, logger);
		//  ConclusionHelper.Print(logger, summary.BenchmarksCases.First().Config.GetCompositeAnalyser().Analyse(summary).ToList());
		//}
		public static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<Md5VsSha256>();
		}
	}

	public class Md5VsSha256
	{
		private const int N = 10000;
		private readonly byte[] data;

		private readonly SHA256 sha256 = SHA256.Create();
		private readonly MD5 md5 = MD5.Create();

		public Md5VsSha256()
		{
			data = new byte[N];
			new Random(42).NextBytes(data);
		}

		[Benchmark]
		public byte[] Sha256() => sha256.ComputeHash(data);

		[Benchmark]
		public byte[] Md5() => md5.ComputeHash(data);
	}

}