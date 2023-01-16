using System;
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
		private Dictionary<CodeTypeDeclaration, Dictionary<string, SortedDictionary<string, CodeExpression>>> allVars = new Dictionary<CodeTypeDeclaration, Dictionary<string, SortedDictionary<string, CodeExpression>>>();
		private Stack<List<string>> currentFuncParams = new Stack<List<string>>();
		private Stack<HashSet<string>> excCatchVars = new Stack<HashSet<string>>();
		private Stack<List<string>> globalFuncVars = new Stack<List<string>>();
		private Stack<List<string>> localFuncVars = new Stack<List<string>>();
		private Dictionary<CodeTypeDeclaration, Stack<Dictionary<string, CodeExpression>>> staticFuncVars = new Dictionary<CodeTypeDeclaration, Stack<Dictionary<string, CodeExpression>>>();

		public string ParseFunction(CodeLine line)
		{
			StartNewFunction();
			var code = line.Code;
			int i;
			var buf = new StringBuilder();
			string name;
			var isStatic = false;

			if (code.StartsWith("static", StringComparison.OrdinalIgnoreCase))
			{
				isStatic = true;
				code = code.Substring(6).Trim(SpaceTab);
			}

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

			var param = buf.ToString().ToLower();//All variables, including function parameters, are lowercase.
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

			if (isStatic)
				method.Attributes |= MemberAttributes.Static;

			var block = new CodeBlock(line, method.Name, method.Statements, CodeBlock.BlockKind.Function, blocks.PeekOrNull());
			block.Type = blockType;
			_ = CloseTopSingleBlock();
			blocks.Push(block);

			if (ParseFunctionParameters(param) is List<CodeParameterDeclarationExpression> funcparams)
			{
				foreach (var funcparam in funcparams)
				{
					_ = method.Parameters.Add(funcparam);
					currentFuncParams.Peek().Add(funcparam.Name);
				}
			}

			methods[typeStack.Peek()][method.Name] = method;
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
			staticFuncVars[typeStack.Peek()].Push(new Dictionary<string, CodeExpression>());
			currentFuncParams.Push(new List<string>());
		}

		private List<CodeParameterDeclarationExpression> ParseFunctionParameters(string code)
		{
			var i = 0;
			var names = new List<CodeParameterDeclarationExpression>();

			while (i < code.Length)
			{
				int x;

				while (i < code.Length && IsSpace(code[i])) i++;// preceeding space

				x = i;// name

				while (i < code.Length && (/*code[i] == '&' || */code[i] == '*' || IsIdentifier(code[i]))) i++;

				if (x == i)
				{
					throw new ParseException(ExUnexpected);
				}
				else
				{
					var part = code.Substring(x, i - x);
					//var byref = false;
					var opt = false;
					var variadic = false;
					//if (part.StartsWith('&'))
					//{
					//  byref = true;
					//  part = part.Substring(1);
					//}

					if (part == "*")
					{
						variadic = true;
						part = "obj";//Parameter is variadic, but unused.
					}
					else if (part.EndsWith('*'))
					{
						variadic = true;
						part = part.TrimEnd('*');
					}
					else if (part.EndsWith('?'))
					{
						opt = true;
						part = part.Trim('?');
					}

					var cpde = new CodeParameterDeclarationExpression(typeof(object), part);

					if (opt)
					{
						_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
						_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(new CodePrimitiveExpression(null))));
					}
					//if (byref)
					//  cpde.Direction = FieldDirection.Ref;
					else if (variadic)
					{
						cpde.Type = new CodeTypeReference(typeof(object[]));
						_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.ParamArrayAttribute))));
					}

					names.Add(cpde);
				}

				while (i < code.Length && IsSpace(code[i])) i++;

				if (i == code.Length)
					break;

				var currParam = names.Last();

				// defaults
				if (code[i] == AssignPre)
				{
					i++;

					while (i < code.Length && (IsSpace(code[i]) || code[i] == Equal)) i++;

					if (i == code.Length)
					{
						throw new ParseException(ExUnexpected);
					}
					else if (code[i] == Multicast)
					{
						_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
						_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(new CodePrimitiveExpression(null))));
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
							_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
							_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(new CodePrimitiveExpression(null))));
						}
						else
						{
							var sub = code.Substring(x, i - x).Trim(Spaces);

							if (sub.ToLower() == "unset")
							{
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(new CodePrimitiveExpression(null))));
								//currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(new CodeSnippetExpression("UnsetArg.Default"))));
							}
							else if (sub.Length == 0)
							{
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(new CodePrimitiveExpression(null))));
							}
							else if (wasstr)
							{
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(new CodePrimitiveExpression(ValidateParameterLiteral(sub)))));
							}
							else if (PrimitiveToExpression(ValidateParameterLiteral(sub)) is CodeExpression expr)
							{
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(expr)));
							}
						}
					}
				}

				if (i < code.Length && code[i] != Multicast)
					throw new ParseException(ExUnexpected);
				else
					i++;
			}

			return names.Count == 0 ? null : names;
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