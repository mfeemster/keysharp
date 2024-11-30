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
		private readonly int startVarIndex = -1;
		private readonly int stopVarIndexDistanceFromEnd;

		internal bool IsStaticFunc { get; private set; }
		internal bool IsStaticProp { get; private set; }
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

				for (var i = 0; i < parameters.Length; i++)
				{
					if (parameters[i].ParameterType == typeof(object[]))
						startVarIndex = i;
					else if (startVarIndex != -1 && stopVarIndexDistanceFromEnd == 0)
						stopVarIndexDistanceFromEnd = parameters.Length - i;
				}

				IsStaticFunc = mi.Attributes.HasFlag(MethodAttributes.Static);

				if (typeof(IFuncObj).IsAssignableFrom(mi.DeclaringType) && mi.Name == "Call")
				{
					callFunc = (inst, obj) => ((IFuncObj)inst).Call(obj);
				}
				else if (ParamLength == 0)
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
								newobj = [obj];
							}
							else
							{
								//The slowest case: a function is trying to be called with a different number of parameters than it actually has, or it's variadic, so manually create an array of parameters that matches the required size.
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

								for (; pi < ParamLength; pi++)
									if (parameters[pi].IsOptional)
										newobj[pi] = parameters[pi].DefaultValue;
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
								for (var i = 0; i < ParamLength; i++)
									if (obj[i] == null && parameters[i].IsOptional)
										obj[i] = parameters[i].DefaultValue;

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
								var i = 0;//The slower case: a function is trying to be called with a different number of parameters than it actually has, so manually create an array of parameters that matches the required size.
								var newobj = paramsPool.Rent();//Using the memory pool in this function seems to help a lot.

								for (; i < objLength && i < ParamLength; i++)
									if (obj[i] == null && parameters[i].IsOptional)
										newobj[i] = parameters[i].DefaultValue;
									else
										newobj[i] = obj[i];

								for (; i < ParamLength; i++)
									if (parameters[i].IsOptional)
										newobj[i] = parameters[i].DefaultValue;

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
}