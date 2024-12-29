namespace Keysharp.Core.Common.Invoke
{
    public interface ICallable
    {
        public object Call(params object[] obj);

        public object CallWithRefs(params object[] obj);
    }
    public interface IFuncObj : ICallable
    {
        public object Inst { get; }
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

		internal BoundFunc(MethodInfo m, object[] ba, object o = null)
			: base(m, o)
		{
			boundargs = ba;
		}

		public override object Call(params object[] obj) => base.Call(CreateArgs(obj).ToArray());

		public override object CallWithRefs(params object[] obj)
		{
			var argsList = CreateArgs(obj);
			var refs = new List<RefHolder>(obj.Length);

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

		private List<object> CreateArgs(params object[] obj)
		{
			int i = 0, argsused = 0;
			var argsList = new List<object>(mph.parameters.Length);

			for (; i < boundargs.Length; i++)
			{
				if (boundargs[i] != null)
				{
					argsList.Add(boundargs[i]);
				}
				else if (argsused < obj.Length)
				{
					argsList.Add(obj[argsused]);
					argsused++;
				}
				else
					argsList.Add(null);
			}

			if (mph.IsVariadic)
			{
				for (; argsused < obj.Length; argsused++)
					argsList.Add(obj[argsused]);
			}
			else
			{
				for (; argsused < obj.Length && argsused < mph.parameters.Length; argsused++)
					argsList.Add(obj[argsused]);
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
		public object Inst => inst;
		public bool IsBuiltIn => mi.DeclaringType.Module.Name.StartsWith("keysharp.core", StringComparison.OrdinalIgnoreCase);
		public bool IsValid => mi != null&& mph != null&& mph.callFunc != null;
		public string Name => mi != null ? mi.Name : "";
		internal bool IsVariadic => isVariadic;

		internal long MaxParams => 9999;//All functions in keysharp are variadic so this property doesn't apply.

		internal long MinParams => 0;//All functions in keysharp are variadic so this property doesn't apply.
		internal MethodPropertyHolder Mph => mph;

		internal FuncObj(string s, object o = null, object paramCount = null)
			: this(o != null ? Reflections.FindAndCacheMethod(o.GetType(), s, paramCount.Ai(-1)) : Reflections.FindMethod(s, paramCount.Ai(-1)), o)
		{
		}

		internal FuncObj(MethodPropertyHolder m, object o = null)
			: this(m?.mi, o)
		{
		}

        internal FuncObj(Delegate m, object o = null)
		: this(m?.GetMethodInfo(), o)
        {
        }

        internal FuncObj(MethodInfo m, object o = null)
		{
			mi = m;
			inst = o;

			if (mi != null)
				Init();
		}

		public IFuncObj Bind(params object[] obj)
		=> new BoundFunc(mi, obj, inst);

		public virtual object Call(params object[] obj) => mph.callFunc(inst, obj);

		public virtual object CallWithRefs(params object[] obj)
		{
			var refs = new List<RefHolder>(obj.Length);

			for (var i = 0; i < obj.Length; i++)
			{
				object p = obj[i];

				if (p is RefHolder rh)
				{
					refs.Add(rh);
					obj[i] = rh.val;
				}
			}

			var val = mph.callFunc(inst, obj);

			for (var i = 0; i < refs.Count; i++)
			{
				var rh = refs[i];
				rh.reassign(obj[rh.index]);
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
					return funcParams[index].ParameterType.IsByRef;
			}
			else
			{
				for (var i = 0; i < funcParams.Length; i++)
					if (funcParams[i].ParameterType.IsByRef)
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

	public delegate void VariadicAction(params object[] o);

	public delegate object VariadicFunction(params object[] args);
}