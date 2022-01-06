using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Windows.Forms;
using Keysharp.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.NET.HostModel.AppHost;

namespace Keysharp.Main
{
	public static class Program
	{
		private static CompilerHelper ch = new CompilerHelper();
		internal static Version Version
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		[STAThread]
		public static int Main(string[] args)
		{
			if (args.Length == 0)
			{
				return Message("Invalid number of arguments: you must pass at least an input script filename as the first argument like so: Keysharp.exe myscript.ahk", true);
			}

			Keysharp.Core.Window.SetProcessDPIAware();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var asm = Assembly.GetExecutingAssembly();
			Environment.CurrentDirectory = Path.GetFullPath(Path.GetDirectoryName(asm.Location));
			var nsname = typeof(Program).Namespace;
			var codeout = false;
			var exeout = false;
			var script = string.Empty;
			var gotscript = false;
			var fromstdin = false;

			for (var i = 0; i < args.Length; i++)
			{
				if (!args[i].StartsWith('-')
#if WINDOWS
						&& !args[i].StartsWith('/')
#endif
				   )
				{
					if (!gotscript)//Script name.
					{
						script = args[i];
						gotscript = true;
						continue;
					}
					else//Parameters.
					{
						Keysharp.Core.Accessors.A_Args.Add(args[i]);
						continue;
					}
				}

#if WINDOWS
				var option = args[i].TrimStart(Keysharp.Core.Core.DashSlash);
#else
				var option = args[i].TrimStart('-');
#endif

				switch (option.ToLowerInvariant())
				{
					case "version":
					case "v":
						return Message($"{nsname} {asm.GetName().Version}", false);

					case "about":
						var license = asm.GetManifestResourceStream(typeof(Program).Namespace + ".license.txt");
						return Message(new StreamReader(license).ReadToEnd(), false);
				}

				var opt = option.ToLowerInvariant();

				switch (opt)
				{
					case "exeout":
						exeout = true;
						break;

					case "codeout":
						codeout = true;
						break;
						//default:
						//  return Message($"Unrecognized switch: {args[i]}", true);
				}
			}

			if (string.IsNullOrEmpty(script))
			{
				var dirs = new string[]//Will need linux specific folder.//MATT
				{
					$"{Environment.CurrentDirectory}\\Keysharp.ahk",//Current executable dir.
					$"{Keysharp.Core.Accessors.A_MyDocuments}\\Keysharp.ahk",//Documents.
				};

				foreach (var dir in dirs)
				{
					if (File.Exists(dir))
					{
						script = dir;
						break;
					}
				}
			}

			if (script == "*")
			{
				fromstdin = true;
				string s;
				var sb = new StringBuilder(2048);

				while ((s = Console.ReadLine()) != null)
					sb.AppendLine(s);

				script = sb.ToString();
			}

			if (string.IsNullOrEmpty(script))
			{
				return Message("No script was specified, no text was read from stdin, and no script named keysharp.ahk was found in the current folder or your documents folder.", true);
			}

			var (domunits, domerrs) = ch.CreateDomFromFile(script);
			string namenoext, path;

			if (!fromstdin)
			{
				namenoext = Path.GetFileNameWithoutExtension(script);
				path = $"{Path.GetDirectoryName(Path.GetFullPath(script))}{Path.DirectorySeparatorChar}{namenoext}";
			}
			else
			{
				namenoext = "pipestdin";
				path = $".{Path.DirectorySeparatorChar}{namenoext}";
			}

			if (domerrs.HasErrors)
			{
				return HandleCompilerErrors(domerrs, script, path, "Compiling script to DOM");
			}

			var (code, exc) = ch.CreateCodeFromDom(domunits);

			if (exc is Exception e)
			{
				return Message($"Creating C# code from DOM: {e.Message}", true);
			}

			code = Keysharp.Scripting.Parser.TrimParens(code);
			code = CompilerHelper.UsingStr + code;//Need to manually add the using static statements.

			//If they want to write out the code, place it in the same folder as the script, with the same name, and .cs extension.
			if (codeout)
			{
				using (var sourceWriter = new StreamWriter(path + ".cs"))
				{
					sourceWriter.WriteLine(code);
				}
			}

			//If they want to write out the code, place it in the same folder as the script, with the same name, and .exe extension.
#if !WINDOWS
			var (results, compileexc) = ch.Compile(code, exeout ? path + ".exe" : string.Empty);

			if (results == null)
			{
				return Message($"Compiling C# code to executable: {(compileexc != null ? compileexc.Message : string.Empty)}", true);
			}
			else if (results.Errors.HasErrors)
			{
				return HandleCompilerErrors(results.Errors, script, path, "Compiling C# code to executable", compileexc != null ? compileexc.Message : string.Empty);
			}

			CompilerHelper.compiledasm = results.CompiledAssembly;
#else
			var (results, ms, compileexc) = ch.Compile(code, namenoext);

			if (results == null)
			{
				return Message($"Compiling C# code to executable: {(compileexc != null ? compileexc.Message : string.Empty)}", true);
			}
			else if (results.Success)
			{
				ms.Seek(0, SeekOrigin.Begin);
				var arr = ms.ToArray();

				if (exeout)
				{
					//File.WriteAllBytes(path + ".exe", arr);
					File.WriteAllBytes(path + ".dll", arr);
					var outputRuntimeConfigPath = Path.ChangeExtension(path, "runtimeconfig.json");
					var currentRuntimeConfigPath = Path.ChangeExtension(typeof(Program).Assembly.Location, "runtimeconfig.json");
					File.Copy(currentRuntimeConfigPath, outputRuntimeConfigPath, true);
					var outputDepsConfigPath = Path.ChangeExtension(path, "deps.json");
					var currentDepsConfigPath = Path.ChangeExtension(typeof(Program).Assembly.Location, "deps.json");
					File.Copy(currentDepsConfigPath, outputDepsConfigPath, true);
					//File.WriteAllText(Path.ChangeExtension(path, "runtimeconfig.json"), CompilerHelper.GenerateRuntimeConfig());
					HostWriter.CreateAppHost(
						appHostSourceFilePath: @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\5.0.11\runtimes\win-x64\native\apphost.exe",
						appHostDestinationFilePath: $"{path}.exe",
						appBinaryFilePath: $"{path}.dll");
				}

				CompilerHelper.compiledasm = Assembly.Load(arr);
			}
			else
			{
				return HandleCompilerErrors(results.Diagnostics, script, path, "Compiling C# code to executable", compileexc != null ? compileexc.Message : string.Empty);
			}

#endif

			if (Keysharp.Core.Env.FindCommandLineArg("validate") != null)
				return 0;//Any other error condition returned 1 already.

			try
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();

				if (CompilerHelper.compiledasm == null)
					throw new Exception("Compilation failed.");

				//Environment.SetEnvironmentVariable("SCRIPT", script);
				var program = CompilerHelper.compiledasm.GetType("Keysharp.Main.Program");
				var main = program.GetMethod("Main");
				var temp = Array.Empty<string>();
				//_ = main.Invoke(null, new object[] { temp });
				_ = main.Invoke(null, new object[] { args });
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
				var trace = $"{path}_execution_errors.txt";

				try
				{
					if (!string.IsNullOrEmpty(trace))
					{
						if (File.Exists(trace))
							File.Delete(trace);

						File.WriteAllText(trace, msg);
					}
				}
				catch (Exception exx)
				{
					msg += $"\n\n{exx.Message}";
				}
				finally
				{
				}

				return Message(msg, true);
			}

