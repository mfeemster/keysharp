using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public class TestRunner
	{
		private const string ext = ".ahk";
		protected string path = string.Format("..{0}..{0}..{0}Keysharp.Tests{0}Code{0}", Path.DirectorySeparatorChar);

		protected bool TestScript(string source, bool testfunc, bool exeout = false)
		{
			var scriptPath = string.Concat(path, source, ext);
			var b2 = true;
			var b1 = HasPassed(RunScript(scriptPath, source, true, exeout));

			if (testfunc && b1)
				b2 = HasPassed(RunScript(scriptPath, source + "_func", true, true, exeout));

			return b1 && b2;
		}

		protected bool HasPassed(string output)
		{
			if (string.IsNullOrEmpty(output))
				return false;

			const string pass = "pass";

			foreach (var remove in new[] { pass, " ", "\n" })
					output = output.Replace(remove, string.Empty);
			return output.Length == 0;
		}

		protected string WrapInFunc(string source)
		{
			var sb = new StringBuilder();
			_ = sb.AppendLine("TestFunc()");
			_ = sb.AppendLine("{");

			using (var sr = new StringReader(source))
			{
				string line;

				while ((line = sr.ReadLine()) != null)
					_ = sb.AppendLine("\t" + line);
			}

			_ = sb.AppendLine("}");
			_ = sb.AppendLine("testfunc()");//Deliberately change case to always make sure case insensitivity works.
			return sb.ToString();
		}

		protected string RunScript(string source, string name, bool execute, bool wrapinfunction, bool exeout) => RunScript(WrapInFunc(File.ReadAllText(source)), name, execute, exeout);

		protected void TestException(Action func)
		{
			var excthrown = false;

			try
			{
				func();
			}
			catch (Exception)
			{
				excthrown = true;
			}

			Assert.IsTrue(excthrown);
		}

		protected string RunScript(string source, string name, bool execute, bool exeout)
		{
			Keysharp.Scripting.Script.OutputDebug(Environment.CurrentDirectory);
			var ch = new CompilerHelper();
			var (domunits, domerrs) = ch.CreateDomFromFile(source);

			if (domerrs.HasErrors)
			{
				foreach (CompilerError err in domerrs)
					Keysharp.Scripting.Script.OutputDebug(err.ErrorText);

				return string.Empty;
			}

			var (code, exc) = ch.CreateCodeFromDom(domunits);

			if (exc is Exception e)
			{
				Keysharp.Scripting.Script.OutputDebug(e.Message);
				return string.Empty;
			}

			code = CompilerHelper.UsingStr + code;

			using (var sourceWriter = new StreamWriter("./" + name + ".cs"))
			{
				sourceWriter.WriteLine(code);
			}

			var asm = Assembly.GetExecutingAssembly();
			var (results, ms, compileexc) = ch.Compile(code, name, Path.GetFullPath(Path.GetDirectoryName(asm.Location)));

			if (compileexc != null)
			{
				Keysharp.Scripting.Script.OutputDebug(compileexc.Message);
				return string.Empty;
			}
			else if (results == null)
			{
				return string.Empty;
			}
			else if (results.Success)
			{
				ms.Seek(0, SeekOrigin.Begin);
				var arr = ms.ToArray();

				if (exeout)
				{
					File.WriteAllBytes("./" + name + ".exe", arr);
					File.WriteAllText("./" + name + ".runtimeconfig.json", CompilerHelper.GenerateRuntimeConfig());//Probably not needed for test exe outputs.
				}

				CompilerHelper.compiledasm = Assembly.Load(arr);
			}
			else
			{
				return string.Empty;
			}

			var buffer = new StringBuilder();
			var output = string.Empty;

			if (execute)
			{
				using (var writer = new StringWriter(buffer))
				{
					try
					{
						Console.SetOut(writer);
						GC.Collect();
						GC.WaitForPendingFinalizers();

						if (CompilerHelper.compiledasm == null)
							throw new Exception("Compilation failed.");

						//Environment.SetEnvironmentVariable("SCRIPT", script);
						var program = CompilerHelper.compiledasm.GetType("Keysharp.CompiledMain.program");
						var main = program.GetMethod("Main");
						var temp = new string[] { };
						var result = main.Invoke(null, new object[] { temp });

						if (result is int i && i != 0)//This is for when an exception is thrown protectedly in the compiled program, the catch blocks make it return 1.
							Console.Write("fail");
					}
					catch (Exception ex)
					{
						if (ex is TargetInvocationException)
							ex = ex.InnerException;

						var error = new StringBuilder();
						_ = error.AppendLine("Execution error:\n");
						_ = error.AppendLine($"{ex.GetType().Name}: {ex.Message}");
						_ = error.AppendLine();
						_ = error.AppendLine(ex.StackTrace);
						var msg = error.ToString();
						Keysharp.Scripting.Script.OutputDebug(msg);
						Console.Write("fail");
						Assert.IsTrue(false);
					}
					finally
					{
						writer.Flush();
						output = buffer.ToString();

						using (var console = Console.OpenStandardOutput())
						{
							var stdout = new StreamWriter(console);
							stdout.AutoFlush = true;
							Console.SetOut(stdout);
						}
					}
				}
			}

			return output;
		}
	}
}
