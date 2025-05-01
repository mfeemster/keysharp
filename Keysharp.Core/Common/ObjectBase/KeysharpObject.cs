namespace Keysharp.Core.Common.ObjectBase
{
	internal interface I__Enum
	{
		public KeysharpEnumerator __Enum(object count);
	}

	public class KeysharpObject : Any
	{
		protected internal Dictionary<string, OwnPropsMap> op;

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

		public KeysharpObject DefineProp(object obj0, object obj1)
		{
			var name = obj0.As();

			if (obj1 is Map map)
			{
				if (op == null)
					op = new Dictionary<string, OwnPropsMap>(new CaseEqualityComp(eCaseSense.Off));

				op[name] = new OwnPropsMap(this, map);
			}
			else if (obj1 is KeysharpObject kso)
			{
				if (kso.op != null)//&& kso.op.TryGetValue(name, out var opm))
				{
					if (op == null)
						op = new Dictionary<string, OwnPropsMap>(new CaseEqualityComp(eCaseSense.Off));

					_ = op.Remove(name);//Clear, but this will prevent defining the property across multiple calls such as first adding value, then get, then set.

					foreach (var kv in kso.op)
					{
						if (op.TryGetValue(name, out var currProp))
							currProp.map[kv.Key] = kv.Value[kv.Key];//Merge.
						else
							op[name] = new OwnPropsMap(this, new Map(false, kv.Value.map));//Create new.
					}

					kso.op.Clear();
				}
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

		public long GetCapacity()
		{
			Error err;
			return Errors.ErrorOccurred(err = new Error("GetCapacity() is not supported or needed in Keysharp. The C# runtime handles all memory.")) ? throw err : 0L;
		}

		public object GetOwnPropDesc(object obj)
		{
			Error err;
			var name = obj.As().ToLower();

			if (op != null && op.TryGetValue(name, out var dynProp))
			{
				var kso = new KeysharpObject();
				var list = new List<object>();
				kso.op = new Dictionary<string, OwnPropsMap>();

				foreach (var kv in dynProp.map)
				{
					list.Add(kv.Key);

					//Must wrap in a function so that when GetMethodOrProperty() unwraps it, it just returns the property and
					//doesn't actually call get().
					if (string.Compare(kv.Key.ToString(), "get", true) == 0)
						list.Add(new FuncObj("Wrap", this).Bind(kv.Value));
					else
						list.Add(kv.Value);
				}

				return Objects.Object(list.ToArray());
			}

			try
			{
				var val = Script.GetPropertyValue(this, name);
				return Objects.Object(["value", val]);
			}
			catch
			{
			}

			return Errors.ErrorOccurred(err = new PropertyError($"Object did not have an OwnProp named {name}.")) ? throw err : null;
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
			var vals = getValues.Ab();
			var user = userOnly.Ab(true);
			var props = new Dictionary<object, object>();

			if (op != null)
			{
				foreach (var kv in op)
				{
					foreach (var propkv in kv.Value.map)
					{
						var prop = propkv.Value;

						if (prop != null)
						{
							if (prop is not FuncObj fo
									|| (fo.Mph.mi != null && fo.Mph.ParamLength <= 1))
								props[kv.Key] = prop;
						}
					}
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

		public void SetBase(params object[] obj)
		{
			Error err;
			_ = Errors.ErrorOccurred(err = new Error(BaseExc)) ? throw err : "";
		}

		public long SetCapacity(object obj)
		{
			var err = new Error("SetCapacity() is not supported or needed in Keysharp. The C# runtime handles all memory.");
			return Errors.ErrorOccurred(err) ? throw err : 0L;
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