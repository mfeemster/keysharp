using Keysharp.Core.Windows;
using Keysharp.Scripting;
using PCRE;
using System;
using System.Text.RegularExpressions;


namespace Keysharp.Core.Common.Strings
{
	public class RegExMatchInfo : KeysharpObject, I__Enum, IEnumerable<(object, object)>
	{
		internal PcreMatch match;
		internal RegexHolder holder;
		public object Count => match.CaptureCount;
		public object Mark => match.Mark;
		public object Success => match.Success;
		public object pos => Pos(); //Lower-cased because of the naming conflict with the method

		public new (Type, object) super => (typeof(KeysharpObject), this);

		public RegExMatchInfo(params object[] args) => _ = __New(args);

		public static implicit operator long(RegExMatchInfo r) => r.Pos();

		public new object __New(params object[] args)
		{
			match = args[0] as PcreMatch;
			holder = args[1] as RegexHolder;

			for (int i = 0; i < match.Groups.Count; i++)
			{
				_ = DefineProp(i,
					Objects.Object(
						[
							"get",
							Functions.GetFuncObj("GetWrapper", this, 2, true).Bind(i)
						]));
			}

			foreach (var name in holder.info.GroupNames)
			{
				if (match.Groups[name] != null)
				_ = DefineProp(name,
				   Objects.Object(
					   [
						   "get",
							Functions.GetFuncObj("GetWrapper", this, 2, true).Bind(name)
					   ]));
			}

			return "";
		}

		public KeysharpEnumerator __Enum(object count) => new RegExIterator(this, count.Ai());

		public IEnumerator<(object, object)> GetEnumerator() => new RegExIterator(this, 2);

		IEnumerator IEnumerable.GetEnumerator() => new RegExIterator(this, 2);


		public object GetWrapper(object obj1, object obj2) => this[obj1];

		public long Len(object obj)
		{
			var g = GetGroup(obj);
			return g != null && g.Success ? g.Length : 0;
		}

		public string Name(object obj)
		{
			var g = GetGroup(obj);
			return g != null && g.Success ? (obj is string o ? o : holder.groupNames[obj.Ai()]) : "";
		}

		public long Pos(object obj = null)
		{
			var g = GetGroup(obj);
			return g != null && g.Success ? g.Index + 1 : 0;
		}

		public override string ToString() => Pos().ToString();

		private PcreGroup GetGroup(object obj)
		{
			var o = obj;

			try
			{
				if (o == null)
					return match.Groups[0];
				else if (o is string s)
					return match.Groups[s];
				else
				{
					var index = Convert.ToInt32(o);

					if (index >= 0 && index <= match.Groups.Count)
						return match.Groups[index];
				}
			} 
			catch (ArgumentOutOfRangeException)
			{
				return null;
			}

			return null;
		}

		public string this[object obj]
		{
			get
			{
				var g = GetGroup(obj);
				return g != null && g.Success ? g.Value : "";
			}
		}
	}

	internal class RegExIteratorData : BaseIteratorData<RegExIterator>
	{
	}

	/// <summary>
	/// A two component iterator for <see cref="RegExMatchInfo"/> which returns the name and the value as a tuple.
	/// </summary>
	internal class RegExIterator : KeysharpEnumerator, IEnumerator<(object, object)>
	{
		/// <summary>
		/// Cache for iterators with either 1 or 2 parameters.
		/// This prevents reflection from having to always be done to find the Call method.
		/// </summary>
		private static FuncObj p1, p2;

		/// <summary>
		/// The internal regex results to be iterated over.
		/// </summary>
		private readonly RegExMatchInfo match;

		/// <summary>
		/// The iterator for the reg ex results.
		/// </summary>
		protected IEnumerator<PcreGroup> iter;
		protected int i = -1;

		/// <summary>
		/// The implementation for <see cref="IEnumerator.Current"/> which gets the key,value tuple at the current iterator position.
		/// </summary>
		public (object, object) Current
		{
			get
			{
				try
				{
					var g = iter.Current;

					if (Count == 1)
						return (g.Value, null);
					else
						return (match.holder.groupNames[i] == "" ? (long)i : match.holder.groupNames[i], g.Value);
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();//Should never happen when using regular loops.
				}
			}
		}

		/// <summary>
		/// The <see cref="IEnumerator.Current"/> implementation that just returns <see cref="Current"/>.
		/// </summary>
		object IEnumerator.Current => Current;

