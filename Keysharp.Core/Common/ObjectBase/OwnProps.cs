namespace Keysharp.Core.Common.ObjectBase
{
	public class OwnPropsIterator : KeysharpEnumerator, IEnumerator<(object, object)>
	{
		private readonly Dictionary<object, object> map;
		private readonly KeysharpObject obj;
		private IEnumerator<KeyValuePair<object, object>> iter;
		public int Count => GetVal ? 2 : 1;

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

		internal bool GetVal { get; set; }

		object IEnumerator.Current => Current;

		public OwnPropsIterator(KeysharpObject o, Dictionary<object, object> m, bool gv)
			: base(null, gv ? 2 : 1)
		{
			Error err;
			obj = o;
			map = m;
			GetVal = gv;
			iter = map.GetEnumerator();
			var fo = new FuncObj("Call", this, Count);

			if (fo.IsValid)
				CallFunc = fo;
			else
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

		private IEnumerator<(object, object)> GetEnumerator() => this;
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
                        Value = kv.Value;
                        break;
                    case "get":
                        Get = kv.Value;
                        break;
                    case "set":
                        Set = kv.Value;
                        break;
                    case "call":
                        Call = kv.Value;
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
                        break;
                    case "get":
                        Get = kv.Value;
                        break;
                    case "set":
                        Set = kv.Value;
                        break;
                    case "call":
                        Call = kv.Value;
                        break;
                }
            }
        }

		public KeysharpObject GetDesc()
		{
            var list = new List<object>();
			if (Value != null)
				list.Add(Value);
            if (Get != null)
                list.Add(Value);
            if (Set != null)
                list.Add(Value);
            if (Call != null)
                list.Add(Value);
            return Keysharp.Core.Objects.Object(list.ToArray());
        }
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