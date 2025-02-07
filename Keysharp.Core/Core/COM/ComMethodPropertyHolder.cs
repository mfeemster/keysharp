#if WINDOWS
namespace Keysharp.Core.COM
{
	unsafe public class ComMethodPropertyHolder : MethodPropertyHolder
	{
		public string Name { get; private set; }

		public ComMethodPropertyHolder(string name)
			: base(null, null)
		{
			Name = name;
			callFunc = (inst, obj) =>
			{
				var t = inst.GetType();
				var args = new object[obj.Length];

				for (var i = 0; i < obj.Length; ++i)
				{
					var o = obj[i];
					var co = o as ComObject;
					var p = co != null ? co.Ptr : o;

					if (p != null)
					{
						if (p is long l)
							args[i] = new nint(l);
						else
							args[i] = p;
					}
				}

				//This appears to work sometimes, but does not always populate reference parameters.
				//Unsure how to know whether a parameter is a ref if the type is never specified.
				var ret = inst.GetType().InvokeMember(Name, BindingFlags.InvokeMethod, null, inst, args);
				//for (var i = 0; i < obj.Length; ++i)
				//{
				//  var o = obj[i];
				//  var a = args[i];
				//  if (o is ComObject co)
				//  {
				//      if ((co.VarType & Com.vt_byref) == Com.vt_byref)
				//      {
				//          if (co.Ptr is long && a is IntPtr ip)
				//              co.Ptr = ip.ToInt64();
				//          else if (co.Ptr is IntPtr && a is long l)
				//              co.Ptr = new nint(l);
				//          else
				//              co.Ptr = a;
				//      }
				//  }
				//}
				return ret;
			};
		}
	}
}

#endif