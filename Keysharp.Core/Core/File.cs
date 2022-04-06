using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Keysharp.Core
{
	public class File : IDisposable
	{
		internal Encoding enc;
		internal int eolconv = 0;
		private BinaryReader br;
		private BinaryWriter bw;
		private bool disposed = false;
		private FileStream fs;
		private TextReader tr;
		private TextWriter tw;

		public long AtEOF
		{
			get
			{
				if (br != null)
					return br.PeekChar() == -1 ? 1 : 0;
				else if (tr != null)
					return tr.Peek() == -1 ? 1 : 0;
				else
					return 0;
			}
		}

		public string Encoding
		{
			get => enc.BodyName;
			set => enc = GetEncoding(value);
		}

		public long Handle => fs != null ? fs.SafeFileHandle.DangerousGetHandle().ToInt64() : 0;

		public long Length
		{
			get => fs != null ? fs.Length : 0;
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
					return 0;
			}

			set => Seek(value);
		}

		public File(params object[] obj)
		{
			var l = obj.L();
			var (filename, mode, access, share) = l.Si3();
			var m = (FileMode)mode;
			var a = (FileAccess)access;
			var s = (FileShare)share;
			enc = (Encoding)l[4];
			eolconv = (int)l[5];

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
							br.BaseStream.Seek(u8.Preamble.Length, SeekOrigin.Begin);
					}
					else if (enc is UnicodeEncoding u16)
					{
						if (u16.Preamble.Length > 0)
							br.BaseStream.Seek(u16.Preamble.Length, SeekOrigin.Begin);
					}
				}
			}
		}

		~File() => Dispose(false);

		public static void FileEncoding(params object[] obj)
		{
			var s = obj.L().S1();

			if (s != "")
				Accessors.A_FileEncoding = s;
		}

		public static Encoding GetEncoding(object s)
		{
			var val = s.ToString().ToLowerInvariant();
			Encoding tempenc;

			if (val.StartsWith("cp"))
				return System.Text.Encoding.GetEncoding(val.Substring(2).ParseInt().Value);

			if (int.TryParse(val, out var cp))
				return System.Text.Encoding.GetEncoding(cp);

			switch (val)
			{
				case "ascii":
				case "us-ascii":
					return System.Text.Encoding.ASCII;

				case "utf-8":
					return System.Text.Encoding.UTF8;

				case "utf-8-raw":
					return new UTF8Encoding(false);//No byte order mark.

				case "utf-16":
				case "unicode":
					return System.Text.Encoding.Unicode;

				case "utf-16-raw":
					return new UnicodeEncoding(false, false);//Little endian, no byte order mark.
			}

			try
			{
				tempenc = System.Text.Encoding.GetEncoding(val);
				return tempenc;
			}
			catch
			{
			}

			return System.Text.Encoding.Unicode;
		}

		public void Close(params object[] obj)
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

		public void RawRead(params object[] obj)
		{
			var (buf, count) = obj.Oi(null, int.MinValue);

			if (br != null)
			{
				byte[] val;

				if (buf is Array arr)
				{
					val = count != int.MinValue ? br.ReadBytes(count) : br.ReadBytes(arr.Count);
					var len = Math.Min(val.Length, arr.Count);

					for (var i = 0; i < len; i++)
						arr[i + 1] = val[i];//Array always expects a 1-based index.
				}
				else if (buf is Buffer buffer)
				{
					var size = (int)(long)buffer.Size;
					val = count != int.MinValue ? br.ReadBytes(count) : br.ReadBytes(size);
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

		public long RawWrite(params object[] obj)
		{
			var (buf, count) = obj.Oi(null, int.MinValue);
			var len = 0;

			if (bw != null)
			{
				if (buf is Buffer buffer)
				{
					len = (int)(count != int.MinValue ? Math.Min((long)buffer.Size, count) : (long)buffer.Size);
					unsafe
					{
						var bytes = new byte[len];
						Marshal.Copy(buffer.Ptr, bytes, 0, len);
						bw.Write(bytes);
					}
				}
				else if (buf is Array arr)
				{
					len = count != int.MinValue ? Math.Min(arr.Count, count) : arr.Count;
					bw.Write((byte[])arr.array.ToArray(typeof(byte)), 0, len);//No way to know what is in the array since they are objects, so just assume they are bytes.
				}
				else if (buf is string s)
				{
					var bytes = enc.GetBytes(s);
					len = count != int.MinValue ? Math.Min(bytes.Length, count) : bytes.Length;
					bw.Write(bytes, 0, len);
				}
			}

			return len;
		}

		public string Read(params object[] obj)
		{
			var s = "";
			var count = obj.L().I1();
			char[] buf = null;
			var read = 0;

			if (count > 0)
				buf = new char[count];

			if (br != null)
			{
				if (count > 0)
					read = br.Read(buf, 0, count);
				else
					s = br.ReadString();
			}
			else if (tr != null)
			{
				if (count > 0)
					read = tr.Read(buf, 0, count);
				else
					s = tr.ReadToEnd();
			}

			if (read > 0)
				s = new string(buf, 0, read);

			s = HandleReadEol(s);
			return s;
		}

		public object ReadChar(params object[] obj) => br != null ? (object)br.ReadByte() : "";

		public object ReadDouble(params object[] obj) => br != null ? (object)br.ReadDouble() : "";

		public object ReadFloat(params object[] obj) => br != null ? (object)br.ReadSingle() : "";

		public object ReadInt(params object[] obj) => br != null ? (object)br.ReadInt32() : "";

		public object ReadInt64(params object[] obj) => br != null ? (object)br.ReadInt64() : "";

		public string ReadLine(params object[] obj)
		{
			var s = "";

			if (br != null)
				s = br.ReadLine();
			else if (tr != null)
				s = tr.ReadLine();

			return s;
		}

		public object ReadShort(params object[] obj) => br != null ? (object)br.ReadInt16() : "";

		//Char in this case is meant to be 1 byte, according to the AHK DllCall() documentation.
		public object ReadUChar(params object[] obj) => br != null ? (object)br.ReadByte() : "";

		public object ReadUInt(params object[] obj) => br != null ? (object)br.ReadUInt32() : "";

		public object ReadUShort(params object[] obj) => br != null ? (object)br.ReadUInt16() : "";

		public void Seek(params object[] obj)
		{
			var (distance, origin) = obj.L().L2(0, long.MinValue);
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
				br.BaseStream.Seek(distance, so);
			else if (bw != null)//Only need to do 1, because they both have the same underlying stream.
				bw.Seek((int)distance, so);
		}

		public long Write(params object[] obj)
		{
			var s = obj.L().S1();
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

		public long WriteChar(params object[] obj)
		{
			if (bw != null)
			{
				bw.Write((byte)obj.L().L1());//Char in this case is meant to be 1 byte, according to the AHK DllCall() documentation.
				return 1;
			}
			else
				return 0;
		}

		public long WriteDouble(params object[] obj)
		{
			if (bw != null)
			{
				bw.Write(obj.L().D1());
				return 8;
			}
			else
				return 0;
		}

		public long WriteFloat(params object[] obj)
		{
			if (bw != null)
			{
				bw.Write((float)obj.L().D1());
				return 4;
			}
			else
				return 0;
		}

		public long WriteInt(params object[] obj)
		{
			if (bw != null)
			{
				bw.Write((int)obj.L().L1());
				return 4;
			}
			else
				return 0;
		}

		public long WriteInt64(params object[] obj)
		{
			if (bw != null)
			{
				bw.Write(obj.L().L1());
				return 8;
			}
			else
				return 0;
		}

		public long WriteLine(params object[] obj)
		{
			var s = obj.L().S1();
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

		public long WriteShort(params object[] obj)
		{
			if (bw != null)
			{
				bw.Write((short)obj.L().L1());
				return 2;
			}
			else
				return 0;
		}

		public long WriteUChar(params object[] obj)
		{
			if (bw != null)
			{
				bw.Write((byte)obj.L().L1());
				return 1;
			}
			else
				return 0;
		}

		public long WriteUInt(params object[] obj)
		{
			if (bw != null)
			{
				bw.Write((uint)obj.L().L1());
				return 4;
			}
			else
				return 0;
		}

		public long WriteUShort(params object[] obj)
		{
			if (bw != null)
			{
				bw.Write((ushort)obj.L().L1());
				return 2;
			}
			else
				return 0;
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