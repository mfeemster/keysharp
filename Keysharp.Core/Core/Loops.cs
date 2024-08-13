using EnumerationOptions = System.IO.EnumerationOptions;

namespace Keysharp.Core
{
	public static class Loops
	{
		/// <summary>
		/// LoopStack is marked as [ThreadStatic] because it must actually be thread safe for real threads.
		/// </summary>
		[ThreadStatic]
		private static Stack<LoopInfo> loopStack;

		internal static Stack<LoopInfo> LoopStack => loopStack ?? (loopStack = new Stack<LoopInfo>());

		public static long Inc()
		{
			var s = LoopStack;
			return s.Count > 0 ? ++s.Peek().index : 0;
		}

		/// <summary>
		/// Perform a series of commands repeatedly: either the specified number of times or until break is encountered.
		/// </summary>
		/// <param name="n">How many times (iterations) to perform the loop.</param>
		/// <returns></returns>
		public static IEnumerable Loop(object obj)
		{
			if (!(obj is string ss) || ss != string.Empty)
			{
				var n = obj.Al();
				var info = Push();

				if (n != -1)
				{
					for (; info.index < n && !Keysharp.Core.Flow.hasExited;)//Check info.index because the caller can change A_Index inside of the loop.
						yield return ++info.index;
				}
				else
				{
					while (!Keysharp.Core.Flow.hasExited)
						yield return ++info.index;
				}

				//The caller *MUST* call Pop(). This design is used because this
				//function may exit prematurely if the caller does a goto or break out
				//of the loop. In which case, all code below the yield return statement
				//would not get executed. So the burden is shifted to the caller to pop.
			}
		}

		/// <summary>
		/// Retrieves each element of an array with its key if any.
		/// </summary>
		/// <param name="array">An array or object.</param>
		/// <returns>The current element.</returns>
		public static IEnumerable LoopEach(object array)
		{
			if (array == null)
				yield break;

			var info = new LoopInfo { type = LoopType.Each };
			var s = LoopStack;
			s.Push(info);
			var type = array.GetType();

			if (array is Map map)
			{
				foreach (var (k, v) in map)
				{
					if (Keysharp.Core.Flow.hasExited)
						break;

					info.result = new[] { k, v };
					info.index++;
					yield return info.result;
				}
			}
			else if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				var enumerator = ((IEnumerable)array).GetEnumerator();

				while (enumerator.MoveNext())
				{
					if (Keysharp.Core.Flow.hasExited)
						break;

					info.result = new[] { null, enumerator.Current };
					info.index++;
					yield return info.result;
				}
			}

			_ = s.Pop();
		}

		/// <summary>
		/// Retrieves the specified files or folders, one at a time.
		/// </summary>
		/// <param name="path">The name of a single file or folder, or a wildcard pattern.</param>
		/// <param name="mode">One of the following digits, or blank to use the default:
		/// <list>
		/// <item><code>D</code> Include directories (folders).</item>
		/// <item><code>F</code> Include files. If both F and D are omitted, files are included but not folders.</item>
		/// <item><code>R</code> Recurse into subdirectories (subfolders). All subfolders will be recursed into, not just those whose names match FilePattern. If R is omitted, files and folders in subfolders are not included.</item>
		/// </list>
		/// </param>
		public static IEnumerable LoopFile(object obj0, object obj1 = null)
		{
			bool d = false, f = true, r = false;
			var info = Push(LoopType.Directory);
			var path = obj0.As();
			var mode = obj1.As();
			//Dialogs.MsgBox(Path.GetFullPath(path));
			//Dialogs.MsgBox(Accessors.A_WorkingDir);

			//Convert something like "*.txt" to "./*.txt".
			if (!path.StartsWith("\\\\") && !char.IsLetter(path[0]) && path[0] != '.' && path[0] != '/')
				path = "." + Path.DirectorySeparatorChar + path;

			if (!string.IsNullOrEmpty(mode))
			{
				d = mode.Contains('d', StringComparison.OrdinalIgnoreCase);
				f = mode.Contains('f', StringComparison.OrdinalIgnoreCase);
				r = mode.Contains('r', StringComparison.OrdinalIgnoreCase);
			}

			if (!d && !f)
				f = true;

			var dir = Path.GetDirectoryName(path);
			var pattern = Path.GetFileName(path);
			info.path = dir;

			foreach (var file in GetFiles(dir, pattern, d, f, r))
			{
				if (Keysharp.Core.Flow.hasExited)
					break;

				info.file = file;
				info.index++;
				yield return file;
			}

			//Caller must call Pop() after the loop exits.
		}

