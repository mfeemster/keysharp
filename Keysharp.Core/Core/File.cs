using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//TODO
using Microsoft.VisualBasic.FileIO;

namespace Keysharp.Core
{
	public class KeysharpFile : IDisposable
	{
		internal Encoding enc;

		internal int eolconv = 0;

		private BinaryReader br;

		private BinaryWriter bw;

		private bool disposed = false;

		private FileStream fs;

		private TextReader tr;

		private TextWriter tw;

		public long AtEOF
		{
			get
			{
				if (br != null)
					return br.PeekChar() == -1 ? 1L : 0L;
				else if (tr != null)
					return tr.Peek() == -1 ? 1L : 0L;
				else
					return 0L;
			}
		}

		public string Encoding
		{
			get => enc.BodyName;
			set => enc = GetEncoding(value);
		}

		public long Handle => fs != null ? fs.SafeFileHandle.DangerousGetHandle().ToInt64() : 0;

		public long Length
		{
			get => fs != null ? fs.Length : 0L;
			set => fs?.SetLength(value);
		}

		public long Pos
		{
			get
			{
				if (br != null)
					return br.BaseStream.Position;
				else if (bw != null)
					return bw.BaseStream.Position;
				else
					return 0L;
			}

			set => Seek(value);
		}

		internal KeysharpFile(string filename, FileMode mode, FileAccess access, FileShare share, System.Text.Encoding encoding, long eol)
		{
			var m = mode;
			var a = access;
			var s = share;
			enc = encoding;
			eolconv = (int)eol;

			if (filename == "*")
			{
				if ((a & FileAccess.Read) == FileAccess.Read)
					tr = Console.In;

				if ((a & FileAccess.Write) == FileAccess.Write)
					tw = Console.Out;
			}
			else if (filename == "**")
			{
				if ((a & FileAccess.Read) == FileAccess.Read)
					tr = Console.In;

				if ((a & FileAccess.Write) == FileAccess.Write)
					tw = Console.Error;
			}
			else
			{
				var exists = false;

				if (filename.StartsWith("h*"))
				{
					var handle = filename.Substring(2).ParseLong(false);

					if (handle.HasValue)
					{
						exists = true;
						fs = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(handle.Value), false), a, 4096);
					}
				}
				else
				{
					if (System.IO.File.Exists(filename))
						exists = true;

					fs = new FileStream(filename, m, a, s);
				}

				if ((a & FileAccess.Read) == FileAccess.Read)
					br = new BinaryReader(fs, enc);

				if ((a & FileAccess.Write) == FileAccess.Write)
					bw = new BinaryWriter(fs, enc);

