namespace Keysharp.Core.Common.Invoke
{
	public interface IPointable
	{
		public long Ptr { get; }
	}
    public interface IFuncObj
    {
        public object Inst { get; set; }
		public bool IsBuiltIn { get; }
		public bool IsValid { get; }
		public string Name { get; }

		public IFuncObj Bind(params object[] obj);

        public object Call(params object[] obj);
		public object CallInst(object inst, params object[] obj);

        public object CallWithRefs(params object[] obj);

        public bool IsByRef(object obj = null);

		public bool IsOptional(object obj = null);
	}

	public class BoundFunc : FuncObj
	{
		internal object[] boundargs;

		internal BoundFunc(MethodInfo m, object[] ba, object o = null)
			: base(m, o)
		{
			boundargs = ba;
			int argCount = ba.Length;
			// Find last non-null argument which determines the actual provided argument count
			for (; argCount > 0; argCount--)
			{
				if (ba[argCount - 1] != null)
					break;
			}
			if (argCount < ba.Length)
				System.Array.Resize(ref boundargs, argCount);


			if (argCount > MaxParams && !IsVariadic)
				throw new Error("Too many arguments bound to function");

			// Now calculate the new MinParams/MaxParams
			int minParams = (int)MinParams;
			int maxParams = (int)MaxParams;
			for (int i = 0; i < argCount && i < maxParams; i++)
			{
				// Empty slots do not change the counts
				if (ba[i] == null)
					continue;
				// If the index is greater than minimum param count then only MaxParams can be decreased.
				// We don't overflow into the variadic parameter because of the maxParams check above.
				if (i < minParams)
				{
					if (MinParams > 0)
						MinParams--;
				}
				if (MaxParams > 0)
					MaxParams--;
			}
		}

		public override IFuncObj Bind(params object[] args)
		{
			object[] newbound = new object[boundargs.Length + args.Length];
			System.Array.Copy(boundargs, newbound, boundargs.Length);
			int skipped = 0;
			for (int i = 0; i < boundargs.Length && skipped < args.Length; i++)
			{
				if (newbound[i] == null)
				{
					newbound[i] = args[skipped];
					skipped++;
				}
			}
			int leftCount = args.Length - skipped;
			if (leftCount > 0)
			{
				System.Array.Copy(args, args.Length - leftCount, newbound, boundargs.Length, leftCount);
			}
			return new BoundFunc(mi, newbound, Inst);
		}

		/// <summary>
		/// Even calling with no args might still need ref processing because some of the
		/// bound args might be refs. So always forward to <see cref="CallWithRefs(object[])"/>
		/// </summary>
		/// <param name="args">Forwarded on to <see cref="CallWithRefs(object[])"/></param>
		/// <returns>The return value of the bound function.</returns>
		public override object Call(params object[] args) => base.Call(CreateArgs(args).ToArray());
		public override object CallInst(object inst, params object[] args) => base.Call(CreateArgs([inst, ..args]).ToArray());

        public override object CallWithRefs(params object[] args)
		{
			var argsList = CreateArgs(args);
			var argsArray = new object[argsList.Count];

			for (var i = 0; i < argsList.Count; ++i)
			{
				var p = argsList[i];

				if (p is RefHolder rh)
				{
					rh.index = i;//Must change the index since the array has changed.
					argsArray[i] = rh.val;
				}
				else
					argsArray[i] = p;
			}

			var val = base.Call(argsArray);

			for (int i = 0, argsIndex = 0; i < argsList.Count; ++i)
			{
				//If it was a RefHolder, then reassign regardless if it was passed from the bound args or the passed in args.
				if (argsList[i] is RefHolder rh)
				{
					rh.reassign(argsArray[rh.index]);//Use value from new array.
				}
				else if (argsIndex < args.Length
						 && i < mph.parameters.Length//This seems like it should always be true.
						 && mph.parameters[i].ParameterType.IsByRef
						)//It wasn't a RefHolder, so determine where it should go.
				{
					args[argsIndex++] = argsArray[i];//Reassign all the way back to the original.
				}
			}

			return val;
		}

