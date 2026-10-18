namespace Keysharp.Parsing.Syntax
{
	// Structural children, including scope bodies. Optional children can be null; each pass selects its own scope boundary.
	internal static class AstChildren
	{
		private static readonly List<Stmt> emptyStmts = [];
		private static readonly List<Expr> emptyExprs = [];

		// Separate iterators keep each state machine from retaining every node shape's nested loops.
		internal static IEnumerable<Node> Of(Node node) => node switch
		{
			ProgramNode program => program.Body,
			UnaryExpr unary => One(unary.Operand),
			BinaryExpr binary => Two(binary.Left, binary.Right),
			AssignExpr assignment => Two(assignment.Target, assignment.Value),
			TernaryExpr ternary => Three(ternary.Cond, ternary.Then, ternary.Else),
			GroupExpr group => One(group.Inner),
			SequenceExpr sequence => sequence.Items,
			DerefExpr dereference => One(dereference.Name),
			CallExpr call => CallArguments(call.Callee, call.Args),
			MemberExpr member => One(member.Target),
			DynMemberExpr member => Two(member.Target, member.NameExpr),
			IndexExpr indexed => CallArguments(indexed.Target, indexed.Args),
			ArrayExpr array => Arguments(array.Elements),
			MapExpr map => MapEntries(map),
			ObjectExpr obj => ObjectEntries(obj),
			FatArrowExpr function => FunctionParts(function.Params, function.Body, function.BlockBody),
			ExpressionStmt expression => One(expression.Expr),
			Block block => block.Body,
			IfStmt conditional => Three(conditional.Cond, conditional.Then, conditional.Else),
			WhileStmt loop => WhileParts(loop),
			LoopStmt loop => LoopParts(loop),
			SpecialLoopStmt loop => SpecialLoopParts(loop),
			ForStmt loop => ForParts(loop),
			SwitchStmt choice => SwitchParts(choice),
			TryStmt attempt => TryParts(attempt),
			ReturnStmt ret => One(ret.Value),
			ThrowStmt thrown => One(thrown.Value),
			GotoStmt jump => One(jump.Expression),
			DeclStmt declaration => declaration.Items,
			HotkeyDef hotkey => Two(hotkey.Body, hotkey.Func),
			HotstringDef hotstring => Two(hotstring.Body, hotstring.Func),
			FunctionDecl function => FunctionParts(function.Params, function.Body, function.ArrowBody),
			ClassDecl type => ClassParts(type),
			AppDirective app => One(app.Value),
			_ => System.Array.Empty<Node>()
		};

		private static IEnumerable<Node> One(Node first) { yield return first; }
		private static IEnumerable<Node> Two(Node first, Node second) { yield return first; yield return second; }
		private static IEnumerable<Node> Three(Node first, Node second, Node third)
		{ yield return first; yield return second; yield return third; }

		private static IEnumerable<Node> Arguments(List<Argument> arguments)
		{
			foreach (var argument in arguments) { yield return argument.Value; yield return argument.NameExpr; }
		}

		private static IEnumerable<Node> CallArguments(Node target, List<Argument> arguments)
		{
			yield return target;
			foreach (var argument in arguments) { yield return argument.Value; yield return argument.NameExpr; }
		}

		private static IEnumerable<Node> MapEntries(MapExpr map)
		{
			foreach (var entry in map.Entries) { yield return entry.Key; yield return entry.Value; }
		}

		private static IEnumerable<Node> ObjectEntries(ObjectExpr obj)
		{
			foreach (var entry in obj.Entries) { yield return entry.Key; yield return entry.Value; }
		}

		private static IEnumerable<Node> FunctionParts(List<Param> parameters, Node body, Node arrow)
		{
			foreach (var parameter in parameters) yield return parameter.Default;
			yield return body;
			yield return arrow;
		}

		private static IEnumerable<Node> WhileParts(WhileStmt loop)
		{ yield return loop.Cond; yield return loop.Body; yield return loop.Until; yield return loop.Else; }

		private static IEnumerable<Node> LoopParts(LoopStmt loop)
		{ yield return loop.Count; yield return loop.Body; yield return loop.Until; yield return loop.Else; }

		private static IEnumerable<Node> ForParts(ForStmt loop)
		{ yield return loop.Enumerable; yield return loop.Body; yield return loop.Until; yield return loop.Else; }

		private static IEnumerable<Node> SpecialLoopParts(SpecialLoopStmt loop)
		{
			foreach (var argument in loop.Args ?? emptyExprs) yield return argument;
			yield return loop.Body;
			yield return loop.Until;
			yield return loop.Else;
		}

		private static IEnumerable<Node> SwitchParts(SwitchStmt choice)
		{
			yield return choice.Value;
			yield return choice.CaseSense;
			foreach (var branch in choice.Cases)
			{
				foreach (var value in branch.Values) yield return value;
				foreach (var child in branch.Body) yield return child;
			}
			foreach (var child in choice.Default ?? emptyStmts) yield return child;
		}

		private static IEnumerable<Node> TryParts(TryStmt attempt)
		{
			yield return attempt.Body;
			foreach (var catcher in attempt.Catches) yield return catcher.Body;
			yield return attempt.Else;
			yield return attempt.Finally;
		}

		private static IEnumerable<Node> ClassParts(ClassDecl type)
		{
			foreach (var import in type.Imports) yield return import;
			foreach (var field in type.Fields) { yield return field.Init; yield return field.TypeExpr; }
			foreach (var method in type.Methods)
			{
				foreach (var parameter in method.Params) yield return parameter.Default;
				yield return method.Body;
				yield return method.ArrowBody;
			}
			foreach (var property in type.Properties)
			{
				foreach (var parameter in property.Params) yield return parameter.Default;
				yield return property.GetBody;
				yield return property.GetArrow;
				yield return property.SetBody;
				yield return property.SetArrow;
			}
			foreach (var init in type.StaticInit) yield return init;
			foreach (var init in type.InstanceInit) yield return init;
			foreach (var child in type.Nested) yield return child;
			foreach (var directive in type.CSharpBlocks ?? []) yield return directive;
		}
	}
}
