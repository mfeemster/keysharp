#if WINDOWS
namespace Keysharp.Core.COM
{
	public unsafe class ComObject : KeysharpObject//ComValue
	{
		internal static long F_OWNVALUE = 1;
		internal static int MaxVtableLen = 16;
		internal List<IFuncObj> handlers = [];
		internal object item;

		public long Flags { get; set; }

		public object Item
		{
			get => Ptr;
			set => Ptr = value;
		}

		public object Ptr
		{
			get
			{
				if (item is IntPtr ip)
					return ip;//.ToInt64();
				else
					return item;
			}
			set
			{
				object temp = null;
				var longVal = 0L;
				var wasObj = false;

				if (value is IntPtr ip)
					longVal = ip.ToInt64();
				else if (value is long l)
					longVal = l;

				if ((VarType & Com.vt_byref) == Com.vt_byref)
				{
					item = longVal;
					return;
				}
				else
				{
					switch (VarType)
					{
						case Com.vt_empty://No value
							break;

						case Com.vt_null://SQL-style Null
							temp = null;
							break;

						case Com.vt_i2://16-bit signed int
						case Com.vt_ui2://16-bit unsigned int
							temp = longVal & 0xFFFF;
							break;

						case Com.vt_i4://32-bit signed int
						case Com.vt_r4://32-bit floating-point number
						case Com.vt_ui4://32-bit unsigned int
						case Com.vt_error://Error code(32-bit integer)
							temp = longVal & 0xFFFFFFFF;
							break;

						case Com.vt_r8://64-bit floating-point number
						case Com.vt_cy://Currency
						case Com.vt_i8://64-bit signed int
						case Com.vt_ui8://64-bit unsigned int
						case Com.vt_date://Date
						case Com.vt_int://Signed machine int
						case Com.vt_uint://Unsigned machine int
							temp = longVal;
							break;

						case Com.vt_bool://Boolean True(-1) or False(0)
							temp = value.Ab() ? -1L : 0L;//The true value for a variant is actually -1.
							break;

						case Com.vt_bstr://COM string (Unicode string with length prefix)
						case Com.vt_dispatch://COM object
						case Com.vt_variant://VARIANT(must be combined with VT_ARRAY or VT_BYREF)
						case Com.vt_unknown://IUnknown interface pointer
							wasObj = true;
							temp = longVal != 0L ? longVal : value;
							break;

						case Com.vt_decimal://(not supported)
							temp = longVal;
							break;

						case Com.vt_i1://8-bit signed int
						case Com.vt_ui1://8-bit unsigned int
							temp = longVal & 0x0F;
							break;

						case Com.vt_record://User-defined type -- NOT SUPPORTED
							break;

						case Com.vt_array://SAFEARRAY
							wasObj = true;
							temp = longVal;
							break;
							//case Com.vt_byref    ://Pointer to another type of value (0x4000)
							//  break;
					}
				}

				if (wasObj)
				{
					if (longVal != 0L)
					{
						temp = Marshal.GetObjectForIUnknown(new nint(longVal));
					}

					//else if (value is long l && l > 0)// && Marshal.IsComObject(value))
					//{
					//  try
					//  {
					//      temp = Marshal.GetObjectForIUnknown(new IntPtr(l));//This can just be a pointer to memory, in which case it'll throw.
					//  }
					//  catch (Exception)
					//  {
					//  }
					//}
					//else
					//  temp = value;
				}

				if (temp != null && Marshal.IsComObject(temp))
				{
					if (temp is IDispatch id)
					{
						item = id;
						return;
					}
				}

				item = temp;
			}

			/*
			    set
			    {
			    object temp;

			    if (value is IntPtr ip && ip != IntPtr.Zero)
			        temp = Marshal.GetObjectForIUnknown(ip);
			    else if (value is long l && l > 0)
			        temp = Marshal.GetObjectForIUnknown(new IntPtr(l));
			    else
			        temp = value;

			    if (temp != null && Marshal.IsComObject(temp))
			    {
			        if (temp is IDispatch id)
			            item = id;
			        else
			            item = temp;
			    }
			    else
			        item = value;
			    }
			*/
		}

		public new (Type, object) super => (typeof(KeysharpObject), this);

		public int VarType { get; set; }

		public ComObject(params object[] args) => _ = __New(args);

		internal ComObject(object varType, object value, object flags = null) => _ = __New(varType, value, flags);

		internal ComObject()
		{
		}

		~ComObject()
		{
			if (Marshal.IsComObject(Ptr))
				_ = Marshal.ReleaseComObject(Ptr);
			else if (Ptr is IntPtr ip)
				_ = Marshal.Release(ip);
		}

		public object __New(params object[] args)
		{
			var varType = args[0];
			var value = args[1];
			var flags = args.Length > 2 ? args[2] : null;
			var vt = (int)varType.Al();
			var co = ValueToVarType(value, vt, true);
			VarType = vt;
			Flags = flags != null ? flags.Al() : 0L;

			if (VarType == Com.vt_bstr && value is not long)
				Flags |= F_OWNVALUE;