		public static long LoopIndex()
		{
			var s = LoopStack;
			return s.Count > 0 ? s.Peek().index : 0;
		}

		/// <summary>
		/// Retrieves substrings (fields) from a string, one at a time.
		/// </summary>
		/// <param name="input">The string to parse.</param>
		/// <param name="delimiters">One of the following:
		/// <list>
		/// <item>the word <code>CSV</code> to parse in comma seperated value format;</item>
		/// <item>a sequence of characters to treat as delimiters;</item>
		/// <item>blank to parse each character of the string.</item>
		/// </list>
		/// </param>
		/// <param name="omit">An optional list of characters (case sensitive) to exclude from the beginning and end of each substring.</param>
		/// <returns></returns>
		public static IEnumerable LoopParse(object obj0, object obj1 = null, object obj2 = null)
		{
			var input = obj0.As();
			var delimiters = obj1.As();
			var omit = obj2.As();
			var info = Push(LoopType.Parse);

			if (delimiters.ToLowerInvariant() == Keywords.Keyword_CSV)
			{
				var reader = new StringReader(input);
				var part = new StringBuilder();
				bool str = false, next = false;

				while (true)
				{
					var current = reader.Read();

					if (current == -1)
						goto collect;

					const char tokenStr = '"', tokenDelim = ',';
					var sym = (char)current;

					switch (sym)
					{
						case tokenStr:
							if (str)
							{
								if ((char)reader.Peek() == tokenStr)
								{
									_ = part.Append(tokenStr);
									_ = reader.Read();
								}
								else
									str = false;
							}
							else
							{
								if (next)
									_ = part.Append(tokenStr);
								else
									str = true;
							}

							break;

						case tokenDelim:
							if (str)
								goto default;

							goto collect; // sorry

						default:
							next = true;
							_ = part.Append(sym);
							break;
					}

					continue;
					collect:
					next = false;
					var result = part.ToString();
					part.Length = 0;
					info.result = result;
					info.index++;
					yield return result;

					if (current == -1 || Keysharp.Core.Flow.hasExited)
						break;
				}
			}
			else
			{
				string[] parts;

				var remove = omit.ToCharArray();

				if (string.IsNullOrEmpty(delimiters))
					parts = input.ToCharArray().Select(x => x.ToString().Trim(remove)).Where(x => x != string.Empty).ToArray();
				else
					parts = input.Split(delimiters.ToCharArray(), StringSplitOptions.None).Select(x => x.Trim(remove)).Where(x => x != string.Empty).ToArray();

				foreach (var part in parts)
				{
					if (Keysharp.Core.Flow.hasExited)
						break;

					info.result = part;
					info.index++;
					yield return part;
				}
			}

			//Caller must call Pop() after the loop exits.
		}

		/// <summary>
		/// Retrieves the lines in a text file, one at a time.
		/// </summary>
		/// <param name="input">The name of the text file whose contents will be read by the loop</param>
		/// <param name="output">The optional name of the file to be kept open for the duration of the loop. If "*", then write to standard output.</param>
		/// <returns>Yield return each line in the input file</returns>
		public static IEnumerable LoopRead(object obj0, object obj1 = null)
		{
			var input = obj0.As();
			var output = obj1.As();
			var info = Push(LoopType.File);
			//Dialogs.MsgBox(Path.GetFullPath(input));

			if (output.Length > 0)
				info.filename = output;

			if (!System.IO.File.Exists(input))
				yield break;

			using (var reader = System.IO.File.OpenText(input))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					if (Keysharp.Core.Flow.hasExited)
						break;

					info.line = line;
					info.index++;
					yield return line;
				}
			}

			//Caller must call Pop() after the loop exits.
		}
