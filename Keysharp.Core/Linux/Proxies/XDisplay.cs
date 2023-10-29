using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Keysharp.Core.Linux.X11;

namespace Keysharp.Core.Linux.Proxies
{
	/// <summary>
	/// Proxy around a X11 xDisplay
	/// </summary>
	internal class XDisplay : IDisposable
	{
		private static IntPtr _defaultDisp = IntPtr.Zero;

		internal static XDisplay Default
		{
			get
			{
				if (_defaultDisp == IntPtr.Zero)
					_defaultDisp = Xlib.XOpenDisplay(IntPtr.Zero);

				return new XDisplay(_defaultDisp);
			}
		}

		internal IntPtr Handle { get; } = IntPtr.Zero;

		internal XWindow Root => new XWindow(this, Xlib.XDefaultRootWindow(Handle));

		internal XDisplay(IntPtr prt) => Handle = prt;

		public void Dispose()
		{
			if (Handle != IntPtr.Zero && Handle != _defaultDisp)
				Xlib.XCloseDisplay(Handle);
		}

		/// <summary>
		/// Returns the window which currently has input focus
		/// </summary>
		/// <returns></returns>
		internal XWindow XGetInputFocus()
		{
			_ = Xlib.XGetInputFocus(Handle, out var hwndWnd, out var focusState);
			return new XWindow(this, hwndWnd);
		}

		/// <summary>
		/// Returns all Windows of this XDisplay
		/// </summary>
		/// <returns></returns>
		internal IEnumerable<XWindow> XQueryTree() => XQueryTree(Root);

		/// <summary>
		/// Return all child xWindows from given xWindow
		/// </summary>
		/// <param name="windowToObtain"></param>
		/// <returns></returns>
		internal IEnumerable<XWindow> XQueryTree(XWindow windowToObtain)
		{
			_ = Xlib.XQueryTree(Handle, windowToObtain.ID, out var root_return, out var parent_return, out var children_return, out var nchildren_return);
			var childs = new int[nchildren_return];
			Marshal.Copy(children_return, childs, 0, nchildren_return);
			var wins = new List<XWindow>();

			foreach (var id in childs)
			{
				wins.Add(new XWindow(this, id));
			}

			return wins;
		}
	}
}