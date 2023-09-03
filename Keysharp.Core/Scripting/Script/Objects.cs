using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml.Linq;
using Keysharp.Core;
using Keysharp.Core.COM;

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
				else if (item is ComObjArray coa)
				{
					var actualindex = position < 0 ? coa.array.Length + position : position;
					coa.array.SetValue(value, actualindex);
					return value;
				}
				else if (item == null)
				{
					return null;
				}
			}
			else if (index.Length == 0)//Access brackets with no index like item.prop[] := 123.
			{
				if (Reflections.FindAndCacheMethod(typetouse, "set_Item", 0) is MethodPropertyHolder mph1)
				{
					mph1.callFunc(item, index.Concat(new object[] { value }));
					return value;
				}
			}

			if (Reflections.FindAndCacheMethod(typetouse, "set_Item", index.Length + 1) is MethodPropertyHolder mph2)
			{
				mph2.callFunc(item, index.Concat(new object[] { value }));
				return value;
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
				else if (item is ComObjArray coa)
				{
					var actualindex = position < 0 ? coa.array.Length + position : position;
					return coa.array.GetValue(actualindex);
				}

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

			if (Reflections.FindAndCacheMethod(typetouse, "get_Item", len) is MethodPropertyHolder mph1)
				return mph1.callFunc(item, index);

			throw new IndexError($"Attempting to get index of {key} on item {item} failed.");
		}
	}
}