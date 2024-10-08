#if LINUX
namespace Keysharp.Core.Linux.Devices
{
	public class AggregateInputReader : IDisposable
	{
		private Dictionary<string, InputReader> _readers = new ();

		public event InputReader.RaiseKeyPress OnKeyPress;

		public AggregateInputReader()
		{
			//var timer = new System.Timers.Timer();
			//timer.Interval = 10 * 1000;
			//timer.Enabled = true;
			//timer.Elapsed += (_, _) => Scan();
			//timer.Start();
			Scan();
		}

		private void ReaderOnOnKeyPress(KeyPressEvent e)
		{
			OnKeyPress?.Invoke(e);
		}

		private void Scan()
		{
			var files = Directory.GetFiles("/dev/input/", "event*");

			foreach (var file in files)
			{
				if (_readers.ContainsKey(file))
				{
					continue;
				}

				var reader = new InputReader(file);
				reader.OnKeyPress += ReaderOnOnKeyPress;
				_readers.Add(file, reader);
			}

			var deadReaders = _readers.Values.Where(r => r.Faulted);

			foreach (var dr in deadReaders)
			{
				_ = _readers.Remove(dr.Path);
				dr.OnKeyPress -= ReaderOnOnKeyPress;
				dr.Dispose();
			}
		}

		public void Dispose()
		{
			foreach (var d in _readers.Values)
			{
				d.OnKeyPress -= ReaderOnOnKeyPress;
				d.Dispose();
			}

			_readers = null;
		}
	}
}
#endif