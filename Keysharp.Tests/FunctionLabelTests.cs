using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	/// <summary>
	/// Function tests don't need to also be wrapped in a function, so pass false to all.
	/// </summary>
	public class FunctionAndLabelTests : TestRunner
	{
		[Test, Category("Function"), NonParallelizable]
		public void AllGlobalInFunc() => Assert.IsTrue(TestScript("func-all-global", false));

		[Test, Category("Function"), NonParallelizable]
		public void AllLocalInFunc() => Assert.IsTrue(TestScript("func-all-local", false));

		[Test, Category("Function"), NonParallelizable]
		public void BoundFunc() => Assert.IsTrue(TestScript("func-bound", false));

		[Test, Category("Function"), NonParallelizable]
		public void CombinedParamsInFunc() => Assert.IsTrue(TestScript("func-combined-params", false));

		[Test, Category("Function"), NonParallelizable]
		public void DefParamsInFunc() => Assert.IsTrue(TestScript("func-def-params", false));

		[Test, Category("Function"), NonParallelizable]
		public void DynVarsInFunc() => Assert.IsTrue(TestScript("func-dyn-vars", false));

		[Test, Category("Function"), NonParallelizable]
		public void FatArrowFunc() => Assert.IsTrue(TestScript("func-fat-arrow", false));

		[Test, Category("Function"), NonParallelizable]
		public void GlobalLocalInFunc() => Assert.IsTrue(TestScript("func-global-local", false));

		[Test, Category("Function"), NonParallelizable]
		public void GlobalLocalStaticInFunc() => Assert.IsTrue(TestScript("func-global-local-static", false));

		[Test, Category("Function"), NonParallelizable]
		public void GlobalStaticInFunc() => Assert.IsTrue(TestScript("func-global-static", false));

		[Test, Category("Function"), NonParallelizable]
		public void LabelInFunc() => Assert.IsTrue(TestScript("func-label", true));

		[Test, Category("Function"), NonParallelizable]
		public void LocalStaticInFunc() => Assert.IsTrue(TestScript("func-local-static", false));

		[Test, Category("Function"), NonParallelizable]
		public void OptParamsInFunc() => Assert.IsTrue(TestScript("func-opt-params", false));

		[Test, Category("Function"), NonParallelizable]
		public void ParamsInFunc() => Assert.IsTrue(TestScript("func-params", false));

		[Test, Category("Function"), NonParallelizable]
		public void RefParamsInFunc() => Assert.IsTrue(TestScript("func-ref-params", false));

		[Test, Category("Function"), NonParallelizable]
		public void ReturnFunc() => Assert.IsTrue(TestScript("func-return", false));

		[SetUp]
		public void Setup()
		{
			//For some reason, RefParamsInFunc() will not succeed on linux without these when run with all other tests in the group.
			//It runs fine on its own though. Add these two statements to make group testing succeed.
			Reflections.Clear();
			Reflections.Initialize();
		}

		[Test, Category("Function"), NonParallelizable]
		public void VarParamsInFunc() => Assert.IsTrue(TestScript("func-var-params", false));
	}
}