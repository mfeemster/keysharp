namespace Keysharp.Core.Common.ObjectBase
{
	public class OwnPropsMap : Map
	{
		public KeysharpObject Parent { get; private set; }

		public OwnPropsMap(KeysharpObject kso, Map map)
			: base(false)
		{
			_ = __New(kso, map);
		}

		public new object __New(params object[] args)
		{
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

	internal class OwnPropsIteratorData
	{
		internal FuncObj p1, p2;

		internal OwnPropsIteratorData()
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
			var p = Count <= 1 ? script.OwnPropsIteratorData.p1 : script.OwnPropsIteratorData.p2;
			var fo = (FuncObj)p.Clone();
			fo.Inst = this;
			CallFunc = fo;
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

		public void Dispose() => Reset();

		public bool MoveNext() => iter.MoveNext();

		public void Reset() => iter = map.GetEnumerator();
	}
}