			Ptr = co.Ptr;
			return "";
		}

		public void CallEvents()
		{
			var result = handlers?.InvokeEventHandlers(this);
		}

		public void Clear()
		{
			VarType = 0;
			Ptr = null;
			Flags = 0L;
		}

		internal static void ValueToVariant(object val, ComObject variant)
		{
			if (val is string s)
			{
				variant.VarType = Com.vt_bstr;
				variant.Ptr = s.Clone();
				return;
			}
			else if (val is long l)
			{
				variant.VarType = (l == (int)l) ? Com.vt_i4 : Com.vt_i8;
				variant.Ptr = l;
				return;
			}
			else if (val is int i)
			{
				variant.VarType = Com.vt_i4;
				variant.Ptr = i;
				return;
			}
			else if (val is double d)
			{
				variant.VarType = Com.vt_r8;
				variant.Ptr = d;
				return;
			}
			else if (val is IntPtr ptr)
			{
				variant.VarType = Com.vt_i8;
				variant.Ptr = ptr;
				return;
			}
			else if (val is ComObject co)
			{
				variant.VarType = co.VarType;
				variant.Ptr = co.Ptr;

				if (co.VarType == Com.vt_dispatch || co.VarType == Com.vt_unknown)
				{
					Com.ObjAddRef(co);
				}
				else if ((co.Flags & F_OWNVALUE) == F_OWNVALUE)
				{
					if ((variant.VarType & ~Com.vt_typemask) == Com.vt_array && co.Ptr is ComObjArray coa)
					{
						variant.Ptr = coa.array.Clone();//Copy array since both sides will call Destroy().
					}
					else if (variant.VarType == Com.vt_bstr)
					{
						variant.Ptr = co.Ptr.ToString().Clone();//Copy the string.
					}
				}

				return;
			}

			variant.VarType = Com.vt_dispatch;
			variant.Ptr = val is IDispatch id ? id : val;
		}

		internal static ComObject ValueToVarType(object val, int varType, bool callerIsComValue)
		{
			ComObject co;

			if (varType == Com.vt_variant)
			{
				co = new ComObject();//Use the empty constructors on purpose so they don't keep recursing into this method.
				ValueToVariant(val, co);
				return co;
			}

			if (varType == Com.vt_bool)
			{
				return new ComObject()
				{
					VarType = varType,
					//Ptr = val.Ab() ? -1L : 0L
					Ptr = val.Ab() ? -1 : 0
				};
			}

			if (val is IntPtr ptr)
				val = ptr.ToInt64();

			if (val is long l)
			{
				co = new ComObject
				{
					VarType = Com.vt_i8,
					Ptr = l
				};

				switch (varType)
				{
					//case Com.vt_r4:
					//case Com.vt_r8:
					//case Com.vt_date:
					//  break;
					case Com.vt_cy:
						co.Ptr = l * 10000L;
						return co;

					case Com.vt_bstr://These seem not to apply here because we already assigned l above.
					case Com.vt_dispatch:
					case Com.vt_unknown:
						if (callerIsComValue)
							return co;

						break;

					default:
						return co;
				}
			}
			else
				ValueToVariant(val, co = new ComObject());

			if (co.VarType != varType)//Attempt to coerce var to the correct type.
			{
				var origVal = co.Ptr;
				var newVal = co.Ptr;

				if (Com.VariantChangeTypeEx(out newVal, ref origVal, Thread.CurrentThread.CurrentCulture.LCID, 0, (short)varType) < 0)
				{
					co.Clear();
					return null;
				}

				co.Ptr = newVal;
				co.VarType = varType;
			}

			return co;
		}
	}

	internal delegate IntPtr Del0(IntPtr a);

	internal delegate IntPtr Del01(IntPtr a, IntPtr a0);

	internal delegate IntPtr Del02(IntPtr a, IntPtr a0, IntPtr a1);

	internal delegate IntPtr Del03(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2);

	internal delegate IntPtr Del04(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3);

	internal delegate IntPtr Del05(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4);

	internal delegate IntPtr Del06(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5);

	internal delegate IntPtr Del07(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6);

	internal delegate IntPtr Del08(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7);

	internal delegate IntPtr Del09(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8);

	internal delegate IntPtr Del10(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9);

	internal delegate IntPtr Del11(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10);

	internal delegate IntPtr Del12(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11);

	internal delegate IntPtr Del13(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12);

	internal delegate IntPtr Del14(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12, IntPtr a13);

	internal delegate IntPtr Del15(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12, IntPtr a13, IntPtr a14);

	internal delegate IntPtr Del16(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12, IntPtr a13, IntPtr a14, IntPtr a15);

	internal delegate IntPtr Del17(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12, IntPtr a13, IntPtr a14, IntPtr a15, IntPtr a16);

	internal delegate IntPtr Del18(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12, IntPtr a13, IntPtr a14, IntPtr a15, IntPtr a16, IntPtr a17);

	internal delegate IntPtr DelNone();
}

#endif