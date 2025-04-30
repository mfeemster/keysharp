namespace Keysharp.Core.Common.Invoke
{
	public class DelegateHolder
	{
		internal IFuncObj funcObj;
		private static readonly AssemblyBuilder dynamicAssembly;
		private static readonly ModuleBuilder dynamicModule;
		private static readonly ConcurrentDictionary<int, Type> _typeCache = new();
		private readonly bool fast;

		public Delegate DelRef;
		internal bool Reference { get; }

		static DelegateHolder()
		{
			AssemblyName an = new AssemblyName("DynamicDelegates");
			dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
			dynamicModule = dynamicAssembly.DefineDynamicModule("M");
		}

		public DelegateHolder(object obj, bool f, bool r, int pc)
		{
			funcObj = Functions.GetFuncObj(obj, null, true);
			fast = f;
			Reference = r;

			if (pc < 0)
			{
				if (Reference)
					pc = 32;
				else if (funcObj is FuncObj fo)
				{
					pc = (int)fo.MaxParams;
					if (fo is BoundFunc bo)
						pc -= bo.boundargs.Count();
				}
			}
			DelRef = BuildStubDelegate(Math.Clamp(pc, 0, 32));
		}

		public Type BuildType(int arity)
		{
			string typeName = "DynamicCallback" + arity;
			var tb = dynamicModule.DefineType(typeName,
			  TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class,
			  typeof(MulticastDelegate));

			// 1) ctor(object, IntPtr)
			var ctor = tb.DefineConstructor(
				MethodAttributes.Public | MethodAttributes.HideBySig |
				MethodAttributes.RTSpecialName,
				CallingConventions.Standard,
				new[] { typeof(object), typeof(IntPtr) });
			ctor.SetImplementationFlags(MethodImplAttributes.Runtime);

			// 2) Invoke method
			var paramTypes = Enumerable.Repeat(typeof(long), arity).ToArray();
			var mb = tb.DefineMethod("Invoke",
				MethodAttributes.Public | MethodAttributes.HideBySig |
				MethodAttributes.NewSlot | MethodAttributes.Virtual,
				CallingConventions.Standard,
				typeof(long),    // return
				paramTypes);
			mb.SetImplementationFlags(MethodImplAttributes.Runtime);

			// 3) Bake the type
			return tb.CreateType();
		}

		/// <summary>
		/// Emits (and returns) a managed delegate of signature
		///   <c>long Callback(long p0, long p1, …, long pN)</c>
		/// that will package its parameters into a <c>long[]</c>
		/// and invoke this instance’s <see cref="DelegateTrampoline(long[])"/>.
		/// </summary>
		/// <param name="argumentCount">The number of <c>long</c> parameters (0–31).</param>
		/// <returns>
		///   A <see cref="Delegate"/> which can be passed to
		///   <see cref="Marshal.GetFunctionPointerForDelegate"/> to obtain the native pointer.
		/// </returns>
		private Delegate BuildStubDelegate(int argumentCount)
		{
			// 1) Build the delegate type Func<long,…,long> (argumentCount longs in, one long out)
			var delegateType = _typeCache.GetOrAdd(argumentCount, BuildType);

			// 2) Create a DynamicMethod: static long Stub(DelegateHolder, long p0…pN)
			var parameterTypes = new Type[argumentCount + 1];
			parameterTypes[0] = typeof(DelegateHolder);
			for (int i = 0; i < argumentCount; i++)
				parameterTypes[i + 1] = typeof(long);

			var dm = new DynamicMethod(
				name: $"DynamicDelegate_{argumentCount}",
				returnType: typeof(long),
				parameterTypes: parameterTypes,
				m: typeof(DelegateHolder).Module,
				skipVisibility: true);

			var il = dm.GetILGenerator();

			// Local: long[] arr
			var arrLocal = il.DeclareLocal(typeof(long[]));

			// 3) arr = new long[argumentCount];
			il.Emit(OpCodes.Ldc_I4, argumentCount);
			il.Emit(OpCodes.Newarr, typeof(long));
			il.Emit(OpCodes.Stloc, arrLocal);

			// 4) Load the arguments into the array
			for (int i = 0; i < argumentCount; i++)
			{
				il.Emit(OpCodes.Ldloc, arrLocal);    // load array
				il.Emit(OpCodes.Ldc_I4, i);           // load index
				il.Emit(OpCodes.Ldarg, i + 1);       // load parameter p_i
				il.Emit(OpCodes.Stelem_I8);           // arr[i] = p_i
			}

			// 5) callvirt this.DelegateTrampoline(arr)
			il.Emit(OpCodes.Ldarg_0);                // load 'this'
			il.Emit(OpCodes.Ldloc, arrLocal);        // load the array
			var mi = typeof(DelegateHolder)
						.GetMethod(nameof(DelegateTrampoline),
								   BindingFlags.Instance | BindingFlags.Public);
			il.Emit(OpCodes.Callvirt, mi);

			// 6) return
			il.Emit(OpCodes.Ret);

			// 7) Create a delegate, binding 'this' as the first argument
			return dm.CreateDelegate(delegateType, this);
		}

		/// <summary>
		/// StubDelegate calls this function with the arguments packed into an array.
		/// Then we create a new green thread an run the target function there with the received arguments.
		/// </summary>
		/// <param name="args">Argument list for the target function.</param>
		/// <returns>Result of the target function converted to a long.</returns>
		public long DelegateTrampoline(long[] args)
		{
			object val = null;

			if (DelRef != null)
			{
				_ = Flow.TryCatch(() =>
				{
					(bool, ThreadVariables) btv = (false, null);

					if (!fast)
						btv = Threads.BeginThread();

					if (Reference)
					{
						var handle = GCHandle.Alloc(args, GCHandleType.Pinned);
						try
						{
							unsafe
							{
								long* ptr = (long*)handle.AddrOfPinnedObject().ToPointer();
								val = funcObj.Call((long)ptr);
							}
						}
						finally
						{
							handle.Free();
						}
					}
					else
						val = funcObj.Call(System.Array.ConvertAll(args, item => (object)item));

					if (!fast)
						_ = Threads.EndThread(btv.Item1);
				}, !fast);//Pop on exception because EndThread() above won't be called.
			}

			return ConvertResult(val);
		}

		internal static long ConvertResult(object val)
		{
			if (val is int i)
				return i;
			else if (val is long l)
				return l;
			else if (val is bool b)
				return b ? 1L : 0L;
			else if (val is double d)
				return (long)d;
			else if (val is string s && s.Length == 0)
				return 0L;
			else if (val is null)
				return 0L;
			else if (val is IntPtr ip)
				return ip.ToInt64();

			return 0L;
		}

		internal void Clear()
		{
			DelRef = null;
			funcObj = null;
		}

		internal long DirectCall(params object[] parameters)
		{
			object val = null;
			_ = Flow.TryCatch(() =>
			{
				(bool, ThreadVariables) btv = (false, null);

				if (!fast)
					btv = Threads.BeginThread();

				if (Reference)
				{
					var helper = new ComArgumentHelper(parameters);

					var handle = GCHandle.Alloc(helper.args, GCHandleType.Pinned);
					try
					{
						unsafe
						{
							long* ptr = (long*)handle.AddrOfPinnedObject().ToPointer();
							val = funcObj.Call((long)ptr);
						}
					}
					finally
					{
						handle.Free();
					}
				}
				else
				{
					var helper = new DllArgumentHelper(parameters);
					val = funcObj.Call(helper.args);
				}

				if (!fast)
					_ = Threads.EndThread(btv.Item1);
			}, !fast);//Pop on exception because EndThread() above won't be called.
			return ConvertResult(val);
		}
	}
}