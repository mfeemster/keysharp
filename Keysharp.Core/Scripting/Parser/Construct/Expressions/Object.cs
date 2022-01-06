using System;
using System.CodeDom;
using System.Collections.Generic;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private bool IsArrayExtension(object item) => item is CodeMethodInvokeExpression cmie&& cmie.Method.MethodName == InternalMethods.ExtendArray.MethodName;

		private bool IsJsonObject(object item) => item is CodeMethodInvokeExpression cmie&& cmie.Method.MethodName == InternalMethods.Index.MethodName;

		//private void ParseObject(List<object> parts, out CodePrimitiveExpression[] keys, out CodeExpression[] values)
		private void ParseObject(List<object> parts, out CodeExpression[] keys, out CodeExpression[] values, bool create)//MATT
		{
			//var names = new List<CodePrimitiveExpression>();
			var names = new List<CodeExpression>();
			var entries = new List<CodeExpression>();

			for (var i = 0; i < parts.Count; i++)
			{
				CodeExpression value = null;
				var hadQuotes = false;

				if (!(parts[i] is string name))//Each token shouldn't be anything else other than a string.
					throw new ParseException(ExUnexpected);

				//If enclosed in quotes, extract the enclosed string.
				if (name.Length > 2 && name[0] == StringBound && name[name.Length - 1] == StringBound)
				{
					hadQuotes = true;
					name = name.Substring(1, name.Length - 2);
				}

				var paren = name.StartsWith(ParenOpen);//If it starts with a paren, then it's a varible reference, so take special action.

				//Ensure the token is not empty and is a valid identifier.
				//if (name.Length == 0 || !IsIdentifier(name))
				//if (name.Length == 0 || !(IsIdentifier(name) || paren))
				//throw new ParseException(ExInvalidVarName);

				if (paren)//V1 style, use parens to signify an object.
				{
					var varname = parts[i + 1] as string;
					names.Add(VarId(varname, false));
					i += 2;
				}
				else if (hadQuotes)
					names.Add(new CodePrimitiveExpression(name));
				else if (IsIdentifier(name))//V2 style, no parens needed.
					names.Add(VarId(parts[i] as string, false));
				else
				{
					var l = parts[i].ParseLong(false);

					if (l.HasValue)
					{
						names.Add(new CodeSnippetExpression($"{l.Value}L"));
					}
					else
					{
						var d = parts[i].ParseDouble(false);

						if (d.HasValue)
							names.Add(new CodePrimitiveExpression(d.Value));
						else
							throw new ParseException(ExInvalidVarName);
					}
				}

				i++;//Ensure the next token is a : char.

				if (i == parts.Count)
					goto collect;

				if (!(parts[i] is string assign))
					throw new ParseException(ExUnexpected);

				if (assign.Length == 1 && assign[0] == Multicast)
					goto collect;

				if (!(assign.Length == 1 && (assign[0] == Equal || assign[0] == HotkeyBound)))//Should be an = or : char.
					throw new ParseException(ExUnexpected);

				i++;

				if (i == parts.Count)
					goto collect;

				//Now get the value portion, which comes after the : char.
				var sub = new List<object>();
				var next = Set(parts, i);

				if (next == 0) // no enclosing set (...){...}[...] so scan until next boundary.
				{
					for (next = i; next < parts.Count; next++)
					{
						if (parts[next] is string pn && pn[0] == Multicast)
							break;
					}
				}
				else
					next++; // set function returns n-1 index

				for (; i < next; i++)
					sub.Add(parts[i]);

				i--;
				value = ParseExpression(sub, create);
				i++;

				if (i == parts.Count)
					goto collect;

				if (!(parts[i] is string delim))
					throw new ParseException(ExUnexpected);

				if (!(delim.Length == 1 && delim[0] == Multicast))
					throw new ParseException(ExUnexpected);

				collect:
				entries.Add(value ?? new CodePrimitiveExpression(null));
			}

			keys = names.ToArray();
			values = entries.ToArray();
		}
	}
}