		private List<object> CreateArgs(params object[] args)
		{
			int i = 0, argsused = 0;
			var argsList = new List<object>(mph.parameters.Length);

			for (; i < boundargs.Length; i++)
			{
				if (boundargs[i] != null)
				{
					argsList.Add(boundargs[i]);
				}
				else if (argsused < args.Length)
				{
					argsList.Add(args[argsused]);
					argsused++;
				}
				else
					argsList.Add(null);
			}

			for (; argsused < args.Length; argsused++)
				argsList.Add(args[argsused]);

			while (argsList.Count < mph.parameters.Length)
			{
				var param = mph.parameters[argsList.Count];

				if (param.Attributes.HasFlag(ParameterAttributes.HasDefault))
					argsList.Add(param.DefaultValue);
				else
					break;
			}

			return argsList;
		}
	}

	public class Closure : FuncObj
	{
        internal Closure(Delegate m, object o = null) : base(m, o) { }
    }

	public class FuncObj : KeysharpObject, IFuncObj
	{
		protected MethodInfo mi;
		protected MethodPropertyHolder mph;

		[PublicForTestOnly]
		public object Inst { get; set; }
		public Type DeclaringType => mi.DeclaringType;
		public bool IsClosure => Inst != null && mi.DeclaringType?.DeclaringType == Inst.GetType();

		public bool IsBuiltIn => mi.DeclaringType.Namespace != TheScript.ProgramType.Namespace;
		public bool IsValid => mi != null && mph != null && mph.CallFunc != null;
		string _name = null;
		public string Name {
			get
			{
				if (_name != null)
					return _name;

				if (mi == null)
					return _name = "";

				var nameAttrs = mi.GetCustomAttributes(typeof(UserDeclaredNameAttribute));
				if (nameAttrs.Any())
				{
					return _name = ((UserDeclaredNameAttribute)nameAttrs.First()).Name;
				}

				string funcName = mi.Name;
				var prefixes = new[] { "static", "get_", "set_" };
				foreach (var p in prefixes)
				{
					if (funcName.StartsWith(p, StringComparison.Ordinal))
						funcName = funcName.Substring(p.Length);
				}

				if (IsBuiltIn || mi.DeclaringType.Name == Keywords.MainClassName)
					return _name = funcName;

				string declaringType = mi.DeclaringType.FullName;

				var idx = declaringType.IndexOf(Keywords.MainClassName + "+");
				string nestedPath = idx < 0
					? declaringType       // no “Program.” found, just return whole
					: declaringType.Substring(idx + Keywords.MainClassName.Length + 1);

				return _name = $"{nestedPath.Replace('+', '.')}.{funcName}";
			}
		}
		public (Type, object) super => (typeof(KeysharpObject), this);
		public bool IsVariadic => mph.variadicParamIndex != -1;
		public long MaxParams { get; internal set; } = 0;
		public long MinParams { get; internal set; } = 0;
		internal int VariadicIndex => mph.variadicParamIndex;
		internal MethodPropertyHolder Mph => mph;

		internal FuncObj(string s, object o = null, object paramCount = null)
			: this(GetMethodInfo(s, o, paramCount), o)
		{
		}

        private static MethodInfo GetMethodInfo(string s, object o, object paramCount)
        {
            if (o != null)
            {
				var mitup = Script.GetMethodOrProperty(o, s, paramCount.Ai(-1));
				if (mitup.Item2 is FuncObj fo)
					return fo.mph.mi;
				else if (mitup.Item2 is MethodPropertyHolder mph)
					return mph.mi;
                // Try to find and cache the method
                var method = Reflections.FindAndCacheMethod(o.GetType(), s, paramCount.Ai(-1));
                if (method != null)
                    return method.mi;

				throw new TargetError("Unable to find a method object for the requested method " + s);
            }

            // Fallback to finding the method without an object
            return Reflections.FindMethod(s, paramCount.Ai(-1))?.mi;
        }

