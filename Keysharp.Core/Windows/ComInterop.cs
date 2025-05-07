#if WINDOWS
using static Keysharp.Scripting.Script;

namespace Keysharp.Core.Common.ObjectBase
{
	public partial class KeysharpObject : Any, IReflect
	{
		#region IReflect implementation
		public FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			// only own (no base) and only if there's a Value slot
			if (Script.TryGetOwnPropsMap(this, name, out var opm, searchBase: false, type: OwnPropsMapType.Value))
				return new SimpleFieldInfo(name);
			return null;
		}
		public FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			var list = new List<FieldInfo>();
			if (op != null)
			{
				foreach (var kv in op)
				{
					if (kv.Value.Value != null)  // only explicit Value entries
						list.Add(new SimpleFieldInfo(kv.Key));
				}
			}
			return list.ToArray();
		}
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
		public PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			var list = new List<PropertyInfo>();
			if (Script.TryGetProps(this, out var props, true, OwnPropsMapType.Get | OwnPropsMapType.Set))
			{

				foreach (var kv in props)
				{
					var opm = kv.Value;
					bool hasGet = opm.Get != null;
					bool hasSet = opm.Set != null;
					list.Add(new SimplePropertyInfo(kv.Key, hasGet, hasSet));
				}
			}
			return list.ToArray();
		}
		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr) => null;
		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder? binder, Type? type, Type[] types, ParameterModifier[]? modifiers) => null;
		public MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
		{
			var list = new List<MemberInfo>();
			var f = GetField(name, bindingAttr);
			if (f != null) list.Add(f);
			var p = GetProperty(name, bindingAttr);
			if (p != null) list.Add(p);
			var ms = GetMethods(bindingAttr);
			foreach (var m in ms) if (string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase))
					list.Add(m);
			return list.ToArray();
		}
		public MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			var all = new List<MemberInfo>();
			all.AddRange(GetFields(bindingAttr));
			all.AddRange(GetProperties(bindingAttr));
			all.AddRange(GetMethods(bindingAttr));
			return all.ToArray();
		}

		const int DISPID_VALUE = 0;
		const int DISPID_UNKNOWN = -1;
		const int DISPID_PROPERTYPUT = -3;
		const int DISPID_NEWENUM = -4;
		const int DISPID_EVALUATE = -5;
		const int DISPID_CONSTRUCTOR = -6;
		const int DISPID_DESTRUCTOR = -7;
		const int DISPID_COLLECT = -8;

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
			if (name == null || name == "")
				throw new Error("Invoked member name can't be empty");

			if (args == null)
				args = [];

			var argCount = args.Length;

			if (args.Length > 0 && args[^1] is object[] tail && tail != null)
			{
				// Last parameter was variadic and C# converted the arguments to object[],
				// so let's concat it back.
				int headCount = argCount - 1;
				int tailCount = tail.Length;
				var result = new object[headCount + tailCount];
				System.Array.Copy(args, 0, result, 0, headCount);
				System.Array.Copy(tail, 0, result, headCount, tailCount);
				args = result;
			}

			for (int i = 0; i < argCount; i++)
			{
				if (args[i] is System.Reflection.Missing)
					args[i] = null;
			}

			if (name.Equals("_NewEnum", StringComparison.OrdinalIgnoreCase)
				|| name.Equals($"[DISPID={DISPID_NEWENUM}]", StringComparison.OrdinalIgnoreCase))
			{
				name = "__Enum";
				if (args.Length == 0)
					args = [2];
			}
			else if (name.Equals("__Item", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("_Item", StringComparison.OrdinalIgnoreCase)
				|| name.Equals($"[DISPID={DISPID_VALUE}]", StringComparison.OrdinalIgnoreCase))
			{
				if (this is Array)
				{
					for (int i = 0; i < argCount - 1; i++)
					{
						args[i] = args[i].Ai() + 1;
					}
				}

				if (target != null && target is FuncObj fo)
				{
					return fo.Call(args);
				}

				if ((invokeAttr & BindingFlags.GetProperty) != 0
					|| (invokeAttr & BindingFlags.GetField) != 0)
				{
					return Script.Index(target ?? this, args);
				}
				else
				{
					object value = argCount > 0 ? args[^1] : null;
					var indices = new object[argCount > 0 ? argCount - 1 : 0];
					System.Array.Copy(args, indices, indices.Length);

					return Script.SetObject(value, target ?? this, indices);
				}
			}

			// indexer? AutoHotkey uses DISPID=0 for __Item
			else if (name.StartsWith("[DISPID=", StringComparison.OrdinalIgnoreCase))
			{
				// parse the number inside the brackets
				var dispStr = name.Substring(8, name.Length - 9); // drop "[DISPID=" and "]"
				if (!int.TryParse(dispStr, out int dispId))
					throw new Error($"Failed to parse DISPID from {name}");
				switch (dispId)
				{
					case DISPID_CONSTRUCTOR:
						name = "__New";
						break;
					case DISPID_DESTRUCTOR:
						name = "__Delete";
						break;
				}

				throw new Error($"Failed to invoke property/method for {name}");
			}

			// property getter?
			if ((invokeAttr & BindingFlags.GetProperty) != 0)
				return Script.GetPropertyValue(target ?? this, name);

			// property setter?
			if ((invokeAttr & BindingFlags.SetProperty) != 0)
			{
				Script.SetPropertyValue(target ?? this, name, argCount > 0 ? args[0] : null);
				return null;
			}

			// method call
			if ((invokeAttr & BindingFlags.InvokeMethod) != 0)
				return Script.Invoke(target ?? this, name, args);

			throw new MissingMemberException($"Member '{name}' not found");
		}

		public Type UnderlyingSystemType => typeof(KeysharpObject);
		#endregion
	}

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

	/// <summary>
	/// A fake FieldInfo exposing only a name and treating everything as object.
	/// </summary>
	sealed class SimpleFieldInfo : FieldInfo
	{
		readonly string _name;
		public SimpleFieldInfo(string name) { _name = name; }

		public override string Name => _name;
		public override Type FieldType => typeof(object);
		public override object GetValue(object obj) => Script.GetPropertyValue(obj, _name);
		public override void SetValue(object obj, object val, BindingFlags bindingFlags, Binder binder, CultureInfo ci)
			=> Script.SetPropertyValue(obj, _name, new object[] { val });

		#region All other members just delegate / throw NotSupported
		public override FieldAttributes Attributes => FieldAttributes.Public;
		public override RuntimeFieldHandle FieldHandle => throw new NotSupportedException();
		public override Type DeclaringType => typeof(KeysharpObject);
		public override object[] GetCustomAttributes(bool inherit) => System.Array.Empty<object>();
		public override object[] GetCustomAttributes(Type attrType, bool inherit) => System.Array.Empty<object>();
		public override bool IsDefined(Type attrType, bool inherit) => false;
		public override Module Module => typeof(KeysharpObject).Module;
		public override Type ReflectedType => typeof(KeysharpObject);
		#endregion
	}

	/// <summary>
	/// A fake PropertyInfo exposing only name, read/write and delegating to Script.Get/SetPropertyValue.
	/// </summary>
	sealed class SimplePropertyInfo : PropertyInfo
	{
		readonly string _name;
		readonly bool _canRead, _canWrite;
		public SimplePropertyInfo(string name, bool canRead, bool canWrite)
		{
			_name = name;
			_canRead = canRead;
			_canWrite = canWrite;
		}

		public override string Name => _name;
		public override bool CanRead => _canRead;
		public override bool CanWrite => _canWrite;
		public override Type PropertyType => typeof(object);
		public override MethodInfo[] GetAccessors(bool nonPublic) => System.Array.Empty<MethodInfo>();
		public override MethodInfo GetGetMethod(bool nonPublic) => null;
		public override MethodInfo GetSetMethod(bool nonPublic) => null;
		public override object GetValue(object obj, BindingFlags bindingFlags, Binder binder, object[] index, CultureInfo ci)
			=> Script.GetPropertyValue(obj, _name);
		public override void SetValue(object obj, object value, BindingFlags bindingFlags, Binder binder, object[] index, CultureInfo ci)
			=> Script.SetPropertyValue(obj, _name, new object[] { value });

		#region Other members stubbed out
		public override ParameterInfo[] GetIndexParameters() => System.Array.Empty<ParameterInfo>();
		public override Type DeclaringType => typeof(KeysharpObject);
		public override object[] GetCustomAttributes(bool inherit) => System.Array.Empty<object>();
		public override object[] GetCustomAttributes(Type attrType, bool inherit) => System.Array.Empty<object>();
		public override bool IsDefined(Type attrType, bool inherit) => false;
		public override PropertyAttributes Attributes => PropertyAttributes.None;
		public override Module Module => typeof(KeysharpObject).Module;
		public override Type ReflectedType => typeof(KeysharpObject);
		#endregion
	}
}
#endif