using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//MATT
using Keysharp.Scripting;
using Microsoft.VisualBasic.FileIO;

namespace Keysharp.Core
{
	public static class Disk
	{
		/// <summary>
		/// Copies a folder along with all its sub-folders and files.
		/// </summary>
		/// <param name="source">Path of the source directory.</param>
		/// <param name="destination">Path of the destination directory.</param>
		/// <param name="flag">
		/// <list type="bullet">
		/// <item><term>0</term>: <description>(default) do not overwrite existing files</description></item>
		/// <item><term>1</term>: <description>overwrite existing files</description></item>
		/// </list>
		/// </param>
		public static void DirCopy(params object[] obj)
		{
			var (source, dest, flag) = obj.L().S3();
			var overwrite = Options.OnOff(flag) ?? false;
			Accessors.A_ErrorLevel = 0;

			try
			{
				source = Path.GetFullPath(source);
				dest = Path.GetFullPath(dest);
				CopyDirectory(source, dest, overwrite);
			}
			catch (IOException)
			{
				Accessors.A_ErrorLevel = 1;
			}
		}

		/// <summary>
		/// Creates a directory.
		/// </summary>
		/// <param name="path">Path of the directory to create.</param>
		public static void DirCreate(params object[] obj)
		{
			var path = obj.L().S1();
			Accessors.A_ErrorLevel = 0;

			try
			{
				_ = Directory.CreateDirectory(path);
				Accessors.A_ErrorLevel = Directory.Exists(path) ? 0 : 1;
			}
			catch (IOException)
			{
				Accessors.A_ErrorLevel = 1;
			}
		}

		/// <summary>
		/// Deletes a folder.
		/// </summary>
		/// <param name="Path">Name of the directory to delete, which is assumed to be in %A_WorkingDir% if an absolute path isn't specified.</param>
		/// <param name="Recurse">
		/// <list type="">
		/// <item>0 (default): Do not remove files and sub-directories contained in DirName. In this case, if DirName is not empty, no action will be taken and Accessors.A_ErrorLevel will be set to 1.</item>
		/// <item>1: Remove all files and subdirectories (like the DOS DelTree command).</item>
		/// </list>
		/// </param>
		public static void DirDelete(params object[] obj)
		{
			var (path, recurse) = obj.L().Sb();

			try
			{
				Directory.Delete(path, recurse);
				Accessors.A_ErrorLevel = 0;
			}
			catch (Exception) { Accessors.A_ErrorLevel = 1; }
		}

