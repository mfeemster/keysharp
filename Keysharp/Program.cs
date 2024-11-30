using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.NET.HostModel.AppHost;
using Microsoft.Win32;
using Keysharp.Core;
using Keysharp.Scripting;
using System.Globalization;

namespace Keysharp.Main
{
	public static class Program
	{
		private static readonly CompilerHelper ch = new CompilerHelper();
		private static readonly char dotNetMajorVersion = '9';

		internal static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		[STAThread]
		public static int Main(string[] args)
		{
			Task writeExeTask = null;
			Task writeCodeTask = null;

			try
			{
				Window.SetProcessDPIAware();
				CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
				CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
				var asm = Assembly.GetExecutingAssembly();
				var exePath = Path.GetFullPath(asm.Location);
				var exeName = Path.GetFileNameWithoutExtension(exePath);
				var exeDir = Path.GetFullPath(Path.GetDirectoryName(exePath));
				var nsname = typeof(Program).Namespace;
				var codeout = false;
				var exeout = false;
				var assembly = false;
				var assemblyType = "Keysharp.CompiledMain.program";
				var assemblyMethod = "Main";
				var script = string.Empty;
				var gotscript = false;
				var fromstdin = false;
				var validate = false;
				string[] scriptArgs = [];

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
							script = args[i] == "*" ? "*" : Path.GetFullPath(args[i]);
							gotscript = true;
							scriptArgs = args.Skip(i + 1).ToArray();
							Env.KeysharpArgs = args.Take(i + 1).ToArray();
							continue;
						}
						else//Parameters.
						{
							//No need to add args to A_Args, because it will be handled in the compiled program with HandleCommandLineParams().
							continue;
						}
					}

					var option = args[i].TrimStart(Keywords.DashSlash);
					var opt = option.ToLowerInvariant();

					switch (opt)
					{
						case "version":
						case "v":
							return Message($"{asm.GetName().Version}", false);

						case "validate":
							Script.ValidateThenExit = validate = true;
							break;

						case "about":
							var license = asm.GetManifestResourceStream(typeof(Program).Namespace + ".license.txt");
							return Message(new StreamReader(license).ReadToEnd(), false);

						case "assembly":
							assembly = true;

							if (i + 2 < args.Length && args[i + 1] != "*" && !File.Exists(args[i + 1]))
							{
								assemblyType = args[i + 1];
								assemblyMethod = args[i + 2];
								i += 2;
							}

							break;

						case "exeout":
							exeout = true;
							break;

						case "codeout":
							codeout = true;
							break;

						case "include":
							i++;
							break;
#if WINDOWS

						case "install"://To be called by the installer during installation.
							InstallToPath(exeDir);
							return 0;

						case "uninstall"://To be called by the uninstaller during uninstallation.
							RemoveFromPath(exeDir);
							return 0;
							//default:
							//  return Message($"Unrecognized switch: {args[i]}", true);
#endif
					}
				}

				//Message($"Operating off of script: {script} in current dir: {Environment.CurrentDirectory} for full path: {Path.GetFullPath(script)}", false);

