namespace Keysharp.Core.Common.Invoke
{
	public interface IPointable
	{
		public long Ptr { get; }
	}

	public interface IFuncObj
	{
		public object Inst { get; }
		public bool IsBuiltIn { get; }
		public bool IsValid { get; }
		public string Name { get; }

		public IFuncObj Bind(params object[] obj);

		public object Call(params object[] obj);

		public object CallWithRefs(params object[] obj);

		public bool IsByRef(object obj = null);

		public bool IsOptional(object obj = null);
	}

	internal class BoundFunc : FuncObj
	{
		internal object[] boundargs;

		public new (Type, object) super => (typeof(FuncObj), this);

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
		public override object Call(params object[] args) => CallWithRefs(args);

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

			if (mph.IsVariadic)
			{
				for (; argsused < args.Length; argsused++)
					argsList.Add(args[argsused]);
			}
			else
			{
				for (; argsused < args.Length && argsList.Count < mph.parameters.Length; argsused++)
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
		protected bool isVariadic;
		protected MethodInfo mi;
		protected MethodPropertyHolder mph;

		[PublicForTestOnly]
		public object Inst { get; internal set; }
		public bool IsBuiltIn => mi.DeclaringType.Module.Name.StartsWith("keysharp.core", StringComparison.OrdinalIgnoreCase);
		public bool IsValid => mi != null && mph != null && mph.IsCallFuncValid;
		public string Name => mi != null ? mi.Name : "";
		public new (Type, object) super => (typeof(KeysharpObject), this);
		public bool IsVariadic => isVariadic;
		public long MaxParams { get; internal set; } = 0;
		public long MinParams { get; internal set; } = 0;
		internal int VariadicIndex => mph.startVarIndex;
		internal MethodPropertyHolder Mph => mph;

		internal FuncObj(string s, object o = null, object paramCount = null)
			: this(o != null ? Reflections.FindAndCacheMethod(o.GetType(), s, paramCount.Ai(-1)) : Reflections.FindMethod(s, paramCount.Ai(-1)), o)
		{
		}

		internal FuncObj(MethodPropertyHolder m, object o = null)
			: this(m?.mi, o)
		{
		}

		internal FuncObj(Delegate d, object o = null)
			: this(d.Method, o)
		{
		}

		internal FuncObj(MethodInfo m, object o = null)
		{
			mi = m;
			Inst = o;

			if (mi != null)
				Init();
		}

		public virtual IFuncObj Bind(params object[] args)
		=> new BoundFunc(mi, args, Inst);

		public virtual object Call(params object[] args) => mph.CallFunc(Inst, args);

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