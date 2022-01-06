using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Keysharp.Core.Linux.X11;
using Keysharp.Core.Linux.X11.Events;

namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Singleton class to keep track of all active windows and their
	/// events to help the hotkey and window management code
	/// </summary>
	internal class XConnectionSingleton : IDisposable
	{
		// These are the events we subscribe to, in order to allow hotkey/hotstring support
		private static readonly EventMasks SelectMask = EventMasks.KeyPress |
				EventMasks.FocusChange | EventMasks.SubstructureNofity |
				EventMasks.KeyRelease | EventMasks.Exposure;

		private static XConnectionSingleton Instance;
		private Thread Listener;

		// HACK: X sometimes throws a BadWindow Error because windows are quickly deleted
		// We set a placeholder errorhandler for some time and restore it later
		private bool mSurpressErrors = false;

		private XErrorHandler OldHandler;

		private bool run = true;
		private bool Success = true;

		private List<int> Windows;

		internal IntPtr Handle { get; private set; }

		internal bool SurpressErrors
		{
			set
			{
				if (value && !mSurpressErrors)
					OldHandler = Xlib.XSetErrorHandler(delegate { Success = false; return 0; });
				else if (!value && mSurpressErrors)
					_ = Xlib.XSetErrorHandler(OldHandler);

				mSurpressErrors = value;
			}

			get { return mSurpressErrors; }
		}

		private XConnectionSingleton()
		{
			Windows = new List<int>();
			// Kick off a thread listening to X events
			Listener = new Thread(Listen);
			Listener.Start();
		}

		public void Dispose()
		{
			//Listener.Abort();
			run = false;
			Xlib.XCloseDisplay(Handle);
		}

		internal static XConnectionSingleton GetInstance()
		{
			if (Instance == null)
				Instance = new XConnectionSingleton();

			return Instance;
		}

		private void FishEvent()
		{
			var Event = new XEvent();
			Xlib.XNextEvent(Handle, ref Event);
			OnEvent?.Invoke(Event);

			if (Event.type == XEventName.CreateNotify)
			{
				var Window = Event.CreateWindowEvent.window;
				Success = true;
				Windows.Add(Window);
				SurpressErrors = true;

				if (Success)
					RecurseTree(Handle, Window);

				SurpressErrors = false;
			}
			else if (Event.type == XEventName.DestroyNotify)
			{
				_ = Windows.Remove(Event.DestroyWindowEvent.window);
			}
		}

		private void Listen()
		{
			Handle = Xlib.XOpenDisplay(IntPtr.Zero);
			// Select all the windows already present
			SurpressErrors = true;
			RecurseTree(Handle, Xlib.XDefaultRootWindow(Handle));
			SurpressErrors = false;

			while (run)
			{
				FishEvent();
				Thread.Sleep(10); // Be polite
			}
		}

		/// <summary>
		/// In the X Window system, windows can have sub windows. This function crawls a
		/// specific function, and then recurses on all child windows. It is called to
		/// select all initial windows. It make some time (~0.5s)
		/// </summary>
		/// <param name="Display"></param>
		/// <param name="RootWindow"></param>
		private void RecurseTree(IntPtr Display, int RootWindow)
		{
			int[] Children;

			if (!Windows.Contains(RootWindow))
				Windows.Add(RootWindow);

			// Request all children of the given window, along with the parent
			_ = Xlib.XQueryTree(Display, RootWindow, out var RootWindowRet, out var ParentWindow, out var ChildrenPtr, out var NChildren);

			if (NChildren != 0)
			{
				// Fill the array with zeroes to prevent NullReferenceException from glue layer
				Children = new int[NChildren];
				Marshal.Copy(ChildrenPtr, Children, 0, NChildren);
				_ = Xlib.XSelectInput(Display, RootWindow, SelectMask);

				// Subwindows shouldn't be forgotten, especially since everything is a subwindow of RootWindow
				for (var i = 0; i < NChildren; i++)
				{
					if (Children[i] != 0)
					{
						_ = Xlib.XSelectInput(Display, Children[i], SelectMask);
						RecurseTree(Display, Children[i]);
					}
				}
			}
		}

		internal event XEventHandler OnEvent;
	}

	internal delegate void XEventHandler(XEvent Event);
}