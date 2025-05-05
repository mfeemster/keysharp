namespace Keysharp.Core.Common.Invoke
{
	/// <summary>
	/// Creates a native callback pointer which can be used to call the target function object.
	/// The callback starts from a stub which inserts the pointer to this DelegateHolder
	/// instance to the end of the argument list and forwards the call to SharedTrampoline,
	/// which in turn packs the arguments into an array and calls Dispatch, which then
	/// pushes a new green thread (unless Fast mode is used) and calls the target function.
	/// </summary>
	public sealed class DelegateHolder : KeysharpObject, IDisposable, IPointable
	{
		internal IFuncObj funcObj;
		readonly bool _fast, _reference;
		readonly int _arity;

		// Self GCHandle & pointer to self
		readonly GCHandle _selfHandle;
		readonly IntPtr _ctx;

		// Precomputed trampolines for parameter counts 0..32
		static readonly IntPtr[] _trampolinePtrs;

		// Native function pointer to pass into unmanaged code.
		public IntPtr Ptr { get; }

		static DelegateHolder()
		{
			_trampolinePtrs = Enumerable.Range(0, 33)
							  .Select(n =>
			{
				var mi = typeof(DelegateHolder)
						 .GetMethod($"SharedTrampoline{n}", BindingFlags.Static | BindingFlags.NonPublic);
				RuntimeHelpers.PrepareMethod(mi.MethodHandle);
				return mi.MethodHandle.GetFunctionPointer();
			})
			.ToArray();
		}

		/// <summary>
		/// Creates a holder and allocates a native stub that embeds its context.
		/// </summary>
		public DelegateHolder(object function, bool fast, bool reference, int paramCount)
		{
			funcObj = Functions.GetFuncObj(function, null, true);
			_fast = fast;
			_reference = reference;
			_arity = Math.Clamp(paramCount < 0
								? (_reference ? 32 : (funcObj is FuncObj fo
										? (int)fo.MaxParams - ((fo is BoundFunc bo) ? bo.boundargs.Count() : 0)
										: 0))
								: paramCount,
								0, 32);
			// Pin self and store handle as context
			_selfHandle = GCHandle.Alloc(this, GCHandleType.Normal);
			_ctx = GCHandle.ToIntPtr(_selfHandle);
			// Create native stub which inserts context and calls SharedTrampoline
			Ptr = NativeThunkFactory.CreateThunk(_trampolinePtrs[_arity], _ctx, _arity);
		}

		// Shared unmanaged-callable trampolines
		[UnmanagedCallersOnly]
		static long SharedTrampoline0(IntPtr ctx)
		=> Dispatch(ctx, System.Array.Empty<long>());

		[UnmanagedCallersOnly]
		static long SharedTrampoline1(long p0, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline2(long p0, long p1, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline3(long p0, long p1, long p2, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline4(long p0, long p1, long p2, long p3, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline5(long p0, long p1, long p2, long p3, long p4, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline6(long p0, long p1, long p2, long p3, long p4, long p5, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline7(long p0, long p1, long p2, long p3, long p4, long p5, long p6, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline8(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline9(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline10(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline11(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline12(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline13(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline14(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline15(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline16(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline17(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline18(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline19(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline20(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline21(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, IntPtr ctx)
		=> Dispatch(ctx, new[] {p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline22(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline23(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22
							   });

		[UnmanagedCallersOnly]
		static long SharedTrampoline24(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline25(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline26(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline27(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline28(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, long p27, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26, p27 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline29(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, long p27, long p28, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26, p27, p28 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline30(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, long p27, long p28, long p29, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26, p27, p28, p29 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline31(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, long p27, long p28, long p29, long p30, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26, p27, p28, p29, p30 });

		[UnmanagedCallersOnly]
		static long SharedTrampoline32(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, long p27, long p28, long p29, long p30, long p31, IntPtr ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26, p27, p28, p29, p30, p31 });

		/// <summary>
		/// SharedTrampoline calls this function with the arguments packed into an array.
		/// We then create a new green thread an run the target function there with the received arguments.
		/// </summary>
		/// <param name="ctx">Pointer to this DelegateHolder instance.</param>
		/// <param name="args">Argument list for the target function.</param>
		/// <returns>Result of the target function converted to a long.</returns>
		static long Dispatch(IntPtr ctx, long[] args)
		{
			// Recover the DelegateHolder by reversing the GCHandle
			var handle = GCHandle.FromIntPtr(ctx);

			if (!(handle.Target is DelegateHolder holder))
			{
				Error err;
				return Errors.ErrorOccurred(err = new Error($"Invalid DelegateHolder pointer passed to Dispatch.")) ? throw err : 0L;
			}

			object val = null;
			_ = Flow.TryCatch(() =>
			{
				var state = holder._fast
							? (true, (ThreadVariables)null)
							: Threads.BeginThread();

				if (holder._reference)
				{
					var gh = GCHandle.Alloc(args, GCHandleType.Pinned);

					try
					{
						unsafe
						{
							long* ptr = (long*)gh.AddrOfPinnedObject().ToPointer();
							val = holder.funcObj.Call((long)ptr);
						}
					}
					finally { gh.Free(); }
				}
				else
				{
					val = holder.funcObj.Call(System.Array.ConvertAll(args, item => (object)item));
				}

				if (!holder._fast)
					_ = Threads.EndThread(state.Item1);
			}, !holder._fast);
			return ConvertResult(val);
		}

		internal static long ConvertResult(object val) => val switch
	{
			int i => i,
				long l => l,
				bool b => b ? 1L : 0L,
				double d => (long)d,
					string s => s.Length == 0 ? 0L : 0L,
					IntPtr ip => ip.ToInt64(),
					_ => 0L
		};

		/// <summary>Frees the native stub and drops references.</summary>
		public void Clear()
		{
			// Free the handle and context
			_selfHandle.Free();
			NativeThunkFactory.FreeThunk(Ptr);
			funcObj = null;
		}

		public void Dispose() => Clear();
	}

	/// <summary>
	/// Allocates and frees small 64-byte native thunks which insert a context pointer to the end of the argument list
	/// and then calls SharedTrampoline.
	///
	/// WARNING: THIS HAS BEEN TESTED ONLY ON 64-BIT WINDOWS
	/// </summary>
	static class NativeThunkFactory
	{
		public static unsafe IntPtr CreateThunk(IntPtr trampPtr, IntPtr ctx, int arity)
		{
			IntPtr mem = ExecutableMemoryPoolManager.Rent();
			byte* ptr = (byte*)mem;
			int disp;

			switch (RuntimeInformation.ProcessArchitecture)
			{
				case Architecture.X64:
#if WINDOWS
					if (arity < 4)
					{
						// opcode for MOV reg,imm64 depends on register:
						//    RCX=48 B9, RDX=48 BA, R8=49 B8, R9=49 B9
						switch (arity)
						{
							case 0: *ptr++ = 0x48; *ptr++ = 0xB9; break; // mov rcx

							case 1: *ptr++ = 0x48; *ptr++ = 0xBA; break; // mov rdx

							case 2: *ptr++ = 0x49; *ptr++ = 0xB8; break; // mov r8

							default: *ptr++ = 0x49; *ptr++ = 0xB9; break; // mov r9
						}

						*((ulong*)ptr) = (ulong)ctx.ToInt64(); ptr += 8;
					}
					else
					{
						disp = 8 * (arity + 1);
						// 1) mov rax, ctx
						*ptr++ = 0x48; *ptr++ = 0xB8;
						*((ulong*)ptr) = (ulong)ctx.ToInt64(); ptr += 8;
						// 2) mov qword ptr [rsp+disp], rax
						*ptr++ = 0x48; *ptr++ = 0x89; *ptr++ = 0x84; *ptr++ = 0x24;
						*((uint*)ptr) = (uint)disp; ptr += 4;
					}

					// mov rax, trampPtr
					*ptr++ = 0x48; *ptr++ = 0xB8;
					*((ulong*)ptr) = (ulong)trampPtr.ToInt64(); ptr += 8;
					// jmp rax
					*ptr++ = 0xFF; *ptr++ = 0xE0;
					break;
#else

					// On SystemV x86-64 the first six integer args go in
					//   RDI, RSI, RDX, RCX, R8, R9
					// beyond that they spill onto the stack (in the 128-byte red zone).
					if (arity < 6)
					{
						// MOV <reg>, imm64
						switch (arity)
						{
							case 0: // RDI ← ctx
								*ptr++ = 0x48; *ptr++ = 0xBF;
								break;

							case 1: // RSI ← ctx
								*ptr++ = 0x48; *ptr++ = 0xBE;
								break;

							case 2: // RDX ← ctx
								*ptr++ = 0x48; *ptr++ = 0xBA;
								break;

							case 3: // RCX ← ctx
								*ptr++ = 0x48; *ptr++ = 0xB9;
								break;

							case 4: // R8  ← ctx
								*ptr++ = 0x49; *ptr++ = 0xB8;
								break;

							default: // case 5: R9  ← ctx
								*ptr++ = 0x49; *ptr++ = 0xB9;
								break;
						}

						// imm64 for ctx
						*((ulong*)ptr) = (ulong)ctx.ToInt64();
						ptr += 8;
					}
					else
					{
						// spill into the red zone at [RSP + disp]
						// 7th argument slot lives at [RSP+8], 8th at [RSP+16], …
						disp = 8 * (arity + 1);
						// MOV RAX, ctx
						*ptr++ = 0x48; *ptr++ = 0xB8;
						*((ulong*)ptr) = (ulong)ctx.ToInt64();
						ptr += 8;
						// MOV [RSP+disp], RAX
						// opcode: 48 89 84 24 <disp:32>
						*ptr++ = 0x48;
						*ptr++ = 0x89;
						*ptr++ = 0x84;
						*ptr++ = 0x24;
						*((uint*)ptr) = (uint)disp;
						ptr += 4;
					}

					// Now tail-jump into the real function:
					// MOV RAX, trampPtr
					*ptr++ = 0x48; *ptr++ = 0xB8;
					*((ulong*)ptr) = (ulong)trampPtr.ToInt64();
					ptr += 8;
					// JMP RAX
					*ptr++ = 0xFF; *ptr++ = 0xE0;
#endif

				case Architecture.X86:
					// write ctx at [esp + 4*(arity+1)]
					disp = 4 * (arity + 1);
					// mov dword ptr [esp+disp], imm32
					*ptr++ = 0xC7;
					*ptr++ = 0x44;
					*ptr++ = 0x24;
					*ptr++ = (byte)disp;
					*((uint*)ptr) = (uint)ctx.ToInt32(); ptr += 4;
					// then jmp to the trampoline
					*ptr++ = 0xB8;                                   // mov eax, imm32
					*((uint*)ptr) = (uint)trampPtr.ToInt32(); ptr += 4;
					*ptr++ = 0xFF; *ptr++ = 0xE0;                   // jmp eax
					break;

				case Architecture.Arm64:
					if (arity < 8)
					{
						// Case A: extra arg fits in a register X<arity>
						// 1) LDR X<arity>, [PC, #imm0]   ; imm0 = 1 → literal at +12
						uint imm0 = 1;
						uint ldrCtxReg = 0x58000000u        // LDR literal opcode
										 | (imm0 << 5)        // imm19 = 1
										 | (uint)arity;       // Rt = arity
						*((uint*)ptr) = ldrCtxReg; ptr += 4;
						// 2) LDR X16, [PC, #imm1]        ; imm1 = 2 → literal at +20
						uint imm1 = 2;
						uint ldrTramp = 0x58000000u
										| (imm1 << 5)
										| 16u;               // Rt = 16 for X16
						*((uint*)ptr) = ldrTramp; ptr += 4;
						// 3) BR X16
						*((uint*)ptr) = 0xD61F0200; ptr += 4;
					}
					else
					{
						// Case B: extra arg must go on the stack
						// 1) Load ctx into X8
						//    LDR X8, [PC, #imm0] with imm0 = 1
						uint imm0 = 1;
						uint ldrCtxX8 = 0x58000000u
										| (imm0 << 5)
										| 8u;                // Rt = 8 for X8
						*((uint*)ptr) = ldrCtxX8; ptr += 4;
						// 2) SUB SP, SP, #16            ; carve out an 8-byte slot (aligned)
						*((uint*)ptr) = 0x910043FF; ptr += 4;  // encoding for SUB SP,SP,#16
						// 3) STR X8, [SP], #0           ; store ctx at top of stack
						//    (STR Xt, [SP, #imm]) imm=0
						*((uint*)ptr) = 0xF90003E8; ptr += 4;  // encoding for STR X8, [SP,#0]
						// 4) LDR X16, [PC, #imm1]       ; load trampPtr
						uint imm1 = 2;
						uint ldrTrampX16 = 0x58000000u
										   | (imm1 << 5)
										   | 16u;
						*((uint*)ptr) = ldrTrampX16; ptr += 4;
						// 5) BR X16
						*((uint*)ptr) = 0xD61F0200; ptr += 4;
					}

					// 6) Literal pool: ctx then trampPtr
					*((ulong*)ptr) = (ulong)ctx.ToInt64(); ptr += 8;
					*((ulong*)ptr) = (ulong)trampPtr.ToInt64(); ptr += 8;
					break;

				default:
					throw new PlatformNotSupportedException($"Architecture {RuntimeInformation.ProcessArchitecture} not supported");
			}

			return mem;
		}

		public static void FreeThunk(IntPtr ptr)
		{
			ExecutableMemoryPoolManager.Return(ptr);
		}
	}

	/// <summary>
	/// Manages executable memory in 512-byte pages, providing fixed 64-byte chunks for DelegateHolder.
	/// Automatically allocates new pages when needed and reuses freed chunks.
	/// This is needed because VirtualAlloc is quite a heavy function, best called as few times as possible.
	/// </summary>
	public sealed class ExecutableMemoryPoolManager
	{
		private const int PageSize = 512;
		private const int ChunkSize = 64;
		private static readonly object _lock = new object();

		// All allocated pages
		private static readonly List<IntPtr> _pages = new List<IntPtr>();
		// Free chunk addresses
		private static readonly Stack<IntPtr> _freeChunks = new Stack<IntPtr>();

		// Current page and offset
		private static IntPtr _currentPage;
		private static int _currentOffset;

#if LINUX || OSX
		[DllImport("libc", SetLastError = true)]
		private static extern IntPtr mmap(IntPtr addr, UIntPtr length, int prot, int flags, int fd, IntPtr offset);
		[DllImport("libc", SetLastError = true)]
		private static extern int munmap(IntPtr addr, UIntPtr length);

		private const int PROT_READ = 1;
		private const int PROT_WRITE = 2;
		private const int PROT_EXEC = 4;
		private const int MAP_PRIVATE = 2;
		private const int MAP_ANONYMOUS = 0x20;
#endif

		/// <summary>
		/// Rents a 32-byte executable chunk.
		/// </summary>
		public static IntPtr Rent()
		{
			lock (_lock)
			{
				if (_freeChunks.Count > 0)
					return _freeChunks.Pop();

				// Carve from current page
				if (_currentPage == IntPtr.Zero || _currentOffset + ChunkSize > PageSize)
				{
					var page = AllocatePage();
					_pages.Add(page);
					_currentPage = page;
					_currentOffset = 0;
				}

				var ptr = _currentPage + _currentOffset;
				_currentOffset += ChunkSize;
				return ptr;
			}
		}

		/// <summary>
		/// Returns a previously rented chunk for reuse.
		/// </summary>
		public static void Return(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero) return;

			lock (_lock)
			{
				_freeChunks.Push(ptr);
			}
		}

		/// <summary>
		/// Releases all allocated pages.
		/// </summary>
		public static void Dispose()
		{
			lock (_lock)
			{
#if WINDOWS

				foreach (var page in _pages)
					WindowsAPI.VirtualFree(page, UIntPtr.Zero, (uint)VirtualAllocExTypes.MEM_RELEASE);

#elif LINUX || OSX

				foreach (var page in _pages)
					munmap(page, (UIntPtr)PageSize);

#endif
				_pages.Clear();
				_freeChunks.Clear();
				_currentPage = IntPtr.Zero;
				_currentOffset = 0;
			}
		}

		private static IntPtr AllocatePage()
		{
#if WINDOWS
			var ptr = WindowsAPI.VirtualAlloc(IntPtr.Zero, (UIntPtr)PageSize, (uint)VirtualAllocExTypes.MEM_COMMIT, (uint)AccessProtectionFlags.PAGE_EXECUTE_READWRITE);

			if (ptr == IntPtr.Zero)
				throw new InvalidOperationException($"VirtualAlloc failed: {Marshal.GetLastWin32Error()}");

			return ptr;
#elif LINUX || OSX
			var ptr = mmap(IntPtr.Zero, (UIntPtr)PageSize, PROT_READ | PROT_WRITE | PROT_EXEC, MAP_PRIVATE | MAP_ANONYMOUS, -1, IntPtr.Zero);

			if (ptr == new IntPtr(-1))
				throw new InvalidOperationException("mmap failed");

			return ptr;
#else
#error Unsupported OS for AllocatePage
#endif
		}
	}
}
