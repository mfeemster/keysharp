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
			long handle = -1;
			long imageType = -1;
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
				var cur = new Cursor(filename);
				handle = cur.Handle.ToInt64();
				imageType = 2;
			}
			else if (ImageHelper.LoadImage(filename, width, height, iconnumber) is Bitmap bmp)
			{
				if (ImageHelper.IsIcon(filename))
				{
					handle = bmp.GetHicon().ToInt64();
					imageType = 1;
				}
				else
				{
					handle = bmp.GetHbitmap().ToInt64();
					imageType = 0;
				}
			}

			return new Keysharp.Core.Map(new Dictionary<object, object>()
			{
				{ "Handle", handle },
				{ "ImageType", imageType }
			});
		}
	}
}