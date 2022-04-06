using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Keysharp.Core
{
	public class Misc
	{
		public static Process FindProcess(string name)
		{
			if (int.TryParse(name, out var id))
				return System.Diagnostics.Process.GetProcessById(id);

			const string exe = ".exe";

			if (name.EndsWith(exe, StringComparison.OrdinalIgnoreCase))
				name = name.Substring(0, name.Length - exe.Length);

			var prc = System.Diagnostics.Process.GetProcessesByName(name);
			return prc.Length > 0 ? prc[0] : null;
		}

		public static string NormalizeEol(string text, string eol = null)
		{
			const string CR = "\r", LF = "\n", CRLF = "\r\n";
			eol = eol ?? Environment.NewLine;

			switch (eol)
			{
				case CR:
					return text.Replace(CRLF, CR).Replace(LF, CR);

				case LF:
					return text.Replace(CRLF, LF).Replace(CR, LF);

				case CRLF:
					return text.Replace(CR, string.Empty).Replace(LF, CRLF);
			}

			return text;
		}

		public static IEnumerator Obj__Enum(IEnumerable obj) => obj.__Enum();

		public static IEnumerator Obj_NewEnum(IEnumerable obj) => obj.__Enum();

		public static long ObjCount(Keysharp.Core.Array obj) => (long)obj.Length;

		public static object ObjDelete(Keysharp.Core.Array obj, params object[] values) => obj.Delete(values);

		public static object ObjGetCapacity(Keysharp.Core.Array obj) => obj.Capacity;

		public static bool ObjHas(Keysharp.Core.Array obj, params object[] values) => obj.Has(values);

		public static void ObjInsertAt(Keysharp.Core.Array obj, params object[] values) => obj.InsertAt(values);

		public static long ObjLength(Keysharp.Core.Array obj) => (long)obj.Length;

		public static object ObjMaxIndex(Keysharp.Core.Array obj) => obj.MaxIndex();

		public static object ObjMinIndex(Keysharp.Core.Array obj) => obj.MinIndex();

		public static void ObjPush(Keysharp.Core.Array obj, params object[] values) => obj.Push(values);

		public static object ObjRawGet<K, V>(Dictionary<K, V> dictionary, K key)
		{
			if (dictionary.TryGetValue(key, out var val))
				return val;

			return string.Empty;
		}

		//This Obj*() wrappers are for V1 only.

		public static void ObjRawSet<K, V>(Dictionary<K, V> dictionary, K key, V val) => dictionary[key] = val;

		public static object ObjRemoveAt(Keysharp.Core.Array obj, params object[] values) => obj.RemoveAt(values);

		public static object ObjSetCapacity(Keysharp.Core.Array obj, params object[] values) => obj.Capacity = values.L()[0];

		public static object Pop(Keysharp.Core.Array obj) => obj.Pop();

	}
}