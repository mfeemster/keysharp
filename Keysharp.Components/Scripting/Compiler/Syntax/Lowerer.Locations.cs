using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Keysharp.Components.Scripting;
using Keysharp.Parsing.Syntax;

namespace Keysharp.Compilation.Syntax
{
	// Statement locations: before a statement runs, it writes where it is into the frame the dispatcher pushed for its
	// function, as AutoHotkey keeps its current line, so an Error reports the line of every frame without any PDB.
	internal sealed partial class Lowerer
	{
		private const string FrameLocal = "KS_frame";
		private const int LineBits = Keysharp.Runtime.CallStack.LineBits;

		// Marks the location stamps, which --transpile leaves out: they instrument the program rather than express it.
		internal static readonly SyntaxAnnotation LocationAnnotation = new("Keysharp.Location");

		// The dispatcher keeps this frame until return. Capturing the class avoids Roslyn's ref-local handling.
		// Build the local directly to avoid initializing Roslyn's parser.
		private static readonly StatementSyntax LocationDecl = SyntaxFactory.LocalDeclarationStatement(
				SyntaxFactory.VariableDeclaration(Ty("Keysharp.Runtime.CallStack.Frame"))
					.AddVariables(SyntaxFactory.VariableDeclarator(FrameLocal).WithInitializer(SyntaxFactory.EqualsValueClause(
						Access("Keysharp.Runtime.CallStack.Top")))))
			.WithAdditionalAnnotations(LocationAnnotation);

		private static readonly System.StringComparer SourcePathComparer = Parser.SourcePathComparer;
		private readonly ObjectStackPool<List<Stmt>> _scratchLowerBody = new(() => [], item => item.Clear());
		// The text each file was parsed from, by path.
		private readonly Dictionary<string, string> _sourceTexts = new(SourcePathComparer);
		// The files statements come from, with their text, by file index: the main script first, then the others in the
		// order the lowering meets them.
		private readonly OrderedDictionary<string, string> _sourceFiles = new(SourcePathComparer);


		private bool _locationOverflowReported;
		private bool _stamped;

		/// <summary>The text of each file a script run from source was compiled from, by file index; null for compiled output.</summary>
		public IReadOnlyList<string> SourceTexts => _output != ScriptCompilationOutput.InMemory ? null : [.. _sourceFiles.Values];

		private void AddSourceTexts(string file, string text, ProgramNode prog)
		{
			_sourceTexts[file] = text;

			foreach (var (path, included) in prog.IncludedSources)
				_sourceTexts[path] = included;
		}

		private int Location(Node node)
		{
			var file = string.IsNullOrEmpty(node.File) ? _scriptPath : node.File;
			var index = _sourceFiles.IndexOf(file);

			if (index < 0)
			{
				index = _sourceFiles.Count;
				_sourceFiles.Add(file, _sourceTexts.GetValueOrDefault(file));
			}

			if ((uint)node.Line >= 1u << LineBits || (uint)index >= 1u << (32 - LineBits))
			{
				if (!_locationOverflowReported)
				{
					_locationOverflowReported = true;
					Diag($"{NodeAnchor(node)}source location exceeds the supported limit of 1,048,575 lines or 4,096 files");
				}

				return 0;
			}

			return index << LineBits | node.Line;
		}

		private ExpressionSyntax SetLocation(Node node)
		{
			_stamped = true;
			return Assign(Member(Id(FrameLocal), "Location"), IntLit(Location(node)));
		}

		private StatementSyntax Stamp(Node node) => ExprStmt(SetLocation(node)).WithAdditionalAnnotations(LocationAnnotation);

		// A case value evaluated at run time records its own location, as the switch's is left behind by then.
		// `(KS_frame.Location = location) >= int.MinValue && test` does so without changing the test's result.
		private ExpressionSyntax CaseGuard(Expr value, ExpressionSyntax test) => value is LiteralExpr ? test
			: SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression,
				SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanOrEqualExpression,
					SyntaxFactory.ParenthesizedExpression(SetLocation(value)), Access("System.Int32.MinValue")),
				test).WithAdditionalAnnotations(LocationAnnotation);

		// Adds a lowered statement to a body, after its stamp unless it runs nothing able to raise an error.
		private void AddStmt(List<StatementSyntax> body, Stmt s)
		{
			if (LowerStmt(s) is not { } lowered)
				return;

			if (s is not (Block or LabelStmt or FunctionDecl or DirectiveStmt or BreakStmt or ContinueStmt or GotoStmt or ReturnStmt { Value: null }))
				body.Add(Stamp(s));

			body.Add(lowered);
		}

		// The body of an if, loop, try or catch, lowered as a statement list so that a single statement records its location too.
		private BlockSyntax LowerBody(Stmt body)
		{
			if (body is Block block)
			{
				return SyntaxFactory.Block(LowerStmtList(block.Body));
			}
			else
			{
				var bodyList = _scratchLowerBody.Get();
				bodyList.Add(body);
				var result = SyntaxFactory.Block(LowerStmtList(bodyList));
				_scratchLowerBody.Return(bodyList);
				return result;
			}
		}

		// Compiled output can name two files alike, and the later one then gains a number before its extension.
		private AttributeListSyntax SourceFilesAttribute()
		{
			var names = _sourceFiles.Keys.Select(SourceName).ToArray();
			var taken = new HashSet<string>(names, SourcePathComparer);
			var seen = new HashSet<string>(SourcePathComparer);

			for (var i = 0; i < names.Length; i++)
			{
				if (seen.Add(names[i]))
					continue;

				var extension = System.IO.Path.GetExtension(names[i]);
				var stem = names[i][..^extension.Length];
				var suffix = 1;

				do names[i] = $"{stem} ({++suffix}){extension}";
				while (!taken.Add(names[i]));
			}

			return Attr("Keysharp.Runtime.SourceFiles", names.Select(Str).ToArray());
		}

		// A script run from source names its files by full path. Compiled output names library files by their library
		// folder, other files inside the script's folder relative to it, and remaining files by name alone.
		private string SourceName(string file)
		{
			if (_output == ScriptCompilationOutput.InMemory || file == "*")
				return file;

			foreach (var (dir, marker) in Parser.SharedLibraryDirs())
				if (Below(dir, file) is { } relative)
					return marker + relative;

			if (Below(_includeDir ?? System.IO.Path.GetDirectoryName(_scriptPath), file) is { } local)
				return "./" + local;

			return "<External>/" + System.IO.Path.GetFileName(file);
		}

		// The path of a file inside a folder, with forward slashes, or null when it is not inside.
		private static string Below(string folder, string file)
		{
			if (string.IsNullOrEmpty(folder))
				return null;

			var relative = System.IO.Path.GetRelativePath(folder, file).Replace('\\', '/');
			return System.IO.Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith("../") ? null : relative;
		}
	}
}
