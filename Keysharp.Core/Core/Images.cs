using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Keysharp.Core.Common;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//MATT
using System.Linq;
using System.Diagnostics;
using System.Globalization;

namespace Keysharp.Core
{
	public static class Images
	{
		public class ImageHandleAndType
		{
			public long Handle { get; set; }
			public long ImageType { get; set; }
		}

		public static ImageHandleAndType LoadPicture(params object[] obj)
		{
			var (filename, options) = obj.L().S2();
			var ret = new ImageHandleAndType()
			{
				Handle = -1,
				ImageType = -1
			};
			var opts = Options.ParseOptions(options);
			var width = int.MinValue;
			var height = int.MinValue;
			var icon = int.MinValue;

			foreach (var opt in opts)
			{
				if (Options.TryParse(opt, "w", ref width)) { }
				else if (Options.TryParse(opt, "h", ref height)) { }
				else if (Options.TryParse(opt, "icon", ref icon)) { if (icon > 0) --icon; }
			}

			var ext = System.IO.Path.GetExtension(filename).ToLower();

			if (ext == ".cur")
			{
				var cur = new Cursor(filename);
				ret.Handle = cur.Handle.ToInt64();
				ret.Handle = 2;
			}
			else if (ImageHelper.LoadImage(filename, width, height, icon) is Bitmap bmp)
			{
				if (ImageHelper.IsIcon(filename))
				{
					ret.Handle = bmp.GetHicon().ToInt64();
					ret.ImageType = 1;
				}
				else
				{
					ret.Handle = bmp.GetHbitmap().ToInt64();
					ret.Handle = 0;
				}
			}

			return ret;
		}
	}
}
