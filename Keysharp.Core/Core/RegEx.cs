namespace Keysharp.Core
{
	internal class RegExData
	{
		internal readonly Lock locker = new ();
		internal RegEx.RegexEntry regdkt = [];
		internal ConcurrentLfu<string, Func<PcreMatch, string>> ReplacementCache = new (Caching.DefaultCacheCapacity);
		internal Func<string, Func<PcreMatch, string>> parseReplace = null;

		internal Func<string, Func<PcreMatch, string>> ParseReplace
		{
			get
			{
				if (parseReplace == null)
				{
					var asm = typeof(PcreRegex).Assembly;
					// 2) find the internal class by its full name
					var rpType = asm.GetType("PCRE.Internal.ReplacementPattern", throwOnError: true);
					var mi = rpType.GetMethod("Parse", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
					parseReplace = (Func<string, Func<PcreMatch, string>>)Delegate.CreateDelegate(
									   typeof(Func<string, Func<PcreMatch, string>>),
									   mi);
				}

				return parseReplace;
			}
		}
	}

	/// <summary>
	/// Public interface for regex-related functions.
	/// </summary>
	public static class RegEx
	{
		/// <summary>
		/// <see cref="RegExMatch(object, object, ref object, object)"/>
		/// </summary>
		public static long RegExMatch(object haystack, object needle)
		{
			object outvar = new VarRef(null);
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
		/// <returns>The <see cref="RegExMatchInfo"/> object which contains the matches, if any.</returns>
		/// <exception cref="Error">An <see cref="Error"/> exception is thrown on failure.</exception>
		public static long RegExMatch(object haystack, object needle, object outputVar, object startingPos)
		{
			Error err;
			var input = haystack.As();
			var n = needle.As();
			var index = startingPos.Ai(1);
			IFuncObj callout = null;
			RegexHolder exp;

			try
			{
				exp = new RegexHolder(input, n);//This will not throw PCRE style errors like the documentation says.
			}
			catch (Exception ex)
			{
				return Errors.ErrorOccurred(err = new Error("Regular expression compile error", "", ex.Message)) ? throw err : 0L;
			}

			if (index < 0)
			{
				index = input.Length + index;

				if (index < 0)
					index = 0;
			}
			else
				index = Math.Min(Math.Max(0, index - 1), input.Length);

			PcreCalloutResult MatchCalloutHandler(PcreCallout pcre_callout)
			{
				if (callout == null)
				{
					string calloutString = pcre_callout.Number == 0 ? pcre_callout.String : null;
					string name = calloutString != null && calloutString != "" ? calloutString : "pcre_callout";
					callout = Functions.GetFuncObj(name, null);
				}

				int result = callout.Call(
								 new RegExMatchInfo(pcre_callout.Match, exp),
								 (long)pcre_callout.Number,
								 pcre_callout.PatternPosition,
								 haystack,
								 needle).ParseInt() ?? 0;

				if (result > 1)
					result = 1;
				else if (result < -1)
				{
					Error err;
					return Errors.ErrorOccurred(err = new Error($"PCRE matching error", null, (long)result)) ? throw err : PcreCalloutResult.Abort;
				}

				return (PcreCalloutResult)result;
			}

			try
			{
				var res = new RegExMatchInfo(exp.regex.Match(input, index, MatchCalloutHandler), exp);
				var pos = res.Pos();
				Script.SetPropertyValue(outputVar, "__Value", pos > 0 ? res : "");
				return pos;
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
			object outputVarCount = new VarRef(null);
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
		/// <param name="needleRegEx">The pattern to search for, which is a PCRE2 regular expression.<br/>
		/// The pattern's options (if any) must be included at the beginning of the string followed by a close-parenthesis.<br/>
		/// For example, the pattern i)abc.*123 would turn on the case-insensitive option and search for "abc",<br/>
		/// followed by zero or more occurrences of any character, followed by "123".<br/>
		/// If there are no options, the ")" is optional; for example, )abc is equivalent to abc.<br/>
		/// Although needleRegEx cannot contain binary zero, the pattern \x00 can be used to match a binary zero within haystack.
		/// </param>
		/// <param name="replacement">
		/// If blank or omitted, NeedleRegEx will be replaced with blank (empty), meaning it will be omitted from the return value.<br/>
		/// Otherwise, specify the string to be substituted for each match, which is plain text (not a regular expression).<br/>
		/// This can also be a function object, which gets called with one argument (RegExMatchInfo) and must return the replacement string.
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
			var rd = TheScript.RegExData;
			IFuncObj callout = null;
			string replace = null;
			Func<PcreMatch, string> replaceParser = null;

			if (replacement is IFuncObj ifo)
				callout = ifo;
			else
			{
				replace = replacement.As();
				replaceParser = rd.ReplacementCache.GetOrAdd(replace, rd.ParseReplace);
			}

			var l = limit.Ai(-1);
			var index = startingPos.Ai(1);
			int n = 0;
			RegexHolder exp;

			try
			{
				exp = new RegexHolder(input, needle);//This will not throw PCRE style errors like the documentation says.
			}
			catch (Exception ex)
			{
				return Errors.ErrorOccurred(err = new Error("Regular expression compile error", "", ex.Message)) ? throw err : input;
			}

			if (l < 1)
				l = int.MaxValue;

			if (index < 0)
			{
				index = input.Length + index;

				if (index < 0)
					index = 0;
			}
			else
				index = Math.Min(Math.Max(0, index - 1), input.Length);

			string CalloutHandler(PcreMatch match)
			{
				n++;

				if (callout != null)
					return callout.Call(new RegExMatchInfo(match, exp)).As();

				return replaceParser(match);
			}

			try
			{
				string result = exp.regex.Replace(input, CalloutHandler, l, index);
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
		internal class RegexEntry : KeyedCollection<string, RegexHolder>
		{
			/// <summary>
			/// Return the tag property of the <see cref="RegexHolder">.
			/// </summary>
			/// <param name="item">The <see cref="RegexHolder"/> whose tag field will be returned.</param>
			/// <returns>The tag field of the item.</returns>
			protected override string GetKeyForItem(RegexHolder item) => item.tag;
		}
	}
}