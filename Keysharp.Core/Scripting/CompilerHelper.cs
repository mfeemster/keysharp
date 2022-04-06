using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyModel;

#if WINDOWS

	using Microsoft.CodeAnalysis.CSharp;

#endif

namespace Keysharp.Scripting
{
	public class CompilerHelper
	{
		/// <summary>
		/// Needed as a static here so it can be accessed in other areas of Keysharp.Core, such as in Accessors,
		/// to determine if the executing code is a standalone executable, or a script that was compiled and ran through
		/// the main program.
		/// </summary>
		public static Assembly compiledasm;

		private Parser parser;

		/// <summary>
		/// Define the compile unit to use for code generation.
		/// </summary>
		//CodeCompileUnit targetUnit;

		//CodeTypeDeclaration targetClass;

		//CodeEntryPointMethod entryPoint;
		/// <summary>
		/// For some reason, the CodeEntryPoint object doesn't seem to allow adding parameters, so we use the base and manually set values and add string[] args.
		/// </summary>
		//CodeMemberMethod entryPoint;
		//System.Web.Configuration.WebConfigurationManager cfg = new System.Web.Configuration.WebConfigurationManager();
		//Need to manually add the using static statements.
		public static readonly string UsingStr =
			@"using static Keysharp.Core.Accessors;
using static Keysharp.Core.Core;
//using static Keysharp.Core.Common.Window.WindowItemBase;
using static Keysharp.Core.Common.Keyboard.HotstringDefinition;
using static Keysharp.Core.Dialogs;
using static Keysharp.Core.Disk;
using static Keysharp.Core.DllHelper;
using static Keysharp.Core.Env;
using static Keysharp.Core.File;
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

";

