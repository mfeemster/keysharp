#if WINDOWS
namespace Keysharp.Core.COM
{
	internal class ComMethodInfo
	{
		internal ParameterModifier[] modifiers;
		internal Type[] expectedTypes;
	}

	unsafe public class ComMethodPropertyHolder : MethodPropertyHolder
	{
		static Dictionary<object, Dictionary<string, ComMethodInfo>> comMethodCache = [];

		public string Name { get; private set; }

		public ComMethodPropertyHolder(string name)
		{
			Name = name;
			_callFunc = (inst, obj) =>
			{
				var t = inst.GetType();
				var args = new object[obj.Length];

				for (var i = 0; i < obj.Length; ++i)
				{
					var o = obj[i];
					var co = o as ComObject;//Unsure how or if this will even still work. Should just pass a regular variable and let the marshaller handle it.
					var p = co != null ? co.Ptr : o;
					args[i] = p;
				}

				var ret = InvokeComMethodWithTypeInfo(inst, Name, args);

				for (int i = 0; i < args.Length; i++)
				{
					var o = args[i];
					var l = 0L;
					var d = 0.0;

					//Convert back to types Keysharp uses.
					if (o.ParseLong(ref l, false, false))
						obj[i] = l;
					else if (o.ParseDouble(ref d, false, true))
						obj[i] = d;
					else
						obj[i] = args[i];

					//Debug.OutputDebug($"Parameter {i}: {results[i]} (type: {results[i]?.GetType().Name ?? "null"})");
				}

				return ret;
			};
		}


		///<summary>
		///Dynamically invokes a COM method by querying all available type-info interfaces on the COM object.
		///It locates an ITypeInfo that defines the method, retrieves the FUNCDESC to determine expected parameter
		///types and by-reference modifiers, converts input parameters accordingly, and then uses InvokeMember().
		///</summary>
		///<param name="comObject">The COM object implementing IDispatch.</param>
		///<param name="methodName">The method name to invoke.</param>
		///<param name="inputParameters">An array of input values (for in/out parameters).</param>
		///<returns>The updated parameters array (including out/ref values) after invocation.</returns>
		private static object InvokeComMethodWithTypeInfo(object comObject, string methodName, object[] inputParameters)
		{
			Error err;

