namespace Keysharp.Core.Common.Strings
{
	public class RegExResults : KeysharpObject, IEnumerable
	{
		private Match match;
		public object Count => match.Groups.Count - 1L;
		public object Mark => match.Groups.Count > 0 ? match.Groups[ ^ 1].Name : "";
		public object Success => match.Success;

		public RegExResults(params object[] args) : base(args) { }

        public new (Type, object) super => (typeof(RegExResults), this);

        public static implicit operator long(RegExResults r) => r.Pos();

		public override object __New(params object[] args)
		{
			match = args[0] as Match;
			return "";
		}

		public IEnumerator GetEnumerator() => match.Groups.GetEnumerator();

		public long Len(object obj)
		{
			var g = GetGroup(obj);
			return g != null && g.Success ? g.Length : 0;
		}

		public string Name(object obj)
		{
			var g = GetGroup(obj);
			return g != null && g.Success ? g.Name : "";
		}

		public long Pos(object obj = null)
		{
			var g = GetGroup(obj);
			return g != null && g.Success ? g.Index + 1 : 0;
		}

		public override string ToString() => Pos().ToString();

		private Group GetGroup(object obj)
		{
			var o = obj;

			if (o == null)
				return match;
			else if (o is string s)
				return match.Groups[s];
			else
			{
				var index = Convert.ToInt32(o);

				if (index == 0)
					return match;
				else if (index > 0 && index <= match.Groups.Count)
					return match.Groups[index];
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

	internal class RegexWithTag : Regex
	{
		internal string tag;

		internal RegexWithTag(string s)
			: base(s)
		{
		}

		internal RegexWithTag(string s, RegexOptions options)
			: base(s, options)
		{
		}
	}
}