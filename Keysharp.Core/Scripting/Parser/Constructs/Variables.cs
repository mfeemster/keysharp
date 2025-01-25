namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private CodeExpression InternalVariable => VarRef(string.Concat(Scope, scopeChar + "\0", InternalID), false);

		//internal CodeExpression VarId(string name, bool create, bool dyn = false) => name == args ? new CodeSnippetExpression(args) : VarId(VarIdExpand(VarNormalizedName(name)), create, dyn);
		internal CodeExpression VarId(CodeLine codeLine, string name, bool create, bool dyn = false) => VarId(VarIdExpand(codeLine, VarNormalizedName(name)), create, dyn);

		private bool IsVarAssignment(object expr) => (expr as CodeExpression).WasCboeAssign() is CodeBinaryOperatorExpression;

		private bool IsVarReference(object expr) => expr is CodeArrayIndexerExpression || expr is CodeVariableReferenceExpression;

		private CodeBinaryOperatorExpression VarAssign(CodeArrayIndexerExpression name, CodeExpression value) => new (name, CodeBinaryOperatorType.Assign, value);

		private CodeExpression VarId(CodeExpression name, bool create, bool dyn = false)
		{
			if (name is CodePrimitiveExpression raw)
			{
				if (raw.Value is string toplevelvar)
				{
					var tempscope = Scope;//Cache because the property does a lot of work.
					var staticscopedvar = tempscope + scopeChar + toplevelvar;
					var currType = typeStack.Peek();
					_ = allGlobalVars.TryPeek(out var allglobal);
					_ = allStaticVars.TryPeek(out var allstat);
					_ = localFuncVars.TryPeek(out var l);
					_ = staticFuncVars[currType].TryPeek(out var stat);
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
								var dkt = allVars[currType].GetOrAdd(tempscope);

								if (!dkt.ContainsKey(toplevelvar))//Ensure it hasn't been declared and initialized yet.
									dkt[toplevelvar] = nullPrimitive;
							}
							else if (!isparam && allglobal)
							{
								var dkt = allVars[currType].GetOrAdd("");

								if (!dkt.ContainsKey(toplevelvar))
									dkt[toplevelvar] = nullPrimitive;
							}
						}

						var expr = isparam ? VarRef(toplevelvar, false) : VarRef(!isstatic ? toplevelvar : staticscopedvar, false);
						expr.UserData["origtypescope"] = currType;//Needed later when properly casing direct function references.

						//Must take special action to case correct all global variable references inside of classes.
						//Because if they are referring to a property in a base class, which is a built-in class, then
						//the case will not match.
						//User defined base types don't need this because all of their properties are lowercase.
						if (expr is CodeVariableReferenceExpression cvre)
						{
							if (InClassDefinition() && allglobal)// && !create
							{
								if (currType.BaseTypes.Count > 0)//Every class should derive from something.
								{
									var bpi = PropExistsInBuiltInClass(currType.BaseTypes[0].BaseType, cvre.VariableName, 0);//Determine if the variable name matched a property defined in a base class that was a built-in type.

									if (bpi.Item1)
										cvre.VariableName = bpi.Item2.Name;
								}
							}

							//This is an attempt to allow direct assignments to methods by just using their name.
							//They are case corrected and passed to FuncObj().
							if (!create && !isparam && !islocal && !explicitLocal)
							{
								if (MethodExistsInTypeOrBase(currType.Name, cvre.VariableName) is CodeMemberMethod cmm)
								{
									cvre.VariableName = cmm.Name;
									var tempfunc = (CodeMethodReferenceExpression)InternalMethods.Func;
									expr = new CodeMethodInvokeExpression(tempfunc, cvre);
								}
								else if (Reflections.FindBuiltInMethod(cvre.VariableName, -1) is MethodPropertyHolder mph)
								{
									cvre.VariableName = mph.mi.DeclaringType.FullName + "." + mph.mi.Name;
									var tempfunc = (CodeMethodReferenceExpression)InternalMethods.Func;
									expr = new CodeMethodInvokeExpression(tempfunc, cvre);
								}
							}
						}

						return expr;
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

			if (!code.Contains(Resolve))
				return new CodePrimitiveExpression(code);

			if (!DynamicVars)
			{
				throw new ParseException(ExNoDynamicVars, codeLine);
			}

			var id = false;
			var sub = new StringBuilder();
			//var parts = new List<CodeExpression>();
			var exprs = new List<CodeExpressionStatement>();
			var parts = new List<string>();
			var legacyBetween = codeLine.Code.AsSpan().IndexOf("between") >= 0;
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
						var ss = sub.ToString();
						parts.Add(ss);
						sub.Length = 0;
						id = false;
					}
					else
					{
						var ss = sub.ToString();

						if (ss.Length > 0)
						{
							var sstrim = ss.AsSpan().Trim();

							if (IsIdentifier(ss) || legacyBetween && exprVerbalOperatorsAlt.Contains(sstrim))
								parts.Add("\"" + ss + "\"");
							else
								parts.Add(ss);
						}

						sub.Length = 0;
						id = true;
					}
				}
				else
					_ = sub.Append(sym);
			}

			if (sub.Length != 0)
			{
				if (!id)
					parts.Add("\"" + sub.ToString() + "\"");
				else
					parts.Add(sub.ToString());
			}

			foreach (var part in parts)
				exprs.AddRange(ParseMultiExpression(codeLine, part, false));

			var concat = StringConcat(exprs.Select(ex => ex.Expression).ToArray());

			if (legacyBetween && exprs.Count > 2 && exprs[1].Expression is CodePrimitiveExpression cpe && cpe.Value.ToString().Contains("and"))//Extreme hack to attempt to support legacy "between" operator such as: if y "between" x and z
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
						return emptyStringPrimitive;

					var all = blocks.ToArray();

					for (var i = all.Length - 1; i > -1; i--)
						if (all[i].Kind == CodeBlock.BlockKind.Label)
							return new CodePrimitiveExpression(all[i].Name);
				}

				return emptyStringPrimitive;

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