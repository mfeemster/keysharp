namespace Keysharp.Core.Common.ObjectBase
{
	internal interface I__Enum
	{
		public KeysharpEnumerator __Enum(object count);
	}

#if WINDOWS
	[Guid("98D592E1-0CE8-4892-82C5-F219B040A390")]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ProgId("Keysharp.Script")]
	public partial class KeysharpObject : Any, IReflect
#else
	public class KeysharpObject : Any
#endif
	{
		protected internal Dictionary<string, OwnPropsDesc> op;

		public new (Type, object) super => (typeof(Any), this);

		public KeysharpObject()
		{
			__Init();
		}

		public object __New(params object[] args) => "";

		/// <summary>
		/// Return a cloned copy of the object.
		/// Just calling MemberwiseClone() is sufficient to clone all of the properties as well
		/// as the OwnProps object op.
		/// </summary>
		/// <returns>A cloned copy of the object.</returns>
		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public object DefineProp(object obj0, object obj1)
		{
			var name = obj0.As();

			if (op == null)
				op = new Dictionary<string, OwnPropsDesc>(StringComparer.OrdinalIgnoreCase);

			if (obj1 is Map map)
			{
				if (!op.ContainsKey(name))
					op[name] = new OwnPropsDesc(this, map);
				else
				{
					if (map.map.Count > 1 && map.map.Any(k => k.Key.ToString().Equals("value", StringComparison.OrdinalIgnoreCase)))
						return Errors.ValueErrorOccurred("Value can't be defined along with get, set, or call.");

					op[name].Merge(map);
				}
			}
			else if (obj1 is KeysharpObject kso)
			{
				if (kso.op != null)//&& kso.op.TryGetValue(name, out var opm))
				{
					if (kso.op.Count > 1 && kso.op.Any(k => k.Key.ToString().Equals("value", StringComparison.OrdinalIgnoreCase)))
						return Errors.ValueErrorOccurred("Value can't be defined along with get, set, or call.");

					if (op.TryGetValue(name, out var currProp))
					{
						currProp.MergeOwnPropsValues(kso.op);
					}
					else
					{
						op[name] = new OwnPropsDesc(this);
						op[name].MergeOwnPropsValues(kso.op);
					}

					kso.op.Clear();
				}
			}
			else
			{
				return Errors.ArgumentErrorOccurred(obj1, 2);
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

			return DefaultObject;
		}

		public long GetCapacity() => (long)Errors.ErrorOccurred("GetCapacity() is not supported or needed in Keysharp. The C# runtime handles all memory.", DefaultErrorLong);

		public object GetOwnPropDesc(object obj)
		{
			var name = obj.As();

			if (op != null && op.TryGetValue(name, out var dynProp))
			{
				return dynProp.GetDesc();
			}

			try
			{
				var val = Script.GetPropertyValue(this, name);
				return Objects.Object(["value", val]);
			}
			catch
			{
			}

			return Errors.PropertyErrorOccurred($"Object did not have an OwnProp named {name}.");
		}

		public long HasOwnProp(object obj)
		{
			var name = obj.As().ToLower();

			if (op != null && op.ContainsKey(name))
				return 1L;

			return Reflections.FindOwnProp(GetType(), name) ? 1L : 0L;
		}

		public long OwnPropCount()
		{
			var ct = 0L;
			var isMapOnly = GetType() == typeof(Map);

			if (op != null)
				ct += op.Count;

			if (!isMapOnly)
			{
				_ = Reflections.FindAndCacheProperty(GetType(), "", -1);
				ct += Reflections.OwnPropCount(GetType());
			}

			return ct;
		}

		public object OwnProps(object getValues = null, object userOnly = null)
		{
			var vals = getValues.Ab(true);
			var user = userOnly.Ab(true);
			var props = new Dictionary<object, object>();

			if (op != null)
			{
				foreach (var kv in op)
				{
					props[kv.Key] = kv.Value;
				}
			}

			_ = Reflections.FindAndCacheProperty(GetType(), "", -1);
			var valProps = Reflections.GetOwnProps(GetType(), user);

			foreach (var mph in valProps)
				if (mph.pi != null && mph.ParamLength == 0)
					props[mph.pi.Name] = mph;

			return new OwnPropsIterator(this, props, vals);
		}

		public virtual void PrintProps(string name, StringBuffer sb, ref int tabLevel)
		{
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

				if (val != this && val is KeysharpObject kso2)//Check against this to prevent stack overflow.
				{
					kso2.PrintProps(propName.ToString(), sb, ref tabLevel);
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

		public void SetBase(params object[] obj) => _ = Errors.ErrorOccurred(BaseExc);

		public long SetCapacity(object obj)
		{
			var err = new Error("SetCapacity() is not supported or needed in Keysharp. The C# runtime handles all memory.");
			return Errors.ErrorOccurred(err) ? throw err : DefaultErrorLong;
		}

		public object Wrap(object obj) => obj;

		protected static object __StaticInit() => "";

		protected internal void Init__Item()
		{
			_ = DefineProp("__Item",
						   Objects.Object(
							   [
								   "get",
								   Functions.GetFuncObj("ItemWrapper", this, 1, true),
								   "set",
								   Functions.GetFuncObj("ItemWrapper", this, 1, true)
							   ]));
		}

		/// <summary>
		/// Wrapper so that querying for the __Item property will succeed.
		/// </summary>
		/// <param name="obj">Unused.</param>
		/// <returns>this</returns>
		public object ItemWrapper(object obj) => this;

		/// <summary>
		/// Placeholder for property initialization code that derived classes will call *before* __New() gets called.
		/// </summary>
		protected virtual void __Init()
		{
		}
	}
}