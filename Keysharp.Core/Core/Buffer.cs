﻿namespace Keysharp.Core
{
	/// <summary>
	/// Encapsulates a block of memory for use with advanced techniques such as DllCall, structures, StrPut and raw file I/O.<br/>
	/// Buffer objects are typically created by calling <see cref="Collections.Buffer"/>,<br/>
	/// but can also be returned by <see cref="Files.FileRead"/> with the "RAW" option.
	/// </summary>
	public class Buffer : KeysharpObject, IDisposable, IPointable
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
		/// SafeHandle wrapper for the native memory pointer.
		/// </summary>
		private NativeMemoryHandle _ptr;

		/// <summary>
		/// Gets the pointer to the memory.
		/// </summary>
		public long Ptr
		{
			get => _ptr?.DangerousGetHandle() ?? 0L;
			private set => _ptr = new NativeMemoryHandle((nint)value);
		}

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

					if (_ptr != null)
					{
						unsafe
						{
							var src = (byte*)_ptr.DangerousGetHandle();
							var dst = (byte*)newptr.ToPointer();
							System.Buffer.MemoryCopy(src, dst, val, size);
						}
						var old = _ptr;
						_ptr = new NativeMemoryHandle(newptr);
						old.Dispose();
					}
					else
						_ptr = new NativeMemoryHandle(newptr);
				}

				size = val;
			}
		}


		/// <summary>
		/// Calls <see cref="__New"/> to initialize a new instance of the <see cref="Buffer"/> class.
		/// </summary>
		/// <param name="args">The data to initially store in the buffer</param>
		public Buffer(params object[] args) : base(args) { }

		public static object Call(object @this, object byteCount = null, object fillByte = null)
		{
			Type t = @this.GetType();
			return Activator.CreateInstance(t, [byteCount, fillByte]);
			//new Buffer(byteCount, fillByte);
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
		public override unsafe object __New(params object[] obj)
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
					Size = bytearray.Length;//Performs the allocation.

					if (size > 0)
						Marshal.Copy(bytearray, 0, _ptr.DangerousGetHandle(), Math.Min((int)size, bytearray.Length));
				}
				else if (obj0 is Array array)
				{
					var ct = array.array.Count;
					Size = ct;
					var bp = _ptr.DangerousGetHandle();

					for (var i = 0; i < ct; i++)
						Unsafe.Write((void*)nint.Add(bp, i), (byte)Script.ForceLong(array.array[i]));//Access the underlying array[] directly for performance.
				}
				else//This will be called by the user.
				{
					var bytecount = obj0.Al(0);
					var fill = obj.Length > 1 ? obj[1].Al(long.MinValue) : long.MinValue;
					Size = bytecount;

					if (bytecount > 0)
					{
						byte val = fill != long.MinValue ? (byte)(fill & 255) : (byte)0;
						Unsafe.InitBlockUnaligned((void*)_ptr.DangerousGetHandle(), val, (uint)bytecount);
					}
				}
			}

			return DefaultObject;
		}

		/// <summary>
		/// Destructor that manually calls <see cref="Dispose"/> to free the raw memory contained in the buffer.
		/// </summary>
		public override object __Delete()
		{
			Dispose(true);
			return DefaultObject;
		}

		/// <summary>
		/// Dispose the object and set a flag so it doesn't get disposed twice.
		/// </summary>
		/// <param name="disposing">If true, disposing already, so skip, else dispose.</param>
		public object Dispose(bool disposing)
		{
			if (!disposed)
			{
				_ptr?.Dispose();
				Size = 0;
				disposed = true;
			}
			return DefaultObject;
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
		/// Converts the contents of the buffer to a hex string.
		/// </summary>
		/// 
		public string ToHex() => Convert.ToHexString(AsSpan());

		/// <summary>
		/// Converts the contents of the buffer to a base64 string.
		/// </summary>
		public string ToBase64() => Convert.ToBase64String(AsSpan());

		/// <summary>
		/// Returns the contents of the buffer as a byte array.
		/// </summary>
		public byte[] ToByteArray()
		{
			int size = (int)(long)Size;
			byte[] dataArray = new byte[size];
			Marshal.Copy(_ptr.DangerousGetHandle(), dataArray, 0, size);
			return dataArray;
		}

		/// <summary>
		/// Returns a mutable Span wrapper for the raw buffer.
		/// </summary>
		public unsafe Span<byte> AsSpan() => new Span<byte>((byte*)_ptr.DangerousGetHandle(), (int)(long)Size);

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
				if (index > 0 && index <= size)
				{
					unsafe
					{
						var ptr = (byte*)_ptr.DangerousGetHandle();
						return ptr[index - 1];
					}
				}
				else
					return (long)Errors.IndexErrorOccurred($"Invalid index of {index} for buffer of size {Size}.", DefaultErrorLong);
			}
		}
	}

	/// <summary>
	/// Wrapper for native memory pointers. It's used for two reasons: firstly, classes derived from
	/// CriticalFinalizerObject (like SafeHandle) are guaranteed to have finalizers executed so
	/// this prevents memory leaks if the assembly is unexpectedly unloaded. Second, critical finalizers
	/// are ran after regular ones which gives user-code time to run any __Delete methods before the
	/// memory is released. This is important in cases like an object holding a Buffer with the destructor
	/// set up to clean resources up (eg VariantClear). Without using SafeHandle the Buffer finalizer
	/// may be called first causing the resource-cleanup to fail.
	/// </summary>
	sealed class NativeMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		public NativeMemoryHandle() : base(ownsHandle: true) { }

		public NativeMemoryHandle(nint p, bool owns = true) : base(owns)
		{
			SetHandle(p);
		}

		protected override bool ReleaseHandle()
		{
			Marshal.FreeHGlobal(handle);
			return true;
		}
	}
}