			return 0;
		}

#if WINDOWS
		private static int HandleCompilerErrors(ImmutableArray<Diagnostic> diagnostics, string filename, string path, string desc, string message = "")
		{
			var errstr = CompilerHelper.HandleCompilerErrors(diagnostics, filename, desc, message);

			if (errstr != "")
			{
				File.WriteAllText($"{path}_compiler_errors.txt", errstr);
				_ = Message(errstr, true);
				return 1;
			}

			return 0;
		}
#endif

		private static int HandleCompilerErrors(CompilerErrorCollection results, string filename, string path, string desc, string message = "")
		{
			var (errors, warnings) = CompilerHelper.GetCompilerErrors(results, filename);
			var failed = errors != "";

			if (failed)
			{
				var errstr = $"{desc} failed:\n\n{errors}\n\n\n{warnings}\n\n{message}";
				File.WriteAllText($"{path}_compiler_errors.txt", errstr);
				_ = Message(errstr, true);
			}
			else
				File.Delete($"{path}_compiler_errors.txt");

			return failed ? 1 : 0;
		}

		private static int Message(string text, bool error)
		{
			if (error)
			{
				ch.PrintCompilerErrors(text);
			}
			else
			{
				_ = MessageBox.Show(text, typeof(Program).Namespace, MessageBoxButtons.OK, MessageBoxIcon.Information);
				Console.Out.WriteLine(text);
			}

			return error ? 1 : 0;
		}
	}
}