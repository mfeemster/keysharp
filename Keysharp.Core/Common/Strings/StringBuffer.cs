using System.Configuration;
using System.Drawing;

namespace Keysharp.Core.Common.Strings
{
	unsafe public class StringBuffer : KeysharpObject
	{
		private byte* _buffer; // pointer to unmanaged UTF-16 buffer
		private long _capacity = 0; // in wchar (char) units
		private long _position = 0;  // current position in the buffer when appending characters
		private Encoding _encoding;
		private int _bytesPerChar;

		public StringBuffer(params object[] args) : base(args) { }

		~StringBuffer()
		{
			if (_buffer != null)
				NativeMemory.Free(_buffer);
		}

		public static implicit operator string(StringBuffer s) => 
			s._bytesPerChar == 1 ? Marshal.PtrToStringAnsi((IntPtr)s._buffer, (int)s._capacity) : Marshal.PtrToStringUni((IntPtr)s._buffer, (int)s._capacity);

		public override object __New(params object[] args)
		{
			var str = args.Length > 0 ? args[0].ToString() : "";
			var capacity = args.Length > 1 && args[1] != null ? args[1].Ai() + 1 : Math.Max(str.Length + 1, 256);
			_encoding = args.Length > 2 && args[2].As().Equals("ANSI", StringComparison.OrdinalIgnoreCase) ? Encoding.Default : Encoding.Unicode;
			_bytesPerChar = _encoding == Encoding.Unicode ? sizeof(char) : 1;
			_capacity = capacity;
			_buffer = (byte*)NativeMemory.Alloc((nuint)(_capacity * _bytesPerChar));
			Append(str);
			return "";
		}

		public long Ptr
		{
			get {
				UpdateBufferFromEntangledString();
				return (long)_buffer;
			}
		}

		public object Capacity
		{
			get => _capacity;
			set
			{
				var newCapacity = (long)value;
				// ReAllocHGlobal will preserve existing bytes up to the new size
				_buffer = (byte*)NativeMemory.Realloc(_buffer, (nuint)((newCapacity + 1) * _bytesPerChar));
				if (newCapacity < _capacity)
				{
					NativeMemory.Clear(_buffer + (newCapacity * _bytesPerChar), (nuint)_bytesPerChar);
				}
				_capacity = newCapacity;
			}
		}

		public object Append(string text)
		{
			if (text == null) throw new Error("String cannot be unset");
			int len = text.Length;
			EnsureCapacity(_position + len);

			if (_bytesPerChar == 1)
			{
				byte[] bytes = _encoding.GetBytes(text);
				fixed (byte* src = bytes)
				{
					NativeMemory.Copy(src, _buffer + _position, (nuint)bytes.Length);
				}
				_position += len;

				_buffer[_position] = 0;
			} else
			{
				// Copy chars
				char[] bytes = text.ToCharArray();
				fixed (char* src = bytes)
				{
					NativeMemory.Copy(src, _buffer + (nint)_position * _bytesPerChar, (nuint)bytes.Length);
				}

				_position += len;

				*((char*)(_buffer + _position * _bytesPerChar)) = '\0';
			}
			return _position;
		}

		public object AppendLine(string text = "")
		{
			Append(text);
			return Append(Environment.NewLine);
		}

		public object Clear()
		{
			_position = 0;
			if (_bytesPerChar == 1)
				_buffer[0] = 0;
			else
				*((char*)_buffer) = '\0';
			return "";
		}

		private void EnsureCapacity(long requiredCapacity)
		{
			if (requiredCapacity > _capacity)
				Capacity = requiredCapacity;
		}

		public object EntangledString { get; set; }

		public object UpdateEntangledStringFromBuffer() => EntangledString != null ? Script.SetPropertyValue(EntangledString, "__Value", ToString()) : null;
		public object UpdateBufferFromEntangledString()
		{
			if (EntangledString == null)
				return null;
			var str = Script.GetPropertyValue(EntangledString, "__Value") as string;
			str ??= "";
			var requiredCapacity = Math.Max(_capacity, str.Length);
			EnsureCapacity(requiredCapacity);
			Clear();
			Append(str);
			return str;
		}

		public override void PrintProps(string name, StringBuffer sbuf, ref int tabLevel)
		{
			var indent = new string('\t', tabLevel);
			var fieldType = GetType().Name;
			var str = ToString();

			if (name.Length == 0)
				_ = sbuf.AppendLine($"{indent}{str} ({fieldType})");
			else
				_ = sbuf.AppendLine($"{indent}{name}: {str} ({fieldType})");
		}

		public override string ToString() => (string)this;
	}
}