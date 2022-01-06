using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keysharp.Core
{
	public partial class Core
	{
		public static NotifyIcon Tray;

		public static void FileEncoding(params object[] obj)
		{
			var s = obj.L().S1();

			if (s != "")
				Accessors.A_FileEncoding = s;
		}

		public static Encoding GetEncoding(object s)
		{
			var val = s.ToString().ToLowerInvariant();
			Encoding enc;

			if (val.StartsWith("cp"))
				return Encoding.GetEncoding(val.Substring(2).ParseInt().Value);

			if (int.TryParse(val, out var cp))
				return Encoding.GetEncoding(cp);

			switch (val)
			{
				case "ascii":
				case "us-ascii":
					return Encoding.ASCII;

				case "utf-8":
					return Encoding.UTF8;

				case "utf-8-raw":
					return new UTF8Encoding(false);//No byte order mark.

				case "utf-16":
				case "unicode":
					return Encoding.Unicode;

				case "utf-16-raw":
					return new UnicodeEncoding(false, false);//Little endian, no byte order mark.
			}

			try
			{
				enc = Encoding.GetEncoding(val);
				return enc;
			}
			catch
			{
			}

			return Encoding.Unicode;
		}

		/// <summary>
		/// A function.
		/// </summary>
		/// <param name="args">Parameters.</param>
		/// <returns>A value.</returns>
		public delegate object GenericFunction(params object[] args);
		//Original used GenericFunction, but it appears hotkey/string functions don't ever use their return values, and having return statements makes them hard to generate.
		//Revisit this later if needed.//MATT
		public delegate void HotFunction(object[] o);//As written IronAHK does not pass args, but once we redesign, we probably will.//MATT
		public delegate object ClipFunction(params object[] o);
		public delegate void ClipUpdateDel(params object[] o);
		public delegate void SimpleDelegate();

		public static Task<int> LaunchInThread(object func, object[] o)//Determine later the optimal threading model.//TODO
		{
			Keysharp.Scripting.Script.totalExistingThreads++;

			if (func is GenericFunction gf)
			{
				var tsk = Task.Factory.StartNew(() => gf(o));
				return tsk.ContinueWith((_) => Keysharp.Scripting.Script.totalExistingThreads--);
				//return tsk;
			}
			else if (func is HotFunction hf)
			{
				var tsk = Task.Factory.StartNew(() => hf(o));
				return tsk.ContinueWith((_) => Keysharp.Scripting.Script.totalExistingThreads--);
				//return tsk;
			}
			else if (func is IFuncObj ifo)
			{
				var tsk = Task.Factory.StartNew(() => ifo.Call(o));
				return tsk.ContinueWith((_) => Keysharp.Scripting.Script.totalExistingThreads--);
			}

			return Task.FromResult<int>(1);
		}

		//internal stat
		//public static Task LaunchInThread(HotFunction func, object[] o)//Determine later the optimal threading model.//TODO
		//{
		//  return Task.Factory.StartNew(() =>
		//  {
		//      func(o);
		//  });
		//}
	}
}