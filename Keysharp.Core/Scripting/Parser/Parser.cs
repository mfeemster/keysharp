using tsmd = System.Collections.Generic.Dictionary<System.CodeDom.CodeTypeDeclaration, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.CodeDom.CodeMethodInvokeExpression>>>;

namespace Keysharp.Scripting
{
	public partial class Parser : ICodeParser
	{
		public const string mainClassName = "program";
		public static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
		public static readonly CultureInfo inv = CultureInfo.InvariantCulture;

		internal const string scopeChar = "_";
		internal const string varsPropertyName = "Vars";

		internal static CodeAttributeDeclaration cad;

		/// <summary>
		/// The order of these is critically important. Items that start with the same character must go from longest to shortest.
		/// </summary>
		internal static FrozenSet<string> contExprOperators = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			",",
			//"%",
			".=",
			".",
			"??=",
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

		internal static FrozenSet<string>.AlternateLookup<ReadOnlySpan<char>> contExprOperatorsAlt = contExprOperators.GetAlternateLookup<ReadOnlySpan<char>>();

		internal static List<string> contExprOperatorsList = contExprOperators.ToList();

		internal static CodeTypeReference ctrdva = new (typeof(DefaultValueAttribute));

		internal static CodeTypeReference ctrpaa = new (typeof(ParamArrayAttribute));

		internal static CodePrimitiveExpression emptyStringPrimitive = new ("");

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

		internal static FrozenSet<string>.AlternateLookup<ReadOnlySpan<char>> exprVerbalOperatorsAlt = exprVerbalOperators.GetAlternateLookup<ReadOnlySpan<char>>();

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
			FlowThrow
		} .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

