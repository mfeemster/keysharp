namespace Keysharp.Core
{
	public static class Files
	{
		/// <summary>
		/// Writes text to the end of a file, creating it first if necessary.
		/// </summary>
		/// <param name="text">The text to append to <paramref name="file"/>.</param>
		/// <param name="filename">The name of the file to be appended.
		/// <list type="bullet">
		/// <item><term>Binary mode</term>: <description>to append in binary mode rather than text mode, prepend an asterisk.</description></item>
		/// <item><term>Standard output (stdout)</term>: <description>specifying an asterisk (*) causes <paramref name="text"/> to be written to the console.</description></item>
		/// </list>
		/// </param>
		public static void FileAppend(object obj0, object obj1 = null, object obj2 = null)
		{
			try
			{
				var text = obj0;
				var filename = obj1.As();
				var encoding = ThreadAccessors.A_FileEncodingRaw;
				var raw = false;
				var crlf = false;
				TextWriter tw = null;

				if (obj2.As() is string options)
				{
					foreach (Range r in options.AsSpan().SplitAny(Keywords.SpaceTabSv))
					{
						var split = options.AsSpan(r).Trim(Keywords.SpaceTab);//Need to supply chars to trim else \n would get automatically trimmed.

						if (split.Length > 0)
						{
							switch (split)
							{
								case var b when split.Equals("ascii", StringComparison.OrdinalIgnoreCase):
								case var b2 when split.Equals("us-ascii", StringComparison.OrdinalIgnoreCase):
									encoding = System.Text.Encoding.ASCII;
									break;

								case var b when split.Equals("utf-8", StringComparison.OrdinalIgnoreCase):
									encoding = System.Text.Encoding.UTF8;
									break;

								case var b when split.Equals("utf-8-raw", StringComparison.OrdinalIgnoreCase):
									encoding = new UTF8Encoding(false);//Not byte order mark.
									break;

								case var b when split.Equals("utf-16", StringComparison.OrdinalIgnoreCase):
								case var b2 when split.Equals("unicode", StringComparison.OrdinalIgnoreCase):
									encoding = System.Text.Encoding.Unicode;
									break;

								case var b when split.Equals("utf-16-raw", StringComparison.OrdinalIgnoreCase):
									encoding = new UnicodeEncoding(false, false);//Little endian, no byte order mark.
									break;

								case var b when split.Equals("raw", StringComparison.OrdinalIgnoreCase):
									raw = true;
									break;

								case var b when split.Equals("`n", StringComparison.OrdinalIgnoreCase):
								case var b2 when split.Equals("\n", StringComparison.OrdinalIgnoreCase):
									crlf = true;
									break;
							}
						}
					}
				}

				if (string.IsNullOrEmpty(filename))
				{
					var info = Loops.Peek(LoopType.File);

					if (info != null)
					{
						if (info.filename != string.Empty)
						{
							if (info.sw == null)
							{
								if (info.filename == "*")
									info.sw = Console.Out;
								else if (info.filename == "**")
									info.sw = Console.Error;
								else
									info.sw = new StreamWriter(info.filename, true, encoding);
							}

							tw = info.sw;
						}
					}
				}
				else if (filename == "*")
					tw = Console.Out;
				else if (filename == "**")
					tw = Console.Error;
				else
					tw = new StreamWriter(filename, true, encoding);

				if (tw != null)
				{
					if (text is string s)
					{
						if (s == "fail")
							Keysharp.Scripting.Script.OutputDebug(s);

						if (raw)
							//sw.Write(Encoding.Unicode.GetBytes(s));
							tw.Write(s.AsSpan());
						else if (crlf)
							tw.Write(Regex.Replace(s, @"\r\n|\n\r|\n|\r", "\r\n"));//Gotten from https://stackoverflow.com/questions/140926/normalize-newlines-in-c-sharp
						else
							tw.Write(s);
					}
					else if (tw is StreamWriter sw)
					{
						if (text is Keysharp.Core.Array arr)//Most common will be array.
						{
							sw.BaseStream.Write(arr.ToByteArray().ToArray());
						}
						else if (text is Keysharp.Core.Buffer buf)
						{
							var len = (int)(long)buf.Size;
							unsafe
							{
								var bytes = new byte[len];
								Marshal.Copy(buf.Ptr, bytes, 0, len);
								sw.BaseStream.Write(bytes);
							}
						}
						else if (text is byte[] ib)
						{
							sw.BaseStream.Write(ib);
						}
						else if (text is IList il)//It was some other type of container, rare.
						{
							sw.BaseStream.Write(il.ToByteArray().ToArray());
						}
					}

					if (!string.IsNullOrEmpty(filename) && !filename.StartsWith("*"))
						tw.Close();
				}
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}
		}

		/// <summary>
		/// Copies one or more files.
		/// </summary>
		/// <param name="source">The name of a single file or folder, or a wildcard pattern.</param>
		/// <param name="destination">The name or pattern of the destination.</param>
		/// <param name="flag">
		/// <list type="bullet">
		/// <item><term>0</term>: <description>(default) do not overwrite existing files</description></item>
		/// <item><term>1</term>: <description>overwrite existing files</description></item>
		/// </list>
		/// </param>
		public static void FileCopy(object obj0, object obj1, object obj2 = null)
		{
			var source = obj0.As();
			var dest = obj1.As();
			var flag = obj2.Ab();
			FileCopyMove(source, dest, flag, false);
		}

		/// <summary>
		/// Creates a shortcut to a file.
		/// https://www.fluxbytes.com/csharp/create-shortcut-programmatically-in-c/
		/// </summary>
		/// <param name="target">Path to the shortcut file.</param>
		/// <param name="link">The file referenced by the shortcut.</param>
		/// <param name="workingDir">The working directory.</param>
		/// <param name="args">Arguments to start <paramref name="link"/> with.</param>
		/// <param name="description">A summary of the shortcut.</param>
		/// <param name="icon"></param>
		/// <param name="shortcutKey">A hotkey activator.</param>
		/// <param name="iconNumber"></param>
		/// <param name="runState">The window state for the target program the shortcut refers to:
		/// 1 - Normal (this is the default)
		/// 3 - Maximized
		/// 7 - Minimized
		/// </param>
		public static void FileCreateShortcut(object obj0, object obj1, object obj2 = null, object obj3 = null, object obj4 = null, object obj5 = null, object obj6 = null, object obj7 = null, object obj8 = null)
		{
			var target = obj0.As();
			var link = obj1.As();
			var workingDir = obj2.As();
			var args = obj3.As();
			var description = obj4.As();
			var icon = obj5.As();

			if (target == "")
				throw new ValueError("Shortcut target cannot be an empty string.");

#if LINUX
			var type = obj6.As();
			target = Path.GetFullPath(target);

			if (workingDir != "" || args != "" || description != "" || icon != "")
			{
				var creator = new ShortcutCreator();
				icon = Path.GetFullPath(icon);
				creator.Add("Icon", icon);
				creator.Add("Path", workingDir);
				creator.Add("Comment", description);

				if (args != "")
					creator.Add("Exec", target + " " + args);
				else
					creator.Add("Exec", target);

				if (type != "")
				{
					if (long.TryParse(type, out var tp))
					{
						if (tp == 0)//Wasn't a number, assume text and use verbatim.
							creator.Add("Type", type);
						else if (tp == 1)//Was a number, test for 1 2 or 3.
							creator.Add("Type", "Application");
						else if (tp == 2)
							creator.Add("Type", "Link");
						else if (tp == 3)
							creator.Add("Type", "Directory");
						else//Was something else, so just assume it's of type Application.
							creator.Add("Type", "Application");
					}
					else
						creator.Add("Type", type);
				}
				else//Was not specified, so just assume it's of type Application.
					creator.Add("Type", "Application");

				creator.Save(link);
				$"chmod +x '{link}'".Bash();
			}
			else
				$"ln -sf '{target}' '{link}'".Bash();

#elif WINDOWS
			var shortcutKey = obj6.As();
			var iconNumber = obj7.Al(0);
			var runState = obj8.Al(1);
			var shell = new IWshRuntimeLibrary.WshShell();
			var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(Path.GetFullPath(link));
			shortcut.TargetPath = Path.GetFullPath(target);

			if (workingDir != "")
				shortcut.WorkingDirectory = Path.GetFullPath(workingDir);

			shortcut.Arguments = args;
			shortcut.Description = description;

			if (icon != "")
				shortcut.IconLocation = $"{Path.GetFullPath(icon)}, {iconNumber}";

			var mods = "";
			var i = 0;

			for (; i < shortcutKey.Length; i++)
			{
				char ch = shortcutKey[i];

				if (ch == '^')
					mods += "Ctrl+";
				else if (ch == '!')
					mods += "Alt+";
				else if (ch == '+')
					mods += "Shift+";
				else
					break;
			}

			if (shortcutKey.Length > 0)
			{
				if (mods.Length == 0)// For backwards compatibility: if modifiers omitted, assume CTRL+ALT.
					shortcut.Hotkey = $"Ctrl+Alt+{shortcutKey}";
				else
					shortcut.Hotkey = $"{mods}{shortcutKey.Substring(i)}";
			}
			else
				shortcut.Hotkey = "";

			shortcut.WindowStyle = (int)runState;
			shortcut.Save();
#endif
		}

		/// <summary>
		/// Deletes one or more files.
		/// </summary>
		/// <param name="pattern">The name of a file or a wildcard pattern.</param>
		public static void FileDelete(object obj)
		{
			var s = obj.As();
			var path = Path.GetDirectoryName(s);
			var dir = new DirectoryInfo(path?.Length == 0 ? "./" : path);
			var filename = Path.GetFileName(s);
			var failures = 0;

			foreach (var file in dir.EnumerateFiles(filename))
			{
				try
				{
					file.Delete();
				}
				catch
				{
					failures++;
				}
			}

			if (failures > 0)
				throw new Error($"Failed {failures} times moving or copying files.", "", failures);
		}

		public static string FileDirName(object obj) => Path.GetDirectoryName(Path.GetFullPath(obj.As()));

		public static void FileEncoding(object obj)
		{
			var s = obj.As();

			if (s != "")
				Accessors.A_FileEncoding = s;
		}

		public static string FileExist(object obj)
		{
			var s = obj.As();

			try
			{
				var path = Path.GetDirectoryName(s);
				var dir = new DirectoryInfo(path);
				var filename = Path.GetFileName(s);

				foreach (var file in dir.EnumerateFiles(filename))
					return Conversions.FromFileAttribs(System.IO.File.GetAttributes(file.FullName));
			}
			catch
			{
			}

			return string.Empty;
		}

		public static string FileFullPath(object obj) => Path.GetFullPath(obj.As());

		public static string FileGetAttrib(object obj)
		{
			var s = obj.As();

			try
			{
				if (Directory.Exists(s) || System.IO.File.Exists(s))
					return Conversions.FromFileAttribs(System.IO.File.GetAttributes(s));
				else if (s?.Length == 0)
					return Conversions.FromFileAttribs(System.IO.File.GetAttributes(Accessors.A_LoopFileFullPath));
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}

			return string.Empty;
		}

		public static void FileGetShortcut(object obj)
		{
			object outTarget = null;
			object outDir = null;
			object outArgs = null;
			object outDescription = null;
			object outIcon = null;
			object outIconNum = null;
			object outRunState = null;
			FileGetShortcut(obj,
							ref outTarget,
							ref outDir,
							ref outArgs,
							ref outDescription,
							ref outIcon,
							ref outIconNum,
							ref outRunState);
		}

		public static void FileGetShortcut(object obj,
										   ref object outTarget)
		{
			object outDir = null;
			object outArgs = null;
			object outDescription = null;
			object outIcon = null;
			object outIconNum = null;
			object outRunState = null;
			FileGetShortcut(obj,
							ref outTarget,
							ref outDir,
							ref outArgs,
							ref outDescription,
							ref outIcon,
							ref outIconNum,
							ref outRunState);
		}

		public static void FileGetShortcut(object obj,
										   ref object outTarget,
										   ref object outDir)
		{
			object outArgs = null;
			object outDescription = null;
			object outIcon = null;
			object outIconNum = null;
			object outRunState = null;
			FileGetShortcut(obj,
							ref outTarget,
							ref outDir,
							ref outArgs,
							ref outDescription,
							ref outIcon,
							ref outIconNum,
							ref outRunState);
		}

		public static void FileGetShortcut(object obj,
										   ref object outTarget,
										   ref object outDir,
										   ref object outArgs)
		{
			object outDescription = null;
			object outIcon = null;
			object outIconNum = null;
			object outRunState = null;
			FileGetShortcut(obj,
							ref outTarget,
							ref outDir,
							ref outArgs,
							ref outDescription,
							ref outIcon,
							ref outIconNum,
							ref outRunState);
		}

		public static void FileGetShortcut(object obj,
										   ref object outTarget,
										   ref object outDir,
										   ref object outArgs,
										   ref object outDescription)
		{
			object outIcon = null;
			object outIconNum = null;
			object outRunState = null;
			FileGetShortcut(obj,
							ref outTarget,
							ref outDir,
							ref outArgs,
							ref outDescription,
							ref outIcon,
							ref outIconNum,
							ref outRunState);
		}

		public static void FileGetShortcut(object obj,
										   ref object outTarget,
										   ref object outDir,
										   ref object outArgs,
										   ref object outDescription,
										   ref object outIcon)
		{
			object outIconNum = null;
			object outRunState = null;
			FileGetShortcut(obj,
							ref outTarget,
							ref outDir,
							ref outArgs,
							ref outDescription,
							ref outIcon,
							ref outIconNum,
							ref outRunState);
		}

		public static void FileGetShortcut(object obj,
										   ref object outTarget,
										   ref object outDir,
										   ref object outArgs,
										   ref object outDescription,
										   ref object outIcon,
										   ref object outIconNum)
		{
			object outRunState = null;
			FileGetShortcut(obj,
							ref outTarget,
							ref outDir,
							ref outArgs,
							ref outDescription,
							ref outIcon,
							ref outIconNum,
							ref outRunState);
		}

		/// <summary>
		/// Retrieves information about a shortcut file.
		/// </summary>
		/// <param name="link"></param>
		/// <param name="target"></param>
		/// <param name="workingDir"></param>
		/// <param name="args"></param>
		/// <param name="description"></param>
		/// <param name="icon"></param>
		/// <param name="iconNumber"></param>
		/// <param name="runState"></param>
		public static void FileGetShortcut(object obj,
										   ref object outTarget,
										   ref object outDir,
										   ref object outArgs,
										   ref object outDescription,
										   ref object outIcon,
										   ref object outIconNum,
										   ref object outRunState)
		{
			var link = Path.GetFullPath(obj.As());
#if LINUX
			var dest = $"readlink -f '{link}'".Bash();

			if (link == dest)//Was not just a simple symlink.
			{
				var sc = new ShortcutCreator(link);
				outTarget = sc.Get("Exec");
				outDir = sc.Get("Path");
				outDescription = sc.Get("Comment");
				outIcon = sc.Get("Icon");
				outIconNum = sc.Get("Type");

				if (outTarget is string s && s.Length > 0)
				{
					if (s[0] != '"' && s[0] != '\'')
					{
						var splits = s.Split(Keywords.SpaceTab, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

						if (splits.Length > 1)
							outArgs = splits[1];
						else
							outArgs = "";
					}
					else//It was quoted.
					{
						var firstArgIndex = s.FindFirstNotInQuotes(" ");
						var tempArgs = firstArgIndex != -1 && firstArgIndex < s.Length - 1 ? s.Substring(firstArgIndex + 1) : "";
						outArgs = tempArgs.Trim();
					}
				}
				else
					outArgs = "";//No way to determine args.
			}
			else
			{
				outTarget = dest;
				outDir = Path.GetDirectoryName(dest);
				outArgs = "";
				outDescription = "";
				outIcon = "";
				outIconNum = "";
			}

			outRunState = "";
#elif WINDOWS

			try
			{
				var shell = new IWshRuntimeLibrary.WshShell();
				var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(Path.GetFullPath(link));
				var commaindex = shortcut.IconLocation.LastIndexOf(',');
				var iconno = "0";
				var iconstr = shortcut.IconLocation;

				if (commaindex != -1)
				{
					iconstr = iconstr.Substring(0, commaindex);
					iconno = shortcut.IconLocation.Substring(commaindex + 1).Trim();
				}

				outTarget = shortcut.TargetPath;
				outDir = shortcut.WorkingDirectory;
				outArgs = shortcut.Arguments;
				outDescription = shortcut.Description;
				outIcon = iconstr;
				outIconNum = iconno;//How to get this?
				outRunState = shortcut.WindowStyle;//How to get this?
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}

#endif
		}

		/// <summary>
		/// Retrieves the size of a file.
		/// </summary>
		/// <param name="result">The name of the variable in which to store the retrieved size.</param>
		/// <param name="file">The name of the target file.</param>
		/// <param name="units">
		/// <para>If present, this parameter causes the result to be returned in units other than bytes:</para>
		/// <list type="bullet">
		/// <item><term>K</term>: <description>kilobytes</description></item>
		/// <item><term>M</term>: <description>megabytes</description></item>
		/// <item><term>G</term>: <description>gigabytes</description></item>
		/// </list>
		/// </param>
		public static long FileGetSize(object obj0 = null, object obj1 = null)
		{
			long result;
			var file = obj0.As();
			var units = obj1.As();

			if (file?.Length == 0)
				file = Accessors.A_LoopFileFullPath;

			try
			{
				var scale = 1024L;
				var size = (new FileInfo(file)).Length;

				if (!string.IsNullOrEmpty(units))
				{
					switch (units[0])
					{
						case 'k':
						case 'K':
							size /= scale;
							break;

						case 'm':
						case 'M':
							size /= scale * scale;
							break;

						case 'g':
						case 'G':
							size /= scale * scale * scale;
							break;

						case 't':
						case 'T':
							size /= scale * scale * scale * scale;
							break;
					}
				}

				result = size;
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}

			return result;
		}

		/// <summary>
		/// Retrieves the datetime stamp of a file or folder.
		/// </summary>
		/// <param name="result">The name of the variable in which to store the retrieved date-time in format YYYYMMDDHH24MISS in local time.</param>
		/// <param name="file">The name of the target file or folder.</param>
		/// <param name="time">
		/// <para>Which timestamp to retrieve:</para>
		/// <list type="bullet">
		/// <item><term>M</term>: <description>(default) modification time</description></item>
		/// <item><term>C</term>: <description>reation time</description></item>
		/// <item><term>A</term>: <description>last access time</description></item>
		/// </list>
		/// </param>
		public static string FileGetTime(object obj0 = null, object obj1 = null)
		{
			var file = obj0.As();
			var time = obj1.As("M");

			if (file?.Length == 0)
				file = Accessors.A_LoopFileFullPath;

			if (!System.IO.File.Exists(file))
				return "";

			var date = new DateTime();
			var info = new FileInfo(file);

			switch (time[0])
			{
				case 'c':
				case 'C':
					date = info.CreationTime;
					break;

				case 'a':
				case 'A':
					date = info.LastAccessTime;
					break;

				case 'm':
				case 'M':
				default:
					date = info.LastWriteTime;
					break;
			}

			return Conversions.ToYYYYMMDDHH24MISS(date);
		}

		/// <summary>
		/// Retrieves the version information of a file.
		/// </summary>
		/// <param name="obj">The name of the target file.</param>
		/// <returns>This function returns the version number of the specified file.</returns>
		public static string FileGetVersion(object obj)
		{
			var file = obj.As();
			var result = "";

			if (file?.Length == 0)
				file = Accessors.A_LoopFileFullPath;

			try
			{
				var info = FileVersionInfo.GetVersionInfo(file);
				result = info.FileVersion;
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}

			return result;
		}

		public static string FileInstall(object obj0, object obj1, object obj2 = null) => throw new Error("Compiling files into an executable is not supported in Keysharp");

		/// <summary>
		/// Moves or renames one or more files.
		/// </summary>
		/// <param name="source">The name of a single file or a wildcard pattern.</param>
		/// <param name="destination">The name or pattern of the destination.</param>
		/// <param name="flag">
		/// <list type="bullet">
		/// <item><term>0</term>: <description>(default) do not overwrite existing files</description></item>
		/// <item><term>1</term>: <description>overwrite existing files</description></item>
		/// </list>
		/// </param>
		public static void FileMove(object obj0, object obj1, object obj2 = null)
		{
			var source = obj0.As();
			var dest = obj1.As();
			var flag = obj2.Ab();
			FileCopyMove(source, dest, flag, true);
		}

		public static KeysharpFile FileOpen(object obj0, object obj1, object obj2 = null)
		{
			var filename = obj0.As();
			var flags = obj1.As();
			var enc = obj2.As();
			var encoding = ThreadAccessors.A_FileEncodingRaw;
			var ienc = enc.ParseInt(false);

			if (ienc.HasValue)
				encoding = System.Text.Encoding.GetEncoding(ienc.Value);
			else if (enc != "")
				encoding = GetEncoding(enc);

			var mode = FileMode.Open;
			var access = FileAccess.ReadWrite;
			var share = FileShare.ReadWrite | FileShare.Delete;
			var shareset = false;
			var eolconv = 0;

			foreach (Range r in flags.AsSpan().SplitAny(Keywords.Spaces))
			{
				var i = 0;
				var flag = flags.AsSpan(r).Trim();

				if (flag.Length > 0)
				{
					if (flag.Equals("r", StringComparison.OrdinalIgnoreCase))
					{
						mode = FileMode.Open;
						access = FileAccess.Read;
					}
					else if (flag.Equals("w", StringComparison.OrdinalIgnoreCase) || (int.TryParse(flag, out i) && i == 1))
					{
						mode = FileMode.Create;
						access = FileAccess.ReadWrite;
					}
					else if (flag.Equals("a", StringComparison.OrdinalIgnoreCase) || (int.TryParse(flag, out i) && i == 2))
					{
						mode = FileMode.Append;
						access = FileAccess.Write;
					}
					else if (flag.Equals("rw", StringComparison.OrdinalIgnoreCase) || (int.TryParse(flag, out i) && i == 3))
					{
						mode = FileMode.OpenOrCreate;
						access = FileAccess.ReadWrite;
					}
					else if (flag.Equals("h", StringComparison.OrdinalIgnoreCase))
					{
						filename = "h*" + filename;
					}
					else if (flag.Equals("\n", StringComparison.OrdinalIgnoreCase) || (int.TryParse(flag, out i) && i == 4))
					{
						eolconv = 4;
					}
					else if (flag.Equals("\r", StringComparison.OrdinalIgnoreCase) || (int.TryParse(flag, out i) && i == 8))
					{
						eolconv = 8;
					}
					else if (flag.Equals("-", StringComparison.OrdinalIgnoreCase))
					{
						share = FileShare.None;
						shareset = true;
					}
					else if (flag.StartsWith('-'))
					{
						if (flag.Contains('r'))
						{
							share &= ~FileShare.Read;
							shareset = true;
						}

						if (flag.Contains('w'))
						{
							share &= ~FileShare.Write;
							shareset = true;
						}

						if (flag.Contains('d'))
						{
							share &= ~FileShare.Delete;
							shareset = true;
						}
					}
					else
					{
						var b = int.TryParse(flag, out i);
						share = FileShare.None;

						if (!b)
						{
							shareset = true;
						}
						else
						{
							if ((i & 0x100) == 0x100)
							{
								share |= FileShare.Read;
								shareset = true;
							}

							if ((i & 0x200) == 0x200)
							{
								share |= FileShare.Write;
								shareset = true;
							}

							if ((i & 0x400) == 0x400)
							{
								share |= FileShare.Delete;
								shareset = true;
							}
						}
					}
				}
			}

			if (!shareset)
				share = FileShare.ReadWrite | FileShare.Delete;

			try
			{
				return new KeysharpFile(filename, mode, access, share, encoding, eolconv);
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}
		}

		/// <summary>
		/// Read the contents of a file.
		/// </summary>
		/// <param name="OutputVar">The name of the variable in which to store the retrieved content.</param>
		/// <param name="filename">
		/// <para>The file path, optionally preceded by one or more of the following options:</para>
		/// <list type="bullet">
		/// <item><term>*c</term>: <description>treat the source as binary rather than text, <paramref name="OutputVar"/> will be a byte array.</description></item>
		/// <item><term>*m<c>n</c></term>: <description>stop reading at <c>n</c> bytes.</description></item>
		/// <item><term>*t</term>: <description>replace all occurrences of <c>`r`n</c> with <c>`n</c>. This option is ignored in binary mode.</description></item>
		/// </list>
		/// </param>
		public static object FileRead(object obj0, object obj1 = null)
		{
			object output = null;
			var filename = obj0.As();
			var options = obj1.As();
			var enc = ThreadAccessors.A_FileEncodingRaw;

			if (string.IsNullOrEmpty(filename))
				return "";

			var max = -1;
			bool binary = false, nocrlf = false;

			foreach (Range r in options.AsSpan().SplitAny(Keywords.SpaceTabSv))
			{
				var split = options.AsSpan(r).Trim(Keywords.SpaceTab);//Need to supply chars to trim else \n would get automatically trimmed.

				if (split.Length > 0)
				{
					if (Options.TryParse(split, "m", ref max))
					{
					}
					else
					{
						if (split.Equals("raw", StringComparison.OrdinalIgnoreCase))
							binary = true;
						else if (split[0] == '\n')
							nocrlf = true;
						else
							enc = GetEncoding(split.ToString());//Will internally convert to string.
					}
				}
			}

			if (max == 0)
				return "";

			if (binary)
			{
				try
				{
					var temparr = max == -1 ? System.IO.File.ReadAllBytes(filename) : new BinaryReader(System.IO.File.OpenRead(filename)).ReadBytes(max);
					output = new Buffer(temparr);
				}
				catch (Exception ex)
				{
					throw new OSError(ex);
				}
			}
			else
			{
				string text;

				try
				{
					if (max != -1)
					{
						using (var fs = new FileStream(filename, FileMode.Open))
						{
							using (var br = new BinaryReader(fs))
							{
								var buf = br.ReadBytes(max);
								text = enc.GetString(buf);
							}
						}
					}
					else
						text = System.IO.File.ReadAllText(filename);
				}
				catch (Exception ex)
				{
					throw new OSError(ex);
				}

				if (max != -1)
					text = text.Substring(0, max);

				if (nocrlf)
					text = text.Replace("\r\n", "\n");

				output = text;
			}

			return output;
		}

		/// <summary>
		/// Sends a file or directory to the recycle bin, if possible.
		/// </summary>
		/// <param name="FilePattern">
		/// <para>The name of a single file or a wildcard pattern such as C:\Temp\*.tmp. FilePattern is assumed to be in %A_WorkingDir% if an absolute path isn't specified.</para>
		/// <para>To recycle an entire directory, provide its name without a trailing backslash.</para>
		/// </param>
		public static void FileRecycle(object obj)
		{
			try
			{
				var s = obj.As();
				var path = Path.GetDirectoryName(s);
				var dir = new DirectoryInfo(path);
				var filename = Path.GetFileName(s);
#if LINUX
				$"gio trash {s}".Bash();
#elif WINDOWS

				foreach (var file in dir.EnumerateFiles(filename))
					//This appears to be not implemented in mono:
					//https://github.com/mono/mono-basic/blob/master/vbruntime/Microsoft.VisualBasic/Microsoft.VisualBasic.FileIO/FileSystemOperation.vb
					//May need some type of system call for non-windows OS.
					Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

#endif
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}
		}

		/// <summary>
		/// Empties the recycle bin.
		/// </summary>
		/// <param name="Root">If omitted, the recycle bin for all drives is emptied. Otherwise, specify a drive letter such as C:\</param>
		public static void FileRecycleEmpty(object obj = null)
		{
			var s = obj.As();

			try
			{
#if LINUX
				"gio trash --empty".Bash();
#elif WINDOWS
				_ = WindowsAPI.SHEmptyRecycleBin(IntPtr.Zero, s != "" ? s : null, WindowsAPI.SHERB_NOCONFIRMATION | WindowsAPI.SHERB_NOPROGRESSUI | WindowsAPI.SHERB_NOSOUND);
#endif
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}
		}
		/// <summary>
		/// Changes the attributes of one or more files or folders. Wildcards are supported.
		/// </summary>
		/// <param name="attributes">The attributes to change (see Remarks).</param>
		/// <param name="filePattern">
		/// <para>The name of a single file or folder, or a wildcard pattern such as C:\Temp\*.tmp. FilePattern is assumed to be in %A_WorkingDir% if an absolute path isn't specified.</para>
		/// <para>If omitted, the current file of the innermost enclosing File-Loop will be used instead.</para>
		/// <param name="mode">Zero or more of the following letters:
		/// <list type="">
		/// <item>d: Include directories (folders).</item>
		/// <item>f: Include files. If both F and D are omitted, files are included but not folders.</item>
		/// <item>r: Subfolders are recursed into so that files and folders contained therein are operated upon if they match FilePattern. All subfolders will be recursed into, not just those whose names match FilePattern. If R is omitted, files and folders in subfolders are not included.</item>
		/// </list>
		/// </param>
		public static void FileSetAttrib(object obj0, object obj1 = null, object obj2 = null)
		{
			var attributes = obj0.As();
			var file = obj1.As(Accessors.A_LoopFileFullPath);
			var mode = obj2.As();
			var dodirs = mode.Contains('d', StringComparison.OrdinalIgnoreCase);
			var dofiles = mode.Contains('f', StringComparison.OrdinalIgnoreCase);
			var recurse = mode.Contains('r', StringComparison.OrdinalIgnoreCase);
			var failures = 0;

			if (!dodirs && !dofiles)
				dofiles = true;

			foreach (var path in Conversions.ToFiles(file, dofiles, dodirs, recurse))
			{
				try
				{
					var set = Conversions.ToFileAttribs(attributes, System.IO.File.GetAttributes(path));
					System.IO.File.SetAttributes(path, set);

					if (set == FileAttributes.None)
						set = FileAttributes.Normal;
					else if (set.HasFlag(FileAttributes.Normal) && set != FileAttributes.Normal)//It was Normal and something else.
							set &= ~FileAttributes.Normal;//Remove normal because it can only be used alone. Just use the other bits.

					if (System.IO.File.GetAttributes(path) != set)
						failures++;
				}
				catch (Exception)
				{
					failures++;
				}
			}

			if (failures != 0)
				throw new Error($"Failed {failures} times setting file attributes.", "", failures);
		}
		/// <summary>
		/// Changes the datetime stamp of one or more files or folders. Wildcards are supported.
		/// </summary>
		/// <param name="YYYYMMDDHH24MISS">If blank or omitted, it defaults to the current time. Otherwise, specify the time to use for the operation (see Remarks for the format). Years prior to 1601 are not supported.</param>
		/// <param name="FilePattern">
		/// <para>The name of a single file or folder, or a wildcard pattern such as C:\Temp\*.tmp. FilePattern is assumed to be in %A_WorkingDir% if an absolute path isn't specified.</para>
		/// <para>If omitted, the current file of the innermost enclosing File-Loop will be used instead.</para>
		/// </param>
		/// <param name="WhichTime">Which timestamp to set:
		/// <list type="">
		/// <item>M = Modification time (this is the default if the parameter is blank or omitted)</item>
		/// <item>C = Creation time</item>
		/// <item>A = Last access time </item>
		/// </list>
		/// </param>
		/// <param name="OperateOnFolders">
		/// <list type="">
		/// <item>0 (default) Folders are not operated upon (only files).</item>
		/// <item>1 All files and folders that match the wildcard pattern are operated upon.</item>
		/// <item>2 Only folders are operated upon (no files).</item>
		/// <para>Note: If FilePattern is a single folder rather than a wildcard pattern, it will always be operated upon regardless of this setting.</para>
		/// </list>
		/// </param>
		/// <param name="Recurse">
		/// <list type="">
		/// <item>0 (default) Subfolders are not recursed into.</item>
		/// <item>1 Subfolders are recursed into so that files and folders contained therein are operated upon if they match FilePattern. All subfolders will be recursed into, not just those whose names match FilePattern. However, files and folders with a complete path name longer than 259 characters are skipped over as though they do not exist. Such files are rare because normally, the operating system does not allow their creation.</item>
		/// </list>
		/// </param>
		public static void FileSetTime(object obj0 = null, object obj1 = null, object obj2 = null, object obj3 = null)
		{
			var YYYYMMDDHH24MISS = obj0.As();
			var file = obj1.As();
			var whichtime = obj2.As("M");
			var mode = obj3.As();
			var dodirs = mode.Contains('d', StringComparison.OrdinalIgnoreCase);
			var dofiles = mode.Contains('f', StringComparison.OrdinalIgnoreCase);
			var recurse = mode.Contains('r', StringComparison.OrdinalIgnoreCase);
			var failures = 0;

			if (!dodirs && !dofiles)
				dofiles = true;

			if (file?.Length == 0)
				file = Accessors.A_LoopFileFullPath;

			var time = Conversions.ToDateTime(YYYYMMDDHH24MISS);

			foreach (var path in Conversions.ToFiles(file, dofiles, dodirs, recurse))
			{
				try
				{
					var set = new DateTime();

					switch (whichtime[0])
					{
						case 'm':
						case 'M':
							System.IO.File.SetLastWriteTime(path, time);
							set = System.IO.File.GetLastWriteTime(path);
							break;

						case 'c':
						case 'C':
							System.IO.File.SetCreationTime(path, time);
							set = System.IO.File.GetCreationTime(path);
							break;

						case 'a':
						case 'A':
							System.IO.File.SetLastAccessTime(path, time);
							set = System.IO.File.GetLastAccessTime(path);
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}

					if (set != time)
							failures++;
				}
				catch { failures++; }
			}

			if (failures != 0)
				throw new Error($"Failed {failures} times setting file time.", "", failures);
		}
		internal static Encoding GetEncoding(object s)
		{
			var val = s.ToString().ToLowerInvariant();
			Encoding tempenc;

			if (val.StartsWith("cp"))
				return System.Text.Encoding.GetEncoding(val.Substring(2).ParseInt().Value);

			if (int.TryParse(val, out var cp))
				return System.Text.Encoding.GetEncoding(cp);

			switch (val)
			{
				case "ascii":
				case "us-ascii":
					return System.Text.Encoding.ASCII;

				case "utf-8":
					return System.Text.Encoding.UTF8;

				case "utf-8-raw":
					return new UTF8Encoding(false);//No byte order mark.

				case "utf-16":
				case "unicode":
					return System.Text.Encoding.Unicode;

				case "utf-16-raw":
					return new UnicodeEncoding(false, false);//Little endian, no byte order mark.
			}

			try
			{
				tempenc = System.Text.Encoding.GetEncoding(val);
				return tempenc;
			}
			catch
			{
			}

			return System.Text.Encoding.Unicode;
		}
		/// <summary>
		/// Expand wildcards in a filename and extension.
		/// </summary>
		/// <param name="source">The source filename, with wildcards</param>
		/// <param name="dest">The destination filename, with wildcards to be expanded.</param>
		/// <returns>The expanded destination string if any wildcards were present, else the destination string as it was passed in.</returns>
		private static string ExpandFilenameWildcard(string source, string dest)
		{
			var srcfile = Path.GetFileNameWithoutExtension(source);
			var srcext = Path.GetExtension(source);
			var destfile = Path.GetFileNameWithoutExtension(dest);
			var destex = Path.GetExtension(dest);

			if (destex?.Length == 0)
				destex = srcext.Contains('.') ? ".*" : "*";

			srcext = srcext.TrimStart('.');
			return destfile.ReplaceFirst("*", srcfile) + (destex.Contains('*') ? destex.ReplaceFirst("*", srcext) : destex);
		}
		private static void FileCopyMove(string source, string dest, bool flag, bool move)
		{
			var failures = 0;
			var dfull = Path.GetFullPath(dest).TrimEnd(Path.DirectorySeparatorChar);
			var sfull = Path.GetFullPath(source).TrimEnd(Path.DirectorySeparatorChar);

			if (Directory.Exists(sfull))
				sfull += $"{Path.DirectorySeparatorChar}*";

			if (Directory.Exists(dfull))
				dfull += $"{Path.DirectorySeparatorChar}*";

			var sfname = Path.GetFileName(sfull);
			var dfname = Path.GetFileName(dfull);
			var sdname = Path.GetDirectoryName(sfull);
			var ddname = Path.GetDirectoryName(dfull);
			var files = Directory.GetFiles(sdname, sfname, System.IO.SearchOption.TopDirectoryOnly);

			if (files.Length == 0 && sfull.IndexOfAny("*?".ToCharArray()) == -1)
				throw new Error($"{sfull} did not contain any files.");

			if (!Directory.Exists(ddname))
				throw new Error($"Folder {ddname} did not exist.");

			foreach (var f in files)
			{
				try
				{
					var name = Path.GetFileName(f);
					var dname = dfname.Contains('*') ? ExpandFilenameWildcard(name, dfname) : dfname;
					var s = Path.Combine(sdname, name);
					var d = Path.Combine(ddname, dname);

					if (Directory.Exists(s))//Ensure it's not a folder (again) just to be safe. AHK did this.
						continue;

					if (move)
						System.IO.File.Move(s, d, flag);
					else
						System.IO.File.Copy(s, d, flag);
				}
				catch { failures++; }
			}

			if (failures > 0)
				throw new Error($"Failed {failures} times moving or copying files.", "", failures);
		}
	}

#if LINUX
	internal class ShortcutCreator
	{
		private Dictionary<string, string> fields = new Dictionary<string, string>()
		{
			{ "Type", "Application" },
			{ "Name", "" },
			{ "Comment", "" },
			{ "Icon", "" },
			{ "Exec", "" },
			{ "Path", "" },
		};

		internal ShortcutCreator()
		{
		}

		internal ShortcutCreator(string path)
		{
			var lines = File.ReadAllLines(path);

			if (lines.Length > 0)
			{
				if (lines[0].StartsWith("[Desktop Entry]", StringComparison.OrdinalIgnoreCase))
				{
					for (var i = 1; i < lines.Length; i++)
					{
						var splits = lines[i].Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

						if (splits.Length > 1)
						{
							Add(splits[0], splits[1]);
						}
					}
				}
			}
		}

		internal void Add(string name, string value)
		{
			if (fields.ContainsKey(name))
				fields[name] = value;
		}

		internal string Get(string name) => fields.TryGetValue(name, out var val) ? val : "";

		internal void Save(string path) => File.WriteAllText(path, Write());

		internal string Write()
		{
			return "[Desktop Entry]" + Environment.NewLine + string.Join(Environment.NewLine, fields.Where(kv => kv.Value != "").Select(kv => $"{kv.Key}={kv.Value}"));
		}
	}
#endif
}