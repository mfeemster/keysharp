using System;
using System.IO;
using System.Text;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private string StripComment(string code)
		{
			if (string.IsNullOrEmpty(code))
				return code;

			var reader = new StringReader(code);
			var buf = new StringBuilder(code.Length);
			string line;

			while ((line = reader.ReadLine()) != null)
			{
				_ = buf.Append(StripCommentSingle(line));
				_ = buf.Append(newlineToUse);
			}

			var length = newlineToUse.Length;
			_ = buf.Remove(buf.Length - length, length);
			return buf.ToString();
		}

		private string StripCommentSingle(string code)
		{
			var spaced = false;

			for (var i = 0; i < code.Length; i++)
			{
				if (IsCommentAt(code, i))
					return code.Substring(0, i - (spaced ? 1 : 0));

				spaced = IsSpace(code[i]);
			}

			return code;
		}

		private bool IsCommentAt(string code, int offset)
		{
			var spaced = offset == 0 || IsSpace(code[offset - 1]);
#if LEGACY
			return code.Length - offset >= Comment.Length && MemoryExtensions.Equals(code.AsSpan(offset, Comment.Length), Comment, StringComparison.Ordinal) && spaced;
#endif
#if !LEGACY
			return code[offset] == Comment && spaced;
#endif
		}

		private bool IsCommentLine(string code) =>
#if LEGACY
		code.Length >= Comment.Length&& MemoryExtensions.Equals(code.AsSpan(0, Comment.Length), Comment, StringComparison.Ordinal);
#endif
#if !LEGACY
		return code.Length > 0 && code[0] == Comment;
#endif


		private bool IsEmptyStatement(string code)
		{
			for (var i = 0; i < code.Length; i++)
			{
				if (IsCommentAt(code, i))
					return true;
				else if (!IsSpace(code[i]))
					return false;
			}

			return true;
		}
	}
}