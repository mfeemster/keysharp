using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
#if WINDOWS
	using Keysharp.Core.Windows;
#endif
using Keysharp.Scripting;

namespace Keysharp.Core
{
	public static class Env
	{
		/// <summary>
		/// Gotten from AHK.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static ClipboardAll ClipboardAll(object obj0 = null, object obj1 = null)
		{
#if WINDOWS
			var data = obj0;
			var size = obj1.Al(long.MinValue);

			if (data is ClipboardAll a)
			{
				RestoreClipboardAll(a, size != long.MinValue ? size : (long)a.Size);
				return a;
			}

			using (var ms = new MemoryStream())
			{
				var dibToOmit = 0;
				var bw = new BinaryWriter(ms);
				var dataObject = Clipboard.GetDataObject();

				foreach (var format in dataObject.GetFormats())
				{
					var fi = ClipFormatStringToInt(format);

					switch (fi)
					{
						case WindowsAPI.CF_BITMAP:
						case WindowsAPI.CF_ENHMETAFILE:
						case WindowsAPI.CF_DSPENHMETAFILE:
							continue;//These formats appear to be specific handle types, not always safe to call GlobalSize() for.
					}

					if (fi == WindowsAPI.CF_TEXT || fi == WindowsAPI.CF_OEMTEXT || fi == dibToOmit)
						continue;

					if (dibToOmit == 0)
					{
						if (fi == WindowsAPI.CF_DIB)
							dibToOmit = WindowsAPI.CF_DIBV5;
						else if (fi == WindowsAPI.CF_DIBV5)
							dibToOmit = WindowsAPI.CF_DIB;
					}
				}

				foreach (var format in dataObject.GetFormats())
				{
					var fi = ClipFormatStringToInt(format);
					var nulldata = false;

					switch (fi)
					{
						case WindowsAPI.CF_BITMAP:
						case WindowsAPI.CF_ENHMETAFILE:
						case WindowsAPI.CF_DSPENHMETAFILE:
							// These formats appear to be specific handle types, not always safe to call GlobalSize() for.
							continue;
					}

					if (fi == WindowsAPI.CF_TEXT || fi == WindowsAPI.CF_OEMTEXT || fi == dibToOmit)
						continue;

					var buf = GetClipboardData(fi, ref nulldata);

					if (buf != null)
					{
					}
					else if (nulldata)
						buf = System.Array.Empty<byte>();//This format usually has null data.
					else
						continue;//GetClipboardData() failed: skip this format.

					bw.Write(fi);
					bw.Write(buf.Length);
					bw.Write(buf);
				}

				if (ms.Position > 0)
				{
					bw.Write(0);
					return new ClipboardAll(ms.ToArray());
				}
			}

#elif LINUX
			throw new NotImplementedException();
#endif
			return new ClipboardAll(System.Array.Empty<byte>());
		}

		public static bool ClipWait(object obj0 = null, object obj1 = null)
		{
			//Will need to see if this works in a cross platform way.//TODO
			var timeout = obj0.Ad(double.MinValue);
			var type = obj1.Ab();
			var checktime = timeout != double.MinValue;
			var frequency = 100;
			var time = checktime ? (long)(Math.Abs(timeout) * 1000) : long.MaxValue;

			for (var i = 0L; !checktime || i < time; i += frequency)
			{
				if (!type ? Clipboard.ContainsText() || Clipboard.ContainsFileDropList() : !IsClipboardEmpty())
					return true;

				Keysharp.Core.Flow.Sleep(frequency);
			}

			return false;
		}

		/// <summary>
		/// Retrieves an environment variable.
		/// </summary>
		/// <param name="name">The name of the environment variable to retrieve.</param>
		/// <returns>The value of the specified environment variable if it exists, else empty string.</returns>
		public static string EnvGet(object obj) => Environment.GetEnvironmentVariable(obj.As()) ?? string.Empty;

		/// <summary>
		/// Writes a value to a variable contained in the environment.
		/// </summary>
		/// <param name="name">Name of the environment variable to use, e.g. <c>PATH</c>.</param>
		/// <param name="value">Value to set the environment variable to.</param>
		public static void EnvSet(object obj0, object obj1 = null) => Environment.SetEnvironmentVariable(obj0.As(), obj1 as string);

		/// <summary>
		/// Notifies the operating system and all running applications that environment variables have changed.
		/// </summary>
		public static void EnvUpdate()
		{
#if WINDOWS

			//SendMessage() freezes when running in a unit test. PostMessage seems to work. Use SendMessageTimeout().
			try { _ = WindowsAPI.SendMessageTimeout(new IntPtr(WindowsAPI.HWND_BROADCAST), WindowsAPI.WM_SETTINGCHANGE, 0u, IntPtr.Zero, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 1000, out var result); }
			catch (Exception ex) { throw new OSError(ex); }

#elif LINUX
			throw new NotImplementedException();
#endif
		}

		public static string FindCommandLineArg(string arg, bool startswith = true)
		{
			if (startswith)
				return Environment.GetCommandLineArgs().FirstOrDefault(x => (x.StartsWith('-')
						|| x.StartsWith('/')) && x.Trim(Keywords.DashSlash).StartsWith(arg, StringComparison.OrdinalIgnoreCase));
			else
				return Environment.GetCommandLineArgs().FirstOrDefault(x => (x.StartsWith('-')
						|| x.StartsWith('/')) && x.Trim(Keywords.DashSlash).Contains(arg, StringComparison.OrdinalIgnoreCase));
		}