			try
			{
				var found = false;
				var foundFuncDesc = false;
				var paramCount = -1;
				Type[] expectedTypes = null;
				ParameterModifier[] modifiers = null;
				Dictionary<int, object> refs = new();

				if (comMethodCache.TryGetValue(comObject, out var objDkt))
				{
					if (objDkt.TryGetValue(methodName, out var cmi))
					{
						modifiers = cmi.modifiers;
						expectedTypes = cmi.expectedTypes;
						found = true;
					}
				}

				if (!found)
				{
					ITypeInfo ti = null;
					var dispatch = (IDispatch)comObject;

					if (comObject is IProvideClassInfo ipci)//May not be necessary.
						_ = ipci.GetClassInfo(out ti);
					else
						_ = dispatch.GetTypeInfo(0, 0, out ti);

					//Put this in a try catch because GetIDsOfNames() will fail in a unit test scenario,
					//but run fine everywhere else for reasons unknown.
					var names = new string[] { methodName };
					var dispIds = new int[1];
					var dummy = Guid.Empty;
					_ = dispatch.GetIDsOfNames(ref dummy, names, 1, Com.LOCALE_SYSTEM_DEFAULT, dispIds);
					//If we get here, this type - info defines the method.
					var foundDispId = dispIds[0];
					ti.GetContainingTypeLib(out var typeLib, out int pIndex);//This is the only way to properly get all interfaces.
					var typeCount = typeLib.GetTypeInfoCount();

					for (var i = 0; i < typeCount; i++)
					{
						typeLib.GetTypeInfo(i, out ti);

						try
						{
							//Get the type attributes.
							ti.GetTypeAttr(out var pTypeAttr);
							var typeAttr = Marshal.PtrToStructure<TYPEATTR>(pTypeAttr);

							//If the type has functions, enumerate them.
							for (int j = 0; j < typeAttr.cFuncs; j++)
							{
								ti.GetFuncDesc(j, out var pFuncDesc);
								var funcDesc = Marshal.PtrToStructure<FUNCDESC>(pFuncDesc);

								//Get the function's name from its memid.
								if (foundDispId == funcDesc.memid)
								{
									ti.GetDocumentation(funcDesc.memid, out var name, out var docString, out var helpContext, out var helpFile);

									if (name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
									{
                                        foundFuncDesc = true;
										methodName = name;
										PopulateModifiers(funcDesc);
										i = int.MaxValue - 1;
										ti.ReleaseFuncDesc(pFuncDesc);
										break;
									}
								}

								ti.ReleaseFuncDesc(pFuncDesc);
							}

							ti.ReleaseTypeAttr(pTypeAttr);
						}
						finally
						{
							_ = Marshal.ReleaseComObject(ti);
						}
					}

					if (!foundFuncDesc)
						return Errors.ErrorOccurred(err = new TypeError($"COM call to '{methodName}()' could not be found in any type-info interface.")) ? throw err : null;

					void PopulateModifiers(FUNCDESC funcDesc)
					{
						paramCount = funcDesc.cParams;
						//Prepare expected type array and build a ParameterModifier.

						if (paramCount != 0)
						{
							expectedTypes = new Type[paramCount];
							var modifier = new ParameterModifier(paramCount);

							//Read each ELEMDESC from lprgelemdescParam.
							for (int i = 0; i < paramCount; i++)
							{
								var pElemDesc = new IntPtr(funcDesc.lprgelemdescParam.ToInt64() + (i * Marshal.SizeOf<ELEMDESC>()));
								var elemDesc = Marshal.PtrToStructure<ELEMDESC>(pElemDesc);
								//First, check if VT_BYREF is set.
								var isByRef = (elemDesc.tdesc.vt & Com.vt_byref) != 0;
								//Mask out VT_BYREF to get the base VARTYPE.
								var vtBase = (short)(elemDesc.tdesc.vt & ~Com.vt_byref);

								//If the base type is VT_PTR (26), then we try to get the pointed-to type.
								if (vtBase == Com.vt_ptr)
								{
									//VT_PTR typically means the parameter is a pointer (i.e. byref).
									//Mark it as byref.
									modifier[i] = true;

									if (elemDesc.tdesc.lpValue != IntPtr.Zero)
									{
										//Read the pointed-to TYPEDESC.
										var pointedType = Marshal.PtrToStructure<TYPEDESC>(elemDesc.tdesc.lpValue);
										var pointedVt = (short)(pointedType.vt & ~Com.vt_byref);
										ConvertType(i, pointedVt);
									}
									else
									{
										//No pointed-to type info; assume object.
										expectedTypes[i] = typeof(object);
									}
								}
								else
								{
									//Otherwise, use the normal mapping.
									modifier[i] = isByRef;
									ConvertType(i, vtBase);
								}
								if (modifier[i] && i < inputParameters.Length)
								{
									refs[i] = inputParameters[i];
									inputParameters[i] = Script.GetPropertyValue(inputParameters[i], "__Value");
                                }
                            }

							modifiers = [modifier];
						}
					}
				}

				paramCount = Math.Min(inputParameters.Length, expectedTypes?.Length ?? 0);

				//Convert input parameters to the expected types.
				for (var i = 0; i < paramCount; i++)
				{
					try
					{
						inputParameters[i] ??= "";

                        var et = expectedTypes[i];
						var it = inputParameters[i].GetType();

						if (et == it)
							continue;

						if (et == typeof(string))
							inputParameters[i] = inputParameters[i].As();
						else if (et == typeof(int))
							inputParameters[i] = inputParameters[i].Ai();
						else if (et == typeof(uint))
							inputParameters[i] = inputParameters[i].Aui();
						else if (et == typeof(long))
							inputParameters[i] = inputParameters[i].Al();
						else if (et == typeof(ulong))
							inputParameters[i] = (ulong)inputParameters[i].Al();
						else if (et == typeof(double))
							inputParameters[i] = inputParameters[i].Ad();
						else if (et == typeof(float))
							inputParameters[i] = (float)inputParameters[i].Ad();
						else if (et == typeof(short))
							inputParameters[i] = (short)inputParameters[i].Al();
						else if (et == typeof(ushort))
							inputParameters[i] = (ushort)inputParameters[i].Aui();
						else if (et == typeof(bool))
							inputParameters[i] = (short)(inputParameters[i].Ab() ? -1 : 0);
						else if (et == typeof(sbyte))
							inputParameters[i] = (sbyte)inputParameters[i].Ai();
						else if (et == typeof(byte))
							inputParameters[i] = (byte)inputParameters[i].Aui();
						else
							inputParameters[i] = Convert.ChangeType(inputParameters[i], expectedTypes[i], CultureInfo.CurrentCulture);
					}
					catch (Exception)
					{
						return Errors.ErrorOccurred(err = new TypeError($"COM call to '{methodName}()' failed to convert parameter {i} of type {inputParameters[i].GetType()} to type {expectedTypes[i].FullName}.")) ? throw err : null;
					}
				}

				//Invoke the method using InvokeMember.
				object ret = null;

				if (modifiers == null)
					ret = comObject.GetType().InvokeMember(
							  methodName,
							  BindingFlags.InvokeMethod | BindingFlags.GetProperty,
							  null,
							  comObject,
							  []);
				else
					ret = comObject.GetType().InvokeMember(
							  methodName,
							  BindingFlags.InvokeMethod | BindingFlags.GetProperty,
							  null,
							  comObject,
							  inputParameters,
							  modifiers,
							  CultureInfo.CurrentCulture,
							  null);

				foreach (var r in refs)
					Script.SetPropertyValue(r.Value, "__Value", inputParameters[r.Key]);

				//If no exception thrown and it wasn't cached, cache the info.
				if (!found)
				{
					_ = comMethodCache.GetOrAdd(comObject).GetOrAdd(methodName, new ComMethodInfo()
					{
						modifiers = modifiers,
						expectedTypes = expectedTypes
					});
				}

				return ret;
				void ConvertType(int i, short vt)
				{

					expectedTypes[i] = vt switch
				{
						Com.vt_i2 => typeof(short),
							Com.vt_i4 or Com.vt_int => typeof(int),
							Com.vt_i8 => typeof(long),
							Com.vt_r4 => typeof(float),
							Com.vt_r8 => typeof(double),
							Com.vt_bool => typeof(bool),
							Com.vt_bstr => typeof(string),
							Com.vt_variant => typeof(object),
							_ => typeof(object),
					};
				}
			}
			catch (Exception ex)
			{
				return Errors.ErrorOccurred(err = new TypeError($"COM call to '{methodName}()' failed: {ex.Message}.")) ? throw err : null;
			}
		}
	}
}

#endif