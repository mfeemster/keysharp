﻿namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for image-related functions.
	/// </summary>
	public static class Images
	{
		/// <summary>
		/// Loads a picture from file and returns a bitmap or icon handle.
		/// </summary>
		/// <param name="filename">The filename of the picture, which is usually assumed to be in <see cref="A_WorkingDir"/> if an absolute path isn't specified.<br/>
		/// If the name of a DLL or EXE file is given without a path, it may be loaded from the directory of the current executable or a system directory.
		/// </param>
		/// <param name="options">If blank or omitted, it defaults to no options. Otherwise, specify a string of one or more of the following options,<br/>
		/// each separated from the next with a space or tab:<br/>
		/// Wn and Hn: The width and height to load the image at, where n is an integer. If one dimension is omitted or -1, it is calculated automatically based on the other dimension,<br/>
		/// preserving aspect ratio. If both are omitted, the image's original size is used. If either dimension is 0, the original size is used for that dimension.<br/>
		/// For example: "w80 h50", "w48 h-1" or "w48" (preserve aspect ratio), "h0 w100" (use original height but override width).<br/>
		/// Iconn: Indicates which icon to load from a file with multiple icons (generally an EXE or DLL file). For example, "Icon2" loads the file's second icon.<br/>
		/// Any supported image format can be converted to an icon by specifying "Icon1".
		/// </param>
		/// <param name="outImageType">If omitted, the corresponding value will not be stored, and the return value will always be a bitmap handle (icons/cursors are converted if necessary)<br/>
		/// because reliably using or deleting an icon/cursor/bitmap handle requires knowing which type it is.<br/>
		/// Otherwise, specify a reference to the output variable in which to store a number indicating the type of handle being returned: 0 (IMAGE_BITMAP), 1 (IMAGE_ICON) or 2 (IMAGE_CURSOR).
		/// </param>
		/// <returns>A bitmap or icon handle depending on whether a picture or icon is specified and whether the &outImageType parameter is present or not.</returns>
		public static object LoadPicture(object filename, object options = null, [ByRef] object outImageType = null)
		{
			var file = filename.As();
			var opts = options.As();
			nint handle = 0;
			var width = int.MinValue;
			var height = int.MinValue;
			var icon = "";
			object iconnumber = 0L;
			var disposeHandle = false;

			foreach (Range r in opts.AsSpan().SplitAny(Spaces))
			{
				var opt = opts.AsSpan(r).Trim();

				if (opt.Length > 0)
				{
					if (Options.TryParse(opt, "w", ref width)) { }
					else if (Options.TryParse(opt, "h", ref height)) { }
					else if (Options.TryParseString(opt, "icon", ref icon)) { iconnumber = ImageHelper.PrepareIconNumber(icon); }
				}
			}

			var ext = Path.GetExtension(file).ToLower();
			(Bitmap, object) ret;

			if (ext == ".cur")
			{
				var cur = new Cursor(file);
				handle = cur.Handle;
				if (outImageType != null) Script.SetPropertyValue(outImageType, "__Value", 2L);
			}
			else if ((ret = ImageHelper.LoadImage(file, width, height, iconnumber)).Item1 is Bitmap bmp)
			{
				//Calling GetHbitmap() and GetHicon() creates a persistent handle that keeps the bitmap in memory, and must be destroyed later.
				if (ret.Item2 is Icon ic)
				{
					handle = ic.Handle;
					disposeHandle = false;
					if (outImageType != null) Script.SetPropertyValue(outImageType, "__Value", 1L);
				}
				else if (ImageHelper.IsIcon(file))
				{
					handle = ret.Item1.GetHicon();
					disposeHandle = true;
					if (outImageType != null) Script.SetPropertyValue(outImageType, "__Value", 1L);
				}
				else
				{
					handle = bmp.GetHbitmap();
					disposeHandle = true;
					if (outImageType != null) Script.SetPropertyValue(outImageType, "__Value", 0L);
				}
			}

			return new GdiHandleHolder(handle, disposeHandle);
		}
	}
}