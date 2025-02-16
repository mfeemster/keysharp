namespace Keysharp.Core
{
	/// <summary>
	/// Encapsulates a block of memory for use with advanced techniques such as DllCall, structures, StrPut and raw file I/O.<br/>
	/// Buffer objects are typically created by calling <see cref="Collections.Buffer"/>,<br/>
	/// but can also be returned by <see cref="Files.FileRead"/> with the "RAW" option.
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
		/// Gets or sets the size of the buffer.<br/>
		/// If value is greater than the existing size, a new buffer is created with length == value and<br/>
		/// the existing data in the old buffer is copied to the beginning of the new buffer.<br/>
		/// The old buffer is then deleted.
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
		/// The implementation for <see cref="KeysharpObject.super"/> for this class to return this type.
		/// </summary>
		public new (Type, object) super => (typeof(Buffer), this);

		/// <summary>
		/// Calls <see cref="__New"/> to initialize a new instance of the <see cref="Buffer"/> class.
		/// </summary>
		/// <param name="args">The data to initially store in the buffer</param>
		public Buffer(params object[] args) => _ = __New(args);

		/// <summary>
		/// Destructor that manually calls <see cref="Dispose"/> to free the raw memory contained in the buffer.
		/// </summary>
		~Buffer()
		{
			Dispose(true);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Buffer"/> class.
		/// </summary>
		/// <param name="obj">The optional data to initialize the <see cref="Buffer"/> with. This can be:<br/>
		///     empty: Ptr remains null.<br/>
		///     byte[]: Copied one byte at a time to the pointer.<br/>
		///     <see cref="Array"/>: Convert each element to a byte and copy one at a time to the pointer.<br/>
		///     Integer[, Integer]: Sets length to the first value and optionally sets each byte to the second value.
		/// </param>
		/// <returns>Empty string, unused.</returns>
		public unsafe object __New(params object[] obj)
		{
			Init__Item();

			if (obj == null || obj.Length == 0)
			{
				Size = 0;
			}
			else
			{
				var obj0 = obj[0];

				if (obj0 is byte[] bytearray)//This will sometimes be passed internally within the library.
				{
					Size = bytearray.Length;//Performs the allocation.

					if (size > 0)
						Marshal.Copy(bytearray, 0, Ptr, Math.Min((int)size, bytearray.Length));
				}
				else if (obj0 is Array array)
				{
					var ct = array.array.Count;
					Size = ct;

					for (var i = 0; i < ct; i++)
						Marshal.WriteByte(Ptr, i, (byte)Script.ForceLong(array.array[i]));//Access the underlying array[] directly for performance.
				}
				else//This will be called by the user.
				{
					var bytecount = obj0.Al(0);
					var fill = obj.Length > 1 ? obj[1].Al(long.MinValue) : long.MinValue;
					Size = bytecount;
					var ptr = Ptr.ToPointer();

					if (bytecount > 0)
					{
						byte val = fill != long.MinValue ? (byte)(fill & 255) : (byte)0;
						Unsafe.InitBlockUnaligned(ptr, val, (uint)bytecount);
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
		/// The implementation for <see cref="IDisposable.Dispose"/> which just calls Dispose(true).
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
		/// <exception cref="IndexError">An <see cref="IndexError"/> exception is thrown if index is zero or out of range.</exception>
		public long this[long index]
		{
			get
			{
				Error err;

				if (index > 0 && index <= size)
				{
					unsafe
					{
						var ptr = (byte*)Ptr.ToPointer();
						return ptr[index - 1];
					}
				}
				else
					return Errors.ErrorOccurred(err = new IndexError($"Invalid index of {index} for buffer of size {Size}.")) ? throw err : 0L;
			}
		}
	}
}