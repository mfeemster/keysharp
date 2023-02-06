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
using System.Xml.Linq;
using Keysharp.Core;
using Keysharp.Core.Common.Keyboard;

namespace Keysharp.Scripting
{
	public partial class Parser : ICodeParser
	{
		public static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
		private const string mainClassName = "program";
		private const string mainScope = "";
		private readonly Type bcl = typeof(Script).BaseType;
		private CodeAttributeDeclarationCollection assemblyAttributes = new CodeAttributeDeclarationCollection();
		private CompilerHelper Ch;
		private string fileName;

		private CodeStatementCollection initial = new CodeStatementCollection();
		private long line;

		//private CodeEntryPointMethod main = new CodeEntryPointMethod();
		private CodeMemberMethod main = new CodeMemberMethod();
		private Stack<CodeTypeDeclaration> typeStack = new Stack<CodeTypeDeclaration>();
		private Dictionary<CodeTypeDeclaration, Dictionary<string, CodeMemberMethod>> methods = new Dictionary<CodeTypeDeclaration, Dictionary<string, CodeMemberMethod>>();
		private Dictionary<CodeTypeDeclaration, Dictionary<string, CodeMemberProperty>> properties = new Dictionary<CodeTypeDeclaration, Dictionary<string, CodeMemberProperty>>();
		private string name = string.Empty;
		private CodeStatementCollection prepend = new CodeStatementCollection();
		private Dictionary<CodeTypeDeclaration, Dictionary<string, List<CodeMethodInvokeExpression>>> setPropertyValueCalls = new Dictionary<CodeTypeDeclaration, Dictionary<string, List<CodeMethodInvokeExpression>>>();
		private Dictionary<CodeTypeDeclaration, Dictionary<string, List<CodeMethodInvokeExpression>>> getPropertyValueCalls = new Dictionary<CodeTypeDeclaration, Dictionary<string, List<CodeMethodInvokeExpression>>>();
		private Dictionary<CodeTypeDeclaration, Dictionary<string, List<CodeMethodInvokeExpression>>> getMethodCalls = new Dictionary<CodeTypeDeclaration, Dictionary<string, List<CodeMethodInvokeExpression>>>();
		private List<CodeMethodInvokeExpression> allMethodCalls = new List<CodeMethodInvokeExpression>(100);

		private StringBuilder sbld = new StringBuilder();
		private CodeNamespace mainNs = new CodeNamespace("Keysharp.CompiledMain");
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
		}