		public static string FindCommandLineArgVal(string arg, bool startswith = true)
		{
			var args = Environment.GetCommandLineArgs();

			for (var i = 0; i < args.Length; i++)
			{
				if ((args[i].StartsWith('-') || args[i].StartsWith('/')) && args[i].StartsWith(arg, StringComparison.OrdinalIgnoreCase))
					if (i < args.Length - 1)
						return args[i + 1];
			}

			return null;
		}

		public static void HandleCommandLineParams(string[] args) => Keysharp.Core.Accessors.A_Args.AddRange(args);

		/// <summary>
		/// The clipboard object doesn't provide a way to determine if it's truly empty or not.
		/// This method taken from: https://www.codeproject.com/questions/1089557/check-with-csharp-if-the-clipboard-is-really-empty
		/// attempts to provide such functionality.
		/// </summary>
		/// <returns>True if empty, else false.</returns>
		public static bool IsClipboardEmpty()
		{
			var dataFormats = typeof(DataFormats).GetFields(BindingFlags.Public | BindingFlags.Static)
							  .Select(f => f.Name);
			var containsSomething = dataFormats.Any(Clipboard.ContainsData);
			return !containsSomething;
		}

		public static void OnClipboardChange(object obj0, object obj1 = null)
		{
			if (obj0 is IFuncObj fo)
				Script.ClipFunctions.ModifyEventHandlers(fo, obj1.Al(1));
			else
				throw new TypeError("Object passed to OnClipboardChange() was not a function object.");
		}

		/// <summary>
		/// Retrieves screen resolution, multi-monitor info, dimensions of system objects, and other system properties.
		/// </summary>
		/// <param name="output">The variable to store the result.</param>
		/// <param name="command"></param>
		/// <param name="param"></param>
		public static object SysGet(object obj)
		{
#if WINDOWS

			if (obj is Keysharp.Core.Windows.SystemMetric en)
				return Keysharp.Core.Windows.WindowsAPI.GetSystemMetrics(en);

			return Keysharp.Core.Windows.WindowsAPI.GetSystemMetrics((SystemMetric)obj.Ai());
#elif LINUX
			throw new NotImplementedException();
#endif
		}

		internal static int ClipFormatStringToInt(string fmt) => DataFormats.GetFormat(fmt) is DataFormats.Format d ? d.Id : 0;

		/// <summary>
		/// Gets the data on the clipboard in the specified format.
		/// Gotten from: http://pinvoke.net/default.aspx/user32/GetClipboardData.html
		/// </summary>
		/// <param name="format"></param>
		/// <param name="nulldata"></param>
		/// <returns></returns>
		internal static byte[] GetClipboardData(int format, ref bool nulldata)
		{
			if (format != 0)
			{
#if WINDOWS

				if (WindowsAPI.OpenClipboard((long)Accessors.A_ClipboardTimeout))
				{
					byte[] buf;
					var gLock = IntPtr.Zero;

					try
					{
						var clipdata = WindowsAPI.GetClipboardData(format, ref nulldata);//Get pointer to clipboard data in the selected format.
						var length = (int)WindowsAPI.GlobalSize(clipdata);
						gLock = WindowsAPI.GlobalLock(clipdata);
						buf = new byte[length];

						if (length != 0)
							Marshal.Copy(gLock, buf, 0, length);
					}
					finally
					{
						_ = WindowsAPI.GlobalUnlock(gLock);
						_ = WindowsAPI.CloseClipboard();
					}

					return buf;
				}

#elif LINUX
				throw new NotImplementedException();
#endif
			}

			return null;
		}

		//internal static DataFormats.Format IntToClipFormat(int i) => DataFormats.GetFormat(i);

		internal static void RestoreClipboardAll(ClipboardAll clip, long length)
		{
			unsafe
			{
#if WINDOWS

				if (WindowsAPI.OpenClipboard((long)Accessors.A_ClipboardTimeout))//Need to leave it open for it to work when using the Windows API.
				{
					var ptr = clip.Ptr;
					length = Math.Min(Math.Max(0U, length), (long)clip.Size);

					for (var index = 0; index < length;)
					{
						var cliptype = (uint)Marshal.ReadInt32(ptr, index);

						if (cliptype == 0)
							break;

						index += 4;
						var size = Marshal.ReadInt32(ptr, index);
						index += 4;

						if (index + size < length)
						{
							var hglobal = Marshal.AllocHGlobal(size);
							System.Buffer.MemoryCopy((ptr + index).ToPointer(), hglobal.ToPointer(), size, size);
							_ = WindowsAPI.SetClipboardData(cliptype, hglobal);
							//Do not free hglobal here.
							index += size;
						}
					}

					_ = WindowsAPI.CloseClipboard();
				}

#elif LINUX
				throw new NotImplementedException();
#endif
			}
		}
	}

	public class ClipboardAll : Buffer
	{
		public ClipboardAll(byte[] obj)
			: base(obj)
		{
		}
	}
}