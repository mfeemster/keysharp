using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Keysharp.Scripting;

namespace Keysharp.Core
{
	public class Error : KeysharpException
	{
		public Error(params object[] obj)
			: base(obj)
		{
		}
	}

	public class IndexError : Error
	{
		public IndexError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class KeyError : IndexError
	{
		public KeyError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class KeysharpException : Exception
	{
		protected string message = "";

		public string ExcType { get; set; }
		public string Extra { get; set; }
		public string File { get; set; }
		public long Line { get; set; }

		//public new string Message => message;
		public override string Message => message;//Must be done this way, else the reflection dictionary sees it as a dupe from the base.

		public string Stack { get; set; }
		public string What { get; set; }
		//Must be ExcType and not Type, else the reflection dictionary sees it as a dupe from the base.

		public KeysharpException(params object[] obj)
		{
			var (msg, what, extra) = obj.L().S3();
			var frame = new StackFrame(1);
			var frames = new StackTrace(true).GetFrames();

			foreach (var tempframe in frames)
			{
				var type = tempframe.GetMethod().DeclaringType;

				if (!type.IsSubclassOf(typeof(Exception)))
				{
					frame = tempframe;
					break;
				}
			}

			var meth = frame.GetMethod();
			var s = $"{meth.DeclaringType.FullName}.{meth.Name}()";
			message = msg;
			What = what != "" ? what : s;
			Extra = extra;
			//If this is a parsing error, then File and Line need to be set by the calling code.
			File = frame.GetFileName();
			Line = frame.GetFileLineNumber();
			Stack = new StackTrace(frame).ToString();
		}

		public override string ToString()
		{
			var sb = new StringBuilder(512);
			_ = sb.AppendLine($"Message: {Message}\n");
			_ = sb.AppendLine($"What: {What}\n");
			_ = sb.AppendLine($"Extra: {Extra}\n");
			_ = sb.AppendLine($"File: {File}\n");
			_ = sb.AppendLine($"Line: {Line}\n");
			_ = sb.AppendLine($"Local stack: {Stack}");
			_ = sb.AppendLine($"Full stack: {StackTrace}");
			return sb.ToString();
		}
	}

	public class MemberError : UnsetError
	{
		public MemberError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class MemoryError : Error
	{
		public MemoryError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class MethodError : MemberError
	{
		public MethodError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class OSError : Error
	{
		public long Number { get; set; }

		public OSError(params object[] obj)
			: base(obj)
		{
			var e = obj.Length > 0 ? obj[0] as Exception : null;
			Win32Exception w32ex = null;

			if ((w32ex = e as Win32Exception) == null)
				if (e != null)
					w32ex = e.InnerException as Win32Exception;

			Number = w32ex != null ? w32ex.ErrorCode : Accessors.A_LastError;
			message = new Win32Exception((int)Number).Message;
		}
	}

	public class ParseException : Error
	{
		public ParseException(string message)
			: this(message, default(int)) { }

		public ParseException(string message, CodeLine line)
			: this(message, line.LineNumber, line.FileName) { }

		public ParseException(string message, int line)
			: this(message, line, "") { }

		public ParseException(string message, int line, string file)
			: base(message)
		{
			Line = line;
			File = file;
		}
	}

	public class PropertyError : MemberError
	{
		public PropertyError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class TargetError : Error
	{
		public TargetError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class TimeoutError : Error
	{
		public TimeoutError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class TypeError : Error
	{
		public TypeError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class UnsetError : Error
	{
		public UnsetError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class UnsetItemError : UnsetError
	{
		public UnsetItemError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class ValueError : Error
	{
		public ValueError(params object[] obj)
			: base(obj)
		{
		}
	}

	public class ZeroDivisionError : Error
	{
		public ZeroDivisionError(params object[] obj)
			: base(obj)
		{
		}
	}
}