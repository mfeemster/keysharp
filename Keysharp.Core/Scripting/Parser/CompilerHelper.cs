namespace Keysharp.Scripting
{
	public class CompilerHelper
	{
		//CodeEntryPointMethod entryPoint;
		/// <summary>
		/// For some reason, the CodeEntryPoint object doesn't seem to allow adding parameters, so we use the base and manually set values and add string[] args.
		/// </summary>
		//CodeMemberMethod entryPoint;
		//System.Web.Configuration.WebConfigurationManager cfg = new System.Web.Configuration.WebConfigurationManager();
		//Need to manually add the using static statements.
#if WINDOWS

		public static readonly string UsingStr =
			@"using static Keysharp.Core.Accessors;
using static Keysharp.Core.COM.Com;
using static Keysharp.Core.Collections;
using static Keysharp.Core.Common.Keyboard.HotkeyDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringManager;
using static Keysharp.Core.ControlX;
using static Keysharp.Core.Dialogs;
using static Keysharp.Core.Dir;
using static Keysharp.Core.Dll;
using static Keysharp.Core.Drive;
using static Keysharp.Core.EditX;
using static Keysharp.Core.Env;
using static Keysharp.Core.Errors;
using static Keysharp.Core.External;
using static Keysharp.Core.Files;
using static Keysharp.Core.Flow;
using static Keysharp.Core.Functions;
using static Keysharp.Core.GuiHelper;
using static Keysharp.Core.ImageLists;
using static Keysharp.Core.Images;
using static Keysharp.Core.Ini;
using static Keysharp.Core.Input;
using static Keysharp.Core.Keyboard;
using static Keysharp.Core.Loops;
using static Keysharp.Core.Maths;
using static Keysharp.Core.Menu;
using static Keysharp.Core.Misc;
using static Keysharp.Core.Monitor;
using static Keysharp.Core.Mouse;
using static Keysharp.Core.Network;
using static Keysharp.Core.Processes;
using static Keysharp.Core.RealThreads;
using static Keysharp.Core.RegEx;
using static Keysharp.Core.Registrys;
using static Keysharp.Core.Screen;
using static Keysharp.Core.Security;
using static Keysharp.Core.SimpleJson;
using static Keysharp.Core.Sound;
using static Keysharp.Core.Strings;
using static Keysharp.Core.ToolTips;
using static Keysharp.Core.Types;
using static Keysharp.Core.WindowX;
using static Keysharp.Core.Windows.WindowsAPI;
using static Keysharp.Scripting.Script.Operator;
using static Keysharp.Scripting.Script;
";

#else
		public static readonly string UsingStr =
			@"using static Keysharp.Core.Accessors;
using static Keysharp.Core.Collections;
using static Keysharp.Core.Common.Keyboard.HotkeyDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringManager;
using static Keysharp.Core.ControlX;
using static Keysharp.Core.Dialogs;
using static Keysharp.Core.Dir;
//using static Keysharp.Core.Dll;
using static Keysharp.Core.Drive;
using static Keysharp.Core.EditX;
using static Keysharp.Core.Env;
using static Keysharp.Core.Errors;
using static Keysharp.Core.External;
using static Keysharp.Core.Files;
using static Keysharp.Core.Flow;
using static Keysharp.Core.Functions;
using static Keysharp.Core.GuiHelper;
using static Keysharp.Core.ImageLists;
using static Keysharp.Core.Images;
using static Keysharp.Core.Ini;
using static Keysharp.Core.Input;
using static Keysharp.Core.Keyboard;
using static Keysharp.Core.Loops;
using static Keysharp.Core.Maths;
using static Keysharp.Core.Menu;
using static Keysharp.Core.Misc;
using static Keysharp.Core.Monitor;
using static Keysharp.Core.Mouse;
using static Keysharp.Core.Network;
using static Keysharp.Core.Processes;
using static Keysharp.Core.RealThreads;
using static Keysharp.Core.RegEx;
using static Keysharp.Core.Screen;
using static Keysharp.Core.Security;
using static Keysharp.Core.SimpleJson;
using static Keysharp.Core.Sound;
using static Keysharp.Core.Strings;
using static Keysharp.Core.ToolTips;
using static Keysharp.Core.Types;
using static Keysharp.Core.WindowX;
using static Keysharp.Scripting.Script.Operator;
using static Keysharp.Scripting.Script;
";
#endif

		/// <summary>
		/// Needed as a static here so it can be accessed in other areas of Keysharp.Core, such as in Accessors,
		/// to determine if the executing code is a standalone executable, or a script that was compiled and ran through
		/// the main program.
		/// </summary>
		public static Assembly compiledasm;
		public static byte[] compiledBytes;

		private readonly CodeGeneratorOptions cgo = new ()
		{
			IndentString = "\t",
			VerbatimOrder = true,
			BracingStyle = "C"
		};

		private readonly CodeDomProvider provider = CodeDomProvider.CreateProvider("csharp", new Dictionary<string, string>
		{
			{
				"CompilerDirectoryPath", Path.Combine(Environment.CurrentDirectory, "./roslyn")
			}
		});

		private Parser parser;

		/// <summary>
		/// Define the compile unit to use for code generation.
		/// </summary>
		//CodeCompileUnit targetUnit;
		public CompilerHelper()
		{
			parser = new Parser(this);
		}

		public static string GenerateRuntimeConfig()
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new Utf8JsonWriter(
					stream,
				new JsonWriterOptions() { Indented = true }
			))
				{
					writer.WriteStartObject();
					writer.WriteStartObject("runtimeOptions");
					writer.WriteStartObject("framework");
					writer.WriteString("name", "Microsoft.WindowsDesktop.App");
					writer.WriteString(
						"version",
						RuntimeInformation.FrameworkDescription.Replace(".NET ", "")
					);
					writer.WriteEndObject();
					writer.WriteEndObject();
					writer.WriteEndObject();
				}
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		public static (string, string) GetCompilerErrors(CompilerErrorCollection results, string filename = "")
		{
			var sbe = new StringBuilder();
			var sbw = new StringBuilder();

			if (results.HasErrors)
			{
				_ = sbe.AppendLine("The following errors occurred:");
			}

#if DEBUG

			if (results.HasWarnings)
			{
				_ = sbw.AppendLine("The following warnings occurred:");
			}

#endif

			foreach (CompilerError error in results)
			{
				var file = string.IsNullOrEmpty(error.FileName) ? filename : error.FileName;
				file = Path.GetFileName(file);

				if (file.Length == 0)
					file = "*";

				_ = !error.IsWarning
					? sbe.AppendLine($"\n{error.ErrorText}")
					: sbw.AppendLine($"\n{error.ErrorText}");
			}

			return (sbe.ToString(), sbw.ToString());
		}

		public static string HandleCompilerErrors(ImmutableArray<Diagnostic> diagnostics, string filename, string desc, string message = "")
		{
			var sbe = new StringBuilder();
			var sbw = new StringBuilder();

			foreach (var diag in diagnostics)
			{
				var str = $"{Path.GetFileName(filename)}{diag.Location.GetLineSpan()} - {diag.GetMessage()}";

				if (diag.Severity == DiagnosticSeverity.Warning)
					_ = sbw.AppendLine($"\t{str}");

				if (diag.Severity == DiagnosticSeverity.Error)
					_ = sbe.AppendLine($"\t{str}");
			}

#if DEBUG

			if (sbw.Length != 0)
			{
				_ = sbw.Insert(0, "The following warnings occurred:\n");
			}

#endif

			if (sbe.Length != 0)
			{
				_ = sbe.Insert(0, "The following errors occurred:\n");
				return $"{desc} failed.\n\n{sbe}\n{sbw}" + (message != "" ? "\n" + message : "");//Needed to break this up so the AStyle formatter doesn't misformat it.
			}

			return "";
		}

		public (EmitResult, MemoryStream, Exception) Compile(string code, string outputname, string currentDir)
		{
			try
			{
				var tree = SyntaxFactory.ParseSyntaxTree(code,
						   new CSharpParseOptions(LanguageVersion.LatestMajor, DocumentationMode.None, SourceCodeKind.Regular));
				var coreDir = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
				var desktopDir = Path.GetDirectoryName(typeof(Form).GetTypeInfo().Assembly.Location);
				var usings = new List<string>()//These aren't what show up in the output .cs file. See Parser.GenerateCompileUnit() for that.
				{
					"System",
					"System.Collections",
					"System.Collections.Generic",
					"System.Data",
					"System.Drawing",
					"System.IO",
					"System.Linq",
					"System.Reflection",
					"System.Runtime",
					"System.Windows.Forms",
					"System.Runtime.InteropServices",
					"Keysharp.Core"
				};
				var references = new List<MetadataReference>
				{
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "mscorlib.dll")),
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.dll")),
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Collections.dll")),
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Data.dll")),
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.IO.dll")),
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Linq.dll")),
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Reflection.dll")),
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll")),
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "netstandard.dll")),//Is this even still needed?//TODO
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Private.CoreLib.dll")),
					MetadataReference.CreateFromFile(Path.Combine(desktopDir, "System.Drawing.Common.dll")),
					MetadataReference.CreateFromFile(Path.Combine(desktopDir, "System.Windows.Forms.dll")),
					//This will be the build output folder when running from within the debugger, and the install folder when running from an installation.
					//Note that Keysharp.Core.dll *must* remain in that location for a compiled executable to work.
					//MetadataReference.CreateFromFile(Path.Combine(Environment.CurrentDirectory, "Keysharp.Core.dll")),
					MetadataReference.CreateFromFile(Path.Combine(currentDir, "Keysharp.Core.dll")),
				};
				var ms = new MemoryStream();
				var compilation = CSharpCompilation.Create(outputname)
								  .WithOptions(
									  new CSharpCompilationOptions(OutputKind.WindowsApplication)
									  .WithUsings(usings)
									  .WithOptimizationLevel(OptimizationLevel.Release)
									  .WithPlatform(Platform.AnyCpu)
									  .WithConcurrentBuild(true)
								  )
								  .AddReferences(references)
								  .AddSyntaxTrees(tree)
								  ;
				// Apparently there isn't a good way to read app.manifest contents from the running process,
				// so instead we recreate it here.
				// Any change in the manifest should be reflected here and in Keysharp app.manifest file.
				var manifestContents =
					@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
					<assembly xmlns=""urn:schemas-microsoft-com:asm.v1"" manifestVersion=""1.0"">
						<trustInfo xmlns=""urn:schemas-microsoft-com:asm.v2"">
							<security>
								<requestedPrivileges xmlns=""urn:schemas-microsoft-com:asm.v3"">
									<requestedExecutionLevel level=""asInvoker"" uiAccess=""false"" />
								</requestedPrivileges>
							</security>
						</trustInfo>
						<asmv3:application xmlns:asmv3=""urn:schemas-microsoft-com:asm.v3"">
							<asmv3:windowsSettings xmlns=""http://schemas.microsoft.com/SMI/2005/WindowsSettings"">
								<!-- Extra info: https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests -->
								<disableWindowFiltering xmlns=""http://schemas.microsoft.com/SMI/2011/WindowsSettings"">true</disableWindowFiltering>
								<longPathAware xmlns=""http://schemas.microsoft.com/SMI/2016/WindowsSettings"">true</longPathAware>
							</asmv3:windowsSettings>
						</asmv3:application>
					</assembly>";
				var manifestStream = new MemoryStream();
				var writer = new StreamWriter(manifestStream);
				writer.Write(manifestContents);
				writer.Flush();
				manifestStream.Position = 0;
				var msi = Assembly.GetEntryAssembly().GetManifestResourceStream("Keysharp.Keysharp.ico");
				var res = compilation.CreateDefaultWin32Resources(true, false, manifestStream, msi);//The first argument must be true to embed version/assembly information.
				var compilationResult = compilation.Emit(ms, win32Resources: res);
				return (compilationResult, ms, null);
			}
			catch (Exception e)
			{
				return (null, null, e);
			}
		}

		public (string, Exception) CreateCodeFromDom(CodeCompileUnit[] units)
		{
			var sb = new StringBuilder(100000);

			try
			{
				foreach (var unit in units)
				{
					var sourceWriter = new StringWriter();
					provider.GenerateCodeFromCompileUnit(unit, sourceWriter, cgo);//Generating code, then compiling that relieves us of any manual traversal of the DOM.
					_ = sb.Append(sourceWriter.ToString());
				}
			}
			catch (Exception e)
			{
				return (sb.ToString(), e);
			}

			return (sb.ToString(), null);
		}

		public (CodeCompileUnit[], CompilerErrorCollection) CreateDomFromFile(params string[] fileNames)
		{
			// Creating the DOM relies on querying methods from Reflections, but if previously
			// a script assembly has been loaded (eg when running tests) then unwanted methods
			// may be found from there. Thus clear all cached methods here, and reinitialize while
			// ignoring the "program" namespace.
			Reflections.Clear();
			Reflections.Initialize(true);
			Flow.ResetState();
			var units = new CodeCompileUnit[fileNames.Length];
			var errors = new CompilerErrorCollection();
			var enc = Encoding.Default;
			var x = Env.FindCommandLineArg("cp");
			var (pushed, btv) = Threads.BeginThread();//Some internal parsing uses Accessors, so a thread must be present.

			if (pushed)
			{
				parser = new Parser(this);

				if (x != null)
				{
					x = x.Trim(DashSlash);

					if (x.Length > 2 && int.TryParse(x.AsSpan().Slice(2), out var codepage))
						enc = Encoding.GetEncoding(codepage);
				}

				for (var i = 0; i < fileNames.Length; i++)//This has likely never been tested with more than one file at a time. Need to figure that out.//TODO
				{
					try
					{
						if (File.Exists(fileNames[i]))
						{
							Script.scriptName = fileNames[i];
							using var reader = new StreamReader(Script.scriptName, enc);
							units[i] = parser.Parse(reader, Path.GetFullPath(fileNames[i]));
						}
						else
						{
							Script.scriptName = "*";
							using var reader = new StringReader(fileNames[i]);
							units[i] = parser.Parse(reader, "*");//In memory.
						}
					}
					catch (ParseException e)
					{
						_ = errors.Add(new CompilerError(e.File, (int)e.Line, 0, "0", e.Message));
					}
					catch (Exception e)
					{
						_ = errors.Add(new CompilerError { ErrorText = e.Message });
					}
					finally
					{
					}
				}

				_ = Threads.EndThread(pushed);
			}

			return (units, errors);
		}

		public void PrintCompilerErrors(string s)
		{
			if (parser.ErrorStdOut || Env.FindCommandLineArg("errorstdout") != null)
				Core.Debug.OutputDebug(s);//For this to show on the command line, they need to pipe to more like: | more
			else
				_ = MessageBox.Show(s, "Keysharp", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		internal string CodeToString(CodeExpression expr)
		{
			using (TextWriter tx = new StringWriter())
			{
				provider.GenerateCodeFromExpression(expr, tx, cgo);
				return tx.ToString();
			}
		}

		internal string CreateEscapedIdentifier(string variable) => provider.CreateEscapedIdentifier(variable);

		internal object EvaluateCode(string code)
		{
			var coreDir = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
			var usings = new List<string>()//These aren't what show up in the output .cs file.
			{
				"System"
			};
			var references = new List<MetadataReference>
			{
				MetadataReference.CreateFromFile(Path.Combine(coreDir, "mscorlib.dll")),
				MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.dll")),
				MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Private.CoreLib.dll"))
			};
			string finalCode = @"
using System;

namespace Dyn
{
	public class DynamicCode
	{
		public object Evaluate()
		{
			return " + code + @";
		}
	}
}";
			var tree = SyntaxFactory.ParseSyntaxTree(finalCode,
					   new CSharpParseOptions(LanguageVersion.LatestMajor, DocumentationMode.None, SourceCodeKind.Regular));
			var ms = new MemoryStream();
			var compilation = CSharpCompilation.Create("DynamicCode")
							  .WithOptions(
								  new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
								  .WithUsings(usings)
								  .WithOptimizationLevel(OptimizationLevel.Debug)//Quick evaluations don't need to be optimized.
								  .WithPlatform(Platform.AnyCpu)
								  .WithConcurrentBuild(true)
							  )
							  .AddReferences(references)
							  .AddSyntaxTrees(tree)
							  ;
			var results = compilation.Emit(ms);

			if (results.Success)
			{
				_ = ms.Seek(0, SeekOrigin.Begin);
				var arr = ms.ToArray();
				var compiledasm = Assembly.Load(arr);
				object o = compiledasm.CreateInstance("Dyn.DynamicCode");
				Type t = o.GetType();
				return t.GetMethod("Evaluate").Invoke(o, null);
			}
			else
				throw new ParseException($"Failed to compile: {code}.");
		}

		internal bool IsValidIdentifier(string variable) => provider.IsValidIdentifier(variable);
	}
}