				if (!exists && bw != null)
				{
					if (enc is UTF8Encoding u8)
					{
						if (u8.Preamble.Length > 0)
							bw.Write(u8.Preamble);
					}
					else if (enc is UnicodeEncoding u16)
					{
						if (u16.Preamble.Length > 0)
							bw.Write(u16.Preamble);
					}
				}
				else if (exists && br != null)
				{
					if (enc is UTF8Encoding u8)
					{
						if (u8.Preamble.Length > 0)
							_ = br.BaseStream.Seek(u8.Preamble.Length, SeekOrigin.Begin);
					}
					else if (enc is UnicodeEncoding u16)
					{
						if (u16.Preamble.Length > 0)
							_ = br.BaseStream.Seek(u16.Preamble.Length, SeekOrigin.Begin);
					}
				}
			}
		}

		~KeysharpFile() => Dispose(false);

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
				var enc = KeysharpFile.GetEncoding(Accessors.A_FileEncoding);
				var raw = false;
				var crlf = false;
				TextWriter tw = null;

				if (obj2.As() is string options)
				{
					var splits = options.Split(' ');

					foreach (var split in splits)
					{
						var val = split.ToString().ToLowerInvariant();

						switch (val)
						{
							case "ascii":
							case "us-ascii":
								enc = System.Text.Encoding.ASCII;
								break;

							case "utf-8":
								enc = System.Text.Encoding.UTF8;
								break;

							case "utf-8-raw":
								enc = new UTF8Encoding(false);//Not byte order mark.
								break;

							case "utf-16":
							case "unicode":
								enc = System.Text.Encoding.Unicode;
								break;

							case "utf-16-raw":
								enc = new UnicodeEncoding(false, false);//Little endian, no byte order mark.
								break;

							case "raw":
								raw = true;
								break;

							case "`n":
								crlf = true;
								break;
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
									info.sw = new StreamWriter(info.filename, true, enc);
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
					tw = new StreamWriter(filename, true, enc);

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
				shortcut.IconLocation = $"{(Path.GetFullPath(icon))}, {iconNumber}";

			shortcut.Hotkey = shortcutKey != "" ? $"Ctrl+Alt+{shortcutKey}" : "";
			shortcut.WindowStyle = (int)runState;
			shortcut.Save();
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
		public static ShortcutOutput FileGetShortcut(object obj)
		{
			var link = obj.As();

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

				return new ShortcutOutput()
				{
					OutTarget = shortcut.TargetPath,
					OutDir = shortcut.WorkingDirectory,
					OutArgs = shortcut.Arguments,
					OutDescription = shortcut.Description,
					OutIcon = iconstr,
					OutIconNum = iconno,//How to get this?
					OutRunState = shortcut.WindowStyle//How to get this?
				};
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}
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
			var encoding = Accessors.FileEncoding;
			var ienc = enc.ParseInt(false);

			if (ienc.HasValue)
				encoding = System.Text.Encoding.GetEncoding(ienc.Value);
			else if (enc != "")
				encoding = KeysharpFile.GetEncoding(enc);

			var mode = FileMode.Open;
			var access = FileAccess.ReadWrite;
			var share = FileShare.ReadWrite | FileShare.Delete;
			var shareset = false;
			var eolconv = 0;

			foreach (var flag in Options.ParseOptions(flags))
			{
				if (flag == "r")
				{
					mode = FileMode.Open;
					access = FileAccess.Read;
				}
				else if (flag == "w" || flag.ParseInt(false) == 1)
				{
					mode = FileMode.Create;
					access = FileAccess.ReadWrite;
				}
				else if (flag == "a" || flag.ParseInt(false) == 2)
				{
					mode = FileMode.Append;
					access = FileAccess.Write;
				}
				else if (flag == "rw" || flag.ParseInt(false) == 3)
				{
					mode = FileMode.OpenOrCreate;
					access = FileAccess.ReadWrite;
				}
				else if (flag == "h")
				{
					filename = "h*" + filename;
				}
				else if (flag == "\n" || flag.ParseInt(false) == 4)
				{
					eolconv = 4;
				}
				else if (flag == "\r" || flag.ParseInt(false) == 8)
				{
					eolconv = 8;
				}
				else if (flag == "-")
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
					var i = flag.ParseInt(false);
					share = FileShare.None;

					if (i == 0)
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
			var enc = Accessors.FileEncoding;

			if (string.IsNullOrEmpty(filename))
				return "";

			var max = -1;
			bool binary = false, nocrlf = false;
			var splits = options.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			foreach (var split in splits)
			{
				if (Options.TryParse(split, "m", ref max)) { }
				else
				{
					var lower = split.ToLowerInvariant();

					switch (lower)
					{
						case "raw":
							binary = true;
							break;

						case "\n":
							nocrlf = true;
							break;

						default:
							enc = KeysharpFile.GetEncoding(lower);
							break;
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
			var s = obj.As();
			var path = Path.GetDirectoryName(s);
			var dir = new DirectoryInfo(path);
			var filename = Path.GetFileName(s);

			foreach (var file in dir.EnumerateFiles(filename))
				//This appears to be not implemented in mono:
				//https://github.com/mono/mono-basic/blob/master/vbruntime/Microsoft.VisualBasic/Microsoft.VisualBasic.FileIO/FileSystemOperation.vb
				//May need some type of system call for non-windows OS.
				Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
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
				_ = WindowsAPI.SHEmptyRecycleBin(IntPtr.Zero, s != "" ? s : null, WindowsAPI.SHERB_NOCONFIRMATION | WindowsAPI.SHERB_NOPROGRESSUI | WindowsAPI.SHERB_NOSOUND);
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

		public static Encoding GetEncoding(object s)
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

		public void Close()
		{
			br?.Close();
			bw?.Close();
			tr?.Close();
			tw?.Close();
			fs?.Close();
		}

		public virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				Close();
				disposed = true;
			}
		}

		public void RawRead(object obj0, object obj1 = null)
		{
			var buf = obj0;
			var count = obj1.Al(long.MinValue);

			if (br != null)
			{
				byte[] val;

				if (buf is Array arr)
				{
					val = count != long.MinValue ? br.ReadBytes((int)count) : br.ReadBytes(arr.Count);
					var len = Math.Min(val.Length, arr.Count);

					for (var i = 0; i < len; i++)
						arr.array[i] = val[i];//Access the underlying ArrayList directly for performance.
				}
				else if (buf is Buffer buffer)
				{
					var size = (int)(long)buffer.Size;
					val = count != long.MinValue ? br.ReadBytes((int)count) : br.ReadBytes(size);
					var len = Math.Min(val.Length, size);
					unsafe
					{
						var ptr = (byte*)buffer.Ptr.ToPointer();

						for (var i = 0; i < len; i++)
							ptr[i] = val[i];
					}
				}
			}
		}

		public long RawWrite(object obj0, object obj1 = null)
		{
			var buf = obj0;
			var count = obj1.Al(long.MinValue);
			var len = 0;

			if (bw != null)
			{
				if (buf is Buffer buffer)
				{
					len = (int)(count != long.MinValue ? Math.Min((long)buffer.Size, count) : (long)buffer.Size);
					unsafe
					{
						var bytes = new byte[len];
						Marshal.Copy(buffer.Ptr, bytes, 0, len);
						bw.Write(bytes);
					}
				}
				else if (buf is Array arr)
				{
					len = count != long.MinValue ? Math.Min(arr.Count, (int)count) : arr.Count;
					bw.Write(arr.array.ConvertAll(el => (byte)el.ParseLong(false).Value).ToArray(), 0, len);//No way to know what is in the array since they are objects, so convert them to bytes.
				}
				else if (buf is string s)
				{
					var bytes = enc.GetBytes(s);
					len = count != long.MinValue ? Math.Min(bytes.Length, (int)count) : bytes.Length;
					bw.Write(bytes, 0, len);
				}
			}

			return len;
		}

		public string Read(object obj)
		{
			var s = "";
			var count = obj.Al();
			char[] buf = null;
			var read = 0;

			if (count > 0)
				buf = new char[count];

			if (br != null)
			{
				if (count > 0)
					read = br.Read(buf, 0, (int)count);
				else
					s = br.ReadString();
			}
			else if (tr != null)
			{
				if (count > 0)
					read = tr.Read(buf, 0, (int)count);
				else
					s = tr.ReadToEnd();
			}

			if (read > 0)
				s = new string(buf, 0, read);

			s = HandleReadEol(s);
			return s;
		}

		public object ReadChar() => br != null ? br.ReadByte() : "";

		public object ReadDouble() => br != null ? br.ReadDouble() : "";

		public object ReadFloat() => br != null ? br.ReadSingle() : "";

		public object ReadInt() => br != null ? br.ReadInt32() : "";

		public object ReadInt64() => br != null ? br.ReadInt64() : "";

		public string ReadLine()
		{
			var s = "";

			if (br != null)
				s = br.ReadLine();
			else if (tr != null)
				s = tr.ReadLine();

			return s;
		}

		public object ReadShort() => br != null ? br.ReadInt16() : "";

		//Char in this case is meant to be 1 byte, according to the AHK DllCall() documentation.
		public object ReadUChar() => br != null ? br.ReadByte() : "";

		public object ReadUInt() => br != null ? br.ReadUInt32() : "";

		public object ReadUShort() => br != null ? br.ReadUInt16() : "";

		public void Seek(object obj0, object obj1 = null)
		{
			var distance = obj0.Al();
			var origin = obj1.Al(long.MinValue);
			SeekOrigin so;

			if (origin == 0)
				so = SeekOrigin.Begin;
			else if (origin == 1)
				so = SeekOrigin.Current;
			else if (origin == 2)
				so = SeekOrigin.End;
			else if (distance < 0)
				so = SeekOrigin.End;
			else
				so = SeekOrigin.Begin;

			if (br != null)
				_ = br.BaseStream.Seek(distance, so);
			else if (bw != null)//Only need to do 1, because they both have the same underlying stream.
				_ = bw.Seek((int)distance, so);
		}

		public long Write(object obj)
		{
			var s = obj.As();
			var len = 0L;

			if (bw != null)
			{
				s = HandleWriteEol(s);
				var bytes = enc.GetBytes(s);
				bw.Write(bytes);
				len = bytes.Length;
			}
			else if (tw != null)
			{
				tw.Write(s);
				len = enc.GetByteCount(s);
			}

			return len;
		}

		public long WriteChar(object obj)
		{
			if (bw != null)
			{
				bw.Write((byte)obj.Al());//Char in this case is meant to be 1 byte, according to the AHK DllCall() documentation.
				return 1L;
			}
			else
				return 0L;
		}

		public long WriteDouble(object obj)
		{
			if (bw != null)
			{
				bw.Write(obj.Ad());
				return 8L;
			}
			else
				return 0L;
		}

		public long WriteFloat(object obj)
		{
			if (bw != null)
			{
				bw.Write((float)obj.Ad());
				return 4L;
			}
			else
				return 0L;
		}

		public long WriteInt(object obj)
		{
			if (bw != null)
			{
				bw.Write((int)obj.Al());
				return 4L;
			}
			else
				return 0L;
		}

		public long WriteInt64(object obj)
		{
			if (bw != null)
			{
				bw.Write(obj.Al());
				return 8L;
			}
			else
				return 0L;
		}

		public long WriteLine(object obj)
		{
			var s = obj.As();
			byte[] bytes;
			var len = 0L;

			if (s != "")
				len = Write(s);

			s = eolconv == 4 ? "\r\n" : "\n";

			if (bw != null)
			{
				bytes = enc.GetBytes(s);
				bw.Write(bytes);
				len += bytes.Length;
			}
			else if (tw != null)
			{
				tw.Write(s);
				len += enc.GetByteCount(s);
			}

			return len;
		}

		public long WriteShort(object obj)
		{
			if (bw != null)
			{
				bw.Write((short)obj.Al());
				return 2L;
			}
			else
				return 0L;
		}

		public long WriteUChar(object obj)
		{
			if (bw != null)
			{
				bw.Write((byte)obj.Al());
				return 1L;
			}
			else
				return 0L;
		}

		public long WriteUInt(object obj)
		{
			if (bw != null)
			{
				bw.Write((uint)obj.Al());
				return 4L;
			}
			else
				return 0L;
		}

		public long WriteUShort(object obj)
		{
			if (bw != null)
			{
				bw.Write((ushort)obj.Al());
				return 2L;
			}
			else
				return 0L;
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
			return destfile.ReplaceFirst("*", srcfile) + (destex.IndexOf("*") != -1 ? destex.ReplaceFirst("*", srcext) : destex);
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
					var dname = dfname.IndexOf('*') != -1 ? ExpandFilenameWildcard(name, dfname) : dfname;
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

		private static string ResolveShortcut(string filePath)
		{
			try
			{
				// IWshRuntimeLibrary is in the COM library "Windows Script Host Object Model"
				var shell = new IWshRuntimeLibrary.WshShell();
				var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(filePath);
				return shortcut.TargetPath;
			}
			catch (COMException)
			{
				// A COMException is thrown if the file is not a valid shortcut (.lnk) file
				return null;
			}
		}

		private string HandleReadEol(string s)
		{
			if (eolconv == 4)
				s = s.Replace("\r\n", "\n");
			else if (eolconv == 8)
				s = s.Replace("\r", "\n");

			return s;
		}

		private string HandleWriteEol(string s)
		{
			if (eolconv == 4)
				s = s.Replace("\n", "\r\n");

			return s;
		}
	}
}