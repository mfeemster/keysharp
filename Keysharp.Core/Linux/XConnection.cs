#if LINUX
namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Singleton class to keep track of all active windows and their
	/// events to help the hotkey and window management code
	/// </summary>
	internal class XConnectionSingleton : IDisposable
	{
		// These are the events we subscribe to, in order to allow hotkey/hotstring support
		private static readonly EventMasks selectMask = EventMasks.KeyPress |
				EventMasks.FocusChange | EventMasks.SubstructureNofity |
				EventMasks.KeyRelease | EventMasks.Exposure;

		private static XConnectionSingleton instance;//Should make this part of an instance data class like we do on windows.//TODO
		private Thread listener;

		private XErrorHandler oldHandler;

		private bool run = true;

		private bool success = true;

		// HACK: X sometimes throws a BadWindow Error because windows are quickly deleted
		// We set a placeholder errorhandler for some time and restore it later
		private bool suppressErrors = false;

		private List<long> windows;

		internal nint Handle { get; private set; }

		internal bool SuppressErrors
		{
			set
			{
				if (value && !suppressErrors)
					oldHandler = Xlib.XSetErrorHandler(delegate { success = false; return 0; });
				else if (!value && suppressErrors)
					_ = Xlib.XSetErrorHandler(oldHandler);

				suppressErrors = value;
			}

			get => suppressErrors;
		}

		private XConnectionSingleton()
		{
			windows = new List<long>();
			// Kick off a thread listening to X events
			listener = new Thread(Listen);
			listener.Start();
		}

		public void Dispose()
		{
			//Listener.Abort();
			run = false;
			Xlib.XCloseDisplay(Handle);
		}

		internal static XConnectionSingleton GetInstance()
		{
			if (instance == null)
				instance = new XConnectionSingleton();

			return instance;
		}

		private void FishEvent()
		{
			var Event = new XEvent();
			Xlib.XNextEvent(Handle, ref Event);
			OnEvent?.Invoke(Event);

			if (Event.type == XEventName.CreateNotify)
			{
				var Window = Event.CreateWindowEvent.window;
				success = true;
				windows.Add(Window);
				SuppressErrors = true;

				if (success)
					RecurseTree(Handle, Window);

				SuppressErrors = false;
			}
			else if (Event.type == XEventName.DestroyNotify)
			{
				_ = windows.Remove(Event.DestroyWindowEvent.window);
			}
		}

		private void Listen()
		{
			Handle = Xlib.XOpenDisplay(0);
			// Select all the windows already present
			SuppressErrors = true;
			RecurseTree(Handle, Xlib.XDefaultRootWindow(Handle));
			SuppressErrors = false;

			while (run)
			{
				FishEvent();
				Keysharp.Core.Flow.Sleep(10); // Be polite
			}
		}

		/// <summary>
		/// In the X Window system, windows can have sub windows. This function crawls a
		/// specific function, and then recurses on all child windows. It is called to
		/// select all initial windows. It make some time (~0.5s)
		/// </summary>
		/// <param name="display"></param>
		/// <param name="rootWindow"></param>
		private unsafe void RecurseTree(nint display, long rootWindow)
		{
			if (!windows.Contains(rootWindow))
				windows.Add(rootWindow);

			// Request all children of the given window, along with the parent
			if (Xlib.XQueryTree(display, rootWindow, out var rootWindowRet, out var parentWindow, out var childrenPtr, out var nChildren) != 0)
			{
				if (nChildren != 0)
				{
					var pSource = (long*)childrenPtr.ToPointer();
					_ = Xlib.XSelectInput(display, rootWindow, selectMask);

					// Subwindows shouldn't be forgotten, especially since everything is a subwindow of RootWindow
					for (var i = 0; i < nChildren; i++)
					{
						var child = pSource[i];

						if (child != 0)
						{
							_ = Xlib.XSelectInput(display, child, selectMask);
							RecurseTree(display, child);
						}
					}
				}
			}
		}

		internal event XEventHandler OnEvent;
	}

	internal delegate void XEventHandler(XEvent Event);
}
#endif