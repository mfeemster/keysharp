namespace Keysharp.Core
{
	public static class Errors
	{
		public static Error Error(params object[] obj) => new (obj);

		public static bool ErrorOccurred(Error err)
		{
			if (Script.onErrorHandlers != null)
			{
				foreach (var handler in Script.onErrorHandlers)
				{
					var result = handler.Call(err, err.ExcType);

					if (result.IsCallbackResultNonEmpty() && result.ParseLong(false) == 1L)
						return false;
				}
			}

			if (err.ExcType == Keywords.Keyword_ExitApp)
				_ = Flow.ExitAppInternal(Flow.ExitReasons.Critical);

			return err.ExcType != Keywords.Keyword_Return;//Don't report an error if it was just an exit from a thread.
		}

		public static IndexError IndexError(params object[] obj) => new (obj);

		public static KeyError KeyError(params object[] obj) => new (obj);

		public static MemberError MemberError(params object[] obj) => new (obj);

		public static MemoryError MemoryError(params object[] obj) => new (obj);

		public static MethodError MethodError(params object[] obj) => new (obj);

		public static void OnError(object obj0, object obj1 = null)
		{
			var e = obj0;
			var i = obj1.Al(1L);
			var del = Functions.GetFuncObj(e, null, true);

			if (Script.onErrorHandlers == null)
				Script.onErrorHandlers = new List<IFuncObj>();

			Script.onErrorHandlers.ModifyEventHandlers(del, i);
		}

		public static OSError OSError(params object[] obj) => new (obj);

		public static PropertyError PropertyError(params object[] obj) => new (obj);

		public static TargetError TargetError(params object[] obj) => new (obj);

		public static TimeoutError TimeoutError(params object[] obj) => new (obj);

		public static TypeError TypeError(params object[] obj) => new (obj);

		public static UnsetItemError UnsetItemError(params object[] obj) => new (obj);

		public static ValueError ValueError(params object[] obj) => new (obj);

		public static ZeroDivisionError ZeroDivisionError(params object[] obj) => new (obj);
	}

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

		//Must be ExcType and not Type, else the reflection dictionary sees it as a dupe from the base.
		public string ExcType { get; set; } = Keywords.Keyword_Exit;

		public string Extra { get; set; }
		public string File { get; set; }
		public long Line { get; set; }

		//public new string Message => message;
		public override string Message => ToString();//Must be done this way, else the reflection dictionary sees it as a dupe from the base.

		public string RawMessage => message;
		public string Stack { get; set; }
		public string What { get; set; }

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
			Stack = FixStackTrace(new StackTrace(frame).ToString());
		}

		public override string ToString()
		{
			var st = FixStackTrace(StackTrace);
			var sb = new StringBuilder(512);
			_ = sb.AppendLine($"\tMessage: {message}");
			_ = sb.AppendLine($"\tWhat: {What}");
			_ = sb.AppendLine($"\tExtra/Code: {Extra}");
			_ = sb.AppendLine($"\tFile: {File}");
			_ = sb.AppendLine($"\tLine: {Line}");
			_ = sb.AppendLine($"\tLocal stack:\n\t\t{Stack}\n");
			_ = sb.AppendLine($"\tFull stack:\n\t\t{st}");
			return sb.ToString();
		}

		/// <summary>
		/// Strip out the full paths and only show the file names.
		/// </summary>
		/// <param name="stack"></param>
		/// <returns></returns>
		private static string FixStackTrace(string stack)
		{
			var delim = " in ";
			var lines = stack.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			for (var i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				var firstIn = line.IndexOf(delim) + delim.Length;
				var lastColon = line.LastIndexOf(':');

				if (firstIn != -1 && lastColon > firstIn)
				{
					var path = line.Substring(firstIn, lastColon - firstIn);
					var filename = Path.GetFileName(path);
					lines[i] = line.Replace(path, filename).Replace(".cs:line", ".cs, line");
				}
			}

			return string.Join("\n\t\t", lines);
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
			: this(message, default, "") { }

		public ParseException(string message, CodeLine codeLine)
			: this(message, codeLine.LineNumber, codeLine.Code, codeLine.FileName) { }

		public ParseException(string message, int line, string code)
			: this(message, line, code, "") { }

		public ParseException(string message, int line, string code, string file)
			: base(message)
		{
			Line = line;
			File = file;
			Extra = code;
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