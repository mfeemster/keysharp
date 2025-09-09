﻿#if WINDOWS
namespace Keysharp.Core.COM
{
	public unsafe class ComMethodPropertyHolder : MethodPropertyHolder
	{
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

					//Convert back to types Keysharp uses.
					if (o.ParseLong(out long l, false, false))
						obj[i] = l;
					else if (o.ParseDouble(out double d, false, true))
						obj[i] = d;
					else
						obj[i] = args[i];

					//KeysharpEnhancements.OutputDebugLine($"Parameter {i}: {results[i]} (type: {results[i]?.GetType().Name ?? "null"})");
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
			nint pUnk = Marshal.GetIUnknownForObject(comObject);
			_ = Marshal.Release(pUnk);

			try
			{
				var found = false;
				var paramCount = -1;
				Type[] expectedTypes = null;
				ParameterModifier[] modifiers = null;
				Dictionary<int, object> refs = new();

				if (Script.TheScript.ComMethodData.comMethodCache.TryGet(pUnk, out var objDkt))
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

					if (ti == null)
						goto AfterTypeInfoQuery;

					//Put this in a try catch because GetIDsOfNames() will fail in a unit test scenario,
					//but run fine everywhere else for reasons unknown.
					var names = new string[] { methodName };
					var dispIds = new int[1];
					var dummy = Guid.Empty;
					_ = dispatch.GetIDsOfNames(ref dummy, names, 1, Com.LOCALE_SYSTEM_DEFAULT, dispIds);

					if (dispIds.Length == 0)
						goto AfterTypeInfoQuery;

					//If we get here, this type - info defines the method.
					var foundDispId = dispIds[0];
					ti.GetContainingTypeLib(out var typeLib, out int pIndex);//This is the only way to properly get all interfaces.

					if (typeLib == null)
						goto AfterTypeInfoQuery;

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
                                        found = true;
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

					AfterTypeInfoQuery:

					if (!found)
					{
						paramCount = inputParameters.Length;
						expectedTypes = new Type[paramCount];
						var modifier = new ParameterModifier(paramCount);

						for (int i = 0; i < paramCount; i++)
						{
							expectedTypes[i] = typeof(object);
							modifier[i] = false;
						}

						modifiers = [modifier];
						found = true; // Do not cache
						//return Errors.ErrorOccurred(err = new TypeError($"COM call to '{methodName}()' could not be found in any type-info interface.");
					}
					else
						found = false; // Set back to false to cache the result

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
								var pElemDesc = new nint(funcDesc.lprgelemdescParam.ToInt64() + (i * Marshal.SizeOf<ELEMDESC>()));
								var elemDesc = Marshal.PtrToStructure<ELEMDESC>(pElemDesc);
								//First, check if VT_BYREF is set.
								var isByRef = ((VarEnum)elemDesc.tdesc.vt & VarEnum.VT_BYREF) != 0;
								//Mask out VT_BYREF to get the base VARTYPE.
								var vtBase = (VarEnum)elemDesc.tdesc.vt & ~VarEnum.VT_BYREF;

								//If the base type is VT_PTR (26), then we try to get the pointed-to type.
								if (vtBase == VarEnum.VT_PTR)
								{
									//VT_PTR typically means the parameter is a pointer (i.e. byref).
									//Mark it as byref.
									modifier[i] = true;

									if (elemDesc.tdesc.lpValue != 0)
									{
										//Read the pointed-to TYPEDESC.
										var pointedType = Marshal.PtrToStructure<TYPEDESC>(elemDesc.tdesc.lpValue);
										var pointedVt = (VarEnum)pointedType.vt & ~VarEnum.VT_BYREF;
										expectedTypes[i] = Com.ConvertVarTypeToCLRType(pointedVt);
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
									expectedTypes[i] = Com.ConvertVarTypeToCLRType(vtBase);
								}
								if (modifier[i] && i < inputParameters.Length && inputParameters[i] is KeysharpObject)
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
						{
							if (it == typeof(long) || it == typeof(nint))
							{
								var buf = inputParameters[i].Ai();

								if (buf <= int.MaxValue && buf >= int.MinValue)
									inputParameters[i] = buf;
							}
							else if (it == typeof(ulong))
							{
								var buf = inputParameters[i].Aui();

								if (buf <= uint.MaxValue && buf >= uint.MinValue)
									inputParameters[i] = buf;
							}

							inputParameters[i] = Convert.ChangeType(inputParameters[i], expectedTypes[i], CultureInfo.CurrentCulture);
						}
					}
					catch (Exception)
					{
						return Errors.TypeErrorOccurred(inputParameters[i], expectedTypes[i], DefaultErrorObject);
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
					_ = TheScript.ComMethodData.comMethodCache
						.GetOrAdd(pUnk, key => new Dictionary<string, ComMethodInfo>(StringComparer.OrdinalIgnoreCase))
						.GetOrAdd(methodName, new ComMethodInfo()
					{
						modifiers = modifiers,
						expectedTypes = expectedTypes
					});
				}

				return ret;
			}
			catch (Exception ex)
			{
				return Errors.ErrorOccurred($"COM call to '{methodName}()' failed: {ex.Message}.");
			}
		}
	}

	internal class ComMethodData
	{
		internal ConcurrentLfu<nint, Dictionary<string, ComMethodInfo>> comMethodCache = new (Caching.DefaultCacheCapacity);
	}

	internal class ComMethodInfo
	{
		internal Type[] expectedTypes;
		internal ParameterModifier[] modifiers;
	}
}

#endif