				if (string.IsNullOrEmpty(script))
				{
					var dirs = new string[]
					{
						$"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}{exeName}.ahk",//Current executable dir.
						$"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}{exeName}.ks"
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

				if (assembly)
				{
					using (var reader = new BinaryReader(script == "*" ? Console.OpenStandardInput() : File.Open(script, FileMode.Open)))
					{
						int length = reader.ReadInt32();
						byte[] assemblyBytes = reader.ReadBytes(length);
						Assembly scriptAsm = Assembly.Load(assemblyBytes);
						Type type = scriptAsm.GetType(assemblyType);
						MethodInfo method = type.GetMethod(assemblyMethod);
						_ = method.Invoke(null, [scriptArgs]);
						return 0;
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

				if (!File.Exists(script))
					return Message($"Could not find the script file {script}.", true);

#if DEBUG
				Script.OutputDebug($"Creating DOM from {script}");
#endif
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

#if DEBUG
				Script.OutputDebug("Creating code from DOM.");
#endif
				var (code, exc) = ch.CreateCodeFromDom(domunits);

				if (exc is Exception e)
					return Message($"Creating C# code from DOM: {e.Message}", true);

				code = CompilerHelper.UsingStr + code;//Need to manually add the using static statements.

				//If they want to write out the code, place it in the same folder as the script, with the same name, and .cs extension.
				if (codeout)
				{
					writeCodeTask = Task.Run(() =>
					{
						var codePath = $"{path}.cs";

						try
						{
							using (var sourceWriter = new StreamWriter(codePath))
							{
								sourceWriter.WriteLine(code);
							}
						}
						catch (Exception writeex)
						{
							Message($"Writing code to {codePath} failed: {writeex.Message}", true);
						}
					});
				}

				//If they want to write out the code, place it in the same folder as the script, with the same name, and .exe extension.
				//Message($"Before compiling, setting current dir to {Environment.CurrentDirectory}", false);
#if DEBUG
				Script.OutputDebug("Compiling code.");
#endif
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
							var finalPath = "";

							try
							{
								var ver = GetLatestDotNetVersion();//Windows only.
								var outputRuntimeConfigPath = Path.ChangeExtension(path, "runtimeconfig.json");
								var currentRuntimeConfigPath = Path.ChangeExtension(exePath, "runtimeconfig.json");
								File.WriteAllBytes(path + ".dll", arr);
								File.Copy(currentRuntimeConfigPath, outputRuntimeConfigPath, true);
								//var outputDepsConfigPath = Path.ChangeExtension(path, "deps.json");
								//var currentDepsConfigPath = Path.ChangeExtension(loc, "deps.json");
								//File.Copy(currentDepsConfigPath, outputDepsConfigPath, true);
								//Message($"About to write executable to {path}.exe/dll.", false);
#if LINUX
								finalPath = path;
								HostWriter.CreateAppHost(
									appHostSourceFilePath: @$"/lib/dotnet/sdk/{ver}/AppHostTemplate/apphost",
									appHostDestinationFilePath: finalPath,
									appBinaryFilePath: $"{namenoext}.dll",
									windowsGraphicalUserInterface: false,
									assemblyToCopyResorcesFrom: $"{path}.dll");
#elif WINDOWS
								finalPath = $"{path}.exe";
								HostWriter.CreateAppHost(
									appHostSourceFilePath: @$"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\{ver}\runtimes\win-x64\native\apphost.exe",
									appHostDestinationFilePath: finalPath,
									appBinaryFilePath: $"{namenoext}.dll",
									windowsGraphicalUserInterface: true,
									assemblyToCopyResorcesFrom: $"{path}.dll");
#endif
								var ksCorePath = Path.Combine(exeDir, "Keysharp.Core.dll");
								//Need to copy Keysharp.Core from the install path to folder the script resides in. Without it, the compiled exe cannot be run in a standalone manner.
								//MessageBox.Show($"scriptdir = {scriptdir}");
								//MessageBox.Show($"About to copy from {ksCorePath} to {Path.Combine(scriptdir, "Keysharp.Core.dll")}");

								if (string.Compare(exeDir, scriptdir, true) != 0)
									if (File.Exists(ksCorePath))
										File.Copy(ksCorePath, Path.Combine(scriptdir, "Keysharp.Core.dll"), true);
							}
							catch (Exception writeex)
							{
								Message($"Writing executable to {finalPath} failed: {writeex.Message}", true);
							}
						});
					}

					CompilerHelper.compiledasm = Assembly.Load(arr);
				}
				else
				{
					return HandleCompilerErrors(results.Diagnostics, script, path, "Compiling C# code to executable", compileexc != null ? compileexc.Message : string.Empty);
				}

				if (validate)
					return 0;//Any other error condition returned 1 already.

				GC.Collect();
				GC.WaitForPendingFinalizers();

				if (CompilerHelper.compiledasm == null)
					throw new Exception("Compilation failed.");

				var program = CompilerHelper.compiledasm.GetType("Keysharp.CompiledMain.program");
				var main = program.GetMethod("Main");
#if DEBUG
				Script.OutputDebug("Running compiled code.");
#endif
				_ = main.Invoke(null, [scriptArgs]);
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
				var trace = $"{Accessors.A_AppData}/Keysharp/execution_errors.txt";

				try
				{
					//if (System.IO.File.Exists(trace))
					//  System.IO.File.Delete(trace);
					//System.IO.File.WriteAllText(trace, msg);
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
#if LINUX
			var dir = Directory.GetDirectories(@"/lib/dotnet/sdk/").Select(System.IO.Path.GetFileName).Where(x => x.StartsWith(dotNetMajorVersion)).OrderByDescending(x => new Version(x)).FirstOrDefault();
#elif WINDOWS
			var dir = Directory.GetDirectories(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\").Select(Path.GetFileName).Where(x => x.StartsWith(dotNetMajorVersion)).OrderByDescending(x => new Version(x.Contains("-rc") ? x.Substring(0, x.IndexOf("-rc")) : x)).FirstOrDefault();
#endif
			return dir;
		}

#if WINDOWS

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

#endif

		private static int HandleCompilerErrors(ImmutableArray<Diagnostic> diagnostics, string filename, string path, string desc, string message = "")
		{
			var errstr = CompilerHelper.HandleCompilerErrors(diagnostics, filename, desc, message);

			if (errstr != "")
			{
				//System.IO.File.WriteAllText($"{Keysharp.Core.Accessors.A_AppData}/Keysharp/compiler_errors.txt", errstr);
				_ = Message(errstr, true);
				return 1;
			}

			return 0;
		}

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
				//System.IO.File.WriteAllText($"{Keysharp.Core.Accessors.A_AppData}/Keysharp/compiler_errors.txt", errstr);
				_ = Message(errstr, true);
			}
			else
			{
				try
				{
					//System.IO.File.Delete($"{Keysharp.Core.Accessors.A_AppData}/Keysharp/compiler_errors.txt");
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
				Script.OutputDebug(text);
			}

			return error ? 1 : 0;
		}
	}
}