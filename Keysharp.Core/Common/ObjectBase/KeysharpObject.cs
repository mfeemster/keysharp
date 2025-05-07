using System.Reflection;
using static Keysharp.Scripting.Script;

[assembly: ComVisible(true)]

namespace Keysharp.Core.Common.ObjectBase
{
	internal interface I__Enum
	{
		public IFuncObj __Enum(object count);
	}

	[Guid("934B0E6A-9B50-4248-884B-BE5A9BC66B39")]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ProgId("Keysharp.Script")]
	public class KeysharpObject : Any, IReflect
	{
        protected internal Dictionary<string, OwnPropsDesc> op = new Dictionary<string, OwnPropsDesc>(StringComparer.OrdinalIgnoreCase);

        MethodInfo mi;
		// In some cases we wish to skip the automatic calls to __Init and __New (eg when creating OwnProps),
		// so in those cases we can initialize with `skipLogic: true`
		protected bool SkipConstructorLogic { get; }

        public KeysharpObject(params object[] args)
		{
			// Skip Map and OwnPropsMap because SetPropertyValue will cause recursive stack overflow
			// (if the property doesn't exist then a new Map is created which calls this function again)
			if (Script.Variables.Prototypes == null || SkipConstructorLogic
                // Hack way to check that Prototypes/Statics are initialized
                || Script.Variables.Statics.Count < 10)
            {
				__New(args);
				return;
			}

            var t = GetType();
            Script.Variables.Statics.TryGetValue(t, out KeysharpObject value);
			if (value == null)
			{
				__New(args);
                return;
			}

			this.op["base"] = new OwnPropsDesc(this, value.op["prototype"].Value);
			GC.SuppressFinalize(this); // Otherwise if the constructor throws then the destructor is called
            Script.Invoke(this, "__Init");
            Script.Invoke(this, "__New", args);
			GC.ReRegisterForFinalize(this);
        }

        public KeysharpObject(bool skipLogic)
        {
            SkipConstructorLogic = skipLogic;
        }

		public virtual object static__New(params object[] args) => "";

		public virtual object __New(params object[] args) => "";

		public object Call(params object[] args) => Activator.CreateInstance(this.GetType(), args);

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
					op = new Dictionary<string, OwnPropsDesc>(StringComparer.OrdinalIgnoreCase);

				if (!op.ContainsKey(name))
					op[name] = new OwnPropsDesc(this, map);
				else
					op[name].Merge(map);
			}
			else if (obj1 is KeysharpObject kso)
			{
				if (kso.op != null)//&& kso.op.TryGetValue(name, out var opm))
				{
					if (op == null)
						op = new Dictionary<string, OwnPropsDesc>(StringComparer.OrdinalIgnoreCase);

					if (op.TryGetValue(name, out var currProp))
					{
						currProp.MergeOwnPropsValues(kso.op);
					}
					else
					{
						op[name] = new OwnPropsDesc();
						op[name].MergeOwnPropsValues(kso.op);
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
				ct += op.Count - 1; // Substract 1 to account for the auto-generated base property

			if (!isMapOnly)
			{
				_ = Reflections.FindAndCacheProperty(GetType(), "", -1);
				ct += Reflections.OwnPropCount(GetType());
			}

			return ct;
		}

		public object OwnProps(object getValues = null, object userOnly = null)
		{
			var vals = getValues == null || getValues.Ab();
			var user = userOnly.Ab(true);
			var props = new Dictionary<object, object>();

			if (op != null)
			{
				foreach (var kv in op)
				{
					if (kv.Key == "base")
						continue;

					props[kv.Key] = kv.Value;
				}
			}

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

		public virtual object static__Init() => "";

		/// <summary>
		/// Placeholder for property initialization code that derived classes will call *before* __New() gets called.
		/// </summary>
		public virtual object __Init() => "";

		#region IReflect implementation

		public FieldInfo GetField(string name, BindingFlags bindingAttr) => null;
		public FieldInfo[] GetFields(BindingFlags bindingAttr) => System.Array.Empty<FieldInfo>();
		public MethodInfo GetMethod(string name, BindingFlags bindingAttr) => null;
		public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers) => null;
		public MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			List<MethodInfo> meths = new();
			KeysharpObject kso = this;
			if (Script.TryGetProps(this, out var props, true, OwnPropsMapType.Call))
			{
				foreach (var prop in props)
				{
					var opm = prop.Value;
					if (opm.Call is FuncObj fo && fo != null)
					{
						if (fo.Mph.mi.Name.Equals(prop.Key, StringComparison.OrdinalIgnoreCase))
							meths.Add(fo.Mph.mi);
						else
							meths.Add(new RenamedMethodInfo(fo.Mph.mi, prop.Key));
					}
				}
			}

			return meths.ToArray();
		}
		public PropertyInfo[] GetProperties(BindingFlags bindingAttr) => System.Array.Empty<PropertyInfo>();
		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr) => null;
		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder? binder, Type? type, Type[] types, ParameterModifier[]? modifiers) => null;
		public MemberInfo[] GetMember(string name, BindingFlags bindingAttr) => System.Array.Empty<MemberInfo>();
		public MemberInfo[] GetMembers(BindingFlags bindingAttr) => System.Array.Empty<MemberInfo>();
		public object InvokeMember(
			string name,
			BindingFlags invokeAttr,
			Binder binder,
			object target,
			object[] args,
			ParameterModifier[] modifiers,
			System.Globalization.CultureInfo culture,
			string[] namedParameters)
		{
			// property getter?
			if ((invokeAttr & BindingFlags.GetProperty) != 0)
				return Script.GetPropertyValue(target, name);

			// property setter?
			if ((invokeAttr & BindingFlags.SetProperty) != 0)
			{
				Script.SetPropertyValue(target, name, args);
				return null;
			}

			// method call
			if ((invokeAttr & BindingFlags.InvokeMethod) != 0)
				return Script.Invoke(target, name, args);

			throw new MissingMemberException($"Member '{name}' not found");
		}

		public Type UnderlyingSystemType => typeof(KeysharpObject);

		/// <summary>
		/// A MethodInfo that delegates to an underlying MethodInfo
		/// but returns a different Name.
		/// </summary>
		sealed class RenamedMethodInfo : MethodInfo
		{
			readonly MethodInfo _inner;
			readonly string _fakeName;
			public RenamedMethodInfo(MethodInfo inner, string fakeName)
			{
				_inner = inner;
				_fakeName = fakeName;
			}

			public override string Name => _fakeName;

			// everything else just delegates to _inner…
			public override ICustomAttributeProvider ReturnTypeCustomAttributes
				=> _inner.ReturnTypeCustomAttributes;
			public override MethodAttributes Attributes
				=> _inner.Attributes;
			public override Type DeclaringType
				=> _inner.DeclaringType;
			public override RuntimeMethodHandle MethodHandle
				=> _inner.MethodHandle;
			public override Type ReflectedType
				=> _inner.ReflectedType;
			public override MethodImplAttributes GetMethodImplementationFlags()
				=> _inner.GetMethodImplementationFlags();
			public override ParameterInfo[] GetParameters()
				=> _inner.GetParameters();
			public override object[] GetCustomAttributes(bool inherit)
				=> _inner.GetCustomAttributes(inherit);
			public override object[] GetCustomAttributes(Type attrType, bool inherit)
				=> _inner.GetCustomAttributes(attrType, inherit);
			public override bool IsDefined(Type attrType, bool inherit)
				=> _inner.IsDefined(attrType, inherit);
			public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
				=> _inner.Invoke(obj, invokeAttr, binder, parameters, culture);
			public new object Invoke(object obj, object[] parameters)
				=> _inner.Invoke(obj, parameters);
			public override MethodInfo GetBaseDefinition()
				=> _inner.GetBaseDefinition();
			public override Type ReturnType
				=> _inner.ReturnType;
			public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
				=> _inner.MakeGenericMethod(typeArguments);
			public override bool ContainsGenericParameters
				=> _inner.ContainsGenericParameters;
			public override bool IsGenericMethod
				=> _inner.IsGenericMethod;
			public override bool IsGenericMethodDefinition
				=> _inner.IsGenericMethodDefinition;
			public override Type[] GetGenericArguments()
				=> _inner.GetGenericArguments();
		}

		#endregion
	}
}