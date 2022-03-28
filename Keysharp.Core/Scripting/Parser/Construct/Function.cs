using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private const string args = "args";
		private const string initParams = "initparams";

		private Stack<bool> allGlobalVars = new Stack<bool>();
		private Stack<bool> allStaticVars = new Stack<bool>();
		private Dictionary<string, SortedSet<string>> allVars = new Dictionary<string, SortedSet<string>>();
		private Stack<List<string>> currentFuncParams = new Stack<List<string>>();
		private Stack<HashSet<string>> excCatchVars = new Stack<HashSet<string>>();
		private Stack<List<string>> globalFuncVars = new Stack<List<string>>();
		private Stack<List<string>> localFuncVars = new Stack<List<string>>();
		private Stack<Dictionary<string, CodeExpression>> staticFuncVars = new Stack<Dictionary<string, CodeExpression>>();
		//This will probably not work with global funcs and class funcs with the same name, need some type of more granular scoping.//MATT

		public string ParseFunction(CodeLine line)
		{
			StartNewFunction();
			var code = line.Code;
			int i;
			var buf = new StringBuilder();
			string name;

			for (i = 0; i < code.Length; i++)
			{
				var sym = code[i];

				if (IsIdentifier(sym))
					_ = buf.Append(sym);
				else if (sym == ParenOpen)
				{
					i++;
					break;
				}
				else
					throw new ParseException(ExUnexpected);
			}

			if (buf.Length == 0)
				throw new ParseException(ExUnexpected);

			name = buf.ToString();
			buf.Length = 0;

			if (IsLocalMethodReference(name))
				throw new ParseException($"Duplicate function {name}");

			var blockType = CodeBlock.BlockType.Expect;
			var str = false;
			var stop = false;

			for (; i < code.Length; i++)
			{
				var sym = code[i];

				switch (sym)
				{
					case StringBound:
						str = !str;
						goto default;

					case ParenClose:
						if (str)
							goto default;
						else
							stop = true;

						break;

					default:
						_ = buf.Append(sym);
						break;
				}

				if (stop)
					break;
			}

			if (!stop)
				throw new ParseException("Expected closing parenthesis");

			var param = buf.ToString();
			buf.Length = 0;
			i++;

			for (; i < code.Length; i++)
			{
				var sym = code[i];

				if (sym == BlockOpen)
				{
					blockType = CodeBlock.BlockType.Within;
					break;
				}
				else if (IsCommentAt(code, i))
					break;
				else if (!IsSpace(sym))
					throw new ParseException(ExUnexpected);
			}

			var method = LocalMethod(name);
			var block = new CodeBlock(line, method.Name, method.Statements, CodeBlock.BlockKind.Function, blocks.PeekOrNull());
			block.Type = blockType;
			_ = CloseTopSingleBlock();
			blocks.Push(block);
			var fix = ParseFunctionParameters(param);

			if (fix.Item1 != null)//This creates the code that puts parameters into the global map.//MATT
			{
				//method.Parameters.Clear();//MATT
				//method.Parameters.AddRange(fix);
				//_ = method.Statements.Add(fix.Item1);
				//method.Statements.AddRange(fix.Item1);
				method.UserData.Add(initParams, fix.Item1);//Save it for later, will be added to the statements when the function is done being parsed.
				currentFuncParams.Peek().AddRange(fix.Item2);
			}

			//method.Parameters.Add(new CodeParameterDeclarationExpression("System.Object", "x"));
			methods.Add(method.Name, method);
			//This appears to be trying to create some type of delegate, but is not what we want to do.//MATT
			//var type = typeof(Core.Core.GenericFunction);
			//var typeref = new CodeTypeReference();
			//typeref.UserData.Add(RawData, type);
			//var del = new CodeDelegateCreateExpression(typeref, new CodeTypeReferenceExpression(className), method.Name);
			//var obj = VarAssign(VarRef(mainScope + ScopeVar + method.Name), del);
			//_ = prepend.Add(new CodeExpressionStatement(obj));
			//allGlobalVars = false;
			//globalFuncVars.Clear();
			//localFuncVars.Clear();
			return method.Name;
		}

		public string ParseFunctionName(string code)
		{
			var buf = new StringBuilder();

			for (var i = 0; i < code.Length; i++)
			{
				var sym = code[i];

				if (IsIdentifier(sym))
					_ = buf.Append(sym);
				else if (sym == ParenOpen)
					break;
				else
					throw new ParseException(ExUnexpected);
			}

			if (buf.Length == 0)
				throw new ParseException(ExUnexpected);

			return buf.ToString();
		}

		internal void StartNewFunction()
		{
			_ = CloseTopLabelBlock();
			allGlobalVars.Push(false);
			allStaticVars.Push(false);
			globalFuncVars.Push(new List<string>());
			localFuncVars.Push(new List<string>());
			staticFuncVars.Push(new Dictionary<string, CodeExpression>());
			currentFuncParams.Push(new List<string>());
		}

		private (CodeStatementCollection, List<string>) ParseFunctionParameters(string code)
		//private CodeParameterDeclarationExpression[] ParseFunctionParameters(string code)//What we want is an array of parameters.//MATT
		{
			//var paramdecs = new List<CodeParameterDeclarationExpression>();
			//List<CodePrimitiveExpression> names = new List<CodePrimitiveExpression>(), defaults = new List<CodePrimitiveExpression>();
			var names = new List<CodePrimitiveExpression>();
			var defaults = new List<CodeExpression>();
			var i = 0;

			while (i < code.Length)
			{
				int x;
				//CodeParameterDeclarationExpression cpde;

				// preceeding space
				while (IsSpace(code[i])) i++;

				// name
				x = i;

				while (i < code.Length && IsIdentifier(code[i])) i++;

				if (x == i)
					throw new ParseException(ExUnexpected);
				else
				{
					var part = code.Substring(x, i - x);
					//bool byref;// = false;

					if (part.Equals(FunctionParamRef, System.StringComparison.OrdinalIgnoreCase))
					{
						//byref = true; // TODO: handle byref variables
						do { i++; } while (i < code.Length && IsSpace(code[i]));

						x = i;

						while (i < code.Length && IsIdentifier(code[i])) i++;

						if (x == i)
							throw new ParseException("Unspecified parameter name");

						part = code.Substring(x, i - x);
					}

					//cpde = new CodeParameterDeclarationExpression("System.Object", VarNormalizedName(part));//Want this, and not the code below.//MATT
					//cpde.Direction = byref ? FieldDirection.Ref : FieldDirection.In;
					//paramdecs.Add(cpde);
					//names.Add(new CodePrimitiveExpression((byref ? mainScope : Scope) + ScopeVar + VarNormalizedName(part)));
					names.Add(new CodePrimitiveExpression(VarNormalizedName(Scope + ScopeVar + VarNormalizedName(part))));//Function params are always scoped.
				}

				//if (i < code.Length && code[i] == '*')//Support variadic function parameters.//MATT
				//_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration("params"));

				while (i < code.Length && IsSpace(code[i])) i++;

				if (i == code.Length)
					break;

				// defaults
				if (code[i] == AssignPre)
				{
					i++;

					while (IsSpace(code[i]) || code[i] == Equal) i++;

					if (i == code.Length)
						throw new ParseException(ExUnexpected);
					else if (code[i] == Multicast)
					{
						//_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
						defaults.Add(new CodePrimitiveExpression(null));
					}
					else
					{
						var str = false;
						var wasstr = false;
						x = i;

						while (!(i == code.Length || (code[i] == Multicast && !str)))
						{
							if (code[i++] == StringBound)
							{
								str = !str;
								wasstr = !str;
							}
						}

						if (x == i)
						{
							//_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
							defaults.Add(new CodePrimitiveExpression(null));
						}
						else
						{
							var sub = code.Substring(x, i - x).Trim(Spaces);
							//_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
							//_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(new CodePrimitiveExpression(sub))));//MATT

							if (sub.ToLower() == "unset")
								defaults.Add(new CodeSnippetExpression("UnsetArg.Default"));
							else if (sub.Length == 0)
								defaults.Add(new CodePrimitiveExpression(null));
							else if (wasstr)
								defaults.Add(new CodePrimitiveExpression(ValidateParameterLiteral(sub)));
							else if (PrimitiveToExpression(ValidateParameterLiteral(sub)) is CodeExpression expr)
								defaults.Add(expr);
						}
					}
				}
				else
					defaults.Add(new CodePrimitiveExpression(null));

				// next
				if (i < code.Length && code[i] != Multicast && code[i] != '*')//Add support for variadic parameters specified by asterisk.//MATT
					throw new ParseException(ExUnexpected);
				else
					i++;
			}

			if (names.Count == 0)
				return (null, null);

			var statements = new CodeStatementCollection();

			for (var index = 0; index < names.Count; index++)
			{
				var cmie = new CodeMethodInvokeExpression((CodeMethodReferenceExpression)InternalMethods.Parameter,
						new CodeArgumentReferenceExpression(args),
						index < defaults.Count ? defaults[index] : new CodePrimitiveExpression(null),
						new CodePrimitiveExpression(index));
				_ = statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(object)), names[index].Value as string, cmie));
			}

			return (statements, names.Select(x => (x.Value as string)/*?.ToLower()*/).ToList());
			//var cms = new CodeExpressionStatement();
			//var fix = new CodeMethodInvokeExpression();
			//fix.Method = (CodeMethodReferenceExpression)InternalMethods.Parameters;
			//_ = fix.Parameters.Add(new CodeArrayCreateExpression(typeof(string), names.ToArray()));
			//_ = fix.Parameters.Add(new CodeArgumentReferenceExpression(args));
			//_ = fix.Parameters.Add(new CodeArrayCreateExpression(typeof(object), defaults.ToArray()));
			//return (new CodeExpressionStatement(fix), names.Select(x => (x.Value as string)/*?.ToLower()*/).ToList());
			//return paramdecs.ToArray();//MATT
		}

		private string ValidateParameterLiteral(string code)
		{
			const string err = "Default parameter value expects a literal";

			if (code.Length == 0)
				return null;

			var cs = System.StringComparison.OrdinalIgnoreCase;

			if (code.Equals(TrueTxt, cs) || code.Equals(FalseTxt, cs) || code.Equals(NullTxt, cs))
				return code;

			if (code[0] == StringBound)
			{
				var str = true;

				for (var i = 1; i < code.Length; i++)
				{
					if (code[i] == StringBound)
					{
						str = !str;
						var n = i + 1;

						if (n < code.Length && code[n] == code[i])
						{
							i = n;
						}
						else if (n != code.Length)
							throw new ParseException(err);
					}
				}

				if (str)
					throw new ParseException(err);

				code = code.Substring(1, code.Length - 2);
				_ = code.Replace(new string(StringBound, 2), string.Empty);
			}
			else if (!IsPrimitiveObject(code))
				throw new ParseException(err);

			return code;
		}
	}

	public class UnsetArg
	{
		public static readonly UnsetArg Default = new UnsetArg();
	}
}