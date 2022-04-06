using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Keysharp.Core.Common.Threading;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//MATT
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
		public static ClipboardAll ClipboardAll(params object[] obj)
		{
			var (data, size) = obj.L().Oi(null, int.MinValue);

			if (data is ClipboardAll a)
			{
				RestoreClipboardAll(a, size != int.MinValue ? size : a.Count);
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

			return new ClipboardAll(System.Array.Empty<byte>());
		}

		public static void ClipWait(params object[] obj)
		{
			//Will need to see if this works in a cross platform way.//MATT
			var (timeout, type) = obj.L().Db(double.MinValue);
			var checktime = timeout != double.MinValue;
			var frequency = 100;
			var time = checktime ? (long)(Math.Abs(timeout) * 1000) : long.MaxValue;

			for (var i = 0L; !checktime || i < time; i += frequency)
			{
				if (!type ? Clipboard.ContainsText() || Clipboard.ContainsFileDropList() : !IsClipboardEmpty())
				{
					Accessors.A_ErrorLevel = 0;
					return;
				}

				System.Threading.Thread.Sleep(frequency);
			}

			Accessors.A_ErrorLevel = 1;
		}

		/// <summary>
		/// Retrieves an environment variable.
		/// </summary>
		/// <param name="name">The name of the environment variable to retrieve.</param>
		/// <returns>The value of the specified environment variable if it exists, else empty string.</returns>
		public static string EnvGet(params object[] obj) => Environment.GetEnvironmentVariable(obj.L()[0] as string) ?? string.Empty;

		/// <summary>
		/// Writes a value to a variable contained in the environment.
		/// </summary>
		/// <param name="name">Name of the environment variable to use, e.g. <c>PATH</c>.</param>
		/// <param name="value">Value to set the environment variable to.</param>
		public static void EnvSet(params object[] obj) => Environment.SetEnvironmentVariable(obj.L()[0] as string, obj[1] as string);

		/// <summary>
		/// Notifies the operating system and all running applications that environment variables have changed.
		/// </summary>
		public static void EnvUpdate()
		{
			Accessors.A_ErrorLevel = 0;

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				//SendMessage() freezes when running in a unit test. PostMessage seems to work.//MATT
				//try { _ = WindowsAPI.SendMessage(new IntPtr(WindowsAPI.HWND_BROADCAST), WindowsAPI.WM_SETTINGCHANGE, IntPtr.Zero, IntPtr.Zero); }
				//try { _ = WindowsAPI.PostMessage(new IntPtr(WindowsAPI.HWND_BROADCAST), WindowsAPI.WM_SETTINGCHANGE, IntPtr.Zero, IntPtr.Zero); }
				try { _ = WindowsAPI.SendMessageTimeout(new IntPtr(WindowsAPI.HWND_BROADCAST), WindowsAPI.WM_SETTINGCHANGE, 0u, IntPtr.Zero, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 1000, out var result); }
				catch (Exception) { Accessors.A_ErrorLevel = 1; }
			}

			//Linux probably has some built in cmd exe which does this.//MATT
		}

		public static string FindCommandLineArg(string arg, bool startswith = true)
		{
			if (startswith)
				return Environment.GetCommandLineArgs().FirstOrDefault(x => (x.StartsWith('-')
#if WINDOWS
						|| x.StartsWith('/')
#endif
																			) && x.Trim(Keysharp.Core.Core.DashSlash).StartsWith(arg, StringComparison.OrdinalIgnoreCase));
			else
				return Environment.GetCommandLineArgs().FirstOrDefault(x => (x.StartsWith('-')
#if WINDOWS
						|| x.StartsWith('/')
#endif
																			) && x.Trim(Keysharp.Core.Core.DashSlash).Contains(arg, StringComparison.OrdinalIgnoreCase));
		}

		public static string FindCommandLineArgVal(string arg, bool startswith = true)
		{
			var args = Environment.GetCommandLineArgs();

			for (var i = 0; i < args.Length; i++)
			{
				if ((args[i].StartsWith('-')
#if WINDOWS
						|| args[i].StartsWith('/')
#endif
					) && args[i].StartsWith(arg, StringComparison.OrdinalIgnoreCase))
				{
					if (i < args.Length - 1)
						return args[i + 1];
				}
			}

			return null;
		}

		public static void HandleCommandLineParams(string[] args)
		{
			var arr = new Array();

			for (var i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith('-')
#if WINDOWS
						|| args[i].StartsWith('/')
#endif
				   )
				{
					arr.Add(args[i]);

					if (args[i].StartsWith("/include", StringComparison.OrdinalIgnoreCase) && i < args.Length - 1)
						arr.Add(args[++i]);
				}
			}

			Keysharp.Core.Accessors.A_Args = arr;
		}

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
			var containsSomething = dataFormats.Any(x => Clipboard.ContainsData(x));
			return !containsSomething;
		}

		public static void OnClipboardChange(params object[] obj)
		{
			try
			{
				var o = obj.L();
				var label = o[0] as string;
				var onoff = 1;

				if (o.Count > 1)
					onoff = Convert.ToInt32(o[1]);

				var method = Reflections.FindLocalMethod(label);

				if (method == null)
					throw new ArgumentNullException();

				//var proc = (Core.HotFunction)Delegate.CreateDelegate(typeof(Core.HotFunction), method);
				var proc = (ClipFunction)Delegate.CreateDelegate(typeof(ClipFunction), method);

				if (onoff == 1)
					Script.ClipFunctions.Add(proc);
				else if (onoff == -1)
					Script.ClipFunctions.Insert(0, proc);
				else if (onoff == 0)
					_ = Script.ClipFunctions.RemoveAll(p => p == proc);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Accessors.A_ErrorLevel = 1;
				throw new ArgumentException();
			}
		}

		/// <summary>
		/// Retrieves screen resolution, multi-monitor info, dimensions of system objects, and other system properties.
		/// </summary>
		/// <param name="output">The variable to store the result.</param>
		/// <param name="command"></param>
		/// <param name="param"></param>
		public static object SysGet(params object[] obj)
		{
			var o = obj.L()[0];

			if (o is Keysharp.Core.Windows.SystemMetric en)
				return Keysharp.Core.Windows.WindowsAPI.GetSystemMetrics(en);

			if (Script.IsNumeric(o))
			{
				var i = Convert.ToInt32(o);
				return Keysharp.Core.Windows.WindowsAPI.GetSystemMetrics((SystemMetric)i);
			}

			return "";
		}

		internal static int ClipFormatStringToInt(string fmt) => DataFormats.GetFormat(fmt) is DataFormats.Format d ? d.Id : 0;

		/// <summary>
		/// Gets the data on the clipboard in the format specified by the selected item of the specified listbox.
		/// Gotten from: http://pinvoke.net/default.aspx/user32/GetClipboardData.html
		/// </summary>
		internal static byte[] GetClipboardData(int format, ref bool nulldata)
		{
			if (format != 0)
			{
				if (WindowsAPI.OpenClipboard(Accessors.ClipboardTimeout))
				{
					byte[] buf;

					try
					{
						var clipdata = WindowsAPI.GetClipboardData(format, ref nulldata);//Get pointer to clipboard data in the selected format
						var length = (int)WindowsAPI.GlobalSize(clipdata);
						var gLock = WindowsAPI.GlobalLock(clipdata);
						buf = new byte[length];

						if (length != 0)
							Marshal.Copy(gLock, buf, 0, length);
					}
					finally
					{
						_ = WindowsAPI.CloseClipboard();
					}

					return buf;
				}
			}

			return null;
		}

		internal static DataFormats.Format IntToClipFormat(int i) => DataFormats.GetFormat(i);

		internal static void RestoreClipboardAll(ClipboardAll arr, int length)
		{
			var bytes = (byte[])arr.array.ToArray(typeof(byte));

			if (WindowsAPI.OpenClipboard(Accessors.ClipboardTimeout))//Need to leave it open for it to work when using the Windows API.
			{
				length = Math.Min(Math.Max(0, length), bytes.Length);

				for (var index = 0; index < length;)
				{
					var cliptype = BitConverter.ToUInt32(bytes, index);

					if (cliptype == 0)
						break;

					index += 4;
					var size = BitConverter.ToInt32(bytes, index);
					index += 4;

					if (index + size < length)
					{
						var hglobal = Marshal.AllocHGlobal(size);
						Marshal.Copy(bytes, index, hglobal, size);
						_ = WindowsAPI.SetClipboardData(cliptype, hglobal);
						Marshal.FreeHGlobal(hglobal);
						index += size;
					}
				}

				_ = WindowsAPI.CloseClipboard();
			}
		}

		private static void MainWindow_ClipboardUpdate(object sender, EventArgs e) => throw new NotImplementedException();
	}

	public class ClipboardAll : Array
	{
		public ClipboardAll(byte[] obj)
			: base(obj)
		{
		}
	}
}