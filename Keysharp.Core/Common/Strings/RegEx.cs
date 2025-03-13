namespace Keysharp.Core.Common.Strings
{
	public class RegExResults : KeysharpObject, I__Enum, IEnumerable<(object, object)>
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

			for (int i = 0; i < match.Groups.Count; i++)
			{
				var g = match.Groups[i];
				DefineProp(g.Name,
						   Keysharp.Core.Objects.Object(
							   [
								   "get",
								   Keysharp.Core.Functions.GetFuncObj("GetWrapper", this, 2, true).Bind(g.Name)
							   ]));

				if (i.ToString() != g.Name)//No need to add it twice if the name matches the index.
					DefineProp(i,
							   Keysharp.Core.Objects.Object(
								   [
									   "get",
									   Keysharp.Core.Functions.GetFuncObj("GetWrapper", this, 2, true).Bind(g.Name)
								   ]));
			}

			return "";
		}

		public KeysharpEnumerator __Enum(object count) => new RegExIterator(match, count.Ai());

		public IEnumerator<(object, object)> GetEnumerator() => new RegExIterator(match, 2);

		IEnumerator IEnumerable.GetEnumerator() => new RegExIterator(match, 2);


		public object GetWrapper(object obj1, object obj2) => this[obj1];

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

		public string this[params object[] obj]
		{
			get
			{
				var g = GetGroup(obj.Length == 0 ? null : obj[0]);
				return g != null && g.Success ? g.Value : "";
			}
		}
	}

	/// <summary>
	/// A two component iterator for <see cref="RegExResults"/> which returns the name and the value as a tuple.
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
		private readonly Match match;

		/// <summary>
		/// The iterator for the reg ex results.
		/// </summary>
		protected IEnumerator<Group> iter;

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
						return (g.Name, g.Value);
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
		public RegExIterator(Match m, int c)
			: base(null, c)
		{
			match = m;
			iter = ((IEnumerable<Group>)match.Groups).GetEnumerator();
			var p = c <= 1 ? p1 : p2;
			var fo = (FuncObj)p.Clone();
			fo.Inst = this;
			CallFunc = fo;
		}

		/// <summary>
		/// Static constructor to initialize function objects.
		/// </summary>
		static RegExIterator()
		{
			Error err;
			var mi1 = Reflections.FindAndCacheMethod(typeof(RegExIterator), "Call", 1);
			p1 = new FuncObj(mi1, null);

			if (!p1.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object was invalid.")) ? throw err : "";

			var mi2 = Reflections.FindAndCacheMethod(typeof(RegExIterator), "Call", 2);
			p2 = new FuncObj(mi2, null);

			if (!p2.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object was invalid.")) ? throw err : "";
		}

        public override object Call(params object[] args)
        {
            if (MoveNext())
            {
                if (args.Length == 1)
                {
                    Script.SetPropertyValue(args[0], "__Value", Current.Item1);
                }
                else if (args.Length >= 2)
                {
                    Script.SetPropertyValue(args[0], "__Value", Current.Item1);
                    Script.SetPropertyValue(args[1], "__Value", Current.Item2);
                }
                return true;
            }
            return false;
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
		public bool MoveNext() => iter.MoveNext();

		/// <summary>
		/// The implementation for <see cref="IEnumerator.Reset"/> which resets the iterator.
		/// </summary>
		public void Reset() => iter = ((IEnumerable<Group>)match.Groups).GetEnumerator();

		/// <summary>
		/// Gets the enumerator which is just this.
		/// </summary>
		/// <returns>this as an <see cref="IEnumerator{(object, object)}"/>.</returns>
		private IEnumerator<(object, object)> GetEnumerator() => this;
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