using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		//CodeArrayIndexerExpression InternalVariable => VarRef(string.Concat(Scope, ScopeVar + "\0", InternalID));
		private CodeExpression InternalVariable => VarRef(string.Concat(Scope, ScopeVar + "\0", InternalID), false);

		//internal CodeArrayIndexerExpression VarId(string name) => VarId(VarIdExpand(VarNormalizedName(name)));
		internal CodeExpression VarId(string name, bool create, bool dyn = false) => name == args ? new CodeSnippetExpression(args) : VarId(VarIdExpand(VarNormalizedName(name)), create, dyn);

		private bool IsVarAssignment(object expr) => expr is CodeBinaryOperatorExpression cboe&& cboe.Operator == CodeBinaryOperatorType.Assign;

		private bool IsVarReference(object expr) => expr is CodeArrayIndexerExpression || expr is CodeVariableReferenceExpression;

		private CodeBinaryOperatorExpression VarAssign(CodeArrayIndexerExpression name, CodeExpression value) => new CodeBinaryOperatorExpression(name, CodeBinaryOperatorType.Assign, value);

		//private CodeArrayIndexerExpression VarId(CodeExpression name)
		private CodeExpression VarId(CodeExpression name, bool create, bool dyn = false)
		{
			//var scope = (allGlobalVars ? "" : Scope) + ScopeVar;
			if (name is CodePrimitiveExpression raw)
			{
				if (raw.Value is string s)
				{
					//var dyn = s.StartsWith(Resolve) && s.EndsWith(Resolve);
					var tempscope = Scope;//Cache because the property does a lot of work.
					//if (tempscope == "")
					//  _ = globalVars.Add(s);
					var toplevelvar = /*ScopeVar + */s;
					var scopedvar = tempscope + ScopeVar + toplevelvar;
					_ = allGlobalVars.TryPeek(out var allglobal);
					_ = allStaticVars.TryPeek(out var allstat);
					_ = localFuncVars.TryPeek(out var l);
					_ = staticFuncVars.TryPeek(out var stat);
					_ = currentFuncParams.TryPeek(out var f);
					allglobal |= globalFuncVars.TryPeek(out var gg) && gg.Contains(s);
					var explicitLocal = l != null && l.Contains(/*s*/scopedvar);
					var explicitstatic = stat != null && stat.TryGetValue(/*s*/scopedvar, out _);//Static is special in that it needs to use the scoped var.
					var isstatic = allstat || explicitstatic;
					var islocal = tempscope != "" && !isstatic && (!allglobal || explicitLocal);
					scopedvar = (isstatic || islocal ? tempscope + ScopeVar : "") + toplevelvar;
					var isparam = f != null && f.Contains(scopedvar);//Function parameters are always scoped.
					create &= !excCatchVars.TryPeek(out var exc) || (!exc.Contains(s) && !exc.Contains(scopedvar));//Need either no exceptions, or exception var didn't match, to create. Use s because scope was already prepended when the catch was parsed.

					//The logic in this function has grown unweildy and needs to be reworked and condensed.
					//Further, it doesn't even work right: read only global vars are still considered function locals, when they should be global.
					if (explicitLocal)
						isstatic = false;//Local always overrides static.

					if (explicitstatic) { }
					else if (!islocal)
						create &= !allglobal;

					if (!dyn)
					{
						if (create)
						{
							if (/*create && */!isparam && isstatic && stat != null)
							{
								_ = stat.GetOrAdd(/*s*/scopedvar);
							}
							else if (/*create && */!isparam && !isstatic/* && islocal*/)
							{
								_ = allVars.GetOrAdd(tempscope).Add(scopedvar);
							}
							else if (!isparam && allglobal)
								_ = allVars.GetOrAdd("").Add(toplevelvar);
						}

						//if (g || (globalFuncVars.TryPeek(out var p) && p.Contains(s)))
						//{
						//  //_ = allVars.GetOrAdd(tempscope).Add(/*ScopeVar + */s);
						//  //return VarRef(/*ScopeVar + */s);
						//}
						return isparam ? VarRef(scopedvar, false) : VarRef((allglobal && !explicitLocal && !isstatic) ? toplevelvar : scopedvar, false);
					}
					else
						//return VarRef(StringConcat(new CodePrimitiveExpression(scope), name));
						return VarRef(toplevelvar, true);//Dyn vars only work with global static vars, not local function vars.
				}
			}

			return name;//It was something else other than CodePrimitiveExpression, so it was probably a CodeArrayIndexerExpression.
		}

		private CodeExpression VarIdExpand(string code)
		{
			code = EscapedString(code, true);

			if (IsPrimitiveObject(code, out var result))
				return result is long l ? new CodeSnippetExpression($"{l}L") : new CodePrimitiveExpression(result);

			if (code.IndexOf(Resolve) == -1)
				return new CodePrimitiveExpression(code);

			if (!DynamicVars)
			{
				throw new ParseException(ExNoDynamicVars);
			}

			var id = false;
			var sub = new StringBuilder();
			var parts = new List<CodeExpression>();

			for (var i = 0; i < code.Length; i++)
			{
				var sym = code[i];

				if (sym == Resolve && (i == 0 || code[i - 1] != Escape))
				{
					if (id)
					{
						if (sub.Length == 0)
							throw new ParseException(ExEmptyVarRef, i);

						//sub.Append(Resolve);//MATT
						parts.Add(VarRefOrPrimitive(VarIdOrConstant(sub.ToString(), false, true)));
						sub.Length = 0;
						id = false;
					}
					else
					{
						parts.Add(new CodePrimitiveExpression(sub.ToString()));
						sub.Length = 0;
						//sub.Append(Resolve);//MATT
						id = true;
					}
				}
				else if (id && !IsIdentifier(sym))
					throw new ParseException(ExInvalidVarToken, i);
				else
					_ = sub.Append(sym);
			}

			if (sub.Length != 0)
				parts.Add(new CodePrimitiveExpression(sub.ToString()));

			if (parts.Count == 1)
				return new CodePrimitiveExpression(code);

			var all = parts.ToArray();
			var concat = StringConcat(all);

			if (all[0] is CodePrimitiveExpression cpe && cpe.Value.ToString()?.Length == 0)//For some reason, this is the case with legacy "between" statements where the args are enclosed in %%.
				return concat;

			if (concat is CodeArrayIndexerExpression)//If there was only one variable lookup, it will already be in the form of Vars[].
			{
				return concat;
			}
			else//If there was mroe than one variable lookup, it needs to be wrapped in one final Vars[] lookup.
			{
				var vars = new CodePropertyReferenceExpression(null, VarProperty);
				return new CodeArrayIndexerExpression(vars, concat);
			}
		}

		private CodeExpression VarIdOrConstant(string name, bool create, bool dyn)
		{
			switch (name.ToLowerInvariant())
			{
				case "a_linenumber":
					return new CodeCastExpression(new CodeTypeReference(typeof(long)), new CodePrimitiveExpression(line));

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
					return VarId(name, create, dyn);
			}
		}

		private CodeExpression VarMixedExpr(object part) =>
		IsVarReference(part) ?
		VarRefOrPrimitive(part) :
		IsVarAssignment(part) ?
		(CodeBinaryOperatorExpression)part
		: part is CodeExpression ce
		? ce
		: new CodePrimitiveExpression(part);

		private string VarNormalizedName(string name) => name.ToLowerInvariant();

		//private CodeArrayIndexerExpression VarRef(params CodeExpression[] name)//Is this really what we want? Would be way more efficient to make global vars just be members of Program.//MATT
		//private CodeExpression VarRef(CodeExpression name)//Is this really what we want? Would be way more efficient to make global vars just be members of Program.//MATT
		private CodeExpression VarRef(string name, bool dyn)//Is this really what we want? Would be way more efficient to make global vars just be members of Program.//MATT
		{
			//var vars = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(Script)), VarProperty);
			//if (name is CodePrimitiveExpression cpe && cpe.Value is string s)
			if (!dyn)
			{
				//return new CodeSnippetExpression(s.Replace(ScopeVar[0], '_'));
				return new CodeVariableReferenceExpression(name/*.Replace(ScopeVar[0], '_')*/);
			}
			else
			{
				//old way where everything was a map.//MATT
				var vars = new CodePropertyReferenceExpression(null, VarProperty);//Cut down on the extreme namespace and type qualification verbosity.
				return new CodeArrayIndexerExpression(vars, new CodePrimitiveExpression(name));
			}
		}

		//private CodeArrayIndexerExpression VarRef(string name) => VarRef(new CodePrimitiveExpression(name));
		//private CodeExpression VarRef(string name, bool dyn) => VarRef(new name), dyn);

		private CodeExpression VarRefOrPrimitive(object var)
		{
			if (var is CodePrimitiveExpression cpe)
				return cpe;

			if (var is CodeSnippetExpression cse)//MATT
				return cse;

			if (var is CodeVariableReferenceExpression cvre)
				return cvre;

			if (!IsVarReference(var))
				throw new ArgumentException();

			return (CodeArrayIndexerExpression)var;
		}
	}
}