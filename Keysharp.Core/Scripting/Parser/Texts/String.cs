using System;
using System.IO;
using System.Linq;
using System.Text;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private string EscapedString(string code, bool resolve)
		{
			if (code.Length == 0)
				return string.Empty;

			var buffer = new StringBuilder(code.Length + 32);
			var escaped = false;

			foreach (var sym in code)
			{
				if (escaped)
				{
					switch (sym)
					{
						case 'n': _ = buffer.Append('\n'); break;

						case 'r': _ = buffer.Append('\r'); break;

						case 'b': _ = buffer.Append('\b'); break;

						case 't': _ = buffer.Append('\t'); break;

						case 'v': _ = buffer.Append('\v'); break;

						case 'a': _ = buffer.Append('\a'); break;

						case 'f': _ = buffer.Append('\f'); break;

						case 's': _ = buffer.Append(' '); break;

						case '0': _ = buffer.Append('\0'); break;

						case '"': _ = buffer.Append('"'); break;

						default:
							if (sym == Resolve)
								_ = buffer.Append(Escape);

							_ = buffer.Append(sym);
							break;
					}

					escaped = false;
				}
				else if (sym == Escape)
					escaped = true;
				else
					_ = buffer.Append(sym);
			}

			return buffer.ToString();
		}

		private string MultilineString(string code)
		{
			var reader = new StringReader(code);
			var line = reader.ReadLine().Trim(Spaces);

			if (line.Length < 1 || line[0] != ParenOpen)
				throw new ArgumentException();

			var join = Environment.NewLine;
			bool ltrim = false, rtrim = false, stripComments = false, percentResolve = true, literalEscape = false;

			if (line.Length > 2)
			{
				if (line.Contains("%"))
				{
					percentResolve = false;
					line = line.Replace("%", string.Empty);
				}

				//if (line.Contains(","))//Unsure what this was for.
				//  line = line.Replace(",", string.Empty);
				foreach (var option in line.Substring(1).Trim().Split(Spaces, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower()))
				{
					if (option.StartsWith(Keyword_Join))
					{
						join = EscapedString(option.Substring(4), false);
					}
					else
					{
						switch (option)
						{
							case "ltrim":
								ltrim = true;
								break;

							case "ltrim0":
								break;

							case "rtrim":
								rtrim = true;
								break;

							case "rtrim0":
								break;

							case "comments":
							case "comment":
							case "com":
							case "c":
								stripComments = true;
								break;

							case "`":
								literalEscape = true;
								break;

							default:
								const string joinOpt = "join";

								if (option.Length > joinOpt.Length && option.Substring(0, joinOpt.Length).Equals(joinOpt, System.StringComparison.OrdinalIgnoreCase))
									join = option.Substring(joinOpt.Length).Replace("`s", " ");
								else
									throw new ParseException(ExMultiStr);

								break;
						}
					}
				}
			}

			var str = new StringBuilder(code.Length);
			var resolve = Resolve.ToString();
			var escape = Escape.ToString();
			var cast = Multicast.ToString();
			var resolveEscaped = string.Concat(escape, resolve);
			var escapeEscaped = new string(Escape, 2);
			var castEscaped = string.Concat(escape, cast);

			while ((line = reader.ReadLine()) != null)
			{
				var check = line.Trim();

				if (check.Length > 0 && check[0] == ParenClose)
					break;

				if (ltrim && rtrim)
					line = line.Trim(Spaces);
				else if (ltrim)
					line = line.TrimStart(Spaces);
				else if (rtrim)
					line = line.TrimEnd(Spaces);

				if (stripComments)
					line = StripComment(line);

				if (!percentResolve)
					line = line.Replace(resolve, resolveEscaped);

				if (literalEscape)
					line = line.Replace(escape, escapeEscaped);

				line = line.Replace("\"", "\"\"");
				line = line.Replace(cast, castEscaped);
				_ = str.Append(line);
				_ = str.Append(join);
			}

			if (str.Length == 0)
				return string.Empty;

			_ = str.Remove(str.Length - join.Length, join.Length);
			return str.ToString();
		}

		//private string Replace(string input, string search, string replace)
		//{
		//  var buf = new StringBuilder(input.Length);
		//  int z = 0, n = 0, l = search.Length;
		//
		//  while (z < input.Length && (z = input.IndexOf(search, z, System.StringComparison.OrdinalIgnoreCase)) != -1)
		//  {
		//      if (n < z)
		//          _ = buf.Append(input, n, z - n);
		//
		//      _ = buf.Append(replace);
		//      z += l;
		//      n = z;
		//  }
		//
		//  if (n < input.Length)
		//      _ = buf.Append(input, n, input.Length - n);
		//
		//  return buf.ToString();
		//}
	}
}