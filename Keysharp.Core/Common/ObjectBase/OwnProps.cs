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

	internal class OwnPropsIteratorData : BaseIteratorData<OwnPropsIterator>
	{
	}

	internal class OwnPropsIterator : KeysharpEnumerator, IEnumerator<(object, object)>
	{
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
					if (kv.Value is OwnPropsDesc op)
					{
						if (op.Value != null)
							return (kv.Key, op.Value);
						else if (op.Get != null && op.Get is FuncObj fo)
							return (kv.Key, fo.Call(obj));
						else if (op.Call != null)
							return (kv.Key, op.Call);
					} 
					else if (kv.Value is MethodPropertyHolder mph)
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
		private bool _getVal = false;
		internal bool GetVal {
			get => _getVal;
			set
			{
				_getVal = value;
				var p = value ? Script.TheScript.OwnPropsIteratorData.p2 : Script.TheScript.OwnPropsIteratorData.p1;
				fo = (FuncObj)p.Clone();
				fo.Inst = this;
				CallFunc = fo;
			}
		}

		internal OwnPropsIterator(KeysharpObject o, Dictionary<object, object> m, bool gv)
			: base(null, gv ? 2 : 1)
		{
			obj = o;
			map = m;
			GetVal = gv;
			iter = map.GetEnumerator();
		}

		public override object Call([ByRef] object obj0)
		{
			if (MoveNext())
			{
				GetVal = false;
				Script.SetPropertyValue(obj0, "__Value", Current.Item1);
				return true;
			}

			return false;
		}

		public override object Call([ByRef] object obj0, [ByRef] object obj1)
		{
			if (MoveNext())
			{
				GetVal = true;
				Script.SetPropertyValue(obj0, "__Value", Current.Item1);
				Script.SetPropertyValue(obj1, "__Value", Current.Item2);
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

		public void Merge(OwnPropsDesc opd)
		{
			if (opd.Value != null)
				Value = opd.Value;
			if (opd.Get != null)
				Get = opd.Get;
			if (opd.Set != null)
				Set = opd.Set;
			if (opd.Call != null)
				Call = opd.Call;
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