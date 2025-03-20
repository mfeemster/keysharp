namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for external-related functions.
	/// </summary>
	public static class External
	{
		/// <summary>
		/// Returns the binary number stored at the specified address+offset.
		/// </summary>
		/// <param name="source">A <see cref="Buffer"/>-like object or memory address.</param>
		/// <param name="offset">If blank or omitted (or when using 2-parameter mode), it defaults to 0.<br/>
		/// Otherwise, specify an offset in bytes which is added to source to determine the source address.
		/// </param>
		/// <param name="type">One of the following strings: UInt, Int, Int64, Short, UShort, Char, UChar, Double, Float, Ptr or UPtr</param>
		/// <returns>The binary number at the specified address+offset.</returns>
		/// <exception cref="TypeError">A <see cref="TypeError"/> exception is thrown the address could not be determined.</exception>
		public unsafe static object NumGet(object source, object offset, object type = null)
		{
			Error err;
			int off;
			string t;
			var address = source;

			if (type == null)
			{
				off = 0;
				t = offset.As("UInt");
			}
			else
			{
				off = offset is IntPtr ip ? ip.ToInt32() : offset.Ai();
				t = type.As("UInt");
			}

			IntPtr addr;
			var buf = address as Buffer;
			t = t.ToLower();

			if (buf != null)
				addr = buf.Ptr;
			else if (address is object[] objarr && objarr.Length > 0)//Assume the first element was a long which was an address.
				addr = new nint(objarr[0].Al());
			else if (address is IntPtr ptr)
				addr = ptr;
			else if (address is long l)
				addr = new IntPtr(l);
			else if (address is int i)
				addr = new IntPtr(i);

#if WINDOWS
			else if (t == "ptr" && address is ComObject co)
			{
				var pUnk = Marshal.GetIUnknownForObject(co.Ptr);
				addr = pUnk;//Don't dererference here, it'll be done below.
				_ = Marshal.Release(pUnk);
			}
			else if (t == "ptr" && Marshal.IsComObject(address))
			{
				var pUnk = Marshal.GetIUnknownForObject(address);
				addr = pUnk;//Ditto.
				_ = Marshal.Release(pUnk);
			}
			else
				return Errors.ErrorOccurred(err = new TypeError($"Could not convert address argument of type {address.GetType()} into an IntPtr. Type must be int, long, ComObject or Buffer.")) ? throw err : null;

#else
			else
				return Errors.ErrorOccurred(err = new TypeError($"Could not convert address argument of type {address.GetType()} into an IntPtr. Type must be int, long, or Buffer.")) ? throw err : null;

#endif

			switch (t)
			{
				case "uint":
					if (buf != null && (off + 4 > (long)buf.Size))
						return Errors.ErrorOccurred(err = new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}.")) ? throw err : null;

					return (long)(uint)Marshal.ReadInt32(addr, off);

				case "int":
					if (buf != null && (off + 4 > (long)buf.Size))
						return Errors.ErrorOccurred(err = new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}.")) ? throw err : null;

					return (long)Marshal.ReadInt32(addr, off);

				case "short":
					if (buf != null && (off + 2 > (long)buf.Size))
						return Errors.ErrorOccurred(err = new IndexError($"Memory access exceeded buffer size. Offset {off} + length 2 > buffer size {(long)buf.Size}.")) ? throw err : null;

					return (long)Marshal.ReadInt16(addr, off);

				case "ushort":
					if (buf != null && (off + 2 > (long)buf.Size))
						return Errors.ErrorOccurred(err = new IndexError($"Memory access exceeded buffer size. Offset {off} + length 2 > buffer size {(long)buf.Size}.")) ? throw err : null;

					return (long)(ushort)Marshal.ReadInt16(addr, off);

				case "char":
					if (buf != null && (off + 1 > (long)buf.Size))
						return Errors.ErrorOccurred(err = new IndexError($"Memory access exceeded buffer size. Offset {off} + length 1 > buffer size {(long)buf.Size}.")) ? throw err : null;

					return (long)(sbyte)Marshal.ReadByte(addr, off);

				case "uchar":
					if (buf != null && (off + 1 > (long)buf.Size))
						return Errors.ErrorOccurred(err = new IndexError($"Memory access exceeded buffer size. Offset {off} + length 1 > buffer size {(long)buf.Size}.")) ? throw err : null;

					return (long)Marshal.ReadByte(addr, off);

				case "double":
					if (buf != null && (off + 8 > (long)buf.Size))
						return Errors.ErrorOccurred(err = new IndexError($"Memory access exceeded buffer size. Offset {off} + length 8 > buffer size {(long)buf.Size}.")) ? throw err : null;

					unsafe
					{
						var ptr = (double*)(addr + off).ToPointer();
						var val = *ptr;
						return val;
					}

				case "float":
					if (buf != null && (off + 4 > (long)buf.Size))
						return Errors.ErrorOccurred(err = new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}.")) ? throw err : null;

					unsafe
					{
						var ptr = (float*)(addr + off).ToPointer();
						return double.Parse((*ptr).ToString());//Need to convert to string to make it exact, else there can be lots of rounding/trailing digits.
					}

				case "int64":
				case "ptr":
				case "uptr":
				default:
					if (buf != null && (off + 8 > (long)buf.Size))
						return Errors.ErrorOccurred(err = new IndexError($"Memory access exceeded buffer size. Offset {off} + length 8 > buffer size {(long)buf.Size}.")) ? throw err : null;

					var ipoff = IntPtr.Add(addr, off);
					//var pp = (long*)ipoff.ToPointer();
					//return *pp;
					return Marshal.ReadIntPtr(ipoff).ToInt64();//Dereference here.
			}
		}

		/// <summary>
		/// Stores one or more numbers in binary format at the specified address+offset.
		/// </summary>
		/// <param name="type">One of the following strings: UInt, UInt64, Int, Int64, Short, UShort, Char, UChar, Double, Float, Ptr or UPtr.</param>
		/// <param name="number">The number to store.</param>
		/// <param name="target">A <see cref="Buffer"/>-like object or memory address.</param>
		/// <param name="offset">If omitted, it defaults to 0. Otherwise, specify an offset in bytes which is added to Target to determine the target address.</param>
		/// <returns>The address to the right of the last item written.</returns>
		/// <exception cref="IndexError">An <see cref="IndexError"/> exception is thrown if the offset exceeds the bounds of the memory or if it couldn't be determined.</exception>
		public static long NumPut(params object[] obj)
		{
			byte[] ConvertToInt(object input)
			{
				if (input is IntPtr ip)
					return BitConverter.GetBytes(ip.ToInt64());
				else if (input is ulong ul)
					return BitConverter.GetBytes(ul);
				else
					return BitConverter.GetBytes(Convert.ToUInt64(input.Al()));
			}
			Error err;
			IntPtr addr = IntPtr.Zero;
			Buffer buf;
			var offset = 0;
			int lastPairIndex;
			var offsetSpecified = !((obj.Length & 1) == 1);
			object target;

			if (offsetSpecified)
			{
				lastPairIndex = obj.Length - 4;
				offset = obj.Ai(obj.Length - 1);
				target = obj[obj.Length - 2];
			}
			else
			{
				lastPairIndex = obj.Length - 3;
				target = obj[obj.Length - 1];
			}

			buf = target as Buffer;

			if (buf != null)
				addr = buf.Ptr;
			else if (target is IntPtr ptr)
				addr = ptr;
			else if (target is long l)
				addr = new IntPtr(l);
			else if (target is int i)
				addr = new IntPtr(i);

			for (var i = 0; i <= lastPairIndex; i += 2)
			{
				var inc = 0;
				var type = obj[i] as string;
				var number = obj[i + 1];
				byte[] bytes;

				switch (type.ToLower())
				{
					case "int":
						bytes = BitConverter.GetBytes(number.Ai());
						inc = 4;
						break;

					case "uint":
						bytes = BitConverter.GetBytes(number.Aui());
						inc = 4;
						break;

					case "float":
						bytes = BitConverter.GetBytes(number.Af());
						inc = 4;
						break;

					case "short":
						bytes = BitConverter.GetBytes((short)number.Ai());
						inc = 2;
						break;

					case "ushort":
						bytes = BitConverter.GetBytes((ushort)number.Aui());
						inc = 2;
						break;

					case "char":
						bytes = [(byte)number.Ai()];
						inc = 1;
						break;

					case "uchar":
						bytes = [(byte)number.Ai()];
						inc = 1;
						break;

					case "double":
						bytes = BitConverter.GetBytes(number.Ad());
						inc = 8;
						break;

					case "int64":
						bytes = BitConverter.GetBytes(number.Al());
						inc = 8;
						break;

					case "uint64":
					case "ptr":
					case "uptr":
						if (number is DelegateHolder dh)
							bytes = BitConverter.GetBytes(Marshal.GetFunctionPointerForDelegate(dh.DelRef));
						else
							bytes = ConvertToInt(number);

						inc = 8;
						break;

					default:
						bytes = System.Array.Empty<byte>();
						inc = 0;
						break;
				}

				var finalAddr = IntPtr.Add(addr, offset);

				if (buf != null)
				{
					if ((offset + bytes.Length) <= (long)buf.Size)
					{
						Marshal.Copy(bytes, 0, finalAddr, bytes.Length);
						offset += inc;
					}
					else
						return Errors.ErrorOccurred(err = new IndexError($"Memory access exceeded buffer size. Offset {offset} + length {bytes.Length} > buffer size {(long)buf.Size}.")) ? throw err : 0L;
				}
				else if (addr != IntPtr.Zero)
				{
					Marshal.Copy(bytes, 0, finalAddr, bytes.Length);
					offset += inc;
				}
				else
					return Errors.ErrorOccurred(err = new IndexError($"Could not parse target {target} as a Buffer or memory address.")) ? throw err : 0L;
			}

			return IntPtr.Add(addr, offset).ToInt64();
		}
	}
}