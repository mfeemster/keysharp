namespace Keysharp.Core.Common.Invoke
{
    public interface ICallable
    {
        public object Call(params object[] obj);

        public object CallWithRefs(params object[] obj);
    }
    public interface IFuncObj : ICallable
    {
        public object Inst { get; set; }
		public bool IsBuiltIn { get; }
		public bool IsValid { get; }
		public string Name { get; }

		public IFuncObj Bind(params object[] obj);

		public bool IsByRef(object obj = null);

		public bool IsOptional(object obj = null);
	}

	internal class BoundFunc : FuncObj
	{
		internal object[] boundargs;

		public new (Type, object) super => (typeof(BoundFunc), this);

		internal BoundFunc(MethodInfo m, object[] ba, object o = null)
			: base(m, o)
		{
			boundargs = ba;
		}

		public override IFuncObj Bind(params object[] args)
		{
			boundargs = boundargs.Concat(args);
			return this;
		}

		public override object Call(params object[] args) => base.Call(CreateArgs(args).ToArray());

        public override object CallWithRefs(params object[] args)
		{
			var argsList = CreateArgs(args);
			var refs = new List<RefHolder>(args.Length);

			for (var i = 0; i < argsList.Count; i++)
			{
				if (argsList[i] is RefHolder rh)
				{
					rh.index = i;//Must change the index since the array has changed.
					refs.Add(rh);
					argsList[i] = rh.val;
				}
			}

			var argsArray = argsList.ToArray();
			var val = base.Call(argsArray);

			for (var i = 0; i < refs.Count; i++)
			{
				var rh = refs[i];
				rh.reassign(argsArray[rh.index]);//Use value from new array.
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

			if (mph.IsVariadic)
			{
				for (; argsused < args.Length; argsused++)
					argsList.Add(args[argsused]);
			}
			else
			{
				for (; argsused < args.Length && argsused < mph.parameters.Length; argsused++)
					argsList.Add(args[argsused]);
			}

			while (argsList.Count < mph.parameters.Length)
			{
				var param = mph.parameters[argsList.Count];

				if (param.Attributes.HasFlag(ParameterAttributes.HasDefault))
					argsList.Add(param.DefaultValue);
				else
					argsList.Add(null);
			}

			return argsList;
		}
	}

	internal class FuncObj : KeysharpObject, IFuncObj
	{
		protected bool anyRef;
		protected object inst;
		protected bool isVariadic;
		protected MethodInfo mi;
		protected MethodPropertyHolder mph;
        new public static object __Static { get; set; }
        public object Inst
		{
			get => inst;
			set => inst = value;
		}
		public Type DeclaringType => mi.DeclaringType;
		public bool IsClosure => inst != null && mi.DeclaringType?.DeclaringType == inst.GetType();

        public bool IsBuiltIn => mi.DeclaringType.Module.Name.StartsWith("keysharp.core", StringComparison.OrdinalIgnoreCase);
		public bool IsValid => mi != null&& mph != null&& mph.callFunc != null;
		public string Name => mi != null ? mi.Name : "";
		internal bool IsVariadic => isVariadic;

		internal long MaxParams => 9999;//All functions in keysharp are variadic so this property doesn't apply.

		internal long MinParams => 0;//All functions in keysharp are variadic so this property doesn't apply.
		internal MethodPropertyHolder Mph => mph;

		public new (Type, object) super => (typeof(FuncObj), this);

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
		: this(Reflections.FindAndCacheMethod(Reflections.stringToTypes[t], s, paramCount.Ai(-1)))
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
			inst = o;

            if (mi != null)
				Init();
		}

		public virtual IFuncObj Bind(params object[] args)
		=> new BoundFunc(mi, args, inst);

		public virtual object Call(params object[] obj)
		{
			// No `this` is required
			if (mph.IsStaticFunc)
				return mph.callFunc(null, obj);
			// No other choice but to use `inst`
			else if (obj.Length == 0)
				return mph.callFunc(inst, obj);
			// `this` is required but not present
			else if (inst == null)
				return mph.callFunc(obj[0], obj.Skip(1).ToArray());
			// `this` is present in FuncObj but the user-provided `this` does not match the required type
			else if (!DeclaringType.IsAssignableFrom(obj[0].GetType()))
                return mph.callFunc(inst, obj.ToArray());
			// Not sure if this should use `obj[0]` or `inst`
            else
				return mph.callFunc(obj[0], obj.Skip(1).ToArray());
        }

		/*
		public virtual object CallWithInst(object inst, params object[] obj) {
			var decl = mph.mi.DeclaringType;
			var instt = inst.GetType();

			// If we have a compiler-generated closure *and* its type matches inst, then call it without 
			// passing inst as `this`
            if (Inst != null && mph.mi.DeclaringType?.DeclaringType == inst.GetType())
                return mph.callFunc(Inst, obj);
            else if (mph.IsStaticFunc || (Inst != null && mph.mi.DeclaringType != inst.GetType()))
				return mph.callFunc(Inst, new[] { inst }.Concat(obj).ToArray());
			else 
				return mph.callFunc(inst, obj);
		}
		*/

        public virtual object CallWithRefs(params object[] args)
		{
			var refs = new List<RefHolder>(args.Length);

			for (var i = 0; i < args.Length; i++)
			{
				object p = args[i];

				if (p is RefHolder rh)
				{
					refs.Add(rh);
					args[i] = rh.val;
				}
			}

			var val = mph.callFunc(inst, args);

			for (var i = 0; i < refs.Count; i++)
			{
				var rh = refs[i];
				rh.reassign(args[rh.index]);
			}

			return val;
		}

		public bool IsByRef(object obj = null)
		{
			var index = obj.Ai();
			var funcParams = mi.GetParameters();

			if (index > 0)
			{
				index--;

				if (index < funcParams.Length)
					return funcParams[index].ParameterType.IsByRef || funcParams[index].ParameterType == typeof(Misc.VarRef);
			}
			else
			{
				for (var i = 0; i < funcParams.Length; i++)
					if (funcParams[i].ParameterType.IsByRef || funcParams[index].ParameterType == typeof(Misc.VarRef))
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
			mph = new MethodPropertyHolder(mi, null);
			var parameters = mph.parameters;

			foreach (var p in parameters)
			{
				if (p.ParameterType.IsByRef)
				{
					anyRef = true;
					break;
				}
			}

			isVariadic = parameters.Length > 0 && parameters[parameters.Length - 1].ParameterType == typeof(object[]);
		}
	}

	public delegate void SimpleDelegate();

	public delegate void VariadicAction(params object[] args);

	public delegate object VariadicFunction(params object[] args);
}