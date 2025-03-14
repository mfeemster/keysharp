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
				//var bstrs = new List<IntPtr>();
				//HashSet<GCHandle> gcHandles = [];

				for (var i = 0; i < obj.Length; ++i)
				{
					var o = obj[i];
					//
					//This is working code for most cases.
					//
					var co = o as ComObject;
					var p = co != null ? co.Ptr : o;

					if (p != null)
					{
						if (p is long l)
							args[i] = new nint(l);
						else
							args[i] = p;
					}

					//
					// Below this lines is various bits of test code, none of which worked.
					//
					/*
					    if (o is ComObject co)
					    {
					    var v = co.ToVariant();
					    var gch = GCHandle.Alloc(v, GCHandleType.Pinned);
					    _ = gcHandles.Add(gch);
					    var intptr = gch.AddrOfPinnedObject();
					    args[i] = intptr;
					    //Marshal.getobject
					    //args[i] = v;

					    //var intp = (int*)v.data.pdispVal.ToPointer();
					    //args[i] = intp;
					    if (co.Ptr is string s)//If it was a string, it will have been converted to bstr.
					        bstrs.Add(v.data.bstrVal);
					    }
					    else if (o is long l)
					    args[i] = new nint(l);
					    else
					    args[i] = o;
					*/
					//var co = o as ComObject;
					//var p = co != null ? co.Ptr : o;
					/*
					    if (p != null)
					    {
					    if (p is long l)
					    {
					        //args[i] = (object)0;
					        args[i] = new nint(l);
					        Marshal.WriteInt64((nint)args[i], long.MaxValue);
					        var readback = Marshal.ReadInt64((nint)args[i]);
					        //Marshal.WriteInt16(args[i], 0, (short)co.VarType); // vt = 3 (VT_I4)
					        //Marshal.WriteInt16(args[i], 0, 3); // vt = 3 (VT_I4)
					        //Marshal.WriteInt32(args[i], 8, 0); // intVal = 0 (CHILDID_SELF)
					    }
					    else
					    {
					        //test only
					        IntPtr variantPtr = Marshal.AllocCoTaskMem(16); // VARIANT is 16 bytes
					        Marshal.WriteInt16(variantPtr, 0, 3); // vt = 3 (VT_I4)
					        Marshal.WriteInt16(variantPtr, 2, 0);
					        Marshal.WriteInt16(variantPtr, 4, 0);
					        Marshal.WriteInt16(variantPtr, 6, 0);
					        //Marshal.WriteInt32(variantPtr, 8, 0); // intVal = 0 (CHILDID_SELF)
					        Marshal.WriteInt64(variantPtr, 8, 0L); // intVal = 0 (CHILDID_SELF)
					        args[i] = variantPtr;
					        //var firsttwo = Marshal.ReadInt16((nint)args[i]);
					        //actual code here
					        //args[i] = p;
					    }
					    }*/
				}

				//This appears to work sometimes, but does not always populate reference parameters.
				//Unsure how to know whether a parameter is a ref if the type is never specified.
				var ret = inst.GetType().InvokeMember(Name, BindingFlags.InvokeMethod, null, inst, args);
				//test code
				/*
				                foreach (var bstr in bstrs)
				                    Marshal.FreeBSTR(bstr);

				                foreach (var gch in gcHandles)
				                    gch.Free();
				*/
				//Marshal.FreeCoTaskMem((IntPtr)args.Last());
				//
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