        internal FuncObj(string s, string t, object paramCount = null)
		: this(Reflections.FindAndCacheMethod(Script.TheScript.ReflectionsData.stringToTypes[t], s, paramCount.Ai(-1)))
        {
        }

        internal FuncObj(string s, Type t, object paramCount = null)
		: this(Reflections.FindAndCacheMethod(t, s, paramCount.Ai(-1)))
        {
        }

        internal FuncObj(MethodPropertyHolder m, object o = null)
			: this(m?.mi, o)
		{
		}

		internal FuncObj(Delegate m, object o = null)
		: this(m?.GetMethodInfo(), o)
        {
			this.Inst = m.Target;
        }

		internal FuncObj(MethodInfo m, object o = null)
		{
			mi = m;
			Inst = o;

			if (Script.TheScript.Vars.Prototypes.Count > 1)
			{
				Script.TheScript.Vars.Prototypes.TryGetValue(GetType(), out Any value);
				_base = value;
			}

			if (mi != null)
				Init();
		}

		public virtual IFuncObj Bind(params object[] args)
		=> new BoundFunc(mi, args, Inst);

		public virtual object Call(params object[] obj) => mph.CallFunc(Inst, obj);
		public virtual object CallInst(object inst, params object[] obj)
		{
			if (Inst == null)
			{
				return mph.CallFunc(inst, obj);
			}
			else
			{
				int count = obj.Length;
				object[] args = new object[count + 1];
				args[0] = inst;
				System.Array.Copy(obj, 0, args, 1, count);
				return mph.CallFunc(Inst, args);
			}
		}
        public virtual object CallWithRefs(params object[] args)
		{
			var argsArray = new object[args.Length];

			for (var i = 0; i < args.Length; i++)
			{
				var p = args[i];

				if (p is RefHolder rh)
				{
					rh.index = i;//Might not be needed here, but just to be safe.
					argsArray[i] = rh.val;
				}
				else
					argsArray[i] = p;
			}

			var val = mph.CallFunc(Inst, argsArray);

			for (var i = 0; i < args.Length; i++)
			{
				if (args[i] is RefHolder rh)
					rh.reassign(argsArray[rh.index]);
				//args.Length could exceed parameters.Length if the last param was variadic.
				//So don't assign back because variadic parameters aren't expected to be reference params, even though
				//they might technically be able to be called that way with reflection.
				else if (i < args.Length && i < mph.parameters.Length && mph.parameters[i].ParameterType.IsByRef)
					args[i] = argsArray[i];//Reassign all the way back to the original.
			}

			return val;
		}

		public override bool Equals(object obj) => obj is FuncObj fo ? fo.mi == mi : false;

		public bool Equals(FuncObj value) => value.mi == mi;

		public override int GetHashCode() => mi.GetHashCode();

		public bool IsByRef(object obj = null)
		{
			var index = obj.Ai();
			var funcParams = mi.GetParameters();

			if (index > 0)
			{
				index--;

				if (index < funcParams.Length)
					return funcParams[index].ParameterType.IsByRef || funcParams[index].GetCustomAttribute(typeof(ByRefAttribute)) != null;
			}
			else
			{
				for (var i = 0; i < funcParams.Length; i++)
					if (funcParams[i].ParameterType.IsByRef || funcParams[index].GetCustomAttribute(typeof(ByRefAttribute)) != null)
						return true;
			}

			return false;
		}

		public bool IsOptional(object obj = null)
		{
			var index = obj.Ai();
			var funcParams = mi.GetParameters();

			if (index > 0)
			{
				index--;

				if (index < funcParams.Length)
					return funcParams[index].IsOptional;
			}
			else
			{
				for (var i = 0; i < funcParams.Length; i++)
					if (funcParams[i].IsOptional)
						return true;
			}

			return false;
		}

		private void Init()
		{
			mph = MethodPropertyHolder.GetOrAdd(mi);
			MinParams = mph.MinParams;
			MaxParams = mph.MaxParams;
		}
	}

	public delegate void SimpleDelegate();

	public delegate void VariadicAction(params object[] args);

	public delegate object VariadicFunction(params object[] args);
}