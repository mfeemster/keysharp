namespace System.Collections.Generic
{
	internal static class SystemCollectionsGenericExtensions
	{
		//internal static IEnumerable<T> Flatten<T>(this IEnumerable<T> enumerable)
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

		internal static void AddRange<T>(this HashSet<T> hash, IEnumerable<T> add) where T : class, new ()
		{
			foreach (var item in add)
				_ = hash.Add(item);
		}

		internal static bool AddUnique<T>(this IList<T> list, T t)
		{
			if (!list.Contains(t))
			{
				list.Add(t);
				return true;
			}

			return false;
		}

		internal static Dictionary<T, T2> Append<T, T2>(this Dictionary<T, T2> dkt1, Dictionary<T, T2> dkt2)
		{
			foreach (var kv in dkt2)
				_ = dkt1.TryAdd(kv.Key, kv.Value);

			return dkt1;
		}

		internal static Type GetEnumeratedType<T>(this IEnumerable<T> e) => typeof(T);

		/// <summary>
		/// Retrieve an element from a dictionary if it exists, else add it and return
		/// the newly added element.
		/// </summary>
		/// <typeparam name="K">The key type of the dictionary</typeparam>
		/// <typeparam name="V">The value type of the dictionary</typeparam>
		/// <param name="dictionary">The dictionary to get or add from</param>
		/// <param name="k">The key value to look up</param>
		/// <returns>The existing or newly added element</returns>
		internal static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K k) where V : new ()
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
		internal static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K k, V v)
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
		internal static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K k, Func<V> constructionFunc)
		{
			if (!dictionary.TryGetValue(k, out var val))
				dictionary.Add(k, val = constructionFunc());

			return val;
		}

		internal static V GetOrAdd<K, V>(this OrderedDictionary dictionary, K k) where V : class, new ()
		{
			if (!dictionary.Contains(k))
			{
				var v = new V();
				dictionary.Add(k, v);
				return v;
			}

			return (V)dictionary[k];
		}

		internal static V GetOrAdd<K, V, P1>(this OrderedDictionary dictionary, K k, P1 p1) where V : class
		{
			if (!dictionary.Contains(k))
			{
				var type = typeof(V);
				var ctor = type.GetConstructor(
							   [
								   typeof(P1)
							   ]);

				if (ctor != null)
				{
					var val = ctor.Invoke(
								  [
									  p1
								  ]) as V;
					dictionary.Add(k, val);
					return val;
				}
			}

			return dictionary[k] as V;
		}

		internal static object InvokeEventHandlers(this IEnumerable<IFuncObj> handlers, params object[] obj)
		{
			object result = null;

			if (handlers.Count() > 0)
			{
				var inst = obj.Length > 0 ? obj[0].GetControl() : null;
				//lock (ehLock)
				{
					var oldHandle = Script.HwndLastUsed;
					var oldEventInfo = Accessors.A_EventInfo;
					var (pushed, tv) = Threads.BeginThread();

					if (pushed)//If we've exceeded the number of allowable threads, then just do nothing.
					{
						tv.eventInfo = oldEventInfo;
						_ = Flow.TryCatch(() =>
						{
							if (inst is Control ctrl && ctrl.FindForm() is Form form)
								Script.HwndLastUsed = form.Handle;

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

							Threads.EndThread(true);
						}, true);//Pop on exception because the Pop above won't be called.
					}

					Script.HwndLastUsed = oldHandle;
				}
			}

			return result;
		}

		internal static Dictionary<T, T2> Merge<T, T2>(this Dictionary<T, T2> dkt1, Dictionary<T, T2> dkt2)
		{
			var merged = new Dictionary<T, T2>();

			foreach (var kv in dkt1)
				merged.Add(kv.Key, kv.Value);

			foreach (var kv in dkt2)
				_ = merged.TryAdd(kv.Key, kv.Value);

			return merged;
		}

		internal static void ModifyEventHandlers(this List<IFuncObj> handlers, IFuncObj fo, long i)
		{
			if (i > 0)
				handlers.Add(fo);
			else if (i < 0)
				handlers.Insert(0, fo);
			else
				_ = handlers.RemoveAll(d => d.Name == fo.Name);
		}

		internal static T PeekOrNull<T>(this Stack<T> stack) where T : class => stack.TryPeek(out var result) ? result : null;

		internal static T PopOrNull<T>(this Stack<T> stack) where T : class => stack.TryPop(out var result) ? result : null;
	}
}