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

        internal static ConcurrentDictionary<MethodInfo, Func<object, object[], object>> delegateCache = new();

		internal bool IsStaticFunc { get; private set; }
		internal bool IsStaticProp { get; private set; }
		internal bool IsBind { get; private set; }
		internal bool IsVariadic => startVarIndex != -1;
		internal int ParamLength { get; }
		internal int MinParams = 0;
		internal int MaxParams = 9999;

		internal MethodPropertyHolder(MethodInfo m, PropertyInfo p)
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
					var pmi = parameters[i];
					if (pmi.ParameterType == typeof(object[]))
						startVarIndex = i;
					else if (startVarIndex != -1 && stopVarIndexDistanceFromEnd == 0)
						stopVarIndexDistanceFromEnd = parameters.Length - i;

					if (!(pmi.IsOptional || pmi.IsVariadic() || pmi.ParameterType == typeof(object[])))
						MinParams++;
				}
				if (startVarIndex == -1)
					MaxParams = parameters.Length;

				IsStaticFunc = mi.Attributes.HasFlag(MethodAttributes.Static);
				var isFuncObj = typeof(IFuncObj).IsAssignableFrom(mi.DeclaringType);

				if (isFuncObj && mi.Name == "Bind")
					IsBind = true;

				if (isFuncObj && mi.Name == "Call")
				{
					callFunc = (inst, obj) => ((IFuncObj)inst).Call(obj);
				} else
				{
                    var del = delegateCache.GetOrAdd(mi, key => DelegateFactory.CreateDelegate(mi));

					if (isGuiType)
                    {
                        callFunc = (inst, obj) =>
                        {
                            var ctrl = (inst ?? obj[0]).GetControl();
                            object ret = null;
                            ctrl.CheckedInvoke(() =>
                            {
                                ret = del(inst, obj);
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
								ret = mi.Invoke(inst, null);
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
									ret = mi.Invoke(inst, newobj);
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
										ret = mi.Invoke(inst, obj);
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
										ret = mi.Invoke(inst, newobj);
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
				MinParams = MaxParams = ParamLength;

				if (pi.GetAccessors().Any(x => x.IsStatic))
				{
					IsStaticProp = true;

					if (isGuiType)
					{
						callFunc = (inst, obj) =>//Gui calls aren't worth optimizing further.
						{
							object ret = null;
							var ctrl = (inst ?? obj[0]).GetControl();//If it's a gui control, then invoke on the gui thread.
							ctrl.CheckedInvoke(() =>
							{
								ret = pi.GetValue(null);
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
								pi.SetValue(null, obj);
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
							var ctrl = (inst ?? obj[0]).GetControl();//If it's a gui control, then invoke on the gui thread.
							ctrl.CheckedInvoke(() =>
							{
								ret = pi.GetValue(inst ?? obj[0]);
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
								pi.SetValue(inst, obj);
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

            string GenerateMethodInfoCacheKey()
            {
                var sb = new StringBuilder();

                sb.Append(mi.DeclaringType.FullName);
                sb.Append("+");
                sb.Append(mi.Name);
                sb.Append("<");

                // For methods, use the return type and parameter list.
                sb.Append(mi.ReturnType.FullName);
                foreach (var param in mi.GetParameters())
                {
                    sb.Append(",").Append(param.ParameterType.FullName);
                    if (param.HasDefaultValue)
                    {
                        // Append the default value or "null" if it's null.
                        sb.Append("=").Append(param.DefaultValue == null ? "null" : param.DefaultValue.ToString());
                    }
                    else
                    {
                        sb.Append("=NoDefault");
                    }
                }
                sb.Append(">");

                return sb.ToString();
            }
        }

		internal static void ClearCache()
		{
			delegateCache.Clear();
		}
	}
    class ArgumentError : Error
    {
        public ArgumentError()
            : base(new TargetParameterCountException().Message)
        {
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

            string dynamicMethodName = "DynamicInvoke_" + (method.DeclaringType != null ? method.DeclaringType.Name + "." : "") + method.Name;

            DynamicMethod dm = new DynamicMethod(
                dynamicMethodName,
                typeof(object),
                new Type[] { typeof(object), typeof(object[]) },
                method.Module,
                true);

            ILGenerator il = dm.GetILGenerator();

            // --- Declare unified locals ---
            LocalBuilder paramOffset = il.DeclareLocal(typeof(int));   // will be 0 for static or for instance when ldarg0 is non-null; 1 for instance if ldarg0 is null.
            LocalBuilder argsLocal = il.DeclareLocal(typeof(object[]));  // the effective arguments array

            // Ensure that the caller-supplied argument array is not null.
            Label argsNonNull = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_1);        // load ldarg1
            il.Emit(OpCodes.Brtrue_S, argsNonNull); // if not null, branch
                                                    // Otherwise, create an empty object array.
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Starg_S, 1);       // store the empty array into ldarg1 (or into a local)
            il.MarkLabel(argsNonNull);

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
            bool isSetter = method.Name.StartsWith("set_") || method.Name.StartsWith(Keywords.ClassStaticPrefix + "set_");
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
                il.Emit(OpCodes.Newobj, typeof(ArgumentError)
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
                    il.Emit(OpCodes.Newobj, typeof(ArgumentError)
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

                if (isSetter)
                    requiredCount--; // Allow value to be null

                // Check that availableCountLocal >= requiredCount.
                il.Emit(OpCodes.Ldloc, availableCountLocal);
                il.Emit(OpCodes.Ldc_I4, requiredCount);
                Label normalOkLabel = il.DefineLabel();
                il.Emit(OpCodes.Bge_S, normalOkLabel);
                il.Emit(OpCodes.Newobj, typeof(ArgumentError)
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
                    il.Emit(OpCodes.Newobj, typeof(ArgumentError)
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
                    if (pi.IsOptional || (isSetter && i == (parameters.Length - 1)))
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
                        ConstructorInfo exCtor = typeof(ArgumentError)
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
                    if (pi.IsOptional || (isSetter && i == (parameters.Length - 1)))
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
                        ConstructorInfo exCtor2 = typeof(ArgumentError)
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

        public static void EmitThrowError(ILGenerator il, string errorMessage, string methodName, LocalBuilder code)
        {
            // Create a new object[3]
            il.Emit(OpCodes.Ldc_I4_3);                  // push constant 3 (array size)
            il.Emit(OpCodes.Newarr, typeof(object));    // create new object[3]

            // Store errorMessage at index 0
            il.Emit(OpCodes.Dup);                       // duplicate array reference
            il.Emit(OpCodes.Ldc_I4_0);                  // index 0
            il.Emit(OpCodes.Ldstr, errorMessage);       // push errorMessage string
            il.Emit(OpCodes.Stelem_Ref);                // array[0] = errorMessage

            
            // Store methodName at index 1
            il.Emit(OpCodes.Dup);                       // duplicate array reference
            il.Emit(OpCodes.Ldc_I4_1);                  // index 1
            il.Emit(OpCodes.Ldstr, methodName);         // push methodName string
            il.Emit(OpCodes.Stelem_Ref);                // array[1] = methodName


            // Store code at index 2
            il.Emit(OpCodes.Dup);                       // duplicate array reference
            il.Emit(OpCodes.Ldc_I4_2);                  // index 2
            il.Emit(OpCodes.Ldloc, code);               // push code (int)
            il.Emit(OpCodes.Box, typeof(int));          // box the int value
            il.Emit(OpCodes.Stelem_Ref);                // array[2] = code

            // Get the constructor: Error(object[] args)
            ConstructorInfo errorCtor = typeof(Error).GetConstructor(new[] { typeof(object[]) });
            // Create a new Error instance using the array
            il.Emit(OpCodes.Newobj, errorCtor);
            // Throw the error.
            il.Emit(OpCodes.Throw);
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
            else if (type == typeof(DBNull))
            {
                //FieldInfo dbNullField = typeof(DBNull).GetField("Value", BindingFlags.Public | BindingFlags.Static);
                //il.Emit(OpCodes.Ldsfld, dbNullField);
                il.Emit(OpCodes.Ldnull);
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