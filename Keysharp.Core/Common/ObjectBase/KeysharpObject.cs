namespace Keysharp.Core.Common.ObjectBase
{
	internal interface I__Enum
	{
		public IFuncObj __Enum(object count);
	}
	public class KeysharpObject : Any
	{
		public KeysharpObject(params object[] args) : base(args) { }
		public KeysharpObject(bool skipLogic) : base(skipLogic) { }

		public object staticCall(params object[] args)
		{
			var kso = new KeysharpObject();
			var count = (args.Length / 2) * 2;

			for (var i = 0; i < count; i += 2)
			{
				kso.op[args[i].ToString()] = new OwnPropsDesc(kso, args[i + 1]);
			}

			return kso;
		}

		/// <summary>
		/// Return a cloned copy of the object.
		/// Just calling MemberwiseClone() is sufficient to clone all of the properties as well
		/// as the OwnProps object op.
		/// </summary>
		/// <returns>A cloned copy of the object.</returns>
		public new object Clone()
		{
			return MemberwiseClone();
		}

		public object DefineProp(object obj0, object obj1) => Objects.ObjDefineProp(this, obj0, obj1);

		public object DeleteProp(object obj)
		{
			var name = obj.As().ToLower();

			if (op != null && op.Remove(name, out var map))
			{
				if (op.Count == 0)
					op = null;//Make all subsequent member access faster because this won't have to be checked first.

				return map.Value;
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
					if (kv.Key == "__Static") //This throws if the value is tried to access because "this" is passed
						continue;

					props[kv.Key] = kv.Value;
				}
			}

			return new OwnPropsIterator(this, props, vals).fo;
		}

		public virtual void PrintProps(string name, StringBuffer sb, ref int tabLevel)
		{
			var fieldType = GetType().Name;
			var opi = (OwnPropsIterator)((FuncObj)OwnProps(true, false)).Inst;
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

		[PublicForTestOnly]
		public override Any Base
		{
			get => _base;
			set => Objects.ObjSetBase(this, value);
		}

		public long SetCapacity(object obj)
		{
			var err = new Error("SetCapacity() is not supported or needed in Keysharp. The C# runtime handles all memory.");
			return Errors.ErrorOccurred(err) ? throw err : DefaultErrorLong;
		}
	}
}