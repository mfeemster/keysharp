using slmd = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.CodeDom.CodeMethodInvokeExpression>>;
using tsmd = System.Collections.Generic.Dictionary<System.CodeDom.CodeTypeDeclaration, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.CodeDom.CodeMethodInvokeExpression>>>;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Antlr4.Runtime;
using static System.Net.Mime.MediaTypeNames;
using System;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Diagnostics.Metrics;
using Antlr4.Runtime.Atn;
using Keysharp.Core;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Scripting.Parser.Helpers;
using System.Collections;

namespace Keysharp.Scripting
{
    public enum eScope
    {
        Local,
        Global,
        Static
    }
    public enum eNameCase
    {
        Lower,
        Upper,
        Title
    };
    internal partial class Parser
	{
        public static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
		public static readonly CultureInfo inv = CultureInfo.InvariantCulture;

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

		internal static List<string> nonContExprOperatorsList = ["++", "--"];
		internal static CodePrimitiveExpression nullPrimitive = new (null);
		internal static CodeTypeReference objTypeRef = new (typeof(object));
		internal static CodeTypeReference ctrpaa = new (typeof(ParamArrayAttribute));
		internal static CodeTypeReference ctrdva = new (typeof(DefaultValueAttribute));
		internal static CodeAttributeDeclaration cad;
		internal static CodePrimitiveExpression emptyStringPrimitive = new ("");

		internal static FrozenSet<string> propKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			FlowGet,
			FlowSet
		} .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

		internal static FrozenSet<string>.AlternateLookup<ReadOnlySpan<char>> propKeywordsAlt = propKeywords.GetAlternateLookup<ReadOnlySpan<char>>();

		internal bool errorStdOut;
		internal CodeStatementCollection initial = [];//These are placed at the very beginning of Main().
		internal List<string> mainFuncInitial = new();
		internal string name = string.Empty;
		internal bool noTrayIcon;
		internal bool persistent;
		internal bool persistentValueSetByUser;
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

		private readonly Stack<List<string>> globalFuncVars = new ();
		private readonly Stack<List<string>> localFuncVars = new ();

        private readonly CompilerHelper Ch;
        public string fileName;

        private readonly char[] ops = [Equal, Not, Greater, Less];
		public List<IToken> codeTokens = [];
		private List<CodeLine> codeLines = [];

		private uint switchCount;
		private uint tryCount;

        public CompilationUnitSyntax compilationUnit;
        public NamespaceDeclarationSyntax namespaceDeclaration;
        public Class mainClass;
        public Function mainFunc;
        public Function autoExecFunc;
        public Function currentFunc;
        public SeparatedSyntaxList<AttributeSyntax> assemblies = new();
        public List<StatementSyntax> DHHR = new(); // positional directives, hotkeys, hotstrings, remaps
        public uint hotIfCount = 0;
        public uint hotkeyCount = 0;
        public uint hotstringCount = 0;
        public bool isHotkeyDefinition = false;

        internal PreReader reader;

        public Stack<(Function, HashSet<string>)> FunctionStack = new();
		public Stack<Class> ClassStack = new();

        public Class currentClass;

        public HashSet<string> globalVars = [];
        public HashSet<string> accessibleVars = [];

		public static Dictionary<string, Type> BuiltinTypes = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> AllTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> UserTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> UserFuncs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public uint loopDepth = 0;
        public string loopLabel = null;
        public uint functionDepth = 0;
		public uint classDepth = 0;
        public uint tryDepth = 0;
        public uint tempVarCount = 0;

		public uint lambdaCount = 0;

        public const string LoopEnumeratorBaseName = InternalPrefix + "e";

        public static NameSyntax ScriptOperateName = CreateQualifiedName("Keysharp.Scripting.Script.Operate");
        public static NameSyntax ScriptOperateUnaryName = CreateQualifiedName("Keysharp.Scripting.Script.OperateUnary");

		public List<StatementSyntax> generalDirectiveStatements = new();
		public Dictionary<string, string> generalDirectives = new(StringComparer.InvariantCultureIgnoreCase) 
		{
			{ "ClipboardTimeout", null },
            { "ErrorStdOut", null },
            { "HotIfTimeout", null },
            { "MaxThreads", null },
            { "MaxThreadsBuffer", null },
            { "MaxThreadsPerHotkey", null },
            { "NoTrayIcon", null },
            { "WinActivateForce", null },
            { "AssemblyTitle", null },
            { "AssemblyDescription", null },
            { "AssemblyConfiguration", null },
            { "AssemblyCompany", null },
            { "AssemblyProduct", null },
            { "AssemblyCopyright", null },
            { "AssemblyTrademark", null },
			{ "AssemblyVersion", null }
        };

