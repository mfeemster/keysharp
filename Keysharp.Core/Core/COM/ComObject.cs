#if WINDOWS
namespace Keysharp.Core.COM
{
	public unsafe class ComObject : KeysharpObject, IDisposable//ComValue
	{
		internal static readonly long F_OWNVALUE = 1;
		internal static readonly int MaxVtableLen = 16;
		internal List<IFuncObj> handlers = [];
		internal object item;
		private ComObject tempCo;//Must keep a reference else it will throw an exception about the RCW being separated from the object.

		public object Ptr
		{
			get => item;

			set
			{
				object temp = null;
				var longVal = 0L;
				var wasObj = false;

				if (value is long l)
					longVal = l;

				if ((VarType & VarEnum.VT_BYREF) == VarEnum.VT_BYREF)
				{
					item = longVal;
					return;
				}
				else
				{
					switch (VarType)
					{
						case VarEnum.VT_EMPTY://No value
							break;

						case VarEnum.VT_NULL://SQL-style Null
							temp = null;
							break;

						case VarEnum.VT_I2://16-bit signed int
						case VarEnum.VT_UI2://16-bit unsigned int
							temp = longVal & 0xFFFF;
							break;

						case VarEnum.VT_I4://32-bit signed int
						case VarEnum.VT_R4://32-bit floating-point number
						case VarEnum.VT_UI4://32-bit unsigned int
						case VarEnum.VT_ERROR://Error code(32-bit integer)
							temp = longVal & 0xFFFFFFFF;
							break;

						case VarEnum.VT_R8://64-bit floating-point number
						case VarEnum.VT_CY://Currency
						case VarEnum.VT_I8://64-bit signed int
						case VarEnum.VT_UI8://64-bit unsigned int
						case VarEnum.VT_DATE://Date
						case VarEnum.VT_INT://Signed machine int
						case VarEnum.VT_UINT://Unsigned machine int
							temp = longVal;
							break;

						case VarEnum.VT_BOOL://Boolean True(-1) or False(0)
							temp = value.Ab() ? -1L : 0L;//The true value for a variant is actually -1.
							break;

						case VarEnum.VT_BSTR://COM string (Unicode string with length prefix)
						case VarEnum.VT_DISPATCH://COM object
						case VarEnum.VT_VARIANT://VARIANT(must be combined with VT_ARRAY or VT_BYREF)
						case VarEnum.VT_UNKNOWN://IUnknown interface pointer
							wasObj = true;
							temp = longVal != 0L ? longVal : value;
							break;

						case VarEnum.VT_DECIMAL://(not supported)
							temp = longVal;
							break;

						case VarEnum.VT_I1://8-bit signed int
						case VarEnum.VT_UI1://8-bit unsigned int
							temp = longVal & 0x0F;
							break;

						case VarEnum.VT_RECORD://User-defined type -- NOT SUPPORTED
							break;

						case VarEnum.VT_ARRAY://SAFEARRAY
							wasObj = true;
							temp = longVal;
							break;
							//case VarEnum.VT_BYREF    ://Pointer to another type of value (0x4000)
							//  break;
					}
				}

				// It doesn't make sense to convert anything other than IDispatch, because
				// the resulting object couldn't be used as a "native object" anyway.
				if (wasObj && VarType == VarEnum.VT_DISPATCH)
				{
					if (longVal != 0L)
					{
						var nptr = new nint(longVal);
						temp = Marshal.GetObjectForIUnknown(nptr);
						Marshal.Release(nptr);
					}

					//else if (value is long l && l > 0)// && Marshal.IsComObject(value))
					//{
					//  try
					//  {
					//      temp = Marshal.GetObjectForIUnknown(new nint(l));//This can just be a pointer to memory, in which case it'll throw.
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

			    if (value is nint ip && ip != 0)
			        temp = Marshal.GetObjectForIUnknown(ip);
			    else if (value is long l && l > 0)
			        temp = Marshal.GetObjectForIUnknown(new nint(l));
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
		public VarEnum VarType { get; set; }
		internal long Flags { get; set; }

		public ComObject(params object[] args) : base(args) { }

		internal ComObject(object varType, object value, object flags = null) : base(varType, value, flags) { }

		public object __Delete()
		{
			Dispose();
			return null;
		}

		public override object __New(params object[] args)
		{
			if (args.Length == 0 || args[0] == null) return "";
			var varType = args[0];
			var value = args[1];
			var flags = args.Length > 2 ? args[2] : null;
			var vt = (VarEnum)varType.Al();
			var co = ValueToVarType(value, vt, true);
			VarType = vt;
			Flags = flags != null ? flags.Al() : 0L;

			if (VarType == VarEnum.VT_BSTR && value is not long)
				Flags |= F_OWNVALUE;

			Ptr = co.Ptr;
			tempCo = co;
			return "";
		}

		public object this[params object[] index] 
		{
			get
			{
				if (index.Length == 0 && (VarType & VarEnum.VT_BYREF) != 0)
				{
					return ReadVariant(Ptr.Al(), VarType);
				}
				return Script.Index(this, index);
			}
			set
			{
				if (index.Length == 0 && (VarType & VarEnum.VT_BYREF) != 0)
				{
					WriteVariant(Ptr.Al(), VarType, value);
				} 
				else
					Script.SetObject(value, this, index);
			}
		}

		public void Dispose()
		{
			if (Ptr == null)
				return;

			if (VarType == VarEnum.VT_UNKNOWN || VarType == VarEnum.VT_DISPATCH)
			{
				if (Ptr is long lp && lp != 0L)
					_ = Marshal.Release((nint)lp);
				else if (Marshal.IsComObject(Ptr))
					Marshal.ReleaseComObject(Ptr);
			}

			Ptr = null;
		}

		internal static object ReadVariant(long ptrValue, VarEnum vtRaw)
		{
			nint dataPtr = (nint)ptrValue;
			VarEnum vt = vtRaw & ~VarEnum.VT_BYREF;

			switch (vt)
			{
				// ── Integers → long ───────────────────────────────────────────────
				case VarEnum.VT_I1:
					return (long)(sbyte)Marshal.ReadByte(dataPtr);
				case VarEnum.VT_UI1:
					return (long)Marshal.ReadByte(dataPtr);
				case VarEnum.VT_I2:
					return (long)Marshal.ReadInt16(dataPtr);
				case VarEnum.VT_UI2:
					return (long)(ushort)Marshal.ReadInt16(dataPtr);
				case VarEnum.VT_I4:
				case VarEnum.VT_INT:
					return (long)Marshal.ReadInt32(dataPtr);
				case VarEnum.VT_UI4:
				case VarEnum.VT_UINT:
					return (long)(uint)Marshal.ReadInt32(dataPtr);
				case VarEnum.VT_I8:
					return Marshal.ReadInt64(dataPtr);
				case VarEnum.VT_UI8:
					return (long)(ulong)Marshal.ReadInt64(dataPtr);

				// ── Boolean → bool ───────────────────────────────────────────────
				case VarEnum.VT_BOOL:
					// COM VARIANT_BOOL is a 16-bit short: 0 or −1
					return Marshal.ReadInt16(dataPtr) != 0;

				// ── Floating point / date → double ───────────────────────────────
				case VarEnum.VT_R4:
					// Read 4-byte float, then promote
					float f = Marshal.PtrToStructure<float>(dataPtr);
					return (double)f;
				case VarEnum.VT_R8:
				case VarEnum.VT_DATE:
					// VT_DATE is also stored as an 8-byte IEEE double
					return Marshal.PtrToStructure<double>(dataPtr);

				// ── BSTR → string ────────────────────────────────────────────────
				case VarEnum.VT_BSTR:
					{
						nint bstr = Marshal.ReadIntPtr(dataPtr);
						return bstr == 0
							 ? string.Empty
							 : Marshal.PtrToStringBSTR(bstr);
					}

				default:
					{
						nint unk = Marshal.ReadIntPtr(dataPtr);
						if (unk == 0)
							return null;
						return new ComObject
						{
							VarType = vtRaw & ~VarEnum.VT_BYREF,
							Ptr = unk,
						};
					}
			}
		}

		/// <summary>
		/// Write a primitive value back into a COM VARIANT payload.
		/// Supports VT_I1/UI1/I2/UI2/I4/UI4/I8/UI8, VT_BOOL, VT_R4, VT_R8/VT_DATE.
		/// </summary>
		public static void WriteVariant(long ptrValue, VarEnum vtRaw, object value)
		{
			nint dataPtr = new nint(ptrValue);
			VarEnum vt = vtRaw & ~VarEnum.VT_BYREF;

			if (value is IPointable ip)
				value = ip.Ptr;

			switch (vt)
			{
				// ── Integers ────────────────────────────────────────────────────
				case VarEnum.VT_I1:
					// sbyte → marshal as one signed byte
					sbyte sb = Convert.ToSByte(value);
					Marshal.WriteByte(dataPtr, unchecked((byte)sb));
					break;

				case VarEnum.VT_UI1:
					// byte
					byte ub = Convert.ToByte(value);
					Marshal.WriteByte(dataPtr, ub);
					break;

				case VarEnum.VT_I2:
					short i2 = Convert.ToInt16(value);
					Marshal.WriteInt16(dataPtr, i2);
					break;

				case VarEnum.VT_UI2:
					ushort ui2 = Convert.ToUInt16(value);
					// Marshal.WriteInt16 writes a signed short, so reinterpret
					Marshal.WriteInt16(dataPtr, unchecked((short)ui2));
					break;

				case VarEnum.VT_I4:
				case VarEnum.VT_INT:
					int i4 = Convert.ToInt32(value);
					Marshal.WriteInt32(dataPtr, i4);
					break;

				case VarEnum.VT_UI4:
				case VarEnum.VT_UINT:
					uint ui4 = Convert.ToUInt32(value);
					Marshal.WriteInt32(dataPtr, unchecked((int)ui4));
					break;

				case VarEnum.VT_I8:
					long i8 = Convert.ToInt64(value);
					Marshal.WriteInt64(dataPtr, i8);
					break;

				case VarEnum.VT_UI8:
					ulong ui8 = Convert.ToUInt64(value);
					// Marshal.WriteInt64 writes signed; reinterpret
					Marshal.WriteInt64(dataPtr, unchecked((long)ui8));
					break;

				// ── Boolean ────────────────────────────────────────────────────
				case VarEnum.VT_BOOL:
					// VARIANT_BOOL is a 16-bit short: -1 (TRUE) or 0 (FALSE)
					bool b = Convert.ToBoolean(value);
					short vb = (short)(b ? -1 : 0);
					Marshal.WriteInt16(dataPtr, vb);
					break;

				// ── Floating-point / Date ─────────────────────────────────────
				case VarEnum.VT_R4:
					// store 4-byte float
					float f = Convert.ToSingle(value);
					byte[] fb = BitConverter.GetBytes(f);
					Marshal.Copy(fb, 0, dataPtr, fb.Length);
					break;

				case VarEnum.VT_R8:
				case VarEnum.VT_DATE:
					// store 8-byte double
					double d = Convert.ToDouble(value);
					byte[] db = BitConverter.GetBytes(d);
					Marshal.Copy(db, 0, dataPtr, db.Length);
					break;

				// ── BSTR → string ─────────────────────────────────────────────────
				case VarEnum.VT_BSTR:
					{
						// free old BSTR
						nint oldBstr = Marshal.ReadIntPtr(dataPtr);
						if (oldBstr != 0)
							WindowsAPI.SysFreeString(oldBstr);

						// allocate new BSTR (null → zero pointer)
						string s = value as string;
						IntPtr newBstr = string.IsNullOrEmpty(s)
							? IntPtr.Zero
							: Marshal.StringToBSTR(s);

						Marshal.WriteIntPtr(dataPtr, newBstr);
					}
					break;

				// ── COM interfaces → IUnknown pointer ─────────────────────────────
				case VarEnum.VT_DISPATCH:
				case VarEnum.VT_UNKNOWN:
					{
						// release old pointer
						nint oldPtr = Marshal.ReadIntPtr(dataPtr);
						if (oldPtr != 0)
							Marshal.Release(oldPtr);

						// get new pointer (allow passing either IntPtr or RCW)
						nint newPtr = value switch
						{
							long ptr => (nint)ptr,
							null => 0,
							_ => Marshal.GetIUnknownForObject(value)
						};

						Marshal.WriteIntPtr(dataPtr, newPtr);
					}
					break;

				// ── Unsupported ────────────────────────────────────────────────
				default:
					throw new NotSupportedException($"Writing VARTYPE {vt} is not supported.");
			}
		}

		internal static void ValueToVariant(object val, ComObject variant)
		{
			if (val is string s)
			{
				variant.VarType = VarEnum.VT_BSTR;
				variant.Ptr = s.Clone();
				return;
			}
			else if (val is long l)
			{
				variant.VarType = (l == (int)l) ? VarEnum.VT_I4 : VarEnum.VT_I8;
				variant.Ptr = l;
				return;
			}
			else if (val is int i)
			{
				variant.VarType = VarEnum.VT_I4;
				variant.Ptr = i;
				return;
			}
			else if (val is double d)
			{
				variant.VarType = VarEnum.VT_R8;
				variant.Ptr = d;
				return;
			}
			else if (val is ComObject co)
			{
				variant.VarType = co.VarType;
				variant.Ptr = co.Ptr;

				if (co.VarType == VarEnum.VT_DISPATCH || co.VarType == VarEnum.VT_UNKNOWN)
				{
					_ = Com.ObjAddRef(co);
				}
				else if ((co.Flags & F_OWNVALUE) == F_OWNVALUE)
				{
					if ((VarEnum)((int)variant.VarType & ~Com.vt_typemask) == VarEnum.VT_ARRAY && co.Ptr is ComObjArray coa)
					{
						variant.Ptr = coa.array.Clone();//Copy array since both sides will call Destroy().
					}
					else if (variant.VarType == VarEnum.VT_BSTR)
					{
						variant.Ptr = co.Ptr.ToString().Clone();//Copy the string.
					}
				}

				return;
			}

			variant.VarType = VarEnum.VT_DISPATCH;
			variant.Ptr = val is IDispatch id ? id : val;
		}

		internal static ComObject ValueToVarType(object val, VarEnum varType, bool callerIsComValue)
		{
			ComObject co;

			if ((varType & VarEnum.VT_BYREF) != 0)
			{
				return new ComObject()
				{
					VarType = varType,
					Ptr = val
				};
			}

			if (varType == VarEnum.VT_VARIANT)
			{
				co = new ComObject();//Use the empty constructors on purpose so they don't keep recursing into this method.
				ValueToVariant(val, co);
				return co;
			}

			if (varType == VarEnum.VT_BOOL)
			{
				return new ComObject()
				{
					VarType = varType,
					//Ptr = val.Ab() ? -1L : 0L
					Ptr = val.Ab() ? -1 : 0
				};
			}

			if (val is long l)
			{
				co = new ComObject
				{
					VarType = VarEnum.VT_I8,
					Ptr = l
				};

				switch (varType)
				{
					//case VarEnum.VT_R4:
					//case VarEnum.VT_R8:
					//case VarEnum.VT_DATE:
					//  break;
					case VarEnum.VT_CY:
						co.Ptr = l * 10000L;
						return co;

					case VarEnum.VT_BSTR://These seem not to apply here because we already assigned l above.
					case VarEnum.VT_DISPATCH:
					case VarEnum.VT_UNKNOWN:
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

		internal void CallEvents()
		{
			var result = handlers?.InvokeEventHandlers(this);
		}

		internal void Clear()
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
			else if (Ptr is nint ip)
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
	}
}

#endif