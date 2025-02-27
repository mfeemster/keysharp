using System;
using System.Reflection;
using System.Reflection.Emit;
using Keysharp.Core.Windows;
using Label = System.Reflection.Emit.Label;

namespace Keysharp.Core.Common.Invoke
{
	public class MethodPropertyHolder
	{
		public Func<object, object[], object> callFunc;
		internal readonly MethodInfo mi;
		internal readonly ParameterInfo[] parameters;
		internal readonly PropertyInfo pi;
		internal readonly Action<object, object> setPropFunc;
		protected readonly ConcurrentStackArrayPool<object> paramsPool;
		private readonly bool isGuiType;
		private readonly bool anyOptional;
		private readonly int startVarIndex = -1;
		private readonly int stopVarIndexDistanceFromEnd;

		internal bool IsStaticFunc { get; private set; }
		internal bool IsStaticProp { get; private set; }
		internal bool IsBind { get; private set; }
		internal bool IsVariadic => startVarIndex != -1;
		internal int ParamLength { get; }

		public MethodPropertyHolder(MethodInfo m, PropertyInfo p)
		{
			mi = m;
			pi = p;

			if (mi != null)
			{
                parameters = mi.GetParameters();
				ParamLength = parameters.Length;
				isGuiType = Gui.IsGuiType(mi.DeclaringType);
				anyOptional = parameters.Any(p => p.IsOptional || p.IsVariadic());

                for (var i = 0; i < parameters.Length; i++)
				{
					if (parameters[i].ParameterType == typeof(object[]))
						startVarIndex = i;
					else if (startVarIndex != -1 && stopVarIndexDistanceFromEnd == 0)
						stopVarIndexDistanceFromEnd = parameters.Length - i;
				}

				IsStaticFunc = mi.Attributes.HasFlag(MethodAttributes.Static);
				var isFuncObj = typeof(IFuncObj).IsAssignableFrom(mi.DeclaringType);

				if (isFuncObj && mi.Name == "Bind")
					IsBind = true;

				if (isFuncObj && mi.Name == "Call")
				{
					callFunc = (inst, obj) => ((IFuncObj)inst).Call(obj);
				} else
				{
                    var del = DelegateFactory.CreateDelegate(mi);

                    if (isGuiType)
                    {
                        callFunc = (inst, obj) =>
                        {
                            var ctrl = inst.GetControl();
                            object ret = null;
                            ctrl.CheckedInvoke(() =>
                            {
                                var oldHandle = Script.HwndLastUsed;

                                if (ctrl != null && ctrl.FindForm() is Form form)
                                    Script.HwndLastUsed = form.Handle;

                                object ret = del(inst, obj);
                                Script.HwndLastUsed = oldHandle;
                            }, true);
                            return ret;
                        };
                    }
                    else
                        callFunc = del;
                }
				return;
				
				if (ParamLength == 0)
				{
					if (isGuiType)
					{
						callFunc = (inst, obj) =>
						{
							object ret = null;
							var ctrl = inst.GetControl();//If it's a gui control, then invoke on the gui thread.
							ctrl.CheckedInvoke(() =>
							{
								var oldHandle = Script.HwndLastUsed;

								if (ctrl != null && ctrl.FindForm() is Form form)
									Script.HwndLastUsed = form.Handle;

								ret = mi.Invoke(inst, null);
								Script.HwndLastUsed = oldHandle;
							}, true);//This can be null if called before a Gui object is fully initialized.
							return ret;
						};
					}
					else
						callFunc = (inst, obj) => mi.Invoke(inst, null);
				}
				else
				{
					paramsPool = new ConcurrentStackArrayPool<object>(ParamLength);

					if (IsVariadic)
					{
                        callFunc = (inst, obj) =>
						{
							object[] newobj = null;
							object[] lastArr = null;
							var objLength = obj.Length;

							if (ParamLength == 1)
							{
								if (obj.Length == 1)
									newobj = [obj];
								else
									newobj = [obj.FlattenNativeArray(true).ToArray()];
							}
							else
							{
								//The slowest case: a function is trying to be called with a different number of parameters than it actually has, or it's variadic,
								//so manually create an array of parameters that matches the required size.
								var oi = 0;
								var pi = 0;
								newobj = paramsPool.Rent();

								for (; oi < objLength && pi < ParamLength; pi++)
								{
									if (pi == startVarIndex)
									{
										if (objLength == ParamLength && obj[pi] is object[] obarr)//There was just one variadic parameter and it was already an array, usuall from a call like func(arr*).
										{
											newobj[pi] = obarr;
										}
										else
										{
											var om1 = obj[pi] == null ? 0 : (objLength - startVarIndex) - stopVarIndexDistanceFromEnd;//obj[pi] == null means null was passed for the variadic parameter.
											lastArr = new object[om1];//Can't really use a pool here because we don't know the exact size ahead of time.

											for (var i = 0; i < om1; i++)
											{
												lastArr[i] = obj[pi + i];
												oi++;
											}

											newobj[pi] = lastArr;
										}
									}
									else
									{
										var ooi = obj[oi++];
										var ppi = parameters[pi];

										if (ooi == null && ppi.IsOptional)
											newobj[pi] = ppi.DefaultValue;
										else
											newobj[pi] = ooi;
									}
								}
                                //newobj[^1] ??= new object[] { };

								if (anyOptional)
									for (; pi < ParamLength; pi++)
										if (parameters[pi].IsOptional)
											newobj[pi] = parameters[pi].DefaultValue;
										else if (parameters[pi].IsVariadic())
											newobj[pi] = System.Array.Empty<object>();
							}

							//Any remaining items in newobj are null by default.
							object ret = null;

							if (!isGuiType)
							{
								ret = mi.Invoke(inst, newobj);
							}
							else//If it's a gui control, then invoke on the gui thread.
							{
								var ctrl = inst.GetControl();
								ctrl.CheckedInvoke(() =>
								{
									var oldHandle = Script.HwndLastUsed;

									if (ctrl != null && ctrl.FindForm() is Form form)
										Script.HwndLastUsed = form.Handle;

									ret = mi.Invoke(inst, newobj);
									Script.HwndLastUsed = oldHandle;
								}, true);//This can be null if called before a Gui object is fully initialized.
							}

							//In case any params were references.
							if (ParamLength > 1)
							{
								var len = Math.Min(newobj.Length, obj.Length);

								if (startVarIndex != -1)
								{
									for (var pi = 0; pi < len; pi++)
									{
										if (pi == startVarIndex)//Expand the variadic args back into the flat array.
										{
											var arr = newobj[pi] as object[];

											if (objLength == ParamLength)
											{
												obj[pi] = arr;
											}
											else
											{
												for (var vi = 0; pi < obj.Length && vi < arr.Length; pi++, vi++)
													obj[pi] = arr[vi];
											}
										}
										else
											obj[pi] = newobj[pi];
									}
								}
								else
									System.Array.Copy(newobj, obj, len);

								_ = paramsPool.Return(newobj, true);
							}

							return ret;
						};
					}
					else
					{
                        callFunc = (inst, obj) =>
						{
							object ret = null;
							var objLength = obj.Length;

							if (ParamLength == objLength)
							{
								if (anyOptional)
									for (var pi = 0; pi < ParamLength; ++pi)
										if (obj[pi] == null && parameters[pi].IsOptional)
											obj[pi] = parameters[pi].DefaultValue;

                                if (!isGuiType)
								{
									ret = mi.Invoke(inst, obj);
								}
								else//If it's a gui control, then invoke on the gui thread.
								{
									var ctrl = inst.GetControl();
									ctrl.CheckedInvoke(() =>
									{
										var oldHandle = Script.HwndLastUsed;

										if (ctrl != null && ctrl.FindForm() is Form form)
											Script.HwndLastUsed = form.Handle;

										ret = mi.Invoke(inst, obj);
										Script.HwndLastUsed = oldHandle;
									}, true);//This can be null if called before a Gui object is fully initialized.
								}
							}
							else
							{
								var pi = 0;//The slower case: a function is trying to be called with a different number of parameters than it actually has, so manually create an array of parameters that matches the required size.
								var newobj = paramsPool.Rent();//Using the memory pool in this function seems to help a lot.

								for (; pi < objLength && pi < ParamLength; ++pi)
									if (obj[pi] == null && parameters[pi].IsOptional)
										newobj[pi] = parameters[pi].DefaultValue;
									else
										newobj[pi] = obj[pi];

								if (anyOptional)
									for (; pi < ParamLength; ++pi)
										if (parameters[pi].IsOptional)
											newobj[pi] = parameters[pi].DefaultValue;

                                //Any remaining items in newobj are null by default.
                                if (!isGuiType)
								{
									ret = mi.Invoke(inst, newobj);
								}
								else//If it's a gui control, then invoke on the gui thread.
								{
									var ctrl = inst.GetControl();
									ctrl.CheckedInvoke(() =>
									{
										var oldHandle = Script.HwndLastUsed;

										if (ctrl != null && ctrl.FindForm() is Form form)
											Script.HwndLastUsed = form.Handle;

										ret = mi.Invoke(inst, newobj);
										Script.HwndLastUsed = oldHandle;
									}, true);//This can be null if called before a Gui object is fully initialized.
								}

								System.Array.Copy(newobj, obj, Math.Min(newobj.Length, objLength));//In case any params were references.
								_ = paramsPool.Return(newobj, true);
							}

							return ret;
						};
					}
				}
			}
			else if (pi != null)
			{
				isGuiType = Gui.IsGuiType(pi.DeclaringType);
				parameters = pi.GetIndexParameters();
				ParamLength = parameters.Length;

				if (pi.GetAccessors().Any(x => x.IsStatic))
				{
					IsStaticProp = true;

					if (isGuiType)
					{
						callFunc = (inst, obj) =>//Gui calls aren't worth optimizing further.
						{
							object ret = null;
							var ctrl = inst.GetControl();//If it's a gui control, then invoke on the gui thread.
							ctrl.CheckedInvoke(() =>
							{
								var oldHandle = Script.HwndLastUsed;

								if (ctrl != null && ctrl.FindForm() is Form form)
									Script.HwndLastUsed = form.Handle;

								ret = pi.GetValue(null);
								Script.HwndLastUsed = oldHandle;
							}, true);//This can be null if called before a Gui object is fully initialized.

							if (ret is int i)
								ret = (long)i;//Try to keep everything as long.

							return ret;
						};
						setPropFunc = (inst, obj) =>
						{
							var ctrl = inst.GetControl();//If it's a gui control, then invoke on the gui thread.
							ctrl.CheckedInvoke(() =>
							{
								var oldHandle = Script.HwndLastUsed;

								if (ctrl != null && ctrl.FindForm() is Form form)
									Script.HwndLastUsed = form.Handle;

								pi.SetValue(null, obj);
								Script.HwndLastUsed = oldHandle;
							}, true);//This can be null if called before a Gui object is fully initialized.
						};
					}
					else
					{
						if (pi.PropertyType == typeof(int))
						{
							callFunc = (inst, obj) =>
							{
								var ret = pi.GetValue(null);

								if (ret is int i)
									ret = (long)i;//Try to keep everything as long.

								return ret;
							};
						}
						else
							callFunc = (inst, obj) => pi.GetValue(null);

						setPropFunc = (inst, obj) => pi.SetValue(null, obj);
					}
				}
				else
				{
					if (isGuiType)
					{
						callFunc = (inst, obj) =>
						{
							object ret = null;
							var ctrl = inst.GetControl();//If it's a gui control, then invoke on the gui thread.
							ctrl.CheckedInvoke(() =>
							{
								var oldHandle = Script.HwndLastUsed;

								if (ctrl != null && ctrl.FindForm() is Form form)
									Script.HwndLastUsed = form.Handle;

								ret = pi.GetValue(inst);
								Script.HwndLastUsed = oldHandle;
							}, true);//This can be null if called before a Gui object is fully initialized.

							if (ret is int i)
								ret = (long)i;//Try to keep everything as long.

							return ret;
						};
						setPropFunc = (inst, obj) =>
						{
							var ctrl = inst.GetControl();//If it's a gui control, then invoke on the gui thread.
							ctrl.CheckedInvoke(() =>
							{
								var oldHandle = Script.HwndLastUsed;

								if (ctrl != null && ctrl.FindForm() is Form form)
									Script.HwndLastUsed = form.Handle;

								pi.SetValue(inst, obj);
								Script.HwndLastUsed = oldHandle;
							}, true);//This can be null if called before a Gui object is fully initialized.
						};
					}
					else
					{
						if (pi.PropertyType == typeof(int))
						{
							callFunc = (inst, obj) =>
							{
								var ret = pi.GetValue(inst);

								if (ret is int i)
									ret = (long)i;//Try to keep everything as long.

								return ret;
							};
						}
						else
							callFunc = (inst, obj) => pi.GetValue(inst);

						setPropFunc = pi.SetValue;
					}
				}
			}
		}
    }
	public static class DelegateFactory
    {

