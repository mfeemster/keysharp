using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Keysharp.Scripting
{
	public partial class Parser : ICodeParser
	{
		public static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
		private const string className = "Program";
		private const string mainScope = "";
		private readonly Type bcl = typeof(Script).BaseType;
		private CodeAttributeDeclarationCollection assemblyAttributes = new CodeAttributeDeclarationCollection();
		private CompilerHelper Ch;
		private string fileName;

		private CodeStatementCollection initial = new CodeStatementCollection();
		private long line;

		//private CodeEntryPointMethod main = new CodeEntryPointMethod();
		private CodeMemberMethod main = new CodeMemberMethod();
		private Dictionary<string, CodeMemberMethod> methods = new Dictionary<string, CodeMemberMethod>();
		private string name = string.Empty;
		private CodeStatementCollection prepend = new CodeStatementCollection();

		//private List<CodeStatement> prepend = new List<CodeStatement>();
		private StringBuilder sbld = new StringBuilder();

		private CodeTypeDeclaration targetClass;
		//Not sure if we always want this locale, might want others? See what AHK supports.//MATT

		private CompilerParameters CompilerParameters { get; set; }

		static Parser()
		{
			ScanLibrary();
		}

		public Parser(CompilerHelper ch, CompilerParameters options)
		{
			Ch = ch;
			CompilerParameters = options;
			main = new CodeMemberMethod()
			{
				Attributes = MemberAttributes.Public | MemberAttributes.Static,
				Name = "Main"
			};
			main.ReturnType = new CodeTypeReference(typeof(int));
			_ = main.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string[]), "args"));
			_ = main.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(STAThreadAttribute))));
			targetClass = new CodeTypeDeclaration(className)
			{
				IsClass = true,
				//IsPartial = true,
				TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
			};
			_ = targetClass.Members.Add(main);
			//methods.Add(mainScope, main);
		}

		public static string TrimParens(string code)
		{
			var parenssb = new StringBuilder(code.Length);
			var spaceandtabs = new char[] { ' ', '\t' };
			var anyparens = false;
			var badline = false;

			//Microsoft's expression code erroneously adds parens where they shouldn't be, so remove them from the code here whenever a line starts with a paren.
			foreach (var line in code.SplitLines())
			{
				var either = false;
				var trimmedline = line.Trim(spaceandtabs);
				var startparen = trimmedline.StartsWith("(");// && !trimmedline.EndsWith("),");
				var endparen = trimmedline.EndsWith(");");

				if (startparen && endparen)
				{
					var noparensline = line.Remove(line.IndexOf('('), 1);
					var lastrparen = line.LastIndexOf(')');
					noparensline = noparensline.Remove(Math.Max(0, lastrparen - 1), 1);
					_ = parenssb.AppendLine(noparensline);
					anyparens = true;
					either = true;
				}
				else if (startparen)
				{
					badline = true;
					var noparensline = line.Remove(line.IndexOf('('), 1);
					_ = parenssb.AppendLine(noparensline);
					anyparens = true;
					either = true;
				}
				else if (badline && endparen)
				{
					var lastrparen = line.LastIndexOf(')');
					//var noparensline = line.Remove(Math.Max(0, lastrparen - 1), 1);
					var noparensline = line.Remove(Math.Max(0, lastrparen), 1);
					_ = parenssb.AppendLine(noparensline);
					badline = false;
					either = true;
				}

				if (!either)
					_ = parenssb.AppendLine(line);
			}

			return anyparens ? parenssb.ToString() : code;
		}

		/// <summary>
		/// Return a DOM representation of a script.
		/// </summary>
		public CodeCompileUnit GenerateCompileUnit()
		{
			var unit = new CodeCompileUnit();
			//var space = new CodeNamespace(bcl.Namespace + ".Instance");
			//_ = unit.Namespaces.Add(space);
			var ns = new CodeNamespace("Keysharp.CompiledMain");
			ns.Imports.Add(new CodeNamespaceImport("System"));
			ns.Imports.Add(new CodeNamespaceImport("System"));
			ns.Imports.Add(new CodeNamespaceImport("System.Collections"));
			ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
			ns.Imports.Add(new CodeNamespaceImport("System.Data"));
			ns.Imports.Add(new CodeNamespaceImport("System.IO"));
			ns.Imports.Add(new CodeNamespaceImport("System.Reflection"));
			ns.Imports.Add(new CodeNamespaceImport("System.Runtime.InteropServices"));
			ns.Imports.Add(new CodeNamespaceImport("System.Text"));
			ns.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
			ns.Imports.Add(new CodeNamespaceImport("System.Windows.Forms"));
			ns.Imports.Add(new CodeNamespaceImport("Keysharp.Core"));
			ns.Imports.Add(new CodeNamespaceImport("Keysharp.Scripting"));
			ns.Imports.Add(new CodeNamespaceImport("Array = Keysharp.Core.Array"));
			ns.Imports.Add(new CodeNamespaceImport("Buffer = Keysharp.Core.Buffer"));
			_ = ns.Types.Add(targetClass);
			_ = unit.Namespaces.Add(ns);
			AddAssemblyAttribute(typeof(AssemblyBuildVersionAttribute), Keysharp.Core.Accessors.A_AhkVersion);
			unit.AssemblyCustomAttributes.AddRange(assemblyAttributes);
			assemblyAttributes.Clear();
			//var container = new CodeTypeDeclaration(className);
			//container.BaseTypes.Add(bcl);
			//container.Attributes = MemberAttributes.Private;
			//_ = ns.Types.Add(targetClass);
			//var csc = new CodeStatementCollection();
			//csc.Add()
			//main.Statements.AddRange(new CodeStatementCollection(prepend.Where(x => !(x is CodeMethodReturnStatement)).ToArray()));
			//for (var i = 0; i < prepend.Count; i++)
			//{
			//  if (prepend[i] is CodeMethodReturnStatement)
			//      continue;

			//  //break;
			//  main.Statements.Add(prepend[i]);
			//}

			foreach (var p in prepend)
				if (!(p is CodeMethodReturnStatement))
				{
					if (p is CodeStatement cs)
						_ = main.Statements.Add(cs);
					else if (p is CodeExpression ce)
						_ = main.Statements.Add(ce);
				}

			if (Persistent)
			{
				var inv = (CodeMethodInvokeExpression)InternalMethods.RunMainWindow;
				_ = inv.Parameters.Add(new CodeSnippetExpression("name"));
				_ = inv.Parameters.Add(new CodeSnippetExpression("UserMainCode"));
				_ = main.Statements.Add(new CodeExpressionStatement(inv));
			}
			else
			{
				_ = main.Statements.Add(new CodeMethodInvokeExpression(null, "UserMainCode"));
				_ = main.Statements.Add(new CodeMethodInvokeExpression(null, "Keysharp.Core.Flow.Sleep", new CodeExpression[] { new CodePrimitiveExpression(-2L) }));
			}

			var exit0 = (CodeMethodInvokeExpression)InternalMethods.ExitApp;
			_ = exit0.Parameters.Add(new CodePrimitiveExpression(0));
			var exit1 = (CodeMethodInvokeExpression)InternalMethods.ExitApp;
			_ = exit1.Parameters.Add(new CodePrimitiveExpression(1));
			//Wrap the entire body of Main() in a try/catch block.
			//First try to catch our own special exceptions, and if the exception type was not that, then just look for regular system exceptions.
			var tcf = new CodeTryCatchFinallyStatement();
			//
			var ctch2 = new CodeCatchClause("kserr", new CodeTypeReference("Keysharp.Core.Error"));
			var ces = new CodeSnippetExpression("ErrorOccurred(kserr)");
			var msg = new CodeSnippetExpression("MsgBox(\"Uncaught Keysharp exception:\\r\\n\" + kserr)");
			var ccs = new CodeConditionStatement(ces);
			_ = ccs.TrueStatements.Add(msg);
			_ = ctch2.Statements.Add(ccs);
			_ = ctch2.Statements.Add(new CodeExpressionStatement(exit1));
			_ = ctch2.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(1)));//Add a failure return statement at the end of the catch block.
			_ = tcf.CatchClauses.Add(ctch2);
			//
			var ctch = new CodeCatchClause("mainex", new CodeTypeReference("System.Exception"));
			_ = ctch.Statements.Add(new CodeSnippetExpression(@"var ex = mainex.InnerException ?? mainex;

				if (ex is Keysharp.Core.Error kserr)
				{
					if (ErrorOccurred(kserr))
					{
						MsgBox(""Uncaught Keysharp exception:\r\n"" + kserr);
					}
				}
				else
				{
					MsgBox(""Uncaught exception:\r\n"" + ""Message: "" + ex.Message + ""\r\nStack: "" + ex.StackTrace);
				}
"));
			//_ = ctch.Statements.Add(new CodeSnippetExpression("MsgBox(\"Uncaught exception:\\r\\n\" + \"Message: \" + mainex.Message + \"\\r\\nStack: \" + mainex.StackTrace)"));
			_ = ctch.Statements.Add(new CodeExpressionStatement(exit1));
			_ = ctch.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(1)));//Add a failure return statement at the end of the catch block.
			_ = tcf.CatchClauses.Add(ctch);
			var tempstatements = main.Statements;
			tcf.TryStatements.AddRange(tempstatements);
			_ = tcf.TryStatements.Add(new CodeExpressionStatement(exit0));
			_ = tcf.TryStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(0)));//Add a successful return statement at the end of the try block.
			main.Statements.Clear();
			_ = main.Statements.Add(tcf);
			//It's hard to properly set return statements during parsing, so ensure their correctness here.//MATT
			methods.Values.Where(meth =>
								 meth.ReturnType.BaseType != "System.Void"
								 && !(meth.Statements.Cast<CodeStatement>().LastOrDefault() is CodeMethodReturnStatement)
								).ToList().ForEach(meth2 =>
												   meth2.Statements.Add(new CodeMethodReturnStatement(
														   new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Empty"))));

			//meth2.Statements.Add(new CodeMethodReturnStatement(new CodeDefaultValueExpression(meth2.ReturnType))));
			while (invokes.Count != 0)
				StdLib();

			foreach (var method in methods.Values)
				_ = targetClass.Members.Add(method);

			return unit;
		}

		public CodeCompileUnit Parse(TextReader codeStream) => Parse(codeStream, string.Empty);

		public CodeCompileUnit Parse(TextReader codeStream, string nm)
		{
			sbld = new StringBuilder();
			name = nm;
			Init();//Init here every time because this may be reused within a single program run, such as in Keyview.
			var lines = Read(codeStream, nm);
			Statements(lines);

			//if (Persistent)
			//{
			//  var inv = (CodeMethodInvokeExpression)InternalMethods.RunMainWindow;
			//  _ = inv.Parameters.Add(new CodeSnippetExpression("name"));
			//  _ = initial.Add(new CodeExpressionStatement(inv));
			//}

			if (!NoTrayIcon)
				_ = initial.Add(new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.CreateTrayMenu));

			_ = initial.Add(new CodeSnippetExpression("HandleCommandLineParams(args)"));

			if (WinActivateForce)
				_ = initial.Add(new CodeSnippetExpression("Keysharp.Core.Accessors.WinActivateForce = true"));

			var inst = (CodeMethodInvokeExpression)InternalMethods.HandleSingleInstance;
			_ = inst.Parameters.Add(new CodeSnippetExpression("name"));
			_ = inst.Parameters.Add(new CodeSnippetExpression($"eScriptInstance.{(name == "*" ? "Off" : SingleInstance)}"));
			_ = initial.Add(new CodeExpressionStatement(inst));
			_ = initial.Add(new CodeSnippetExpression("Keysharp.Scripting.Script.SetName(name)"));
			_ = initial.Add(new CodeSnippetExpression("Keysharp.Scripting.Script.Variables.InitGlobalVars()"));
			var namevar = new CodeVariableDeclarationStatement("System.String", "name", new CodeSnippetExpression($"@\"{name}\""));
			_ = initial.Add(namevar);
			var userMainMethod = new CodeMemberMethod()
			{
				Attributes = MemberAttributes.Public | MemberAttributes.Static,
				Name = "UserMainCode"
			};
			userMainMethod.Statements.AddRange(main.Statements);
			main.Statements.Clear();
			_ = targetClass.Members.Add(userMainMethod);

			foreach (CodeStatement stmt in initial)
				main.Statements.Insert(0, stmt);

			if (allVars.TryGetValue(mainScope, out var globalVars))
			{
				foreach (var globalvar in globalVars)
				{
					_ = targetClass.Members.Add(new CodeMemberField(typeof(object), globalvar.Replace(ScopeVar[0], '_'))
					{
						Attributes = MemberAttributes.Public | MemberAttributes.Static
					});
				}
			}

			return GenerateCompileUnit();
		}
	}
}