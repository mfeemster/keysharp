namespace Keysharp.Core.Common.ObjectBase
{
	public class OwnPropsMap : Map
	{
		public KeysharpObject Parent { get; private set; }

        public OwnPropsMap(KeysharpObject kso, Map map) : base(skipLogic: true) => __New(kso, map);

		public override object __New(params object[] args)
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

	internal class OwnPropsIterator : KeysharpEnumerator, IEnumerator<(object, object)>
	{
		private static FuncObj p1, p2;
		private readonly Dictionary<object, object> map;
		private readonly KeysharpObject obj;
		private IEnumerator<KeyValuePair<object, object>> iter;

		public (object, object) Current
		{
			get
			{
				var kv = iter.Current;

				if (GetVal)
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
		internal new int Count => GetVal ? 2 : 1;
		internal bool GetVal { get; set; }

		internal OwnPropsIterator(KeysharpObject o, Dictionary<object, object> m, bool gv)
			: base(null, gv ? 2 : 1)
		{
			obj = o;
			map = m;
			GetVal = gv;
			iter = map.GetEnumerator();
			var p = Count <= 1 ? p1 : p2;
			var fo = (FuncObj)p.Clone();
			fo.Inst = this;
			CallFunc = fo;
		}

		/// <summary>
		/// Static constructor to initialize function objects.
		/// </summary>
		static OwnPropsIterator()
		{
			Error err;
			var mi1 = Reflections.FindAndCacheMethod(typeof(OwnPropsIterator), "Call", 1);
			p1 = new FuncObj(mi1, null);

			if (!p1.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object was invalid.")) ? throw err : "";

			var mi2 = Reflections.FindAndCacheMethod(typeof(OwnPropsIterator), "Call", 2);
			p2 = new FuncObj(mi2, null);

			if (!p2.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object was invalid.")) ? throw err : "";
		}

		public override object Call(ref object obj0)
		{
			if (MoveNext())
			{
				GetVal = false;
				(obj0, _) = Current;
				return true;
			}

			return false;
		}

		public override object Call(ref object obj0, ref object obj1)
		{
			if (MoveNext())
			{
				GetVal = true;
				(obj0, obj1) = Current;
				return true;
			}

			return false;
		}

        public override object Call(object[] obj)
        {
			GetVal = obj.Length != 1;

            if (MoveNext())
            {
                if (GetVal)
                {
                    Script.SetPropertyValue(obj[0], "__Value", Current.Item1);
                    Script.SetPropertyValue(obj[1], "__Value", Current.Item2);
                }
                else
                    Script.SetPropertyValue(obj[0], "__Value", Current.Item1);
                return true;
            }
            return false;
        }

        public void Dispose() => Reset();

		public bool MoveNext() => iter.MoveNext();

		public void Reset() => iter = map.GetEnumerator();
	}

	public class OwnPropsDesc
	{
        public KeysharpObject Parent { get; private set; }
        public object Value;
        public object Get;
        public object Set;
        public object Call;

        public OwnPropsDesc()
		{
			Parent = null;
		}

        public OwnPropsDesc(KeysharpObject kso, object set_Value = null, object set_Get = null, object set_Set = null, object set_Call = null)
        {
            Parent = kso;
			Value = set_Value;
			Get = set_Get;
			Set = set_Set;
			Call = set_Call;
        }


        public OwnPropsDesc(KeysharpObject kso, Map map)
		{
			Parent = kso;
			Merge(map);
        }

		public bool IsEmpty
		{
			get => Value == null && Get == null && Set == null && Call == null;
		}

        public void Merge(Dictionary<string, OwnPropsDesc> map)
        {
            foreach (var kv in map)
            {
                switch (kv.Key.ToLower())
                {
                    case "value":
                        Value = kv.Value.Value;
						Get = null;
						Set = null;
						Call = null;
                        break;
                    case "get":
                        Get = kv.Value.Get;
						Value = null;
                        break;
                    case "set":
                        Set = kv.Value.Set;
                        Value = null;
                        break;
                    case "call":
                        Call = kv.Value.Call;
                        Value = null;
                        break;
                }
            }
        }

        public void MergeOwnPropsValues(Dictionary<string, OwnPropsDesc> map)
        {
            foreach (var kv in map)
            {
                switch (kv.Key.ToLower())
                {
                    case "value":
                        Value = kv.Value.Value;
                        Get = null;
                        Set = null;
                        Call = null;
                        break;
                    case "get":
                        Get = kv.Value.Value;
                        Value = null;
                        break;
                    case "set":
                        Set = kv.Value.Value;
                        Value = null;
                        break;
                    case "call":
                        Call = kv.Value.Value;
                        Value = null;
                        break;
                }
            }
        }

        public void Merge(Map map)
		{
            foreach (var kv in map.map)
            {
                switch (kv.Key.ToString().ToLower())
                {
                    case "value":
                        Value = kv.Value;
						Get = null;
						Set = null;
						Call = null;
                        break;
                    case "get":
                        Get = kv.Value;
						Value = null;
                        break;
                    case "set":
                        Set = kv.Value;
                        Value = null;
                        break;
                    case "call":
                        Call = kv.Value;
                        Value = null;
                        break;
                }
            }
        }

		public KeysharpObject GetDesc()
		{
            var map = new KeysharpObject();
            var list = new List<object>();
            map.op = new Dictionary<string, OwnPropsDesc>(StringComparer.OrdinalIgnoreCase);

			if (Value != null)
				map.op["value"] = new OwnPropsDesc(Parent, Value);
			if (Get != null)
                map.op["get"] = new OwnPropsDesc(Parent, Get);
			if (Set != null)
                map.op["set"] = new OwnPropsDesc(Parent, Set);
			if (Call != null)
                map.op["call"] = new OwnPropsDesc(Parent, Call);
			return map;
        }
    }
}