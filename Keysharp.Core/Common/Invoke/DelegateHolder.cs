
namespace Keysharp.Core.Common.Invoke
{
	public class DelegateHolder : KeysharpObject, IPointable
	{
		internal IFuncObj funcObj;
		private static readonly ModuleBuilder _modBuilder;
		private static readonly ConcurrentDictionary<int, Type> _closedTypeCache = new ();
		private static readonly ConcurrentDictionary<int, DynamicMethod> _stubCache = new ();

		public IntPtr Ptr { get; internal set; }
		public Delegate DelRef { get; private set; }
		internal bool Reference { get; }
		private readonly bool _fast;

		static DelegateHolder()
		{
			var asm = AssemblyBuilder.DefineDynamicAssembly(
						  new AssemblyName("DynamicDelegates"), AssemblyBuilderAccess.Run);
			_modBuilder = asm.DefineDynamicModule("M");
		}

		internal static void ClearCache()
		{
			_closedTypeCache.Clear();
			_stubCache.Clear();
		}

		public DelegateHolder(object obj, bool fast, bool reference, int paramCount)
		{
			funcObj = Functions.GetFuncObj(obj, null, true);
			_fast = fast;
			Reference = reference;
			int arity = Math.Clamp(
							paramCount < 0
							? (reference
							   ? 32
							   : (funcObj is FuncObj fo
								  ? (int)fo.MaxParams - ((fo is BoundFunc bo) ? bo.boundargs.Count() : 0)
								  : 0))
							: paramCount,
							0, 32);
			// build or get stub dynamic method for this arity with definition `static long Stub(DelegateHolder, long p0…pN)`
			var stub = _stubCache.GetOrAdd(arity, BuildStubMethod);
			// build or get closed delegate type Func<long0..longN>
			var closedType = _closedTypeCache.GetOrAdd(arity, BuildClosedType);
			// create closed delegate binding 'this'
			DelRef = stub.CreateDelegate(closedType, this);
			Ptr = Marshal.GetFunctionPointerForDelegate(DelRef);
		}

		/// <summary>
		/// Returns a DynamicMethod with type <c>static long Stub(DelegateHolder, long p0…pN)</c>
		/// that will package its parameters into a <c>long[]</c>
		/// and invoke this instance’s <see cref="DelegateTrampoline(long[])"/>.
		/// </summary>
		/// <param name="arity">The number of <c>long</c> parameters.</param>
		/// <returns>
		///   A <see cref="DynamicMethod"/> which can be used to create a closed delegate.
		/// </returns>
		private static DynamicMethod BuildStubMethod(int arity)
		{
			// 1) Create type <DelegateHolder, long p0..pN>
			var paramTypes = new Type[arity + 1];
			paramTypes[0] = typeof(DelegateHolder);

			for (int i = 0; i < arity; i++)
				paramTypes[i + 1] = typeof(long);

			// 2) Create a DynamicMethod: static long Stub(DelegateHolder, long p0…pN)
			var dm = new DynamicMethod(
				name: $"DynamicDelegate_{arity}",
				returnType: typeof(long),
				parameterTypes: paramTypes,
				m: typeof(DelegateHolder).Module,
				skipVisibility: true);
			// 3) Create the function body which loads the arguments from long0..longN to long[N]
			// and then calls this.DelegateTrampoline(long[N])
			var il = dm.GetILGenerator();
			var arrLocal = il.DeclareLocal(typeof(long[]));
			// new long[arity]
			il.Emit(OpCodes.Ldc_I4, arity);
			il.Emit(OpCodes.Newarr, typeof(long));
			il.Emit(OpCodes.Stloc, arrLocal);

			// fill array
			for (int i = 0; i < arity; i++)
			{
				il.Emit(OpCodes.Ldloc, arrLocal);
				il.Emit(OpCodes.Ldc_I4, i);
				il.Emit(OpCodes.Ldarg, i + 1);
				il.Emit(OpCodes.Stelem_I8);
			}

			// call this.DelegateTrampoline(arr)
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldloc, arrLocal);
			var mi = typeof(DelegateHolder)
					 .GetMethod(nameof(DelegateTrampoline), BindingFlags.Instance | BindingFlags.Public);
			il.EmitCall(OpCodes.Callvirt, mi, null);
			il.Emit(OpCodes.Ret);
			return dm;
		}

		// Build a type to accept <DelegateHolder, long0..longN>
		private static Type BuildClosedType(int arity)
		{
			string name = $"DynamicCallback{arity}";
			var tb = _modBuilder.DefineType(
						 name,
						 TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class,
						 typeof(MulticastDelegate));
			// ctor(object, IntPtr)
			var ctor = tb.DefineConstructor(
						   MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName,
						   CallingConventions.Standard,
						   new[] { typeof(object), typeof(IntPtr) });
			ctor.SetImplementationFlags(MethodImplAttributes.Runtime);
			// Invoke(long...)
			var paramTypes = Enumerable.Repeat(typeof(long), arity).ToArray();
			var mb = tb.DefineMethod(
						 "Invoke",
						 MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
						 CallingConventions.Standard,
						 typeof(long),
						 paramTypes);
			mb.SetImplementationFlags(MethodImplAttributes.Runtime);
			return tb.CreateType();
		}

		/// <summary>
		/// StubDelegate calls this function with the arguments packed into an array.
		/// Then we create a new green thread an run the target function there with the received arguments.
		/// </summary>
		/// <param name="args">Argument list for the target function.</param>
		/// <returns>Result of the target function converted to a long.</returns>
		public unsafe long DelegateTrampoline(long[] args)
		{
			object val = null;

			if (DelRef != null)
			{
				_ = Flow.TryCatch(() =>
				{
					var state = _fast ? (true, (ThreadVariables)null) : Threads.BeginThread();

					if (Reference)
					{
						var gh = GCHandle.Alloc(args, GCHandleType.Pinned);

						try
						{
							long* ptr = (long*)gh.AddrOfPinnedObject().ToPointer();
							val = funcObj.Call((long)ptr);
						}
						finally { gh.Free(); }
					}
					else
					{
						val = funcObj.Call(System.Array.ConvertAll(args, item => (object)item));
					}

					if (!_fast) _ = Threads.EndThread(state.Item1);
				}, !_fast);
			}

			return ConvertResult(val);
		}

		internal static long ConvertResult(object val)
		{

			return val switch
		{
				int i => i,
				long l => l,
				bool b => b ? 1L : 0L,
				double d => (long)d,
					string s => s.Length == 0 ? 0L : 0L,
					IntPtr ip => ip.ToInt64(),
					_ => 0L
			};
		}

		internal void Clear()
		{
			DelRef = null;
			funcObj = null;
			Ptr = 0;
		}
	}
}