		internal void Init()
		{
			ErrorStdOut = false;
			MaxThreadsPerHotkey = 1;
			MaxThreadsTotal = 10;
			HotExprTimeout = 1000;
			MaxThreadsBuffer = false;
			SuspendExempt = false;
			HotstringDefinition.hsSuspendExempt = false;
			preloadedDlls.Clear();
			//NoEnv = NoEnvDefault;
			NoTrayIcon = NoTrayIconDefault;
			Persistent = PersistentDefault;
			SingleInstance = eScriptInstance.Force;
			WinActivateForce = WinActivateForceDefault;
			HotstringNoMouse = false;
			HotstringEndChars = string.Empty;
			HotstringNewOptions = string.Empty;
			//LTrimForced = false;
			IfWinActive_WinTitle = string.Empty;
			IfWinActive_WinText = string.Empty;
			IfWinExist_WinTitle = string.Empty;
			IfWinExist_WinText = string.Empty;
			IfWinNotActive_WinTitle = string.Empty;
			IfWinNotActive_WinText = string.Empty;
			IfWinNotExist_WinTitle = string.Empty;
			IfWinNotExist_WinText = string.Empty;
			includes.Clear();
			allVars.Clear();
			methods.Clear();
			properties.Clear();
			typeStack.Clear();
			staticFuncVars.Clear();
			setPropertyValueCalls.Clear();
			getPropertyValueCalls.Clear();
			getMethodCalls.Clear();
			allMethodCalls.Clear();
			initial = new CodeStatementCollection();
			prepend = new CodeStatementCollection();
			mainNs = new CodeNamespace("Keysharp.CompiledMain");
			main = new CodeMemberMethod()
			{
				Attributes = MemberAttributes.Public | MemberAttributes.Static,
				Name = "Main"
			};
			main.ReturnType = new CodeTypeReference(typeof(int));
			_ = main.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string[]), "args"));
			_ = main.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(STAThreadAttribute))));
			targetClass = AddType(mainClassName);
			_ = targetClass.Members.Add(main);
		}

		private CodeTypeDeclaration AddType(string typename)
		{
			var ctd = new CodeTypeDeclaration(typename)
			{
				IsClass = true,
				//IsPartial = true,
				TypeAttributes = TypeAttributes.Public
			};
			typeStack.Push(ctd);

			if (typename == mainClassName)
				_ = mainNs.Types.Add(ctd);
			else
				_ = targetClass.Members.Add(ctd);

			methods[ctd] = new Dictionary<string, CodeMemberMethod>();
			properties[ctd] = new Dictionary<string, CodeMemberProperty>();
			allVars[ctd] = new Dictionary<string, SortedDictionary<string, CodeExpression>>();
			staticFuncVars[ctd] = new Stack<Dictionary<string, CodeExpression>>();
			setPropertyValueCalls[ctd] = new Dictionary<string, List<CodeMethodInvokeExpression>>();
			getPropertyValueCalls[ctd] = new Dictionary<string, List<CodeMethodInvokeExpression>>();
			getMethodCalls[ctd] = new Dictionary<string, List<CodeMethodInvokeExpression>>();
			return ctd;
		}

		private bool VarExistsAtCurrentOrParentScope(CodeTypeDeclaration currentType, string currentScope, string varName)
		{
			if (allVars.TryGetValue(currentType, out var typeFuncs))
			{
				foreach (CodeTypeMember typemember in currentType.Members)//First, see if the type contains the variable.
				{
					if (typemember is CodeSnippetTypeMember ctsm && ctsm.Name == varName)
						return true;
				}

				if (typeFuncs.TryGetValue(currentScope, out var scopeVars))//The type didn't contain the variable, so see if the local function scope contains it.
				{
					if (scopeVars.ContainsKey(varName))
						return true;
				}
			}

			return false;
		}

		private bool TypeExistsAtCurrentOrParentScope(CodeTypeDeclaration currentType, string currentScope, string varName)
		{
			foreach (CodeTypeDeclaration type in mainNs.Types)
				if (type.Name == varName)
					return true;

			foreach (CodeTypeMember type in targetClass.Members)//Nested types beyond one level are not supported, so this should handle all cases.
				if (type is CodeTypeDeclaration ctd)
					if (ctd.Name == varName)
						return true;

			return false;
		}

		private CodeTypeDeclaration FindUserDefinedType(string typeName)
		{
			foreach (CodeTypeMember type in targetClass.Members)
				if (type is CodeTypeDeclaration ctd)
					if (ctd.Name == typeName)
						return ctd;

			return null;
		}

		private bool IsUserDefinedType(string typeName)
		{
			foreach (CodeTypeMember type in targetClass.Members)
				if (type is CodeTypeDeclaration ctd)
					if (ctd.Name == typeName)
						return true;

			return false;
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
			mainNs.Imports.Add(new CodeNamespaceImport("System"));
			mainNs.Imports.Add(new CodeNamespaceImport("System"));
			mainNs.Imports.Add(new CodeNamespaceImport("System.Collections"));
			mainNs.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
			mainNs.Imports.Add(new CodeNamespaceImport("System.Data"));
			mainNs.Imports.Add(new CodeNamespaceImport("System.IO"));
			mainNs.Imports.Add(new CodeNamespaceImport("System.Reflection"));
			mainNs.Imports.Add(new CodeNamespaceImport("System.Runtime.InteropServices"));
			mainNs.Imports.Add(new CodeNamespaceImport("System.Text"));
			mainNs.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
			mainNs.Imports.Add(new CodeNamespaceImport("System.Windows.Forms"));
			mainNs.Imports.Add(new CodeNamespaceImport("Keysharp.Core"));
			mainNs.Imports.Add(new CodeNamespaceImport("Keysharp.Scripting"));
			mainNs.Imports.Add(new CodeNamespaceImport("Array = Keysharp.Core.Array"));
			mainNs.Imports.Add(new CodeNamespaceImport("Buffer = Keysharp.Core.Buffer"));
			_ = unit.Namespaces.Add(mainNs);
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
			foreach (var typeMethods in methods)
				typeMethods.Value.Values.Where(meth =>
											   meth.ReturnType.BaseType != "System.Void"
											   && !(meth.Statements.Cast<CodeStatement>().LastOrDefault() is CodeMethodReturnStatement)
											  ).ToList().ForEach(meth2 =>
													  meth2.Statements.Add(new CodeMethodReturnStatement(
															  new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Empty"))));

			//Find every call to SetPropertyValue() and determine if it's actually setting it for a type, in which case it's setting a static member of a type, and thus needs to be converted to SetPropertyValueT().
			foreach (var cmietype in setPropertyValueCalls)
			{
				foreach (var cmietypefunc in cmietype.Value)
				{
					foreach (var cmie in cmietypefunc.Value)
					{
						if (cmie.Parameters[0] is CodeVariableReferenceExpression cvre)
						{
							var name = cvre.VariableName;

							if (!VarExistsAtCurrentOrParentScope(cmietype.Key, cmietypefunc.Key, name)
									&& TypeExistsAtCurrentOrParentScope(cmietype.Key, cmietypefunc.Key, name))
							{
								cmie.Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(Script)), "SetStaticMemberValueT");
								cmie.Method.TypeArguments.Add(name);
								cmie.Parameters.RemoveAt(0);
							}
						}
					}
				}
			}

			//Do the same for all calls to GetPropertyValue().
			foreach (var cmietype in getPropertyValueCalls)
			{
				foreach (var cmietypefunc in cmietype.Value)
				{
					foreach (var cmie in cmietypefunc.Value)
					{
						if (cmie.Parameters[0] is CodeVariableReferenceExpression cvre)
						{
							var name = cvre.VariableName;

							if (!VarExistsAtCurrentOrParentScope(cmietype.Key, cmietypefunc.Key, name)
									&& TypeExistsAtCurrentOrParentScope(cmietype.Key, cmietypefunc.Key, name))
							{
								cmie.Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(Script)), "GetStaticMemberValueT");
								cmie.Method.TypeArguments.Add(name);
								cmie.Parameters.RemoveAt(0);
							}
						}
					}
				}
			}

			//Do the same for all calls to GetMethodOrProperty().
			foreach (var cmietype in getMethodCalls)
			{
				foreach (var cmietypefunc in cmietype.Value)
				{
					foreach (var cmie in cmietypefunc.Value)
					{
						if (cmie.Parameters[0] is CodeVariableReferenceExpression cvre)
						{
							var name = cvre.VariableName;

							if (!VarExistsAtCurrentOrParentScope(cmietype.Key, cmietypefunc.Key, name)
									&& TypeExistsAtCurrentOrParentScope(cmietype.Key, cmietypefunc.Key, name))
							{
								cmie.Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(Script)), "GetStaticMethodT");
								cmie.Method.TypeArguments.Add(name);
								cmie.Parameters.RemoveAt(0);
							}
						}
					}
				}
			}

			foreach (var cmie in allMethodCalls)
			{
				if (IsUserDefinedType(cmie.Method.MethodName))
				{
					cmie.Method = new CodeMethodReferenceExpression(new CodeSnippetExpression(cmie.Method.MethodName), "Call");
				}
			}

			while (invokes.Count != 0)
				StdLib();

			foreach (var typeMethods in methods)
			{
				CodeMemberMethod newmeth = null, callmeth = null;

				if (typeMethods.Key != targetClass)
				{
					foreach (var method in typeMethods.Value.Values)
					{
						if (newmeth == null && string.Compare(method.Name, "__New", true) == 0)//__New() and Call() have already been added.
							newmeth = method;
						else if (callmeth == null && string.Compare(method.Name, "Call", true) == 0)
							callmeth = method;
						else if (string.Compare(method.Name, "__Init", true) == 0)
						{
						}
						else
							_ = typeMethods.Key.Members.Add(method);

						if (string.Compare(method.Name, "__Delete", true) == 0)
						{
							_ = typeMethods.Key.Members.Add(new CodeSnippetTypeMember($"\t\t\t~{typeMethods.Key.Name}() {{ __Delete(); }}"));
						}
						else if (string.Compare(method.Name, "__Enum", true) == 0)
						{
							var getEnumMeth = new CodeMemberMethod();
							getEnumMeth.Name = "IEnumerable.GetEnumerator";
							getEnumMeth.Attributes = MemberAttributes.Final;
							getEnumMeth.ReturnType = new CodeTypeReference("IEnumerator");
							getEnumMeth.Statements.Add(new CodeSnippetExpression("return MakeBaseEnumerator(__Enum())"));
							typeMethods.Key.Members.Add(getEnumMeth);
							var paramVal = 1;

							if (method.Parameters.Count > 0)
							{
								var methParam = method.Parameters[0];
								var val = methParam.Name.ParseInt(false);

								if (val.HasValue && val.Value > 0)
									paramVal = val.Value;

								method.Parameters.Clear();
							}

							var leftParen = paramVal > 1 ? "(" : "";
							var rightParen = paramVal > 1 ? ")" : "";
							var objTypes = string.Join(',', Enumerable.Repeat("object", paramVal).ToArray());
							var baseTypeStr = $"IEnumerable<{leftParen}{objTypes}{rightParen}>";
							var returnTypeStr = $"IEnumerator<{leftParen}{objTypes}{rightParen}>";
							var baseCtr = new CodeTypeReference(baseTypeStr);
							var returnCtr = new CodeTypeReference(returnTypeStr);
							typeMethods.Key.BaseTypes.Add(baseCtr);
							//
							getEnumMeth = new CodeMemberMethod();
							getEnumMeth.Name = "GetEnumerator";
							getEnumMeth.Attributes = MemberAttributes.Public | MemberAttributes.Final;
							getEnumMeth.ReturnType = returnCtr;
							getEnumMeth.Statements.Add(new CodeSnippetExpression($"return ({returnTypeStr})MakeBaseEnumerator(__Enum())"));
							typeMethods.Key.Members.Add(getEnumMeth);
						}
					}

					var thisconstructor = typeMethods.Key.Members.Cast<CodeTypeMember>().FirstOrDefault(ctm => ctm is CodeConstructor) as CodeConstructor;

					if (newmeth != null)
					{
						_ = typeMethods.Key.Members.Add(newmeth);

						if (thisconstructor != null)
						{
							thisconstructor.Parameters.Clear();
							thisconstructor.Parameters.AddRange(newmeth.Parameters);
							//get param names and pass.
							var newparams = newmeth.Parameters.Cast<CodeParameterDeclarationExpression>().Select(p => p.Name).ToArray();
							var newparamnames = string.Join(',', newparams);
							_ = thisconstructor.Statements.Add(new CodeSnippetExpression($"__New({newparamnames})"));
							callmeth.Parameters.Clear();
							callmeth.Parameters.AddRange(newmeth.Parameters);
							callmeth.Statements.Clear();
							_ = callmeth.Statements.Add(new CodeSnippetExpression($"return new {typeMethods.Key.Name}({newparamnames})"));
						}
					}
					else
					{
						callmeth.Statements.Clear();
						_ = callmeth.Statements.Add(new CodeSnippetExpression($"return new {typeMethods.Key.Name}()"));
						callmeth.Parameters.Clear();
					}

					var baseType = typeMethods.Key.BaseTypes[0].BaseType;

					if (baseType != "KeysharpObject" && thisconstructor != null)
					{
						if (FindUserDefinedType(baseType) is CodeTypeDeclaration ctdbase)
						{
							if (ctdbase.Members.Cast<CodeTypeMember>().FirstOrDefault(ctm => ctm is CodeConstructor) is CodeConstructor ctm2)
							{
								var i = 0;
								thisconstructor.BaseConstructorArgs.Clear();

								for (; i < thisconstructor.Parameters.Count && i < ctm2.Parameters.Count; i++)//Iterate through all of the parameters in this class's constructor.
									_ = thisconstructor.BaseConstructorArgs.Add(new CodeSnippetExpression($"{thisconstructor.Parameters[i].Name}"));

								for (; i < ctm2.Parameters.Count; i++)//Fill whatever remains of the base constructor parameters with empty strings.
									_ = thisconstructor.BaseConstructorArgs.Add(new CodeSnippetExpression("\"\""));
							}
						}
					}
				}
				else
				{
					foreach (var method in typeMethods.Value.Values)
						_ = typeMethods.Key.Members.Add(method);
				}
			}

			foreach (var typeProperties in properties)
			{
				if (typeProperties.Key != targetClass)
				{
					foreach (var propkv in typeProperties.Value)
					{
						var prop = propkv.Value;

						if (string.Compare(prop.Name, "__Item", true) == 0)
							prop.Name = "Item";

						_ = typeProperties.Key.Members.Add(propkv.Value);
					}
				}
			}

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

			foreach (var typekv in allVars)
			{
				if (typekv.Key == targetClass)
				{
					foreach (var scopekv in typekv.Value.Where(kv => kv.Key.Length == 0))
					{
						foreach (var globalvar in scopekv.Value)
						{
							var name = globalvar.Key.Replace(ScopeVar[0], '_');
							_ = typekv.Key.Members.Add(new CodeSnippetTypeMember()
							{
								Text = $"\t\tpublic static object {name} {{ get; set; }}"
							});
						}
					}
				}
				else
				{
					foreach (var scopekv in typekv.Value.Where(kv => kv.Key.Length == 0))
					{
						CodeMemberMethod initcmm = null;

						foreach (var globalvar in scopekv.Value)
						{
							var name = globalvar.Key.Replace(ScopeVar[0], '_');
							var isstatic = globalvar.Value.UserData.Contains("isstatic");
							var init = globalvar.Value is CodeExpression ce ? Ch.CodeToString(ce) : "\"\"";
							_ = typekv.Key.Members.Add(new CodeSnippetTypeMember()
							{
								Text = isstatic ? $"\t\t\tpublic static object {name} {{ get; set; }} = {init};"
									   : $"\t\t\tpublic object {name} {{ get; set; }}"
							});

							if (init.Length > 0 && !isstatic)
							{
								if (initcmm == null)
								{
									foreach (CodeTypeMember ctm in typekv.Key.Members)
									{
										if (ctm is CodeMemberMethod cmm && cmm.Name == "__Init")
										{
											initcmm = cmm;
											break;
										}
									}
								}

								if (initcmm != null)
									_ = initcmm.Statements.Add(new CodeSnippetExpression($"{name} = {init}"));
							}
						}
					}
				}
			}

			return GenerateCompileUnit();
		}
	}
}