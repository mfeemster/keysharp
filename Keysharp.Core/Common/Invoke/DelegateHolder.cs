namespace Keysharp.Core.Common.Invoke
{
	public class DelegateHolder
	{
		internal PlaceholderFunction delRef;
		internal IFuncObj funcObj;
		//protected readonly ConcurrentStackArrayPool<IntPtr> paramsPool = new (31);
		private readonly bool fast;
		private readonly long paramCount = -1;

		public Delegate DelRef => delRef;

		internal bool Reference { get; }

		public DelegateHolder(object obj, bool f, bool r)
		{
			funcObj = Functions.GetFuncObj(obj, null, true);
            if (funcObj is FuncObj fo)
                paramCount = fo.MaxParams;
            fast = f;
			Reference = r;
			delRef = (PlaceholderFunction)Delegate.CreateDelegate(typeof(PlaceholderFunction), this, "DelegatePlaceholder");
		}

		public long DelegatePlaceholder(long p1 = 0L, long p2 = 0L, long p3 = 0L, long p4 = 0L, long p5 = 0L, long p6 = 0L, long p7 = 0L, long p8 = 0L,
										long p9 = 0L, long p10 = 0L, long p11 = 0L, long p12 = 0L, long p13 = 0L, long p14 = 0L, long p15 = 0L, long p16 = 0L,
										long p17 = 0L, long p18 = 0L, long p19 = 0L, long p20 = 0L, long p21 = 0L, long p22 = 0L, long p23 = 0L, long p24 = 0L,
										long p25 = 0L, long p26 = 0L, long p27 = 0L, long p28 = 0L, long p29 = 0L, long p30 = 0L, long p31 = 0L)
		{
			object val = null;
			long[] arr = new long[31];
            arr[0] = p1;
            arr[1] = p2;
            arr[2] = p3;
            arr[3] = p4;
            arr[4] = p5;
            arr[5] = p6;
            arr[6] = p7;
            arr[7] = p8;
            arr[8] = p9;
            arr[9] = p10;
            arr[10] = p11;
            arr[11] = p12;
            arr[12] = p13;
            arr[13] = p14;
            arr[14] = p15;
            arr[15] = p16;
            arr[16] = p17;
            arr[17] = p18;
            arr[18] = p19;
            arr[19] = p20;
            arr[20] = p21;
            arr[21] = p22;
            arr[22] = p23;
            arr[23] = p24;
            arr[24] = p25;
            arr[25] = p26;
            arr[26] = p27;
            arr[27] = p28;
            arr[28] = p29;
            arr[29] = p30;
            arr[30] = p31;

			System.Array.Resize(ref arr, (int)paramCount);


            if (delRef != null)
			{
				_ = Flow.TryCatch(() =>
				{
					(bool, ThreadVariables) btv = (false, null);

					if (!fast)
						btv = Threads.BeginThread();

					if (Reference)
					{
						val = DelegatePlaceholderArr(arr);
					}
					else
						val = funcObj.Call(System.Array.ConvertAll(arr, p => (object)p));

					if (!fast)
						_ = Threads.EndThread(btv.Item1);
				}, !fast);//Pop on exception because EndThread() above won't be called.
			}

			return ConvertResult(val);
		}

		public long DelegatePlaceholderArr(long[] arr)
		{
			object val = null;

			if (delRef != null)
			{
				if (Reference)
				{
					unsafe
					{
						fixed (long* pin = &arr[0])
						{
							var ptr = new IntPtr(pin);
							val = funcObj.Call(ptr.ToInt64());
						}
					}
				}
				else if (arr.Length >= 31)
				{
					val = delRef.Invoke(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5],
										arr[6], arr[7], arr[8], arr[9], arr[10], arr[11],
										arr[12], arr[13], arr[14], arr[15], arr[16], arr[17],
										arr[18], arr[19], arr[20], arr[21], arr[22], arr[23],
										arr[24], arr[25], arr[26], arr[27], arr[28], arr[29],
										arr[30]);
				}
				else
				{
					_ = Flow.TryCatch(() =>
					{
						(bool, ThreadVariables) btv = (false, null);

						if (!fast)
							btv = Threads.BeginThread();

						val = funcObj.Call(arr);

						if (!fast)
							_ = Threads.EndThread(btv.Item1);
					}, !fast);//Pop on exception because EndThread() above won't be called.
				}
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
			delRef = null;
			funcObj = null;
		}

		internal long DirectCall(params object[] parameters)
		{
			if (Reference)
			{
				var helper = new ComArgumentHelper(parameters);
				return DelegatePlaceholderArr(helper.args);
			}
			else
			{
				object val = null;
				_ = Flow.TryCatch(() =>
				{
					var helper = new DllArgumentHelper(parameters);
					(bool, ThreadVariables) btv = (false, null);

					if (!fast)
						btv = Threads.BeginThread();

					val = funcObj.Call(helper.args);

					if (!fast)
						_ = Threads.EndThread(btv.Item1);
				}, !fast);//Pop on exception because EndThread() above won't be called.
				return ConvertResult(val);
			}
		}

        public delegate long PlaceholderFunction(long p1 = 0L, long p2 = 0L, long p3 = 0L, long p4 = 0L, long p5 = 0L, long p6 = 0L, long p7 = 0L, long p8 = 0L,
                long p9 = 0L, long p10 = 0L, long p11 = 0L, long p12 = 0L, long p13 = 0L, long p14 = 0L, long p15 = 0L, long p16 = 0L,
                long p17 = 0L, long p18 = 0L, long p19 = 0L, long p20 = 0L, long p21 = 0L, long p22 = 0L, long p23 = 0L, long p24 = 0L,
                long p25 = 0L, long p26 = 0L, long p27 = 0L, long p28 = 0L, long p29 = 0L, long p30 = 0L, long p31 = 0L);
    }
}