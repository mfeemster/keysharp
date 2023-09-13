using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Keysharp.Core
{
	public static class Dir
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
		public static void DirCopy(object obj0, object obj1, object obj2 = null)
		{
			var source = obj0.As();
			var dest = obj1.As();
			var overwrite = obj2.Ab();
			source = Path.GetFullPath(source);
			dest = Path.GetFullPath(dest);
			CopyDirectory(source, dest, overwrite);
		}

		/// <summary>
		/// Creates a directory.
		/// </summary>
		/// <param name="path">Path of the directory to create.</param>
		public static void DirCreate(object obj)
		{
			try
			{
				_ = Directory.CreateDirectory(obj.As());
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}
		}

		/// <summary>
		/// Deletes a folder.
		/// </summary>
		/// <param name="Path">Name of the directory to delete, which is assumed to be in %A_WorkingDir% if an absolute path isn't specified.</param>
		/// <param name="Recurse">
		/// <list type="">
		/// <item>0 (false): Do not remove files and sub-directories contained in DirName. In this case, if DirName is not empty, no action is taken and an exception is thrown.</item>
		/// <item>1: Remove all files and subdirectories (like the DOS DelTree command).</item>
		/// </list>
		/// </param>
		public static void DirDelete(object obj0, object obj1 = null)
		{
			try
			{
				Directory.Delete(obj0.As(), obj1.Ab());
			}
			catch (Exception ex)
			{
				throw new Error(ex.Message);
			}
		}

		/// <summary>
		/// Returns the attributes of a file if it exists.
		/// </summary>
		/// <param name="pattern">The name of a file or wildcard pattern.</param>
		/// <returns>A blank string if no files or folders are found, otheriwse the attributes of the first match.</returns>
		public static string DirExist(object obj)
		{
			try//This can throw if the directory doesn't exist.
			{
				foreach (var file in Drive.Glob(obj.As()))
					return Conversions.FromFileAttribs(System.IO.File.GetAttributes(file));
			}
			catch
			{
			}

			return string.Empty;
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
		public static void DirMove(object obj0, object obj1, object obj2 = null)
		{
			var source = obj0.As();
			var dest = obj1.As();
			var flag = obj2.As();
			var rename = false;
			var movein = false;

			//If dest exists as a file, never copy.
			if (System.IO.File.Exists(dest))
				throw new Exception($"Cannot move {source} to {dest} because destination is a file.");

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
				throw new Exception($"Cannot rename {source} to {dest} because it already exists.");

			if (!Directory.Exists(source))
				throw new Exception($"Cannot move {source} to {dest} because source does not exist.");

			if (movein && Directory.Exists(dest))
				dest = Path.Combine(dest, Path.GetFileName(source.TrimEnd(Path.DirectorySeparatorChar)));

			MoveDirectory(source, dest);
		}

		/// <summary>
		/// Changes the script's current working directory.
		/// </summary>
		/// <param name="DirName">The name of the new working directory, which is assumed to be a subfolder of the current %A_WorkingDir% if an absolute path isn't specified.</param>
		public static void SetWorkingDir(object obj) => Accessors.A_WorkingDir = obj.As();

		public static void SplitPath(object obj)
		{
			object outFileName = null;
			object outDir = null;
			object outExtension = null;
			object outNameNoExt = null;
			object outDrive = null;
			SplitPath(obj, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive);
		}

		public static void SplitPath(object obj, ref object outFileName)
		{
			object outDir = null;
			object outExtension = null;
			object outNameNoExt = null;
			object outDrive = null;
			SplitPath(obj, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive);
		}

		public static void SplitPath(object obj, ref object outFileName, ref object outDir)
		{
			object outExtension = null;
			object outNameNoExt = null;
			object outDrive = null;
			SplitPath(obj, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive);
		}

		public static void SplitPath(object obj, ref object outFileName, ref object outDir, ref object outExtension)
		{
			object outNameNoExt = null;
			object outDrive = null;
			SplitPath(obj, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive);
		}

		public static void SplitPath(object obj, ref object outFileName, ref object outDir, ref object outExtension, ref object outNameNoExt)
		{
			object outDrive = null;
			SplitPath(obj, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive);
		}

		/// <summary>
		/// Separates a file name or URL into its name, directory, extension, and drive.
		/// </summary>
		/// <param name="path">The path and file to split</param>
		/// <returns>An object with the following fields:
		/// OutFileName: The file name without its path. The file's extension is included.
		/// OutDir: The directory of the file, including drive letter or share name (if present). The final backslash is not included even if the file is located in a drive's root directory.
		/// OutExtension: The file's extension (e.g. TXT, DOC, or EXE). The dot is not included.
		/// OutNameNoExt: The file name without its path, dot and extension.
		/// OutDrive: The drive letter or server name of the file. If the file is on a local or mapped drive, the variable will be set to the drive letter followed by a colon (no backslash). If the file is on a network path (UNC), the variable will be set to the share name, e.g. \\Workstation01
		/// </returns>
		public static void SplitPath(object obj, ref object outFileName, ref object outDir, ref object outExtension, ref object outNameNoExt, ref object outDrive)
		{
			var path = obj.As();
			var input = Path.GetFullPath(path);
			outFileName = Path.GetFileName(input);
			outDir = Path.GetDirectoryName(input);
			outExtension = Path.GetExtension(input).Trim('.');
			outNameNoExt = Path.GetFileNameWithoutExtension(input);
			outDrive = Path.GetPathRoot(input);
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
	}
}