		internal static FrozenSet<string>.AlternateLookup<ReadOnlySpan<char>> flowOperatorsAlt = flowOperators.GetAlternateLookup<ReadOnlySpan<char>>();

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
			FlowThrow
		} .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

		internal static List<string> nonContExprOperatorsList = ["++", "--"];
		internal static CodePrimitiveExpression nullPrimitive = new (null);
		internal static CodeTypeReference objTypeRef = new (typeof(object));

		internal static FrozenSet<string> propKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			FlowGet,
			FlowSet
		} .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

		internal static FrozenSet<string>.AlternateLookup<ReadOnlySpan<char>> propKeywordsAlt = propKeywords.GetAlternateLookup<ReadOnlySpan<char>>();

		internal readonly CodeMemberField HotstringManagerObject;
		internal readonly string HotstringManagerObjectName;
		internal readonly CodeSnippetExpression HotstringManagerObjectSnippet;
		internal readonly CodeMemberField ScriptObject;
		internal readonly string ScriptObjectName;
		internal readonly CodeSnippetExpression ScriptObjectSnippet;
		internal readonly string VarPrefix = "_ks_";
		internal bool errorStdOut;
		internal CodeStatementCollection initial = [];

		//These are placed at the very beginning of Main().
		internal string name = string.Empty;

		internal bool noTrayIcon;
		internal bool persistent;
		internal bool persistentValueSetByUser;
		internal PreReader preReader;
		private const string args = "args";
		private const string initParams = "initparams";
		private const string mainScope = "";
		private static readonly char[] directiveDelims = Spaces.Concat([Multicast]);

		private static readonly FrozenSet<string> persistentTerms = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			"settimer",
			"menu",
			"hotkey",
			"hotstring",
			"onmessage",
			"onclipboardchange",
			"gui",
			"persistent",
			"showdebug"
		} .ToFrozenSet(StringComparer.InvariantCultureIgnoreCase);

		private readonly Stack<bool> allGlobalVars = new ();
		private readonly tsmd allMethodCalls = [];
		private readonly Stack<bool> allStaticVars = new ();
		private readonly Dictionary<CodeTypeDeclaration, Dictionary<string, OrderedDictionary<string, CodeExpression>>> allVars = [];//Needs to be ordered so that code variables are generated in the order they were declared.
		private readonly List<CodeObjectCreateExpression> arrayCreations = [];
		private readonly CodeAttributeDeclarationCollection assemblyAttributes = [];
		private readonly Dictionary<CodeExpression, CodeSnippetExpression> assignSnippets = [];
		private readonly Stack<CodeBlock> blocks = new ();
		private readonly CompilerHelper Ch;
		private readonly List<CodeSnippetTypeMember> codeSnippetTypeMembers = new ();
		private readonly Stack<List<string>> currentFuncParams = new ();
		private readonly Stack<CodeStatementCollection> elses = new ();
		private readonly Stack<HashSet<string>> excCatchVars = new ();
		private readonly tsmd getMethodCalls = [];
		private readonly tsmd getPropertyValueCalls = [];
		private readonly Stack<List<string>> globalFuncVars = new ();
		private readonly Dictionary<CodeGotoStatement, CodeBlock> gotos = [];
		private readonly Stack<List<string>> localFuncVars = new ();

		private readonly CodeMemberMethod main = new ()
		{
			Attributes = MemberAttributes.Public | MemberAttributes.Static,
			Name = "Main"
		};

		private readonly CodeNamespace mainNs = new ("Keysharp.CompiledMain");
		private readonly Dictionary<CodeTypeDeclaration, Dictionary<string, CodeMemberMethod>> methods = [];

		private readonly char[] ops = [Equal, Not, Greater, Less];

		private readonly Dictionary<CodeTypeDeclaration, Dictionary<string, List<CodeMemberProperty>>> properties = [];

		private readonly tsmd setPropertyValueCalls = [];

		private readonly Stack<CodeBlock> singleLoops = new ();

		private readonly List<CodeMethodInvokeExpression> stackedHotkeys = [];

		private readonly List<CodeMethodInvokeExpression> stackedHotstrings = [];

		private readonly Dictionary<CodeTypeDeclaration, Stack<Dictionary<string, CodeExpression>>> staticFuncVars = [];

		private readonly Stack<CodeSwitchStatement> switches = new ();

		private readonly CodeTypeDeclaration targetClass;

		private readonly CodeStatementCollection topStatements = new ();

		private readonly Stack<CodeTypeDeclaration> typeStack = new ();

		private readonly CodeMemberMethod userMainMethod;
		private bool blockOpen;
		private uint caseCount;
		private List<CodeLine> codeLines = [];
		private uint exCount;
		private string fileName;
		private bool hardCreateOverride = true;
		private int internalID;
		private int labelCount;
		private string lastHotkeyFunc = "";
		private string lastHotstringFunc = "";
		private bool memberVarsStatic = false;
		private CodeStatementCollection parent;
		private CodeBlock parentBlock;
		private uint switchCount;
		private uint tryCount;

		public Parser(CompilerHelper ch)
		{
			Ch = ch;
			userMainMethod = new ()
			{
				Attributes = MemberAttributes.Public | MemberAttributes.Static,
				Name = $"{VarPrefix}UserMainCode",
				ReturnType = objTypeRef
			};
			main.ReturnType = new CodeTypeReference(typeof(int));
			_ = main.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string[]), "args"));
			_ = main.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(STAThreadAttribute))));
			targetClass = AddType(mainClassName);
			_ = targetClass.Members.Add(main);
			ScriptObjectName = $"{VarPrefix}s";
			ScriptObjectSnippet = new CodeSnippetExpression(ScriptObjectName);
			ScriptObject = new CodeMemberField(typeof(Keysharp.Scripting.Script), ScriptObjectName);
			ScriptObject.Attributes |= MemberAttributes.Static;
			_ = targetClass.Members.Add(ScriptObject);
			//
			HotstringManagerObjectName = $"{VarPrefix}hm";
			HotstringManagerObjectSnippet = new CodeSnippetExpression(HotstringManagerObjectName);
			HotstringManagerObject = new CodeMemberField(typeof(Keysharp.Core.Common.Keyboard.HotstringManager), HotstringManagerObjectName);
			HotstringManagerObject.Attributes |= MemberAttributes.Static;
			_ = targetClass.Members.Add(HotstringManagerObject);
			cad = new CodeAttributeDeclaration(ctrpaa);
		}

		public static string GetKeywords() => string.Join(' ', keywords);

		public static bool IsTypeOrBase(Type t1, string t2)
		{
			while (t1 != null)
			{
				var nameToUse = t1.FullName;

				if (!string.IsNullOrEmpty(t1.Namespace))
					nameToUse = nameToUse.TrimStartOf($"{t1.Namespace}.").Replace('+', '.').TrimStartOf("program.");

				if (string.Compare(nameToUse, t2, true) == 0)
					return true;

				t1 = t1.BaseType;
			}

			return false;
		}

		/// <summary>
		/// Return a DOM representation of a Script.TheScript.
		/// </summary>
		public CodeCompileUnit GenerateCompileUnit()
		{
			var unit = new CodeCompileUnit();
			var dummyRefVarCount = 0;
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
			mainNs.Imports.Add(new CodeNamespaceImport("Keysharp.Core.Common"));
			mainNs.Imports.Add(new CodeNamespaceImport("Keysharp.Core.Common.File"));
			mainNs.Imports.Add(new CodeNamespaceImport("Keysharp.Core.Common.Invoke"));
			mainNs.Imports.Add(new CodeNamespaceImport("Keysharp.Core.Common.ObjectBase"));
			mainNs.Imports.Add(new CodeNamespaceImport("Keysharp.Core.Common.Strings"));
			mainNs.Imports.Add(new CodeNamespaceImport("Keysharp.Core.Common.Threading"));
			mainNs.Imports.Add(new CodeNamespaceImport("Keysharp.Scripting"));
			mainNs.Imports.Add(new CodeNamespaceImport("Array = Keysharp.Core.Array"));
			mainNs.Imports.Add(new CodeNamespaceImport("Buffer = Keysharp.Core.Buffer"));
			_ = unit.Namespaces.Add(mainNs);
			AddAssemblyAttribute(typeof(AssemblyBuildVersionAttribute), A_AhkVersion);
			unit.AssemblyCustomAttributes.AddRange(assemblyAttributes);
			assemblyAttributes.Clear();
			var inv = new CodeMethodInvokeExpression(ScriptObjectSnippet, "RunMainWindow");
			_ = inv.Parameters.Add(new CodeSnippetExpression("name"));
			_ = inv.Parameters.Add(new CodeSnippetExpression($"{VarPrefix}UserMainCode"));
			_ = inv.Parameters.Add(new CodePrimitiveExpression(EitherPeristent()));
			_ = main.Statements.Add(new CodeExpressionStatement(inv));
			_ = main.Statements.Add(new CodeMethodInvokeExpression(ScriptObjectSnippet, "WaitThreads"));
			var exit0 = (CodeMethodInvokeExpression)InternalMethods.ExitApp;
			_ = exit0.Parameters.Add(new CodePrimitiveExpression(0));
			var exit1 = (CodeMethodInvokeExpression)InternalMethods.ExitApp;
			_ = exit1.Parameters.Add(new CodePrimitiveExpression(1));
			var exitIfNotPersistent = new CodeMethodInvokeExpression(ScriptObjectSnippet, "ExitIfNotPersistent");
			//Wrap the entire body of Main() in a try/catch block.
			//First try to catch our own special exceptions, and if the exception type was not that, then just look for regular system exceptions.
			var tcf = new CodeTryCatchFinallyStatement();
			//
			var ctch2 = new CodeCatchClause("kserr", new CodeTypeReference("Keysharp.Core.Error"));
			var pushcse = new CodeSnippetExpression($"var ({VarPrefix}pushed, {VarPrefix}btv) = {ScriptObjectName}.Threads.BeginThread()");
			var msg = new CodeSnippetExpression("MsgBox(\"Uncaught Keysharp exception:\\r\\n\" + kserr, $\"{Accessors.A_ScriptName}: Unhandled exception\", \"iconx\")");
			var popcse = new CodeSnippetExpression($"{ScriptObjectName}.Threads.EndThread({VarPrefix}pushed)");
			var ccsHandled = new CodeConditionStatement(new CodeSnippetExpression("!kserr.Handled"));
			var ccsProcessed = new CodeConditionStatement(new CodeSnippetExpression("!kserr.Processed"));
			_ = ccsProcessed.TrueStatements.Add(new CodeSnippetExpression("_ = ErrorOccurred(kserr, kserr.ExcType)"));
			var cmrsexit = new CodeMethodReturnStatement(new CodeSnippetExpression("Environment.ExitCode"));
			_ = ccsHandled.TrueStatements.Add(pushcse);
			_ = ccsHandled.TrueStatements.Add(msg);
			_ = ccsHandled.TrueStatements.Add(popcse);
			_ = ctch2.Statements.Add(ccsProcessed);
			_ = ctch2.Statements.Add(ccsHandled);
			_ = ctch2.Statements.Add(new CodeExpressionStatement(exit1));
			_ = ctch2.Statements.Add(cmrsexit);
			_ = tcf.CatchClauses.Add(ctch2);
			//
			var ctch3 = new CodeCatchClause("exitex", new CodeTypeReference("Keysharp.Core.Flow.UserRequestedExitException"));
			_ = ctch3.Statements.Add(cmrsexit);
			_ = tcf.CatchClauses.Add(ctch3);
			var ctch = new CodeCatchClause("mainex", new CodeTypeReference("System.Exception"));
			_ = ctch.Statements.Add(new CodeSnippetExpression(@"var ex = mainex.InnerException ?? mainex;

				if (ex is Keysharp.Core.Error kserr)
				{
					if (!kserr.Processed)
						_ = ErrorOccurred(kserr, kserr.ExcType);

					if (!kserr.Handled)
					{
						var (_ks_pushed, _ks_btv) = _ks_s.Threads.BeginThread();
						MsgBox(""Uncaught Keysharp exception:\r\n"" + kserr, $""{Accessors.A_ScriptName}: Unhandled exception"", ""iconx"");
						_ks_s.Threads.EndThread(_ks_pushed);
					}
				}
				else
				{
					var (_ks_pushed, _ks_btv) = _ks_s.Threads.BeginThread();
					MsgBox(""Uncaught exception:\r\n"" + ""Message: "" + ex.Message + ""\r\nStack: "" + ex.StackTrace, $""{Accessors.A_ScriptName}: Unhandled exception"", ""iconx"");
					_ks_s.Threads.EndThread(_ks_pushed);
				}
"));
			//_ = ctch.Statements.Add(new CodeSnippetExpression("MsgBox(\"Uncaught exception:\\r\\n\" + \"Message: \" + mainex.Message + \"\\r\\nStack: \" + mainex.StackTrace)"));
			_ = ctch.Statements.Add(new CodeExpressionStatement(exit1));
			_ = ctch.Statements.Add(cmrsexit);
			_ = tcf.CatchClauses.Add(ctch);
			var tempstatements = main.Statements;
			tcf.TryStatements.AddRange(tempstatements);
			_ = tcf.TryStatements.Add(cmrsexit);
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
							var propname = "";

							if (cmie.Parameters.Count > 1 && cmie.Parameters[1] is CodePrimitiveExpression cpe)
								propname = cpe.Value.ToString();

							var name = cvre.VariableName;

							if (!VarExistsAtCurrentOrParentScope(cmietype.Key, cmietypefunc.Key, name)
									&& TypeExistsAtCurrentOrParentScope(cmietype.Key, cmietypefunc.Key, name))
							{
								if (MethodExistsInTypeOrBase(name, propname) is CodeMemberMethod cmm)
								{
									cmie.Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(Script)), "GetStaticMemberValueT");
									cmie.Method.TypeArguments.Add(name);
									cmie.Parameters[0] = new CodeSnippetExpression($"{name}.{propname}");
									cmie.Parameters.RemoveAt(1);
								}
								else
								{
									cmie.Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(Script)), "GetStaticMemberValueT");
									cmie.Method.TypeArguments.Add(name);
									cmie.Parameters.RemoveAt(0);
								}
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
			var argsSnippet = new CodeSnippetExpression("args");
			var ctrObjectArray = new CodeTypeReference(typeof(object[]));
			var tsVar = DateTime.UtcNow.ToString("__MMddyyyyHHmmssfffffff");

			while (targetClass.Members.Cast<CodeTypeMember>().Any(ctm => ctm.Name == tsVar))
				tsVar = "_" + tsVar;

			var ctrpaa = new CodeTypeReference(typeof(ParamArrayAttribute));
			var cad = new CodeAttributeDeclaration(ctrpaa);
			var cdpeArgs = new CodeParameterDeclarationExpression(typeof(object[]), "args");
			_ = cdpeArgs.CustomAttributes.Add(cad);

			foreach (var typeMethods in methods)
			{
				CodeMemberMethod newmeth = null, callmeth = null, initmeth = null;

				if (typeMethods.Key != targetClass)
				{
					var baseType = typeMethods.Key.BaseTypes[0].BaseType;
					var origNewParams = new List<CodeParameterDeclarationExpression>();

					foreach (var method in typeMethods.Value.Values)
					{
						if (newmeth == null && string.Compare(method.Name, "__New", true) == 0)//__New() and Call() have already been added.
						{
							//New must be virtual for classes derived from a built-in type and
							//be declared override for classes derived from user-defined types.
							method.Attributes = MemberAttributes.Public;
							method.Name = "__New";//Ensure it's properly cased.

							if (method.Parameters.Count == 1 && method.Parameters[0].IsVariadic())
							{
								//When __New() is declared with an anonymous variadic parameter like __New(*),
								//then the parameter will be named args, so don't declare another variable by the same name.
								if (method.Parameters[0].Name != "args")
									method.Statements.Insert(0, new CodeExpressionStatement(new CodeSnippetExpression($"object {name = Ch.CreateEscapedIdentifier(method.Parameters[0].Name)} = args")));
							}
							else
							{
								for (var i = method.Parameters.Count - 1; i >= 0; i--)
								{
									var varName = Ch.CreateEscapedIdentifier(method.Parameters[i].Name);
									method.Statements.Insert(0, new CodeExpressionStatement(new CodeSnippetExpression($"object {varName} = args.Length > {i} ? args[{i}] : null")));
									allVars.GetOrAdd(typeMethods.Key).GetOrAdd(method.Name).Add(varName, new CodeVariableReferenceExpression(varName));//More of a declaration than a reference, but it works.
								}
							}

							origNewParams = method.Parameters.Cast<CodeParameterDeclarationExpression>().ToList();
							method.Parameters.Clear();
							_ = method.Parameters.Add(cdpeArgs);
							newmeth = method;
						}
						else if (callmeth == null && method.Attributes.HasFlag(MemberAttributes.Static) && string.Compare(method.Name, "Call", true) == 0)
						{
							method.Name = "Call";
							callmeth = method;
						}
						else if (string.Compare(method.Name, "__Init", true) == 0)
						{
							method.Name = "__Init";
							initmeth = method;
						}
						else if (string.Compare(method.Name, "__StaticInit", true) == 0)
						{
							method.Name = "__StaticInit";
							method.Attributes = MemberAttributes.Private | MemberAttributes.Static;
							method.Parameters.Clear();
							_ = typeMethods.Key.Members.Add(method);
						}
						else
							_ = typeMethods.Key.Members.Add(method);

						if (string.Compare(method.Name, "__Delete", true) == 0)
						{
							method.Name = "__Delete";
							_ = typeMethods.Key.Members.Add(new CodeSnippetTypeMember($"\t\t\t~{typeMethods.Key.Name}() {{ __Delete(); }}") { Name = typeMethods.Key.Name });
						}

						/*
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
						*/
					}

					var thisconstructor = typeMethods.Key.Members.Cast<CodeTypeMember>().FirstOrDefault(ctm => ctm is CodeConstructor cc && !cc.Attributes.HasFlag(MemberAttributes.Static)) as CodeConstructor;
					var userDefinedBase = FindUserDefinedType(baseType);
					var extendsUserType = userDefinedBase != null;
					var userDefinedCall = false;

					//To properly support the calling hierarchy of __New(), it must be declared differently
					//depending on where in the hierarchy the class is defined.
					if (newmeth != null)
					{
						//__New() will be virtual by default, which we only want to be the case for the first level down
						//from built-in classes. After that, they must be declared as override.
						if (extendsUserType)
							newmeth.Attributes |= MemberAttributes.Override;

						_ = typeMethods.Key.Members.Add(newmeth);
					}
					else if (userDefinedBase == null)
					{
						//If deriving from a built-in class, then __New() must be defined as a virtual method at this level
						newmeth = new CodeMemberMethod();
						newmeth.Attributes = MemberAttributes.Public;//Virtual by default.
						newmeth.Name = "__New";
						newmeth.ReturnType = objTypeRef;
						_ = newmeth.Statements.Add(new CodeMethodReturnStatement(emptyStringPrimitive));
						_ = newmeth.Parameters.Add(cdpeArgs);
						_ = typeMethods.Key.Members.Add(newmeth);
					}

					if (callmeth == null)
					{
						callmeth = new CodeMemberMethod
						{
							Name = "Call",
							ReturnType = new CodeTypeReference(typeMethods.Key.Name),
							Attributes = MemberAttributes.Public | MemberAttributes.Static
						};
						//Body of Call() will be added later.
						_ = typeMethods.Key.Members.Add(callmeth);
						methods[typeStack.Peek()][callmeth.Name] = callmeth;
					}
					else
					{
						userDefinedCall = true;
						_ = typeMethods.Key.Members.Add(callmeth);
						methods[typeStack.Peek()][callmeth.Name] = callmeth;
					}

					//The lines relating to args and __New() work whether any params were declared or not.
					if (thisconstructor != null)
					{
						var ctorParams = origNewParams.Select(p => Ch.CreateEscapedIdentifier(p.Name)).ToArray();
						var newparamnames = string.Join(", ", ctorParams);

						//First call Init() in the constructor if it was derived from a built-in type.
						//Because it's declared virtual, this will call to the most derived type,
						//then cascade the calls back down.
						//Next, do the same with __New().
						//Manually declared __New() methods in classes that derive from user-defined types
						//should manually call super.__New().
						//Note that classes which define __New() and are one level derived from built-in types
						//don't need to call super.__New() because the built-in class will call it automatically
						//because __New() is not virtual at the built-in type level.
						if (userDefinedBase == null)
						{
							if (newmeth != null)
								_ = thisconstructor.Statements.Add(new CodeSnippetExpression("__New(args)"));
						}

						thisconstructor.Parameters.Clear();
						_ = thisconstructor.Parameters.Add(cdpeArgs);

						if (!userDefinedCall)
						{
							//Get param names and pass.
							callmeth.Parameters.Clear();
							callmeth.Statements.Clear();

							if (origNewParams.Count > 0)
							{
								callmeth.Parameters.AddRange(new CodeParameterDeclarationExpressionCollection(origNewParams.ToArray()));
								_ = callmeth.Statements.Add(new CodeSnippetExpression($"return new {typeMethods.Key.Name}({newparamnames})"));
							}
							else
							{
								_ = callmeth.Parameters.Add(cdpeArgs);
								_ = callmeth.Statements.Add(new CodeSnippetExpression($"return new {typeMethods.Key.Name}(args)"));
							}
						}
					}

					var thisStaticConstructor = typeMethods.Key.Members.Cast<CodeTypeMember>().FirstOrDefault(ctm => ctm is CodeTypeConstructor cc) as CodeTypeConstructor;

					if (thisStaticConstructor != null)
					{
						_ = thisStaticConstructor.Statements.Add(new CodeSnippetExpression($"__StaticInit()"));
					}

					var rawBaseTypeName = typeof(KeysharpObject).Name;

					if (baseType != rawBaseTypeName && thisconstructor != null)
					{
						if (userDefinedBase != null)
						{
							rawBaseTypeName = userDefinedBase.Name;

							if (userDefinedBase.Members.Cast<CodeTypeMember>().FirstOrDefault(ctm => ctm is CodeConstructor) is CodeConstructor ctm2)
							{
								thisconstructor.BaseConstructorArgs.Clear();
								_ = thisconstructor.BaseConstructorArgs.Add(argsSnippet);
								//var i = 0;
								//for (; i < thisconstructor.Parameters.Count && i < ctm2.Parameters.Count; i++)//Iterate through all of the parameters in this class's constructor.
								//  _ = thisconstructor.BaseConstructorArgs.Add(new CodeSnippetExpression($"{thisconstructor.Parameters[i].Name}"));
								//for (; i < ctm2.Parameters.Count; i++)//Fill whatever remains of the base constructor parameters with nulls.
								//  _ = thisconstructor.BaseConstructorArgs.Add(nullPrimitive);
							}
						}
						else//Try built in types.
						{
							foreach (var typekv in Script.TheScript.ReflectionsData.typeToStringMethods)
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
											_ = thisconstructor.BaseConstructorArgs.Add(argsSnippet);
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

					//Once we have the properly cased base type, add a super property to every class.
					//This is done because we can't just do it once in Any because GetType() always resolves to the most derived type which is not what we want.
					var superprop = new CodeMemberProperty
					{
						Name = "super",
						Type = new CodeTypeReference(typeof((Type, object))),
						Attributes = MemberAttributes.Public | MemberAttributes.New | MemberAttributes.Final
					};
					_ = superprop.GetStatements.Add(new CodeSnippetExpression($"return (typeof({rawBaseTypeName}), this)"));
					_ = typeMethods.Key.Members.Add(superprop);
				}
				else
				{
					foreach (var method in typeMethods.Value.Values)
						_ = typeMethods.Key.Members.Add(method);
				}
			}

			foreach (var cmietype in allMethodCalls)
			{
				foreach (var cmietypefunc in cmietype.Value)
				{
					foreach (var cmie in cmietypefunc.Value)
					{
						for (int i = 0; i < cmie.Parameters.Count; i++)//Convert function arguments which were direct function references.
						{
							var wasCast = false;
							var funcParam = cmie.Parameters[i];
							var cvreParam = funcParam as CodeVariableReferenceExpression;

							if (cvreParam == null)
							{
								if (funcParam is CodeCastExpression cce)
								{
									if (cce.Expression is CodeVariableReferenceExpression cvre2)
									{
										wasCast = true;
										cvreParam = cvre2;
									}
								}
							}

							if (cvreParam != null)
							{
								var type = cvreParam.UserData["origtype"] is CodeTypeDeclaration ctd ? ctd : targetClass;
								var reEval = ReevaluateCodeVariableReference(type, cmietypefunc.Key, cvreParam);
								cmie.Parameters[i] = wasCast ? new CodeCastExpression(typeof(object), reEval) : reEval;//Recast to object because it will never have been cast to anything else.
							}
						}

						//Handle changing myfuncobj() to myfuncobj.Call().
						if (VarExistsAtCurrentOrParentScope(cmietype.Key, cmietypefunc.Key, cmie.Method.MethodName))
						{
							var refIndexes = ParseArguments(cmie.Parameters);
							var callFuncName = "";

							if (refIndexes.Count > 0)
							{
								var newParams = ConvertDirectParamsToInvoke(cmie.Parameters);
								cmie.Parameters.Clear();
								cmie.Parameters.AddRange(newParams);
								callFuncName = "CallWithRefs";
							}
							else
							{
								callFuncName = "Call";
							}

							HandleAllVariadicParams(cmie);
							var funccmie = new CodeMethodInvokeExpression((CodeMethodReferenceExpression)InternalMethods.Func, new CodeVariableReferenceExpression(cmie.Method.MethodName.ToLower()));
							cmie.Method = new CodeMethodReferenceExpression(funccmie, callFuncName);
						}
						else if (GetUserDefinedTypename(cmie.Method.MethodName) is CodeTypeDeclaration ctd && ctd.Name.Length > 0)//Convert myclass() to myclass.Call().
						{
							//This is also done below when snippets are reevaluated because property initializers are implemented as snippets.
							var callMeth = ctd.Members.Cast<CodeTypeMember>().First(ctm => ctm is CodeMemberMethod cmm && cmm.Name == "Call") as CodeMemberMethod;
							cmie.Method = new CodeMethodReferenceExpression(new CodeSnippetExpression(ctd.Name), "Call");
							HandlePartialVariadicParams(cmie, callMeth);
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
										var refVarName = "";
										var mp = methParams[i];

										//Need to check for parameters that have a default value, but were supplied null
										//by placing a comma at their position in the argument list.
										//Look up the default value and pass that instead, taking special action for ref params.
										if (mp.DefaultValue() is CodeExpression defVal)
										{
											var defValStr = Ch.CodeToString(defVal);

											if (mp.Direction == FieldDirection.Ref)
											{
												var par = cmie.UserData["parentstatements"] as CodeStatementCollection;
												var line = (int)cmie.UserData["parentline"];
												refVarName = $"defRefVal{dummyRefVarCount++}";
												par.Insert(line, new CodeExpressionStatement(new CodeSnippetExpression($"object {refVarName} = {defValStr}")));
												cmie.Parameters[i] = new CodeSnippetExpression($"ref {refVarName}");
											}
											else
												cmie.Parameters[i] = new CodeSnippetExpression(defValStr);
										}
										else if (mp.Direction == FieldDirection.Ref)//Ref with no default. Null can't be passed, so just pass a ref to a dummy variable.
										{
											refVarName = tsVar;
											createDummyRef = true;
											cmie.Parameters[i] = new CodeSnippetExpression($"ref {refVarName}");
										}
									}
								}
							}

							//If they supplied less than the required number of arguments, fill in only default ref params.
							if (methParams.Count > cmie.Parameters.Count)
							{
								var newParams = new List<CodeExpression>(methParams.Count);

								for (var i = cmie.Parameters.Count; i < methParams.Count; i++)
								{
									var mp = methParams[i];

									if (!mp.IsVariadic() && mp.DefaultValue() is CodeExpression defVal)
									{
										string refVarName;
										var defValStr = Ch.CodeToString(defVal);

										if (mp.Direction == FieldDirection.Ref)
										{
											var par = cmie.UserData["parentstatements"] as CodeStatementCollection;
											var line = (int)cmie.UserData["parentline"];
											refVarName = $"defRefVal{dummyRefVarCount++}";
											par.Insert(line, new CodeExpressionStatement(new CodeSnippetExpression($"object {refVarName} = {defValStr}")));
											_ = cmie.Parameters.Add(new CodeSnippetExpression($"ref {refVarName}"));
										}
										else
										{
											_ = cmie.Parameters.Add(new CodeSnippetExpression(defValStr));
										}
									}
								}
							}

							HandlePartialVariadicParams(cmie, cmm);
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
											var refVarName = "";
											var mp = methParams[i];

											//Need to check for parameters that have a default value, but were supplied null
											//by placing a comma at their position in the argument list.
											//Look up the default value and pass that instead, taking special action for ref params.
											if (mp.DefaultValue != null && mp.DefaultValue.GetType() != typeof(DBNull))
											{
												var defValStr = Ch.CodeToString(new CodePrimitiveExpression(mp.DefaultValue));

												if (mp.ParameterType.IsByRef && !mp.IsOut)
												{
													var par = cmie.UserData["parentstatements"] as CodeStatementCollection;
													var line = (int)cmie.UserData["parentline"];
													refVarName = $"defRefVal{dummyRefVarCount++}";
													par.Insert(line, new CodeExpressionStatement(new CodeSnippetExpression($"object {refVarName} = {defValStr}")));
													cmie.Parameters[i] = new CodeSnippetExpression($"ref {refVarName}");
												}
												else
												{
													cmie.Parameters[i] = new CodeSnippetExpression(defValStr);
												}
											}
											else if (mp.ParameterType.IsByRef && !mp.IsOut)//Ref with no default. Null can't be passed, so just pass a ref to a dummy variable.
											{
												refVarName = tsVar;
												createDummyRef = true;
												cmie.Parameters[i] = new CodeSnippetExpression($"ref {refVarName}");
											}
										}
									}
								}

								//If they supplied less than the required number of arguments, fill in only default ref params.
								if (methParams.Length > cmie.Parameters.Count)
								{
									for (var i = cmie.Parameters.Count; i < methParams.Length; i++)
									{
										var mp = methParams[i];

										if (!mp.IsVariadic() && mp.IsOptional && mp.DefaultValue != null && mp.DefaultValue.GetType() != typeof(DBNull))
										{
											var defValStr = Ch.CodeToString(new CodePrimitiveExpression(mp.DefaultValue));

											if (mp.ParameterType.IsByRef && !mp.IsOut)
											{
												var par = cmie.UserData["parentstatements"] as CodeStatementCollection;
												var line = (int)cmie.UserData["parentline"];
												var refVarName = $"defRefVal{dummyRefVarCount++}";
												par.Insert(line, new CodeExpressionStatement(new CodeSnippetExpression($"object {refVarName} = {defValStr}")));
												_ = cmie.Parameters.Add(new CodeSnippetExpression($"ref {refVarName}"));
											}
											else
											{
												_ = cmie.Parameters.Add(new CodeSnippetExpression(defValStr));
											}
										}
									}
								}

								HandlePartialVariadicParams(cmie, mph2.mi);
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

			foreach (var arrayCreation in arrayCreations)
			{
				for (var ai = 0; ai < arrayCreation.Parameters.Count; ++ai)
				{
					var p = arrayCreation.Parameters[ai];
					var isVariadic = p.UserData["variadic"] is bool b && b;

					if (p is CodeVariableReferenceExpression cvre)
					{
						var type = cvre.UserData["origtype"] is CodeTypeDeclaration ctd ? ctd : targetClass;
						var scope = cvre.UserData["origscope"] as string;
						arrayCreation.Parameters[ai] = ReevaluateCodeVariableReference(type, scope, cvre);
					}

					if (isVariadic)//Any element can be variadic in an array construction.
					{
						var spreadCode = Ch.CodeToString(arrayCreation.Parameters[ai]);
						arrayCreation.Parameters[ai] = new CodeSnippetExpression($"..Keysharp.Scripting.Script.FlattenParam({spreadCode})");
					}
				}

				var paramsSnippet = new CodeSnippetExpression($"[{string.Join<object>(", ", arrayCreation.Parameters.Cast<CodeExpression>().Select(pe => Ch.CodeToString(pe)))}]");
				arrayCreation.Parameters.Clear();
				_ = arrayCreation.Parameters.Add(paramsSnippet);
			}

			if (createDummyRef)
			{
				_ = targetClass.Members.Add(new CodeSnippetTypeMember()
				{
					Name = name,
					Text = $"\t\tpublic static object {tsVar};"
				});
			}

			//Must explicitly mark all index operators as override if they exist in a base, and the parameter count matches.
			//This is because in the function IndexAt(), Array[] and Map[] are called directly, after doing a cast check.
			//First go through all user defined properties and change the name __Item to Item.
			foreach (var typeProperties in properties)
			{
				if (typeProperties.Key != targetClass)
				{
					var didItem = false;

					foreach (var propkv in typeProperties.Value.ToArray())
					{
						var propList = propkv.Value;

						foreach (var prop in propList)
						{
							if (string.Compare(prop.Name, "__Item", true) == 0)
							{
								prop.Name = "Item";
								//prop.Attributes = MemberAttributes.Public;//Make virtual at a minimum, which might get converted to override below.
								_ = typeProperties.Value.Remove("__item");//Was stored as lowercase.
								typeProperties.Value.GetOrAdd("Item").Add(prop);

								if (!didItem)
								{
									didItem = true;
									var ctors = typeProperties.Key.Members.Cast<CodeTypeMember>().Where(ctm => ctm is CodeConstructor cc && !cc.Attributes.HasFlag(MemberAttributes.Static)).Cast<CodeConstructor>();

									foreach (var ctor in ctors)
										ctor.Statements.Insert(0, new CodeExpressionStatement(new CodeSnippetExpression("Init__Item()")));
								}
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
							//Virtual can never work because resolution is wrong when super is used.
							//if (prop.Name == "Item" && typeProperties.Key.BaseTypes.Count > 0)
							//{
							//  var bpi = PropExistsInTypeOrBase(typeProperties.Key.BaseTypes[0].BaseType, "Item", prop.Parameters.Count);
							//  if (bpi.Item1 && prop.Parameters.Count == bpi.Item3.Count)
							//  {
							//      var ipindex = 0;
							//      if (bpi.Item3.All(ip => ip.Equals(new CommonParameterInfo(
							//                                            prop.Parameters[ipindex].IsVariadic(),
							//                                            prop.Parameters[ipindex].Direction == FieldDirection.Ref,
							//                                            prop.Parameters[ipindex++].Type.BaseType))))
							//          prop.Attributes = MemberAttributes.Public | MemberAttributes.Override;
							//  }
							//}
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

			var hotkeyInitCmie = new CodeMethodInvokeExpression(
				new CodeMethodReferenceExpression(
					new CodeTypeReferenceExpression("Keysharp.Core.Common.Keyboard.HotkeyDefinition"), "ManifestAllHotkeysHotstringsHooks"));

			if (topStatements.Count > 0)
			{
				//Readd rather than insert;
				var userCodeStatements = new CodeStatementCollection
				{
					Capacity = topStatements.Count + userMainMethod.Statements.Count
				};

				foreach (CodeStatement hkc in topStatements)
					_ = userCodeStatements.Add(hkc);

				_ = userCodeStatements.Add(hotkeyInitCmie);

				foreach (CodeStatement s in userMainMethod.Statements)
					_ = userCodeStatements.Add(s);

				userMainMethod.Statements.Clear();
				userMainMethod.Statements.AddRange(userCodeStatements);
			}
			else
				_ = userMainMethod.Statements.Add(hotkeyInitCmie);

			_ = userMainMethod.Statements.Add(EitherPeristent() ? exitIfNotPersistent : exit0);
			_ = userMainMethod.Statements.Add(new CodeMethodReturnStatement(emptyStringPrimitive));
			methods.GetOrAdd(targetClass)[userMainMethod.Name] = userMainMethod;
			_ = targetClass.Members.Add(userMainMethod);

			//Assignments as snippets need to be reevaluated.
			//Despite having to compile code again, this took < 1ms in debug mode for over 100 assignments,
			//so it shouldn't matter.
			foreach (var assign in assignSnippets)
				_ = ReevaluateSnippets(assign.Value);

			//Static member properties need to be reevaluated.
			foreach (var member in codeSnippetTypeMembers)
				ReevaluateStaticProperties(member);

			return unit;
		}

		public CodeCompileUnit Parse(TextReader codeStream) => Parse(codeStream, string.Empty);

		public CodeCompileUnit Parse(TextReader codeStream, string nm)
		{
			name = nm;
			preReader = new PreReader(this);
			codeLines = preReader.Read(codeStream, name);
#if DEBUG
			//File.WriteAllLines("./finalscriptcode.txt", codeLines.Select((cl) => $"{cl.LineNumber}: {cl.Code}"));
#endif
			Statements();

			if (!noTrayIcon)
				_ = initial.Add(new CodeMethodInvokeExpression(ScriptObjectSnippet, "CreateTrayMenu"));

			if (persistentValueSetByUser)
				_ = initial.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(Flow)), "Persistent"), [new CodePrimitiveExpression(true)]));

			_ = initial.Add(new CodeSnippetExpression("Keysharp.Core.Env.HandleCommandLineParams(args)"));
			var inst = (CodeMethodInvokeExpression)InternalMethods.HandleSingleInstance;
			_ = inst.Parameters.Add(new CodeSnippetExpression("name"));
			_ = inst.Parameters.Add(new CodeSnippetExpression($"eScriptInstance.{preReader.SingleInstance}"));
			var condInst = new CodeConditionStatement(inst, new CodeMethodReturnStatement(new CodeSnippetExpression("Environment.ExitCode = 0")));
			_ = initial.Add(new CodeSnippetExpression($"{ScriptObject.Name}.SetName(name)"));
			_ = initial.Add(new CodeAssignStatement(HotstringManagerObjectSnippet, new CodeSnippetExpression($"{ScriptObjectName}.HotstringManager")));
			_ = initial.Add(condInst);
			_ = initial.Add(new CodeAssignStatement(ScriptObjectSnippet, new CodeObjectCreateExpression(typeof(Keysharp.Scripting.Script))));

			foreach (var (p, s) in preReader.PreloadedDlls)//Add after Script.Init() call above, because the statements will be added in reverse order.
			{
				var cmie = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Keysharp.Scripting.Script.Variables"), "AddPreLoadedDll");
				_ = cmie.Parameters.Add(new CodePrimitiveExpression(p));
				_ = cmie.Parameters.Add(new CodePrimitiveExpression(s));
				initial.Add(new CodeExpressionStatement(cmie));
			}

			var namevar = new CodeVariableDeclarationStatement("System.String", "name", new CodePrimitiveExpression(name));
			_ = initial.Add(namevar);

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
							if (globalvar.Value == null)
								continue;

							var name = globalvar.Key.Replace(scopeChar[0], '_');

							if (name == "this")//Never create a "this" variable inside of a class definition.
								continue;

							name = Ch.CreateEscapedIdentifier(name);
							_ = typekv.Key.Members.Add(new CodeSnippetTypeMember()
							{
								Name = name,
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
							if (globalvar.Value == null)
								continue;

							var ce = globalvar.Value;
							var name = globalvar.Key.Replace(scopeChar[0], '_');

							if (name == "this")//Never create a "this" variable inside of a class definition.
								continue;

							name = Ch.CreateEscapedIdentifier(name);
							var isstatic = ce.UserData.Contains("isstatic");
							var init = Ch.CodeToString(ce);
							//If the property existed in a base class and is a simple assignment in this class, then assume
							//they meant to reference the base property rather than create a new one.
							(bool, string, List<CommonParameterInfo>) bn;

							//Determine if the variable name matched a property defined in a base class.
							//Note this is not done for static variables because those are always assumed to be new declarations.
							if (!isstatic && (bn = PropExistsInTypeOrBase(typekv.Key.BaseTypes[0].BaseType, name, 0)).Item1)
							{
								name = bn.Item2;
							}
							else
							{
								var cstm = new CodeSnippetTypeMember()
								{
									Name = name
								};

								if (isstatic)
								{
									cstm.UserData["orig"] = ce;
									cstm.Text = $"\t\t\tpublic static object {name} {{ get; set; }} = {init};";
									codeSnippetTypeMembers.Add(cstm);
								}
								else
									cstm.Text = $"\t\t\tpublic object {name} {{ get; set; }}";

								_ = typekv.Key.Members.Add(cstm);
							}

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
									{
										var tempcboe = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(name),
												CodeBinaryOperatorType.Assign,
												ce);
										var tempsnippet = BinOpToSnippet(tempcboe);//Snippet is needed here and elsewhere because a CBOE generates too many parens which causes a compile error.
										//This will get reevaluated at the end of GenerateCompileUnit() which will convert any calls like
										//classname() to classname.Call().
										_ = initcmm.Statements.Add(tempsnippet);
									}

									foreach (DictionaryEntry kv in ce.UserData)
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
			allVars[ctd] = new Dictionary<string, OrderedDictionary<string, CodeExpression>>(StringComparer.OrdinalIgnoreCase);
			staticFuncVars[ctd] = new Stack<Dictionary<string, CodeExpression>>();
			setPropertyValueCalls[ctd] = [];
			getPropertyValueCalls[ctd] = [];
			getMethodCalls[ctd] = [];
			allMethodCalls[ctd] = [];
			return ctd;
		}

		private void CheckPersistent(string name)
		{
			if (persistent)
				return;

			if (persistentTerms.Contains(name))
				persistent = true;
		}

		private bool EitherPeristent() => persistent || persistentValueSetByUser;

		private CodeTypeDeclaration FindUserDefinedType(string typeName)
		{
			foreach (CodeTypeMember type in targetClass.Members)
				if (type is CodeTypeDeclaration ctd)
					if (string.Compare(ctd.Name, typeName, true) == 0)
						return ctd;

			return null;
		}

		private CodeTypeDeclaration GetUserDefinedTypename(string typeName)
		{
			foreach (CodeTypeMember type in targetClass.Members)
				if (type is CodeTypeDeclaration ctd)
					if (string.Compare(ctd.Name, typeName, true) == 0)
						return ctd;

			return null;
		}

		private void HandleAllVariadicParams(CodeMethodInvokeExpression cmie)
		{
			var pces = cmie.Parameters.Cast<CodeExpression>();
			var cmielast = pces.LastOrDefault() as CodeMethodInvokeExpression;
			var lastisstar = cmielast != null && cmielast.Method.MethodName == "FlattenParam";

			//After all arguments were parsed, we need to take one last special action if the last argument was using the * spread operator.
			//The entire list must be converted into an array, with the last argument being preceded by a C# .. spread operator.
			//CodeDOM has no support for such so it must all be done via snippets.
			if (lastisstar && pces.Count() > 1)
			{
				var argStrings = pces.Select(ce => ce == pces.Last() ? $"..{Ch.CodeToString(ce)}" : Ch.CodeToString(ce)).ToList();
				var finalArgStr = $"[{string.Join(", ", argStrings)}]";
				cmie.Parameters.Clear();
				_ = cmie.Parameters.Add(new CodeSnippetExpression(finalArgStr));
			}
		}

		private void HandleAllVariadicParams(CodeMethodInvokeExpression cmie, CodeMemberMethod cmm)
		{
			//Method that was declared.
			var pdes = cmm.Parameters.Cast<CodeParameterDeclarationExpression>();
			var cpde = pdes.LastOrDefault();
			var lastDeclIsStar = cpde != null && cpde.IsVariadic();
			//Method as it was called.
			var pces = cmie.Parameters.Cast<CodeExpression>();
			var cmielast = pces.LastOrDefault() as CodeMethodInvokeExpression;
			var lastCalledIsStar = cmielast != null && cmielast.Method.MethodName == "FlattenParam";

			//After all arguments were parsed, we need to take one last special action if the last argument was using the * spread operator.
			//The entire list must be converted into an array, with the last argument being preceded by a C# .. spread operator.
			//CodeDOM has no support for such so it must all be done via snippets.
			if (lastDeclIsStar && lastCalledIsStar && pces.Count() > 1)
			{
				var argStrings = pces.Select(ce => ce == pces.Last() ? $"..{Ch.CodeToString(ce)}" : Ch.CodeToString(ce)).ToList();
				var finalArgStr = $"[{string.Join(", ", argStrings)}]";
				cmie.Parameters.Clear();
				_ = cmie.Parameters.Add(new CodeSnippetExpression(finalArgStr));
			}
		}

		private void HandlePartialVariadicParams(CodeMethodInvokeExpression cmie, CodeMemberMethod cmm)
		{
			//Method that was declared.
			var pdes = cmm.Parameters.Cast<CodeParameterDeclarationExpression>();
			var cpde = pdes.LastOrDefault();
			var lastDeclIsStar = cpde != null && cpde.IsVariadic();
			//Method as it was called.
			var pces = cmie.Parameters.Cast<CodeExpression>();
			var cmielast = pces.LastOrDefault() as CodeMethodInvokeExpression;
			var lastCalledIsStar = cmielast != null && cmielast.Method.MethodName == "FlattenParam";

			//After all arguments were parsed, we need to take one last special action if the last argument was using the * spread operator.
			//The entire list must be converted into an array, with the last argument being preceded by a C# .. spread operator.
			//CodeDOM has no support for such so it must all be done via snippets.
			if (lastDeclIsStar && lastCalledIsStar)
			{
				var indexOfDeclStar = pdes.Count() - 1;
				var indexOfCalledStar = pces.Count() - 1;

				if ((cmie.Parameters.Count - indexOfDeclStar) > 1)
				{
					var callNonVariadic = pces.SkipLast(indexOfDeclStar);
					var callVariadic = pces.Skip(indexOfDeclStar);
					var argStrings = callVariadic.Select(ce => ce == callVariadic.Last() ? $"..{Ch.CodeToString(ce)}" : Ch.CodeToString(ce)).ToList();
					var finalArgStr = $"[{string.Join(", ", argStrings)}]";

					while (cmie.Parameters.Count > indexOfDeclStar)
						cmie.Parameters.Remove(cmie.Parameters[cmie.Parameters.Count - 1]);

					_ = cmie.Parameters.Add(new CodeSnippetExpression(finalArgStr));
				}
			}
		}

		private void HandlePartialVariadicParams(CodeMethodInvokeExpression cmie, MethodInfo mi)
		{
			//Method that was declared.
			var mip = mi.GetParameters();
			var pi = mip.LastOrDefault();
			var lastDeclIsStar = pi.IsVariadic();
			//Method as it was called.
			var pces = cmie.Parameters.Cast<CodeExpression>();
			var cmielast = pces.LastOrDefault() as CodeMethodInvokeExpression;
			var lastCalledIsStar = cmielast != null && cmielast.Method.MethodName == "FlattenParam";

			//After all arguments were parsed, we need to take one last special action if the last argument was using the * spread operator.
			//The entire list must be converted into an array, with the last argument being preceded by a C# .. spread operator.
			//CodeDOM has no support for such so it must all be done via snippets.
			if (lastDeclIsStar && lastCalledIsStar)
			{
				var indexOfDeclStar = mip.Length - 1;
				var indexOfCalledStar = pces.Count() - 1;

				if ((cmie.Parameters.Count - indexOfDeclStar) > 1)
				{
					var callNonVariadic = pces.SkipLast(indexOfDeclStar);
					var callVariadic = pces.Skip(indexOfDeclStar);
					var argStrings = callVariadic.Select(ce => ce == callVariadic.Last() ? $"..{Ch.CodeToString(ce)}" : Ch.CodeToString(ce)).ToList();
					var finalArgStr = $"[{string.Join(", ", argStrings)}]";

					while (cmie.Parameters.Count > indexOfDeclStar)
						cmie.Parameters.Remove(cmie.Parameters[cmie.Parameters.Count - 1]);

					_ = cmie.Parameters.Add(new CodeSnippetExpression(finalArgStr));
				}
			}
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

		private (bool, PropertyInfo) PropExistsInBuiltInClass(string baseType, string p, int paramCount)
		{
			if (Script.TheScript.ReflectionsData.stringToTypeProperties.TryGetValue(p, out var props))
			{
				//Must iterate rather than look up because we only have the string name, not the type.
				foreach (var typekv in props)
				{
					if (string.Compare(typekv.Key.Name, baseType, true) == 0)
					{
						var bpi = PropExistsInTypeOrBase(typekv.Key, p, paramCount);

						if (bpi.Item1)
							return bpi;
						else
							break;
					}
				}
			}

			return (false, null);
		}

		private (bool, string, List<CommonParameterInfo>) PropExistsInTypeOrBase(string t, string p, int paramCount)
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
										return (true, prop.Name, prop.Parameters.Cast<CodeParameterDeclarationExpression>().Select(cpde => new CommonParameterInfo(
											cpde.IsVariadic(),
											cpde.Direction == FieldDirection.Ref,
											cpde.Type.BaseType
										)).ToList());

							//Wasn't found in fully declared properties, but simple assignments at the class namespace level are later treated as properties, so check those.
							var ctd = allVars.FirstOrDefault(kv => kv.Key.Name == t);

							if (ctd.Key != null)
							{
								foreach (var scopekv in ctd.Value.Where(kv => kv.Key.Length == 0))//Only want variables (properties) declared at class scope.
								{
									if (scopekv.Value.TryGetValue(p, out var ce))
									{
										return (true, p, []);//These types aren't relevant for the code that calls this function.
									}
								}
							}

							//Wasn't found in this type, so check its base.
							if (typekv.Key.BaseTypes.Count > 0)
							{
								t = typekv.Key.BaseTypes[0].BaseType;
								//The base might have been a user defined type, or a built in type. Check built in type first.
								//Ex: subclass : theclass : Array
								var bpi = PropExistsInBuiltInClass(t, p, paramCount);

								if (bpi.Item1)
									return (bpi.Item1, bpi.Item2.Name, bpi.Item2.GetIndexParameters().Select(ipi => new CommonParameterInfo(
										ipi.IsVariadic(),
										ipi.ParameterType.IsByRef,
										ipi.ParameterType.FullName
									)).ToList());

								break;//Either the property was not found, or the base was not a built in type, so try again with base class.
							}
							else
								return (false, null, []);
						}
					}

					//Nothing found in user defined types or their bases, so try built in types.
					//Ex: theclass : Array
					if (!anyFound)
					{
						var bpi = PropExistsInBuiltInClass(t, p, paramCount);

						if (bpi.Item1)
							return (bpi.Item1, bpi.Item2.Name, bpi.Item2.GetIndexParameters().Select(ipi => new CommonParameterInfo(
								ipi.IsVariadic(),
								ipi.ParameterType.IsByRef,
								ipi.ParameterType.FullName
							)).ToList());
						else
							break;
					}
				}
			}

			return (false, "", []);
		}

		private (bool, PropertyInfo) PropExistsInTypeOrBase(Type t, string p, int paramCount)
		{
			while (t != null)
			{
				var props = t.GetProperties();

				foreach (var prop in props)
					if (prop.GetIndexParameters().Length == paramCount)
						if (string.Compare(prop.Name, p, true) == 0)
							return (true, prop);

				t = t.BaseType;
			}

			return (false, null);
		}

		private CodeExpression ReevaluateCodeVariableReference(CodeTypeDeclaration ctd, string scope, CodeVariableReferenceExpression cvre)
		{
			var doThis = false;
			string str = null;
			var varName = cvre.VariableName.TrimStart('@');

			if (MethodExistsInTypeOrBase(targetClass.Name, varName) is CodeMemberMethod cmm)
				str = cmm.Name;
			else if (targetClass != ctd && MethodExistsInTypeOrBase(ctd.Name, varName) is CodeMemberMethod cmm2)
			{
				if (!cvre.UserData["isstatic"].Ab()
						&&
						(scope == "" //Was a class property declaration and assignment.
						 ||
						 //Or was inside of a non-static class method.
						 ctd.Members.Cast<CodeTypeMember>().Any(ctm => ctm is CodeMemberMethod cmm
								 && string.Compare(cmm.Name, scope, true) == 0
								 && !ctm.Attributes.HasFlag(MemberAttributes.Static))
						))
					doThis = true;

				str = cmm2.Name;
			}
			else if (Reflections.FindBuiltInMethod(varName, -1) is MethodPropertyHolder mph)
				str = mph.mi.DeclaringType.FullName + "." + mph.mi.Name;

			if (str != null)
				if (/*scope == "" ||*/ !VarExistsAtCurrentOrParentScope(ctd, scope, Ch.CreateEscapedIdentifier(cvre.VariableName)))
				{
					cvre.VariableName = str;
					var tempfunc = (CodeMethodReferenceExpression)InternalMethods.Func;

					if (doThis)
						return new CodeMethodInvokeExpression(tempfunc, cvre, new CodeThisReferenceExpression());
					else
						return new CodeMethodInvokeExpression(tempfunc, cvre);
				}

			return cvre;
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
				foreach (CodeTypeMember typemember in currentType.Members)//Check if the type contains the variable.
				{
					if (typemember is CodeSnippetTypeMember cstm && string.Compare(Ch.CreateEscapedIdentifier(cstm.Name), varName, true) == 0)
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
					if (method.Parameters.Cast<CodeParameterDeclarationExpression>().Any(p => string.Compare(Ch.CreateEscapedIdentifier(p.Name), varName, true) == 0))
						return true;
				}
			}

			if (excCatchVars.TryPeek(out var exc))
				if (exc.Contains(varName))
					return true;

			if (currentFuncParams.TryPeek(out var pl))
				if (pl.Contains(varName))
					return true;

			if (staticFuncVars.TryGetValue(currentType, out var st))
				if (st.TryPeek(out var stat))
					if (stat.TryGetValue(varName, out var sv))
						return true;

			if (currentType != targetClass)//Check if it's a property in the current class.
			{
				foreach (CodeTypeMember typemember in targetClass.Members)
				{
					if (typemember is CodeSnippetTypeMember cstm && string.Compare(Ch.CreateEscapedIdentifier(cstm.Name), varName, true) == 0)
						return true;
				}
			}

			if (allVars.TryGetValue(targetClass, out var globalTypeFuncs))
			{
				if (globalTypeFuncs.TryGetValue("", out var scopeVars))//The type didn't contain the variable, so check if the global scope contains it.
				{
					if (scopeVars.ContainsKey(varName))
						return true;
				}
			}

			if (PropExistsInTypeOrBase(currentType.Name, varName, -1).Item1)//Finally, check the base classes.
				return true;

			return false;
		}

		internal class CommonParameterInfo
		{
			internal bool IsRef { get; private set; }
			internal bool IsVariadic { get; private set; }
			internal string Type { get; private set; }

			internal CommonParameterInfo(bool isVariadic, bool isRef, string type)
			{
				IsVariadic = isVariadic;
				IsRef = isRef;
				Type = type;
			}

			public bool Equals(CommonParameterInfo other) => IsVariadic == other.IsVariadic
			&& IsRef == other.IsRef
			&& Type == other.Type;

			public override bool Equals(object other) => other is CommonParameterInfo cpi && this == cpi;

			public override int GetHashCode() => base.GetHashCode();
		}
	}
}