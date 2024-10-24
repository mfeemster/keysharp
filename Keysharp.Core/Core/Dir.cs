namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for directory-related functions.
	/// </summary>
	public static class Dir
	{
		private static readonly string pathStart = new (Path.DirectorySeparatorChar, 2);

		/// <summary>
		/// Copies a folder along with all its sub-folders and files (similar to xcopy) or the entire contents of an archive file such as ZIP.
		/// </summary>
		/// <param name="source">Name of the source directory (with no trailing backslash), which is assumed to be in A_WorkingDir if an absolute path isn't specified.</param>
		/// <param name="dest">Name of the destination directory (with no trailing baskslash), which is assumed to be in A_WorkingDir if an absolute path isn't specified.</param>
		/// <param name="overwrite">
		/// If omitted, it defaults to 0. Otherwise, specify one of the following numbers to indicate whether to overwrite files if they already exist:
		///     0/false: Do not overwrite existing files. The operation will fail and have no effect if dest already exists as a file or directory.
		///     1/true: Overwrite existing files.However, any files or subfolders inside dest that do not have a counterpart in source will not be deleted.
		/// </param>
		/// <exception cref="OSError">An OSError exception is thrown if any failure happens while attempting to perform the operation.</exception>
		public static void DirCopy(object source, object dest, object overwrite = null)
		{
			var s = Path.GetFullPath(source.As());
			var d = Path.GetFullPath(dest.As());
			var o = overwrite.Ab();
			CopyDirectory(s, d, o);
		}

		/// <summary>
		/// Creates a folder.
		/// This function will also create all parent directories given in dirName if they do not already exist.
		/// </summary>
		/// <param name="dirName">Name of the directory to create, which is assumed to be in A_WorkingDir if an absolute path isn't specified.</param>
		/// <exception cref="OSError">An OSError exception is thrown if any failure happens while attempting to perform the operation.</exception>
		public static void DirCreate(object dirName)
		{
			try
			{
				_ = Directory.CreateDirectory(dirName.As());
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}
		}

		/// <summary>
		/// Deletes a folder.
		/// </summary>
		/// <param name="dirName">Name of the directory to delete, which is assumed to be in A_WorkingDir if an absolute path isn't specified.</param>
		/// <param name="recurse">If omitted, it defaults to false.
		///     If false, files and subdirectories contained in DirName are not removed.In this case, if DirName is not empty, no action is taken and an exception is thrown.
		///     If true, all files and subdirectories are removed (like the Windows command "rmdir /S").
		/// </param>
		/// <exception cref="OSError">An OSError exception is thrown if any failure happens while attempting to perform the operation.</exception>
		public static void DirDelete(object dirName, object recurse = null)
		{
			try
			{
				Directory.Delete(dirName.As(), recurse.Ab());
			}
			catch (Exception ex)
			{
				throw new OSError(ex.Message);
			}
		}

		/// <summary>
		/// Checks for the existence of a folder and returns its attributes.
		/// </summary>
		/// <param name="filePattern">The path, folder name, or file pattern to check. FilePattern is assumed to be in A_WorkingDir if an absolute path isn't specified.</param>
		/// <returns>
		/// Returns the attributes of the first matching folder. This string is a subset of RASHDOC, where each letter means the following:
		///     R = READONLY
		///     A = ARCHIVE
		///     S = SYSTEM
		///     H = HIDDEN
		///     N = NORMAL
		///     D = DIRECTORY
		///     O = OFFLINE
		///     C = COMPRESSED
		///     Since this function only checks for the existence of a folder, "D" is always present in the return value.If no folder is found, an empty string is returned.
		/// </returns>
		public static string DirExist(object filePattern)
		{
			try//This can throw if the directory doesn't exist.
			{
				foreach (var file in Drive.Glob(filePattern.As()))
					return Conversions.FromFileAttribs(File.GetAttributes(file));
			}
			catch (Exception)
			{
				//Swallow the exception since we still want to return an empty string even if it doesn't exist.
			}

			return string.Empty;
		}

		/// <summary>
		/// Moves a folder along with all its sub-folders and files. It can also rename a folder.
		/// </summary>
		/// <param name="source">Name of the source directory (with no trailing backslash), which is assumed to be in %A_WorkingDir% if an absolute path isn't specified. For example: C:\My Folder </param>
		/// <param name="dest">The new path and name of the directory (with no trailing baskslash), which is assumed to be in %A_WorkingDir% if an absolute path isn't specified. For example: D:\My Folder. Note: Dest is the actual path and name that the directory will have after it is moved; it is not the directory into which Source is moved (except for the known limitation mentioned below). </param>
		/// <param name="overwriteOrRename">
		/// If omitted, it defaults to 0. Otherwise, specify one of the following values to indicate whether to overwrite or rename existing files:
		///     0: Do not overwrite existing files.The operation will fail if dest already exists as a file or directory.
		///     1: Overwrite existing files.However, any files or subfolders inside dest that do not have a counterpart in Source will not be deleted.
		///         Known limitation: If dest already exists as a folder and it is on the same volume as Source, Source will be moved into it rather than overwriting it. To avoid this, see the next option.
		///     2: The same as mode 1 above except that the limitation is absent.
		///     R: Rename the directory rather than moving it.Although renaming normally has the same effect as moving, it is helpful in cases where you want "all or none" behavior; that is, when you don't want the operation to be only partially successful when Source or one of its files is locked (in use). Although this method cannot move Source onto a different volume, it can move it to any other directory on its own volume.
		///         The operation will fail if dest already exists as a file or directory.
		/// </param>
		/// <exception cref="OSError">An OSError exception is thrown if any failure happens while attempting to perform the operation.</exception>
		public static void DirMove(object source, object dest, object overwriteOrRename = null)
		{
			var s = source.As();
			var d = dest.As();
			var flag = overwriteOrRename.As();
			var rename = false;
			var movein = false;

			//If dest exists as a file, never copy.
			if (File.Exists(d))
				throw new OSError($"Cannot move {s} to {d} because destination is a file.");

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
					if (Directory.Exists(d))
						return;

					break;
			}

			if (rename && Directory.Exists(d))
				throw new OSError($"Cannot rename {s} to {d} because it already exists.");

			if (!Directory.Exists(s))
				throw new OSError($"Cannot move {s} to {d} because source does not exist.");

			if (movein && Directory.Exists(d))
				d = Path.Combine(d, Path.GetFileName(s.TrimEnd(Path.DirectorySeparatorChar)));

			MoveDirectory(s, d);
		}

		/// <summary>
		/// Returns the drive portion of path without the backslash.
		/// Ex: C:\folder => C: or \\uncdrive\folder\*.txt => \\uncdrive\folder
		/// Adapted from http://stackoverflow.com/questions/398518/how-to-implement-glob-in-c
		/// </summary>
		/// <param name="path">The path to retrieve the head for.</param>
		/// <returns>The drive portion of the path without the backslash.</returns>
		public static string PathHead(string path)
		{
			if (path.StartsWith(pathStart))
			{
				var dirSep = Path.DirectorySeparatorChar;
				var parts = path.Substring(2).Split(dirSep);
				var head = path.Substring(0, 2) + parts[0] + dirSep;

				if (parts.Length > 1)
					head += parts[1];

				return head;
			}

			return path.Split(Path.DirectorySeparatorChar)[0];
		}

		/// <summary>
		/// Changes the script's current working directory.
		/// </summary>
		/// <param name="dirName">The name of the new working directory, which is assumed to be a subfolder of the current %A_WorkingDir% if an absolute path isn't specified.</param>
		public static void SetWorkingDir(object dirName) => Accessors.A_WorkingDir = dirName.As();

		/// <summary>
		/// <seealso cref="SplitPath(path, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive)"/>
		/// </summary>
		public static void SplitPath(object obj)
		{
			object outFileName = null;
			object outDir = null;
			object outExtension = null;
			object outNameNoExt = null;
			object outDrive = null;
			SplitPath(obj, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive);
		}

		/// <summary>
		/// <seealso cref="SplitPath(path, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive)"/>
		/// </summary>
		public static void SplitPath(object obj, ref object outFileName)
		{
			object outDir = null;
			object outExtension = null;
			object outNameNoExt = null;
			object outDrive = null;
			SplitPath(obj, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive);
		}

		/// <summary>
		/// <seealso cref="SplitPath(path, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive)"/>
		/// </summary>
		public static void SplitPath(object obj, ref object outFileName, ref object outDir)
		{
			object outExtension = null;
			object outNameNoExt = null;
			object outDrive = null;
			SplitPath(obj, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive);
		}

		/// <summary>
		/// <seealso cref="SplitPath(path, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive)"/>
		/// </summary>
		public static void SplitPath(object obj, ref object outFileName, ref object outDir, ref object outExtension)
		{
			object outNameNoExt = null;
			object outDrive = null;
			SplitPath(obj, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive);
		}

		/// <summary>
		/// <seealso cref="SplitPath(path, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive)"/>
		/// </summary>
		public static void SplitPath(object obj, ref object outFileName, ref object outDir, ref object outExtension, ref object outNameNoExt)
		{
			object outDrive = null;
			SplitPath(obj, ref outFileName, ref outDir, ref outExtension, ref outNameNoExt, ref outDrive);
		}

		/// <summary>
		/// Separates a file name or URL into its name, directory, extension, and drive.
		/// </summary>
		/// <param name="path">The file name or URL to be analyzed.</param>
		/// <param name="outFileName">If omitted, the corresponding value will not be stored. Otherwise, specify a reference to the output variable in which to store the file name without its path. The file's extension is included.</param>
		/// <param name="outDir">If omitted, the corresponding value will not be stored. Otherwise, specify a reference to the output variable in which to store the directory of the file, including drive letter or share name (if present). The final backslash is not included even if the file is located in a drive's root directory.</param>
		/// <param name="outExtension">If omitted, the corresponding value will not be stored. Otherwise, specify a reference to the output variable in which to store the file's extension (e.g. TXT, DOC, or EXE). The dot is not included.</param>
		/// <param name="outNameNoExt">If omitted, the corresponding value will not be stored. Otherwise, specify a reference to the output variable in which to store the file name without its path, dot and extension.</param>
		/// <param name="outDrive">If omitted, the corresponding value will not be stored. Otherwise, specify a reference to the output variable in which to store the drive letter or server name of the file. If the file is on a local or mapped drive, the variable will be set to the drive letter followed by a colon (no backslash). If the file is on a network path (UNC), the variable will be set to the share name, e.g. \\Workstation01</param>
		public static void SplitPath(object path, ref object outFileName, ref object outDir, ref object outExtension, ref object outNameNoExt, ref object outDrive)
		{
			var p = path.As();

			if (p.Contains("://"))
			{
				var uri = new Uri(p);
				outDrive = uri.Scheme + "://" + uri.Host;
				var lastSlash = uri.LocalPath.LastIndexOf('/');
				var localPath = uri.LocalPath;

				if (lastSlash != -1)
				{
					var tempFilename = localPath.Substring(lastSlash + 1);

					if (tempFilename.Contains('.'))
					{
						outFileName = tempFilename;
						outExtension = Path.GetExtension(tempFilename).Trim('.');
						outNameNoExt = Path.GetFileNameWithoutExtension(tempFilename);
						localPath = localPath.Substring(0, lastSlash);
					}
					else
						outFileName = outExtension = outNameNoExt = "";
				}

				outDir = (outDrive + localPath).TrimEnd('/');
			}
			else
			{
				var input = Path.GetFullPath(p);
				outFileName = Path.GetFileName(input);
				outExtension = Path.GetExtension(input).Trim('.');
				outNameNoExt = Path.GetFileNameWithoutExtension(input);

				if (p.StartsWith(@"\\"))
				{
					//There appear to be no built in methods to process UNC paths, so do it manually here.
					var nextSlash = input.IndexOf('\\', 2);
					var lastSlash = input.LastIndexOf('\\');

					if (nextSlash == -1)
						outDrive = p;
					else
						outDrive = input.Substring(0, nextSlash);

					if (input.Contains('.'))
					{
						if (lastSlash == -1)
							outDir = input;
						else
							outDir = input.AsSpan().Slice(0, lastSlash).TrimEnd('\\').ToString();
					}
					else
						outDir = input.TrimEnd('\\');
				}
				else
				{
					outDir = Path.GetDirectoryName(input);
					outDrive = Path.GetPathRoot(input);
				}
			}
		}

		/// <summary>
		/// Returns path with the value from PathHead(path) removed from the start.
		/// </summary>
		/// <param name="path">The path to retrieve the tail for.</param>
		/// <returns>The path with the drive portion removed.</returns>
		internal static string PathTail(string path) => !path.Contains(Path.DirectorySeparatorChar.ToString()) ? path : path.Substring(1 + PathHead(path).Length);

		/// <summary>
		/// Private helper for copying a folder from source to dest.
		/// </summary>
		/// <param name="source">The folder to copy from.</param>
		/// <param name="dest">The folder to copy to.</param>
		private static void CopyDirectory(DirectoryInfo source, DirectoryInfo dest)
		{
			if (!dest.Exists)
				dest.Create();

			foreach (var fiSrcFile in source.GetFiles())
				_ = fiSrcFile.CopyTo(Path.Combine(dest.FullName, fiSrcFile.Name));

			foreach (var diSrcDirectory in source.GetDirectories())
				CopyDirectory(diSrcDirectory, new DirectoryInfo(Path.Combine(dest.FullName, diSrcDirectory.Name)));
		}

		/// <summary>
		/// Private helper for copying a folder from source to dest.
		/// </summary>
		/// <param name="source">The folder to copy from.</param>
		/// <param name="dest">The folder to copy to.</param>
		/// <param name="overwrite">Whether to overwrite the contents of dest.</param>
		private static void CopyDirectory(string source, string dest, bool overwrite)
		{
			try
			{
				if (!overwrite && Directory.Exists(dest))
					throw new IOException($"Directory already exists and overwrite is false.");

				_ = Directory.CreateDirectory(dest);
			}
			catch (IOException ioe)
			{
				if (!overwrite)
					throw new OSError($"Failed to create directory {dest}: {ioe.Message}");
			}

			try
			{
				//Special check for archive files.
				var exists = File.Exists(source);

				if (exists && source.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
				{
					ZipFile.ExtractToDirectory(source, dest, overwrite);
				}
				else if (exists && source.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
				{
					using FileStream compressedFileStream = File.Open(source, FileMode.Open);
					using FileStream outputFileStream = File.Create(dest);
					using var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
					decompressor.CopyTo(outputFileStream);
				}
				else if (exists && source.EndsWith(".tar", StringComparison.OrdinalIgnoreCase))
				{
					System.Formats.Tar.TarFile.ExtractToDirectory(source, dest, overwrite);
				}
				else
				{
					foreach (var filepath in Directory.GetFiles(source))
					{
						var basename = Path.GetFileName(filepath);
						var destfile = Path.Combine(dest, basename);
						File.Copy(filepath, destfile, overwrite);
					}

					foreach (var dirpath in Directory.GetDirectories(source))
					{
						var basename = Path.GetFileName(dirpath);
						var destdir = Path.Combine(dest, basename);
						CopyDirectory(dirpath, destdir, overwrite);
					}
				}
			}
			catch (Exception e)
			{
				throw new OSError($"Failed to copy directory {source} to {dest}: {e.Message}");
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
						throw new Error($"Failed to move directory {source} to {dest}: {ex.Message}");
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
						throw new Exception($"Failed to copy directory {source} to {dest}: {ex.Message}");
					}
				}
			}
		}
	}
}