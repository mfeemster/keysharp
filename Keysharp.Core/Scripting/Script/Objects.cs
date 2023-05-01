using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml.Linq;
using Keysharp.Core;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		public static (object, MethodPropertyHolder) GetStaticMethodT<T>(object name) => Reflections.FindAndCacheMethod(typeof(T), name.ToString()) is MethodPropertyHolder mph&& mph.mi != null&& mph.IsStaticFunc
		? (null, mph)
		: throw new MethodError($"Attempting to get method {name} failed.");

		public static object Index(object item, params object[] index) => item == null ? null : IndexAt(item, index);

		public static object SetObject(object value, object item, params object[] index)
		{
			var key = index[0];

			if (item is Map map2)//This function has been redesigned to handle assigning a map key/value pair, or assigning a value to an array index. It is NOT for setting properties.
			{
				map2[key] = value;
				return value;
			}

			var position = (int)ForceLong(key);

			if (item is Core.Array al)
			{
				al[position] = value;
				return value;
			}
			else if (item is Core.Buffer buf)
			{
				throw new IndexError("Cannot call SetObject() on a Buffer object.");
			}
			else if (item == null)
			{
				return null;
			}

			foreach (var mi in item.GetType().GetMethods().Where(m => m.Name == "set_Item"))//Try to see if this is the indexer property __Item.
			{
				var parameters = mi.GetParameters();

				if (parameters.Length > 1)
					return mi.Invoke(item, index.Concat(new object[] { value }));// new object[] { index, value });
				var p = parameters[0];

				if (p.ParameterType == typeof(object))
					return mi.Invoke(item, new object[] { key, value });
				else if (key is long l && p.ParameterType == typeof(long))//Subtract one because arrays are 1-indexed, negative numbers are reverse indexed.
					return mi.Invoke(item, new object[] { l - 1, value });
				else if (key is int i && p.ParameterType == typeof(int))
					return mi.Invoke(item, new object[] { i - 1, value });
			}

			//These are probably never used.
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

			throw new MemberError($"Attempting to set index {key} of object {item} to value {value} failed.");
		}

		private static object IndexAt(object item, params object[] index)
		{
			var key = index == null || index.Length == 0 ? 0 : index[0];

			if (item is Map table)
				return table[key];

			var position = (int)ForceLong(key);

			//The most common is going to be a string, array, map or buffer.
			if (item is string s)
			{
				var actualindex = position < 0 ? s.Length + position : position - 1;
				return s[actualindex];
			}
			else if (item is Core.Array al)
				return al[position];
			else if (item is object[] objarr)//Used for indexing into variadic function params.
			{
				var actualindex = position < 0 ? objarr.Length + position : position - 1;
				return objarr[actualindex];
			}
			else if (item is Core.Buffer buf)
				return buf[position];

			foreach (var mi in item.GetType().GetMethods().Where(m => m.Name == "get_Item"))
			{
				var parameters = mi.GetParameters();

				if (parameters.Length > 1)
					return mi.Invoke(item, index);

				var p = parameters[0];

				if (p.ParameterType == typeof(object))
					return mi.Invoke(item, new object[] { key });
				else if (key is long l && p.ParameterType == typeof(long))//Subtract one because arrays are 1-indexed, negative numbers are reverse indexed.
					return mi.Invoke(item, new object[] { l - 1 });
				else if (key is int i && p.ParameterType == typeof(int))
					return mi.Invoke(item, new object[] { i - 1 });
			}

			//These are probably never used.
			if (item is System.Array array)
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

			throw new IndexError($"Attempting to get index of {key} on item {item} failed.");
		}
	}
}