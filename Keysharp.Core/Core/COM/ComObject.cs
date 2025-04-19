#if WINDOWS
namespace Keysharp.Core.COM
{
	public unsafe class ComObject : KeysharpObject//ComValue
	{
		internal static long F_OWNVALUE = 1;
		internal static int MaxVtableLen = 16;
		internal List<IFuncObj> handlers = [];
		internal object item;
		private ComObject tempCo;//Must keep a reference else it will throw an exception about the RCW being separated from the object.

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

				// It doesn't make sense to convert anything other than IDispatch, because
				// the resulting object couldn't be used as a "native object" anyway.
				if (wasObj && VarType == Com.vt_dispatch)
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
						item = id;
					else
						item = new nint(longVal);//This was put here to prevent the COM tests with the taskbar in guitest.ks from crashing. Unsure if it actually makes sense.

					return;
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
			if (Ptr == null)
				return;
			if (VarType == Com.vt_unknown || VarType == Com.vt_dispatch) {
				if (Ptr is IntPtr ip)
					_ = Marshal.Release(ip);
				else if (Ptr is long lp)
					_ = Marshal.Release((nint)lp);
			}
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
			tempCo = co;
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

		internal VARIANT ToVariant()
		{
			var v = new VARIANT()
			{
				vt = (ushort)VarType
			};

			if (Ptr is long l)//Put most common first.
				v.data.llVal = l;
			else if (Ptr is double d)
				v.data.dblVal = d;
			else if (Ptr is string str)
				v.data.bstrVal = Marshal.StringToBSTR(str);
			else if (Ptr is IntPtr ip)
				v.data.pdispVal = ip;//Works for COM interfaces, safearray and other pointer types.
			else if (Ptr is int i)
				v.data.lVal = i;
			else if (Ptr is uint ui)
				v.data.ulVal = ui;
			else if (Ptr is ulong ul)
				v.data.ullVal = ul;
			else if (Ptr is float f)
				v.data.fltVal = f;
			else if (Ptr is byte b)
				v.data.bVal = b;
			else if (Ptr is sbyte sb)
				v.data.cVal = sb;
			else if (Ptr is short s)
				v.data.iVal = s;
			else if (Ptr is ushort us)
				v.data.uiVal = us;
			else if (Ptr is bool bl)
				v.data.boolVal = (short)(bl ? -1 : 0);

			return v;
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
					_ = Com.ObjAddRef(co);
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
}

#endif