namespace Keysharp.Core.Common.File
{
	public class KeysharpFile : IDisposable
	{
		internal Encoding enc;

		internal int eolconv = 0;

		private readonly BinaryReader br;

		private readonly BinaryWriter bw;

		private bool disposed = false;

		private readonly FileStream fs;

		private readonly TextReader tr;

		private readonly TextWriter tw;

		public long AtEOF
		{
			get
			{
				if (br != null)
					return br.PeekChar() == -1 ? 1L : 0L;
				else if (tr != null)
					return tr.Peek() == -1 ? 1L : 0L;
				else
					return 0L;
			}
		}

		public string Encoding
		{
			get => enc.BodyName;
			set => enc = Files.GetEncoding(value);
		}

		public long Handle => fs != null ? fs.SafeFileHandle.DangerousGetHandle().ToInt64() : 0;

		public long Length
		{
			get => fs != null ? fs.Length : 0L;
			set => fs?.SetLength(value);
		}

		public long Pos
		{
			get
			{
				if (br != null)
					return br.BaseStream.Position;
				else if (bw != null)
					return bw.BaseStream.Position;
				else
					return 0L;
			}

			set => Seek(value);
		}

		internal KeysharpFile(string filename, FileMode mode, FileAccess access, FileShare share, Encoding encoding, long eol)
		{
			var m = mode;
			var a = access;
			var s = share;
			enc = encoding;
			eolconv = (int)eol;

			if (filename == "*")
			{
				if ((a & FileAccess.Read) == FileAccess.Read)
					tr = Console.In;

				if ((a & FileAccess.Write) == FileAccess.Write)
					tw = Console.Out;
			}
			else if (filename == "**")
			{
				if ((a & FileAccess.Read) == FileAccess.Read)
					tr = Console.In;

				if ((a & FileAccess.Write) == FileAccess.Write)
					tw = Console.Error;
			}
			else
			{
				var exists = false;

				if (filename.StartsWith("h*"))
				{
					var handle = filename.Substring(2).ParseLong(false);

					if (handle.HasValue)
					{
						exists = true;
						fs = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(handle.Value), false), a, 4096);
					}
				}
				else
				{
					if (System.IO.File.Exists(filename))
						exists = true;

					fs = new FileStream(filename, m, a, s);
				}

				if ((a & FileAccess.Read) == FileAccess.Read)
					br = new BinaryReader(fs, enc);

				if ((a & FileAccess.Write) == FileAccess.Write)
					bw = new BinaryWriter(fs, enc);

