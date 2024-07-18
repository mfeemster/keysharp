namespace System.Collections
{
	public static class SystemCollectionsExtensions
	{
		public static IEnumerable Flatten(this IEnumerable enumerable)
		{
			if (enumerable is IEnumerable<(object, object)> io)//Iterators for array and map will be this.
			{
				foreach (var el in io)
				{
					var element = el.Item2;

					if (element is IEnumerable candidate && !(element is string))
					{
						foreach (var nested in Flatten(candidate))
							yield return nested;
					}
					else
						yield return element;
				}
			}
			else
			{
				foreach (var element in enumerable)
				{
					if (element is IEnumerable candidate && !(element is string))
					{
						foreach (var nested in Flatten(candidate))
							yield return nested;
					}
					else
						yield return element;
				}
			}
		}

		//public static IEnumerable<object> Flatten(this Memory<object> enumerable)
		//{
		//  if (enumerable is IEnumerable<(object, object)> io)//Iterators for array and map will be this.
		//  {
		//      foreach (var el in io)
		//      {
		//          var element = el.Item2;

		//          if (element is IEnumerable candidate && !(element is string))
		//          {
		//              foreach (var nested in Flatten(candidate))
		//                  yield return nested;
		//          }
		//          else
		//              yield return element;
		//      }
		//  }
		//  else
		//  {
		//      foreach (var element in enumerable)
		//      {
		//          if (element is IEnumerable candidate && !(element is string))
		//          {
		//              foreach (var nested in Flatten(candidate))
		//                  yield return nested;
		//          }
		//          else
		//              yield return element;
		//      }
		//  }
		//}
	}
}

namespace System.Collections.Generic
{
	public static class SystemCollectionsGenericExtensions
	{
		//public static IEnumerable<T> Flatten<T>(this IEnumerable<T> enumerable)
		//{
		//  foreach (var element in enumerable)
		//  {
		//      if (element is IEnumerable<T> candidate)
		//      {
		//          foreach (var nested in Flatten<T>(candidate))
		//              yield return nested;
		//      }
		//      else
		//          yield return element;
		//  }
		//}

		public static bool Ab(this IList obj, int index, bool def = default) => obj.Count > index && obj[index] != null ? Options.OnOff(obj[index]) ?? def : def;

		public static bool Ab(this object obj, bool def = default) => obj != null ? obj.ParseBool() ?? def : def;

		public static double Ad(this IList obj, int index, double def = default) => obj.Count > index && obj[index] != null ? obj[index].ParseDouble().Value : def;

		public static double Ad(this object obj, double def = default) => obj != null ? obj.ParseDouble().Value : def;

		public static float Af(this object obj, float def = default) => obj != null ? obj.ParseFloat().Value : def;

		public static void AddRange<T>(this HashSet<T> hash, IEnumerable<T> add) where T : class, new ()
		{
			foreach (var item in add)
				_ = hash.Add(item);
		}

		public static bool AddUnique<T>(this IList<T> list, T t)
		{
			if (!list.Contains(t))
			{
				list.Add(t);
				return true;
			}

			return false;
		}

		public static int Ai(this object obj, int def = default) => obj != null ? obj.ParseInt().Value : def;

		public static int Ai(this IList obj, int index, int def = default) => obj.Count > index && obj[index] != null ? obj[index].ParseInt().Value : def;

		public static long Al(this object obj, long def = default) => obj != null ? obj.ParseLong().Value : def;

		public static long Al(this IList obj, int index, long def = default) => obj.Count > index && obj[index] != null ? obj[index].ParseLong().Value : def;

		public static object Ao(this IList obj, int index, object def = null) => obj.Count > index ? obj[index] : def;

		public static Dictionary<T, T2> Append<T, T2>(this Dictionary<T, T2> dkt1, Dictionary<T, T2> dkt2)
		{
			foreach (var kv in dkt2)
				_ = dkt1.TryAdd(kv.Key, kv.Value);

			return dkt1;
		}

		public static string As(this object obj, string def = "") => obj != null ? obj.ToString() : def;

		public static string As(this IList obj, int index, string def = "") => obj.Count > index && obj[index] != null ? obj[index].ToString() : def;

