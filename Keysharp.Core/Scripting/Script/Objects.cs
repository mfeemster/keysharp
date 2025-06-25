using Keysharp.Core.Common.Keyboard;

namespace Keysharp.Scripting
{
	public partial class Script
	{

		public static object Index(object item, params object[] index) => item == null ? null : IndexAt(item, index);

		public static object SetObject(object value, object item, params object[] index)
		{
			object key = null;
			Type typetouse;

			try
			{
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

					//This excludes types derived from Array so that super can be used.
					if (item.GetType() == typeof(Keysharp.Core.Array))
					{
						((Keysharp.Core.Array)item)[key] = value;
						return value;
					}
					else if (item.GetType() == typeof(Keysharp.Core.Map))
					{
						((Keysharp.Core.Map)item)[key] = value;
						return value;
					}

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
					else if (item == null)
					{
						return DefaultErrorObject;
					}
				}
				else if (index.Length == 0)//Access brackets with no index like item.prop[] := 123.
				{
					if (Reflections.FindAndCacheInstanceMethod(typetouse, "set_Item", 0) is MethodPropertyHolder mph1)
					{
						_ = mph1.callFunc(item, index.Concat([value]));
						return value;
					}
				}

#if WINDOWS
				if (item is ComObjArray coa)
				{
					coa[index] = value;
					return value;
				}
				else if (item is ComObject co)
				{
					if (index.Length == 0 && (co.vt & VarEnum.VT_BYREF) != 0)
					{
						ComObject.WriteVariant(co.Ptr.Al(), co.vt, value);
						return value;
					}
					else
						return co.Ptr.GetType().InvokeMember("Item", BindingFlags.SetProperty, null, co.Ptr, index.Concat([value]));
				}
				else if (Marshal.IsComObject(item))
					return item.GetType().InvokeMember("Item", BindingFlags.SetProperty, null, item, index.Concat([value]));

#endif
				var il1 = index.Length + 1;

				if (Reflections.FindAndCacheInstanceMethod(typetouse, "set_Item", il1) is MethodPropertyHolder mph2)
				{
					if (il1 == mph2.ParamLength || mph2.IsVariadic)
					{
						_ = mph2.callFunc(item, index.Concat([value]));
						return value;
					}
					else
						return Errors.ValueErrorOccurred($"{il1} arguments were passed to a set indexer which only accepts {mph2.ParamLength}.");
				}
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			return Errors.ErrorOccurred($"Attempting to set index {key} of object {item} to value {value} failed.");
		}

		private static object IndexAt(object item, params object[] index)
		{
			int len;
			object key = null;

			try
			{
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
					//This excludes types derived from Array so that super can be used.
					if (item.GetType() == typeof(Keysharp.Core.Array))
					{
						return ((Keysharp.Core.Array)item)[key];
					}
					else if (item.GetType() == typeof(Keysharp.Core.Map))
					{
						return ((Keysharp.Core.Map)item)[key];
					}

					var position = (int)ForceLong(key);

					//The most common is going to be a string, array, map or buffer.
					if (item is string s)
					{
						var actualindex = position < 0 ? s.Length + position : position - 1;
						return s[actualindex];
					}
					else if (item.GetType() == typeof(Keysharp.Core.Buffer))
					{
						return ((Keysharp.Core.Buffer)item)[position];
					}
					else if (item is object[] objarr)//Used for indexing into variadic function params.
					{
						var actualindex = position < 0 ? objarr.Length + position : position - 1;
						return objarr[actualindex];
					}
					else if (item is System.Array array)
					{
						var actualindex = position < 0 ? array.Length + position : position - 1;
						return array.GetValue(actualindex);
					}
				}

#if WINDOWS
				if (item is ComObjArray coa)
				{
					return coa[index];
				}
				else if (item is ComObject co)
				{
					//Could be an indexer, but MethodPropertyHolder currently doesn't support those
					if (index.Length == 0 && (co.vt & VarEnum.VT_BYREF) != 0)
						return ComObject.ReadVariant(co.Ptr.Al(), co.vt);
					return Invoke((co.Ptr, new ComMethodPropertyHolder("Item")), index);
				}
				else if (Marshal.IsComObject(item))
					return Invoke((item, new ComMethodPropertyHolder("Item")), index);

#endif

				if (Reflections.FindAndCacheInstanceMethod(typetouse, "get_Item", len) is MethodPropertyHolder mph)
				{
					if (len == mph.ParamLength || mph.IsVariadic)
						return mph.callFunc(item, index);
					else
						return Errors.ValueErrorOccurred($"{len} arguments were passed to a get indexer which only accepts {mph.ParamLength}.");
				}
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			return Errors.ErrorOccurred($"Attempting to get index of {key} on item {item} failed.");
		}
	}
}