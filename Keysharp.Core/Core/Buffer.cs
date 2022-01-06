using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Keysharp.Core
{
	public class StringBuffer : KeysharpObject
	{
		public StringBuilder sb;

		public StringBuffer(string str = "", int capacity = 256) => sb = new StringBuilder(str, capacity);

		public static implicit operator string(StringBuffer s) => s.ToString();

		public override string ToString() => sb.ToString();
	}

	public class Buffer : KeysharpObject, IDisposable
	{
		private bool disposed = false;

		public Buffer(params object[] obj) => __New(obj);

		public void __New(params object[] obj)
		{
			var (bytecount, fill) = obj.I2(0, int.MinValue);
			Size = bytecount;//Performs the allocation.

			if (fill != int.MinValue && bytecount > 0)
			{
				var val = (byte)(fill & 255);

				for (var i = 0; i < bytecount; i++)
					Marshal.WriteByte(Ptr, i, val);
			}
		}

		public long this[long index]
		{
			get
			{
				if (index > 0 && index <= size)
				{
					unsafe
					{
						var ptr = (byte*)Ptr.ToPointer();
						return ptr[index - 1];
					}
				}
				else
					throw new Exception($"Invalid index of {index} for buffer of size {Size} in {new StackFrame(0).GetMethod().Name}");
			}
		}

		~Buffer()
		{
			Dispose(true);
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				Marshal.FreeHGlobal(Ptr);
				Ptr = IntPtr.Zero;
				Size = 0;
				disposed = true;
			}
		}

		public IntPtr Ptr { get; private set; }

		private long size;

		public object Size
		{
			get => size;

			set
			{
				var val = value.ParseLong();

				if (val.HasValue && val > size)
				{
					var newptr = Marshal.AllocHGlobal((int)val);

					if (Ptr != IntPtr.Zero)
					{
						unsafe
						{
							var src = (byte*)Ptr.ToPointer();
							var dst = (byte*)newptr.ToPointer();
							System.Buffer.MemoryCopy(src, dst, val.Value, size);
						}
						var old = Ptr;
						Ptr = newptr;
						Marshal.FreeHGlobal(old);
					}
					else
						Ptr = newptr;
				}

				size = val.Value;
			}
		}
	}
}