#if WINDOWS
		/// <summary>
		/// Retrieves the contents of the specified registry subkey, one item at a time.
		/// </summary>
		/// <param name="root">Must be either:
		/// HKEY_LOCAL_MACHINE (or HKLM)
		/// HKEY_USERS (or HKU)
		/// HKEY_CURRENT_USER (or HKCU)
		/// HKEY_CLASSES_ROOT (or HKCR)
		/// HKEY_CURRENT_CONFIG (or HKCC)
		/// HKEY_PERFORMANCE_DATA (or HKPD)
		/// </param>
		/// <param name="key">The name of the key (e.g. Software\SomeApplication). If blank or omitted, the contents of RootKey will be retrieved.</param>
		/// <param name="subkeys">
		/// <list>
		/// <item><code>1</code> subkeys contained within Key are not retrieved (only the values);</item>
		/// <item><code>1</code> all values and subkeys are retrieved;</item>
		/// <item><code>2</code> only the subkeys are retrieved (not the values).</item>
		/// </list>
		/// </param>
		/// <param name="recurse"><code>1</code> to recurse into subkeys, <code>0</code> otherwise.</param>
		/// <returns></returns>
		public static IEnumerable LoopRegistry(object obj0, object obj1 = null)
		{
			bool k = false, v = true, r = false;
			var keyname = obj0.As();
			var mode = obj1.As();

			if (!string.IsNullOrEmpty(mode))
			{
				k = mode.Contains('k', StringComparison.OrdinalIgnoreCase);
				v = mode.Contains('v', StringComparison.OrdinalIgnoreCase);
				r = mode.Contains('r', StringComparison.OrdinalIgnoreCase);
			}

			if (!k && !v)
				v = true;

			var info = Push(LoopType.Registry);
			var (reg, compname, key) = Conversions.ToRegRootKey(keyname);

			if (reg != null)
			{
				info.regVal = string.Empty;
				info.regName = reg.Name;
				info.regKeyName = keyname;
				info.regType = Keywords.Keyword_Key;
				var subkey = reg.OpenSubKey(key, false);
				var l = QueryInfoKey(subkey);
				var dt = DateTime.FromFileTimeUtc(l);
				info.regDate = Conversions.ToYYYYMMDDHH24MISS(dt);

				if (r)
				{
					foreach (var val in GetSubKeys(info, subkey, k, v))
					{
						if (Keysharp.Core.Flow.hasExited)
							break;

						yield return val;
					}
				}
				else
				{
					if (v)
					{
						foreach (var valueName in subkey.GetValueNames().Reverse())
						{
							if (Keysharp.Core.Flow.hasExited)
								break;

							info.index++;
							info.regVal = subkey.GetValue(valueName, string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames);

							if (info.regVal is byte[] ro)
								info.regVal = BitConverter.ToString(ro).Replace("-", string.Empty);

							info.regName = valueName;
							info.regType = Conversions.GetRegistryTypeName(subkey.GetValueKind(valueName));
							yield return valueName;
						}
					}

					if (k)
					{
						foreach (var subKeyName in subkey.GetSubKeyNames().Reverse())//AHK spec says the subkeys and values are returned in reverse.
						{
							if (Keysharp.Core.Flow.hasExited)
								break;

							using (var tempKey = subkey.OpenSubKey(subKeyName, false))
							{
								info.index++;
								info.regVal = string.Empty;
								info.regName = subKeyName.Substring(subKeyName.LastIndexOf('\\') + 1);
								info.regKeyName = tempKey.Name;//The full key path.
								info.regType = Keywords.Keyword_Key;
								l = QueryInfoKey(tempKey);
								dt = DateTime.FromFileTimeUtc(l);
								info.regDate = Conversions.ToYYYYMMDDHH24MISS(dt);
								yield return info.regKeyName;
							}
						}
					}

					info.regDate = string.Empty;//Date is empty outside of keys.
				}
			}

			//Caller must call Pop() after the loop exits.
		}
