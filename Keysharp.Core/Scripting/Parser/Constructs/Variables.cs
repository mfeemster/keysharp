using static Keysharp.Scripting.Keywords;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private CodeExpression InternalVariable => VarRef(string.Concat(Scope, scopeChar + "\0", InternalID), false);

		//internal CodeExpression VarId(string name, bool create, bool dyn = false) => name == args ? new CodeSnippetExpression(args) : VarId(VarIdExpand(VarNormalizedName(name)), create, dyn);
		internal CodeExpression VarId(CodeLine codeLine, string name, bool create, bool dyn = false) => VarId(VarIdExpand(codeLine, VarNormalizedName(name)), create, dyn);

		private bool IsVarAssignment(object expr) => (expr as CodeExpression).WasCboeAssign() is CodeBinaryOperatorExpression;

		private bool IsVarReference(object expr) => expr is CodeArrayIndexerExpression || expr is CodeVariableReferenceExpression;

		private CodeBinaryOperatorExpression VarAssign(CodeArrayIndexerExpression name, CodeExpression value) => new CodeBinaryOperatorExpression(name, CodeBinaryOperatorType.Assign, value);

		private CodeExpression VarId(CodeExpression name, bool create, bool dyn = false)
		{
			if (name is CodePrimitiveExpression raw)
			{
				if (raw.Value is string toplevelvar)
				{
					var tempscope = Scope;//Cache because the property does a lot of work.
					var staticscopedvar = tempscope + scopeChar + toplevelvar;
					_ = allGlobalVars.TryPeek(out var allglobal);
					_ = allStaticVars.TryPeek(out var allstat);
					_ = localFuncVars.TryPeek(out var l);
					_ = staticFuncVars[typeStack.Peek()].TryPeek(out var stat);
					_ = currentFuncParams.TryPeek(out var f);
					allglobal |= globalFuncVars.TryPeek(out var gg) && gg.Contains(toplevelvar);
					var explicitLocal = l != null && l.Contains(toplevelvar);
					var explicitstatic = stat != null && stat.TryGetValue(staticscopedvar, out _);//Static is special in that it needs to use the scoped var.
					var isstatic = allstat || explicitstatic;
					var islocal = tempscope != "" && !isstatic && (!allglobal || explicitLocal);
					var isparam = f != null && f.Contains(toplevelvar);
					create &= !excCatchVars.TryPeek(out var exc) || (!exc.Contains(toplevelvar) && !exc.Contains(toplevelvar));//Need either no exceptions, or exception var didn't match, to create. Use toplevelvar because scope was already prepended when the catch was parsed.

					//The logic in this function has grown unweildy and needs to be reworked and condensed.
					if (explicitLocal)
						isstatic = false;//Local always overrides static.

					if (explicitstatic) { }
					else if (!islocal)
						create &= !allglobal;

					//Don't ever declare a local variable named value if we're inside of a property.
					if (blocks.PeekOrNull() is CodeBlock block && block.Kind == CodeBlock.BlockKind.PropSet && string.Compare(toplevelvar, "value", true) == 0)
						create = false;

					if (!dyn)
					{
						if (create)
						{
							if (!isparam && isstatic && stat != null)
							{
								_ = stat.GetOrAdd(staticscopedvar);
							}
							else if (!isparam && !isstatic/* && islocal*/)
							{
								var dkt = allVars[typeStack.Peek()].GetOrAdd(tempscope);

								if (!dkt.ContainsKey(toplevelvar))//Ensure it hasn't been declared and initialized yet.
									dkt[toplevelvar] = nullPrimitive;
							}
							else if (!isparam && allglobal)
							{
								var dkt = allVars[typeStack.Peek()].GetOrAdd("");

								if (!dkt.ContainsKey(toplevelvar))
									dkt[toplevelvar] = nullPrimitive;
							}
						}

						return isparam ? VarRef(toplevelvar, false) : VarRef(!isstatic ? toplevelvar : staticscopedvar, false);
					}
					else
						return VarRef(toplevelvar, true);//Dyn vars only work with global static vars, not local function vars.
				}
			}

			return name;//It was something else other than CodePrimitiveExpression, so it was probably a CodeArrayIndexerExpression.
		}

		private CodeExpression VarIdExpand(CodeLine codeLine, string code)
		{
			code = EscapedString(code, true);

			if (IsPrimitiveObject(code, out var result))
				return result is long l ? new CodeSnippetExpression($"{l}L") : new CodePrimitiveExpression(result);

			if (code.IndexOf(Resolve) == -1)
				return new CodePrimitiveExpression(code);

			if (!DynamicVars)
			{
				throw new ParseException(ExNoDynamicVars, codeLine);
			}

			var id = false;
			var sub = new StringBuilder();
			var parts = new List<CodeExpression>();
			var single = code.Length > 2
						 && code[0] == Resolve
						 && code[code.Length - 1] == Resolve
						 && code.Count(ch => ch == Resolve) == 2;

			for (var i = 0; i < code.Length; i++)
			{
				var sym = code[i];

				if (sym == Resolve && (i == 0 || code[i - 1] != Escape))
				{
					if (id)
					{
						if (sub.Length == 0)
							throw new ParseException(ExEmptyVarRef, i, code);

						var str = sub.ToString();

						if (single)
							parts.Add(VarRefOrPrimitive(codeLine, VarIdOrConstant(codeLine, str, false, true)));//Do double dispatching for single %variable%.
						else if (Reflections.flatPublicStaticProperties.TryGetValue(str, out var prop))//Else it's a concatenated%variable%, so do single dispatching, but check if the concat includes an accessor.
							parts.Add(new CodeVariableReferenceExpression(prop.Name));
						else//Not an accessor, so just concat as is.
							parts.Add(VarIdOrConstant(codeLine, str, false, false));

						sub.Length = 0;
						id = false;
					}
					else
					{
						parts.Add(new CodePrimitiveExpression(sub.ToString()));
						sub.Length = 0;
						id = true;
					}
				}
				else if (id && !IsIdentifier(sym))
					throw new ParseException(ExInvalidVarToken, i, code);
				else
					_ = sub.Append(sym);
			}

			if (sub.Length != 0)
				parts.Add(new CodePrimitiveExpression(sub.ToString()));

			if (parts.Count == 1)
				return new CodePrimitiveExpression(code);

			var all = parts.ToArray();
			var concat = StringConcat(all);

			if (all[0] is CodePrimitiveExpression cpe && cpe.Value.ToString()?.Length == 0 && code.Contains(" and ", StringComparison.OrdinalIgnoreCase))//For some reason, this is the case with legacy "between" statements where the args are enclosed in %%.
				return concat;

			if (concat is CodeArrayIndexerExpression)//If there was only one variable lookup, it will already be in the form of Vars[].
			{
				return concat;
			}
			else//If there was more than one variable lookup, it needs to be wrapped in one final Vars[] lookup.
			{
				var vars = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Scripting.Script"), varsPropertyName);
				return new CodeArrayIndexerExpression(vars, concat);
			}
		}

		private CodeExpression VarIdOrConstant(CodeLine codeLine, string name, bool create, bool dyn)
		{
			switch (name.ToLowerInvariant())
			{
				case "a_linenumber":
					return new CodeCastExpression(new CodeTypeReference(typeof(long)), new CodePrimitiveExpression(codeLine.LineNumber));

				case "a_linefile":
					return new CodePrimitiveExpression(Path.GetFullPath(fileName));

				case "a_scriptdir":
					return new CodePrimitiveExpression(Path.GetDirectoryName(Path.GetFullPath(fileName)));

				case "a_scriptfullpath":
					return new CodePrimitiveExpression(fileName);

				case "a_scriptname":
					return new CodePrimitiveExpression(Path.GetFileName(Path.GetFullPath(fileName)));

				case "a_thisfunc":
					return new CodePrimitiveExpression(Scope);

				case "a_thislabel":
				{
					if (blocks.Count == 0)
						return new CodePrimitiveExpression(string.Empty);

					var all = blocks.ToArray();

					for (var i = all.Length - 1; i > -1; i--)
						if (all[i].Kind == CodeBlock.BlockKind.Label)
							return new CodePrimitiveExpression(all[i].Name);
				}

				return new CodePrimitiveExpression(string.Empty);

				default:
					return VarId(codeLine, name, create, dyn);
			}
		}

		private CodeExpression VarMixedExpr(CodeLine codeLine, object part)
		{
			if (IsVarReference(part))
				return VarRefOrPrimitive(codeLine, part);
			else if (part is CodePrimitiveExpression cpe)
				return cpe;
			else if (IsVarAssignment(part))
				return (part as CodeExpression).WasCboeAssign();
			else if (part is CodeExpression ce)
				return ce;
			else
				return new CodePrimitiveExpression(part);
		}

		private string VarNormalizedName(string name) => name.ToLowerInvariant();

		private CodeExpression VarRef(string name, bool dyn)
		{
			if (!dyn)
			{
				return new CodeVariableReferenceExpression(name/*.Replace(ScopeVar[0], '_')*/);
			}
			else
			{
				var vars = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Scripting.Script"), varsPropertyName);

				if (Reflections.flatPublicStaticProperties.TryGetValue(name, out var prop))
					return new CodeArrayIndexerExpression(vars, new CodeVariableReferenceExpression(prop.Name));
				else
					return new CodeArrayIndexerExpression(vars, new CodeVariableReferenceExpression(name));
			}
		}

		private CodeExpression VarRefOrPrimitive(CodeLine codeLine, object var)
		{
			if (var is CodePrimitiveExpression cpe)
				return cpe;

			if (var is CodeSnippetExpression cse)
				return cse;

			if (var is CodeVariableReferenceExpression cvre)
				return cvre;

			if (!IsVarReference(var))
				throw new ParseException($"{var} is not a variable reference.", codeLine);

			return (CodeArrayIndexerExpression)var;
		}
	}
}