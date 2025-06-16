namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for error-related functions and classes.
	/// </summary>
	public static class Errors
	{
		/// <summary>
		/// Creates and returns a new <see cref="Error"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="Error"/> object.</returns>
		public static Error Error(params object[] args) => new (args);

		/// <summary>
		/// Calls all registered error handlers, passing in the exception object to each.
		/// If any callback returns a non-empty result, then no further callbacks are called.
		/// If any callback returns -1 and err.ExcType == "Return", then the thread continues because
		/// the calling code won't throw an exception.
		/// </summary>
		/// <param name="err">The exception object to pass to each callback.</param>
		/// <returns>True if err.ExcType is not "Return", else false.</returns>
		public static bool ErrorOccurred(Error err, string excType = Keyword_Return)
		{
			var exitThread = true;

			if (!err.Processed)
			{
				var script = Script.TheScript;

				if (script.onErrorHandlers != null)
				{
					err.ExcType = excType;

					foreach (var handler in script.onErrorHandlers)
					{
						var result = handler.Call(err, err.ExcType);

						if (result.IsCallbackResultNonEmpty())
						{
							err.Handled = true;

							//Calling code will not throw if this is true.
							if (result.ParseLong(false) == -1L && err.ExcType == Keyword_Return)
								exitThread = false;

							break;
						}
					}
				}

				err.Processed = true;
			}

			if (err.ExcType == Keyword_ExitApp)
				_ = Flow.ExitAppInternal(Flow.ExitReasons.Critical, null, false);

			return exitThread;
		}

		/// <summary>
		/// Internal helper to handle argument type errors. Throws a <see cref="ValueError"/> or returns true.
		/// </summary>
		internal static bool ArgumentErrorOccurred(object arg, int position)
		{
			Error err;
			return ErrorOccurred(err = new ValueError($"Invalid argument of type \"{(arg == null ? "unset" : arg.GetType())}\" at position {position}.")) ? throw err : true;
		}

		/// <summary>
		/// Creates and returns a new <see cref="IndexError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="IndexError"/> object.</returns>
		public static IndexError IndexError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="KeyError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="KeyError"/> object.</returns>
		public static KeyError KeyError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="MemberError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="MemberError"/> object.</returns>
		public static MemberError MemberError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="MemoryError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="MemoryError"/> object.</returns>
		public static MemoryError MemoryError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="MethodError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="MethodError"/> object.</returns>
		public static MethodError MethodError(params object[] args) => new (args);

		/// <summary>
		/// Registers a function to be called automatically whenever an unhandled error occurs.<br/>
		/// The callback accepts two parameters:<br/>
		///     thrown: The thrown value, usually an <see cref="Error"/> object.<br/>
		///     mode: The error mode: Return, Exit, or ExitApp.
		/// </summary>
		/// <param name="callback">The function object to call.</param>
		/// <param name="addRemove">An integer specifying which action to take:<br/>
		///     1: Call the callback after any previously registered callbacks.<br/>
		///    -1: Call the callback before any previously registered callbacks.<br/>
		///     0: Remove the callback if it was already contained in the list.
		/// </param>
		public static object OnError(object callback, object addRemove = null)
		{
			var e = callback;
			var i = addRemove.Al(1L);
			var del = Functions.GetFuncObj(e, null, true);
			var script = Script.TheScript;
			
			if (script.onErrorHandlers == null)
				script.onErrorHandlers = [];

			script.onErrorHandlers.ModifyEventHandlers(del, i);
			return null;
		}

		/// <summary>
		/// Creates and returns a new <see cref="OSError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="OSError"/> object.</returns>
		public static OSError OSError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="PropertyError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="PropertyError"/> object.</returns>
		public static PropertyError PropertyError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="TargetError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="TargetError"/> object.</returns>
		public static TargetError TargetError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="TimeoutError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="TimeoutError"/> object.</returns>
		public static TimeoutError TimeoutError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="TypeError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="TypeError"/> object.</returns>
		public static TypeError TypeError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="UnsetError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="UnsetError"/> object.</returns>
		public static UnsetError UnsetError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="UnsetItemError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="UnsetItemError"/> object.</returns>
		public static UnsetItemError UnsetItemError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="ValueError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="ValueError"/> object.</returns>
		public static ValueError ValueError(params object[] args) => new (args);

		/// <summary>
		/// Creates and returns a new <see cref="ZeroDivisionError"/> exception object.
		/// </summary>
		/// <param name="args">The the parameters to pass to the constructor.</param>
		/// <returns>An <see cref="ZeroDivisionError"/> object.</returns>
		public static ZeroDivisionError ZeroDivisionError(params object[] args) => new (args);
	}

	/// <summary>
	/// A general exception object.
	/// </summary>
	public class Error : KeysharpException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Error"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public Error(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for indexing errors.
	/// </summary>
	public class IndexError : Error
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IndexError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public IndexError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for <see cref="Map"/> key errors.
	/// </summary>
	public class KeyError : IndexError
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public KeyError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// The most base exception class for Keysharp exceptions.
	/// </summary>
	public class KeysharpException : Exception
	{
		/// <summary>
		/// The message.
		/// </summary>
		protected string message = "";

		/// <summary>
		/// Gets or sets the exception exit type.
		/// This is used to determine whether the script should exit or not after an exception is thrown.
		/// Must be ExcType and not Type, else the reflection dictionary sees it as a dupe from the base.
		/// </summary>
		public string ExcType { get; internal set; } = Keyword_Exit;

		/// <summary>
		/// Gets or sets the extra text.
		/// </summary>
		public string Extra { get; internal set; }

		/// <summary>
		/// Gets or sets the file the exception occurred in.
		/// </summary>
		public string File { get; internal set; }

		/// <summary>
		/// Whether this exception has been handled yet.
		/// If true, further error messages will not be shown.
		/// This should only ever be used internally or by the generated script code.
		/// </summary>
		public bool Handled { get; set; }

		/// <summary>
		/// Gets or sets the line the exception occured on.
		/// </summary>
		public long Line { get; internal set; }

		/// <summary>
		/// Gets or sets the message.
		/// Must be done this way, else the reflection dictionary sees it as a dupe from the base.
		/// </summary>
		public override string Message => ToString();

		/// <summary>
		/// Whether the global error event handlers have been called as a result
		/// of this exception yet.
		/// If true, they won't be called again for this error.
		/// Note, this is separate from Handled above.
		/// This should only ever be used internally or by the generated script code.
		/// </summary>
		public bool Processed { get; set; }

		/// <summary>
		/// Gets or sets the raw message.
		/// </summary>
		public string RawMessage => message;

		/// <summary>
		/// Gets or sets the stack trace of where the exception occurred.
		/// </summary>
		public string Stack { get; private set; }

		/// <summary>
		/// Gets or sets the description of the error that happened.
		/// </summary>
		public string What { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="KeysharpException"/> class.
		/// </summary>
		/// <param name="msg">A message describing the error that occurred.</param>
		/// <param name="what">A message describing what happened.</param>
		/// <param name="extra">Extra text describing in detail what happened.</param>
		public KeysharpException(params object[] args)
		{
			var (msg, what, extra) = args.L().S3();
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

		/// <summary>
		/// Returns a string representation of the details of the exception.
		/// </summary>
		/// <returns>A summary of the exception.</returns>
		public override string ToString()
		{
			var trace = StackTrace ?? new StackTrace(new StackFrame(1)).ToString();
			var st = FixStackTrace(trace);
			var sb = new StringBuilder(512);
			_ = sb.AppendLine($"\tException: {GetType().Name}");
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
		/// <param name="stack">The stack trace string to fix.</param>
		/// <returns>The stack trace string with full paths removed.</returns>
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

	/// <summary>
	/// An exception class for class member errors.
	/// </summary>
	public class MemberError : UnsetError
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MemberError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public MemberError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for class memory errors.
	/// </summary>
	public class MemoryError : Error
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public MemoryError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for class method errors.
	/// </summary>
	public class MethodError : MemberError
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MethodError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public MethodError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for operating system errors.
	/// </summary>
	public class OSError : Error
	{
		/// <summary>
		/// Gets or sets the OS-specific number that corresponds to the error.
		/// </summary>
		public long Number { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="OSError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public OSError(params object[] args)
			: base(args)
		{
#if WINDOWS
			var e = args.Length > 0 ? args[0] as Exception : null;
			Win32Exception w32ex = null;

			if ((w32ex = e as Win32Exception) == null)
				if (e != null)
					w32ex = e.InnerException as Win32Exception;

			Number = w32ex != null ? w32ex.ErrorCode : A_LastError;
			message = new Win32Exception((int)Number).Message;
#else
			Number = A_LastError;
#endif
		}
	}

	/// <summary>
	/// An exception class for parsing errors.
	/// </summary>
	public class ParseException : Error
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ParseException"/> class.
		/// </summary>
		/// <param name="message">The message describing the error.</param>
		public ParseException(string message)
			: this(message, default, "") { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ParseException"/> class.
		/// </summary>
		/// <param name="message">The message describing the error.</param>
		/// <param name="codeLine">The <see cref="CodeLine"/> object describing the line the error occurred on.</param>
		public ParseException(string message, CodeLine codeLine)
			: this(message, codeLine.LineNumber, codeLine.Code, codeLine.FileName) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ParseException"/> class.
		/// </summary>
		/// <param name="message">The message describing the error.</param>
		/// <param name="codeLine">The line number the error occurred on.</param>
		/// <param name="code">The code where the error occurred.</param>
		public ParseException(string message, int line, string code)
			: this(message, line, code, "") { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ParseException"/> class.
		/// </summary>
		/// <param name="message">The message describing the error.</param>
		/// <param name="codeLine">The line number the error occurred on.</param>
		/// <param name="code">The code where the error occurred.</param>
		/// <param name="file">The file the error occurred in.</param>
		public ParseException(string message, int line, string code, string file)
			: base(message)
		{
			Line = line;
			File = file;
			Extra = code;
		}
	}

	/// <summary>
	/// An exception class for class property errors.
	/// </summary>
	public class PropertyError : MemberError
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public PropertyError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for GUI target errors.
	/// </summary>
	public class TargetError : Error
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TargetError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public TargetError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for when <see cref="SendMessage"/> times out.
	/// </summary>
	public class TimeoutError : Error
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public TimeoutError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for when the type of a value is not as expected.
	/// </summary>
	public class TypeError : Error
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TypeError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public TypeError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for when an attempt is made to perform an operation on an empty value.
	/// </summary>
	public class UnsetError : Error
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UnsetError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public UnsetError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for when an attempt is made to read an empty value within a collection.
	/// </summary>
	public class UnsetItemError : UnsetError
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UnsetItemError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public UnsetItemError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for when an incorrect value is used.
	/// </summary>
	public class ValueError : Error
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ValueError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public ValueError(params object[] args)
			: base(args)
		{
		}
	}

	/// <summary>
	/// An exception class for when an attempt to divide or Mod by 0 is made.
	/// </summary>
	public class ZeroDivisionError : Error
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ZeroDivisionError"/> class.
		/// </summary>
		/// <param name="args">The parameters to pass to the base.</param>
		public ZeroDivisionError(params object[] args)
			: base(args)
		{
		}
	}
}