#endif

		public static IEnumerator MakeBaseEnumerator(object obj)
		{
			if (obj is IEnumerable ie)
				return ie.GetEnumerator();
			else if (obj is IEnumerator ie2)
				return ie2;
			else
				throw new Error($"Object of type {obj.GetType()} was not of a type that could be converted to an IEnumerator.");
		}

		public static IEnumerator<(object, object)> MakeEnumerator(object obj)
		{
			if (obj is IEnumerable<(object, object)> ie0)
				return ie0.GetEnumerator();
			else if (obj is IEnumerator<(object, object)> ie1)
				return ie1;
			else if (obj is object[] oa)
				return new Array(oa).GetEnumerator();
			else if (obj is IEnumerable ie)
				return ie.Cast<object>().Select(o => (o, o)).GetEnumerator();
			else if (obj is null)
				throw new Error($"Object was null and could not be converted to an IEnumerator<object, objecT>.");
			else
				throw new Error($"Object of type {obj.GetType()} was not of a type that could be converted to an IEnumerator<object, objecT>.");
		}

		public static LoopInfo Pop()
		{
			var s = LoopStack;
			var info = s.Count > 0 ? s.Pop() : null;

			if (info != null && info.type == LoopType.File && info.sw != null)
				info.sw.Close();

			return info;
		}

		public static LoopInfo Push(LoopType t = LoopType.Normal)
		{
			var info = new LoopInfo { type = t };
			LoopStack.Push(info);
			return info;
		}

		internal static LoopInfo GetDirLoop()
		{
			var s = LoopStack;

			if (s.Count > 0)
			{
				foreach (var l in s)
				{
					switch (l.type)
					{
						case LoopType.Directory:
							return l;
					}
				}
			}

			return null;
		}

		internal static string GetDirLoopFilename()
		{
			var s = LoopStack;

			if (s.Count == 0)
				return string.Empty;

			foreach (var l in s)
			{
				switch (l.type)
				{
					case LoopType.Directory:
						return l.file as string;
				}
			}

			return null;
		}

		/// <summary>
		/// From https://stackoverflow.com/questions/325931/getting-actual-file-name-with-proper-casing-on-windows-with-net
		/// </summary>
		/// <param name="pathName"></param>
		/// <returns></returns>
		internal static string GetExactPath(string pathName)
		{
			if (!(System.IO.File.Exists(pathName) || Directory.Exists(pathName)))
				return pathName;

			var di = new DirectoryInfo(pathName);

			if (di.Parent != null)
			{
				return Path.Combine(
						   GetExactPath(di.Parent.FullName),
						   di.Parent.GetFileSystemInfos(di.Name)[0].Name);
			}
			else
			{
				return di.Name.ToUpper();
			}
		}

		internal static string GetShortPath(string filename)
		{
#if WINDOWS
			var buffer = new StringBuilder(1024);
			_ = WindowsAPI.GetShortPathName(filename, buffer, buffer.Capacity);
			return buffer.ToString();
#else
			return "";
#endif
		}

		internal static LoopInfo Peek() => LoopStack.PeekOrNull();

		internal static LoopInfo Peek(LoopType looptype)
		{
			foreach (var l in LoopStack)
				if (l.type == looptype)
					return l;

			return null;
		}

		private static IEnumerable<string> GetFiles(string path, string pattern, bool d, bool f, bool r)
		{
			var queue = new Queue<string>();
			queue.Enqueue(path);
			var enumopts = new EnumerationOptions
			{
				AttributesToSkip = FileAttributes.Normal,
				IgnoreInaccessible = true,
				MatchCasing = MatchCasing.CaseInsensitive,
				RecurseSubdirectories = r
			};

			while (queue.Count > 0)
			{
				path = queue.Dequeue();

				if (d)
				{
					IEnumerable<string> subdirs = null;

					try
					{
						subdirs = Directory.EnumerateDirectories(path, pattern, enumopts);
					}
					catch (Exception ex)
					{
						Script.OutputDebug(ex);
					}

					if (subdirs != null)
					{
						foreach (var subdir in subdirs)
						{
							queue.Enqueue(subdir);
							yield return subdir;
						}
					}
				}

				if (f)
				{
					IEnumerable<string> files = null;

					try
					{
						files = Directory.EnumerateFiles(path, pattern, enumopts);
					}
					catch (Exception ex)
					{
						Script.OutputDebug(ex);
					}

					if (files != null)
					{
						foreach (var file in files)
						{
							yield return file;
						}
					}
				}

				if (!enumopts.RecurseSubdirectories)
					break;
			}
		}
