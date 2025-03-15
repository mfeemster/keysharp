#if WINDOWS
using System.Runtime.InteropServices.ComTypes;
using ct = System.Runtime.InteropServices.ComTypes;

namespace Keysharp.Core.COM
{
	unsafe public class ComMethodPropertyHolder : MethodPropertyHolder
	{
		public string Name { get; private set; }

		public ComMethodPropertyHolder(string name)
			: base(null, null)
		{
			Name = name;
			callFunc = (inst, obj) =>
			{
				var t = inst.GetType();
				var args = new object[obj.Length];
				//var bstrs = new List<IntPtr>();
				//HashSet<GCHandle> gcHandles = [];

				for (var i = 0; i < obj.Length; ++i)
				{
					var o = obj[i];
					//
					//This is working code for most cases.
					//
					var co = o as ComObject;
					var p = co != null ? co.Ptr : o;

					if (p != null)
					{
						if (p is long l)
							args[i] = new nint(l);
						else
							args[i] = p;
					}

					//
					// Below this lines is various bits of test code, none of which worked.
					//
					/*
					    if (o is ComObject co)
					    {
					    var v = co.ToVariant();
					    var gch = GCHandle.Alloc(v, GCHandleType.Pinned);
					    _ = gcHandles.Add(gch);
					    var intptr = gch.AddrOfPinnedObject();
					    args[i] = intptr;
					    //Marshal.getobject
					    //args[i] = v;

					    //var intp = (int*)v.data.pdispVal.ToPointer();
					    //args[i] = intp;
					    if (co.Ptr is string s)//If it was a string, it will have been converted to bstr.
					        bstrs.Add(v.data.bstrVal);
					    }
					    else if (o is long l)
					    args[i] = new nint(l);
					    else
					    args[i] = o;
					*/
					//var co = o as ComObject;
					//var p = co != null ? co.Ptr : o;
					/*
					    if (p != null)
					    {
					    if (p is long l)
					    {
					        //args[i] = (object)0;
					        args[i] = new nint(l);
					        Marshal.WriteInt64((nint)args[i], long.MaxValue);
					        var readback = Marshal.ReadInt64((nint)args[i]);
					        //Marshal.WriteInt16(args[i], 0, (short)co.VarType); // vt = 3 (VT_I4)
					        //Marshal.WriteInt16(args[i], 0, 3); // vt = 3 (VT_I4)
					        //Marshal.WriteInt32(args[i], 8, 0); // intVal = 0 (CHILDID_SELF)
					    }
					    else
					    {
					        //test only
					        IntPtr variantPtr = Marshal.AllocCoTaskMem(16); // VARIANT is 16 bytes
					        Marshal.WriteInt16(variantPtr, 0, 3); // vt = 3 (VT_I4)
					        Marshal.WriteInt16(variantPtr, 2, 0);
					        Marshal.WriteInt16(variantPtr, 4, 0);
					        Marshal.WriteInt16(variantPtr, 6, 0);
					        //Marshal.WriteInt32(variantPtr, 8, 0); // intVal = 0 (CHILDID_SELF)
					        Marshal.WriteInt64(variantPtr, 8, 0L); // intVal = 0 (CHILDID_SELF)
					        args[i] = variantPtr;
					        //var firsttwo = Marshal.ReadInt16((nint)args[i]);
					        //actual code here
					        //args[i] = p;
					    }
					    }*/
				}

				//This appears to work sometimes, but does not always populate reference parameters.
				//Unsure how to know whether a parameter is a ref if the type is never specified.
				var ret = inst.GetType().InvokeMember(Name, BindingFlags.InvokeMethod, null, inst, args);
				//test code
				/*
				                foreach (var bstr in bstrs)
				                    Marshal.FreeBSTR(bstr);

				                foreach (var gch in gcHandles)
				                    gch.Free();
				*/
				//Marshal.FreeCoTaskMem((IntPtr)args.Last());
				//
				//for (var i = 0; i < obj.Length; ++i)
				//{
				//  var o = obj[i];
				//  var a = args[i];
				//  if (o is ComObject co)
				//  {
				//      if ((co.VarType & Com.vt_byref) == Com.vt_byref)
				//      {
				//          if (co.Ptr is long && a is IntPtr ip)
				//              co.Ptr = ip.ToInt64();
				//          else if (co.Ptr is IntPtr && a is long l)
				//              co.Ptr = new nint(l);
				//          else
				//              co.Ptr = a;
				//      }
				//  }
				//}
				return ret;
			};
		}


