namespace Keysharp.Core
{
	public class ComMethodPropertyHolder : MethodPropertyHolder
	{
		public string Name { get; private set; }

		public ComMethodPropertyHolder(string name)
			: base(null, null)
		{
			Name = name;
			callFunc = (inst, obj) =>
			{
				var t = inst.GetType();
				var m = t.GetMember(Name);
				return inst.GetType().InvokeMember(Name, BindingFlags.InvokeMethod, null, inst, obj);
			};
		}
	}

	public class MethodPropertyHolder
	{
		public Func<object, object[], object> callFunc;
		internal readonly MethodInfo mi;
		internal readonly ParameterInfo[] parameters;
		internal readonly PropertyInfo pi;
		internal readonly Action<object, object> setPropFunc;
		protected readonly ConcurrentStackArrayPool<object> paramsPool;
		private readonly bool isGuiType;
		private int startVarIndex = -1;
		private int stopVarIndexDistanceFromEnd;

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
				isGuiType = Keysharp.Core.Gui.IsGuiType(mi.DeclaringType);

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
								var oldHandle = Keysharp.Scripting.Script.HwndLastUsed;

								if (ctrl != null && ctrl.FindForm() is Form form)
									Keysharp.Scripting.Script.HwndLastUsed = form.Handle;

								ret = mi.Invoke(inst, null);
								Keysharp.Scripting.Script.HwndLastUsed = oldHandle;
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

							if (ParamLength == 1)
							{
								newobj = [obj];
							}
							else
							{
								//The slowest case: a function is trying to be called with a different number of parameters than it actually has, or it's variadic, so manually create an array of parameters that matches the required size.
								var oi = 0;
								var objLength = obj.Length;
								newobj = paramsPool.Rent();

								for (var pi = 0; oi < objLength && pi < ParamLength; pi++)
								{
									if (pi == startVarIndex)
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
									else
										newobj[pi] = obj[oi++];
								}
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
									var oldHandle = Keysharp.Scripting.Script.HwndLastUsed;

									if (ctrl != null && ctrl.FindForm() is Form form)
										Keysharp.Scripting.Script.HwndLastUsed = form.Handle;

									ret = mi.Invoke(inst, newobj);
									Keysharp.Scripting.Script.HwndLastUsed = oldHandle;
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

											for (var vi = 0; pi < obj.Length && vi < arr.Length; pi++, vi++)
												obj[pi] = arr[vi];
										}
										else
											obj[pi] = newobj[pi];
									}
								}
								else
									System.Array.Copy(newobj, obj, len);

								paramsPool.Return(newobj, true);
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
								if (!isGuiType)
								{
									ret = mi.Invoke(inst, obj);
								}
								else//If it's a gui control, then invoke on the gui thread.
								{
									var ctrl = inst.GetControl();
									ctrl.CheckedInvoke(() =>
									{
										var oldHandle = Keysharp.Scripting.Script.HwndLastUsed;

										if (ctrl != null && ctrl.FindForm() is Form form)
											Keysharp.Scripting.Script.HwndLastUsed = form.Handle;

										ret = mi.Invoke(inst, obj);
										Keysharp.Scripting.Script.HwndLastUsed = oldHandle;
									}, true);//This can be null if called before a Gui object is fully initialized.
								}
							}
							else
							{
								var i = 0;//The slower case: a function is trying to be called with a different number of parameters than it actually has, so manually create an array of parameters that matches the required size.
								//var newobj = new object[paramLength];
								var newobj = paramsPool.Rent();//Using the memory pool in this function seems to help a lot.

								for (; i < objLength && i < ParamLength; i++)
									newobj[i] = obj[i];

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
										var oldHandle = Keysharp.Scripting.Script.HwndLastUsed;

										if (ctrl != null && ctrl.FindForm() is Form form)
											Keysharp.Scripting.Script.HwndLastUsed = form.Handle;

										ret = mi.Invoke(inst, newobj);
										Keysharp.Scripting.Script.HwndLastUsed = oldHandle;
									}, true);//This can be null if called before a Gui object is fully initialized.
								}

								System.Array.Copy(newobj, obj, Math.Min(newobj.Length, objLength));//In case any params were references.
								paramsPool.Return(newobj, true);
							}

							return ret;
						};
					}
				}
			}
			else if (pi != null)
			{
				isGuiType = Keysharp.Core.Gui.IsGuiType(pi.DeclaringType);
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
								var oldHandle = Keysharp.Scripting.Script.HwndLastUsed;

								if (ctrl != null && ctrl.FindForm() is Form form)
									Keysharp.Scripting.Script.HwndLastUsed = form.Handle;

								ret = pi.GetValue(null);
								Keysharp.Scripting.Script.HwndLastUsed = oldHandle;
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
								var oldHandle = Keysharp.Scripting.Script.HwndLastUsed;

								if (ctrl != null && ctrl.FindForm() is Form form)
									Keysharp.Scripting.Script.HwndLastUsed = form.Handle;

								pi.SetValue(null, obj);
								Keysharp.Scripting.Script.HwndLastUsed = oldHandle;
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
								var oldHandle = Keysharp.Scripting.Script.HwndLastUsed;

								if (ctrl != null && ctrl.FindForm() is Form form)
									Keysharp.Scripting.Script.HwndLastUsed = form.Handle;

								ret = pi.GetValue(inst);
								Keysharp.Scripting.Script.HwndLastUsed = oldHandle;
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
								var oldHandle = Keysharp.Scripting.Script.HwndLastUsed;

								if (ctrl != null && ctrl.FindForm() is Form form)
									Keysharp.Scripting.Script.HwndLastUsed = form.Handle;

								pi.SetValue(inst, obj);
								Keysharp.Scripting.Script.HwndLastUsed = oldHandle;
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