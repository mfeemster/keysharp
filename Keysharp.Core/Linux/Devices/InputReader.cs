#if LINUX
namespace Keysharp.Core.Linux.Devices
{
	public class InputReader : IDisposable
	{
		private const int BufferLength = 24;

		private readonly byte[] _buffer = new byte[BufferLength];

		private volatile bool _disposing;
		private FileStream _stream;
		public bool Faulted { get; private set; }

		public string Path { get; }

		static InputReader()
		{
		}

		public InputReader(string path)
		{
			Path = path;

			try
			{
				_stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			}
			catch (UnauthorizedAccessException ex)
			{
				_ = Errors.ErrorOccurred($"The current user doesn't have permission to access input data. Add the user to the input group to correct this error: {ex.Message}", null, Keyword_ExitApp);
				Faulted = true;
			}
			catch (IOException ex)
			{
				_ = Errors.ErrorOccurred($"An error occurred while trying to build the stream for {Path}: {ex.Message}", null, Keyword_ExitApp);
				Faulted = true;
			}

			_ = Task.Run(Run);
		}

		public void Dispose()
		{
			_disposing = true;
			_stream?.Dispose();
			_stream = null;
		}

		private short GetCode()
		{
			var codeBits = new[]
			{
				_buffer[18],
				_buffer[19]
			};
			var code = BitConverter.ToInt16(codeBits, 0);
			return code;
		}

		private EventType GetEventType()
		{
			var typeBits = new[]
			{
				_buffer[16],
				_buffer[17]
			};
			var type = BitConverter.ToInt16(typeBits, 0);
			var eventType = (EventType)type;
			return eventType;
		}

		private int GetValue()
		{
			var valueBits = new[]
			{
				_buffer[20],
				_buffer[21],
				_buffer[22],
				_buffer[23]
			};
			var value = BitConverter.ToInt32(valueBits, 0);
			return value;
		}

		private void HandleKeyPressEvent(short code, int value)
		{
			var c = (EventCode)code;
			var s = (KeyState)value;
			var e = new KeyPressEvent(c, s);
			OnKeyPress?.Invoke(e);
		}

		private void Run()
		{
			while (!_disposing)
			{
				try
				{
					if (!Faulted)
					{
						_ = _stream.Read(_buffer, 0, BufferLength);
					}
				}
				catch (IOException ex)
				{
					_ = Errors.ErrorOccurred($"Error occured while trying to read from the stream for {Path}: {ex.Message}", null, Keyword_ExitApp);
					Faulted = true;
				}

				var type = GetEventType();
				var code = GetCode();
				var value = GetValue();

				switch (type)
				{
					case EventType.EV_KEY:
						HandleKeyPressEvent(code, value);
						break;

					case EventType.EV_REL:
						var axis = (MouseAxis)code;
						var e = new MouseMoveEvent(axis, value);
						OnMouseMove?.Invoke(e);
						break;
				}
			}
		}

		public delegate void RaiseKeyPress(KeyPressEvent e);

		public delegate void RaiseMouseMove(MouseMoveEvent e);

		public event RaiseKeyPress OnKeyPress;

		public event RaiseMouseMove OnMouseMove;
	}
}
#endif