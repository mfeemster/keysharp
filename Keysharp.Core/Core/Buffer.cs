namespace Keysharp.Core
{
	/// <summary>
	/// Encapsulates a block of memory for use with advanced techniques such as DllCall, structures, StrPut and raw file I/O.
	/// Buffer objects are typically created by calling Buffer(), but can also be returned by FileRead with the "RAW" option.
	/// </summary>
	public class Buffer : KeysharpObject, IDisposable
	{
		/// <summary>
		/// Whether the object has been disposed or not.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// The size of the buffer in bytes.
		/// </summary>
		private long size;

		/// <summary>
		/// Gets the pointer to the memory.
		/// </summary>
		public IntPtr Ptr { get; private set; }

		/// <summary>
		/// Gets or sets the size of the buffer.
		/// If value is greater than the existing size, a new buffer is created with length == value and
		/// the existing data in the old buffer is copied to the beginning of the new buffer. The old
		/// buffer is then deleted.
		/// </summary>
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

		/// <summary>
		/// Calls __New() to initialize a new instance of the <see cref="Buffer"/> class.
		/// </summary>
		/// <param name="obj">The data to initially store in the buffer</param>
		public Buffer(params object[] obj) => __New(obj);

		/// <summary>
		/// Destructor that manually calls Dispose() to free the raw memory contained in the buffer.
		/// </summary>
		~Buffer()
		{
			Dispose(true);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Buffer"/> class.
		/// </summary>
		/// <param name="obj">The optional data to initialize the <see cref="Buffer"/> with. This can be:
		///     empty: Ptr remains null.
		///     byte[]: Copied one byte at a time to the pointer.
		///     Array: Convert each element to a byte and copy one at a time to the pointer.
		///     Integer[, Integer]: Sets length to the first value and optionally sets each byte to the second value.
		/// </param>
		/// <returns>Empty string, unused.</returns>
		public override object __New(params object[] obj)
		{
			if (obj == null || obj.Length == 0)
			{
				Size = 0;
			}
			else
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
						Marshal.WriteByte(Ptr, i, (byte)Script.ForceLong(array.array[i]));//Access the underlying ArrayList directly for performance.
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
			}

			return "";
		}

		/// <summary>
		/// Dispose the object and set a flag so it doesn't get disposed twice.
		/// </summary>
		/// <param name="disposing">If true, disposing already, so skip, else dispose.</param>
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

		/// <summary>
		/// The implementation for IDisposable.Dispose() which just calls Dispose(true).
		/// </summary>
		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Indexer which retrieves or sets the value of an array element.
		/// </summary>
		/// <param name="index">The index to get a byte from.</param>
		/// <returns>The value at the index.</returns>
		/// <exception cref="IndexError">An IndexError exception is thrown if index is zero or out of range.</exception>
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
					throw new IndexError($"Invalid index of {index} for buffer of size {Size}.");
			}
		}
	}
}