		/// <summary>
		/// Dynamically invokes a COM method by querying all available type-info interfaces on the COM object.
		/// It locates an ITypeInfo that defines the method, retrieves the FUNCDESC to determine expected parameter
		/// types and by-reference modifiers, converts input parameters accordingly, and then uses InvokeMember().
		/// </summary>
		/// <param name="comObject">The COM object implementing IDispatch.</param>
		/// <param name="methodName">The method name to invoke.</param>
		/// <param name="inputParameters">An array of input values (for in/out parameters).</param>
		/// <returns>The updated parameters array (including out/ref values) after invocation.</returns>
		private static object[] InvokeComMethodWithTypeInfo(object comObject, string methodName, object[] inputParameters)
		{
			// Get the IDispatch pointer.
			IntPtr pDispatch = Marshal.GetIDispatchForObject(comObject);

			try
			{
				IDispatch dispatch = (IDispatch)comObject;
				dispatch.GetTypeInfoCount(out uint typeInfoCount);
				ct.ITypeInfo typeInfoToUse = null;
				int foundDispId = -1;
				FUNCDESC? foundFuncDesc = null;

				// Search through all available ITypeInfo interfaces.
				for (uint i = 0; i < typeInfoCount; i++)
				{
					dispatch.GetTypeInfo(0, 0, out ITypeInfo ti);

					try
					{
						// Try to get the DISPID for the method.
						string[] names = [methodName];
						int[] dispIds = new int[1];
						ti.GetIDsOfNames(names, 1, dispIds);
						// If we get here, this type-info defines the method.
						foundDispId = dispIds[0];
						// Get the TYPEATTR so we can iterate the functions.
						ti.GetTypeAttr(out IntPtr pTypeAttr);
						var typeAttr = Marshal.PtrToStructure<TYPEATTR>(pTypeAttr);

						// Iterate over all functions in this type.
						for (int j = 0; j < typeAttr.cFuncs; j++)
						{
							ti.GetFuncDesc(j, out IntPtr pFuncDesc);
							var funcDesc = Marshal.PtrToStructure<FUNCDESC>(pFuncDesc);

							if (funcDesc.memid == foundDispId)
							{
								foundFuncDesc = funcDesc;
								ti.ReleaseFuncDesc(pFuncDesc);
								break;
							}

							ti.ReleaseFuncDesc(pFuncDesc);
						}

						ti.ReleaseTypeAttr(pTypeAttr);

						if (foundFuncDesc != null)
						{
							typeInfoToUse = ti;
							break;
						}
					}
					catch
					{
						// Method not found in this interface; release and try next.
						Marshal.ReleaseComObject(ti);
					}
				}

				if (typeInfoToUse == null || foundFuncDesc == null)
					throw new Exception($"Method '{methodName}' not found in any type-info interface.");//Make exceptions behave like others.//TODO

				FUNCDESC funcDescFound = foundFuncDesc.Value;
				int paramCount = funcDescFound.cParams;

				if (inputParameters.Length != paramCount)
					throw new ArgumentException("Input parameter count does not match the COM method signature.");

				// Prepare expected type array and build a ParameterModifier.
				var expectedTypes = new Type[paramCount];
				var modifier = new ParameterModifier(paramCount);

				// Read each ELEMDESC from lprgelemdescParam.
				for (int i = 0; i < paramCount; i++)
				{
					var pElemDesc = new IntPtr(funcDescFound.lprgelemdescParam.ToInt64() + i * Marshal.SizeOf(typeof(ELEMDESC)));
					var elemDesc = Marshal.PtrToStructure<ELEMDESC>(pElemDesc);
					// First, check if VT_BYREF is set.
					var isByRef = (elemDesc.tdesc.vt & Com.vt_byref) != 0;
					// Mask out VT_BYREF to get the base VARTYPE.
					var vtBase = (short)(elemDesc.tdesc.vt & ~Com.vt_byref);

					// If the base type is VT_PTR (26), then we try to get the pointed-to type.
					if (vtBase == Com.vt_ptr)
					{
						// VT_PTR typically means the parameter is a pointer (i.e. byref).
						// Mark it as byref.
						modifier[i] = true;

						if (elemDesc.tdesc.lpValue != IntPtr.Zero)
						{
							// Read the pointed-to TYPEDESC.
							var pointedType = Marshal.PtrToStructure<TYPEDESC>(elemDesc.tdesc.lpValue);
							var pointedVt = (short)(pointedType.vt & ~(short)Com.vt_byref);

							switch (pointedVt)
							{
								case Com.vt_i2:
									expectedTypes[i] = typeof(short);
									break;

								case Com.vt_i4:
								case Com.vt_int:
									expectedTypes[i] = typeof(int);
									break;

								case Com.vt_i8:
									expectedTypes[i] = typeof(long);
									break;

								case Com.vt_r4:
									expectedTypes[i] = typeof(float);
									break;

								case Com.vt_r8:
									expectedTypes[i] = typeof(double);
									break;

								case Com.vt_bool:
									expectedTypes[i] = typeof(bool);
									break;

								case Com.vt_bstr:
									expectedTypes[i] = typeof(string);
									break;

								case Com.vt_variant:
									expectedTypes[i] = typeof(object);
									break;

								default:
									expectedTypes[i] = typeof(object);
									break;
							}
						}
						else
						{
							// No pointed-to type info; assume object.
							expectedTypes[i] = typeof(object);
						}
					}
					else
					{
						// Otherwise, use the normal mapping.
						modifier[i] = isByRef;

						switch (vtBase)
						{
							case Com.vt_i2:
								expectedTypes[i] = typeof(short);
								break;

							case Com.vt_i4:
							case Com.vt_int:
								expectedTypes[i] = typeof(int);
								break;

							case Com.vt_i8:
								expectedTypes[i] = typeof(long);
								break;

							case Com.vt_r4:
								expectedTypes[i] = typeof(float);
								break;

							case Com.vt_r8:
								expectedTypes[i] = typeof(double);
								break;

							case Com.vt_bool:
								expectedTypes[i] = typeof(bool);
								break;

							case Com.vt_bstr:
								expectedTypes[i] = typeof(string);
								break;

							case Com.vt_variant:
								expectedTypes[i] = typeof(object);
								break;

							default:
								expectedTypes[i] = typeof(object);
								break;
						}
					}
				}

				// Build the ParameterModifier array.
				var modifiers = new ParameterModifier[] { modifier };
				// Convert input parameters to the expected types.
				var convertedParameters = new object[paramCount];

				for (int i = 0; i < paramCount; i++)
				{
					try
					{
						convertedParameters[i] = Convert.ChangeType(inputParameters[i], expectedTypes[i], CultureInfo.CurrentCulture);
					}
					catch (Exception ex)
					{
						throw new Exception($"Conversion failed for parameter {i} to {expectedTypes[i].FullName}.", ex);
					}
				}

				// Invoke the method using InvokeMember.
				comObject.GetType().InvokeMember(
					methodName,
					BindingFlags.InvokeMethod,
					null,
					comObject,
					convertedParameters,
					modifiers,
					CultureInfo.CurrentCulture,
					null
				);
				// Return the updated parameters array (which now contains out/ref values).
				return convertedParameters;
			}
			finally
			{
				Marshal.Release(pDispatch);
			}

			return null;
		}
	}
}

#endif