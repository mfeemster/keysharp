namespace Keysharp.Core.Common.Invoke
{
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

	internal class BoundFunc : FuncObj
	{
		internal object[] boundargs;

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

		/// <summary>
		/// Even calling with no args might still need ref processing because some of the
		/// bound args might be refs. So always forward to <see cref="CallWithRefs(object[])"/>
		/// </summary>
		/// <param name="args">Forwarded on to <see cref="CallWithRefs(object[])"/></param>
		/// <returns>The return value of the bound function.</returns>
		public override object Call(params object[] args) => base.Call(CreateArgs(args).ToArray());
		public override object CallInst(object inst, params object[] args) => base.Call(inst, CreateArgs(args).ToArray());

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

	internal class Closure : FuncObj
	{
        internal Closure(Delegate m, object o = null) : base(m, o) { }
    }

	public class FuncObj : KeysharpObject, IFuncObj
	{
		protected bool anyRef;
		protected bool isVariadic;
		protected MethodInfo mi;
		protected MethodPropertyHolder mph;
		new public static object __Static { get; set; }
		public object Inst { get; set; }
		public Type DeclaringType => mi.DeclaringType;
		public bool IsClosure => Inst != null && mi.DeclaringType?.DeclaringType == Inst.GetType();

		public bool IsBuiltIn => mi.DeclaringType.Module.Name.StartsWith("keysharp.core", StringComparison.OrdinalIgnoreCase);
		public bool IsValid => mi != null && mph != null && mph.callFunc != null;
		public string Name => mi != null ? mi.Name : "";
		public new (Type, object) super => (typeof(KeysharpObject), this);
		internal bool IsVariadic => isVariadic;
		public long MaxParams { get; internal set; } = 9999;
		public long MinParams { get; internal set; } = 0;
		internal MethodPropertyHolder Mph => mph;

		public Func<object, object[], object> Delegate => mph.callFunc;

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
			Inst = o;

			if (Script.Variables.Prototypes.Count > 1)
			{
				Script.Variables.Prototypes.TryGetValue(GetType(), out KeysharpObject value);
				op["base"] = new OwnPropsDesc(this, value);
			}

			if (mi != null)
				Init();
		}

		public virtual IFuncObj Bind(params object[] args)
		=> new BoundFunc(mi, args, Inst);

		public virtual object Call(params object[] obj) => mph.callFunc(Inst, obj);
		public virtual object CallInst(object inst, params object[] obj)
		{
			if (Inst == null)
				return mph.callFunc(inst, obj);
			else
			{
                int count = obj.Length;
                object[] args = new object[count + 1];
                args[0] = inst;
                System.Array.Copy(obj, 0, args, 1, count);
                return mph.callFunc(Inst, args);
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

			var val = mph.callFunc(Inst, argsArray);

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
					return funcParams[index].ParameterType.IsByRef || funcParams[index].ParameterType == typeof(VarRef);
			}
			else
			{
				for (var i = 0; i < funcParams.Length; i++)
					if (funcParams[i].ParameterType.IsByRef || funcParams[index].ParameterType == typeof(VarRef))
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
			mph = new MethodPropertyHolder(mi, null); // TODO: find a way to cache these (or at least mph.callFunc delegate)
			var parameters = mph.parameters;
			MinParams = mph.MinParams;
			MaxParams = mph.MaxParams;

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