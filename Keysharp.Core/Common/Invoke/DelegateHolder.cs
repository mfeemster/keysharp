namespace Keysharp.Core.Common.Invoke
{
	public class DelegateHolder
	{
		internal PlaceholderFunction delRef;
		internal IFuncObj funcObj;
		protected readonly ConcurrentStackArrayPool<IntPtr> paramsPool = new ConcurrentStackArrayPool<IntPtr>(31);
		private readonly bool fast;

		internal bool Reference { get; }

		public DelegateHolder(object obj, bool f, bool r)
		{
			funcObj = Functions.GetFuncObj(obj, null, true);
			fast = f;
			Reference = r;
			delRef = (PlaceholderFunction)Delegate.CreateDelegate(typeof(PlaceholderFunction), this, "DelegatePlaceholder");
		}

		public long DelegatePlaceholder(IntPtr p1 = new IntPtr(), IntPtr p2 = new IntPtr(), IntPtr p3 = new IntPtr(), IntPtr p4 = new IntPtr(), IntPtr p5 = new IntPtr(), IntPtr p6 = new IntPtr(), IntPtr p7 = new IntPtr(), IntPtr p8 = new IntPtr(),
		IntPtr p9 = new IntPtr(), IntPtr p10 = new IntPtr(), IntPtr p11 = new IntPtr(), IntPtr p12 = new IntPtr(), IntPtr p13 = new IntPtr(), IntPtr p14 = new IntPtr(), IntPtr p15 = new IntPtr(), IntPtr p16 = new IntPtr(),
		IntPtr p17 = new IntPtr(), IntPtr p18 = new IntPtr(), IntPtr p19 = new IntPtr(), IntPtr p20 = new IntPtr(), IntPtr p21 = new IntPtr(), IntPtr p22 = new IntPtr(), IntPtr p23 = new IntPtr(), IntPtr p24 = new IntPtr(),
		IntPtr p25 = new IntPtr(), IntPtr p26 = new IntPtr(), IntPtr p27 = new IntPtr(), IntPtr p28 = new IntPtr(), IntPtr p29 = new IntPtr(), IntPtr p30 = new IntPtr(), IntPtr p31 = new IntPtr())

		{
			object val = null;
			nint[] arr = null;

			if (delRef != null)
			{
				Flow.TryCatch(() =>
				{
					(bool, ThreadVariables) btv = (false, null);

					if (!fast)
						btv = Threads.BeginThread();

					if (Reference)
					{
						arr = paramsPool.Rent();
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
						val = DelegatePlaceholderArr(arr);
						paramsPool.Return(arr);
						arr = null;
					}
					else
						val = funcObj.Call(p1, p2, p3, p4, p5, p6, p7, p8,
										   p9, p10, p11, p12, p13, p14, p15, p16,
										   p17, p18, p19, p20, p21, p22, p23, p24,
										   p25, p26, p27, p28, p29, p30, p31);

					if (!fast)
						Threads.EndThread(btv.Item1);
				}, !fast);//Pop on exception because EndThread() above won't be called.
			}

			if (arr != null)
				_ = paramsPool.Return(arr);

			return ConvertResult(val);
		}

		public long DelegatePlaceholderArr(IntPtr[] arr)
		{
			object val = null;

			if (delRef != null)
			{
				if (Reference)
				{
					unsafe
					{
						fixed (IntPtr* pin = &arr[0])
						{
							var ptr = new IntPtr((long*)pin);
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
					Flow.TryCatch(() =>
					{
						(bool, ThreadVariables) btv = (false, null);

						if (!fast)
							btv = Threads.BeginThread();

						val = funcObj.Call(arr);

						if (!fast)
							Threads.EndThread(btv.Item1);
					}, !fast);//Pop on exception because EndThread() above won't be called.
				}
			}

			return ConvertResult(val);
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
				Flow.TryCatch(() =>
				{
					var helper = new DllArgumentHelper(parameters);
					(bool, ThreadVariables) btv = (false, null);

					if (!fast)
						btv = Threads.BeginThread();

					val = funcObj.Call(helper.args);

					if (!fast)
						Threads.EndThread(btv.Item1);
				}, !fast);//Pop on exception because EndThread() above won't be called.
				return ConvertResult(val);
			}
		}

		private static long ConvertResult(object val)
		{
			if (val is int i)
				return i;
			else if (val is long l)
				return (int)l;
			else if (val is bool b)
				return b ? 1 : 0;
			else if (val is double d)
				return (int)d;
			else if (val is string s && s.Length == 0)
				return 0L;
			else if (val is null)
				return 0L;

			return 0L;
		}

		internal void Clear()
		{
			delRef = null;
			funcObj = null;
		}

		public delegate long PlaceholderFunction(IntPtr p1 = new IntPtr(), IntPtr p2 = new IntPtr(), IntPtr p3 = new IntPtr(), IntPtr p4 = new IntPtr(), IntPtr p5 = new IntPtr(), IntPtr p6 = new IntPtr(), IntPtr p7 = new IntPtr(), IntPtr p8 = new IntPtr(),
		IntPtr p9 = new IntPtr(), IntPtr p10 = new IntPtr(), IntPtr p11 = new IntPtr(), IntPtr p12 = new IntPtr(), IntPtr p13 = new IntPtr(), IntPtr p14 = new IntPtr(), IntPtr p15 = new IntPtr(), IntPtr p16 = new IntPtr(),
		IntPtr p17 = new IntPtr(), IntPtr p18 = new IntPtr(), IntPtr p19 = new IntPtr(), IntPtr p20 = new IntPtr(), IntPtr p21 = new IntPtr(), IntPtr p22 = new IntPtr(), IntPtr p23 = new IntPtr(), IntPtr p24 = new IntPtr(),
		IntPtr p25 = new IntPtr(), IntPtr p26 = new IntPtr(), IntPtr p27 = new IntPtr(), IntPtr p28 = new IntPtr(), IntPtr p29 = new IntPtr(), IntPtr p30 = new IntPtr(), IntPtr p31 = new IntPtr());
	}
}