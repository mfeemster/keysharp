using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Keysharp.Core
{
	public interface IFuncObj
	{
		public string Name { get; }
		public bool IsValid { get; }
		public object Inst { get; }
		public object Call(params object[] obj);
	}

	public static class Function
	{
		public static FuncObj Func(object obj)
		{
			var name = obj.As();
			var fo = new FuncObj(name);
			return fo.Name != "" ? fo
				   : throw new MethodError($"Unable to retrieve method {name}.");
		}

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

		public static FuncObj GetMethod(object obj0, object obj1 = null, object obj2 = null)
		{
			var val = obj0;
			var name = obj1.As();
			//var paramcount = (int)obj2.Al();//Need to find how to use this.//TODO
			var mph = Reflections.FindAndCacheMethod(val.GetType(), name);
			return mph != null && mph.mi != null ? new FuncObj(mph.mi, val)
				   : throw new MethodError($"Unable to retrieve method {name} from object of type {val.GetType()}.");
		}

		public static long HasMethod(object obj0, object obj1 = null, object obj2 = null)
		{
			var val = obj0;
			var name = obj1.As();
			//var paramcount = (int)obj2.Al();//Need to find how to use this.//TODO
			var mi = Reflections.FindAndCacheMethod(val.GetType(), name);
			return mi != null ? 1L : 0L;
		}

		public static BoundFunc ObjBindMethod(params object[] obj)
		{
			var (o, name) = obj.Os();
			object[] args = null;

			if (Reflections.FindAndCacheMethod(o.GetType(), name) is MethodPropertyHolder mph && mph.mi != null)
			{
				if (obj.Length > 2)
					args = obj.Skip(2).ToArray();

				return new BoundFunc(mph.mi, args, o);
			}

			return null;
		}

		public static string ObjGetBase(object obj) => obj.GetType().BaseType.Name;
	}

	public class BoundFunc : FuncObj
	{
		internal object[] boundargs;

		public BoundFunc(MethodInfo m, object[] ba, object o = null)
			: base(m, o)
		{
			boundargs = ba;
		}

		public override object Call(params object[] args)
		{
			int i = 0, argsused = 0;
			var argslist = new List<object>(mph.parameters.Length);

			for (; i < boundargs.Length; i++)
			{
				if (boundargs[i] != null)
				{
					argslist.Add(boundargs[i]);
				}
				else if (argsused < args.Length)
				{
					argslist.Add(args[argsused]);
					argsused++;
				}
				else
					argslist.Add(null);
			}

			for (; argsused < args.Length && argsused < mph.parameters.Length; argsused++)
				argslist.Add(args[argsused]);

			while (argslist.Count < mph.parameters.Length)
			{
				var param = mph.parameters[argslist.Count];

				if (param.Attributes.HasFlag(ParameterAttributes.HasDefault))
					argslist.Add(param.DefaultValue);
				else
					argslist.Add(null);
			}

			return base.Call(argslist.ToArray());
		}
	}

	public class DelegateHolder
	{
		public PlaceholderFunction thisdel;
		private readonly bool fast;
		private readonly IFuncObj funcObj;
		private readonly bool reference;

		public DelegateHolder(object obj, bool f, bool r)
		{
			funcObj = Function.GetFuncObj(obj, null, true);
			fast = f;
			reference = r;
			thisdel = (PlaceholderFunction)Delegate.CreateDelegate(typeof(PlaceholderFunction), this, "DelegatePlaceholder");
		}

		public long DelegatePlaceholder(IntPtr p1, IntPtr p2, IntPtr p3, IntPtr p4, IntPtr p5, IntPtr p6, IntPtr p7, IntPtr p8,
										IntPtr p9, IntPtr p10, IntPtr p11, IntPtr p12, IntPtr p13, IntPtr p14, IntPtr p15, IntPtr p16,
										IntPtr p17, IntPtr p18, IntPtr p19, IntPtr p20, IntPtr p21, IntPtr p22, IntPtr p23, IntPtr p24,
										IntPtr p25, IntPtr p26, IntPtr p27, IntPtr p28, IntPtr p29, IntPtr p30, IntPtr p31)
		{
			object val = null;
			var arr = reference ? new IntPtr[] { p1, p2, p3, p4, p5, p6, p7, p8,
												 p9, p10, p11, p12, p13, p14, p15, p16,
												 p17, p18, p19, p20, p21, p22, p23, p24,
												 p25, p26, p27, p28, p29, p30, p31
											   }
				: null;

			//Need to figure out how fast/slow works with threads here.//TODO
			/*  if (del != null)
			    {
			    if (reference)
			    {
			        unsafe
			        {
			            fixed (IntPtr* pin = &arr[0])
			            {
			                var ptr = new IntPtr((long)pin);
			                val = del.Invoke(ptr);
			            }
			        }
			    }
			    else
			        val = del.Invoke(p1, p2, p3, p4, p5, p6, p7, p8,
			                         p9, p10, p11, p12, p13, p14, p15, p16,
			                         p17, p18, p19, p20, p21, p22, p23, p24,
			                         p25, p26, p27, p28, p29, p30, p31);
			    }

			    else*/
			if (funcObj != null)
			{
				if (reference)
				{
					unsafe
					{
						fixed (IntPtr* pin = &arr[0])
						{
							var ptr = new IntPtr((long)pin);
							val = funcObj.Call(ptr);
						}
					}
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
			else if (val is string s && s?.Length == 0)
				return 0L;
			else if (val is null)
				return 0L;

			return 0L;
		}

		public delegate long PlaceholderFunction(IntPtr p1, IntPtr p2, IntPtr p3, IntPtr p4, IntPtr p5, IntPtr p6, IntPtr p7, IntPtr p8,
				IntPtr p9, IntPtr p10, IntPtr p11, IntPtr p12, IntPtr p13, IntPtr p14, IntPtr p15, IntPtr p16,
				IntPtr p17, IntPtr p18, IntPtr p19, IntPtr p20, IntPtr p21, IntPtr p22, IntPtr p23, IntPtr p24,
				IntPtr p25, IntPtr p26, IntPtr p27, IntPtr p28, IntPtr p29, IntPtr p30, IntPtr p31);
	}

	public class FuncObj : KeysharpObject, IFuncObj
	{
		protected bool anyRef;
		protected bool isVariadic;
		protected object inst;
		protected MethodInfo mi;
		protected MethodPropertyHolder mph;
		public bool IsBuiltIn => mi.DeclaringType.Module.Name.StartsWith("keysharp.core", StringComparison.OrdinalIgnoreCase);
		public object Inst => inst;
		public bool IsValid => mi != null&& mph != null&& mph.callFunc != null;
		public bool IsVariadic => isVariadic;

		public long MaxParams => 9999;//All functions in keysharp are variadic so this property doesn't apply.

		public long MinParams => 0;//All functions in keysharp are variadic so this property doesn't apply.

		public string Name => mi != null ? mi.Name : "";

		public FuncObj(string s, object o = null)
			: this(o != null ? Reflections.FindAndCacheMethod(o.GetType(), s) : Reflections.FindMethod(s), o)
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

		public bool IsByRef(params object[] obj)//Ref args are not supported, so this should always return false.
		{
			var index = (int)obj.Al();
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

		public bool IsOptional(object obj)
		{
			var index = (int)obj.Al();
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