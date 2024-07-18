namespace Keysharp.Core
{
	public static class External
	{
		/// <summary>
		/// Returns a binary number stored at the specified address in memory.
		/// </summary>
		/// <param name="address">The address in memory.</param>
		/// <param name="offset">The offset from <paramref name="address"/>.</param>
		/// <param name="type">Any type outlined in <see cref="DllCall"/>.</param>
		/// <returns>The value stored at the address.</returns>
		public static object NumGet(object obj0, object obj1, object obj2 = null)
		{
			var address = obj0;
			object offset = null;
			string type;

			if (obj2 == null)
			{
				offset = 0;
				type = obj1.As("UInt");
			}
			else
			{
				offset = obj1 is IntPtr ip ? ip.ToInt32() : obj1.Ai();
				type = obj2.As("UInt");
			}

			IntPtr addr;
			var off = (int)offset;
			var buf = address as Buffer;
			type = type.ToLower();

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
			else if (type == "ptr" && address is ComObject co)
			{
				var pUnk = Marshal.GetIUnknownForObject(co.Ptr);
				addr = pUnk;//Don't dererference here, it'll be done below.
				_ = Marshal.Release(pUnk);
			}
			else if (type == "ptr" && Marshal.IsComObject(address))
			{
				var pUnk = Marshal.GetIUnknownForObject(address);
				addr = pUnk;//Ditto.
				_ = Marshal.Release(pUnk);
			}
			else
				throw new TypeError($"Could not convert address argument of type {address.GetType()} into an IntPtr. Type must be int, long, ComObject or Buffer.");

#else
			else
				throw new TypeError($"Could not convert address argument of type {address.GetType()} into an IntPtr. Type must be int, long, or Buffer.");

#endif

			switch (type)
			{
				case "uint":
					if (buf != null && (off + 4 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}.");

					return (long)(uint)Marshal.ReadInt32(addr, off);

				case "int":
					if (buf != null && (off + 4 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}.");

					return (long)Marshal.ReadInt32(addr, off);

				case "short":
					if (buf != null && (off + 2 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 2 > buffer size {(long)buf.Size}.");

					return (long)Marshal.ReadInt16(addr, off);

				case "ushort":
					if (buf != null && (off + 2 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 2 > buffer size {(long)buf.Size}.");

					return (long)(ushort)Marshal.ReadInt16(addr, off);

				case "char":
					if (buf != null && (off + 1 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 1 > buffer size {(long)buf.Size}.");

					return (long)(sbyte)Marshal.ReadByte(addr, off);

				case "uchar":
					if (buf != null && (off + 1 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 1 > buffer size {(long)buf.Size}.");

					return (long)Marshal.ReadByte(addr, off);

				case "double":
					if (buf != null && (off + 8 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 8 > buffer size {(long)buf.Size}.");

					unsafe
					{
						var ptr = (double*)(addr + off).ToPointer();
						var val = *ptr;
						return val;
					}

				case "float":
					if (buf != null && (off + 4 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}.");

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
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 8 > buffer size {(long)buf.Size}.");

					var ipoff = IntPtr.Add(addr, off);
					return Marshal.ReadIntPtr(ipoff).ToInt64();//Dereference here.
			}
		}

		public static long NumPut(params object[] obj)
		{
			IntPtr addr = IntPtr.Zero;
			Buffer buf;
			var offset = 0;
			int lastPairIndex;
			var offsetSpecified = !((obj.Length & 1) == 1);
			object target = null;

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
						bytes = BitConverter.GetBytes((int)Convert.ToInt64(number));
						inc = 4;
						break;

					case "uint":
						bytes = BitConverter.GetBytes((uint)Convert.ToUInt64(number));
						inc = 4;
						break;

					case "float":
						bytes = BitConverter.GetBytes(Convert.ToSingle(number));
						inc = 4;
						break;

					case "short":
						bytes = BitConverter.GetBytes((short)Convert.ToInt64(number));
						inc = 2;
						break;

					case "ushort":
						bytes = BitConverter.GetBytes((ushort)Convert.ToUInt64(number));
						inc = 2;
						break;

					case "char":
						bytes = new byte[] { (byte)Convert.ToInt32(number) };
						inc = 1;
						break;
					case "uchar":
						bytes = new byte[] { (byte)Convert.ToInt32(number) };
						inc = 1;
						break;
					case "double":
						bytes = BitConverter.GetBytes(Convert.ToDouble(number));
						inc = 8;
						break;

					case "int64":
						bytes = BitConverter.GetBytes(Convert.ToInt64(number));
						inc = 8;
						break;

					case "uint64":
					case "ptr":
					case "uptr":
						bytes = BitConverter.GetBytes(Convert.ToUInt64(number));
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
						throw new IndexError($"Memory access exceeded buffer size. Offset {offset} + length {bytes.Length} > buffer size {(long)buf.Size}.");
				}
				else if (addr != IntPtr.Zero)
				{
					Marshal.Copy(bytes, 0, finalAddr, bytes.Length);
					offset += inc;
				}
				else
					throw new IndexError($"Could not parse target {target} as a Buffer or memory address.");
			}

			return IntPtr.Add(addr, offset).ToInt64();
		}
	}
}