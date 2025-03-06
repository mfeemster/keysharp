namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for regex-related functions.
	/// </summary>
	public static class RegEx
	{
		internal static readonly Lock locker = new ();
		internal static RegexEntry regdkt = [];

		/// <summary>
		/// <see cref="RegExMatch(object, object, ref object, object)"/>
		/// </summary>
		public static long RegExMatch(object haystack, object needle)
		{
			object outvar = new Misc.VarRef(null);
			return RegExMatch(haystack, needle, outvar, null);
		}

		/// <summary>
		/// <see cref="RegExMatch(object, object, ref object, object)"/>
		/// </summary>
		public static long RegExMatch(object haystack, object needle, object outvar) => RegExMatch(haystack, needle, outvar, null);

		/// <summary>
		/// Determines whether a string contains a pattern (regular expression).
		/// </summary>
		/// <param name="haystack">The string whose content is searched.</param>
		/// <param name="needle">The pattern to search for, which is a C#-compatible regular expression.<br/>
		/// The pattern's options (if any) must be included at the beginning of the string followed by a close-parenthesis.<br/>
		/// For example, the pattern i)abc.*123 would turn on the case-insensitive option and search for "abc",<br/>
		/// followed by zero or more occurrences of any character, followed by "123".<br/>
		/// If there are no options, the ")" is optional; for example, )abc is equivalent to abc.
		/// </param>
		/// <param name="outputVar">If omitted, no output variable will be used.<br/>
		/// Otherwise, specify a reference to the output variable in which to store a match object,<br/>
		/// which can be used to retrieve the position, length and value of the overall match and of each captured subpattern, if any are present.
		/// </param>
		/// <param name="startingPos">
		/// If omitted, it defaults to 1 (the beginning of haystack).<br/>
		/// Otherwise, specify 2 to start at the second character, 3 to start at the third, and so on.<br/>
		/// If startingPos is beyond the length of haystack, the search starts at the empty string that lies at the end of haystack (which typically results in no match).<br/>
		/// Specify a negative startingPos to start at that position from the right.<br/>
		/// For example, -1 starts at the last character and -2 starts at the next-to-last character.<br/>
		/// If startingPos tries to go beyond the left end of haystack, all of Haystack is searched.<br/>
		/// Specify 0 to start at the end of haystack; i.e.the position to the right of the last character.<br/>
		/// This can be used with zero-width assertions such as (?<=a).<br/>
		/// Regardless of the value of startingPos, the return value is always relative to the first character of haystack.<br/>
		/// For example, the position of "abc" in "123abc789" is always 4.
		/// </param>
		/// <returns>The <see cref="RegExResults"/> object which contains the matches, if any.</returns>
		/// <exception cref="Error">An <see cref="Error"/> exception is thrown on failure.</exception>
		public static long RegExMatch(object haystack, object needle, object outputVar, object startingPos)
		{
			Error err;
			var input = haystack.As();
			var n = needle.As();
			var index = startingPos.Ai(1);
			var reverse = index < 1;
			var str = n + reverse;
			RegexWithTag exp = null;

			lock (locker)//KeyedCollection is not threadsafe, the way ConcurrentDictionary is, so we must lock. We use KC because we need to preserve order to remove the first entry.
			{
				if (!regdkt.TryGetValue(str, out exp))
				{
					try
					{
						exp = Conversions.ParseRegEx(n, reverse);//This will not throw PCRE style errors like the documentation says.
					}
					catch (Exception ex)
					{
						return Errors.ErrorOccurred(err = new Error("Regular expression compile error", "", ex.Message)) ? throw err : 0L;
					}

					exp.tag = str;
					regdkt.Add(exp);

					while (regdkt.Count > 100)
						regdkt.RemoveAt(0);
				}
			}

			if (index < 0)
			{
				index = input.Length + index + 1;

				if (index < 0)
					index = input.Length;
			}
			else
				index = Math.Min(Math.Max(0, index - 1), input.Length);

			try
			{
				var res = new RegExResults(exp.Match(input, index));
                Script.SetPropertyValue(outputVar, "__Value", res);
                return res.Pos();
			}
			catch (Exception ex)
			{
				return Errors.ErrorOccurred(err = new Error("Regular expression execution error", "", ex.Message)) ? throw err : 0L;
			}
		}

		/// <summary>
		/// <see cref="RegExReplace(object, object, object, VarRef, object, object)"/>
		/// </summary>
		public static string RegExReplace(object haystack, object needleRegEx, object replacement = null)
		{
			object outputVarCount = new Misc.VarRef(null);
			return RegExReplace(haystack, needleRegEx, replacement, outputVarCount);
		}

		/// <summary>
		/// <see cref="RegExReplace(object, object, object, ref object, object, object)"/>
		/// </summary>
		public static string RegExReplace(object haystack, object needleRegEx, object replacement, object outputVarCount) => RegExReplace(haystack, needleRegEx, replacement, outputVarCount, null, null);

		/// <summary>
		/// <see cref="RegExReplace(object, object, object, ref object, object, object)"/>
		/// </summary>
		public static string RegExReplace(object haystack, object needleRegEx, object replacement, object outputVarCount, object limit) => RegExReplace(haystack, needleRegEx, replacement, outputVarCount, limit, null);

		/// <summary>
		/// Replaces occurrences of a pattern (regular expression) inside a string.
		/// </summary>
		/// <param name="haystack">The string whose content is searched and replaced.</param>
		/// <param name="needleRegEx">The pattern to search for, which is a C# compatible regular expression.<br/>
		/// The pattern's options (if any) must be included at the beginning of the string followed by a close-parenthesis.<br/>
		/// For example, the pattern i)abc.*123 would turn on the case-insensitive option and search for "abc",<br/>
		/// followed by zero or more occurrences of any character, followed by "123".<br/>
		/// If there are no options, the ")" is optional; for example, )abc is equivalent to abc.<br/>
		/// Although needleRegEx cannot contain binary zero, the pattern \x00 can be used to match a binary zero within haystack.
		/// </param>
		/// <param name="replacement">
		/// If blank or omitted, NeedleRegEx will be replaced with blank (empty), meaning it will be omitted from the return value.<br/>
		/// Otherwise, specify the string to be substituted for each match, which is plain text (not a regular expression).
		/// </param>
		/// <param name="outputVarCount">If omitted, the corresponding value will not be stored.<br/>
		/// Otherwise, specify a reference to the output variable in which to store the number of replacements that occurred (0 if none).
		/// </param>
		/// <param name="limit">If omitted, it defaults to -1, which replaces all occurrences of the pattern found in Haystack.<br/>
		/// Otherwise, specify the maximum number of replacements to allow.<br/>
		/// The part of Haystack to the right of the last replacement is left unchanged.
		/// </param>
		/// <param name="startingPos">
		/// If omitted, it defaults to 1 (the beginning of haystack).<br/>
		/// Otherwise, specify 2 to start at the second character, 3 to start at the third, and so on.<br/>
		/// If startingPos is beyond the length of Haystack, the search starts at the empty string that lies at the end of haystack (which typically results in no replacements).<br/>
		/// Specify a negative startingPos to start at that position from the right.<br/>
		/// For example, -1 starts at the last character and -2 starts at the next-to-last character.<br/>
		/// If startingPos tries to go beyond the left end of haystack, all of haystack is searched.<br/>
		/// Specify 0 to start at the end of haystack; i.e.the position to the right of the last character.<br/>
		/// This can be used with zero-width assertions such as (?<=a).<br/>
		/// Regardless of the value of startingPos, the return value is always a complete copy of haystack -- the only difference is that<br/>
		/// more of its left side might be unaltered compared to what would have happened with a startingPos of 1.
		/// </param>
		/// <returns>A version of haystack whose contents have been replaced by the operation. If no replacements are needed, haystack is returned unaltered.</returns>
		/// <exception cref="Error">An <see cref="Error"/> exception is thrown on failure.</exception>
		public static string RegExReplace(object haystack, object needleRegEx, object replacement, object outputVarCount, object limit, object startingPos)
		{
			Error err;
			var input = haystack.As();
			var needle = needleRegEx.As();
			var replace = replacement.As();
			var l = limit.Ai(-1);
			var index = startingPos.Ai(1);
			var n = 0;
			var reverse = index < 1;
			var str = needle + reverse;
			RegexWithTag exp = null;

			lock (locker)//KeyedCollection is not threadsafe, the way ConcurrentDictionary is, so we must lock. We use KeyedCollection because we need to preserve order to remove the first entry.
			{
				if (!regdkt.TryGetValue(str, out exp))
				{
					try
					{
						exp = Conversions.ParseRegEx(needle, reverse);
					}
					catch (ArgumentException ex)
					{
						return Errors.ErrorOccurred(err = new Error("Regular expression compile error", "", ex.Message)) ? throw err : null;
					}

					exp.tag = str;
					regdkt.Add(exp);

					while (regdkt.Count > 100)
						regdkt.RemoveAt(0);
				}
			}

			if (l < 1)
				l = int.MaxValue;

			if (index < 0)
			{
				index = input.Length + index + 1;

				if (index < 0)
					index = input.Length;
			}
			else
				index = Math.Min(Math.Max(0, index - 1), input.Length);

			string match(Match hit)
			{
				n++;
				return hit.Result(replace);
			}

			try
			{
				var result = exp.Replace(input, match, l, index);
                Script.SetPropertyValue(outputVarCount, "__Value", (long)n);
				return result;
			}
			catch (Exception ex)
			{
				return Errors.ErrorOccurred(err = new Error("Regular expression execution error", "", ex.Message)) ? throw err : null;
			}
		}

		/// <summary>
		/// Thin derivation of a <see cref="KeyedCollection"/> to make it easy to look up
		/// regular expression items.
		/// </summary>
		internal class RegexEntry : KeyedCollection<string, RegexWithTag>
		{
			/// <summary>
			/// Return the tag property of the <see cref="RegexWithTag">.
			/// </summary>
			/// <param name="item">The <see cref="RegexWithTag"/> whose tag field will be returned.</param>
			/// <returns>The tag field of the item.</returns>
			protected override string GetKeyForItem(RegexWithTag item) => item.tag;
		}
	}
}