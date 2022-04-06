using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Keysharp.Core.Common.Threading;

namespace Keysharp.Core
{
	public interface IFuncObj
	{
		public object Call(params object[] obj);
	}

	public static class Function
	{
		public static GenericFunction Func(params object[] obj)//This needs to be consolidated with the code in CallbackCreate(), HotstringLabel() and FunctionReference() //MATT
		{
			var o = obj.L();
			var funcname = o.S1();
			var mi = Reflections.FindLocalRoutine(funcname);
			var del = Delegate.CreateDelegate(typeof(GenericFunction), mi);
			return del as GenericFunction;
		}

		public static FuncObj GetMethod(params object[] obj)
		{
			var (val, name, paramcount) = obj.Osi();
			var mi = Reflections.FindAndCacheMethod(val.GetType(), name);
			return mi != null ? new FuncObj(mi, val)
				   : throw new MethodError($"Unable to retrieve method {name} from object of type {val.GetType()}.");
		}

		public static long HasMethod(params object[] obj)
		{
			var (val, name, paramcount) = obj.Osi();
			var mi = Reflections.FindAndCacheMethod(val.GetType(), name);
			return mi != null ? 1L : 0L;
		}

		public static BoundFunc ObjBindMethod(params object[] obj)
		{
			var (o, name) = obj.Os();
			object[] args = null;

			if (Reflections.FindAndCacheMethod(o.GetType(), name) is MethodInfo mi)
			{
				if (obj.Length > 2)
					args = obj.Skip(2).ToArray();

				return new BoundFunc(mi, args, o);
			}

			return null;
		}

		public static string ObjGetBase(params object[] obj) => obj[0].GetType().BaseType.Name;//Do not flatten, use obj[0] directly.
	}

	public class BoundFunc : FuncObj
	{
		internal object[] boundargs;

		public BoundFunc(MethodInfo m, object[] ba, object o = null)
			: base(m, o)
		{
			boundargs = ba.Select(x => x.ParseObject()).ToArray();
		}

		public override object Call(params object[] args)
		{
			int i = 0, argsused = 0;
			var argslist = new List<object>(boundargs.Length + args.Length);

			for (; i < boundargs.Length; i++)
			{
				if (boundargs[i] != null)
				{
					argslist.Add(boundargs[i]);
				}
				else if (argsused < args.Length)
				{
					argslist.Add(args[argsused].ParseObject());
					argsused++;
				}
				else
					argslist.Add(null);
			}

			for (; argsused < args.Length; argsused++)
				argslist.Add(args[argsused].ParseObject());

			return mi.GetParameters().Length == 0 ? mi.Invoke(inst, null) : mi.Invoke(inst, new object[] { argslist.ToArray() });
		}
	}

	public class DelegateHolder
	{
		public PlaceholderFunction thisdel;
		private readonly GenericFunction del;
		private readonly bool fast;
		private readonly IFuncObj funcObj;
		private readonly bool reference;

		public DelegateHolder(object obj, bool f, bool r)
		{
			obj = obj.ParseObject();

			if (obj is string s)
				del = Function.Func(s);
			else if (obj is IFuncObj fo)
				funcObj = fo;
			else
				throw new TypeError($"Argument of type {obj.GetType()} was not either a string or function object.");

			fast = f;
			reference = r;
			thisdel = (PlaceholderFunction)Delegate.CreateDelegate(typeof(PlaceholderFunction), this, "DelegatePlaceholder");
		}

		public int DelegatePlaceholder(IntPtr p1, IntPtr p2, IntPtr p3, IntPtr p4, IntPtr p5, IntPtr p6, IntPtr p7, IntPtr p8,
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
			if (del != null)
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
			else if (funcObj != null)
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
				return 0;
			else if (val is null)
				return 0;

			return 0;
		}

		public delegate int PlaceholderFunction(IntPtr p1, IntPtr p2, IntPtr p3, IntPtr p4, IntPtr p5, IntPtr p6, IntPtr p7, IntPtr p8,
												IntPtr p9, IntPtr p10, IntPtr p11, IntPtr p12, IntPtr p13, IntPtr p14, IntPtr p15, IntPtr p16,
												IntPtr p17, IntPtr p18, IntPtr p19, IntPtr p20, IntPtr p21, IntPtr p22, IntPtr p23, IntPtr p24,
												IntPtr p25, IntPtr p26, IntPtr p27, IntPtr p28, IntPtr p29, IntPtr p30, IntPtr p31);
	}

	public class FuncObj : KeysharpObject, IFuncObj
	{
		protected object inst;
		protected MethodInfo mi;
		public bool IsBuiltIn => mi.DeclaringType.Module.Name.StartsWith("keysharp.core", StringComparison.OrdinalIgnoreCase);

		public bool IsValid => mi != null;
		public bool IsVariadic => true;//All functions in keysharp are variadic because they use the params keyword.

		public long MaxParams => 1;//All functions in keysharp are variadic so this property doesn't apply.

		public long MinParams => 0;//All functions in keysharp are variadic so this property doesn't apply.

		public string Name { get => mi.Name; }

		public FuncObj(string s, object o = null)
			: this(o != null ? Reflections.FindAndCacheMethod(o.ParseObject().GetType(), s) : Reflections.FindMethod(s), o)
		{
		}

		internal FuncObj(MethodInfo m, object o = null)
		{
			mi = m;
			inst = o != null ? o.ParseObject() : o;
		}

		internal FuncObj(GenericFunction m, object o = null)
		{
			mi = m.Method;
			inst = o != null ? o.ParseObject() : o;
		}

		public BoundFunc Bind(params object[] obj)//Need to figure out making this work.//TODO
		{
			return new BoundFunc(mi, obj, inst);
		}

		public virtual object Call(params object[] obj) => mi.GetParameters().Length == 0 ? mi.Invoke(inst, null) : mi.Invoke(inst, new object[] { obj });

		public bool IsByRef(params object[] obj) => false;//Ref args are not supported.

		public bool IsOptional(params object[] obj) => true;//All functions in keysharp are variadic so all parameters are optional.
	}
}