#if WINDOWS
		private static IEnumerable GetSubKeys(LoopInfo info, RegistryKey key, bool k, bool v)
		{
			//try
			{
				if (v)
				{
					foreach (var val in ProcessRegValues(info, key))
						yield return val;
				}

				var subkeynames = key.GetSubKeyNames();

				if (subkeynames?.Length > 0)
				{
					foreach (var keyname in subkeynames.Reverse())
					{
						//try
						{
							using (var key2 = key.OpenSubKey(keyname, false))
							{
								if (k)
								{
									info.index++;
									info.regVal = string.Empty;
									info.regName = key2.Name.Substring(key2.Name.LastIndexOf('\\') + 1);
									info.regKeyName = key2.Name;//The full key path.
									info.regType = Keywords.Keyword_Key;
									var l = QueryInfoKey(key2);
									var dt = DateTime.FromFileTimeUtc(l);
									info.regDate = Conversions.ToYYYYMMDDHH24MISS(dt);
									yield return info.regKeyName;
								}

								foreach (var val in GetSubKeys(info, key2, k, v))
									yield return val;
							}
						}
						//catch (Exception e)
						//{
						//  //error, do something
						//}
					}
				}
			}
			//catch (Exception e)
			//{
			//  //error, do something
			//}
		}

		private static IEnumerable ProcessRegValues(LoopInfo info, RegistryKey key)
		{
			var valuenames = key.GetValueNames();

			if (valuenames?.Length > 0)
			{
				info.regDate = string.Empty;

				foreach (var valueName in valuenames.Reverse())
				{
					info.index++;
					info.regVal = key.GetValue(valueName, string.Empty, RegistryValueOptions.None);

					if (info.regVal is byte[] ro)
						info.regVal = BitConverter.ToString(ro).Replace("-", string.Empty);

					info.regName = valueName;
					info.regType = Conversions.GetRegistryTypeName(key.GetValueKind(valueName));
					yield return valueName;
				}
			}
		}

		private static long QueryInfoKey(RegistryKey regkey)
		{
			var tv = Threads.GetThreadVariables();

			if (tv.regsb == null)
				tv.regsb = new StringBuilder(1024);
			else
				_ = tv.regsb.Clear();

			var classSize = (uint)(tv.regsb.Capacity + 1);
			_ = WindowsAPI.RegQueryInfoKey(
					regkey.Handle,
					tv.regsb,
					ref classSize,
					IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
					out var l);
			return l;
		}
#endif
	}

	public class LoopInfo
	{
		public object file;
		public string filename = string.Empty;
		public long index;
		public string line;
		public string path;
		public object regDate;
		public string regKeyName;
		public string regName;
		public string regType;
		public object regVal;
		public object result;
		public TextWriter sw;

		public LoopType type = LoopType.Normal;

		public LoopInfo()
		{
		}
	}

	public enum LoopType
	{
		Normal,
#if WINDOWS
		Registry,
#endif
		Directory,
		Parse,
		File,
		Each,
	}
}