		/// <summary>
		/// Initializes a new instance of the <see cref="RegExIterator"/> class.
		/// </summary>
		/// <param name="m">The <see cref="Match"/> to iterate over.</param>
		/// <param name="c">The number of items to return for each iteration.</param>
		public RegExIterator(RegExMatchInfo m, int c)
			: base(null, c)
		{
			match = m;
			iter = match.match.Groups.GetEnumerator();
			var p = c <= 1 ? script.RegExIteratorData.p1 : script.RegExIteratorData.p2;
			var fo = (FuncObj)p.Clone();
			fo.Inst = this;
			CallFunc = fo;
		}

		/// <summary>
		/// Calls <see cref="Current"/> and places the key value in the passed in object reference.
		/// </summary>
		/// <param name="key">A reference to the key value.</param>
		/// <returns>True if the iterator position has not moved past the last element, else false.</returns>
		public override object Call(ref object value)
		{
			if (MoveNext())
			{
				value = Current.Item1;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Calls <see cref="Current"/> and places the name and value in the passed in object references.
		/// </summary>
		/// <param name="name">A reference to the name of the current match.</param>
		/// <param name="value">A reference to the value of the current match.</param>
		/// <returns>True if the iterator position has not moved past the last element, else false.</returns>
		public override object Call(ref object name, ref object value)
		{
			if (MoveNext())
			{
				(name, value) = Current;
				return true;
			}

			return false;
		}

		/// <summary>
		/// The implementation for <see cref="IComparer.Dispose"/> which internally resets the iterator.
		/// </summary>
		public void Dispose() => Reset();

		/// <summary>
		/// The implementation for <see cref="IEnumerator.MoveNext"/> which moves the iterator to the next position.
		/// </summary>
		/// <returns>True if the iterator position has not moved past the last element, else false.</returns>
		public bool MoveNext()
		{
			i++;
			return iter.MoveNext();
		}

		/// <summary>
		/// The implementation for <see cref="IEnumerator.Reset"/> which resets the iterator.
		/// </summary>
		public void Reset()
		{
			i = -1;
			iter = match.match.Groups.GetEnumerator();
		}
	}

	internal class RegexHolder
	{
		internal PcreRegex regex;
		internal PcrePatternInfo info;
		internal string haystack;
		internal string needle; //unmodified RegEx pattern
		internal string pattern; //RegEx pattern with AHK settings removed
		internal string tag;
		internal PcreRegexSettings opts;
		internal string[] groupNames;

		internal static ConcurrentDictionary<string, Func<PcreMatch, string>> ReplacementCache = new ();

		internal static Func<string, Func<PcreMatch, string>> _parseReplace = null;
		internal static Func<string, Func<PcreMatch, string>> ParseReplace
		{
			get
			{
				if (_parseReplace == null)
				{
					var asm = typeof(PcreRegex).Assembly;
					// 2) find the internal class by its full name
					var rpType = asm.GetType("PCRE.Internal.ReplacementPattern", throwOnError: true);
					var mi = rpType.GetMethod("Parse", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
					_parseReplace = (Func<string, Func<PcreMatch, string>>)Delegate.CreateDelegate(
						typeof(Func<string, Func<PcreMatch, string>>),
						mi
					);
				}
				return _parseReplace;
			}
		}

		internal RegexHolder(string hs, string n)
		{
			haystack = hs;
			needle = n;

			PcreRegexSettings settings = null;
			var parenIndex = n.IndexOf(')');

			if (parenIndex != -1)
			{
				var leftParenIndex = n.IndexOf('(');

				if (leftParenIndex == -1 || (leftParenIndex > parenIndex))//Make sure it was just a ) for settings and not a ().
				{
					var span = n.AsSpan(0, parenIndex);
					var substr = n.Substring(parenIndex + 1);
					settings = Conversions.ToRegexOptions(span);

					if (span.Contains('A'))
					{
						substr = "\\A" + substr;
					}
					pattern = substr;
				}
			}
			settings ??= new PcreRegexSettings();
			settings.Options |= PcreOptions.Compiled;

			pattern ??= n;
			opts = settings;
			regex = new PcreRegex(pattern, opts);
			info = regex.PatternInfo;
			groupNames = new string[info.CaptureCount + 1];

			foreach (var name in info.GroupNames)
			{
				foreach (var i in info.GetGroupIndexesByName(name))
					groupNames[i] = name;
			}

			for (int i = 0; i < groupNames.Length; i++)
				groupNames[i] ??= "";
		}
	}
}