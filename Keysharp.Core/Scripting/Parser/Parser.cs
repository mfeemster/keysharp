using static Keysharp.Scripting.Keywords;
using slmd = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.CodeDom.CodeMethodInvokeExpression>>;
using tsmd = System.Collections.Generic.Dictionary<System.CodeDom.CodeTypeDeclaration, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.CodeDom.CodeMethodInvokeExpression>>>;

namespace Keysharp.Scripting
{
	public partial class Parser : ICodeParser
	{
		public static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

		internal const string scopeChar = "_";
		internal const string varsPropertyName = "Vars";

		/// <summary>
		/// The order of these is critically important. Items that start with the same character must go from longest to shortest.
		/// </summary>
		internal static FrozenSet<string> contExprOperators = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			",",
			//"%",
			".=",
			".",
			"??",
			"?",
			"+=",
			"+",
			"-=",
			"-",
			"**",
			"*=",
			"*",
			"!==",
			"!=",
			"!",
			"~",
			"&&",
			"&=",
			"&",
			"^=",
			"^",
			"||",
			"|=",
			"|",
			"//=",
			"//",
			"/=",
			"/",
			"<<=",
			"<<",
			">>>=",
			">>>",
			">>=",
			">>",
			"~=",
			"<=",
			"<",
			">=",
			">",
			":=",
			"==",
			"=>",
			"=",
			":",
			"["
		} .ToFrozenSet(StringComparer.InvariantCultureIgnoreCase);

		internal static List<string> contExprOperatorsList = contExprOperators.ToList();
		internal static CodePrimitiveExpression emptyStringPrimitive = new CodePrimitiveExpression("");

