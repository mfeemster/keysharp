using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.NET.HostModel.AppHost;
using Microsoft.Win32;

namespace Keysharp.Main
{
	public static class Program
	{
		private static readonly CompilerHelper ch = new CompilerHelper();
		private static readonly char dotNetMajorVersion = '8';

		internal static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		[STAThread]
		public static int Main(string[] args)
		{
			Task writeExeTask = null;
			Task writeCodeTask = null;

			try
			{
				if (args.Length == 0)
				{
					return Message("Invalid number of arguments: you must pass at least an input script filename as the first argument like so: Keysharp.exe myscript.ahk", true);
				}

				Keysharp.Core.Window.SetProcessDPIAware();
				var asm = Assembly.GetExecutingAssembly();
				var exePath = Path.GetFullPath(asm.Location);
				var exeDir = Path.GetFullPath(Path.GetDirectoryName(exePath));
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
							script = Path.GetFullPath(args[i]);
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
					var option = args[i].TrimStart(Keywords.DashSlash);
#else
					var option = args[i].TrimStart('-');
#endif
					var opt = option.ToLowerInvariant();

					switch (opt)
					{
						case "version":
						case "v":
							return Message($"{asm.GetName().Version}", false);

						case "about":
							var license = asm.GetManifestResourceStream(typeof(Program).Namespace + ".license.txt");
							return Message(new StreamReader(license).ReadToEnd(), false);

						case "exeout":
							exeout = true;
							break;

						case "codeout":
							codeout = true;
							break;

						case "install"://To be called by the installer during installation.
							InstallToPath(exeDir);
							return 0;

						case "uninstall"://To be called by the uninstaller during uninstallation.
							RemoveFromPath(exeDir);
							return 0;
							//default:
							//  return Message($"Unrecognized switch: {args[i]}", true);
					}
				}

				//Message($"Operating off of script: {script} in current dir: {Environment.CurrentDirectory} for full path: {Path.GetFullPath(script)}", false);

				if (string.IsNullOrEmpty(script))
				{
					var dirs = new string[]//Will need linux specific folders.//TODO
					{
						$"{Environment.CurrentDirectory}\\Keysharp.ahk",//Current executable dir.
						$"{Keysharp.Core.Accessors.A_MyDocuments}\\Keysharp.ahk",//Documents.
						$"{Environment.CurrentDirectory}\\Keysharp.ks",
						$"{Keysharp.Core.Accessors.A_MyDocuments}\\Keysharp.ks",
					};

					foreach (var dir in dirs)
					{
						if (System.IO.File.Exists(dir))
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
					return Message("No script was specified, no text was read from stdin, and no script named keysharp.ahk was found in the current folder or your documents folder.", true);

				if (!System.IO.File.Exists(script))
					return Message($"Could not find the script file {script}.", true);

				var (domunits, domerrs) = ch.CreateDomFromFile(script);
				string namenoext, path, scriptdir;

				if (!fromstdin)
				{
					namenoext = Path.GetFileNameWithoutExtension(script);
					scriptdir = Path.GetDirectoryName(script);
					path = $"{scriptdir}{Path.DirectorySeparatorChar}{namenoext}";
				}
				else
				{
					namenoext = "pipestdin";
					scriptdir = Environment.CurrentDirectory;
					path = $".{Path.DirectorySeparatorChar}{namenoext}";
				}

				if (domerrs.HasErrors)
					return HandleCompilerErrors(domerrs, script, path, "Compiling script to DOM");

				var (code, exc) = ch.CreateCodeFromDom(domunits);

				if (exc is Exception e)
					return Message($"Creating C# code from DOM: {e.Message}", true);

				code = CompilerHelper.UsingStr + code;//Need to manually add the using static statements.

				//If they want to write out the code, place it in the same folder as the script, with the same name, and .cs extension.
				if (codeout)
				{
					writeCodeTask = Task.Run(() =>
					{
						using (var sourceWriter = new StreamWriter(path + ".cs"))
						{
							sourceWriter.WriteLine(code);
						}
					});
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
				//Message($"Before compiling, setting current dir to {Environment.CurrentDirectory}", false);
				var (results, ms, compileexc) = ch.Compile(code, namenoext, exeDir);

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
						writeExeTask = Task.Run(() =>
						{
							try
							{
								var ver = GetLatestDotNetVersion();//Windows only.
								var outputRuntimeConfigPath = Path.ChangeExtension(path, "runtimeconfig.json");
								var currentRuntimeConfigPath = Path.ChangeExtension(exePath, "runtimeconfig.json");
								System.IO.File.WriteAllBytes(path + ".dll", arr);
								System.IO.File.Copy(currentRuntimeConfigPath, outputRuntimeConfigPath, true);
								//var outputDepsConfigPath = Path.ChangeExtension(path, "deps.json");
								//var currentDepsConfigPath = Path.ChangeExtension(loc, "deps.json");
								//File.Copy(currentDepsConfigPath, outputDepsConfigPath, true);
								//Message($"About to write executable to {path}.exe/dll.", false);
								HostWriter.CreateAppHost(
									appHostSourceFilePath: @$"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\{ver}\runtimes\win-x64\native\apphost.exe",
									appHostDestinationFilePath: $"{path}.exe",
									appBinaryFilePath: $"{path}.dll",
									windowsGraphicalUserInterface: true,
									assemblyToCopyResorcesFrom: $"{path}.dll");
								var ksCorePath = Path.Combine(exeDir, "Keysharp.Core.dll");
								//Need to copy Keysharp.Core from the install path to folder the script resides in. Without it, the compiled exe cannot be run in a standalone manner.
								//MessageBox.Show($"scriptdir = {scriptdir}");
								//MessageBox.Show($"About to copy from {ksCorePath} to {Path.Combine(scriptdir, "Keysharp.Core.dll")}");

								if (string.Compare(exeDir, scriptdir, true) != 0)
									if (System.IO.File.Exists(ksCorePath))
										System.IO.File.Copy(ksCorePath, Path.Combine(scriptdir, "Keysharp.Core.dll"), true);
							}
							catch (Exception writeex)
							{
								Message($"Writing executable to {path}.exe failed: {writeex.Message}", true);
							}
						});
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

				GC.Collect();
				GC.WaitForPendingFinalizers();

				if (CompilerHelper.compiledasm == null)
					throw new Exception("Compilation failed.");

				var program = CompilerHelper.compiledasm.GetType("Keysharp.CompiledMain.program");
				var main = program.GetMethod("Main");
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
				var trace = $"{Keysharp.Core.Accessors.A_AppData}/Keysharp/execution_errors.txt";

				try
				{
					if (System.IO.File.Exists(trace))
						System.IO.File.Delete(trace);

					System.IO.File.WriteAllText(trace, msg);
				}
				catch (Exception exx)
				{
					msg += $"\n\n{exx.Message}";
				}
				finally
				{
					writeExeTask?.Wait();
					writeCodeTask?.Wait();
				}

				return Message(msg, true);
			}

			writeExeTask?.Wait();
			writeCodeTask?.Wait();
			return 0;
		}

		internal static string GetLatestDotNetVersion()
		{
			//var key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
			//key = key.OpenSubKey(@"SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App");
			//var versions = key.GetValueNames().Select(v => new Version(v.Split("-")[0])).ToList();
			//versions.Sort();
			//return versions.Last().ToString();
			var dir = Directory.GetDirectories(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\").Select(System.IO.Path.GetFileName).Where(x => x.StartsWith(dotNetMajorVersion)).OrderByDescending(x => new Version(x)).FirstOrDefault();
			return dir;
		}

		internal static void InstallToPath(string path)
		{
			var keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
			var oldPath = (string)Registry.LocalMachine.CreateSubKey(keyName).GetValue("PATH", "", RegistryValueOptions.DoNotExpandEnvironmentNames);//Get non-expanded PATH environment variable.

			if (!oldPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Any(s => string.Compare(s, path, true) == 0))
				Registry.LocalMachine.CreateSubKey(keyName).SetValue("PATH", oldPath + (oldPath.EndsWith(';') ? path : $";{path}"), RegistryValueKind.ExpandString);//Set the path as an an expandable string with the passed in value included.
		}

		internal static void RemoveFromPath(string path)
		{
			var keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
			var oldPath = (string)Registry.LocalMachine.CreateSubKey(keyName).GetValue("PATH", "", RegistryValueOptions.DoNotExpandEnvironmentNames);//Get non-expanded PATH environment variable.
			var newPath = string.Join(';', oldPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Where(s => string.Compare(s, path, true) != 0));
			Registry.LocalMachine.CreateSubKey(keyName).SetValue("PATH", newPath, RegistryValueKind.ExpandString);//Restore the old path to what it was without the passed in value included.
		}

#if WINDOWS

		private static int HandleCompilerErrors(ImmutableArray<Diagnostic> diagnostics, string filename, string path, string desc, string message = "")
		{
			var errstr = CompilerHelper.HandleCompilerErrors(diagnostics, filename, desc, message);

			if (errstr != "")
			{
				System.IO.File.WriteAllText($"{Keysharp.Core.Accessors.A_AppData}/Keysharp/compiler_errors.txt", errstr);
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
				var sb = new StringBuilder(1024);
				_ = sb.AppendLine($"{desc} failed.");

				if (!string.IsNullOrEmpty(errors))
					_ = sb.Append(errors);

				if (!string.IsNullOrEmpty(warnings))
					_ = sb.Append(warnings);

				if (!string.IsNullOrEmpty(message))
					_ = sb.Append(message);

				var errstr = sb.ToString();
				System.IO.File.WriteAllText($"{Keysharp.Core.Accessors.A_AppData}/Keysharp/compiler_errors.txt", errstr);
				_ = Message(errstr, true);
			}
			else
			{
				try
				{
					System.IO.File.Delete($"{Keysharp.Core.Accessors.A_AppData}/Keysharp/compiler_errors.txt");
				}
				catch { }
			}

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
				_ = MessageBox.Show(text, "Keysharp", MessageBoxButtons.OK, MessageBoxIcon.Information);
				Keysharp.Scripting.Script.OutputDebug(text);
			}

			return error ? 1 : 0;
		}
	}
}