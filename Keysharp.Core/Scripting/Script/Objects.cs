using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Keysharp.Core;
#if WINDOWS
	using Keysharp.Core.COM;
#endif

namespace Keysharp.Scripting
{
	public partial class Script
	{
		public static (object, MethodPropertyHolder) GetStaticMethodT<T>(object name, int paramCount) => Reflections.FindAndCacheMethod(typeof(T), name.ToString(), paramCount) is MethodPropertyHolder mph&& mph.mi != null&& mph.IsStaticFunc
		? (null, mph)
		: throw new MethodError($"Attempting to get method {name} failed.");

		public static object Index(object item, params object[] index) => item == null ? null : IndexAt(item, index);

		public static object SetObject(object value, object item, params object[] index)
		{
			object key = null;
			Type typetouse;

			if (item is ITuple otup && otup.Length > 1 && otup[0] is Type t && otup[1] is object o)
			{
				typetouse = t;
				item = o;
			}
			else
				typetouse = item.GetType();

			if (index.Length == 1)
			{
				key = index[0];
				var position = (int)ForceLong(key);

				if (item is object[] objarr)
				{
					var actualindex = position < 0 ? objarr.Length + position : position - 1;
					objarr[actualindex] = value;
					return value;
				}
				else if (item is System.Array array)
				{
					var actualindex = position < 0 ? array.Length + position : position - 1;
					array.SetValue(value, actualindex);
					return value;
				}

#if WINDOWS
				else if (item is ComObjArray coa)
				{
					var actualindex = position < 0 ? coa.array.Length + position : position;
					coa.array.SetValue(value, actualindex);
					return value;
				}

#endif
				else if (item == null)
				{
					return null;
				}
			}
			else if (index.Length == 0)//Access brackets with no index like item.prop[] := 123.
			{
				if (Reflections.FindAndCacheMethod(typetouse, "set_Item", 0) is MethodPropertyHolder mph1)
				{
					_ = mph1.callFunc(item, index.Concat(new object[] { value }));
					return value;
				}
			}

			var il1 = index.Length + 1;

			if (Reflections.FindAndCacheMethod(typetouse, "set_Item", il1) is MethodPropertyHolder mph2)
			{
				if (il1 == mph2.ParamLength || mph2.IsVariadic)
				{
					_ = mph2.callFunc(item, index.Concat(new object[] { value }));
					return value;
				}
				else
				{
					throw new ValueError($"{il1} arguments were passed to a set indexer which only accepts {mph2.ParamLength}.");
				}
			}

			throw new MemberError($"Attempting to set index {key} of object {item} to value {value} failed.");
		}

		private static object IndexAt(object item, params object[] index)
		{
			int len;
			object key = null;

			if (index != null && index.Length > 0)
			{
				len = index.Length;
				key = index[0];
			}
			else
				len = 1;

			Type typetouse;

			if (item is ITuple otup && otup.Length > 1 && otup[0] is Type t && otup[1] is object o)
			{
				typetouse = t;
				item = o;
			}
			else
				typetouse = item.GetType();

			if (len == 1)
			{
				var position = (int)ForceLong(key);

				//The most common is going to be a string, array, map or buffer.
				if (item is string s)
				{
					var actualindex = position < 0 ? s.Length + position : position - 1;
					return s[actualindex];
				}
				else if (item is object[] objarr)//Used for indexing into variadic function params.
				{
					var actualindex = position < 0 ? objarr.Length + position : position - 1;
					return objarr[actualindex];
				}
				else if (item is Core.Buffer buf)
				{
					return buf[position];
				}
				else if (item is System.Array array)
				{
					var actualindex = position < 0 ? array.Length + position : position - 1;
					return array.GetValue(actualindex);
				}

#if WINDOWS
				else if (item is ComObjArray coa)
				{
					var actualindex = position < 0 ? coa.array.Length + position : position;
					return coa.array.GetValue(actualindex);
				}

#endif
				//These are probably never used.
				/*  else if (typeof(IEnumerable).IsAssignableFrom(item.GetType()))
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
				    }*/
			}

			if (Reflections.FindAndCacheMethod(typetouse, "get_Item", len) is MethodPropertyHolder mph)
			{
				if (len == mph.ParamLength || mph.IsVariadic)
					return mph.callFunc(item, index);
				else
					throw new ValueError($"{len} arguments were passed to a get indexer which only accepts {mph.ParamLength}.");
			}

			throw new IndexError($"Attempting to get index of {key} on item {item} failed.");
		}
	}
}