namespace Keysharp.Core.Common.ObjectBase
{
	public class OwnPropsIterator : IEnumerator<(object, object)>
	{
		private readonly bool getVal;
		private readonly Dictionary<object, object> map;
		private readonly KeysharpObject obj;
		private IEnumerator<KeyValuePair<object, object>> iter;

		public (object, object) Current
		{
			get
			{
				var kv = iter.Current;

				if (getVal)
				{
					if (kv.Value is MethodPropertyHolder mph)
						return (kv.Key, mph.callFunc(obj, null));
					else if (kv.Value is FuncObj fo)//ParamLength was verified when this was created in OwnProps().
						return (kv.Key, fo.Call(obj));
					else
						return (kv.Key, kv.Value);
				}

				return (kv.Key, null);
			}
		}

		object IEnumerator.Current => Current;

		public OwnPropsIterator(KeysharpObject o, Dictionary<object, object> m, bool gv)
		{
			obj = o;
			map = m;
			getVal = gv;
			iter = map.GetEnumerator();
		}

		public void Call(ref object obj0) => (obj0, _) = Current;

		public void Call(ref object obj0, ref object obj1) => (obj0, obj1) = Current;

		public void Dispose() => Reset();

		public bool MoveNext() => iter.MoveNext();

		public void Reset() => iter = map.GetEnumerator();

		private IEnumerator<(object, object)> GetEnumerator() => this;
	}

	public class OwnPropsMap : Map
	{
		public KeysharpObject Parent { get; private set; }

		public OwnPropsMap(KeysharpObject kso, Map map) : base(skipLogic: true) => __New(kso, map);

		public object __New(params object[] args)
		{
			if (args.Length == 0) return null;
			var kso = (KeysharpObject)args[0];
			var map = (Map)args[1];
			Parent = kso;
			Default = map.Default;
			Capacity = map.Capacity;
			CaseSense = "Off";

			foreach (var kv in map.map)
				this[kv.Key] = kv.Value;

			return "";
		}

		public override object Clone()
		{
			return new OwnPropsMap(Parent, this);
		}
	}
}