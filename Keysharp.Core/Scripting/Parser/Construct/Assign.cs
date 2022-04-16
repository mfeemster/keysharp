using System.CodeDom;
using System.Text;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		//private CodeExpressionStatement ParseAssign(string code)//MATT
		private CodeAssignStatement ParseAssign(string code)
		{
			string name, value;
			var buf = new StringBuilder(code.Length);
			var i = 0;
			char sym;
			var bound = false;

			for (i = 0; i < code.Length; i++)//Probably a way easier way to do this with split.//MATT
			{
				sym = code[i];

				if (IsIdentifier(sym) || sym == Resolve)
					_ = buf.Append(sym);
				else if (sym == Equal)
				{
					i++;
					bound = true;
					break;
				}
				else if (IsSpace(sym))
					break;
				else
					throw new ParseException(ExUnexpected);
			}

			if (!bound)
			{
				while (i < code.Length)
				{
					sym = code[i];
					i++;

					if (sym == Equal)
					{
						bound = true;
						break;
					}
				}

				if (!bound)
					throw new ParseException(ExUnexpected);
			}

			name = buf.ToString();
			buf.Length = 0;
			value = code.Substring(i);
			value = value.Length == 0 ? null : StripCommentSingle(value.Trim(Spaces));
			CodeExpression left;
			var nameLow = name.ToLowerInvariant();
			left = libProperties.TryGetValue(nameLow, out var pi)
				   ? new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(bcl), pi.Name)//bcl won't work here, need type from property.//MATT
				   : VarId(name, true);
			var result = value == null ? new CodePrimitiveExpression(null) : IsExpressionParameter(value) ? ParseSingleExpression(value.TrimStart(Spaces).Substring(2), false) : VarIdExpand(value);
			return new CodeAssignStatement(left, result);
			//Used to be a binary operator, wrapped in a code expression, but this added erroneous parens. Better to directly use a CodeAssignStatement.//MATT
			//var assign = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.Assign, result);
			//return new CodeExpressionStatement(assign);
		}
	}
}