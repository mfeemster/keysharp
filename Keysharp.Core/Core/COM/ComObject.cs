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

				if ((vt & VarEnum.VT_BYREF) == VarEnum.VT_BYREF
						|| (vt & VarEnum.VT_ARRAY) == VarEnum.VT_ARRAY)
				{
					item = longVal;
					return;
				}
				else
				{
					switch (vt)
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
							temp = longVal != 0L ? longVal : value;
							break;
							//case VarEnum.VT_BYREF    ://Pointer to another type of value (0x4000)
							//  break;
					}
				}

				// It doesn't make sense to convert anything other than IDispatch, because
				// the resulting object couldn't be used as a "native object" anyway.
				if (wasObj && vt == VarEnum.VT_DISPATCH)
				{
					if (longVal != 0L)
					{
						var nptr = new nint(longVal);
						temp = Marshal.GetObjectForIUnknown(nptr);
						_ = Marshal.Release(nptr);
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
						item = new nint(longVal != 0 ? longVal : Marshal.GetIUnknownForObject(temp));//This was put here to prevent the COM tests with the taskbar in guitest.ks from crashing. Unsure if it actually makes sense.

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

		public new (Type, object) super => (typeof(KeysharpObject), this);

		public VarEnum vt;
		public long VarType
		{
			get => (long)vt;
			set => vt = (VarEnum)value.Ai();
		}
		internal long Flags { get; set; }

		public ComObject(params object[] args) => _ = __New(args);

		internal ComObject(object varType, object value, object flags = null) => _ = __New(varType, value, flags);

		internal ComObject()
		{
		}

		~ComObject()
		{
			Dispose();
		}

		public object __Delete()
		{
			Dispose();
			return DefaultObject;
		}

		public new object __New(params object[] args)
		{
			var varType = args[0];
			var value = args[1];
			var flags = args.Length > 2 ? args[2] : null;
			var vt = (VarEnum)varType.Al();
			var co = ValueToVarType(value, vt, true);
			this.vt = vt;
			Flags = flags != null ? flags.Al() : 0L;

			if (this.vt == VarEnum.VT_BSTR && value is not long)
				Flags |= F_OWNVALUE;

			Ptr = co.Ptr;
			tempCo = co;
			return DefaultObject;
		}

		public virtual void Dispose()
		{
			if (Ptr == null)
				return;

			if (vt == VarEnum.VT_UNKNOWN || vt == VarEnum.VT_DISPATCH)
			{
				if (Ptr is long lp && lp != 0L)
					_ = Marshal.Release((nint)lp);
				else if (Marshal.IsComObject(Ptr))
					_ = Marshal.ReleaseComObject(Ptr);
			}
			else if (vt == VarEnum.VT_BSTR && (Flags & F_OWNVALUE) != 0 && Ptr is long)
			{
				WindowsAPI.SysFreeString((nint)Ptr);
			}

			Ptr = null;
		}

		internal static object ReadVariant(long ptrValue, VarEnum vtRaw)
		{
			nint dataPtr = (nint)ptrValue;
			VarEnum vt = vtRaw & ~VarEnum.VT_BYREF;

			if ((vt & VarEnum.VT_ARRAY) != 0)
			{
				VarEnum elemVt = vt & ~VarEnum.VT_ARRAY;
				nint parray = Marshal.ReadIntPtr(dataPtr);
				if (parray == 0)
					return DefaultErrorObject;

				// Wrap without owning: the VARIANT will destroy it on VariantClear.
				return new ComObjArray(elemVt, parray, takeOwnership: false);
			}

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

				case VarEnum.VT_VARIANT:
				{
					VarEnum innerVt = (VarEnum)Marshal.ReadInt16(dataPtr);
					return ReadVariant(ptrValue + 8, innerVt);
				}

				case VarEnum.VT_UNKNOWN:
				case VarEnum.VT_DISPATCH:
				{
					nint punk = Marshal.ReadIntPtr(dataPtr);
					return Objects.ObjFromPtr(punk);
				}

				default:
				{
					nint unk = Marshal.ReadIntPtr(dataPtr);

					if (unk == 0)
						return DefaultErrorObject;

					return new ComObject
					{
						vt = vtRaw & ~VarEnum.VT_BYREF,
						Ptr = unk,
					};
				}
			}
		}

		/// <summary>
		/// Write a primitive value back into a COM VARIANT payload.
		/// Supports VT_I1/UI1/I2/UI2/I4/UI4/I8/UI8, VT_BOOL, VT_R4, VT_R8/VT_DATE, VT_VARIANT.
		/// </summary>
		internal static void WriteVariant(long ptrValue, VarEnum vtRaw, object value)
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
						_ = Marshal.Release(oldPtr);

					// get new pointer (allow passing either IntPtr or RCW)

					nint newPtr = value switch
				{
						long ptr => (nint)ptr,
							null => 0,
							_ => vt == VarEnum.VT_DISPATCH ? Marshal.GetIDispatchForObject(value) : Marshal.GetIUnknownForObject(value)
					};

					Marshal.WriteIntPtr(dataPtr, newPtr);
				}
				break;

				case VarEnum.VT_VARIANT:
				{
					// 1) Choose the right VarEnum for "value"
					VarEnum innerVt;

					if (value is string)
					{
						innerVt = VarEnum.VT_BSTR;
					}
					else if (value is KeysharpObject)
					{
						innerVt = VarEnum.VT_DISPATCH;
					}
					else if (value is ComObjArray coa)
					{
						innerVt = VarEnum.VT_ARRAY | coa._baseType;
					}
					else if (value is double)
					{
						innerVt = VarEnum.VT_R8;
					}
					else if (value is long l)
					{
						innerVt = (l >= int.MinValue && l <= int.MaxValue)
								  ? VarEnum.VT_I4
								  : VarEnum.VT_I8;
					}
					else
					{
						throw new NotSupportedException(
							$"Cannot wrap a {value?.GetType().Name} as VT_VARIANT");
					}

					// 2) Clear previous contents, release BSTRs etc
					_ = VariantHelper.VariantClear(dataPtr);
					// 3) Write the VT and clear the four reserved words
					//    [vt:2][res1:2][res2:2][res3:2]  <-- totals 8 bytes header
					Marshal.WriteInt16(dataPtr, (short)innerVt);
					// 4) Write the payload into the union at offset 8
					//    we simply recurse into our existing writer,
					//    passing the address + 8 and the bare innerVt
					WriteVariant(ptrValue + 8, innerVt, value);
				}
				break;

				// ── SAFEARRAYs (VT_ARRAY | T) ─────────────────────────────────────────────
				case var _ when (vt & VarEnum.VT_ARRAY) != 0:
				{
					// vt holds VT_ARRAY|elemVT, payload is a SAFEARRAY*
					VarEnum elemVt = vt & ~VarEnum.VT_ARRAY;

					nint psaToStore = 0;

					if (value is ComObjArray coa)
					{
						// Defensive: if element type differs, still allow but clone regardless.
						// Clone so that the VARIANT owns its *own* SAFEARRAY (avoids double-destroy).
						int hr = OleAuto.SafeArrayCopy(coa._psa, out nint psaCopy);
						if (hr < 0)
							throw Errors.OSError("SafeArrayCopy failed.", hr);

						psaToStore = psaCopy;
					}
					else if (value is long lp)
					{
						psaToStore = (nint)lp; // caller gave us a raw SAFEARRAY*
					}
					else if (value is nint ip2)
					{
						psaToStore = ip2;
					}
					else
					{
						throw Errors.Error($"Cannot write VT_ARRAY payload from {value?.GetType().Name}.");
					}

					Marshal.WriteIntPtr(dataPtr, psaToStore);
					break;
				}
				// ── Unsupported ────────────────────────────────────────────────
				default:
					throw Errors.Error($"Writing VARTYPE {vt} is not supported.");
			}
		}

		internal static void ValueToVariant(object val, ComObject variant)
		{
			if (val is string s)
			{
				variant.vt = VarEnum.VT_BSTR;
				variant.Ptr = s.Clone();
				return;
			}
			else if (val is long l)
			{
				variant.vt = (l == (int)l) ? VarEnum.VT_I4 : VarEnum.VT_I8;
				variant.Ptr = l;
				return;
			}
			else if (val is int i)
			{
				variant.vt = VarEnum.VT_I4;
				variant.Ptr = i;
				return;
			}
			else if (val is double d)
			{
				variant.vt = VarEnum.VT_R8;
				variant.Ptr = d;
				return;
			}
			else if (val is ComObject co)
			{
				variant.vt = co.vt;
				variant.Ptr = co.Ptr;

				if (co.vt == VarEnum.VT_DISPATCH || co.vt == VarEnum.VT_UNKNOWN)
				{
					_ = Com.ObjAddRef(co);
				}
				else if ((co.Flags & F_OWNVALUE) == F_OWNVALUE)
				{
					if ((VarEnum)((int)variant.vt & ~Com.variantTypeMask) == VarEnum.VT_ARRAY && co.Ptr is ComObjArray coa)
					{
						variant.Ptr = coa.Clone();//Copy array since both sides will call Destroy().
					}
					else if (variant.vt == VarEnum.VT_BSTR)
					{
						variant.Ptr = co.Ptr.ToString().Clone();//Copy the string.
					}
				}

				return;
			}
			else if (Marshal.IsComObject(val))
			{
				if (val is IDispatch idisp)
				{
					variant.vt = VarEnum.VT_DISPATCH;
					variant.Ptr = idisp;
				}
				else
				{
					variant.vt = VarEnum.VT_UNKNOWN;
					variant.Ptr = val;
				}

				return;
			}

			variant.vt = VarEnum.VT_DISPATCH;
			variant.Ptr = val;
		}

		internal static ComObject ValueToVarType(object val, VarEnum varType, bool callerIsComValue)
		{
			ComObject co;

			if ((varType & VarEnum.VT_BYREF) != 0)
			{
				return new ComObject()
				{
					vt = varType,
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
					vt = varType,
					//Ptr = val.Ab() ? -1L : 0L
					Ptr = val.Ab() ? -1 : 0
				};
			}

			if (varType == VarEnum.VT_ARRAY && val is ComObjArray coa )
			{
				return coa;//Don't do anything with it, it's already in the correct form.
			}

			if (val is long l)
			{
				co = new ComObject
				{
					vt = VarEnum.VT_I8,
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

			if (co.vt != varType)//Attempt to coerce var to the correct type.
			{
				var origVal = co.Ptr;
				var newVal = co.Ptr;

				if (Com.VariantChangeTypeEx(out newVal, ref origVal, Thread.CurrentThread.CurrentCulture.LCID, 0, (short)varType) < 0)
				{
					co.Clear();
					return null;
				}

				co.Ptr = newVal;
				co.vt = varType;
			}

			return co;
		}

		internal void CallEvents()
		{
			var result = handlers?.InvokeEventHandlers(this);
		}

		internal void Clear()
		{
			vt = 0;
			Ptr = null;
			Flags = 0L;
		}

		internal VARIANT ToVariant()
		{
			var v = new VARIANT()
			{
				vt = (ushort)vt
			};

			if (Ptr is long l)//Put most common first.
				v.data.llVal = l;
			else if (Ptr is double d)
				v.data.dblVal = d;
			else if (Ptr is string str)
				v.data.bstrVal = Marshal.StringToBSTR(str);
			else if (Ptr is nint ip)
			{
				if ((vt & VarEnum.VT_ARRAY) != 0)
					v.data.parray = ip;     // SAFEARRAY*
				else if (vt == VarEnum.VT_DISPATCH || vt == VarEnum.VT_UNKNOWN)
					v.data.pdispVal = ip;   // IDispatch*/IUnknown*
				else
					v.data.pdispVal = ip;   // fallback
			}
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