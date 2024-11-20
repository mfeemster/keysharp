namespace Keysharp.Scripting
{
	public partial class Parser
	{
		public string ParseFunction(CodeLine codeLine, int n)
		{
			StartNewFunction();
			var code = codeLine.Code;
			int i;
			var buf = new StringBuilder();
			string name;
			var isStatic = false;

			if (code.StartsWith("static", StringComparison.OrdinalIgnoreCase))
			{
				isStatic = true;
				code = code.AsSpan(6).Trim(SpaceTab).ToString();
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
					throw new ParseException(ExUnexpected, codeLine);
			}

			if (buf.Length == 0)
				throw new ParseException(ExUnexpected, codeLine);

			name = buf.ToString();
			buf.Length = 0;

			if (!InClassDefinition() && IsLocalMethodReference(name))
				throw new ParseException("Duplicate function: \"" + name + "\".", codeLine);

			var blockType = CodeBlock.BlockType.Expect;
			var str = false;
			var stop = false;

			for (; i < code.Length; i++)
			{
				var sym = code[i];

				switch (sym)
				{
					case StringBound:
					case StringBoundVerbatim:
					{
						var origi = i;
						var rest = ParseString(codeLine, code, ref i);
						//rest = rest.Substring(1, rest.Length - 2);
						_ = buf.Append(code.Substring(origi, (i - origi) + 1));
						continue;
					}

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
				throw new ParseException("Expected closing parenthesis.", codeLine);

			var isFatArrow = false;
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
				else if (i < code.Length - 2 && sym == '=' && code[i + 1] == '>')
				{
					isFatArrow = true;
					blockType = CodeBlock.BlockType.Expect;
					break;
				}
				else if (IsCommentAt(code, i))
					break;
				else if (!IsSpace(sym))
					throw new ParseException(ExUnexpected, codeLine);
			}

			var method = LocalMethod(name);

			if (isStatic)
				method.Attributes |= MemberAttributes.Static;

			if (isFatArrow)
			{
				var theRest = code.AsSpan(i).TrimStart(FatArrow).Trim().ToString();
				codeLines.Insert(n, new CodeLine(codeLine.FileName, codeLine.LineNumber, "{"));
				codeLines.Insert(n + 1, new CodeLine(codeLine.FileName, codeLine.LineNumber + 1, $"return {theRest}"));
				codeLines.Insert(n + 2, new CodeLine(codeLine.FileName, codeLine.LineNumber + 2, "}"));
				SetLineIndexes();
			}

			var block = new CodeBlock(codeLine, method.Name, method.Statements, CodeBlock.BlockKind.Function, blocks.PeekOrNull());
			block.Type = blockType;
			_ = CloseTopSingleBlock();
			blocks.Push(block);

			if (ParseFunctionParameters(param, codeLine) is List<CodeParameterDeclarationExpression> funcparams)
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

		public string ParseFunctionName(CodeLine codeLine, string code)
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
					throw new ParseException(ExUnexpected, codeLine);
			}

			if (buf.Length == 0)
				throw new ParseException(ExUnexpected, codeLine);

			return buf.ToString();
		}

		internal void EndFunction()
		{
			_ = currentFuncParams.PopOrNull();
			_ = allGlobalVars.TryPop(out _);
			_ = allStaticVars.TryPop(out _);
			_ = globalFuncVars.PopOrNull();
			_ = localFuncVars.PopOrNull();
			_ = staticFuncVars[typeStack.Peek()].PopOrNull();
		}

		internal bool IsLocalMethodReference(string name)
		{
			foreach (var method in methods[targetClass])
				if (method.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
					return true;

			return false;
		}

		internal string LabelMethodName(string raw) => $"label_{raw.GetHashCode():X}_{labelCount++:X}";

		internal void StartNewFunction()
		{
			_ = CloseTopLabelBlock();
			allGlobalVars.Push(false);
			allStaticVars.Push(false);
			globalFuncVars.Push([]);
			localFuncVars.Push([]);
			staticFuncVars[typeStack.Peek()].Push(new Dictionary<string, CodeExpression>(StringComparer.OrdinalIgnoreCase));
			currentFuncParams.Push([]);
		}

		private CodeMemberMethod LocalMethod(string name)
		{
			var method = new CodeMemberMethod { Name = name, ReturnType = objTypeRef };
			method.Attributes = MemberAttributes.Public | MemberAttributes.Final;

			if (typeStack.PeekOrNull() == targetClass)
				method.Attributes |= MemberAttributes.Static;

			return method;
		}

		private CodeMethodInvokeExpression LocalMethodInvoke(string name)
		{
			var invoke = new CodeMethodInvokeExpression();
			invoke.Method.MethodName = name;
			invoke.Method.TargetObject = null;
			return invoke;
		}

		private List<CodeParameterDeclarationExpression> ParseFunctionParameters(string code, CodeLine codeLine)
		{
			var i = 0;
			var names = new List<CodeParameterDeclarationExpression>();

			while (i < code.Length)
			{
				int x;

				while (i < code.Length && IsSpace(code[i])) i++;// preceeding space

				x = i;// name

				while (i < code.Length && (code[i] == '&' || code[i] == '*' || code[i] == '?' || IsIdentifier(code[i]))) i++;

				if (x == i)
				{
					throw new ParseException(ExUnexpected, codeLine);
				}
				else
				{
					var part = code.Substring(x, i - x);
					var byref = false;
					var opt = false;
					var variadic = false;

					if (part.StartsWith('&'))
					{
						byref = true;
						part = part.Substring(1);
					}

					if (part == "*")
					{
						variadic = true;
						part = "args";//Parameter is variadic, but the name is unspecified, so use "args" as a default (because "params" is a reserved word).
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

					var cpde = new CodeParameterDeclarationExpression(typeof(object), part.ToLower());//All variables, including function parameters, are lowercase.

					if (opt)
					{
						_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
						_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(nullPrimitive)));
					}

					if (byref)
					{
						cpde.Direction = FieldDirection.Ref;
					}
					else if (variadic)
					{
						cpde.Type = new CodeTypeReference(typeof(object[]));
						_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(ParamArrayAttribute))));
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
						throw new ParseException(ExUnexpected, codeLine);
					}
					else if (code[i] == Multicast)
					{
						_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
						_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(nullPrimitive)));
					}
					else
					{
						var wasstr = false;
						x = i;

						while (!(i == code.Length || code[i] == Multicast))
						{
							if (code[i] == StringBound || code[i] == StringBoundVerbatim)
							{
								_ = ParseString(codeLine, code, ref i);
								wasstr = true;
							}

							i++;
						}

						if (x == i)
						{
							_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
							_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(nullPrimitive)));
						}
						else
						{
							var sub = code.AsSpan(x, i - x).Trim(Spaces).ToString();

							if (sub.ToLower() == "unset")
							{
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(nullPrimitive)));
								//currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(new CodeSnippetExpression("UnsetArg.Default"))));
							}
							else if (sub.Length == 0)
							{
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(nullPrimitive)));
							}
							else if (wasstr)
							{
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(new CodePrimitiveExpression(ValidateParameterLiteral(sub, codeLine)))));
							}
							else if (PrimitiveToExpression(ValidateParameterLiteral(sub, codeLine)) is CodeExpression expr)
							{
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
								_ = currParam.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(expr)));
							}
						}
					}
				}

				if (i < code.Length && code[i] != Multicast)
					throw new ParseException(ExUnexpected, codeLine);
				else
					i++;
			}

			return names.Count == 0 ? null : names;
		}

		private string ValidateParameterLiteral(string code, CodeLine codeLine)
		{
			const string err = "Default parameter value expects a literal.";

			if (code.Length == 0)
				return null;

			var cs = StringComparison.OrdinalIgnoreCase;

			if (code.Equals(TrueTxt, cs) || code.Equals(FalseTxt, cs) || code.Equals(NullTxt, cs))
				return code;

			if (code[0] == StringBound || code[0] == StringBoundVerbatim)
			{
				var i = 0;
				code = ParseString(codeLine, code, ref i);
				code = code.Substring(1, code.Length - 2);
				//code = EscapedString(code.Substring(1, code.Length - 2), false);
				//var str = true;
				//for (var i = 1; i < code.Length; i++)
				//{
				//  if (code[i] == StringBound || code[i] == StringBoundVerbatim)
				//  {
				//      str = !str;
				//      var n = i + 1;
				//      if (n < code.Length && code[n] == code[i])
				//      {
				//          i = n;
				//      }
				//      else if (n != code.Length)
				//          throw new ParseException(err);
				//  }
				//}
				//if (str)
				//  throw new ParseException(err);
				//code = code.Substring(1, code.Length - 2);
				//code = code.Replace(new string(StringBound, 2), string.Empty);
			}
			else if (!IsPrimitiveObject(code))
				throw new ParseException(err, codeLine);

			return code;
		}
	}

	public class UnsetArg
	{
		public static readonly UnsetArg Default = new UnsetArg();
	}
}