        /// <summary>
        /// Creates a delegate of type Func<object, object[], object></object>that will call the given MethodInfo.
        /// It will check for missing parameters and if the parameter is optional, it uses its DefaultValue.
        /// </summary>
        public static Func<object, object[], object> CreateDelegate(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            ParameterInfo[] parameters = method.GetParameters();
            DynamicMethod dm = new DynamicMethod(
                "DynamicInvoke_" + method.Name,
                typeof(object),
                new Type[] { typeof(object), typeof(object[]) },
                method.Module,
                true);

            ILGenerator il = dm.GetILGenerator();

            // --- Declare unified locals ---
            LocalBuilder paramOffset = il.DeclareLocal(typeof(int));   // will be 0 for static or for instance when ldarg0 is non-null; 1 for instance if ldarg0 is null.
            LocalBuilder argsLocal = il.DeclareLocal(typeof(object[]));  // the effective arguments array

            // --- Compute paramOffset and argsLocal ---
            if (!method.IsStatic)
            {
                // Instance method:
                // Use ldarg0 if non-null; otherwise use args[0] and set offset=1.
                LocalBuilder target = il.DeclareLocal(typeof(object));
                Label useArg0 = il.DefineLabel();
                Label afterTarget = il.DefineLabel();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Brtrue_S, useArg0);
                // ldarg0 is null: set paramOffset = 1 and target = args[0].
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Stloc, paramOffset);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Stloc, target);
                il.Emit(OpCodes.Br_S, afterTarget);
                il.MarkLabel(useArg0);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, paramOffset);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Stloc, target);
                il.MarkLabel(afterTarget);
                // Push the target (cast as needed) for the eventual call.
                il.Emit(OpCodes.Ldloc, target);
                if (method.DeclaringType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, method.DeclaringType);
                else
                    il.Emit(OpCodes.Castclass, method.DeclaringType);

                // For instance methods, we use the caller's argument array unchanged.
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stloc, argsLocal);
            }
            else
            {
                // Static method: always set paramOffset = 0.
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, paramOffset);
                // Set argsLocal = ldarg_1.
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stloc, argsLocal);

                // If the delegate's instance (ldarg0) is non-null, inject it into the argument array.
                Label useOriginal = il.DefineLabel();
                Label afterCombine = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Brfalse_S, useOriginal);

                LocalBuilder origLen = il.DeclareLocal(typeof(int));
                il.Emit(OpCodes.Ldloc, argsLocal);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Conv_I4);
                il.Emit(OpCodes.Stloc, origLen);

                // length = origLen + 1.
                LocalBuilder newLen = il.DeclareLocal(typeof(int));
                il.Emit(OpCodes.Ldloc, origLen);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, newLen);

                // Allocate new array of length newLen.
                il.Emit(OpCodes.Ldloc, newLen);
                il.Emit(OpCodes.Newarr, typeof(object));
                LocalBuilder combined = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Stloc, combined);

                // combined[0] = ldarg0.
                il.Emit(OpCodes.Ldloc, combined);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Stelem_Ref);

                // If newLen > 1, copy original arguments from argsLocal into combined starting at index 1.
                Label skipCopy = il.DefineLabel();
                il.Emit(OpCodes.Ldloc, newLen);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ble_S, skipCopy);
                LocalBuilder idx = il.DeclareLocal(typeof(int));
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, idx);
                Label loopStart = il.DefineLabel();
                Label loopEnd = il.DefineLabel();
                il.MarkLabel(loopStart);
                il.Emit(OpCodes.Ldloc, idx);
                il.Emit(OpCodes.Ldloc, origLen);
                il.Emit(OpCodes.Bge_S, loopEnd);
                // combined[idx + 1] = argsLocal[idx]
                il.Emit(OpCodes.Ldloc, combined);
                il.Emit(OpCodes.Ldloc, idx);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Ldloc, argsLocal);
                il.Emit(OpCodes.Ldloc, idx);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Ldloc, idx);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, idx);
                il.Emit(OpCodes.Br_S, loopStart);
                il.MarkLabel(loopEnd);
                il.MarkLabel(skipCopy);
                // Update argsLocal with the combined array.
                il.Emit(OpCodes.Ldloc, combined);
                il.Emit(OpCodes.Stloc, argsLocal);
                il.MarkLabel(useOriginal);
            }


            // --------------------------------------------------
            // Pre-check: compute the combined argument count.
            // argsLocal was created earlier and holds the combined arguments.
            LocalBuilder providedCountLocal = il.DeclareLocal(typeof(int));
            il.Emit(OpCodes.Ldloc, argsLocal);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Stloc, providedCountLocal);

            // Compute available argument count for parameters:
            LocalBuilder availableCountLocal = il.DeclareLocal(typeof(int));
            il.Emit(OpCodes.Ldloc, providedCountLocal);
            il.Emit(OpCodes.Ldloc, paramOffset);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Stloc, availableCountLocal);

            // Determine if the method is a set_Item overload.
            bool isSetItem = (method.Name == "set_Item");
 
            // Parameter count pre-check
            int formalCount = parameters.Length;
            if (isSetItem)
            {
                // Compute nonOptionalIndexCount over parameters[0 .. formalCount-2]:
                int nonOptionalIndexCount = 0;
                for (int j = 0; j < formalCount - 1; j++)
                {
                    if (!parameters[j].IsOptional)
                        nonOptionalIndexCount++;
                }
                int minRequired = nonOptionalIndexCount + 1; // at least one for the "value"

                // Determine if this is the special case: the index parameter is an object[]
                // (i.e. the second-to-last parameter is of type object[])
                bool isSpecialSetItem = (formalCount > 1 &&
                                         parameters[formalCount - 2].ParameterType == typeof(object[]));

                // --- Emit IL to check that availableCountLocal >= minRequired ---
                il.Emit(OpCodes.Ldloc, availableCountLocal);          // push availableCountLocal
                il.Emit(OpCodes.Ldc_I4, minRequired);                  // push minRequired
                Label setItemOkLabel = il.DefineLabel();
                il.Emit(OpCodes.Bge_S, setItemOkLabel);                // if availableCountLocal >= minRequired, branch
                il.Emit(OpCodes.Newobj, typeof(TargetParameterCountException)
                                         .GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Throw);
                il.MarkLabel(setItemOkLabel);

                // --- For non-special set_Item, also check that availableCountLocal <= formalCount ---
                if (!isSpecialSetItem)
                {
                    il.Emit(OpCodes.Ldloc, availableCountLocal);
                    il.Emit(OpCodes.Ldc_I4, formalCount);
                    Label setItemOkLabel2 = il.DefineLabel();
                    il.Emit(OpCodes.Ble_S, setItemOkLabel2);           // if availableCountLocal <= formalCount, ok
                    il.Emit(OpCodes.Newobj, typeof(TargetParameterCountException)
                                             .GetConstructor(Type.EmptyTypes));
                    il.Emit(OpCodes.Throw);
                    il.MarkLabel(setItemOkLabel2);
                }
            }
            else
            {
                // For normal methods:
                bool hasParams = false;
                if (formalCount > 0)
                {
                    ParameterInfo lastParam = parameters[formalCount - 1];
                    hasParams = lastParam.ParameterType.IsArray;
                }

                // If there is a params parameter, only parameters before it are required.
                int requiredCount = 0;
                int paramLimit = formalCount;
                if (hasParams)
                {
                    paramLimit = formalCount - 1;
                }
                for (int j = 0; j < paramLimit; j++)
                {
                    if (!parameters[j].IsOptional)
                        requiredCount++;
                }

                // Check that availableCountLocal >= requiredCount.
                il.Emit(OpCodes.Ldloc, availableCountLocal);
                il.Emit(OpCodes.Ldc_I4, requiredCount);
                Label normalOkLabel = il.DefineLabel();
                il.Emit(OpCodes.Bge_S, normalOkLabel);
                il.Emit(OpCodes.Newobj, typeof(TargetParameterCountException)
                                         .GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Throw);
                il.MarkLabel(normalOkLabel);

                // Now, if there is no params parameter, ensure that availableCountLocal is not greater than formalCount.
                if (!hasParams)
                {
                    il.Emit(OpCodes.Ldloc, availableCountLocal);
                    il.Emit(OpCodes.Ldc_I4, formalCount);
                    Label normalOkLabel2 = il.DefineLabel();
                    il.Emit(OpCodes.Ble_S, normalOkLabel2);  // if availableCountLocal <= formalCount, ok
                    il.Emit(OpCodes.Newobj, typeof(TargetParameterCountException)
                                             .GetConstructor(Type.EmptyTypes));
                    il.Emit(OpCodes.Throw);
                    il.MarkLabel(normalOkLabel2);
                }
            }

            // --- Parameter Processing ---
            // For each formal parameter at index i, load the effective argument from:
            //    argsLocal[paramOffset + i]
            // If no argument is present, load the default (or for params, an empty array) or throw.
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo pi = parameters[i];

                // Determine special flags.
                // For set_Item, the final parameter ("value") should come from the last argument.
                bool isSetItemValue = isSetItem && (i == parameters.Length - 1);
                // For set_Item, if the second-to-last parameter is of type object[],
                // treat it as a params parameter.
                bool isSpecialParams = isSetItem && (i == parameters.Length - 2) && (pi.ParameterType == typeof(object[]));
                // For non-set_Item methods, a normal params parameter is the last parameter marked with [ParamArray].
                bool isParams = (!isSetItem) &&
                                (i == parameters.Length - 1) &&
                                //pi.IsDefined(typeof(ParamArrayAttribute), false) &&
                                pi.ParameterType.IsArray;

                // --- Branch for Params (or special set_Item params) ---
                if (isParams || isSpecialParams)
                {
                    // Compute count: the number of arguments to pack.
                    // For special params, reserve one argument for the final "value":
                    //    count = argsLocal.Length - (paramOffset + i) - 1
                    // For normal params:
                    //    count = argsLocal.Length - (paramOffset + i)
                    LocalBuilder countLocal = il.DeclareLocal(typeof(int));
                    if (isSpecialParams)
                    {
                        il.Emit(OpCodes.Ldloc, argsLocal);   // push argsLocal
                        il.Emit(OpCodes.Ldlen);                // push argsLocal.Length
                        il.Emit(OpCodes.Conv_I4);
                        il.Emit(OpCodes.Ldloc, paramOffset);
                        il.Emit(OpCodes.Ldc_I4, i);
                        il.Emit(OpCodes.Add);                  // compute (paramOffset + i)
                        il.Emit(OpCodes.Sub);                  // argsLocal.Length - (paramOffset + i)
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Sub);                  // subtract 1 for the final "value"
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc, argsLocal);
                        il.Emit(OpCodes.Ldlen);
                        il.Emit(OpCodes.Conv_I4);
                        il.Emit(OpCodes.Ldloc, paramOffset);
                        il.Emit(OpCodes.Ldc_I4, i);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Sub);                  // argsLocal.Length - (paramOffset + i)
                    }
                    il.Emit(OpCodes.Stloc, countLocal);

                    // Repack the remaining arguments into a new array.
                    Label doRepack = il.DefineLabel();
                    Label repackEnd = il.DefineLabel();
                    il.Emit(OpCodes.Ldloc, countLocal);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Bgt_S, doRepack);
                    {
                        // If count is 0, push an empty array.
                        Type elemType = pi.ParameterType.GetElementType();
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Newarr, elemType);
                        il.Emit(OpCodes.Br_S, repackEnd);
                    }
                    il.MarkLabel(doRepack);
                    LocalBuilder newArray = il.DeclareLocal(pi.ParameterType);
                    il.Emit(OpCodes.Ldloc, countLocal);
                    il.Emit(OpCodes.Newarr, pi.ParameterType.GetElementType());
                    il.Emit(OpCodes.Stloc, newArray);
                    LocalBuilder loopIndex = il.DeclareLocal(typeof(int));
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Stloc, loopIndex);
                    Label loopStart = il.DefineLabel();
                    Label loopCheck = il.DefineLabel();
                    il.Emit(OpCodes.Br_S, loopCheck);
                    il.MarkLabel(loopStart);
                    il.Emit(OpCodes.Ldloc, newArray);
                    il.Emit(OpCodes.Ldloc, loopIndex);
                    il.Emit(OpCodes.Ldloc, argsLocal);
                    il.Emit(OpCodes.Ldloc, paramOffset);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Add);                  // starting index: paramOffset + i
                    il.Emit(OpCodes.Ldloc, loopIndex);
                    il.Emit(OpCodes.Add);                  // add loop index
                    il.Emit(OpCodes.Ldelem_Ref);
                    il.Emit(OpCodes.Stelem_Ref);
                    il.Emit(OpCodes.Ldloc, loopIndex);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Add);
                    il.Emit(OpCodes.Stloc, loopIndex);
                    il.MarkLabel(loopCheck);
                    il.Emit(OpCodes.Ldloc, loopIndex);
                    il.Emit(OpCodes.Ldloc, countLocal);
                    il.Emit(OpCodes.Blt_S, loopStart);
                    il.Emit(OpCodes.Ldloc, newArray);
                    il.MarkLabel(repackEnd);
                }
                else
                {
                    // --- For non-params parameters, load a single element.
                    // Compute effective index: normally paramOffset + i,
                    // except for set_Item value parameter, where it's providedCountLocal - 1.
                    LocalBuilder effectiveIndex = il.DeclareLocal(typeof(int));
                    if (isSetItemValue)
                    {
                        il.Emit(OpCodes.Ldloc, providedCountLocal);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Sub);
                        il.Emit(OpCodes.Stloc, effectiveIndex);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc, paramOffset);
                        il.Emit(OpCodes.Ldc_I4, i);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Stloc, effectiveIndex);
                    }

                    // Check if an argument was provided.
                    Label argProvided = il.DefineLabel();
                    Label afterLoad = il.DefineLabel();
                    il.Emit(OpCodes.Ldloc, effectiveIndex);  // load effective index
                    il.Emit(OpCodes.Ldloc, providedCountLocal);  // load providedCountLocal
                    il.Emit(OpCodes.Blt_S, argProvided);

                    // No argument provided for this parameter.
                    if (pi.IsOptional)
                    {
                        // Load the default value.
                        object defVal = pi.DefaultValue;
                        if (defVal == null || defVal == System.Reflection.Missing.Value)
                        {
                            il.Emit(OpCodes.Ldnull);
                        }
                        else
                        {
                            EmitLoadConstant(il, defVal);
                            if (!pi.ParameterType.IsValueType && defVal.GetType().IsValueType)
                                il.Emit(OpCodes.Box, defVal.GetType());
                        }
                        il.Emit(OpCodes.Br_S, afterLoad);
                    }
                    else
                    {
                        // Not optional: throw exception.
                        ConstructorInfo exCtor = typeof(TargetParameterCountException)
                                                    .GetConstructor(Type.EmptyTypes);
                        il.Emit(OpCodes.Newobj, exCtor);
                        il.Emit(OpCodes.Throw);
                    }
                    il.MarkLabel(argProvided);

                    // Load the argument from the effective index.
                    il.Emit(OpCodes.Ldloc, argsLocal);
                    il.Emit(OpCodes.Ldloc, effectiveIndex);
                    il.Emit(OpCodes.Ldelem_Ref);

                    // Now, check if the loaded value is null.
                    Label notNull = il.DefineLabel();
                    il.Emit(OpCodes.Dup);         // duplicate the loaded argument for the test
                    il.Emit(OpCodes.Brtrue_S, notNull); // if not null, jump to notNull

                    // It is null: remove the null value.
                    il.Emit(OpCodes.Pop);
                    if (pi.IsOptional)
                    {
                        // Load the default value.
                        object defVal = pi.DefaultValue;
                        if (defVal == null || defVal == System.Reflection.Missing.Value)
                            il.Emit(OpCodes.Ldnull);
                        else
                        {
                            EmitLoadConstant(il, defVal);
                            if (!pi.ParameterType.IsValueType && defVal.GetType().IsValueType)
                                il.Emit(OpCodes.Box, defVal.GetType());
                        }
                        il.Emit(OpCodes.Br_S, afterLoad);
                    }
                    else
                    {
                        // If not optional, throw an exception.
                        ConstructorInfo exCtor2 = typeof(TargetParameterCountException)
                                                    .GetConstructor(Type.EmptyTypes);
                        il.Emit(OpCodes.Newobj, exCtor2);
                        il.Emit(OpCodes.Throw);
                    }
                    il.MarkLabel(notNull);

                    il.MarkLabel(afterLoad);
                }

                // Finally, if the formal parameter is a value type, unbox/cast as needed.
                if (pi.ParameterType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, pi.ParameterType);
                else
                    il.Emit(OpCodes.Castclass, pi.ParameterType);
            }

            // --- Call the underlying method ---
            if (method.IsStatic)
                il.Emit(OpCodes.Call, method);
            else
                il.Emit(OpCodes.Callvirt, method);
            if (method.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else if (method.ReturnType.IsValueType)
                il.Emit(OpCodes.Box, method.ReturnType);
            il.Emit(OpCodes.Ret);

            return (Func<object, object[], object>)dm.CreateDelegate(typeof(Func<object, object[], object>));
        }

        /// <summary>
        /// Creates a delegate of type Func<object, object[], object></object>that will call the given MethodInfo.
        /// It will check for missing parameters and if the parameter is optional, it uses its DefaultValue.
        /// </summary>
        public static Func<object, object[], object> CreateDelegate2(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            ParameterInfo[] parameters = method.GetParameters();
            // The dynamic method is associated with the declaring type (if available) so that non-public members can be accessed.
            DynamicMethod dm = new DynamicMethod(
                "DynamicInvoke_" + method.Name,
                typeof(object),
                new Type[] { typeof(object), typeof(object[]) },
                method.Module,
                true);

            ILGenerator il = dm.GetILGenerator();

            // If the method is an instance method, load and cast the first parameter (the instance).
            if (!method.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                if (method.DeclaringType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, method.DeclaringType);
                else
                    il.Emit(OpCodes.Castclass, method.DeclaringType);
            }


            if (method.Name == "set_Item" &&
			method.GetParameters().Length == 2 &&
			method.GetParameters()[0].ParameterType.IsArray)
			{
				// We expect the caller to supply a flattened argument array,
				// where the keys are in args[0..n-2] and the value is in args[n-1].
				// First, compute the number of key arguments: count = args.Length - 1.
				il.Emit(OpCodes.Ldarg_1);   // load args array
				il.Emit(OpCodes.Ldlen);     // get its length (native unsigned int)
				il.Emit(OpCodes.Conv_I4);   // convert to int32
				il.Emit(OpCodes.Ldc_I4_1);  // constant 1
				il.Emit(OpCodes.Sub);       // compute args.Length - 1
											// Allocate a new object[] for the keys.
				il.Emit(OpCodes.Newarr, typeof(object));
				// Store the new array in a local for later use.
				LocalBuilder keysArray = il.DeclareLocal(typeof(object[]));
				il.Emit(OpCodes.Dup);
				il.Emit(OpCodes.Stloc, keysArray);

				// Prepare a local for a loop index.
				LocalBuilder idx = il.DeclareLocal(typeof(int));
				il.Emit(OpCodes.Ldc_I4_0);
				il.Emit(OpCodes.Stloc, idx);

                // We'll need a label for the loop check.
                System.Reflection.Emit.Label loopCheck = il.DefineLabel();
                System.Reflection.Emit.Label loopBody = il.DefineLabel();
				il.Emit(OpCodes.Br_S, loopCheck);

				il.MarkLabel(loopBody);
				// For each index, copy from args[index] to keysArray[index].
				il.Emit(OpCodes.Ldloc, keysArray); // load keys array
				il.Emit(OpCodes.Ldloc, idx);       // index into keys array

				il.Emit(OpCodes.Ldarg_1);          // load caller's args array
				il.Emit(OpCodes.Ldloc, idx);       // use current index
				il.Emit(OpCodes.Ldelem_Ref);       // load args[idx]
				il.Emit(OpCodes.Stelem_Ref);       // store into keysArray[idx]

				// Increment index: idx++
				il.Emit(OpCodes.Ldloc, idx);
				il.Emit(OpCodes.Ldc_I4_1);
				il.Emit(OpCodes.Add);
				il.Emit(OpCodes.Stloc, idx);

				il.MarkLabel(loopCheck);
				// Compare idx with the length of keysArray.
				il.Emit(OpCodes.Ldloc, idx);
				il.Emit(OpCodes.Ldloc, keysArray);
				il.Emit(OpCodes.Ldlen);
				il.Emit(OpCodes.Conv_I4);
				il.Emit(OpCodes.Blt_S, loopBody);

				// Now load the keys array as the first argument.
				il.Emit(OpCodes.Ldloc, keysArray);
				// Then load the value: the last element of the original args array.
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldlen);
				il.Emit(OpCodes.Conv_I4);
				il.Emit(OpCodes.Ldc_I4_1);
				il.Emit(OpCodes.Sub);
				il.Emit(OpCodes.Ldelem_Ref);
			}
			else
			{

				// For each parameter, load the argument if present; otherwise use the default value if optional.
				for (int i = 0; i < parameters.Length; i++)
				{
					ParameterInfo pi = parameters[i];
					// Check if this parameter is the params parameter.
					bool isParams = (i == parameters.Length - 1) &&
									pi.IsDefined(typeof(ParamArrayAttribute), false) &&
									pi.ParameterType.IsArray;

					// Define labels for whether an argument was provided.
					System.Reflection.Emit.Label argPresent = il.DefineLabel();
					System.Reflection.Emit.Label argLoadDone = il.DefineLabel();

					// Load the length of the object[] args.
					il.Emit(OpCodes.Ldarg_1); // load args array
					il.Emit(OpCodes.Ldlen);   // get its length (as native unsigned int)
					il.Emit(OpCodes.Conv_I4); // convert to int32
					il.Emit(OpCodes.Ldc_I4, i); // load constant i
												// If args.Length > i, jump to argPresent.
					il.Emit(OpCodes.Bgt_S, argPresent);

					// No argument provided for this parameter.
					if (isParams)
					{
						// For a params parameter, create an empty array.
						// pi.ParameterType is an array type; get its element type.
						Type elementType = pi.ParameterType.GetElementType();
						il.Emit(OpCodes.Ldc_I4_0);      // array length 0
						il.Emit(OpCodes.Newarr, elementType);
					}
					else if (pi.IsOptional)
					{
						// Load the default value.
						object defaultValue = pi.DefaultValue;
						if (defaultValue == null || defaultValue == System.Reflection.Missing.Value)
						{
							il.Emit(OpCodes.Ldnull);
						}
						else
						{
							EmitLoadConstant(il, defaultValue);
							// If the parameter type is a reference type (like object) but the default constant is a value type, box it.
							if (!pi.ParameterType.IsValueType && defaultValue.GetType().IsValueType)
							{
								il.Emit(OpCodes.Box, defaultValue.GetType());
							}
						}
					}
					else
					{
						// Not enough arguments and parameter is not optional: throw.
						ConstructorInfo exCtor = typeof(TargetParameterCountException)
												 .GetConstructor(Type.EmptyTypes);
						il.Emit(OpCodes.Newobj, exCtor);
						il.Emit(OpCodes.Throw);
					}
					il.Emit(OpCodes.Br_S, argLoadDone);

					// Argument provided: load args[i]
					il.MarkLabel(argPresent);

					if (isParams)
					{
						// Compute remaining argument count: count = args.Length - i.
						LocalBuilder countLocal = il.DeclareLocal(typeof(int));
						il.Emit(OpCodes.Ldarg_1);    // args array
						il.Emit(OpCodes.Ldlen);      // get length
						il.Emit(OpCodes.Conv_I4);
						il.Emit(OpCodes.Ldc_I4, i);  // constant i
						il.Emit(OpCodes.Sub);
						il.Emit(OpCodes.Stloc, countLocal);

						// We'll use one of two strategies:
						// Strategy A: If count == 1 and the single argument is already assignable to the params array type, use it.
						// Strategy B: Otherwise, pack all remaining arguments into a new array.

						LocalBuilder singleArg = il.DeclareLocal(typeof(object));  // to hold the candidate argument
						System.Reflection.Emit.Label doPack = il.DefineLabel();
						System.Reflection.Emit.Label useProvided = il.DefineLabel();

						// Check: if (count != 1) goto doPack.
						il.Emit(OpCodes.Ldloc, countLocal);
						il.Emit(OpCodes.Ldc_I4_1);
						il.Emit(OpCodes.Bne_Un_S, doPack);

						// Otherwise, count == 1: load the single argument.
						il.Emit(OpCodes.Ldarg_1);
						il.Emit(OpCodes.Ldc_I4, i);
						il.Emit(OpCodes.Ldelem_Ref);
						il.Emit(OpCodes.Stloc, singleArg);

						// If singleArg is null, we cannot use it directly—so pack it.
						il.Emit(OpCodes.Ldloc, singleArg);
						System.Reflection.Emit.Label notNull = il.DefineLabel();
						il.Emit(OpCodes.Brtrue_S, notNull);
						il.Emit(OpCodes.Br_S, doPack);

						il.MarkLabel(notNull);
						// Check if singleArg is already an instance of the params type.
						il.Emit(OpCodes.Ldloc, singleArg);
						il.Emit(OpCodes.Isinst, pi.ParameterType); // pi.ParameterType is object[] (for example)
						il.Emit(OpCodes.Dup);
						// If the cast succeeded (non-null), use it.
						il.Emit(OpCodes.Brtrue_S, useProvided);
						// Otherwise, fall through to packing.
						il.Emit(OpCodes.Pop); // remove the null result from isinst
						il.MarkLabel(doPack);

						// ---- REPACK BLOCK: Pack remaining arguments into a new array.
						{
							// newArray = new T[count]
							Type elementType = pi.ParameterType.GetElementType();
							LocalBuilder newArray = il.DeclareLocal(pi.ParameterType);
							il.Emit(OpCodes.Ldloc, countLocal);
							il.Emit(OpCodes.Newarr, elementType);
							il.Emit(OpCodes.Stloc, newArray);

							// We'll need a loop index.
							LocalBuilder loopIndex = il.DeclareLocal(typeof(int));
							il.Emit(OpCodes.Ldc_I4_0);
							il.Emit(OpCodes.Stloc, loopIndex);

							System.Reflection.Emit.Label loopCheck = il.DefineLabel();
							System.Reflection.Emit.Label loopBody = il.DefineLabel();

							il.Emit(OpCodes.Br_S, loopCheck);
							il.MarkLabel(loopBody);
							// newArray[loopIndex] = args[i + loopIndex]
							il.Emit(OpCodes.Ldloc, newArray);       // load newArray
							il.Emit(OpCodes.Ldloc, loopIndex);        // index
							il.Emit(OpCodes.Ldarg_1);                 // args array
							il.Emit(OpCodes.Ldc_I4, i);               // base index i
							il.Emit(OpCodes.Ldloc, loopIndex);        // current loop index
							il.Emit(OpCodes.Add);                     // compute i + loopIndex
							il.Emit(OpCodes.Ldelem_Ref);              // load element at args[i+loopIndex]
							il.Emit(OpCodes.Stelem_Ref);              // newArray[loopIndex] = that element

							// Increment loopIndex.
							il.Emit(OpCodes.Ldloc, loopIndex);
							il.Emit(OpCodes.Ldc_I4_1);
							il.Emit(OpCodes.Add);
							il.Emit(OpCodes.Stloc, loopIndex);

							il.MarkLabel(loopCheck);
							il.Emit(OpCodes.Ldloc, loopIndex);
							il.Emit(OpCodes.Ldloc, countLocal);
							il.Emit(OpCodes.Blt_S, loopBody);

							// Finally load the new array onto the stack.
							il.Emit(OpCodes.Ldloc, newArray);
							il.Emit(OpCodes.Br_S, useProvided); // use the repacked array
						}
						il.MarkLabel(useProvided);
						// If we took the fast path, singleArg is already on the stack.
						// If we repacked, our new array is now on the stack.
					}
					else
					{

						il.Emit(OpCodes.Ldarg_1);
						il.Emit(OpCodes.Ldc_I4, i);
						il.Emit(OpCodes.Ldelem_Ref);

						// If the parameter is optional, check if the provided value is null.
						if (pi.IsOptional && pi.DefaultValue != null)
						{
							// Duplicate the value so we can test it without losing it.
							il.Emit(OpCodes.Dup);
							System.Reflection.Emit.Label nonNull = il.DefineLabel();
							il.Emit(OpCodes.Brtrue_S, nonNull);
							// If it’s null: pop the null value and load the default value.
							il.Emit(OpCodes.Pop);

							object defaultValue = pi.DefaultValue;
							EmitLoadConstant(il, defaultValue);
							if (!pi.ParameterType.IsValueType && defaultValue.GetType().IsValueType)
							{
								il.Emit(OpCodes.Box, defaultValue.GetType());
							}
							il.Emit(OpCodes.Br_S, argLoadDone);
							il.MarkLabel(nonNull);
						}
					}

					il.MarkLabel(argLoadDone);

					// Convert the argument to the parameter type.
					Type paramType = pi.ParameterType;
					if (paramType.IsValueType)
						il.Emit(OpCodes.Unbox_Any, paramType);
					else
						il.Emit(OpCodes.Castclass, paramType);
				}
			}

            // Call the method.
            if (method.IsStatic)
                il.Emit(OpCodes.Call, method);
            else
                il.Emit(OpCodes.Callvirt, method);

            // If the method's return type is void, load null; otherwise box the value type result.
            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if (method.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Box, method.ReturnType);
            }
            il.Emit(OpCodes.Ret);

            return (Func<object, object[], object>)dm.CreateDelegate(typeof(Func<object, object[], object>));
        }

        /// <summary>
        /// Creates a delegate from a PropertyInfo by using its getter method.
        /// </summary>
        public static Func<object, object[], object> CreateDelegate(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            MethodInfo getter = property.GetGetMethod(true);
            if (getter == null)
                throw new ArgumentException("The provided property does not have a getter.", nameof(property));

            return CreateDelegate(getter);
        }

        /// <summary>
        /// Emits IL to load the specified constant onto the evaluation stack.
        /// Supports common primitives, strings, booleans, and enums.
        /// </summary>
        private static void EmitLoadConstant(ILGenerator il, object value)
        {
            if (value == null)
            {
                il.Emit(OpCodes.Ldnull);
                return;
            }

            Type type = value.GetType();
            if (type == typeof(int))
            {
                il.Emit(OpCodes.Ldc_I4, (int)value);
            }
            else if (type == typeof(long))
            {
                il.Emit(OpCodes.Ldc_I8, (long)value);
            }
            else if (type == typeof(float))
            {
                il.Emit(OpCodes.Ldc_R4, (float)value);
            }
            else if (type == typeof(double))
            {
                il.Emit(OpCodes.Ldc_R8, (double)value);
            }
            else if (type == typeof(string))
            {
                il.Emit(OpCodes.Ldstr, (string)value);
            }
            else if (type == typeof(bool))
            {
                il.Emit((bool)value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            }
            else if (type.IsEnum)
            {
                // For enums, load the underlying value and box the enum type.
                Type underlying = Enum.GetUnderlyingType(type);
                if (underlying == typeof(int))
                {
                    il.Emit(OpCodes.Ldc_I4, (int)value);
                }
                else if (underlying == typeof(long))
                {
                    il.Emit(OpCodes.Ldc_I8, (long)value);
                }
                else
                {
                    throw new NotSupportedException("Enum underlying type not supported: " + underlying);
                }
                il.Emit(OpCodes.Box, type);
            }
            else if (type == typeof(decimal))
            {
                // Decompose the decimal into its 4 int bits.
                decimal d = (decimal)value;
                int[] bits = decimal.GetBits(d);
                int lo = bits[0];
                int mid = bits[1];
                int hi = bits[2];
                int flags = bits[3];
                bool isNegative = (flags & 0x80000000) != 0;
                byte scale = (byte)((flags >> 16) & 0x7F);

                // Load the constants and call the decimal constructor (int lo, int mid, int hi, bool isNegative, byte scale)
                il.Emit(OpCodes.Ldc_I4, lo);
                il.Emit(OpCodes.Ldc_I4, mid);
                il.Emit(OpCodes.Ldc_I4, hi);
                il.Emit(isNegative ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Conv_U1); // Convert the top int to a byte for scale

                ConstructorInfo ctor = typeof(decimal).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte) });
                if (ctor == null)
                    throw new InvalidOperationException("The required Decimal constructor was not found.");
                il.Emit(OpCodes.Newobj, ctor);
            }
            else
            {
                throw new NotSupportedException("Constant type not supported: " + type.FullName);
            }
        }
    }

}