		/// <summary>
		/// The order of these is critically important. Items that start with the same character must go from longest to shortest.
		/// </summary>
		internal static FrozenSet<string> exprVerbalOperators = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)

		{
			"is",
			"and",
			"not",
			"or"
		} .ToFrozenSet(StringComparer.InvariantCultureIgnoreCase);

		internal static FrozenSet<string> flowOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			FlowBreak,
			FlowContinue,
			FlowCase,
			FlowClass,
			FlowDefault,
			FlowFor,
			FlowElse,
			FlowGosub,
			FlowGoto,
			FlowIf,
			FlowLoop,
			FlowReturn,
			FlowWhile,
			FunctionLocal,
			FunctionGlobal,
			//FunctionStatic,//Can't have this in here, else it throws off IsFlowOperator().
			FlowTry,
			FlowCatch,
			FlowFinally,
			FlowUntil,
			FlowSwitch,
			//FlowGet,
			//FlowSet,
			Throw
		} .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

		internal static FrozenSet<string> keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			AndTxt,
			OrTxt,
			NotTxt,
			TrueTxt,
			FalseTxt,
			NullTxt,
			IsTxt,
			FlowBreak,
			FlowContinue,
			FlowCase,
			FlowClass,
			FlowDefault,
			FlowFor,
			FlowElse,
			FlowExtends,
			FlowGosub,
			FlowGoto,
			FlowIf,
			FlowLoop,
			FlowReturn,
			FlowWhile,
			FunctionLocal,
			FunctionGlobal,
			FunctionStatic,
			FlowTry,
			FlowCatch,
			FlowFinally,
			FlowUntil,
			FlowSwitch,
			Throw
		} .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

		internal static FrozenSet<string> nonContExprOperators = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			"++",
			"--"
		} .ToFrozenSet(StringComparer.InvariantCultureIgnoreCase);

		internal static List<string> nonContExprOperatorsList = nonContExprOperators.ToList();
		internal static CodePrimitiveExpression nullPrimitive = new CodePrimitiveExpression(null);

		internal static FrozenSet<string> propKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			FlowGet,
			FlowSet
		} .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

		internal bool ErrorStdOut;
		internal CodeStatementCollection initial = new CodeStatementCollection();
		internal string name = string.Empty;
		internal bool NoTrayIcon;
		internal bool Persistent;
		private const string args = "args";
		private const string initParams = "initparams";
		private const string mainClassName = "program";
		private const string mainScope = "";
		private static char[] directiveDelims = Spaces.Concat(new char[] { Multicast });

		private static FrozenSet<string> persistentTerms = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			"settimer",
			"menu",
			"hotkey",
			"hotstring",
			"onmessage",
			"onclipboardchange",
			"gui",
			"persistent"
		} .ToFrozenSet(StringComparer.InvariantCultureIgnoreCase);

		private Stack<bool> allGlobalVars = new Stack<bool>();
		private tsmd allMethodCalls = new tsmd();
		private Stack<bool> allStaticVars = new Stack<bool>();
		private Dictionary<CodeTypeDeclaration, Dictionary<string, SortedDictionary<string, CodeExpression>>> allVars = new Dictionary<CodeTypeDeclaration, Dictionary<string, SortedDictionary<string, CodeExpression>>>();
		private CodeAttributeDeclarationCollection assemblyAttributes = new CodeAttributeDeclarationCollection();
		private HashSet<CodeSnippetExpression> assignSnippets = new ();
		private bool blockOpen;
		private Stack<CodeBlock> blocks = new ();
		private uint caseCount;
		private CompilerHelper Ch;
		private List<CodeLine> codeLines = new List<CodeLine>();
		private Stack<List<string>> currentFuncParams = new Stack<List<string>>();
		private Stack<CodeStatementCollection> elses = new ();
		private Stack<HashSet<string>> excCatchVars = new Stack<HashSet<string>>();
		private uint exCount;
		private string fileName;
		private tsmd getMethodCalls = new tsmd();
		private tsmd getPropertyValueCalls = new tsmd();
		private Stack<List<string>> globalFuncVars = new Stack<List<string>>();
		private Dictionary<CodeGotoStatement, CodeBlock> gotos = new Dictionary<CodeGotoStatement, CodeBlock>();
		private int internalID;
		private List<CodeMethodInvokeExpression> invokes = new List<CodeMethodInvokeExpression>();
		private int labelCount;
		private string lastHotkeyFunc = "";
		private string lastHotstringFunc = "";
		private Stack<List<string>> localFuncVars = new Stack<List<string>>();

		private CodeMemberMethod main = new CodeMemberMethod()
		{
			Attributes = MemberAttributes.Public | MemberAttributes.Static,
			Name = "Main"
		};

		private CodeNamespace mainNs = new CodeNamespace("Keysharp.CompiledMain");
		private bool memberVarsStatic = false;
		private Dictionary<CodeTypeDeclaration, Dictionary<string, CodeMemberMethod>> methods = new Dictionary<CodeTypeDeclaration, Dictionary<string, CodeMemberMethod>>();
		private char[] ops = new char[] { Equal, Not, Greater, Less };
		private CodeStatementCollection parent;
		private CodeBlock parentBlock;
		private CodeStatementCollection prepend = new CodeStatementCollection();
		private Dictionary<CodeTypeDeclaration, Dictionary<string, List<CodeMemberProperty>>> properties = new Dictionary<CodeTypeDeclaration, Dictionary<string, List<CodeMemberProperty>>>();
		private tsmd setPropertyValueCalls = new tsmd();
		private Stack<CodeBlock> singleLoops = new ();
		private List<CodeMethodInvokeExpression> stackedHotkeys = new List<CodeMethodInvokeExpression>();
		private List<CodeMethodInvokeExpression> stackedHotstrings = new List<CodeMethodInvokeExpression>();
		private Dictionary<CodeTypeDeclaration, Stack<Dictionary<string, CodeExpression>>> staticFuncVars = new Dictionary<CodeTypeDeclaration, Stack<Dictionary<string, CodeExpression>>>();
		private uint switchCount;
		private Stack<CodeSwitchStatement> switches = new ();
		private CodeTypeDeclaration targetClass;
		private Stack<CodeTernaryOperatorExpression> ternaries = new ();
		private uint tryCount;
		private Stack<CodeTypeDeclaration> typeStack = new Stack<CodeTypeDeclaration>();

		public Parser(CompilerHelper ch)
		{
			Ch = ch;
			main.ReturnType = new CodeTypeReference(typeof(int));
			_ = main.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string[]), "args"));
			_ = main.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(STAThreadAttribute))));
			targetClass = AddType(mainClassName);
			_ = targetClass.Members.Add(main);
		}

		public static string GetKeywords() => string.Join(' ', keywords);

		public static bool IsTypeOrBase(Type t1, string t2)
		{
			while (t1 != null)
			{
				if (string.Compare(t1.Name, t2, true) == 0)
					return true;

				t1 = t1.BaseType;
			}

			return false;
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
			AddAssemblyAttribute(typeof(AssemblyBuildVersionAttribute), Accessors.A_AhkVersion);
			unit.AssemblyCustomAttributes.AddRange(assemblyAttributes);
			assemblyAttributes.Clear();

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
			var cse = new CodeSnippetExpression("ErrorOccurred(kserr)");
			var pushcse = new CodeSnippetExpression("var (__pushed, __btv) = Keysharp.Core.Common.Threading.Threads.BeginThread()");
			var msg = new CodeSnippetExpression("MsgBox(\"Uncaught Keysharp exception:\\r\\n\" + kserr, $\"{Accessors.A_ScriptName}: Unhandled exception\", \"iconx\")");
			var popcse = new CodeSnippetExpression("Keysharp.Core.Common.Threading.Threads.EndThread(__pushed)");
			var ccs = new CodeConditionStatement(cse);
			_ = ccs.TrueStatements.Add(pushcse);
			_ = ccs.TrueStatements.Add(msg);
			_ = ccs.TrueStatements.Add(popcse);
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
						var (__pushed, __btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
						MsgBox(""Uncaught Keysharp exception:\r\n"" + kserr, $""{Accessors.A_ScriptName}: Unhandled exception"", ""iconx"");
						Keysharp.Core.Common.Threading.Threads.EndThread(__pushed);
					}
				}
				else
				{
					var (__pushed, __btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
					MsgBox(""Uncaught exception:\r\n"" + ""Message: "" + ex.Message + ""\r\nStack: "" + ex.StackTrace, $""{Accessors.A_ScriptName}: Unhandled exception"", ""iconx"");
					Keysharp.Core.Common.Threading.Threads.EndThread(__pushed);
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

			//It's hard to properly set return statements during parsing, so ensure their correctness here.
			foreach (var typeMethods in methods)
				typeMethods.Value.Values.Where(meth =>
											   meth.ReturnType.BaseType != "System.Void"
											   && !(meth.Statements.Cast<CodeStatement>().LastOrDefault() is CodeMethodReturnStatement)
											  ).ToList().ForEach(meth2 =>
													  meth2.Statements.Add(new CodeMethodReturnStatement(emptyStringPrimitive)));

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

			var createDummyRef = false;
			var tsVar = DateTime.Now.ToString("__MMddyyyyHHmmssfffffff");
			var ctrObjectArray = new CodeTypeReference(typeof(object[]));
			var argsSnippet = new CodeSnippetExpression("args");

			while (targetClass.Members.Cast<CodeTypeMember>().Any(ctm => ctm.Name == tsVar))
				tsVar = "_" + tsVar;

			foreach (var cmietype in allMethodCalls)
			{
				foreach (var cmietypefunc in cmietype.Value)
				{
					foreach (var cmie in cmietypefunc.Value)
					{
						//Handle changing myfuncobj() to myfuncobj.Call().
						if (VarExistsAtCurrentOrParentScope(cmietype.Key, cmietypefunc.Key, cmie.Method.MethodName))
						{
							var refIndexes = ParseArguments(cmie.Parameters);

							if (refIndexes.Count > 0)
							{
								var newParams = ConvertDirectParamsToInvoke(cmie.Parameters);
								cmie.Parameters.Clear();
								cmie.Parameters.AddRange(newParams);
								cmie.Method = new CodeMethodReferenceExpression(new CodeSnippetExpression($"/*preventtrim*/((IFuncObj){cmie.Method.MethodName})"), "CallWithRefs");
							}
							else
								cmie.Method = new CodeMethodReferenceExpression(new CodeSnippetExpression($"/*preventtrim*/((IFuncObj){cmie.Method.MethodName})"), "Call");
						}
						else if (GetUserDefinedTypename(cmie.Method.MethodName) is string s && s.Length > 0)//Convert myclass() to myclass.Call().
						{
							cmie.Method = new CodeMethodReferenceExpression(new CodeSnippetExpression(s), "Call");
						}
						//Handle proper casing and refs for all method calls.
						else if (MethodExistsInTypeOrBase(cmietype.Key.Name, cmie.Method.MethodName) is CodeMemberMethod cmm)//It wasn't a built in method, so check user defined methods first.
						{
							var methParams = cmm.Parameters;
							cmie.Method.MethodName = cmm.Name;

							for (var i = 0; i < cmie.Parameters.Count; i++)
							{
								var cp = cmie.Parameters[i];

								if (i < methParams.Count)
								{
									if (cp is CodePrimitiveExpression cpe && cpe.Value == null)
									{
										var mp = methParams[i];

										if (mp.Direction == FieldDirection.Ref)
										{
											cmie.Parameters[i] = new CodeSnippetExpression($"ref {tsVar}");
											createDummyRef = true;
										}
									}
								}
							}

							//If they supplied less than the required number of arguments, fill them in and take special consideration for refs.
							if (methParams.Count > cmie.Parameters.Count)
							{
								var newParams = new List<CodeExpression>(methParams.Count);

								for (var i = cmie.Parameters.Count; i < methParams.Count; i++)
								{
									var mp = methParams[i];

									if (!mp.CustomAttributes.Cast<CodeAttributeDeclaration>().Any(cad => cad.Name == "Optional" || cad.Name == "System.ParamArrayAttribute"))//Ignore variadic object[] parameters.
									{
										if (mp.Direction == FieldDirection.Ref)
										{
											newParams.Add(new CodeSnippetExpression($"ref {tsVar}"));
											createDummyRef = true;
										}
										else
											newParams.Add(nullPrimitive);
									}
								}

								cmie.Parameters.AddRange(newParams.ToArray());
							}
						}
						else if (Reflections.FindBuiltInMethod(cmie.Method.MethodName, -1/*Don't care about paramCount here, just need the name.*/) is MethodPropertyHolder mph && mph.mi != null) //This will find the first built in method with this name, but they are all cased the same, so it should be ok.
						{
							cmie.Method.MethodName = mph.mi.Name;

							if (mph.mi.IsStatic)
								cmie.Method.TargetObject = new CodeTypeReferenceExpression(mph.mi.DeclaringType);

							if (Reflections.FindBuiltInMethod(cmie.Method.MethodName, cmie.Parameters.Count) is MethodPropertyHolder mph2 && mph2.mi != null)//We know the method exists, so try to find the exact match for the number of parameters specified.
							{
								var methParams = mph2.mi.GetParameters();

								for (var i = 0; i < cmie.Parameters.Count; i++)
								{
									var cp = cmie.Parameters[i];

									if (i < methParams.Length)
									{
										if (cp is CodePrimitiveExpression cpe && cpe.Value == null)
										{
											var mp = methParams[i];

											if (mp.ParameterType.IsByRef && !mp.IsOut)
											{
												cmie.Parameters[i] = new CodeSnippetExpression($"ref {tsVar}");
												createDummyRef = true;
											}
										}
									}
								}

								//If they supplied less than the required number of arguments, fill them in and take special consideration for refs.
								if (methParams.Length > cmie.Parameters.Count)
								{
									var newParams = new List<CodeExpression>(methParams.Length);

									for (var i = cmie.Parameters.Count; i < methParams.Length; i++)
									{
										var mp = methParams[i];

										if (!mp.IsOptional)
										{
											if (mp.ParameterType.IsByRef && !mp.IsOut)
											{
												newParams.Add(new CodeSnippetExpression($"ref {tsVar}"));
												createDummyRef = true;
											}
											else if (mp.ParameterType != typeof(object[]))//Do not pass for variadic object[] parameters.
												newParams.Add(nullPrimitive);
										}
									}

									cmie.Parameters.AddRange(newParams.ToArray());
								}
							}
						}

						//Calling a virtual method with super will not work with regular reflection.
						//It will always resolve to the most derived class.
						//So to actually call the base, we need to replace it with a literal call to
						//base._New()
						//No other methods will ever be virtual/override so we should only ever
						//have to do this for __New().
						if (cmie.Method.MethodName == "Invoke"
								&& cmie.Parameters.Count > 0
								&& cmie.Parameters[0] is CodeMethodInvokeExpression cmieGetProperty
								&& cmieGetProperty.Method.MethodName == "GetMethodOrProperty"
								&& cmieGetProperty.Parameters.Count > 1
								&& cmieGetProperty.Parameters[0] is CodeVariableReferenceExpression cvre
								&& cvre.VariableName == "super"
								&& cmieGetProperty.Parameters[1] is CodePrimitiveExpression cpe2
								&& cpe2.Value.ToString() == "__New")
						{
							cmie.Method = new CodeMethodReferenceExpression(new CodeSnippetExpression("base"), "__New");

							if (cmie.Parameters.Count == 1)
							{
								cmie.Parameters.Clear();
								_ = cmie.Parameters.Add(argsSnippet);
							}
							else
							{
								var superParams = cmie.Parameters.Cast<CodeExpression>().Skip(1).ToArray();
								cmie.Parameters.Clear();
								cmie.Parameters.AddRange(superParams);
							}
						}
					}
				}
			}

			if (createDummyRef)
			{
				_ = targetClass.Members.Add(new CodeSnippetTypeMember()
				{
					Name = name,
					Text = $"\t\tpublic static object {tsVar};"
				});
			}

			var ctrpaa = new CodeTypeReference(typeof(System.ParamArrayAttribute));
			var cad = new CodeAttributeDeclaration(ctrpaa);
			var cdpeArgs = new CodeParameterDeclarationExpression(typeof(object[]), "args");
			_ = cdpeArgs.CustomAttributes.Add(cad);

			foreach (var typeMethods in methods)
			{
				CodeMemberMethod newmeth = null, callmeth = null;

				if (typeMethods.Key != targetClass)
				{
					var baseType = typeMethods.Key.BaseTypes[0].BaseType;
					var origNewParams = new List<CodeParameterDeclarationExpression>();

					foreach (var method in typeMethods.Value.Values)
					{
						if (newmeth == null && string.Compare(method.Name, "__New", true) == 0)//__New() and Call() have already been added.
						{
							method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
							method.Name = "__New";//Ensure it's properly cased.

							for (var i = method.Parameters.Count - 1; i >= 0; i--)
								method.Statements.Insert(0, new CodeExpressionStatement(new CodeSnippetExpression($"object {method.Parameters[i].Name} = args.Length > {i} ? args[{i}] : null")));

							origNewParams = method.Parameters.Cast<CodeParameterDeclarationExpression>().ToList();
							method.Parameters.Clear();
							method.Parameters.Add(cdpeArgs);
							newmeth = method;
						}
						else if (callmeth == null && string.Compare(method.Name, "Call", true) == 0)
						{
							method.Name = "Call";
							callmeth = method;
						}
						else if (string.Compare(method.Name, "__Init", true) == 0)
						{
							method.Name = "__Init";
						}
						else
							_ = typeMethods.Key.Members.Add(method);

						if (string.Compare(method.Name, "__Delete", true) == 0)
						{
							method.Name = "__Delete";
							_ = typeMethods.Key.Members.Add(new CodeSnippetTypeMember($"\t\t\t~{typeMethods.Key.Name}() {{ __Delete(); }}") { Name = typeMethods.Key.Name });
						}
						else if (string.Compare(method.Name, "__Enum", true) == 0)
						{
							var getEnumMeth = new CodeMemberMethod();
							method.Name = "__Enum";
							getEnumMeth.Name = "IEnumerable.GetEnumerator";
							getEnumMeth.Attributes = MemberAttributes.Final;
							getEnumMeth.ReturnType = new CodeTypeReference("IEnumerator");
							_ = getEnumMeth.Statements.Add(new CodeSnippetExpression("return MakeBaseEnumerator(__Enum())"));
							_ = typeMethods.Key.Members.Add(getEnumMeth);
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
							_ = typeMethods.Key.BaseTypes.Add(baseCtr);
							//
							getEnumMeth = new CodeMemberMethod
							{
								Name = "GetEnumerator",
								Attributes = MemberAttributes.Public | MemberAttributes.Final,
								ReturnType = returnCtr
							};
							_ = getEnumMeth.Statements.Add(new CodeSnippetExpression($"return ({returnTypeStr})MakeBaseEnumerator(__Enum())"));
							_ = typeMethods.Key.Members.Add(getEnumMeth);
						}
					}

					var thisconstructor = typeMethods.Key.Members.Cast<CodeTypeMember>().FirstOrDefault(ctm => ctm is CodeConstructor) as CodeConstructor;

					if (newmeth != null)
					{
						_ = typeMethods.Key.Members.Add(newmeth);
						newmeth.Statements.Insert(0, new CodeExpressionStatement(new CodeSnippetExpression("__Init()")));
					}
					else
						_ = thisconstructor.Statements.Add(new CodeSnippetExpression("__Init()"));

					//The lines relating to args and __New() work whether any params were declared or not.
					if (thisconstructor != null)
					{
						var ctorParams = origNewParams.Select(p => p.Name).ToArray();
						var newparamnames = string.Join(", ", ctorParams);

						if (baseType == "KeysharpObject")
							_ = thisconstructor.Statements.Add(new CodeSnippetExpression($"__New(args)"));

						thisconstructor.Parameters.Clear();
						thisconstructor.Parameters.Add(cdpeArgs);
						//Get param names and pass.
						callmeth.Parameters.Clear();
						callmeth.Parameters.AddRange(new CodeParameterDeclarationExpressionCollection(origNewParams.ToArray()));
						callmeth.Statements.Clear();
						_ = callmeth.Statements.Add(new CodeSnippetExpression($"return new {typeMethods.Key.Name}({newparamnames})"));
					}

					if (baseType != "KeysharpObject" && thisconstructor != null)
					{
						var rawBaseTypeName = "";

						if (FindUserDefinedType(baseType) is CodeTypeDeclaration ctdbase)
						{
							rawBaseTypeName = ctdbase.Name;

							if (ctdbase.Members.Cast<CodeTypeMember>().FirstOrDefault(ctm => ctm is CodeConstructor) is CodeConstructor ctm2)
							{
								thisconstructor.BaseConstructorArgs.Clear();
								thisconstructor.BaseConstructorArgs.Add(argsSnippet);
								//var i = 0;
								//for (; i < thisconstructor.Parameters.Count && i < ctm2.Parameters.Count; i++)//Iterate through all of the parameters in this class's constructor.
								//  _ = thisconstructor.BaseConstructorArgs.Add(new CodeSnippetExpression($"{thisconstructor.Parameters[i].Name}"));
								//for (; i < ctm2.Parameters.Count; i++)//Fill whatever remains of the base constructor parameters with nulls.
								//  _ = thisconstructor.BaseConstructorArgs.Add(nullPrimitive);
							}
						}
						else//Try built in types.
						{
							foreach (var typekv in Reflections.typeToStringMethods)
							{
								if (string.Compare(typekv.Key.Name, baseType, true) == 0)
								{
									rawBaseTypeName = typekv.Key.Name;
									var ctors = typekv.Key.GetConstructors();

									foreach (var ctor in ctors)
									{
										var ctorparams = ctor.GetParameters();//Built in types will always just have one constructor, such as Array and Map because users don't control those.

										if (ctorparams.Length > 0)
										{
											thisconstructor.BaseConstructorArgs.Clear();
											thisconstructor.BaseConstructorArgs.Add(argsSnippet);
											//var i = 0;
											//for (; i < thisconstructor.Parameters.Count && i < ctorparams.Length; i++)//Iterate through all of the parameters in this class's constructor.
											//  _ = thisconstructor.BaseConstructorArgs.Add(new CodeSnippetExpression($"{thisconstructor.Parameters[i].Name}"));
											//for (; i < ctorparams.Length; i++)//Fill whatever remains of the base constructor parameters with empty strings.
											//  _ = thisconstructor.BaseConstructorArgs.Add(nullPrimitive);
											break;
										}
									}

									break;
								}
							}
						}

						if (rawBaseTypeName.Length != 0)
							typeMethods.Key.BaseTypes[0].BaseType = rawBaseTypeName;//Make sure it's properly cased, so we can, for example, derive from map or Map.
					}
				}
				else
				{
					foreach (var method in typeMethods.Value.Values)
						_ = typeMethods.Key.Members.Add(method);
				}
			}

			//Must explicitly mark all index operators as override if they exist in a base, and the parameter count matches.
			//This is because in the function IndexAt(), Array[] and Map[] are called directly, after doing a cast check.
			//First go through all user defined properties and change the name __Item to Item.
			foreach (var typeProperties in properties)
			{
				if (typeProperties.Key != targetClass)
				{
					foreach (var propkv in typeProperties.Value.ToArray())
					{
						var propList = propkv.Value;

						foreach (var prop in propList)
						{
							if (string.Compare(prop.Name, "__Item", true) == 0)
							{
								prop.Name = "Item";
								//prop.Attributes = MemberAttributes.Public;//Make virtual at a minimum, which might get converted to override below.
								_ = typeProperties.Value.Remove("__item"); //Was stored as lowercase.
								typeProperties.Value.GetOrAdd("Item").Add(prop);
							}
						}
					}
				}
			}

			//Now go through and find which ones need to be declared as override.
			foreach (var typeProperties in properties)
			{
				if (typeProperties.Key != targetClass)
				{
					foreach (var propkv in typeProperties.Value)
					{
						var propList = propkv.Value;

						foreach (var prop in propList)
						{
							//if (typeProperties.Key.BaseTypes.Count > 0)
							//  if (PropExistsInTypeOrBase(typeProperties.Key.BaseTypes[0].BaseType, "Item", prop.Parameters.Count))
							//      prop.Attributes = MemberAttributes.Public | MemberAttributes.Override;
							_ = typeProperties.Key.Members.Add(prop);
						}
					}
				}
			}

			//Handle gotos inside of loops. Note that breaks are handled as they're parsed instead of being handled here.
			foreach (var gkv in gotos)
			{
				var gotoIndex = gkv.Value.Statements.IndexOf(gkv.Key);
				var gotoLoopDepth = GotoLoopDepth(gkv.Value, gkv.Key.Label);
				var pop = new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Pop);

				for (var i = 0; i < gotoLoopDepth; i++)
					gkv.Value.Statements.Insert(gotoIndex, pop);
			}

			foreach (var assign in assignSnippets)//Despite having to compile code again, this took < 1ms in debug mode for over 100 assignments, so it shouldn't matter.
				ReevaluateSnippet(assign);

			return unit;
		}

		public CodeCompileUnit Parse(TextReader codeStream) => Parse(codeStream, string.Empty);

		public CodeCompileUnit Parse(TextReader codeStream, string nm)
		{
			name = nm;
			var reader = new PreReader(this);
			codeLines = reader.Read(codeStream, name);
#if DEBUG
			//File.WriteAllLines("./finalscriptcode.txt", codeLines.Select((cl) => $"{cl.LineNumber}: {cl.Code}"));
#endif
			Statements();

			if (!NoTrayIcon)
				_ = initial.Add(new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.CreateTrayMenu));

			_ = initial.Add(new CodeSnippetExpression("Keysharp.Core.Env.HandleCommandLineParams(args)"));
			var inst = (CodeMethodInvokeExpression)InternalMethods.HandleSingleInstance;
			_ = inst.Parameters.Add(new CodeSnippetExpression("name"));
			//_ = inst.Parameters.Add(new CodeSnippetExpression($"eScriptInstance.{(name == "*" ? "Off" : SingleInstance)}"));
			_ = inst.Parameters.Add(new CodeSnippetExpression($"eScriptInstance.{reader.SingleInstance}"));
			_ = initial.Add(new CodeExpressionStatement(inst));
			_ = initial.Add(new CodeSnippetExpression("Keysharp.Scripting.Script.SetName(name)"));
			_ = initial.Add(new CodeSnippetExpression("Keysharp.Scripting.Script.Variables.InitGlobalVars()"));

			foreach (var (p, s) in reader.PreloadedDlls)//Add after InitGlobalVars() call above, because the statements will be added in reverse order.
			{
				var cmie = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Keysharp.Scripting.Variables"), "AddPreLoadedDll");
				_ = cmie.Parameters.Add(new CodePrimitiveExpression(p));
				_ = cmie.Parameters.Add(new CodePrimitiveExpression(s));
				initial.Add(new CodeExpressionStatement(cmie));
			}

			var namevar = new CodeVariableDeclarationStatement("System.String", "name", new CodePrimitiveExpression(name));
			_ = initial.Add(namevar);
			//_ = initial.Add(new CodeSnippetExpression("Assembly assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(\"C:\\\\Program Files\\\\Keysharp\\\\Keysharp.Core.dll\")"));
			var userMainMethod = new CodeMemberMethod()
			{
				Attributes = MemberAttributes.Public | MemberAttributes.Static,
				Name = "UserMainCode",
				ReturnType = new CodeTypeReference(typeof(object))
			};
			userMainMethod.Statements.AddRange(main.Statements);
			main.Statements.Clear();
			methods.GetOrAdd(targetClass)[userMainMethod.Name] = userMainMethod;

			foreach (CodeStatement stmt in initial)
				main.Statements.Insert(0, stmt);

			foreach (var typekv in allVars)
			{
				if (typekv.Key == targetClass)//Global vars part of main program class.
				{
					foreach (var scopekv in typekv.Value.Where(kv => kv.Key.Length == 0))
					{
						foreach (var globalvar in scopekv.Value)
						{
							var name = globalvar.Key.Replace(scopeChar[0], '_');

							if (name == "this")//Never create a "this" variable inside of a class definition.
								continue;

							//var init = globalvar.Value is CodeExpression ce ? Ch.CodeToString(ce) : "\"\"";
							_ = typekv.Key.Members.Add(new CodeSnippetTypeMember()
							{
								Name = name,
								//Text = $"\t\tpublic static object {name} = {init};"
								Text = $"\t\tpublic static object {name};"
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
							var name = globalvar.Key.Replace(scopeChar[0], '_');

							if (name == "this")//Never create a "this" variable inside of a class definition.
								continue;

							var isstatic = globalvar.Value.UserData.Contains("isstatic");
							var init = globalvar.Value is CodeExpression ce ? Ch.CodeToString(ce) : "\"\"";
							_ = typekv.Key.Members.Add(new CodeSnippetTypeMember()
							{
								Name = name,
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
								{
									if (string.Compare(init, "null", true) != 0)
										_ = initcmm.Statements.Add(new CodeSnippetExpression($"{name} = {init}"));

									foreach (DictionaryEntry kv in globalvar.Value.UserData)
										if (kv.Key is CodeExpressionStatement ces2)
											_ = initcmm.Statements.Add(ces2);
								}
							}
						}
					}
				}
			}

			return GenerateCompileUnit();
		}

		internal bool InClassDefinition() => typeStack.Count > 0 && typeStack.Peek().Name != mainClassName;

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

			methods[ctd] = new Dictionary<string, CodeMemberMethod>(StringComparer.OrdinalIgnoreCase);
			properties[ctd] = new Dictionary<string, List<CodeMemberProperty>>(StringComparer.OrdinalIgnoreCase);
			allVars[ctd] = new Dictionary<string, SortedDictionary<string, CodeExpression>>(StringComparer.OrdinalIgnoreCase);
			staticFuncVars[ctd] = new Stack<Dictionary<string, CodeExpression>>();
			setPropertyValueCalls[ctd] = new slmd();
			getPropertyValueCalls[ctd] = new slmd();
			getMethodCalls[ctd] = new slmd();
			allMethodCalls[ctd] = new slmd();
			return ctd;
		}

		private void CheckPersistent(string name)
		{
			if (Persistent)
				return;

			if (persistentTerms.Contains(name))
				Persistent = true;
		}

		private CodeTypeDeclaration FindUserDefinedType(string typeName)
		{
			foreach (CodeTypeMember type in targetClass.Members)
				if (type is CodeTypeDeclaration ctd)
					if (string.Compare(ctd.Name, typeName, true) == 0)
						return ctd;

			return null;
		}

		private string GetUserDefinedTypename(string typeName)
		{
			foreach (CodeTypeMember type in targetClass.Members)
				if (type is CodeTypeDeclaration ctd)
					if (string.Compare(ctd.Name, typeName, true) == 0)
						return ctd.Name;

			return "";
		}

		private CodeMemberMethod MethodExistsInTypeOrBase(string t, string m)
		{
			if (methods.Count > 0)
			{
				while (!string.IsNullOrEmpty(t))
				{
					var typematched = false;

					foreach (var typekv in methods)
					{
						if (string.Compare(typekv.Key.Name, t, true) == 0)//Find the matching type.
						{
							typematched = true;

							if (typekv.Value.TryGetValue(m, out var cmm))//If the method existed in the type, return.
								return cmm;

							//Wasn't found in this type, so check its base. This will not check for built in types, because those can be gotten with FindBuiltInMethod().
							if (typekv.Key.BaseTypes.Count > 0)
							{
								t = typekv.Key.BaseTypes[0].BaseType;
								break;//Either the property was not found, or the base was not a built in type, so try again with base class.
							}
							else
								return null;
						}
					}

					if (!typematched)
						return null;
				}
			}

			return null;
		}

		private bool PropExistsInBuiltInClass(string baseType, string p, int paramCount)
		{
			if (Reflections.stringToTypeProperties.TryGetValue(p, out var props))
			{
				//Must iterate rather than look up because we only have the string name, not the type.
				foreach (var typekv in props)
				{
					if (string.Compare(typekv.Key.Name, baseType, true) == 0)
					{
						if (PropExistsInTypeOrBase(typekv.Key, p, paramCount))
							return true;
						else
							break;
					}
				}
			}

			return false;
		}

		private bool PropExistsInTypeOrBase(string t, string p, int paramCount)
		{
			if (properties.Count > 0)
			{
				while (!string.IsNullOrEmpty(t))
				{
					var anyFound = false;

					foreach (var typekv in properties)
					{
						if (string.Compare(typekv.Key.Name, t, true) == 0)//Find the matching type.
						{
							anyFound = true;

							if (typekv.Value.TryGetValue(p, out var tempList))//If the property existed in the type, return.
								foreach (var prop in tempList)
									if (prop.Parameters.Count == paramCount)
										return true;

							//Wasn't found in this type, so check its base.
							if (typekv.Key.BaseTypes.Count > 0)
							{
								t = typekv.Key.BaseTypes[0].BaseType;

								//The base might have been a user defined type, or a built in type. Check built in type first.
								//Ex: subclass : theclass : Array
								if (PropExistsInBuiltInClass(t, p, paramCount))
									return true;

								break;//Either the property was not found, or the base was not a built in type, so try again with base class.
							}
							else
								return false;
						}
					}

					//Nothing found in user defined types or their bases, so try built in types.
					//Ex: theclass : Array
					if (!anyFound)
					{
						if (PropExistsInBuiltInClass(t, p, paramCount))
							return true;
						else
							break;
					}
				}
			}

			return false;
		}

		private bool PropExistsInTypeOrBase(Type t, string p, int paramCount)
		{
			while (t != null)
			{
				var props = t.GetProperties();

				foreach (var prop in props)
					if (prop.GetIndexParameters().Length == paramCount)
						if (string.Compare(prop.Name, p, true) == 0)
							return true;

				t = t.BaseType;
			}

			return false;
		}

		private void SetLineIndexes()
		{
			var i = 0;

			foreach (var line in codeLines)
				line.LineNumber = i++;
		}

		private bool TypeExistsAtCurrentOrParentScope(CodeTypeDeclaration currentType, string currentScope, string varName)
		{
			foreach (CodeTypeDeclaration type in mainNs.Types)
				if (string.Compare(type.Name, varName, true) == 0)
					return true;

			foreach (CodeTypeMember type in targetClass.Members)//Nested types beyond one level are not supported, so this should handle all cases.
				if (type is CodeTypeDeclaration ctd)
					if (string.Compare(ctd.Name, varName, true) == 0)
						return true;

			return false;
		}

		private bool VarExistsAtCurrentOrParentScope(CodeTypeDeclaration currentType, string currentScope, string varName)
		{
			if (allVars.TryGetValue(currentType, out var typeFuncs))
			{
				foreach (CodeTypeMember typemember in currentType.Members)//First, check if the type contains the variable.
				{
					if (typemember is CodeSnippetTypeMember cstm && string.Compare(cstm.Name, varName, true) == 0)
						return true;
				}

				if (typeFuncs.TryGetValue(currentScope, out var scopeVars))//The type didn't contain the variable, so check if the local function scope contains it.
				{
					if (scopeVars.ContainsKey(varName))
						return true;
				}
			}

			if (methods.TryGetValue(currentType, out var t))//Check if the variable was a parameter in the current function.
			{
				if (t.TryGetValue(currentScope, out var method))
				{
					if (method.Parameters.Cast<CodeParameterDeclarationExpression>().Any(p => string.Compare(p.Name, varName, true) == 0))
						return true;
				}
			}

			if (currentFuncParams.TryPeek(out var pl))
				if (pl.Contains(varName))
					return true;

			if (staticFuncVars.TryGetValue(currentType, out var st))
				if (st.TryPeek(out var stat))
					if (stat.TryGetValue(varName, out var sv))
						return true;

			if (currentType != targetClass)//Last attempt, check if it's a global variable.
			{
				foreach (CodeTypeMember typemember in targetClass.Members)
				{
					if (typemember is CodeSnippetTypeMember cstm && string.Compare(cstm.Name, varName, true) == 0)
						return true;
				}
			}

			if (allVars.TryGetValue(targetClass, out var globalTypeFuncs))
			{
				if (globalTypeFuncs.TryGetValue("", out var scopeVars))//The type didn't contain the variable, so check if the local function scope contains it.
				{
					if (scopeVars.ContainsKey(varName))
						return true;
				}
			}

			return false;
		}
	}
}