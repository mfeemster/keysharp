﻿namespace Keysharp.Core.Common.Invoke
{
	internal class DelegateData
	{
		// Precomputed trampolines for parameter counts 0..32
		internal readonly nint[] trampolinePtrs;

		internal DelegateData()
		{
			trampolinePtrs = Enumerable.Range(0, 33)
							 .Select(n =>
			{
				var mi = typeof(DelegateHolder)
						 .GetMethod($"SharedTrampoline{n}", BindingFlags.Static | BindingFlags.NonPublic);
				RuntimeHelpers.PrepareMethod(mi.MethodHandle);
				return mi.MethodHandle.GetFunctionPointer();
			}).ToArray();
		}
	}

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
		readonly nint _ctx;

		// Native function pointer to pass into unmanaged code.
		public long Ptr { get; }

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
			Ptr = NativeThunkFactory.CreateThunk(Script.TheScript.DelegateData.trampolinePtrs[_arity], _ctx, _arity);
		}

		// Shared unmanaged-callable trampolines
		[UnmanagedCallersOnly]
		private static long SharedTrampoline0(nint ctx)
		=> Dispatch(ctx, System.Array.Empty<long>());

		[UnmanagedCallersOnly]
		private static long SharedTrampoline1(long p0, nint ctx)
		=> Dispatch(ctx, new[] { p0 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline2(long p0, long p1, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline3(long p0, long p1, long p2, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline4(long p0, long p1, long p2, long p3, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline5(long p0, long p1, long p2, long p3, long p4, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline6(long p0, long p1, long p2, long p3, long p4, long p5, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline7(long p0, long p1, long p2, long p3, long p4, long p5, long p6, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline8(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline9(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline10(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline11(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline12(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline13(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline14(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline15(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline16(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline17(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline18(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline19(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline20(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline21(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, nint ctx)
		=> Dispatch(ctx, new[] {p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline22(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline23(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22
							   });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline24(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline25(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline26(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline27(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline28(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, long p27, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26, p27 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline29(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, long p27, long p28, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26, p27, p28 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline30(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, long p27, long p28, long p29, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26, p27, p28, p29 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline31(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, long p27, long p28, long p29, long p30, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26, p27, p28, p29, p30 });

		[UnmanagedCallersOnly]
		private static long SharedTrampoline32(long p0, long p1, long p2, long p3, long p4, long p5, long p6, long p7, long p8, long p9, long p10, long p11, long p12, long p13, long p14, long p15, long p16, long p17, long p18, long p19, long p20, long p21, long p22, long p23, long p24, long p25, long p26, long p27, long p28, long p29, long p30, long p31, nint ctx)
		=> Dispatch(ctx, new[] { p0, p1, p2, p3, p4,  p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26, p27, p28, p29, p30, p31 });

		/// <summary>
		/// SharedTrampoline calls this function with the arguments packed into an array.
		/// We then create a new green thread an run the target function there with the received arguments.
		/// </summary>
		/// <param name="ctx">Pointer to this DelegateHolder instance.</param>
		/// <param name="args">Argument list for the target function.</param>
		/// <returns>Result of the target function converted to a long.</returns>
		private static long Dispatch(nint ctx, long[] args)
		{
			// Recover the DelegateHolder by reversing the GCHandle
			var handle = GCHandle.FromIntPtr(ctx);

			if (!(handle.Target is DelegateHolder holder))
				return (long)Errors.ErrorOccurred($"Invalid DelegateHolder pointer passed to Dispatch.", DefaultErrorLong);

			object val = null;
			var state = (false, (ThreadVariables)null);
			_ = Flow.TryCatch(() =>
			{
				var script = Script.TheScript;

				if (!holder._fast)
					state = script.Threads.BeginThread();

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

				if (state.Item1)
					_ = script.Threads.EndThread(state);
			}, !state.Item1, state);
			return ConvertResult(val);
		}

		internal static long ConvertResult(object val) => val switch
	{
			long l => l,
				 bool b => b ? 1L : 0L,
				 double d => (long)d,
					 string s => s.Length == 0 ? 0L : 0L,
					 _ => 0L
		};

		/// <summary>Frees the native stub and drops references.</summary>
		public void Clear()
		{
			// Free the handle and context
			_selfHandle.Free();
			NativeThunkFactory.FreeThunk((nint)Ptr);
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
		public static unsafe nint CreateThunk(nint trampPtr, nint ctx, int arity)
		{
			nint mem = Script.TheScript.ExecutableMemoryPoolManager.Rent();
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
					break;
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

		public static void FreeThunk(nint ptr)
		{
			Script.TheScript.ExecutableMemoryPoolManager.Return(ptr);
		}
	}

	/// <summary>
	/// Manages executable memory in 512-byte pages, providing fixed 64-byte chunks for DelegateHolder.
	/// Automatically allocates new pages when needed and reuses freed chunks.
	/// This is needed because VirtualAlloc is quite a heavy function, best called as few times as possible.
	///
	/// The implementation uses a Treiber stack to keep it mostly lock-free.
	/// </summary>
	public sealed class ExecutableMemoryPoolManager
	{
		private const int PageSize = 512;
		private const int ChunkSize = 64;
		private readonly Lock _lock = new ();

		// Treiber‑stack head of free chunks (0 == empty)
		private nint _freeList;

		// All allocated pages
		private readonly List<nint> _pages = new List<nint>();

		// Current page and offset
		private nint _currentPage;
		private int _currentOffset = 0;

#if LINUX || OSX
		[DllImport("libc", SetLastError = true)]
		private static extern nint mmap(nint addr, nint length, int prot, int flags, int fd, nint offset);
		[DllImport("libc", SetLastError = true)]
		private static extern int munmap(nint addr, nint length);

		private const int PROT_READ = 1;
		private const int PROT_WRITE = 2;
		private const int PROT_EXEC = 4;
		private const int MAP_PRIVATE = 2;
		private const int MAP_ANONYMOUS = 0x20;
#endif

		public ExecutableMemoryPoolManager()
		{
			lock (_lock)
			{
				// eagerly allocate first page so _currentPage != 0
				var page = AllocatePage();
				Volatile.Write(ref _currentPage, page);
				// leave _currentOffset == 0
				_pages.Add(page);
			}
		}

		/// <summary>
		/// Rents a 32-byte executable chunk.
		/// </summary>
		public nint Rent()
		{
			// 1) Try lock‑free pop from the free list
			if (TryPopFree(out var recycled))
				return recycled;

			// 2) Fast bump‑pointer carve
			FastCarve:

			while (true)
			{
				// use CAS to bump _currentOffset, and we use the old offset as our memory
				int oldOffset = Volatile.Read(ref _currentOffset);
				int newOffset = oldOffset + ChunkSize;

				if (newOffset <= PageSize) // this means we wouldn't fit inside the current page
				{
					// try to claim [oldOffset, newOffset)
					if (Interlocked.CompareExchange(ref _currentOffset, newOffset, oldOffset) == oldOffset)
					{
						var page = Volatile.Read(ref _currentPage);
						return page + oldOffset;
					}

					// else another thread won the CAS — retry
					continue;
				}

				// fall through to slow‑path to grab lock and allocate a fresh page
				break;
			}

			// 3) Slow path: need a new page
			lock (_lock)
			{
				// it's possible another thread already replaced the page,
				// so check one more time if there's room on the current page:
				if (_currentOffset + ChunkSize <= PageSize)
				{
					goto FastCarve;
				}

				// allocate brand‑new page, reset offset
				var page = AllocatePage();
				Volatile.Write(ref _currentPage, page);
				Volatile.Write(ref _currentOffset, ChunkSize);
				_pages.Add(page);
				// return the first chunk
				return page;
			}
		}

		private bool TryPopFree(out nint ptr)
		{
			while (true)
			{
				var head = Volatile.Read(ref _freeList);

				if (head == 0)
				{
					// no reusable chunks available
					ptr = 0;
					return false;
				}

				// read the next pointer stored at address head
				var next = Marshal.ReadIntPtr(head);

				// try to swing _freeList from head → next
				if (Interlocked.CompareExchange(ref _freeList, next, head) == head)
				{
					ptr = head;
					return true;
				}

				// else retry
			}
		}

		private void PushFree(nint ptr)
		{
			while (true)
			{
				var head = Volatile.Read(ref _freeList);
				// write the old head into the first pointer‑sized bytes of the freed chunk,
				// using the chunk as a node in the Treiber stack
				Marshal.WriteIntPtr(ptr, head);

				// try to swing _freeList from head → ptr
				if (Interlocked.CompareExchange(ref _freeList, ptr, head) == head)
					return;

				// else another thread modified it — retry
			}
		}


		public void Return(nint ptr)
		{
			if (ptr == 0) return;

			PushFree(ptr);
		}

		/// <summary>
		/// Releases all allocated pages.
		/// </summary>
		public void Dispose()
		{
			lock (_lock)
			{
#if WINDOWS

				foreach (var page in _pages)
					WindowsAPI.VirtualFree(page, 0, (uint)VirtualAllocExTypes.MEM_RELEASE);

#elif LINUX || OSX

				foreach (var page in _pages)
					munmap(page, (nint)PageSize);

#endif
				_pages.Clear();
				_currentPage = 0;
				_currentOffset = 0;
			}
		}

		private nint AllocatePage()
		{
#if WINDOWS
			var ptr = WindowsAPI.VirtualAlloc(0, (nint)PageSize, (uint)VirtualAllocExTypes.MEM_COMMIT, (uint)AccessProtectionFlags.PAGE_EXECUTE_READWRITE);
			return ptr == 0 ? throw new InvalidOperationException($"VirtualAlloc failed: {Marshal.GetLastWin32Error()}") : ptr;
#elif LINUX || OSX
			var ptr = mmap(0, (nint)PageSize, PROT_READ | PROT_WRITE | PROT_EXEC, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);

			if (ptr == new nint(-1))
				throw new InvalidOperationException("mmap failed");

			return ptr;
#else
#error Unsupported OS for AllocatePage
			return 0;
#endif
		}
	}
}
