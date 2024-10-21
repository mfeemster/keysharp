namespace Keysharp.Core
{
	public class Buffer : KeysharpObject, IDisposable
	{
		private bool disposed = false;

		private long size;

		public IntPtr Ptr { get; private set; }

		public object Size
		{
			get => size;

			set
			{
				var val = value.Al();

				if (val > size)
				{
					var newptr = Marshal.AllocHGlobal((int)val);

					if (Ptr != IntPtr.Zero)
					{
						unsafe
						{
							var src = (byte*)Ptr.ToPointer();
							var dst = (byte*)newptr.ToPointer();
							System.Buffer.MemoryCopy(src, dst, val, size);
						}
						var old = Ptr;
						Ptr = newptr;
						Marshal.FreeHGlobal(old);
					}
					else
						Ptr = newptr;
				}

				size = val;
			}
		}

		public Buffer(params object[] obj) => __New(obj);

		~Buffer()
		{
			Dispose(true);
		}

		public override object __New(params object[] obj)
		{
			var obj0 = obj[0];

			if (obj0 is byte[] bytearray)//This will sometimes be passed internally within the library.
			{
				Size = bytearray.Length;

				for (var i = 0; i < bytearray.Length; i++)
					Marshal.WriteByte(Ptr, i, bytearray[i]);
			}
			else if (obj0 is Array array)
			{
				var ct = array.array.Count;
				Size = ct;

				for (var i = 0; i < ct; i++)
					Marshal.WriteByte(Ptr, i, (byte)(Script.ForceLong(array.array[i])));//Access the underlying ArrayList directly for performance.
			}
			else//This will be called by the user.
			{
				var bytecount = obj0.Al(0);
				var fill = obj.Length > 1 ? obj[1].Al(long.MinValue) : long.MinValue;
				Size = bytecount;//Performs the allocation.

				if (fill != long.MinValue && bytecount > 0)
				{
					var val = (byte)(fill & 255);

					for (var i = 0; i < bytecount; i++)
						Marshal.WriteByte(Ptr, i, val);
				}
			}

			return "";
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

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
	}
}