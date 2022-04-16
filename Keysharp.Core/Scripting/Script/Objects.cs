using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Keysharp.Core;
using Keysharp.Core.Common.Threading;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		internal static List<GenericFunction> errorHandlers;

		public static Core.Array Array(params object[] obj)
		{
			if (obj.Length == 0)
				return new Core.Array(64);

			var arr = new Core.Array(obj.Length);
			arr.Push(obj);
			return arr;
		}

		public static Keysharp.Core.Buffer Buffer(params object[] obj) => new (obj);

		public static Map Dictionary(object[] keys, object[] values)//MATT
		{
			var table = new Map();
			//var table = new Dictionary<object, object>();
			//values = (object[])values[0];//This caused a crash //MATT

			for (var i = 0; i < keys.Length; i++)
			{
				var name = keys[i];
				var entry = i < values.Length ? values[i] : null;

				if (entry == null)
				{
					if (table.Has(name))
						_ = table.Delete(name);
				}
				else
					table[name] = entry;
			}

			return table;
		}

		public static Error Error(params object[] obj) => new (obj);

		public static bool ErrorOccurred(Error err)
		{
			object result = null;

			if (errorHandlers != null)
			{
				foreach (var handler in errorHandlers)
				{
					result = handler.Invoke(err, err.ExcType);

					if (err.ExcType == Keyword_Return)
					{
						if (result.ParseLong() != -1)
							Flow.Exit(0);
					}
					else if (err.ExcType == Keyword_Exit)
						Flow.Exit(0);
					else if (err.ExcType == Keyword_ExitApp)
						Flow.ExitApp(0);

					if (result.IsCallbackResultNonEmpty() && result.ParseLong(false) == 1)
						return false;
				}
			}

			return true;
		}

		public static object ExtendArray(ref object item, object value)//Unused, but if it does get used, needs to be an Array.//MATT
		{
			if (item == null || !item.GetType().IsArray)
				return null;

			var array = (object[])item;
			var i = array.Length;
			System.Array.Resize(ref array, i + 1);
			array[i] = value;
			item = array;
			return value;
		}

		public static FuncObj FuncObj(params object[] obj) => obj.L().S1O1().Splat((s, o) => new FuncObj(s, o));

		public static object GetMethodOrProperty(object item, object key)//This has to be public because the script will emit it in Main().
		{
			if (item == null || key == null)
				return null;

			if (key is string ks)
			{
				//mi = item.GetType().GetMethods().FirstOrDefault((_mi) => _mi.Name == ks);//Needed because the method might be overloaded.
				if (Reflections.FindAndCacheMethod(item.GetType(), ks) is MethodInfo mi)
					return (item, mi);

				if (Reflections.FindAndCacheProperty(item.GetType(), ks) is PropertyInfo pi)
					return (item, pi);

				//if (GetPropertyValue(item, ks).Item1 is object o)//This is weird in that it gets either a method or a property value. Shouldn't it return the property itself?//TODO
				//return o;
				//Not using extension methods anymore
				//mi = Reflections.FindExtensionMethod(item.GetType(), ks);//Put this last because it's rare.
				//if (mi != null)
				//  return (item, mi);
			}

			return null;
		}

		public static object GetPropertyValue(object item, object name)
		{
			var ret = InternalGetPropertyValue(item, name.ToString());
			return ret.Item2 ? ret.Item1 : "";
		}

		internal static (object, bool) InternalGetPropertyValue(object item, string name)
		{
			var type = item.GetType();
			//PropertyInfo match = null;
			//foreach (var property in type.GetProperties())
			//{
			//  if (property.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
			//  {
			//      match = property;
			//      break;
			//  }
			//}
			//if (!match.CanRead)
			//  return null;
			var match = Reflections.FindAndCacheProperty(type, name);//Cleaned this up.//MATT

			if (match != null)
			{
				try
				{
					var ret = (match.GetValue(item, null), true);

					if (ret.Item1 is int i)
						ret.Item1 = (long)i;//Try to keep everything as long.

					return ret;
				}
				catch (Exception)
				{
				}
			}

			return (null, false);
		}

		public static Gui Gui(params object[] obj) => new (obj);

		public static long HasBase(params object[] obj)
		{
			var (o1, o2) = obj.O2();//Do not flatten, always use the argument directly.
			return o2.GetType().IsAssignableFrom(o1.GetType()) ? 1L : 0L;
		}

		public static object Index(object item, object key)//Completely reworked to handle non string keys, and for optimization.//MATT
		{
			if (item == null)// || key == null)//null is a valid key value.//MATT
				return null;

			if (item is Map table)//This design is terrible and will not work when a key has the same name as a property. Needs to be totally reworked.//MATT
			{
				return table[key];
				//Case sensitivity is handled internally, and this function no longer handles getting property values.
				//if (table.map.TryGetValue(key, out var val))
				//  return val;
				//if (key is string lookup)//Comparison above was case sensitive. It failed, so now try the slower method of comparing each key, without case.
				//{
				//  foreach (var k in table.map.Keys)//Direct use of map member. Normally don't want to do this, but ok since this is internal.
				//      if (k is string check)
				//          if (lookup.Equals(check, System.StringComparison.OrdinalIgnoreCase))
				//              return table[check];
				//}
				//if (key is string ks)//Wasn't a key, perhaps it was a property.
				//  return InternalGetPropertyValue(item, ks).Item1;
			}
			else//This appears to work, but will prevent any properties or extension methods from working on dictionary types.//MATT
			{
				//if (key is string ks)
				//{
				//  var ret = InternalGetPropertyValue(item, ks);
				//  if (ret.Item2)
				//      return ret.Item1;
				//}
				//if (IsNumeric(key))
				//{
				//return IndexAt(item, (int)ForceLong(key));
				return IndexAt(item, key);
				//}
				//else
				//{
				//  if (key is string ks)
				//  {
				//      var mi = Reflections.GetExtensionMethod(item.GetType(), ks);
				//
				//      if (mi != null)
				//          return (item, mi);
				//  }
				//
				//  return GetPropertyValue(item, (string)key);
				//}
			}

			//return null;
		}

		public static IndexError IndexError(params object[] obj) => new (obj);

		/// <summary>
		/// Differs in that locale is always considered.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static long IsAlnum(params object[] obj)
		{
			var s = obj.S1();
			return s?.Length == 0 || s.All(ch => char.IsLetter(ch) || char.IsNumber(ch)) ? 1 : 0;
		}

		/// <summary>
		/// Differs in that locale is always considered.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static long IsAlpha(params object[] obj)
		{
			var s = obj.S1();
			return s?.Length == 0 || s.All(char.IsLetter) ? 1 : 0;
		}

		public static long IsDate(params object[] obj) => IsTime(obj);

		public static long IsDigit(params object[] obj)
		{
			var s = obj.S1();
			return s?.Length == 0 || s.All(char.IsDigit) ? 1 : 0;
		}

		public static long IsFloat(params object[] obj)
		{
			var l = obj;//.L();//Don't flatten, treat as-is.

			if (l.Length < 1)
				return 0;

			var o = l[0];

			if (o is double || o is float || o is decimal)
				return 1;

			var val = o.ParseDouble(false, true);
			return val.HasValue ? 1 : 0;
		}

		/// <summary>
		/// Checks if a function is defined.
		/// </summary>
		/// <param name="name">The name of a function.</param>
		/// <returns><c>true</c> if the specified function exists in the current scope, <c>false</c> otherwise.</returns>
		public static long IsFunc(object name) => Reflections.FindMethod(name.ToString()) is MethodInfo ? 1L : 0L;

		public static long IsInteger(params object[] obj)
		{
			var l = obj;//.L();//Don't flatten, treat as-is.

			if (l.Length < 1)
				return 0;

			var o = l[0];

			if (o is long || o is int || o is uint || o is ulong)
				return 1;

			if (o is double || o is float || o is decimal)
				return 0;

			var val = o.ParseLong(false);
			return val.HasValue ? 1 : 0;
		}

		/// <summary>
		/// Checks if a label is defined.
		/// </summary>
		/// <param name="name">The name of a label.</param>
		/// <returns><c>true</c> if the specified label exists in the current scope, <c>false</c> otherwise.</returns>
		public static long IsLabel(object name)
		{
			//var method = Reflections.LabelMethodName(name);
			//return Reflections.FindLocalMethod(method) != null ? 1 : 0;
			throw new Error("C# does not allow querying labels at runtime.");
		}

		/// <summary>
		/// Differs in that locale is always considered.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static long IsLower(params object[] obj)
		{
			var s = obj.L().S1();
			return s?.Length == 0 || s.All(ch => char.IsLetter(ch) && char.IsLower(ch)) ? 1 : 0;
		}

		public static long IsNumber(params object[] obj) => IsInteger(obj) | IsFloat(obj);

		public static long IsObject(params object[] obj)
		{
			var o = obj;//.L();
			return o.Length > 0 && o[0] is KeysharpObject ? 1 : 0;
		}

		public static long IsSet(params object[] obj)
		{
			if (obj.Length > 0)
			{
				var o = obj[0];

				if (o != UnsetArg.Default && o != null)
					return 1;
			}

			return 0;
		}

		public static long IsSpace(params object[] obj)
		{
			var s = obj.S1();

			foreach (var ch in s)
				if (!Keyword_Spaces.Any(ch2 => ch2 == ch))
					return 0;

			return 1;
		}

		public static long IsTime(params object[] obj)
		{
			var s = obj.S1();
			DateTime dt;

			try
			{
				dt = Conversions.ToDateTime(s);

				if (dt == DateTime.MinValue)
					return 0;
			}
			catch
			{
				return 0;
			}

			int[] t = { DateTime.Now.Year / 100, DateTime.Now.Year % 100, 1, 1, 0, 0, 0, 0 };
			var tempdt = new DateTime(t[1], t[2], t[3], t[4], t[5], t[6], System.Globalization.CultureInfo.CurrentCulture.Calendar);//Will be this if parsing totally failed.
			return dt != tempdt ? 1 : 0;
		}

		/// <summary>
		/// Differs in that locale is always considered.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static long IsUpper(params object[] obj)
		{
			var s = obj.S1();
			return s?.Length == 0 || s.All(ch => char.IsLetter(ch) && char.IsUpper(ch)) ? 1 : 0;
		}

		public static long IsXDigit(params object[] obj)
		{
			var s = obj.S1();
			var sp = s.AsSpan();

			if (sp.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase))
				sp = sp.Slice(2);

			foreach (var ch in sp)
				if (!((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f')))
					return 0;

			return 1;
		}

		public static KeyError KeyError(params object[] obj) => new (obj);

		public static Map Map(params object[] obj) => Object(obj);

		public static MemberError MemberError(params object[] obj) => new (obj);

		public static MemoryError MemoryError(params object[] obj) => new (obj);

		public static Menu Menu() => new ();

		public static MenuBar MenuBar() => new ();

		public static object MenuFromHandle(params object[] obj)
		{
			var handle = new IntPtr(obj[0].ParseLong().Value);
			var menu = System.Windows.Forms.MenuStrip.FromHandle(handle);

			if (menu != null)
				return menu;

			menu = System.Windows.Forms.ContextMenuStrip.FromHandle(handle);

			if (menu != null)
				return menu;

			return "";
		}

		public static MethodError MethodError(params object[] obj) => new (obj);

		public static Map Object(params object[] obj)
		{
			if (obj.Length == 0)
				return new Map();

			var dkt = new Map();
			dkt.Set(obj);
			return dkt;
		}

		public static void OnError(params object[] obj)
		{
			var (e, i) = obj.L().Oi("", 1);
			var del = GuiControl.GetDel(e, null);

			if (errorHandlers == null)
				errorHandlers = new List<GenericFunction>();

			errorHandlers.ModifyEventHandlers(del, i);
		}

		public static OSError OSError(params object[] obj) => new (obj);

		public static PropertyError PropertyError(params object[] obj) => new (obj);

		public static object SetObject(object key, object item, object[] parents, object value)
		{
			if (parents.Length > 0)
			{
				for (var i = parents.Length - 1; i > -1; i--)//No idea what this is actually doing, it's probably left over legacy code.//TODO
				{
					var pi = parents[i];
					var child = Index(item, pi);

					if (child == null)
					{
						if (item is Map map1)
							map1[pi] = item = new Map();
						else
							return null;
					}
					else
						item = child;
				}
			}
			else if (item is Map map2)//This function has been redesigned to handle assigning a map key/value pair, or assigning a value to an array index. It is NOT for setting properties.
			{
				map2[key] = value;
			}
			else if (item is Core.Array al)
			{
				var index = (int)ForceLong(key);
				al[index] = value;
			}
			else if (item == null)
				return null;

			return value;
		}

		public static void SetPropertyValue(object item, object name, object value)
		{
			if (Reflections.FindAndCacheProperty(item.GetType(), name.ToString()) is PropertyInfo pi)
			{
				try
				{
					pi.SetValue(item, value);
				}
				catch (Exception e)
				{
					if (e.InnerException is KeysharpException ke)
						throw ke;
					else
						throw;
				}
			}
		}

		public static Keysharp.Core.StringBuffer StringBuffer(params object[] obj)
		{
			var (str, cap) = obj.L().Si("", 256);
			return new StringBuffer(str, cap);
		}

		public static TargetError TargetError(params object[] obj) => new (obj);

		public static TimeoutError TimeoutError(params object[] obj) => new (obj);

		public static string Type(object t) => t.GetType().Name;

		public static TypeError TypeError(params object[] obj) => new (obj);

		public static ValueError ValueError(params object[] obj) => new (obj);

		public static ZeroDivisionError ZeroDivisionError(params object[] obj) => new (obj);

		public object ObjGetCapacity(params object[] obj)
		{
			return 42;
		}

		public long ObjHasOwnProp(params object[] obj)
		{
			return 1L;
		}

		public long ObjOwnPropCount(params object[] obj)
		{
			return 1L;
		}

		public object ObjOwnProps(params object[] obj)
		{
			return true;
		}

		public void ObjSetBase(params object[] obj)
		{
			throw new Exception(Any.BaseExc);
		}

		public object ObjSetCapacity(params object[] obj)
		{
			return true;
		}

		private static object IndexAt(object item, object index)
		{
			//Subtract one because arrays are 1-indexed.
			//Negative numbers are reverse indexed.
			if (/*position < 0 || */item == null)
				return null;

			var position = ForceInt(index);

			//The most common is going to be a string, array or buffer.
			if (item is string s)
			{
				var actualindex = position < 0 ? s.Length + position : position - 1;
				return s[actualindex];
			}
			else if (item is Core.Array al)
			{
				return al[position];
			}
			else if (item is Core.Buffer buf)
			{
				return buf[position];
			}

			foreach (var mi in item.GetType().GetMethods().Where(m => m.Name == "get_Item"))
			{
				if (index is int i && mi.GetParameters()[0].ParameterType == typeof(int))
					return mi.Invoke(item, new object[] { i - 1 });
				else if (index is long l && mi.GetParameters()[0].ParameterType == typeof(long))
					return mi.Invoke(item, new object[] { l - 1 });
				else if (mi.GetParameters()[0].ParameterType == typeof(object))
					return mi.Invoke(item, new object[] { index });
			}

			if (item is object[] objarr)
			{
				var actualindex = position < 0 ? objarr.Length + position : position - 1;
				return objarr[actualindex];
			}
			else if (item is System.Array array)
			{
				var actualindex = position < 0 ? array.Length + position : position - 1;
				return array.GetValue(actualindex);
			}
			else if (typeof(IEnumerable).IsAssignableFrom(item.GetType()))
			{
				var ienum = (IEnumerable)item;
				var enumerator = ienum.GetEnumerator();
				var i = 0;
				var len = 0;
				var tempenum = ienum.GetEnumerator();

				while (tempenum.MoveNext())
					len++;

				var actualindex = position < 0 ? len + position : position - 1;

				while (enumerator.MoveNext())
				{
					if (i == actualindex)
						return enumerator.Current;

					i++;
				}

				return null;
			}

			return null;
		}
	}
}