		public CompilerHelper()
		{
			parser = new Parser(this, new CompilerParameters());//Needed just so error printing can be done if needed before compilation starts.
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

#if !WINDOWS
		CodeDomProvider provider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();
		public (CompilerResults, Exception) Compile(string code, string outputname)
#else

		private CodeDomProvider provider = CodeDomProvider.CreateProvider("csharp", new Dictionary<string, string>
		{
			{
				"CompilerDirectoryPath", Path.Combine(Environment.CurrentDirectory,
#if DEBUG
													  "./roslyn"
#else
													  "./roslyn"
#endif
													 )
			}
		});

		private CodeGeneratorOptions cgo = new CodeGeneratorOptions
		{
			IndentString = "\t",
			VerbatimOrder = true,
			BracingStyle = "C"
		};

		internal string CodeToString(CodeExpression expr)
		{
			using (TextWriter tx = new StringWriter())
			{
				provider.GenerateCodeFromExpression(expr, tx, cgo);
				return tx.ToString();
			}
		}

		public (EmitResult, MemoryStream, Exception) Compile(string code, string outputname)
#endif
		{
			try
			{
#if !WINDOWS
				var parameters = new CompilerParameters()
				{
					GenerateExecutable = !string.IsNullOrEmpty(outputname),
					IncludeDebugInformation = false,
					GenerateInMemory = true,
					OutputAssembly = outputname,
					MainClass = "Keysharp.Main.Program"
				};
				_ = parameters.ReferencedAssemblies.Add("System.dll");
				_ = parameters.ReferencedAssemblies.Add("System.Collections.dll");
				_ = parameters.ReferencedAssemblies.Add("System.Data.dll");
				_ = parameters.ReferencedAssemblies.Add("System.IO.dll");
				_ = parameters.ReferencedAssemblies.Add("System.Linq.dll");
				_ = parameters.ReferencedAssemblies.Add("System.Reflection.dll");
				_ = parameters.ReferencedAssemblies.Add("System.Runtime.dll");
				_ = parameters.ReferencedAssemblies.Add("System.Private.CoreLib.dll");
				_ = parameters.ReferencedAssemblies.Add("System.Drawing.Common.dll");
				_ = parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
				_ = parameters.ReferencedAssemblies.Add("Keysharp.Core.dll");
				var results = provider.CompileAssemblyFromSource(parameters, code);
				return (results, null);
#else
				var tree = SyntaxFactory.ParseSyntaxTree(code,
						   new CSharpParseOptions(LanguageVersion.CSharp8, DocumentationMode.None, SourceCodeKind.Regular));
				var currentDomain = AppDomain.CurrentDomain;
				var coreDir = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
				var desktopDir = Path.GetDirectoryName(typeof(System.Windows.Forms.Form).GetTypeInfo().Assembly.Location);
				var usings = new List<string>()//These aren't what show up in the output .cs file.
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
					"Keysharp.Core"
				};
				//var references = new List<MetadataReference>();
				//List<string> GetAssemblyFiles(Assembly assembly)
				//{
				//  var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
				//  return assembly.GetReferencedAssemblies()
				//         .Select(name => loadedAssemblies.SingleOrDefault(a => a.FullName == name.FullName)?.Location)
				//         .Where(l => l != null).ToList();
				//}
				//var assemblyFiles = GetAssemblyFiles(typeof(CompilerHelper).Assembly);
				//foreach (var dll in assemblyFiles)
				//  references.Add(MetadataReference.CreateFromFile(dll));
				//foreach (var dll in Directory.GetFiles(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\5.0.0\ref\net5.0", "*.dll"))
				//foreach (var dll in Directory.GetFiles(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.11", "*.dll"))
				//{
				//  var filename = System.IO.Path.GetFileName(dll);
				//
				//  if (!filename.Contains("VisualBasic") && !filename.StartsWith("System.Private") && (filename == "mscorlib.dll" || filename.StartsWith("System") || filename.StartsWith("Microsoft")))
				//      references.Add(MetadataReference.CreateFromFile(dll));
				//}
				//
				////foreach (var dll in Directory.GetFiles(@"C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\5.0.0\ref\net5.0\", " *.dll"))
				//foreach (var dll in Directory.GetFiles(@"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\5.0.11", " *.dll"))
				//{
				//  var filename = System.IO.Path.GetFileName(dll);
				//
				//  if (!filename.Contains("VisualBasic") && !filename.Contains("PresentationFramework") && !filename.Contains("ReachFramework")
				//          && (filename.StartsWith("System") || filename.StartsWith("Microsoft")))
				//      references.Add(MetadataReference.CreateFromFile(dll));
				//}
				//references.Add(MetadataReference.CreateFromFile("Keysharp.Core.dll"));
				//references.Add(MetadataReference.CreateFromFile("Keysharp.Core.dll"));
				//var temp = typeof(object).Assembly.Location;
				//references.Add(MetadataReference.CreateFromFile(temp));
				//coreDir = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.11";
				//desktopDir = @"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\5.0.11";
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
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "netstandard.dll")),
					MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Private.CoreLib.dll")),
					MetadataReference.CreateFromFile(Path.Combine(desktopDir, "System.Drawing.Common.dll")),
					MetadataReference.CreateFromFile(Path.Combine(desktopDir, "System.Windows.Forms.dll")),
					MetadataReference.CreateFromFile("Keysharp.Core.dll"),
				};
				//references =
				//  DependencyContext.Default.CompileLibraries
				//  .SelectMany(cl => cl.ResolveReferencePaths())
				//  .Select(asm => MetadataReference.CreateFromFile(asm))
				//  .ToList();
				//foreach (var lib in DependencyContext.Default.CompileLibraries)
				//{
				//  var refpaths = lib.ResolveReferencePaths();
				//  foreach (var refpath in refpaths)
				//  {
				//      references.Add(MetadataReference.CreateFromFile(refpath));
				//  }
				//}
				var ms = new MemoryStream();
				var compilation = CSharpCompilation.Create(outputname)
								  .WithOptions(
									  new CSharpCompilationOptions(OutputKind.WindowsApplication)
									  //new CSharpCompilationOptions(OutputKind.ConsoleApplication)
									  .WithUsings(usings)
									  .WithOptimizationLevel(OptimizationLevel.Release)
									  .WithPlatform(Platform.AnyCpu))
								  .AddReferences(references)
								  .AddSyntaxTrees(tree)
								  ;
				var msi = Assembly.GetEntryAssembly().GetManifestResourceStream("Keysharp.Keysharp.ico");
				//msi.CopyTo(new FileStream("./testico.ico", FileMode.Create));
				//msi.Seek(0, SeekOrigin.Begin);
				var res = compilation.CreateDefaultWin32Resources(true, true, null, msi);//The first argument must be true to embed version/assembly information.
				var compilationResult = compilation.Emit(ms, win32Resources: res);
				return (compilationResult, ms, null);
#endif
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
				foreach (var unit in units)//Not sure if it should be units or the global targetClass.//MATT
				{
					var sourceWriter = new StringWriter();
					provider.GenerateCodeFromCompileUnit(unit, sourceWriter, cgo);//This relieves us of any manual traversal of the DOM.
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
			var units = new CodeCompileUnit[fileNames.Length];
			var errors = new CompilerErrorCollection();//Maybe make a member so it can be retrieved outside of this.
			var enc = Encoding.Default;
			var x = Keysharp.Core.Env.FindCommandLineArg("cp");
			parser = new Parser(this, new CompilerParameters());

			if (x != null)
			{
				x = x.Trim(Keysharp.Core.Core.DashSlash);

				if (x.Length > 2 && int.TryParse(x.AsSpan().Slice(2), out var codepage))
					enc = Encoding.GetEncoding(codepage);
			}

			for (var i = 0; i < fileNames.Length; i++)
			{
				try
				{
					if (File.Exists(fileNames[i]))
					{
						Script.scriptName = fileNames[i];
						units[i] = parser.Parse(new StreamReader(fileNames[i], enc), Path.GetFullPath(fileNames[i]));
					}
					else
					{
						Script.scriptName = "*";
						units[i] = parser.Parse(new StringReader(fileNames[i]), "*");//In memory.
					}
				}
				catch (Keysharp.Core.ParseException e)
				{
					_ = errors.Add(new CompilerError(e.Source, (int)e.Line, 0, e.Message.GetHashCode().ToString(), e.Message));
				}
				catch (Exception e)
				{
					_ = errors.Add(new CompilerError { ErrorText = e.Message });
				}
				finally { }
			}

			//_ = entryPoint.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(1)));
			return (units, errors);
		}