		/// <summary>
		/// Returns the attributes of a file if it exists.
		/// </summary>
		/// <param name="pattern">The name of a file or wildcard pattern.</param>
		/// <returns>A blank string if no files or folders are found, otheriwse the attributes of the first match.</returns>
		public static string DirExist(params object[] obj)
		{
			var pattern = obj.L().S1();

			try
			{
				foreach (var file in Disk.Glob(pattern))
					return Conversions.FromFileAttribs(System.IO.File.GetAttributes(file));

				return string.Empty;
			}
			catch (IOException)
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Moves a folder along with all its sub-folders and files. It can also rename a folder.
		/// </summary>
		/// <param name="Source">Name of the source directory (with no trailing backslash), which is assumed to be in %A_WorkingDir% if an absolute path isn't specified. For example: C:\My Folder </param>
		/// <param name="Dest">The new path and name of the directory (with no trailing baskslash), which is assumed to be in %A_WorkingDir% if an absolute path isn't specified. For example: D:\My Folder. Note: Dest is the actual path and name that the directory will have after it is moved; it is not the directory into which Source is moved (except for the known limitation mentioned below). </param>
		/// <param name="Flag">
		/// <para>(options) Specify one of the following single characters:</para>
		/// <para>0 (default): Do not overwrite existing files. The operation will fail if Dest already exists as a file or directory.</para>
		/// <para>1: Overwrite existing files. However, any files or subfolders inside Dest that do not have a counterpart in Source will not be deleted. Known limitation: If Dest already exists as a folder and it is on the same volume as Source, Source will be moved into it rather than overwriting it. To avoid this, see the next option.</para>
		/// <para>2: The same as mode 1 above except that the limitation is absent.</para>
		/// <para>R: Rename the directory rather than moving it. Although renaming normally has the same effect as moving, it is helpful in cases where you want "all or none" behavior; that is, when you don't want the operation to be only partially successful when Source or one of its files is locked (in use). Although this method cannot move Source onto a different volume, it can move it to any other directory on its own volume. The operation will fail if Dest already exists as a file or directory.</para>
		/// </param>
		public static void DirMove(params object[] obj)
		{
			var (source, dest, flag) = obj.L().S3();
			var rename = false;
			var movein = false;
			Accessors.A_ErrorLevel = 0;

			//If dest exists as a file, never copy.
			if (System.IO.File.Exists(dest))
			{
				Accessors.A_ErrorLevel = 1;
				throw new Exception($"Cannot move {source} to {dest} because destination is a file.");
			}

			Accessors.A_ErrorLevel = 0;

			switch (flag.ToUpperInvariant())
			{
				case "1":
					movein = true;
					break;

				case "2":
					break;

				case "R":
					rename = true;
					break;

				case "0":
				default:
					if (Directory.Exists(dest))
						return;

					break;
			}

			if (rename && Directory.Exists(dest))
			{
				Accessors.A_ErrorLevel = 1;
				throw new Exception($"Cannot rename {source} to {dest} because it already exists.");
			}

			if (!Directory.Exists(source))
			{
				Accessors.A_ErrorLevel = 1;
				throw new Exception($"Cannot move {source} to {dest} because source does not exist.");
			}

			if (movein && Directory.Exists(dest))
				dest = Path.Combine(dest, Path.GetFileName(source.TrimEnd(Path.DirectorySeparatorChar)));

			MoveDirectory(source, dest);
		}

		/// <summary>
		/// Ejects or retracts the specified CD drive.
		/// </summary>
		/// <param name="path">Path of the CD drive to eject or retract.</param>
		/// <param name="retract">True to retract, else false to eject.</param>
		public static void DriveEject(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			var (dr, b) = obj.L().Sb();
			var drive = Common.Drive.DriveProvider.CreateDrive(new DriveInfo(dr));

			if (b)
				drive.Retract();
			else
				drive.Eject();
		}

		/// <summary>
		/// Retrieves the capacity of a drive, in megabytes.
		/// </summary>
		/// <param name="path">Path of drive to receive information from.</param>
		/// <returns>The capacity of the drive in megabytes</returns>
		public static long DriveGetCapacity(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			return new DriveInfo(obj.L().S1()).TotalSize / 1024 / 1024;
		}

		/// <summary>
		/// Retrieves the file system name of a drive.
		/// </summary>
		/// <param name="path">Path of drive to receive information from.</param>
		/// <returns>The name of the file system of the drive</returns>
		public static string DriveGetFileSystem(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			return new DriveInfo(obj.L().S1()).DriveFormat;
		}

		/// <summary>
		/// Retrieves the label of a drive.
		/// </summary>
		/// <param name="path">Path of drive to receive information from.</param>
		/// <returns>The label of the drive</returns>
		public static string DriveGetLabel(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			return new DriveInfo(obj.L().S1()).VolumeLabel;
		}

		/// <summary>
		/// Returns a string of letters, one character for each drive letter in the system.
		/// </summary>
		/// <param name="type">If omitted, all drive types are retrieved. Otherwise, specify one of the following words to retrieve only a specific type of drive: CDROM, REMOVABLE, FIXED, NETWORK, RAMDISK, UNKNOWN.</param>
		/// <returns>The drive ltters of the system</returns>
		public static string DriveGetList(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			var drivetype = obj.L().S1();
			var matchingDevices = "";
			DriveType? type = null;

			if (!string.IsNullOrEmpty(drivetype))
				type = Common.Mapper.MappingService.Instance.DriveType.LookUpCLRType(drivetype);

			var drives = DriveInfo.GetDrives();

			for (var i = 0; i < drives.Length; i++)
			{
				if (type.HasValue)
				{
					if (i == 0) continue; // performance hack: skip A:\\

					try
					{
						if (drives[i].DriveType == type.Value)
							matchingDevices += drives[i].Name[0];
					}
					catch
					{
						// ignore
					}
				}
				else
				{
					matchingDevices += drives[i].Name[0];
				}
			}

			return matchingDevices;
		}

		/// <summary>
		/// Retrieves the serial number of a drive.
		/// </summary>
		/// <param name="path">Path of drive to receive information from.</param>
		/// <returns>The serial number of the drive</returns>
		public static long DriveGetSerial(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			var drive = obj.L().S1().TrimEnd('\\');
			var serialstr = Wmi.Identifier("Win32_LogicalDisk", "VolumeSerialNumber", $"SELECT * FROM Win32_LogicalDisk WHERE Name = \"{drive}\"");
			return long.TryParse(serialstr, NumberStyles.HexNumber, Parser.culture, out var l) ? l : 0;
		}

		/// <summary>
		/// Retrieves the free disk space of a drive, in megabytes.
		/// </summary>
		/// <param name="path">Path of drive to receive information from.</param>
		/// <returns>The free drive space in megabytes</returns>
		public static long DriveGetSpaceFree(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			return new DriveInfo(obj.L().S1()).TotalFreeSpace / 1024 / 1024;
		}

		/// <summary>
		/// Retrieves the status of a drive.
		/// </summary>
		/// <param name="path">Path of drive to receive information from.</param>
		/// <returns>The status of the drive</returns>
		public static string DriveGetStatus(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			var drv = new DriveInfo(obj.L().S1());
			var val = drv.DriveFormat;//Will throw DriveNotFoundException on invalid paths.
			return drv.IsReady ? "Ready" : "NotReady";
		}

		/// <summary>
		/// Retrieves the status of a CD drive
		/// </summary>
		/// <param name="path">Path of drive to receive information from.</param>
		/// <returns>The status of the CD drive</returns>
		public static string DriveGetStatusCD(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			var drive = obj.L().S1().TrimEnd('\\');
			var sb = new StringBuilder(128);
			var str = $"open {drive} type cdaudio alias cd wait shareable";

			if (WindowsAPI.mciSendString(str, sb, sb.Capacity, IntPtr.Zero) != 0)
			{
				Accessors.A_ErrorLevel = 1;
				return "error";
			}

			var res = WindowsAPI.mciSendString("status cd mode", sb, sb.Capacity, IntPtr.Zero);
			_ = WindowsAPI.mciSendString("close cd wait", null, 0, IntPtr.Zero);

			if (res != 0)
			{
				Accessors.A_ErrorLevel = 1;
				return "error";
			}

			return sb.ToString();
		}

		/// <summary>
		/// Retrieves the type of a drive.
		/// </summary>
		/// <param name="path">Path of drive to receive information from.</param>
		/// <returns>The type of the drive</returns>
		public static string DriveGetType(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			var drv = new DriveInfo(obj.L().S1());
			return Common.Mapper.MappingService.Instance.DriveType.LookUpIAType(drv.DriveType);
		}

		/// <summary>
		/// Prevents the eject feature of the specified drive from working.
		/// </summary>
		/// <param name="path">Path of drive to lock.</param>
		public static void DriveLock(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			var dr = Common.Drive.DriveProvider.CreateDrive(new DriveInfo(obj.L().S1()));
			dr.Lock();
		}

		/// <summary>
		/// Sets the label of a drive. This needs administrator privileges to run.
		/// </summary>
		/// <param name="path">Path of drive to write information to.</param>
		/// <param name="label">The label to set the drive to.</param>
		public static void DriveSetLabel(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			var (drive, label) = obj.L().S2();
			var drv = new DriveInfo(drive);
			drv.VolumeLabel = string.IsNullOrEmpty(label) ? "" : label;
		}

		/// <summary>
		/// Restores the eject feature of the specified drive.
		/// </summary>
		/// <param name="path">Path of drive to unlock.</param>
		public static void DriveUnlock(params object[] obj)
		{
			Accessors.A_ErrorLevel = 0;
			var dr = Common.Drive.DriveProvider.CreateDrive(new DriveInfo(obj.L().S1()));
			dr.UnLock();
		}

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
		public static void FileAppend(params object[] obj)
		{
			try
			{
				var o = obj;//Do not use .L() because the first argument can be an array.
				var text = o[0];
				var filename = o.Length > 1 ? o[1] as string : string.Empty;
				var enc = File.GetEncoding(Accessors.A_FileEncoding);
				var raw = false;
				var crlf = false;
				TextWriter tw = null;

				if (o.Length > 2 && o[2] is string options)
				{
					var splits = options.Split(' ');

					foreach (var split in splits)
					{
						var val = split.ToString().ToLowerInvariant();

						switch (val)
						{
							case "ascii":
							case "us-ascii":
								enc = Encoding.ASCII;
								break;

							case "utf-8":
								enc = Encoding.UTF8;
								break;

							case "utf-8-raw":
								enc = new UTF8Encoding(false);//Not byte order mark.
								break;

							case "utf-16":
							case "unicode":
								enc = Encoding.Unicode;
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

				Accessors.A_ErrorLevel = 0;
			}
			catch (IOException)
			{
				Accessors.A_ErrorLevel = 1;
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
		public static void FileCopy(params object[] obj)
		{
			var (source, dest, flag) = obj.L().S2b();
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
		public static void FileCreateShortcut(params object[] obj)
		{
			var (
				target,
				link,
				workingDir,
				args,
				description,
				icon,
				shortcutKey,
				iconNumber,
				runState) = obj.L().S7i2("", "", "", "", "", "", "", 0, 1);
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
			shortcut.WindowStyle = runState;
			shortcut.Save();
		}

		/// <summary>
		/// Deletes one or more files.
		/// </summary>
		/// <param name="pattern">The name of a file or a wildcard pattern.</param>
		public static void FileDelete(params object[] obj)
		{
			var s = obj.L().S1();

			try
			{
				var path = Path.GetDirectoryName(s);
				var dir = new DirectoryInfo(path?.Length == 0 ? "./" : path);
				var filename = Path.GetFileName(s);

				foreach (var file in dir.EnumerateFiles(filename))
					file.Delete();

				Accessors.A_ErrorLevel = 0;
			}
			catch (IOException)
			{
				Accessors.A_ErrorLevel = 1;
			}
		}

		public static string FileDirName(params object[] obj) => Path.GetDirectoryName(Path.GetFullPath(obj.L().S1()));

		public static string FileExist(params object[] obj)
		{
			var s = obj.L().S1();

			try
			{
				var path = Path.GetDirectoryName(s);
				var dir = new DirectoryInfo(path);
				var filename = Path.GetFileName(s);

				foreach (var file in dir.EnumerateFiles(filename))
					return Conversions.FromFileAttribs(System.IO.File.GetAttributes(file.FullName));

				return string.Empty;
			}
			catch (IOException)
			{
				return string.Empty;
			}
		}

		public static string FileFullPath(params object[] obj) => Path.GetFullPath(obj.L().S1());

		public static string FileGetAttrib(params object[] obj)
		{
			var s = obj.L().S1();

			try
			{
				if (Directory.Exists(s) || System.IO.File.Exists(s))
				{
					Accessors.A_ErrorLevel = 0;
					return Conversions.FromFileAttribs(System.IO.File.GetAttributes(s));
				}
				else if (s?.Length == 0)
				{
					Accessors.A_ErrorLevel = 0;
					return Conversions.FromFileAttribs(System.IO.File.GetAttributes(Accessors.A_LoopFileFullPath));
				}
			}
			catch (IOException)
			{
				Accessors.A_ErrorLevel = 1;
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
		public static ShortcutOutput FileGetShortcut(params object[] obj)
		{
			var link = obj.L().S1();

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

				Accessors.A_ErrorLevel = 0;
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
			catch (COMException)
			{
				Accessors.A_ErrorLevel = 1;
				return null;
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
		public static long FileGetSize(params object[] obj)
		{
			long result;
			var (file, units) = obj.L().S2();

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
				Accessors.A_ErrorLevel = 0;
			}
			catch (Exception)
			{
				result = 0;
				Accessors.A_ErrorLevel = 1;
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
		public static string FileGetTime(params object[] obj)
		{
			var (file, time) = obj.L().S2("", "M");

			if (file?.Length == 0)
				file = Accessors.A_LoopFileFullPath;

			if (!System.IO.File.Exists(file))
			{
				Accessors.A_ErrorLevel = 1;
				return "";
			}

			Accessors.A_ErrorLevel = 0;
			var info = new FileInfo(file);
			var date = new DateTime();

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
		/// <param name="result">The name of the variable in which to store the version number or string.</param>
		/// <param name="file">The name of the target file.</param>
		public static string FileGetVersion(params object[] obj)
		{
			var file = obj.L().S1();
			var result = "";

			if (file?.Length == 0)
				file = Accessors.A_LoopFileFullPath;

			try
			{
				var info = FileVersionInfo.GetVersionInfo(file);
				result = info.FileVersion;
				Accessors.A_ErrorLevel = 0;
			}
			catch (Exception)
			{
				Accessors.A_ErrorLevel = 1;
			}

			return result;
		}

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
		public static void FileMove(params object[] obj)
		{
			var (source, dest, flag) = obj.L().S2b();
			FileCopyMove(source, dest, flag, true);
		}

		public static File FileOpen(params object[] obj)
		{
			var (filename, flags, enc) = obj.L().S3();
			var encoding = Accessors.FileEncoding;
			var ienc = enc.ParseInt(false);

			if (ienc.HasValue)
				encoding = Encoding.GetEncoding(ienc.Value);
			else if (enc != "")
				encoding = File.GetEncoding(enc);

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

			return new File(filename, mode, access, share, encoding, eolconv);
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
		public static object FileRead(params object[] obj)
		{
			object output = null;
			var (filename, options) = obj.L().S2();
			var enc = Accessors.FileEncoding;
			Accessors.A_ErrorLevel = 0;

			if (string.IsNullOrEmpty(filename))
			{
				Accessors.A_ErrorLevel = 1;
				return "";
			}

			Accessors.A_ErrorLevel = 0;
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
							enc = File.GetEncoding(lower);
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
					output = new Array(temparr);
				}
				catch (Exception)
				{
					Accessors.A_ErrorLevel = 1;
					return "";
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
				catch (Exception)
				{
					Accessors.A_ErrorLevel = 1;
					return "";
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
		public static void FileRecycle(params object[] obj)
		{
			var s = obj.L().S1();

			try
			{
				var path = Path.GetDirectoryName(s);
				var dir = new DirectoryInfo(path);
				var filename = Path.GetFileName(s);

				foreach (var file in dir.EnumerateFiles(filename))
					//This appears to be not implemented in mono:
					//https://github.com/mono/mono-basic/blob/master/vbruntime/Microsoft.VisualBasic/Microsoft.VisualBasic.FileIO/FileSystemOperation.vb
					//May need some type of system call for non-windows OS.
					FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

				Accessors.A_ErrorLevel = 0;
			}
			catch (IOException)
			{
				Accessors.A_ErrorLevel = 1;
			}
		}

		/// <summary>
		/// Empties the recycle bin.
		/// </summary>
		/// <param name="Root">If omitted, the recycle bin for all drives is emptied. Otherwise, specify a drive letter such as C:\</param>
		public static void FileRecycleEmpty(params object[] obj)
		{
			var s = obj.L().S1();

			try
			{
				_ = WindowsAPI.SHEmptyRecycleBin(IntPtr.Zero, s, WindowsAPI.SHERB_NOCONFIRMATION | WindowsAPI.SHERB_NOPROGRESSUI | WindowsAPI.SHERB_NOSOUND);
				Accessors.A_ErrorLevel = 0;
			}
			catch (Exception) { Accessors.A_ErrorLevel = 1; }
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
		public static void FileSetAttrib(params object[] obj)
		{
			var (attributes, file, mode) = obj.L().S3("", Accessors.A_LoopFileFullPath, "");
			var dodirs = mode.Contains('d', StringComparison.OrdinalIgnoreCase);
			var dofiles = mode.Contains('f', StringComparison.OrdinalIgnoreCase);
			var recurse = mode.Contains('r', StringComparison.OrdinalIgnoreCase);

			if (!dodirs && !dofiles)
				dofiles = true;

			try
			{
				var error = 0;

				foreach (var path in Conversions.ToFiles(file, dofiles, dodirs, recurse))
				{
					var set = Conversions.ToFileAttribs(attributes, System.IO.File.GetAttributes(path));
					System.IO.File.SetAttributes(path, set);

					if (System.IO.File.GetAttributes(path) != set)
						error++;
				}

				Accessors.A_ErrorLevel = error;//This should be setting exception.extra.//MATT
			}
			catch (Exception)
			{
				Accessors.A_ErrorLevel = 1;
			}
		}

		//  try
		//  {
		//      var sr = new StreamReader(Filename);
		//      var line = string.Empty;
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
		public static void FileSetTime(params object[] obj)
		{
			var (YYYYMMDDHH24MISS, file, whichtime, mode) = obj.L().S4("", "", "M", "");
			var dodirs = mode.Contains('d', StringComparison.OrdinalIgnoreCase);
			var dofiles = mode.Contains('f', StringComparison.OrdinalIgnoreCase);
			var recurse = mode.Contains('r', StringComparison.OrdinalIgnoreCase);

			if (!dodirs && !dofiles)
				dofiles = true;

			if (file?.Length == 0)
				file = Accessors.A_LoopFileFullPath;

			var time = Conversions.ToDateTime(YYYYMMDDHH24MISS);

			try
			{
				var error = 0;

				foreach (var path in Conversions.ToFiles(file, dofiles, dodirs, recurse))
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
							error++;
				}

				Accessors.A_ErrorLevel = error;//This should be setting exception.extra.//MATT
			}
			catch (Exception) { Accessors.A_ErrorLevel = 1; }
		}

		/// <summary>
		/// Changes the script's current working directory.
		/// </summary>
		/// <param name="DirName">The name of the new working directory, which is assumed to be a subfolder of the current %A_WorkingDir% if an absolute path isn't specified.</param>
		public static void SetWorkingDir(params object[] obj) => Accessors.A_WorkingDir = obj.L().S1();

		/// <summary>
		/// Separates a file name or URL into its name, directory, extension, and drive.
		/// </summary>
		/// <param name="path">The path and file to split</param>
		/// <returns>A dictionary with keys and values like so:
		///
		/// "OutFileName": The file name without its path. The file's extension is included.
		/// "OutDir": The directory of the file, including drive letter or share name (if present). The final backslash is not included even if the file is located in a drive's root directory.
		/// "OutExtension": The file's extension (e.g. TXT, DOC, or EXE). The dot is not included.
		/// "OutNameNoExt": The file name without its path, dot and extension.
		/// "OutDrive">The drive letter or server name of the file. If the file is on a local or mapped drive, the variable will be set to the drive letter followed by a colon (no backslash). If the file is on a network path (UNC), the variable will be set to the share name, e.g. \\Workstation01
		///
		/// </returns>
		public static PathOutput SplitPath(params object[] obj)
		{
			var path = obj.L().S1();
			string filename = "", directory = "", extension = "", name = "", root = "";

			try
			{
				Accessors.A_ErrorLevel = 0;
				var input = Path.GetFullPath(path);
				filename = Path.GetFileName(input);
				directory = Path.GetDirectoryName(input);
				extension = Path.GetExtension(input).Trim('.');
				name = Path.GetFileNameWithoutExtension(input);
				root = Path.GetPathRoot(input);
			}
			catch (ArgumentException)
			{
				Accessors.A_ErrorLevel = 1;
			}

			return new PathOutput()
			{
				OutFileName = filename,
				OutDir = directory,
				OutExtension = extension,
				OutNameNoExt = name,
				OutDrive = root
			};
		}

		/// <summary>
		/// adapted from http://stackoverflow.com/questions/398518/how-to-implement-glob-in-c
		/// </summary>
		/// <param name="glob"></param>
		/// <returns></returns>
		internal static IEnumerable<string> Glob(string glob)
		{
			if (System.IO.File.Exists(glob) || Directory.Exists(glob))
			{
				yield return glob;
				yield break;
			}

			foreach (var path in Glob(PathHead(glob) + Path.DirectorySeparatorChar, PathTail(glob)))
			{
				yield return path;
			}
		}

		internal static IEnumerable<string> Glob(string head, string tail)
		{
			if (PathTail(tail) == tail)
			{
				foreach (var path in Directory.GetFiles(head, tail))
				{
					yield return path;
				}
			}
			else
			{
				foreach (var dir in Directory.GetDirectories(head, PathHead(tail)))
				{
					foreach (var path in Glob(Path.Combine(head, dir), PathTail(tail)))
					{
						yield return path;
					}
				}
			}
		}

		internal static string PathHead(string path)
		{
			const int root = 2;

			if (path.StartsWith(new string(Path.DirectorySeparatorChar, root)))
			{
				var parts = path.Substring(root).Split(new[] { Path.DirectorySeparatorChar }, 2);
				var buf = new StringBuilder(root + parts[0].Length + 1 + parts[1].Length);
				_ = buf.Append(path, 0, root);
				_ = buf.Append(parts[0]);
				_ = buf.Append(Path.DirectorySeparatorChar);
				_ = buf.Append(parts[1]);
				return buf.ToString();
			}

			return path.Split(Path.DirectorySeparatorChar)[0];
		}

		internal static string PathTail(string path) => !path.Contains(Path.DirectorySeparatorChar.ToString()) ? path : path.Substring(1 + PathHead(path).Length);

		private static void CopyDirectory(DirectoryInfo source, DirectoryInfo dest)
		{
			if (!dest.Exists) dest.Create();

			foreach (var fiSrcFile in source.GetFiles())
				_ = fiSrcFile.CopyTo(Path.Combine(dest.FullName, fiSrcFile.Name));

			foreach (var diSrcDirectory in source.GetDirectories())
				CopyDirectory(diSrcDirectory, new DirectoryInfo(Path.Combine(dest.FullName, diSrcDirectory.Name)));
		}

		private static void CopyDirectory(string source, string destination, bool overwrite)
		{
			try
			{
				_ = Directory.CreateDirectory(destination);
			}
			catch (IOException)
			{
				if (!overwrite)
					throw;
			}

			foreach (var filepath in Directory.GetFiles(source))
			{
				var basename = Path.GetFileName(filepath);
				var destfile = Path.Combine(destination, basename);
				System.IO.File.Copy(filepath, destfile, overwrite);
			}

			foreach (var dirpath in Directory.GetDirectories(source))
			{
				var basename = Path.GetFileName(dirpath);
				var destdir = Path.Combine(destination, basename);
				CopyDirectory(dirpath, destdir, overwrite);
			}
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
			var sfull = Path.GetFullPath(source).TrimEnd(Path.DirectorySeparatorChar);
			var dfull = Path.GetFullPath(dest).TrimEnd(Path.DirectorySeparatorChar);

			if (Directory.Exists(sfull))
				sfull += $"{Path.DirectorySeparatorChar}*";

			if (Directory.Exists(dfull))
				dfull += $"{Path.DirectorySeparatorChar}*";

			var sfname = Path.GetFileName(sfull);
			var dfname = Path.GetFileName(dfull);
			var sdname = Path.GetDirectoryName(sfull);
			var ddname = Path.GetDirectoryName(dfull);
			var files = Directory.GetFiles(sdname, sfname, System.IO.SearchOption.TopDirectoryOnly);

			//This is supposed to count the number of errors that happen, assign it to a member of a special exception object,
			//then throw that exception. Figure out later.//MATT
			if (files.Length == 0 && sfull.IndexOfAny("*?".ToCharArray()) == -1)
			{
				Accessors.A_ErrorLevel = 1;
				return;
				//throw new IOException($"{sfull} did not contain any files.");
			}

			if (!Directory.Exists(ddname))
			{
				Accessors.A_ErrorLevel = 1;
				return;
			}

			try
			{
				//A_LastError is supposed to be set here.//MATT
				//Marshal.GetLastWin32Error();
				foreach (var f in files)
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
						System.IO.File.Copy(s, d, flag);//Needs try/catch to count errors.
				}

				Accessors.A_ErrorLevel = 0;
			}
			catch (IOException)
			{
				Accessors.A_ErrorLevel = 1;
				//Exception.Extra is supposed to be set here.//MATT
			}
		}

		/// <summary>
		/// Gotten from: https://social.msdn.microsoft.com/forums/windows/en-US/b43cc316-ab96-49cb-8e3b-6de48fbc3453/how-to-move-a-folder-from-one-volume-drive-to-another-in-vbnet
		/// Move source folder to dest if on the same drive. Copy then delete the source if on a different drive.
		/// </summary>
		/// <param name="source">Source folder to copy</param>
		/// <param name="dest">Destination to copy source to</param>
		/// <param name="del">True to delete the source after copying if on different drives, else false to keep both copies.</param>
		private static void MoveDirectory(string source, string dest, bool del = true)
		{
			if (Directory.Exists(source))
			{
				if (Directory.GetDirectoryRoot(source) == Directory.GetDirectoryRoot(dest))
				{
					try
					{
						Directory.Move(source, dest);
					}
					catch (Exception ex)
					{
						throw new Exception($"Failed to move directory: {ex.Message}");
					}
				}
				else
				{
					try
					{
						CopyDirectory(new DirectoryInfo(source), new DirectoryInfo(dest));

						if (del)
							Directory.Delete(source, true);
					}
					catch (Exception ex)
					{
						throw new Exception($"Failed to copy directory: {ex.Message}");
					}
				}
			}
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
	}

	public class PathOutput
	{
		public string OutDir { get; set; }
		public string OutDrive { get; set; }
		public string OutExtension { get; set; }
		public string OutFileName { get; set; }
		public string OutNameNoExt { get; set; }
	}

	public class ShortcutOutput
	{
		public string OutArgs { get; set; }
		public string OutDescription { get; set; }
		public string OutDir { get; set; }
		public string OutIcon { get; set; }
		public string OutIconNum { get; set; }
		public long OutRunState { get; set; }
		public string OutTarget { get; set; }
	}
}