		public static uint Aui(this object obj, uint def = default) => obj != null ? obj.ParseUInt().Value : def;

		public static Type GetEnumeratedType<T>(this IEnumerable<T> e) => typeof(T);

		/// <summary>
		/// Retrieve an element from a dictionary if it exists, else add it and return
		/// the newly added element.
		/// </summary>
		/// <typeparam name="K">The key type of the dictionary</typeparam>
		/// <typeparam name="V">The value type of the dictionary</typeparam>
		/// <param name="dictionary">The dictionary to get or add from</param>
		/// <param name="k">The key value to look up</param>
		/// <returns>The existing or newly added element</returns>
		public static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K k) where V : new ()
		{
			if (!dictionary.TryGetValue(k, out var val))
				dictionary.Add(k, val = new V());

			return val;
		}

		/// <summary>
		/// Retrieve an element from a dictionary if it exists, else add it with the passed in value and return
		/// the newly added element.
		/// </summary>
		/// <typeparam name="K">The key type of the dictionary</typeparam>
		/// <typeparam name="V">The value type of the dictionary</typeparam>
		/// <param name="dictionary">The dictionary to get or add from</param>
		/// <param name="k">The key value to look up</param>
		/// <param name="v">The value to add if it doesn't exist</param>
		/// <returns>The existing or newly added element</returns>
		public static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K k, V v)
		{
			if (!dictionary.TryGetValue(k, out var val))
			{
				dictionary.Add(k, v);
				return v;
			}

			return val;
		}

		/// <summary>
		/// Retrieve an element from a dictionary if it exists, else add it and return
		/// the newly added element.
		/// This differs from the function above in that it takes a Func to allocate the new element.
		/// This is needed because we cannot pass arguments to a generic type constructor without
		/// using reflection.
		/// </summary>
		/// <typeparam name="K">The key type of the dictionary</typeparam>
		/// <typeparam name="V">The value type of the dictionary</typeparam>
		/// <param name="dictionary">The dictionary to get or add from</param>
		/// <param name="k">The key value to look up</param>
		/// <param name="constructionFunc">The construction function.</param>
		/// <returns>The existing or newly added element</returns>
		public static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K k, Func<V> constructionFunc)
		{
			if (!dictionary.TryGetValue(k, out var val))
				dictionary.Add(k, val = constructionFunc());

			return val;
		}

		public static V GetOrAdd<K, V>(this OrderedDictionary dictionary, K k) where V : class, new ()
		{
			if (!dictionary.Contains(k))
			{
				var v = new V();
				dictionary.Add(k, v);
				return v;
			}

			return (V)dictionary[k];
		}

		public static V GetOrAdd<K, V, P1>(this OrderedDictionary dictionary, K k, P1 p1) where V : class
		{
			if (!dictionary.Contains(k))
			{
				var type = typeof(V);
				var ctor = type.GetConstructor(new[]
				{
					typeof(P1)
				});

				if (ctor != null)
				{
					var val = ctor.Invoke(new object[]
					{
						p1
					}) as V;
					dictionary.Add(k, val);
					return val;
				}
			}

			return dictionary[k] as V;
		}

		public static int I1(this IList obj, int def1 = 0) => obj.Ai(0, def1);

		public static (int, object, string, string, string) I1O1S3(this IList obj, int def1 = 0, object def2 = null, string def3 = "", string def4 = "", string def5 = "")
		{
			var r1 = obj.Ai(0, def1);
			var r2 = obj.Ao(1, def2);
			var r3 = obj.As(2, def3);
			var r4 = obj.As(3, def4);
			var r5 = obj.As(4, def5);
			return (r1, r2, r3, r4, r5);
		}

		public static (int, object, object, string, string, string) I1O2S3(this IList obj, int def1 = default, object def2 = null, object def3 = null, string def4 = "", string def5 = "", string def6 = "")
		{
			var r1 = obj.Ai(0, def1);
			var r2 = obj.Ao(1, def2);
			var r3 = obj.Ao(2, def3);
			var r4 = obj.As(3, def4);
			var r5 = obj.As(4, def5);
			var r6 = obj.As(5, def6);
			return (r1, r2, r3, r4, r5, r6);
		}

		public static (int, object, object, object, object, string, string, string, int) I1O4S3I1(this IList obj, int def1 = 0, object def2 = null, object def3 = null, object def4 = null, object def5 = null, string def6 = "", string def7 = "", string def8 = "", int def9 = 0)
		{
			var r1 = obj.Ai(0, def1);
			var r2 = obj.Ao(1, def2);
			var r3 = obj.Ao(2, def3);
			var r4 = obj.Ao(3, def4);
			var r5 = obj.Ao(4, def5);
			var r6 = obj.As(5, def6);
			var r7 = obj.As(6, def7);
			var r8 = obj.As(7, def8);
			var r9 = obj.Ai(8, def9);
			return (r1, r2, r3, r4, r5, r6, r7, r8, r9);
		}

		public static (int, int, int, object, object, string, string, string) I3O2S3(this IList obj, int def1 = 0, int def2 = 0, int def3 = 0, object def4 = null, object def5 = null, string def6 = "", string def7 = "", string def8 = "")
		{
			var r1 = obj.Ai(0, def1);
			var r2 = obj.Ai(1, def2);
			var r3 = obj.Ai(2, def3);
			var r4 = obj.Ao(3, def4);
			var r5 = obj.Ao(4, def5);
			var r6 = obj.As(5, def6);
			var r7 = obj.As(6, def7);
			var r8 = obj.As(7, def8);
			return (r1, r2, r3, r4, r5, r6, r7, r8);
		}

		public static (int, int, int, int, object, string, string, string) I4O1S3(this IList obj, int def1 = default, int def2 = default, int def3 = default, int def4 = default, object def5 = null, string def6 = "", string def7 = "", string def8 = "")
		{
			var r1 = obj.Ai(0, def1);
			var r2 = obj.Ai(1, def2);
			var r3 = obj.Ai(2, def3);
			var r4 = obj.Ai(3, def4);
			var r5 = obj.Ao(4, def5);
			var r6 = obj.As(5, def6);
			var r7 = obj.As(6, def7);
			var r8 = obj.As(7, def8);
			return (r1, r2, r3, r4, r5, r6, r7, r8);
		}

		public static (int, int, int, int, object, object, string, string, string) I4O2S3(this IList obj, int def1 = default, int def2 = default, int def3 = default, int def4 = default, object def5 = null, object def6 = null, string def7 = "", string def8 = "", string def9 = "")
		{
			var r1 = obj.Ai(0, def1);
			var r2 = obj.Ai(1, def2);
			var r3 = obj.Ai(2, def3);
			var r4 = obj.Ai(3, def4);
			var r5 = obj.Ao(4, def5);
			var r6 = obj.Ao(5, def6);
			var r7 = obj.As(6, def7);
			var r8 = obj.As(7, def8);
			var r9 = obj.As(8, def9);
			return (r1, r2, r3, r4, r5, r6, r7, r8, r9);
		}

		public static (int, string, string) Is2(this IList obj, int def1 = default, string def2 = "", string def3 = "")
		{
			var r1 = obj.Ai(0, def1);
			var r2 = obj.As(1, def2);
			var r3 = obj.As(2, def3);
			return (r1, r2, r3);
		}

		public static bool IsCallbackResultNonEmpty(this object result) => result != null&& ((result.ParseLong(false) is long l&& l != 0) || result.ParseBool().IsTrue() || (result is string s&& s != ""));

		public static long L1(this IList obj, long def = default) => obj.Al(0, def);

		public static (long, string, string) Ls2(this IList obj, long def1 = default, string def2 = "", string def3 = "")
		{
			var r1 = obj.Al(0, def1);
			var r2 = obj.As(1, def2);
			var r3 = obj.As(2, def3);
			return (r1, r2, r3);
		}

		public static Dictionary<T, T2> Merge<T, T2>(this Dictionary<T, T2> dkt1, Dictionary<T, T2> dkt2)
		{
			var merged = new Dictionary<T, T2>();

			foreach (var kv in dkt1)
				merged.Add(kv.Key, kv.Value);

			foreach (var kv in dkt2)
				_ = merged.TryAdd(kv.Key, kv.Value);

			return merged;
		}

		public static object InvokeEventHandlers(this IEnumerable<IFuncObj> handlers, params object[] obj)
		{
			object result = null;

			if (handlers.Count() > 0)
			{
				var inst = obj.Length > 0 ? obj[0].GetControl() : null;
				//lock (ehLock)
				{
					var oldHandle = Keysharp.Scripting.Script.HwndLastUsed;
					var (pushed, tv) = Threads.BeginThread();

					if (pushed)//If we've exceeded the number of allowable threads, then just do nothing.
					{
						if (inst is Control ctrl && ctrl.FindForm() is Form form)
							Keysharp.Scripting.Script.HwndLastUsed = form.Handle;

						foreach (var handler in handlers)
						{
							if (handler != null)
							{
								result = handler.Call(obj);

								if (result == null)
									continue;

								if (result.IsCallbackResultNonEmpty())
									break;
							}
						}

						Threads.EndThread(pushed);
					}

					Keysharp.Scripting.Script.HwndLastUsed = oldHandle;
				}
			}

			return result;
		}

		public static void ModifyEventHandlers(this List<IFuncObj> handlers, IFuncObj fo, long i)
		{
			if (i > 0)
				handlers.Add(fo);
			else if (i < 0)
				handlers.Insert(0, fo);
			else
				_ = handlers.RemoveAll(d => d.Name == fo.Name);
		}

		public static object O1(this IList obj, object def1 = null)
		{
			var r1 = obj.Ao(0, def1);
			return r1;
		}

		public static (object, string, string, string, string, string, string, string, string, string, string) O1S10(this IList obj, object def1 = null, string def2 = "", string def3 = "", string def4 = "", string def5 = "", string def6 = "", string def7 = "", string def8 = "", string def9 = "", string def10 = "", string def11 = "")
		{
			var r1 = obj.Ao(0, def1);
			var r2 = obj.As(1, def2);
			var r3 = obj.As(2, def3);
			var r4 = obj.As(3, def4);
			var r5 = obj.As(4, def5);
			var r6 = obj.As(5, def6);
			var r7 = obj.As(6, def7);
			var r8 = obj.As(7, def8);
			var r9 = obj.As(8, def9);
			var r10 = obj.As(9, def10);
			var r11 = obj.As(10, def11);
			return (r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11);
		}

		public static (object, string, double, string, string) O1S1D1S2(this IList obj, object def1 = null, string def2 = "", double def3 = default, string def4 = "", string def5 = "")
		{
			var r1 = obj.Ao(0, def1);
			var r2 = obj.As(1, def2);
			var r3 = obj.Ad(2, def3);
			var r4 = obj.As(3, def4);
			var r5 = obj.As(4, def5);
			return (r1, r2, r3, r4, r5);
		}

		public static (object, string, string, string) O1S3(this IList obj, object def1 = null, string def2 = "", string def3 = "", string def4 = "")
		{
			var r1 = obj.Ao(0, def1);
			var r2 = obj.As(1, def2);
			var r3 = obj.As(2, def3);
			var r4 = obj.As(3, def4);
			return (r1, r2, r3, r4);
		}

		public static (object, object, string, string, int, string, string, string) O2S2I1S3(this IList obj, object def1 = null, object def2 = null, string def3 = "", string def4 = "", int def5 = 0, string def6 = "", string def7 = "", string def8 = "")
		{
			var r1 = obj.Ao(0, def1);
			var r2 = obj.Ao(1, def2);
			var r3 = obj.As(2, def3);
			var r4 = obj.As(3, def4);
			var r5 = obj.Ai(4, def5);
			var r6 = obj.As(5, def6);
			var r7 = obj.As(6, def7);
			var r8 = obj.As(7, def8);
			return (r1, r2, r3, r4, r5, r6, r7, r8);
		}

		public static (object, object, string, string, string) O2S3(this IList obj, object def1 = null, object def2 = null, string def3 = "", string def4 = "", string def5 = "")
		{
			var r1 = obj.Ao(0, def1);
			var r2 = obj.Ao(1, def2);
			var r3 = obj.As(2, def3);
			var r4 = obj.As(3, def4);
			var r5 = obj.As(4, def5);
			return (r1, r2, r3, r4, r5);
		}

		public static (object, object, object, string, string, string) O3S3(this IList obj, object def1 = null, object def2 = null, object def3 = null, string def4 = "", string def5 = "", string def6 = "")
		{
			var r1 = obj.Ao(0, def1);
			var r2 = obj.Ao(1, def2);
			var r3 = obj.Ao(2, def3);
			var r4 = obj.As(3, def4);
			var r5 = obj.As(4, def5);
			var r6 = obj.As(5, def6);
			return (r1, r2, r3, r4, r5, r6);
		}

		public static T PeekOrNull<T>(this Stack<T> stack) where T : class => stack.TryPeek(out var result) ? result : null;

		public static T PopOrNull<T>(this Stack<T> stack) where T : class => stack.TryPop(out var result) ? result : null;

		public static string S1(this IList obj, string def = "") => obj.As(0, def);

		public static (string, double, int, object, string, int, string, string) S1D1I1O1S1I1S3(this IList obj, string def1 = "", double def2 = default, int def3 = default, object def4 = default, string def5 = "", int def6 = default, string def7 = "", string def8 = "")
		{
			var r1 = obj.As(0, def1);
			var r2 = obj.Ad(1, def2);
			var r3 = obj.Ai(2, def3);
			var r4 = obj.Ao(3, def4);
			var r5 = obj.As(4, def5);
			var r6 = obj.Ai(5, def6);
			var r7 = obj.As(6, def7);
			var r8 = obj.As(7, def8);
			return (r1, r2, r3, r4, r5, r6, r7, r8);
		}

		public static (string, object, string, string, string) S1O1S3(this IList obj, string def1 = "", object def2 = null, string def3 = "", string def4 = "", string def5 = "")
		{
			var r1 = obj.As(0, def1);
			var r2 = obj.Ao(1, def2);
			var r3 = obj.As(2, def3);
			var r4 = obj.As(3, def4);
			var r5 = obj.As(4, def5);
			return (r1, r2, r3, r4, r5);
		}

		public static (string, object, object, string, string, string) S1O2S3(this IList obj, string def1 = "", object def2 = null, object def3 = null, string def4 = "", string def5 = "", string def6 = "")
		{
			var r1 = obj.As(0, def1);
			var r2 = obj.Ao(1, def2);
			var r3 = obj.Ao(2, def3);
			var r4 = obj.As(3, def4);
			var r5 = obj.As(4, def5);
			var r6 = obj.As(5, def6);
			return (r1, r2, r3, r4, r5, r6);
		}

		public static (string, string) S2(this IList obj, string def1 = "", string def2 = "")
		{
			var r1 = obj.As(0, def1);
			var r2 = obj.As(1, def2);
			return (r1, r2);
		}

		public static (string, string, string) S3(this IList obj, string def1 = "", string def2 = "", string def3 = "")
		{
			var r1 = obj.As(0, def1);
			var r2 = obj.As(1, def2);
			var r3 = obj.As(2, def3);
			return (r1, r2, r3);
		}

		public static (string, long, string) Sls(this IList obj, string def1 = "", long def2 = default, string def3 = "")
		{
			var r1 = obj.As(0, def1);
			var r2 = obj.Al(1, def2);
			var r3 = obj.As(2, def3);
			return (r1, r2, r3);
		}

		public static IEnumerable<byte> ToByteArray(this IList list)
		{
			IList<byte> arr;

			if (list is IList<byte> bb)
				arr = bb;
			else if (list is IList<double> bd)//If values are passed directly, they'll be of type double.
				arr = bd.Select(value => (byte)Convert.ToInt32(value)).ToList();
			else if (list is Keysharp.Core.Array array)
			{
				arr = new List<byte>(list.Count);

				foreach (var (val, index) in array)
				{
					if (val is byte b)
						arr.Add(b);
					else
						arr.Add((byte)Convert.ToInt64(val));
				}
			}
			else//Something else, probably an ArrayList, attempt to convert, slower.
			{
				arr = new List<byte>(list.Count);

				foreach (var item in list)
				{
					if (item is byte b)
						arr.Add(b);
					else
						arr.Add((byte)Convert.ToInt64(item));
				}
			}

			return arr;
		}
	}
}