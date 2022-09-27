using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Keysharp.Core.Common;

namespace Keysharp.Core
{
	public static class Images
	{
		public static Map LoadPicture(object obj0, object obj1 = null)
		{
			var filename = obj0.As();
			var options = obj1.As();
			var handle = IntPtr.Zero;
			long imageType = -1;
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

			if (ext == ".cur")
			{
				var cur = new Cursor(filename);
				handle = cur.Handle;
				imageType = 2;
			}
			else if (ImageHelper.LoadImage(filename, width, height, iconnumber) is Bitmap bmp)
			{
				//Calling GetHbitmap() and GetHicon() creates a persistent handle that keeps the bitmap in memory, and must be destroyed later.
				if (ImageHelper.IsIcon(filename))
				{
					handle = bmp.GetHicon();
					disposeHandle = true;
					imageType = 1;
				}
				else
				{
					handle = bmp.GetHbitmap();
					disposeHandle = true;
					imageType = 0;
				}
			}

			return new Keysharp.Core.Map(new Dictionary<object, object>()
			{
				{ "Handle", new GdiHandleHolder(handle, disposeHandle) },
				{ "ImageType", imageType }
			});
		}

		public static void CopyImageToClipboard(object obj0, object obj1 = null)
		{
			var filename = obj0.As();
			var options = obj1.As();
			var handle = IntPtr.Zero;
			long imageType = -1;
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

			if (ext == ".cur")
			{
				using (var cur = new Cursor(filename))
				{
					Clipboard.SetImage(Keysharp.Core.Common.ImageHelper.ConvertCursorToBitmap(cur));
				}
			}
			else if (ImageHelper.LoadImage(filename, width, height, iconnumber) is Bitmap bmp)
			{
				Clipboard.SetImage(new Bitmap(bmp));
			}
		}
	}
}