				if (!exists && bw != null)
				{
					if (enc is UTF8Encoding u8)
					{
						if (u8.Preamble.Length > 0)
							bw.Write(u8.Preamble);
					}
					else if (enc is UnicodeEncoding u16)
					{
						if (u16.Preamble.Length > 0)
							bw.Write(u16.Preamble);
					}
				}
				else if (exists && br != null)
				{
					if (enc is UTF8Encoding u8)
					{
						if (u8.Preamble.Length > 0)
							_ = br.BaseStream.Seek(u8.Preamble.Length, SeekOrigin.Begin);
					}
					else if (enc is UnicodeEncoding u16)
					{
						if (u16.Preamble.Length > 0)
							_ = br.BaseStream.Seek(u16.Preamble.Length, SeekOrigin.Begin);
					}
				}
			}
		}

		~KeysharpFile() => Dispose(false);

		public void Close()
		{
			br?.Close();
			bw?.Close();
			tr?.Close();
			tw?.Close();
			fs?.Close();
		}

		public virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				Close();
				disposed = true;
			}
		}

		public void RawRead(object obj0, object obj1 = null)
		{
			var buf = obj0;
			var count = obj1.Al(long.MinValue);

			if (br != null)
			{
				byte[] val;

				if (buf is Array arr)
				{
					val = count != long.MinValue ? br.ReadBytes((int)count) : br.ReadBytes(arr.Count);
					var len = Math.Min(val.Length, arr.Count);

					for (var i = 0; i < len; i++)
						arr.array[i] = val[i];//Access the underlying ArrayList directly for performance.
				}
				else if (buf is Buffer buffer)
				{
					var size = (int)(long)buffer.Size;
					val = count != long.MinValue ? br.ReadBytes((int)count) : br.ReadBytes(size);
					var len = Math.Min(val.Length, size);
					unsafe
					{
						var ptr = (byte*)buffer.Ptr.ToPointer();

						for (var i = 0; i < len; i++)
							ptr[i] = val[i];
					}
				}
			}
		}

		public long RawWrite(object obj0, object obj1 = null)
		{
			var buf = obj0;
			var count = obj1.Al(long.MinValue);
			var len = 0;

			if (bw != null)
			{
				if (buf is Buffer buffer)
				{
					len = (int)(count != long.MinValue ? Math.Min((long)buffer.Size, count) : (long)buffer.Size);
					unsafe
					{
						var bytes = new byte[len];
						Marshal.Copy(buffer.Ptr, bytes, 0, len);
						bw.Write(bytes);
					}
				}
				else if (buf is Array arr)
				{
					len = count != long.MinValue ? Math.Min(arr.Count, (int)count) : arr.Count;
					bw.Write(arr.array.ConvertAll(el => (byte)el.ParseLong(false).Value).ToArray(), 0, len);//No way to know what is in the array since they are objects, so convert them to bytes.
				}
				else if (buf is string s)
				{
					var bytes = enc.GetBytes(s);
					len = count != long.MinValue ? Math.Min(bytes.Length, (int)count) : bytes.Length;
					bw.Write(bytes, 0, len);
				}
			}

			return len;
		}

		public string Read(object obj)
		{
			var s = "";
			var count = obj.Al();
			char[] buf = null;
			var read = 0;

			if (count > 0)
				buf = new char[count];

			if (br != null)
			{
				if (count > 0)
					read = br.Read(buf, 0, (int)count);
				else
					s = br.ReadString();
			}
			else if (tr != null)
			{
				if (count > 0)
					read = tr.Read(buf, 0, (int)count);
				else
					s = tr.ReadToEnd();
			}

			if (read > 0)
				s = new string(buf, 0, read);

			s = HandleReadEol(s);
			return s;
		}

		public object ReadChar() => br != null ? br.ReadByte() : "";

		public object ReadDouble() => br != null ? br.ReadDouble() : "";

		public object ReadFloat() => br != null ? br.ReadSingle() : "";

		public object ReadInt() => br != null ? br.ReadInt32() : "";

		public object ReadInt64() => br != null ? br.ReadInt64() : "";

		public string ReadLine()
		{
			var s = "";

			if (br != null)
				s = br.ReadLine();
			else if (tr != null)
				s = tr.ReadLine();

			return s;
		}

		public object ReadShort() => br != null ? br.ReadInt16() : "";

		//Char in this case is meant to be 1 byte, according to the AHK DllCall() documentation.
		public object ReadUChar() => br != null ? br.ReadByte() : "";

		public object ReadUInt() => br != null ? br.ReadUInt32() : "";

		public object ReadUShort() => br != null ? br.ReadUInt16() : "";

		public void Seek(object obj0, object obj1 = null)
		{
			var distance = obj0.Al();
			var origin = obj1.Al(long.MinValue);
			SeekOrigin so;

			if (origin == 0)
				so = SeekOrigin.Begin;
			else if (origin == 1)
				so = SeekOrigin.Current;
			else if (origin == 2)
				so = SeekOrigin.End;
			else if (distance < 0)
				so = SeekOrigin.End;
			else
				so = SeekOrigin.Begin;

			if (br != null)
				_ = br.BaseStream.Seek(distance, so);
			else if (bw != null)//Only need to do 1, because they both have the same underlying stream.
				_ = bw.Seek((int)distance, so);
		}

		public long Write(object obj)
		{
			var s = obj.As();
			var len = 0L;

			if (bw != null)
			{
				s = HandleWriteEol(s);
				var bytes = enc.GetBytes(s);
				bw.Write(bytes);
				len = bytes.Length;
			}
			else if (tw != null)
			{
				tw.Write(s);
				len = enc.GetByteCount(s);
			}

			return len;
		}

		public long WriteChar(object obj)
		{
			if (bw != null)
			{
				bw.Write((byte)obj.Al());//Char in this case is meant to be 1 byte, according to the AHK DllCall() documentation.
				return 1L;
			}
			else
				return 0L;
		}

		public long WriteDouble(object obj)
		{
			if (bw != null)
			{
				bw.Write(obj.Ad());
				return 8L;
			}
			else
				return 0L;
		}

		public long WriteFloat(object obj)
		{
			if (bw != null)
			{
				bw.Write((float)obj.Ad());
				return 4L;
			}
			else
				return 0L;
		}

		public long WriteInt(object obj)
		{
			if (bw != null)
			{
				bw.Write(obj.Ai());
				return 4L;
			}
			else
				return 0L;
		}

		public long WriteInt64(object obj)
		{
			if (bw != null)
			{
				bw.Write(obj.Al());
				return 8L;
			}
			else
				return 0L;
		}

		public long WriteLine(object obj)
		{
			var s = obj.As();
			byte[] bytes;
			var len = 0L;

			if (s != "")
				len = Write(s);

			s = eolconv == 4 ? "\r\n" : "\n";

			if (bw != null)
			{
				bytes = enc.GetBytes(s);
				bw.Write(bytes);
				len += bytes.Length;
			}
			else if (tw != null)
			{
				tw.Write(s);
				len += enc.GetByteCount(s);
			}

			return len;
		}

		public long WriteShort(object obj)
		{
			if (bw != null)
			{
				bw.Write((short)obj.Al());
				return 2L;
			}
			else
				return 0L;
		}

		public long WriteUChar(object obj)
		{
			if (bw != null)
			{
				bw.Write((byte)obj.Al());
				return 1L;
			}
			else
				return 0L;
		}

		public long WriteUInt(object obj)
		{
			if (bw != null)
			{
				bw.Write((uint)obj.Al());
				return 4L;
			}
			else
				return 0L;
		}

		public long WriteUShort(object obj)
		{
			if (bw != null)
			{
				bw.Write((ushort)obj.Al());
				return 2L;
			}
			else
				return 0L;
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private string HandleReadEol(string s)
		{
			if (eolconv == 4)
				s = s.Replace("\r\n", "\n");
			else if (eolconv == 8)
				s = s.Replace("\r", "\n");

			return s;
		}

		private string HandleWriteEol(string s)
		{
			if (eolconv == 4)
				s = s.Replace("\n", "\r\n");

			return s;
		}
	}
}