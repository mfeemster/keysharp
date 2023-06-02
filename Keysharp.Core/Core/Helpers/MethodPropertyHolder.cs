using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Core.Common.Keyboard;

namespace Keysharp.Core
{
	public class ConcurrentStackPool<T>
	{
		private readonly SlimStack<T[]> collection = new (16); //Unlikely there would ever be more than 16 threads calling a given function at the same time before any of the others returned.
		private readonly int exactSize;
		private readonly object sync = new object();

		public ConcurrentStackPool(int size)
		{
			exactSize = size;
		}

		public T[] Rent()
		{
			return collection.TryPop(out var obj) ? obj : (new T[exactSize]);
		}

		public void Return(T[] array, bool clearArray = false)
		{
			if (clearArray)
				System.Array.Clear(array);

			collection.Push(array);
		}
	}

	public class ComMethodPropertyHolder : MethodPropertyHolder
	{
		public string Name { get; private set; }

		public ComMethodPropertyHolder(string name)
			: base(null, null)
		{
			Name = name;
			callFunc = (inst, obj) =>
			{
				return inst.GetType().InvokeMember(Name, BindingFlags.InvokeMethod, null, inst, obj);
			};
		}
	}

	public class MethodPropertyHolder
	{
		internal Func<object, object[], object> callFunc;
		internal readonly MethodInfo mi;
		internal readonly ParameterInfo[] parameters;
		internal readonly PropertyInfo pi;
		internal readonly Action<object, object> setPropFunc;
		protected readonly ConcurrentStackPool<object> paramsPool;
		private readonly bool isGuiType;
		private int paramLength;
		private int startVarIndex = -1;
		private int stopVarIndexDistanceFromEnd;

		internal bool IsStaticFunc { get; private set; }
		internal bool IsStaticProp { get; private set; }

		public MethodPropertyHolder(MethodInfo m, PropertyInfo p)
		{
			mi = m;
			pi = p;

			if (mi != null)
			{
				parameters = mi.GetParameters();
				paramLength = parameters.Length;
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
				else if (paramLength == 0)
				{
					if (isGuiType)
					{
						callFunc = (inst, obj) =>
						{
							object ret = null;

							if (inst.GetControl() is Control ctrl)//If it's a gui control, then invoke on the gui thread.
								_ = ctrl.CheckedInvoke(() => ret = mi.Invoke(inst, null), false);

							return ret;
						};
					}
					else
						callFunc = (inst, obj) => mi.Invoke(inst, null);
				}
				else
				{
					paramsPool = new ConcurrentStackPool<object>(paramLength);

					if (startVarIndex != -1)//Variadic.
					{
						callFunc = (inst, obj) =>
						{
							object[] newobj = null;
							object[] lastArr = null;

							if (paramLength == 1)
							{
								newobj = new object[] { obj };
							}
							else
							{
								//The slowest case: a function is trying to be called with a different number of parameters than it actually has, or it's variadic, so manually create an array of parameters that matches the required size.
								var oi = 0;
								var objLength = obj.Length;
								newobj = paramsPool.Rent();

								for (var pi = 0; oi < objLength && pi < paramLength; pi++)
								{
									if (pi == startVarIndex)
									{
										var om1 = (objLength - startVarIndex) - stopVarIndexDistanceFromEnd;
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
								ret = mi.Invoke(inst, newobj);
							else if (inst.GetControl() is Control ctrl)//If it's a gui control, then invoke on the gui thread.
								_ = ctrl.CheckedInvoke(() => ret = mi.Invoke(inst, newobj), false);

							if (paramLength > 1)
								paramsPool.Return(newobj, true);

							return ret;
						};
					}
					else
					{
						callFunc = (inst, obj) =>
						{
							object ret = null;
							var objLength = obj.Length;

							if (paramLength == objLength)
							{
								if (!isGuiType)
									ret = mi.Invoke(inst, obj);
								else if (inst.GetControl() is Control ctrl)//If it's a gui control, then invoke on the gui thread.
									_ = ctrl.CheckedInvoke(() => ret = mi.Invoke(inst, obj), false);
							}
							else
							{
								var i = 0;//The slower case: a function is trying to be called with a different number of parameters than it actually has, so manually create an array of parameters that matches the required size.
								//var newobj = new object[paramLength];
								var newobj = paramsPool.Rent();//Using the memory pool in this function seems to help a lot.

								for (; i < objLength && i < paramLength; i++)
									newobj[i] = obj[i];

								//Any remaining items in newobj are null by default.
								if (!isGuiType)
									ret = mi.Invoke(inst, newobj);
								else if (inst.GetControl() is Control ctrl)//If it's a gui control, then invoke on the gui thread.
									_ = ctrl.CheckedInvoke(() => ret = mi.Invoke(inst, newobj), false);

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

				if (pi.GetAccessors().Any(x => x.IsStatic))
				{
					IsStaticProp = true;

					if (isGuiType)
					{
						callFunc = (inst, obj) =>//Gui calls aren't worth optimizing further.
						{
							object ret = null;

							if (inst.GetControl() is Control ctrl)//If it's a gui control, then invoke on the gui thread.
								_ = ctrl.CheckedInvoke(() => ret = pi.GetValue(null), false);

							if (ret is int i)
								ret = (long)i;//Try to keep everything as long.

							return ret;
						};
						setPropFunc = (inst, obj) =>
						{
							if (inst.GetControl() is Control ctrl)//If it's a gui control, then invoke on the gui thread.
								ctrl.CheckedInvoke(() => pi.SetValue(null, obj), false);
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

							if (inst.GetControl() is Control ctrl)//If it's a gui control, then invoke on the gui thread.
								ctrl.CheckedInvoke(() => ret = pi.GetValue(inst), false);

							if (ret is int i)
								ret = (long)i;//Try to keep everything as long.

							return ret;
						};
						setPropFunc = (inst, obj) =>
						{
							if (inst.GetControl() is Control ctrl)//If it's a gui control, then invoke on the gui thread.
								ctrl.CheckedInvoke(() => pi.SetValue(inst, obj), false);
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

	/// <summary>
	/// This class is means to be a highly optimized and stripped down version of the built in Stack collection type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SlimStack<T>
	{
		private readonly List<T> list;
		private readonly int size;
		private int index;

		public SlimStack(int s)
		{
			index = 0;
			size = s;
			list = new List<T>(size);

			for (var i = 0; i < size; i++)
				list.Add(default);
		}

		public bool Push(T obj)
		{
			var next = Interlocked.Increment(ref index);

			if (next > 0 && next <= size)
			{
				list[next - 1] = obj;
				return true;
			}
			else
				_ = Interlocked.Decrement(ref index);//Went too far up, so bump back down.

			return false;//No room, so just don't return the object and let the GC handle it.
		}

		public bool TryPop(out T obj)
		{
			var next = Interlocked.Decrement(ref index);

			if (next >= 0 && next < size)
			{
				obj = list[next];
				return true;
			}
			else
				_ = Interlocked.Increment(ref index);//Went too far down, so bump back up.

			obj = default;
			return false;
		}
	}
}