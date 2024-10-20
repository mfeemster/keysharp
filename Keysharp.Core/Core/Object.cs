namespace Keysharp.Core
{
	public class Any
	{
		public static string BaseExc = "Changing a class base property at runtime cannot be implemented in C#.";

		public object Base => GetType().BaseType;//Documentation says this can be set, but C# doesn't support changing a base at runtime.

		public (Type, object) super
		{
			get
			{
				return (GetType().BaseType, this);
			}
		}

		public virtual IFuncObj GetMethod(object obj0 = null, object obj1 = null) => Function.GetMethod(this, obj0, obj1);

		//public bool DefineProp(object obj0, object obj1)
		//{
		//  var name obj0.As();

		//}

		public long HasBase(object obj) => obj.GetType().IsAssignableFrom(GetType()) ? 1L : 0L;

		public long HasMethod(object obj0 = null, object obj1 = null) => Function.HasMethod(this, obj0, obj1);

		public long HasProp(object obj) => Function.HasProp(this, obj);

		//public virtual string tostring() => ToString();
	}

	public class KeysharpObject : Any
	{
		protected internal Dictionary<string, OwnpropsMap> op;

		public static KeysharpObject Object() => new KeysharpObject();

		public virtual object Clone()
		{
			return MemberwiseClone();
			//If ownprops are implemented, might need to add extra code for those.
		}

		public KeysharpObject DefineProp(object obj0, object obj1)
		{
			var name = obj0.As().ToLower();

			if (obj1 is Keysharp.Core.Map map)
			{
				if (op == null)
					op = new Dictionary<string, OwnpropsMap>(new CaseEqualityComp(eCaseSense.Off));

				var mapCopy = new OwnpropsMap(this, map);
				//if (mapCopy.map.TryGetValue("get", out var getprop) && getprop is FuncObj getfo)
				//  mapCopy.map["get"] = getfo is BoundFunc ? getfo : getfo.Bind(this);
				//if (mapCopy.map.TryGetValue("set", out var setprop) && setprop is FuncObj setfo)
				//  mapCopy.map["set"] = setfo is BoundFunc ? setfo : setfo.Bind(this);
				//if (mapCopy.map.TryGetValue("call", out var callprop) && callprop is FuncObj callfo)
				//  mapCopy.map["call"] = callfo is BoundFunc ? callfo : callfo.Bind(this);
				op[name] = mapCopy;
			}

			return this;
		}

		public object DeleteProp(object obj)
		{
			var name = obj.As().ToLower();

			if (op != null && op.Remove(name, out var map))
			{
				if (op.Count == 0)
					op = null;//Make all subsequent member access faster because this won't have to be checked first.

				return map;
			}

			return "";
		}

		public long GetCapacity() => throw new Keysharp.Core.Error("GetCapacity() is not supported or needed in Keysharp. The C# runtime handles all memory.");

		public object GetOwnPropDesc(object obj)
		{
			var name = obj.As().ToLower();

			if (this is Keysharp.Core.Map map)
			{
				if (map.map.TryGetValue(name, out var mapVal))
				{
					return new OwnpropsMap(this, Misc.Map(["Value", mapVal]));
				}
			}

			if (op != null && op.TryGetValue(name, out var dynProp))
				return dynProp.Clone();

			try
			{
				return Keysharp.Scripting.Script.GetPropertyValue(this, name);
			}
			catch
			{
			}

			throw new PropertyError($"Object did not have an OwnProp named {name}.");
		}

		public long HasOwnProp(object obj)
		{
			var name = obj.As().ToLower();

			if (this is Keysharp.Core.Map map)
				if (map.map.ContainsKey(name))
					return 1L;

			if (op != null && op.ContainsKey(name))
				return 1L;

			return Reflections.FindOwnProp(GetType(), name) ? 1L : 0L;
		}

		public long OwnPropCount()
		{
			var ct = 0L;
			var isMapOnly = GetType() == typeof(Keysharp.Core.Map);

			if (this is Keysharp.Core.Map map)
				ct += map.map.Count;

			if (op != null)
				ct += op.Count;

			if (!isMapOnly)
			{
				_ = Reflections.FindAndCacheProperty(GetType(), "", -1);
				ct += Reflections.OwnPropCount(GetType());
			}

			return ct;
		}

		public object OwnProps(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			var getVals = obj0.Ab();
			var userOnly = obj1.Ab(true);
			var skipMap = obj2.Ab(false);
			var props = new Dictionary<object, object>();

			if (!skipMap)
				if (this is Keysharp.Core.Map map && map.map.Count > 0)
					foreach (var kv in map.map)
						if (!getVals
								|| kv.Value is not FuncObj fo
								|| (fo.Mph.mi != null && fo.Mph.ParamLength <= 1))
							props[kv.Key] = kv.Value;

			if (op != null)
				foreach (var kv in op)
					if (kv.Value.map.TryGetValue("get", out var dynprop))
						if (!getVals
								|| dynprop is not FuncObj fo
								|| (fo.Mph.mi != null && fo.Mph.ParamLength <= 1))
							props[kv.Key] = dynprop;

			_ = Reflections.FindAndCacheProperty(GetType(), "", -1);
			var valProps = Reflections.GetOwnProps(GetType(), userOnly);

			foreach (var mph in valProps)
				if (!getVals || (mph.pi != null && mph.ParamLength == 0))
					props[mph.pi.Name] = mph;

			return new OwnPropsIterator(this, props, getVals);
		}

		public virtual void PrintProps(string name, StringBuffer sbuf, ref int tabLevel)
		{
			var sb = sbuf.sb;
			var fieldType = GetType().Name;
			var opi = (OwnPropsIterator)OwnProps(true, false);
			var indent = new string('\t', tabLevel);

			if (name.Length == 0)
				_ = sb.AppendLine($"{indent} ({fieldType})");
			else
				_ = sb.AppendLine($"{indent}{name}: ({fieldType})");

			tabLevel++;
			indent = new string('\t', tabLevel);

			while (opi.MoveNext())
			{
				var (propName, val) = opi.Current;
				fieldType = val != null ? val.GetType().Name : "";

				if (val is KeysharpObject kso2)
				{
					kso2.PrintProps(propName.ToString(), sbuf, ref tabLevel);
				}
				else if (val != null)
				{
					if (val is string vs)
					{
						var str = "\"" + vs + "\"";//Can't use interpolated string here because the AStyle formatter misinterprets it.
						_ = sb.AppendLine($"{indent}{propName}: {str} ({fieldType})");
					}
					else
						_ = sb.AppendLine($"{indent}{propName}: {val} ({fieldType})");
				}
				else
					_ = sb.AppendLine($"{indent}{propName}: null");
			}

			tabLevel--;
		}

		public virtual object __New(params object[] obj) => "";

		protected static object __StaticInit() => "";

		public void SetBase(params object[] obj) => throw new Exception(Any.BaseExc);

		public long SetCapacity(object obj) => throw new Keysharp.Core.Error("SetCapacity() is not supported or needed in Keysharp. The C# runtime handles all memory.");
	}

	public class OwnPropsIterator : IEnumerator<(object, object)>
	{
		private readonly bool getVal;
		private IEnumerator<KeyValuePair<object, object>> iter;
		private readonly Dictionary<object, object> map;
		private readonly KeysharpObject obj;

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

	public class OwnpropsMap : Keysharp.Core.Map
	{
		public KeysharpObject Parent { get; private set; }

		public OwnpropsMap(KeysharpObject kso, Keysharp.Core.Map map)
		{
			Parent = kso;
			Default = map.Default;
			Capacity = map.Capacity;
			CaseSense = "Off";

			foreach (var kv in map.map)
				this[kv.Key] = kv.Value;
		}

		public override object Clone()
		{
			return new OwnpropsMap(Parent, this);
		}
	}
}