		public static Dictionary<string, string> TypeNameAliases = new(StringComparer.OrdinalIgnoreCase)
		{
			{ "int64", "Integer" },
            { "double", "Float" },
            { "KeysharpObject", "Object" },
            { "FuncObj", "Func" }
		};

        public static ParameterSyntax ThisParam = SyntaxFactory.Parameter(SyntaxFactory.Identifier("@this"))
                    .WithType(PredefinedTypes.Object);

        public static class PredefinedTypes
        {
            public static TypeSyntax Object = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));

            public static TypeSyntax ObjectArray = SyntaxFactory.ArrayType(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                .WithRankSpecifiers(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.ArrayRankSpecifier(
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.OmittedArraySizeExpression()))));

            public static TypeSyntax StringArray = SyntaxFactory.ArrayType(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                .WithRankSpecifiers(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.ArrayRankSpecifier(
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.OmittedArraySizeExpression()))));
        }

        public class Function
        {
            public MethodDeclarationSyntax Method = null;
            public string Name = null;
            public List<StatementSyntax> Body = new();
            public List<ParameterSyntax> Params = new();
            public Dictionary<string, StatementSyntax> Locals = new();
            public HashSet<string> Globals = new HashSet<string>();
            public HashSet<string> Statics = new HashSet<string>();
            public HashSet<string> VarRefs = new HashSet<string>();
            public eScope Scope = eScope.Local;

            public bool Void = false;
            public bool Async = false;
			public bool Public = true;
			public bool Static = true;
            public bool UserStatic = false;

            public bool HasDerefs = false;

            public Function(string name, TypeSyntax returnType = null)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name cannot be null or empty.", nameof(name));

                if (returnType == null)
                    returnType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));

                Name = name;
                Method = SyntaxFactory.MethodDeclaration(returnType, name);
            }

            public BlockSyntax AssembleBody()
            {
                var statements = Locals.Values.ToList();

                if (HasDerefs && Name != Keywords.AutoExecSectionName)
                {
                    var arguments = new List<ArgumentSyntax>();

                    arguments.Add(
                        SyntaxFactory.Argument(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseName("Keysharp.Scripting.eScope"),
                                SyntaxFactory.IdentifierName(Scope.ToString())
                            )
                        )
                    );

                    if (Globals.Count == 0)
                        arguments.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)));
                    else
                    {
                        var literalList = new List<ExpressionSyntax>();
                        foreach (var s in Globals)
                            literalList.Add(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,SyntaxFactory.Literal(s)));

                        // Create an array initializer: { "item1", "item2", ... }
                        var arrayInitializer = SyntaxFactory.InitializerExpression(
                            SyntaxKind.ArrayInitializerExpression,
                            SyntaxFactory.SeparatedList(literalList)
                        );

                        // Create an array creation expression: new string[] { "item1", "item2", ... }
                        var arrayCreation = SyntaxFactory.ArrayCreationExpression(
                            SyntaxFactory.ArrayType(
                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                SyntaxFactory.SingletonList(
                                    SyntaxFactory.ArrayRankSpecifier(
                                        SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                            SyntaxFactory.OmittedArraySizeExpression()
                                        )
                                    )
                                )
                            )
                        ).WithInitializer(arrayInitializer);

                        // Create the object creation expression:
                        // new HashSet<string>( new string[] { ... }, StringComparer.OrdinalIgnoreCase )
                        var objectCreation = SyntaxFactory.ObjectCreationExpression(
                            // Use a generic name for HashSet<string>
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("HashSet")
                            ).WithTypeArgumentList(
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))
                                    )
                                )
                            )
                        ).WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                    new ArgumentSyntax[]
                                    {
                                        // First argument: the string array we just created.
                                        SyntaxFactory.Argument(arrayCreation),
                                        // Second argument: StringComparer.OrdinalIgnoreCase
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName("StringComparer"),
                                                SyntaxFactory.IdentifierName("OrdinalIgnoreCase")
                                            )
                                        )
                                    }
                                )
                            )
                        );

                        arguments.Add(SyntaxFactory.Argument(objectCreation));
                    }

                    foreach (string localName in Locals.Keys) {
                        arguments.Add(
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression, 
                                    SyntaxFactory.Literal(localName)
                                )
                            )
                        );

                        arguments.Add(
                            SyntaxFactory.Argument(
                                Parser.ConstructVarRefFromIdentifier(localName)
                            )
                        );
                    }

                    foreach (string staticName in Statics)
                    {
                        arguments.Add(
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(staticName.Substring(Name.Length + 1))
                                )
                            )
                        );

                        arguments.Add(
                            SyntaxFactory.Argument(
                                Parser.ConstructVarRefFromIdentifier(staticName)
                            )
                        );
                    }

                    // Create the object creation expression:
                    //   new Keysharp.Scripting.Script.Variables.Dereference()
                    ObjectCreationExpressionSyntax newExpr = SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.ParseTypeName("Keysharp.Scripting.Script.Variables.Dereference"))
                        .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments)));

                    // Create the variable declarator for _ks_Derefs with its initializer.
                    VariableDeclaratorSyntax varDeclarator = SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(InternalPrefix + "Derefs"))
                        .WithInitializer(SyntaxFactory.EqualsValueClause(newExpr));

                    // Create the variable declaration: "Dereference _ks_Derefs = new Keysharp.Scripting.Script.Variables.Dereference();"
                    VariableDeclarationSyntax varDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("Keysharp.Scripting.Script.Variables.Dereference"))
                        .WithVariables(SyntaxFactory.SingletonSeparatedList(varDeclarator));

                    statements.Add(SyntaxFactory.LocalDeclarationStatement(varDeclaration));
                }

                statements.AddRange(Body);

                if (!Void)
                {
                    bool hasReturn = statements.OfType<ReturnStatementSyntax>().Any();

                    if (!hasReturn)
                    {
                        // Append a default return ""; statement
                        var defaultReturn = SyntaxFactory.ReturnStatement(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(""))
                        );

                        statements.Add(defaultReturn);
                    }
                }

                return SyntaxFactory.Block(statements);
            }

            public ParameterListSyntax AssembleParams() => SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>(Params));

            public MethodDeclarationSyntax Assemble()
            {
                var modifiers = new List<SyntaxToken>();
				if (Async)
                    modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
                if (Public)
                    modifiers.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
				if (Static)
                    modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

                return Method
                .WithParameterList(AssembleParams())
                .WithBody(AssembleBody())
                .WithModifiers(modifiers.Count == 0 ? default : SyntaxFactory.TokenList(modifiers));
            }
        }

        public class Class
        {
            public string Name = null;
			public string UserDeclaredName = null;
            public string Base = "KeysharpObject";
			public List<BaseTypeSyntax> BaseList = new();
            public List<MemberDeclarationSyntax> Body = new List<MemberDeclarationSyntax>();
            public ClassDeclarationSyntax Declaration = null;

            public bool isInitialization = false;
            public readonly List<(ExpressionSyntax BaseExpr, ExpressionSyntax TargetExpr, ExpressionSyntax Initializer)> deferredInitializations = new();
            public readonly List<(ExpressionSyntax BaseExpr, ExpressionSyntax TargetExpr, ExpressionSyntax Initializer)> deferredStaticInitializations = new();

            public Class(string name, string baseName = "KeysharpObject")
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name cannot be null or empty.", nameof(name));

                Name = name;
                Declaration = SyntaxFactory.ClassDeclaration(name)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
                if (baseName != null)
                    Declaration = Declaration.WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                                SyntaxFactory.SimpleBaseType(CreateQualifiedName(baseName))
                            )
                        )
                    );
            }

			public ClassDeclarationSyntax Assemble()
			{
				return Declaration
                    .WithBaseList(BaseList.Count > 0 ? SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(BaseList)) : default)
					.AddMembers(Body.ToArray());
            }

            public bool ContainsMethod(string methodName, bool searchStatic = false, bool caseSensitive = false)
            {
                if (Body == null) throw new ArgumentNullException(nameof(Body));
                if (string.IsNullOrEmpty(methodName)) throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

                // Adjust string comparison based on case-sensitivity
                var stringComparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                // Search for methods
                foreach (var member in Body)
                {
                    if (member is MethodDeclarationSyntax method)
                    {
                        var methName = method.Identifier.Text;
                        bool isStatic = false;
                        if (methName.StartsWith(Keywords.ClassStaticPrefix))
                        {
                            methName = methName.Substring(Keywords.ClassStaticPrefix.Length);
                            isStatic = true;
                        }
                        // Check method name
                        if (string.Equals(methName, methodName, stringComparison))
                        {
                            if (isStatic == searchStatic) return true;
                        }
                    }
                }

                return false;
            }
        }


        public Parser(CompilerHelper ch)
		{
			Ch = ch;
		}

        public static bool IsTypeOrBase(Type t1, string t2)
        {
            while (t1 != null)
            {
                var nameToUse = t1.FullName;

                if (!string.IsNullOrEmpty(t1.Namespace))
                    nameToUse = nameToUse.TrimStartOf($"{t1.Namespace}.").Replace('+', '.').TrimStartOf(Keywords.MainClassName + ".");

                if (string.Compare(nameToUse, t2, true) == 0)
                    return true;

                t1 = t1.BaseType;
            }

            return false;
        }

        public T Parse<T>(TextReader codeStream) => Parse<T>(codeStream, string.Empty);

		public T Parse<T>(TextReader codeStream, string nm)
		{
			name = nm;
            reader = new PreReader(this);

			fileName = name;
            codeTokens = reader.ReadTokens(codeStream, name);

            var codeTokenSource = new ListTokenSource(codeTokens);
            var codeTokenStream = new CommonTokenStream(codeTokenSource);

            //Console.WriteLine("Lexed");

            /*
            foreach (var token in codeTokens)
            {
                Debug.WriteLine($"Token: {mainLexer.Vocabulary.GetSymbolicName(token.Type)}, Text: '{token.Text}'");
            }
			*/

            MainParser mainParser = new MainParser(codeTokenStream);

            //mainParser.EnableProfiling();

            //mainParser.Trace = true;
            //var listener = new TraceListener();
            //mainParser.AddParseListener(listener);

            //mainParser.ErrorHandler = new BailErrorStrategy();
            //mainParser.AddErrorListener(new DiagnosticErrorListener());
            //mainParser.Interpreter.PredictionMode = PredictionMode.LL_EXACT_AMBIG_DETECTION;

            //var profilingATNSimulator = new ProfilingATNSimulator(mainParser);

            MainParser.ProgramContext programContext = mainParser.program();

            //ProfileParser(mainParser);
            //Console.WriteLine("End");

            VisitMain visitor = new VisitMain(this);

            //var profilingATNSimulator = new ProfilingATNSimulator(mainParser);

            SyntaxNode compilationUnit = visitor.Visit(programContext);

            //var decisionInfo = profilingATNSimulator.getDecisionInfo();
            //Debug.WriteLine(decisionInfo);

            var parseOptions = new CSharpParseOptions(
                languageVersion: LanguageVersion.LatestMajor,
                documentationMode: DocumentationMode.None,
                kind: SourceCodeKind.Regular);

            return (T)(object)SyntaxFactory.SyntaxTree(compilationUnit, parseOptions);
		}

        static void ProfileParser(MainParser parser)
        {
            var profilingATN = parser.GetProfiler();
            var parseInfo = new ParseInfo(profilingATN);

            Console.WriteLine(string.Format("{0,-35}{1,-15}{2,-15}{3,-15}{4,-15}{5,-15}{6,-15}",
                "Rule", "Time", "Invocations", "Lookahead", "Lookahead(Max)", "Ambiguities", "Errors"));

            var ruleNames = parser.RuleNames;
            System.Diagnostics.Debug.WriteLine(string.Format("{0,-35}{1,-15}{2,-15}{3,-15}{4,-15}{5,-15}{6,-15}",
				"Rule",
				"TimeInPrediction",
				"Invocations",
				"SLL_TotalLook",
				"SLL_MaxLook",
				"Abiguities count",
				"Errors"));

            foreach (var decisionInfo in parseInfo.getDecisionInfo())
            {
                var decisionState = parser.Atn.GetDecisionState(decisionInfo.decision);
                var ruleName = ruleNames[decisionState.ruleIndex];

                if (decisionInfo.timeInPrediction > 0)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0,-35}{1,-15}{2,-15}{3,-15}{4,-15}{5,-15}{6,-15}",
                        ruleName,
                        decisionInfo.timeInPrediction,
                        decisionInfo.invocations,
                        decisionInfo.SLL_TotalLook,
                        decisionInfo.SLL_MaxLook,
                        decisionInfo.ambiguities.Count,
                        decisionInfo.errors));
                }
            }
        }

        [PublicForTestOnly]
        public static string EscapeHotkeyTrigger(ReadOnlySpan<char> s)
        {
            var escaped = false;
            var sb = new StringBuilder(s.Length);
            char ch = (char)0;

            for (var i = 0; i < s.Length; ++i)
            {
                ch = s[i];
                escaped = i == 0 && ch == '`';

                if (!escaped)
                    sb.Append(ch);
            }

            if (escaped)
                sb.Append(ch);

            return sb.ToString();
        }
    }
}