namespace Keysharp.Core
{
	public static class Images
	{
		public static void CopyImageToClipboard(object obj0, object obj1 = null)
		{
			var filename = obj0.As();
			var options = obj1.As();
			var opts = Options.ParseOptions(options);
			var width = int.MinValue;
			var height = int.MinValue;
			var icon = "";
			object iconnumber = 0;

			foreach (var opt in opts)
			{
				if (Options.TryParse(opt, "w", ref width)) { }
				else if (Options.TryParse(opt, "h", ref height)) { }
				else if (Options.TryParseString(opt, "icon", ref icon)) { iconnumber = ImageHelper.PrepareIconNumber(icon); }
			}

			var ext = System.IO.Path.GetExtension(filename).ToLower();

			if (ext == ".cur")
			{
				using (var cur = new Cursor(filename))
				{
					Clipboard.SetImage(Keysharp.Core.Common.ImageHelper.ConvertCursorToBitmap(cur));
				}
			}
			else if (ImageHelper.LoadImage(filename, width, height, iconnumber).Item1 is Bitmap bmp)
			{
				Clipboard.SetImage(new Bitmap(bmp));
			}
		}

		public static object LoadPicture(object obj0)
		{
			object obj = null;
			return LoadPicture(obj0, null, ref obj);
		}

		public static object LoadPicture(object obj0, object obj1)
		{
			object obj = null;
			return LoadPicture(obj0, obj1, ref obj);
		}

		public static object LoadPicture(object obj0, object obj1, ref object obj2)
		{
			var filename = obj0.As();
			var options = obj1.As();
			var handle = IntPtr.Zero;
			var opts = Options.ParseOptions(options);
			var width = int.MinValue;
			var height = int.MinValue;
			var icon = "";
			object iconnumber = 0;
			var disposeHandle = false;

			foreach (var opt in opts)
			{
				if (Options.TryParse(opt, "w", ref width)) { }
				else if (Options.TryParse(opt, "h", ref height)) { }
				else if (Options.TryParseString(opt, "icon", ref icon)) { iconnumber = ImageHelper.PrepareIconNumber(icon); }
			}

			var ext = System.IO.Path.GetExtension(filename).ToLower();
			(Bitmap, object) ret;

			if (ext == ".cur")
			{
				var cur = new Cursor(filename);
				handle = cur.Handle;
				obj2 = 2L;
			}
			else if ((ret = ImageHelper.LoadImage(filename, width, height, iconnumber)).Item1 is Bitmap bmp)
			{
				//Calling GetHbitmap() and GetHicon() creates a persistent handle that keeps the bitmap in memory, and must be destroyed later.
				if (ret.Item2 is Icon ic)
				{
					handle = ic.Handle;
					disposeHandle = false;
					obj2 = 1L;
				}
				else if (ImageHelper.IsIcon(filename))
				{
					handle = ret.Item1.GetHicon();
					disposeHandle = true;
					obj2 = 1L;
				}
				else
				{
					handle = bmp.GetHbitmap();
					disposeHandle = true;
					obj2 = 0L;
				}
			}

			return new GdiHandleHolder(handle, disposeHandle);
		}
	}
}