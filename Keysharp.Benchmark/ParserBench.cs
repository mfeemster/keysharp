namespace Keysharp.Benchmark;

public class ParserBench : BaseTest
{
	private const string testScriptPath = @"../../../Keysharp.Tests/Code/Gui/guitest.ks";
	private CompilerHelper ch = new();

	[Benchmark]
	public void FullParse()
	{
		CompilerHelper tempCh = new();
		_ = tempCh.CreateCompilationUnitFromFile(testScriptPath);
	}

	[Benchmark(Baseline = true)]
	public void PartialParse() => _ = ch.CreateCompilationUnitFromFile(testScriptPath);

	[GlobalSetup]
	public void Setup() => ch = new();
}