		public void PrintCompilerErrors(string s) => parser?.PrintCompilerErrors(s);

		public static (string, string) GetCompilerErrors(CompilerErrorCollection results, string filename = "")
		{
			var sbe = new StringBuilder();
			var sbw = new StringBuilder();

			if (results.HasErrors)
			{
				_ = sbe.AppendLine("The following errors occurred:");
				_ = sbe.AppendLine();
			}

			if (results.HasWarnings)
			{
				_ = sbw.AppendLine("The following warnings occurred:");
				_ = sbw.AppendLine();
			}

			foreach (CompilerError error in results)
			{
				var file = string.IsNullOrEmpty(error.FileName) ? filename : error.FileName;
				_ = !error.IsWarning
					? sbe.AppendLine($"{Path.GetFileName(file)}:{error.Line} - {error.ErrorText}")
					: sbw.AppendLine($"{Path.GetFileName(file)}:{error.Line} - {error.ErrorText}");
			}

			return (sbe.ToString(), sbw.ToString());
		}

		public static string HandleCompilerErrors(ImmutableArray<Diagnostic> diagnostics, string filename, string desc, string message = "")
		{
			var sbe = new StringBuilder();
			var sbw = new StringBuilder();

			foreach (var diag in diagnostics)
			{
				var str = $"{Path.GetFileName(filename)}:{diag.Location.GetLineSpan()} - {diag.GetMessage()}";

				if (diag.Severity == DiagnosticSeverity.Warning)
					_ = sbw.AppendLine(str);

				if (diag.Severity == DiagnosticSeverity.Error)
					_ = sbe.AppendLine(str);
			}

			if (sbw.Length != 0)
			{
				_ = sbw.Insert(0, "The following warnings occurred:");
				_ = sbw.AppendLine();
			}

			if (sbe.Length != 0)
			{
				_ = sbe.Insert(0, "The following errors occurred:");
				_ = sbe.AppendLine();
				return $"{desc} failed:\n\n{sbe.ToString()}\n\n\n{sbw.ToString()}{(message != "" ? "\n\n" + message : "")}";
			}

			return "";
		}
	}
}