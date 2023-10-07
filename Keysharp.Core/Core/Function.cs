using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Keysharp.Core
{
	public interface IFuncObj
	{
		public object Inst { get; }
		public bool IsValid { get; }
		public string Name { get; }

		public object Call(params object[] obj);

		public object CallWithRefs(params object[] obj);
	}

	public static class Function
	{
		public static FuncObj Func(object obj, object obj1 = null, object obj2 = null)
		{
			var name = obj.As();
			var fo = new FuncObj(name, obj1, obj2);
			return fo.Name != "" ? fo
				   : throw new MethodError($"Unable to retrieve method {name}.");
		}

		public static FuncObj GetMethod(object obj0, object obj1 = null, object obj2 = null)
		{
			var val = obj0;
			var name = obj1.As();
			var paramCount = obj2.Ai(-1);
			var mph = Reflections.FindAndCacheMethod(val.GetType(), name.Length > 0 ? name : "Call", paramCount);
			return mph != null && mph.mi != null ? new FuncObj(mph.mi, val)
				   : throw new MethodError($"Unable to retrieve method {name} from object of type {val.GetType()} with parameter count {paramCount}.");
		}

		public static long HasMethod(object obj0, object obj1 = null, object obj2 = null)
		{
			var val = obj0;
			var name = obj1.As();
			var paramCount = obj2.Ai(-1);
			var mph = Reflections.FindAndCacheMethod(val.GetType(), name.Length > 0 ? name : "Call", paramCount);
			return mph != null && mph.mi != null ? 1L : 0L;
		}

		public static long HasProp(object obj0, object obj1, object obj2 = null)
		{
			var val = obj0;
			var name = obj1.As();
			var paramCount = obj2.Ai(-1);

			if (obj0 is KeysharpObject kso)
			{
				if (kso.op != null && kso.op.ContainsKey(name))
					return 1L;

				if (obj0 is Keysharp.Core.Map map)
					if (map.map.ContainsKey(name))
						return 1L;
			}

			var mph = Reflections.FindAndCacheProperty(val.GetType(), name, paramCount);
			return mph != null && mph.pi != null ? 1L : 0L;
		}

		public static BoundFunc ObjBindMethod(object obj0, object obj1, object obj2 = null, params object[] obj)
		{
			var o = obj0;
			var paramCount = obj1.Ai(-1);
			var name = obj2.As();
			object[] args = obj;

			if (Reflections.FindAndCacheMethod(o.GetType(), name, paramCount) is MethodPropertyHolder mph && mph.mi != null)
				return new BoundFunc(mph.mi, args, o);

			return null;
		}

		public static string ObjGetBase(object obj) => obj.GetType().BaseType.Name;

		internal static IFuncObj GetFuncObj(object h, object eventObj, bool throwIfBad = false)
		{
			IFuncObj del = null;

			if (h is string s)
			{
				if (s.Length > 0)
				{
					var tempdel = new FuncObj(s, eventObj);

					if (tempdel.IsValid)
						del = tempdel;
					else if (throwIfBad)
						throw new MethodError($"Unable to retrieve method {s} when creating a function object.");
				}//Empty string will just return null, which is a valid value in some cases.
			}
			else if (h is IFuncObj fo)
				del = fo;
			else if (throwIfBad)
				throw new TypeError($"Improper value of {h} was supplied for a function object.");

			return del;
		}
	}

	public class BoundFunc : FuncObj
	{
		internal object[] boundargs;

		public BoundFunc(MethodInfo m, object[] ba, object o = null)
			: base(m, o)
		{
			boundargs = ba;
		}

		public override object Call(params object[] obj) => base.Call(CreateArgs(obj).ToArray());

		public override object CallWithRefs(params object[] obj)
		{
			var argsList = CreateArgs(obj);
			var refs = new List<RefHolder>(obj.Length);

			for (var i = 0; i < argsList.Count; i++)
			{
				if (argsList[i] is RefHolder rh)
				{
					rh.index = i;//Must change the index since the array has changed.
					refs.Add(rh);
					argsList[i] = rh.val;
				}
			}

			var argsArray = argsList.ToArray();
			var val = base.Call(argsArray);

			for (var i = 0; i < refs.Count; i++)
			{
				var rh = refs[i];
				rh.reassign(argsArray[rh.index]);//Use value from new array.
			}

			return val;
		}

		private List<object> CreateArgs(params object[] obj)
		{
			int i = 0, argsused = 0;
			var argsList = new List<object>(mph.parameters.Length);

			for (; i < boundargs.Length; i++)
			{
				if (boundargs[i] != null)
				{
					argsList.Add(boundargs[i]);
				}
				else if (argsused < obj.Length)
				{
					argsList.Add(obj[argsused]);
					argsused++;
				}
				else
					argsList.Add(null);
			}

			if (mph.IsVariadic)
			{
				for (; argsused < obj.Length; argsused++)
					argsList.Add(obj[argsused]);
			}
			else
			{
				for (; argsused < obj.Length && argsused < mph.parameters.Length; argsused++)
					argsList.Add(obj[argsused]);
			}

			while (argsList.Count < mph.parameters.Length)
			{
				var param = mph.parameters[argsList.Count];

				if (param.Attributes.HasFlag(ParameterAttributes.HasDefault))
					argsList.Add(param.DefaultValue);
				else
					argsList.Add(null);
			}

			return argsList;
		}
	}

	public class DelegateHolder
	{
		public PlaceholderFunction delRef;
		internal IFuncObj funcObj;
		protected readonly ConcurrentStackPool<IntPtr> paramsPool = new ConcurrentStackPool<IntPtr>(31);
		private readonly bool fast;
		private readonly bool reference;

		public DelegateHolder(object obj, bool f, bool r)
		{
			funcObj = Function.GetFuncObj(obj, null, true);
			fast = f;
			reference = r;
			delRef = (PlaceholderFunction)Delegate.CreateDelegate(typeof(PlaceholderFunction), this, "DelegatePlaceholder");
		}

		public long DelegatePlaceholder(IntPtr p1 = new IntPtr(), IntPtr p2 = new IntPtr(), IntPtr p3 = new IntPtr(), IntPtr p4 = new IntPtr(), IntPtr p5 = new IntPtr(), IntPtr p6 = new IntPtr(), IntPtr p7 = new IntPtr(), IntPtr p8 = new IntPtr(),
		IntPtr p9 = new IntPtr(), IntPtr p10 = new IntPtr(), IntPtr p11 = new IntPtr(), IntPtr p12 = new IntPtr(), IntPtr p13 = new IntPtr(), IntPtr p14 = new IntPtr(), IntPtr p15 = new IntPtr(), IntPtr p16 = new IntPtr(),
		IntPtr p17 = new IntPtr(), IntPtr p18 = new IntPtr(), IntPtr p19 = new IntPtr(), IntPtr p20 = new IntPtr(), IntPtr p21 = new IntPtr(), IntPtr p22 = new IntPtr(), IntPtr p23 = new IntPtr(), IntPtr p24 = new IntPtr(),
		IntPtr p25 = new IntPtr(), IntPtr p26 = new IntPtr(), IntPtr p27 = new IntPtr(), IntPtr p28 = new IntPtr(), IntPtr p29 = new IntPtr(), IntPtr p30 = new IntPtr(), IntPtr p31 = new IntPtr())

		{
			object val = null;

			//Need to figure out how fast/slow works with threads here.//TODO
			if (delRef != null)
			{
				if (reference)
				{
					var arr = paramsPool.Rent();
					arr[0] = p1;
					arr[1] = p2;
					arr[2] = p3;
					arr[3] = p4;
					arr[4] = p5;
					arr[5] = p6;
					arr[6] = p7;
					arr[7] = p8;
					arr[8] = p9;
					arr[9] = p10;
					arr[10] = p11;
					arr[11] = p12;
					arr[12] = p13;
					arr[13] = p14;
					arr[14] = p15;
					arr[15] = p16;
					arr[16] = p17;
					arr[17] = p18;
					arr[18] = p19;
					arr[19] = p20;
					arr[20] = p21;
					arr[21] = p22;
					arr[22] = p23;
					arr[23] = p24;
					arr[24] = p25;
					arr[25] = p26;
					arr[26] = p27;
					arr[27] = p28;
					arr[28] = p29;
					arr[29] = p30;
					arr[30] = p31;
					val = DelegatePlaceholderArr(arr);
					paramsPool.Return(arr);
				}
				else
					val = funcObj.Call(p1, p2, p3, p4, p5, p6, p7, p8,
									   p9, p10, p11, p12, p13, p14, p15, p16,
									   p17, p18, p19, p20, p21, p22, p23, p24,
									   p25, p26, p27, p28, p29, p30, p31);
			}

			if (val is int i)
				return i;
			else if (val is long l)
				return (int)l;
			else if (val is bool b)
				return b ? 1 : 0;
			else if (val is double d)
				return (int)d;
			else if (val is string s && s.Length == 0)
				return 0L;
			else if (val is null)
				return 0L;

			return 0L;
		}

		public long DelegatePlaceholderArr(IntPtr[] arr)
		{
			object val = null;

			if (delRef != null)
			{
				if (reference)
				{
					unsafe
					{
						fixed (IntPtr* pin = &arr[0])
						{
							var ptr = new IntPtr((long*)pin);
							val = funcObj.Call(ptr.ToInt64());
						}
					}
				}
				else
					val = delRef.Invoke(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5],
										arr[6], arr[7], arr[8], arr[9], arr[10], arr[11],
										arr[12], arr[13], arr[14], arr[15], arr[16], arr[17],
										arr[18], arr[19], arr[20], arr[21], arr[22], arr[23],
										arr[24], arr[25], arr[26], arr[27], arr[28], arr[29],
										arr[30]);
			}

			if (val is int i)
				return i;
			else if (val is long l)
				return (int)l;
			else if (val is bool b)
				return b ? 1 : 0;
			else if (val is double d)
				return (int)d;
			else if (val is string s && s.Length == 0)
				return 0L;
			else if (val is null)
				return 0L;

			return 0L;
		}

		internal void Clear()
		{
			delRef = null;
			funcObj = null;
		}

		public delegate long PlaceholderFunction(IntPtr p1 = new IntPtr(), IntPtr p2 = new IntPtr(), IntPtr p3 = new IntPtr(), IntPtr p4 = new IntPtr(), IntPtr p5 = new IntPtr(), IntPtr p6 = new IntPtr(), IntPtr p7 = new IntPtr(), IntPtr p8 = new IntPtr(),
		IntPtr p9 = new IntPtr(), IntPtr p10 = new IntPtr(), IntPtr p11 = new IntPtr(), IntPtr p12 = new IntPtr(), IntPtr p13 = new IntPtr(), IntPtr p14 = new IntPtr(), IntPtr p15 = new IntPtr(), IntPtr p16 = new IntPtr(),
		IntPtr p17 = new IntPtr(), IntPtr p18 = new IntPtr(), IntPtr p19 = new IntPtr(), IntPtr p20 = new IntPtr(), IntPtr p21 = new IntPtr(), IntPtr p22 = new IntPtr(), IntPtr p23 = new IntPtr(), IntPtr p24 = new IntPtr(),
		IntPtr p25 = new IntPtr(), IntPtr p26 = new IntPtr(), IntPtr p27 = new IntPtr(), IntPtr p28 = new IntPtr(), IntPtr p29 = new IntPtr(), IntPtr p30 = new IntPtr(), IntPtr p31 = new IntPtr());
	}

	public class FuncObj : KeysharpObject, IFuncObj
	{
		protected bool anyRef;
		protected object inst;
		protected bool isVariadic;
		protected MethodInfo mi;
		protected MethodPropertyHolder mph;
		public object Inst => inst;
		public bool IsBuiltIn => mi.DeclaringType.Module.Name.StartsWith("keysharp.core", StringComparison.OrdinalIgnoreCase);
		public bool IsValid => mi != null&& mph != null&& mph.callFunc != null;
		public bool IsVariadic => isVariadic;

		public long MaxParams => 9999;//All functions in keysharp are variadic so this property doesn't apply.

		public long MinParams => 0;//All functions in keysharp are variadic so this property doesn't apply.

		public string Name => mi != null ? mi.Name : "";

		internal MethodPropertyHolder Mph => mph;

		public FuncObj(string s, object o = null, object paramCount = null)
			: this(o != null ? Reflections.FindAndCacheMethod(o.GetType(), s, paramCount.Ai(-1)) : Reflections.FindMethod(s, paramCount.Ai(-1)), o)
		{
		}

		internal FuncObj(MethodPropertyHolder m, object o = null)
			: this(m?.mi, o)
		{
		}

		internal FuncObj(MethodInfo m, object o = null)
		{
			mi = m;
			inst = o;

			if (mi != null)
				Init();
		}

		public BoundFunc Bind(params object[] obj)
		=> new BoundFunc(mi, obj, inst);

		public virtual object Call(params object[] obj) => mph.callFunc(inst, obj);

		public virtual object CallWithRefs(params object[] obj)
		{
			var refs = new List<RefHolder>(obj.Length);

			for (var i = 0; i < obj.Length; i++)
			{
				object p = obj[i];

				if (p is RefHolder rh)
				{
					refs.Add(rh);
					obj[i] = rh.val;
				}
			}

			var val = mph.callFunc(inst, obj);

			for (var i = 0; i < refs.Count; i++)
			{
				var rh = refs[i];
				rh.reassign(obj[rh.index]);
			}

			return val;
		}

		public bool IsByRef(object obj = null)
		{
			var index = obj.Ai();
			var funcParams = mi.GetParameters();

			if (index > 0)
			{
				index--;

				if (index < funcParams.Length)
					return funcParams[index].ParameterType.IsByRef;
			}
			else
			{
				for (var i = 0; i < funcParams.Length; i++)
					if (funcParams[i].ParameterType.IsByRef)
						return true;
			}

			return false;
		}

		public bool IsOptional(object obj = null)
		{
			var index = obj.Ai();
			var funcParams = mi.GetParameters();

			if (index > 0)
			{
				index--;

				if (index < funcParams.Length)
					return funcParams[index].IsOptional;
			}
			else
			{
				for (var i = 0; i < funcParams.Length; i++)
					if (funcParams[i].IsOptional)
						return true;
			}

			return false;
		}

		private void Init()
		{
			mph = new MethodPropertyHolder(mi, null);
			var parameters = mph.parameters;

			foreach (var p in parameters)
			{
				if (p.ParameterType.IsByRef)
				{
					anyRef = true;
					break;
				}
			}

			isVariadic = parameters.Length > 0 && parameters[parameters.Length - 1].ParameterType == typeof(object[]);
		}
	}
}