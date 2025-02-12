namespace Keysharp.Core
{
	/// <summary>
	/// Serialize JSON strings.
	/// </summary>
	public static partial class SimpleJson
	{
		private const char ArrayClose = ']';

		private const char ArrayOpen = '[';

		private const char Escape = '\\';

		private const string ExNoKeyPair = "Expected key pair";

		private const string ExNoMemberVal = "Expected member value";

		private const string ExSeperator = " at position ";

		private const string ExUnexpectedToken = "Unexpected token";

		private const string ExUntermField = "Unterminated field";

		private const string False = "false";

		private const char MemberAssign = ':';

		private const char MemberAssignAlt = '=';

		private const char MemberSeperator = ',';

		private const string Null = "null";

		private const char ObjectClose = '}';

		private const char ObjectOpen = '{';

		private const char Space = ' ';

		private const char StringBoundary = '"';

		private const char StringBoundaryAlt = '\'';

		private const string True = "true";

		/// <summary>
		/// Converts a JSON encoded string into an associative array.
		/// </summary>
		/// <param name="source">The string to decode.</param>
		/// <returns>An associative array.</returns>
		public static Dictionary<string, object> JsonDecode(string source)
		{
			try
			{
				return Decode(source);
			}
			catch (Exception ex)
			{
				Error err;
				return Errors.ErrorOccurred(err = new ValueError(ex.Message)) ? throw err : null;
			}
		}

		/// <summary>
		/// Returns a string containing the JSON representation of <paramref name="data"/>.
		/// </summary>
		/// <param name="data">The associative array to encode.</param>
		/// <returns>A JSON encoded string.</returns>
		public static string JsonEncode(Dictionary<string, object> data) => Encode(data);

		/// <summary>
		/// Convert a JSON string to a dictionary of string key and object value pairs.
		/// </summary>
		/// <param name="Source">The JSON string to evaluate.</param>
		/// <returns>A <see cref="Dictionary&lt;TKey, TValue&gt;"/>.</returns>
		internal static Dictionary<string, object> Decode(string Source)
		{
			var data = new Dictionary<string, object>();
			var pointer = 0;
			DecodeObject(ref data, Scan(Source, ref pointer, ObjectClose));
			return data;
		}

		/// <summary>
		/// Format a dictionary of string key and object value pairs as a JSON string.
		/// </summary>
		/// <param name="Elements">The table of key and values. Objects other than a string, boolean or numeric type have their <code>ToString()</code> method called for a compatible value.</param>
		/// <returns>A JSON representation.</returns>
		internal static string Encode(Dictionary<string, object> Elements) => EncodeObject(Elements);

		private static void DecodeObject(ref Dictionary<string, object> parent, string node)
		{
			Error err;
			var key = string.Empty;
			bool expectVal = false, next = true;

			for (var i = 0; i < node.Length; i++)
			{
				var token = node[i];

				if (char.IsWhiteSpace(token))
					continue;
				else if (expectVal)
				{
					object value = null;

					switch (token)
					{
						case StringBoundary:
							value = Scan(node, ref i, StringBoundary);
							break;

						case StringBoundaryAlt:
							value = Scan(node, ref i, StringBoundaryAlt);
							break;

						case ObjectOpen:
							var sub = new Dictionary<string, object>();
							DecodeObject(ref sub, Scan(node, ref i, ObjectClose));
							value = sub;
							break;

						case ArrayOpen:
							var s = Scan(node, ref i, ArrayClose);
							value = ParseArray(s);
							break;

						case MemberSeperator:
							value = null;
							next = true;
							break;

						default:
							if (IsNumber(token))
								_ = ExtractNumber(ref node, ref i, out value);
							else if (!ExtractBoolean(ref node, ref i, ref value))
							{
								_ = Errors.ErrorOccurred(err = new Error(ErrorMessage(ExNoMemberVal, i))) ? throw err : "";
								return;
							}

							break;
					}

					Value(ref parent, ref key, ref value);
					expectVal = false;
				}
				else if (next)
				{
					next = false;

					if (token == StringBoundary)
						key = Scan(node, ref i, StringBoundary);
					else if (token == StringBoundaryAlt)
						key = Scan(node, ref i, StringBoundaryAlt);
					else
					{
						var keyip = new StringBuilder();

						do
						{
							var c = node[i];

							if (char.IsLetterOrDigit(c) || c == '_')
								_ = keyip.Append(c);
							else
								break;

							i++;
						} while (i < node.Length);

						if (keyip.Length == 0)
						{
							_ = Errors.ErrorOccurred(err = new Error(ErrorMessage(ExNoKeyPair, i))) ? throw err : "";
							return;
						}
						else
						{
							key = keyip.ToString();
							i--;
						}
					}
				}
				else if (token == MemberAssign || token == MemberAssignAlt)
					expectVal = true;
				else if (token == MemberSeperator)
				{
					Value(ref parent, ref key);
					next = true;
				}
				else
				{
					_ = Errors.ErrorOccurred(err = new Error(ErrorMessage(ExUnexpectedToken, i))) ? throw err : "";
					return;
				}
			}

			Value(ref parent, ref key);
		}

		private static string EncodeObject(object node)
		{
			if (node == null)
				return Null;

			var json = new StringBuilder();

			if (node is Dictionary<string, object> pairs)
			{
				_ = json.Append(ObjectOpen);
				var n = pairs.Keys.Count;

				foreach (var key in pairs.Keys)
				{
					_ = json.Append(Space);
					_ = json.Append(StringBoundary);
					_ = json.Append(key);
					_ = json.Append(StringBoundary);
					_ = json.Append(Space);
					_ = json.Append(MemberAssign);
					_ = json.Append(Space);
					_ = json.Append(EncodeObject(pairs[key]));
					n--;
					_ = json.Append(n == 0 ? Space : MemberSeperator);
				}

				_ = json.Append(ObjectClose);
			}
			else if (node is object[] list)
			{
				_ = json.Append(ArrayOpen);
				var n = list.Length;

				foreach (var sub in list)
				{
					_ = json.Append(Space);
					_ = json.Append(EncodeObject(sub));
					n--;
					_ = json.Append(n == 0 ? Space : MemberSeperator);
				}

				_ = json.Append(ArrayClose);
			}
			else if (node is bool b)
				_ = json.Append(b ? True : False);
			else if (node is byte || node is sbyte || node is short || node is ushort || node is int || node is uint || node is long || node is ulong || node is float || node is double || node is decimal)
				_ = json.Append(node.ToString());
			else
			{
				_ = json.Append(StringBoundary);

				if (node is string s)
					_ = json.Append(s);
				else
					_ = json.Append(node.ToString());

				_ = json.Append(StringBoundary);
			}

			return json.ToString();
		}

		private static string ErrorMessage(string text, int position) => string.Concat(text, ExSeperator, position.ToString());

		private static bool ExtractBoolean(ref string node, ref int i, ref object value)
		{
			var r = node.Length - i + 1;

			if (r > Null.Length && string.Equals(node.Substring(i, Null.Length), Null, StringComparison.OrdinalIgnoreCase))
			{
				i += Null.Length;
				value = null;
				return true;
			}
			else if (r > True.Length && string.Equals(node.Substring(i, True.Length), True, StringComparison.OrdinalIgnoreCase))
			{
				i += True.Length;
				value = true;
				return true;
			}
			else if (r > False.Length && string.Equals(node.Substring(i, False.Length), False, StringComparison.OrdinalIgnoreCase))
			{
				i += False.Length;
				value = false;
				return true;
			}

			return false;
		}

		private static bool ExtractNumber(ref string node, ref int i, out object value)
		{
			var s = new StringBuilder();

			while (i < node.Length)
			{
				var c = node[i];

				if (IsNumber(c))
					_ = s.Append(c);
				else
					break;

				i++;
			}

			value = null;

			if (double.TryParse(s.ToString(), out var n))
			{
				value = (int)n == n ? (int)n : n;
				return true;
			}

			return false;
		}

		private static bool IsNumber(char c) => c == '+' || c == '-' || c == '.' || c == 'e' || c == 'E' || (c >= '0'&& c <= '9');

		private static object[] ParseArray(string node)
		{
			Error err;
			var list = new List<object>();
			object value = null;

			for (var i = 0; i < node.Length; i++)
			{
				var token = node[i];

				if (char.IsWhiteSpace(node, i))
					continue;

				switch (token)
				{
					case StringBoundary:
						value = Scan(node, ref i, StringBoundary);
						break;

					case StringBoundaryAlt:
						value = Scan(node, ref i, StringBoundaryAlt);
						break;

					case ObjectOpen:
						var sub = new Dictionary<string, object>();
						DecodeObject(ref sub, Scan(node, ref i, ObjectClose));
						value = sub;
						break;

					case ArrayOpen:
						value = ParseArray(Scan(node, ref i, ArrayClose));
						break;

					case MemberSeperator:
						list.Add(value);
						value = null;
						break;

					default:
						if (IsNumber(token))
							_ = ExtractNumber(ref node, ref i, out value);
						else if (!ExtractBoolean(ref node, ref i, ref value))
							return Errors.ErrorOccurred(err = new Error(ErrorMessage(ExNoMemberVal, i))) ? throw err : null;

						break;
				}
			}

			if (node.Length != 0)
				list.Add(value);

			return list.ToArray();
		}

		private static string Scan(string node, ref int i, char anchor)
		{
			Error err;
			int start = i + 1, skip = 1;
			var inStr = false;

			while (++i < node.Length)
			{
				var token = node[i];

				if ((token == StringBoundary || token == StringBoundaryAlt) && node[i - 1] != Escape)
					inStr = !inStr;

				if ((anchor == ArrayClose && token == ArrayOpen) || (anchor == ObjectClose && token == ObjectOpen))
					skip++;
				else if ((anchor == StringBoundary || anchor == StringBoundaryAlt) && token == anchor)
					break;
				else if (!inStr && token == anchor)
					if (--skip == 0)
						break;

				if (i == node.Length)
					return Errors.ErrorOccurred(err = new Error(ErrorMessage(ExUntermField, i))) ? throw err : null;
			}

			return node.Substring(start, i - start);
		}

		private static void Value(ref Dictionary<string, object> parent, ref string key)
		{
			object value = null;
			Value(ref parent, ref key, ref value);
		}

		private static void Value(ref Dictionary<string, object> parent, ref string key, ref object value)
		{
			if (key.Length == 0)
				return;

			parent[key] = value;
			key = string.Empty;
			value = null;
		}
	}
}